using System;
using System.Collections.Generic;
using NeoMapleStory.Core.Encryption;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Server;
using System.Linq;
using NeoMapleStory.Core.Database.Models;
using System.Data.Entity;

namespace NeoMapleStory.Core.Database
{
    public static class DatabaseHelper
    {
        /**
* 3: ID deleted or blocked<br>
* 4: Incorrect password<br>
* 5: Not a registered id<br>
* 6: System error<br>
* 7: Already logged in<br>
* 8: System error<br>
* 9: System error<br>
* 10: Cannot process so many connections<br>
* 11: Only users older than 20 can use this channel<br>
* 13: Unable to log on as master at this ip<br>
* 14: Wrong gateway or personal info and weird korean button<br>
* 15: Processing request with that korean button!<br>
* 16: Please verify your account through email...<br>
* 17: Wrong gateway or personal info<br>
* 21: Please verify your account through email...<br>
* 23: License agreement<br>
* 25: Maple Europe notice =[<br>
* 27: Some weird full client notice, probably for trial versions<br>
*
*/

        public enum LoginResultCode
        {
            Success = 0, //登陆成功
            Refresh, //刷新  同意协议 选择性别时
            Banned = 6, //封号
            ShieldLogin = 3, //屏蔽了账号登录功能或者已经被删除、终止的账号
            IncorrectPassword = 4, //屏蔽了静态密码或密码输入错误
            Unfind = 5, //未登录的账号
            IsLogged = 7, //当前连接不稳定。请更换其它频道或世界。为您带来不便，请谅解。6 or 8 or 9
            ServerBusy = 10, //目前因链接邀请过多 服务器未能处理。
            GenderNeeded
        }

        public static void ChangeGender(MapleClient c, string username, byte gender)
        {
            using (var db = new NeoMapleStoryDatabase())
            {
                var model = db.Accounts.Where(x => x.Username == username).Select(x => x).FirstOrDefault();
                if (model == null)
                    return;
                model.Gender = gender != 0;
                db.SaveChangesAsync();
            }
        }

        public static bool CheckNameUsed(MapleClient c, string name)
        {
            using (var db = new NeoMapleStoryDatabase())
            {
                return db.Characters.Where(x => x.Name == name).Select(x => x).Any();
            }
        }

        public static LoginResultCode Login(MapleClient c, string username, string password)
        {
            var state = LoginResultCode.Unfind;
            using (var db = new NeoMapleStoryDatabase())
            {
                db.Configuration.LazyLoadingEnabled = false;

                var model = db.Accounts.Where(x => x.Username == username).Select(x => x).Include(x => x.Characters).FirstOrDefault();

                if (model == null) return state;

                c.Account = model;

                if (model.LoginState != LoginStateType.NotLogin)
                    state = LoginResultCode.IsLogged;
                else if (model.Password != Sha256.Get(password, model.PasswordSalt))
                    state = LoginResultCode.IncorrectPassword;
                else if (model.IsPermanentBan || model.TempBanDate != null)
                    state = LoginResultCode.Banned;
                else if (model.Gender == null)
                    state = LoginResultCode.GenderNeeded;
                else
                {                 
                    db.SaveChanges();
                    state = LoginResultCode.Success;
                }
            }
            return state;
        }
    }
}