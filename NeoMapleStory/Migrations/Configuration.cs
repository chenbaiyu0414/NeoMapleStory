namespace NeoMapleStory.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<NeoMapleStory.Core.Database.NeoMapleStoryDatabase>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(NeoMapleStory.Core.Database.NeoMapleStoryDatabase context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            Guid passwordSalt = Guid.NewGuid();
            Guid secretSalt = Guid.NewGuid();
            context.Accounts.AddOrUpdate(new Core.Database.Models.AccountModel
            {
                Username = "cby40899570",
                Password = Core.Encryption.Sha256.Get("cby159753", passwordSalt),
                PasswordSalt = passwordSalt,
                SecretKey = Core.Encryption.Sha256.Get("159753", secretSalt),
                SecretKeySalt = secretSalt,
                BirthDate = DateTime.Now,
                RegisterDate = DateTime.Now,
                IsGm = true,
                NexonPoint = 1000000,
                MaplePoint = 1000000,
                ShoppingPoint = 100000,
                LastLoginIp =System.Net.IPAddress.Parse("127.0.0.1").Address,
                LoginState = Core.Database.Models.LoginStateType.NotLogin,
                IsPermanentBan = false
            });
        }
    }
}
