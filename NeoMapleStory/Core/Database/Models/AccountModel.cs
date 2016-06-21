using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace NeoMapleStory.Core.Database.Models
{
    public enum LoginStateType : byte
    {
        NotLogin,
        WaitingForDetail,
        ServerTransition,
        LoggedIn,
        Waiting,
        ViewAllChar
    }

    [Table("Accounts")]
    public class AccountModel
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Guid PasswordSalt { get; set; }

        public string SecretKey { get; set; }

        public Guid SecretKeySalt { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public DateTime BirthDate { get; set; }

        public DateTime RegisterDate { get; set; }

        public bool? Gender { get; set; }

        public bool IsGm { get; set; }

        public int NexonPoint { get; set; }

        public int MaplePoint { get; set; }

        public int ShoppingPoint { get; set; }

        public long LastLoginIp { get; set; }

        public string LastLoginMac { get; set; }

        public LoginStateType LoginState { get; set; }

        public bool IsPermanentBan { get; set; }

        public DateTime? TempBanDate { get; set; }

        public string BanReson { get; set; }

        public virtual ICollection<CharacterModel> Characters { get; set; }
    }
}