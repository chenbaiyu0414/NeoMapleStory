namespace NeoMapleStory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _06211 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(unicode: false),
                        Password = c.String(unicode: false),
                        PasswordSalt = c.Guid(nullable: false),
                        SecretKey = c.String(unicode: false),
                        SecretKeySalt = c.Guid(nullable: false),
                        Email = c.String(unicode: false),
                        BirthDate = c.DateTime(nullable: false, precision: 0),
                        RegisterDate = c.DateTime(nullable: false, precision: 0),
                        Gender = c.Boolean(),
                        IsGm = c.Boolean(nullable: false),
                        NexonPoint = c.Int(nullable: false),
                        MaplePoint = c.Int(nullable: false),
                        ShoppingPoint = c.Int(nullable: false),
                        LastLoginIp = c.Long(nullable: false),
                        LastLoginMac = c.String(unicode: false),
                        LoginState = c.Byte(nullable: false),
                        IsPermanentBan = c.Boolean(nullable: false),
                        TempBanDate = c.DateTime(precision: 0),
                        BanReson = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Characters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AId = c.Int(nullable: false),
                        WorldId = c.Byte(nullable: false),
                        Name = c.String(unicode: false),
                        JobId = c.Short(nullable: false),
                        GmLevel = c.Byte(nullable: false),
                        Gender = c.Boolean(nullable: false),
                        Level = c.Byte(nullable: false),
                        Meso = c.Int(nullable: false),
                        Exp = c.Int(nullable: false),
                        Dex = c.Short(nullable: false),
                        Int = c.Short(nullable: false),
                        Luk = c.Short(nullable: false),
                        Str = c.Short(nullable: false),
                        Hp = c.Short(nullable: false),
                        MaxHp = c.Short(nullable: false),
                        Mp = c.Short(nullable: false),
                        MaxMp = c.Short(nullable: false),
                        Face = c.Int(nullable: false),
                        Hair = c.Int(nullable: false),
                        Skin = c.Byte(nullable: false),
                        Fame = c.Short(nullable: false),
                        MapId = c.Int(nullable: false),
                        SpawnPoint = c.Byte(nullable: false),
                        PartyId = c.Int(),
                        RemainingAp = c.Short(nullable: false),
                        RemainingSp = c.Short(nullable: false),
                        IsMarried = c.Boolean(nullable: false),
                        AutoHpPot = c.Int(nullable: false),
                        AutoMpPot = c.Int(nullable: false),
                        EquipSlots = c.Byte(nullable: false),
                        UseSlots = c.Byte(nullable: false),
                        SetupSlots = c.Byte(nullable: false),
                        EtcSlots = c.Byte(nullable: false),
                        CashSlots = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AId, cascadeDelete: true)
                .Index(t => t.AId);
            
            CreateTable(
                "dbo.InventoryItems",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CId = c.Int(nullable: false),
                        StorageId = c.Int(nullable: false),
                        ItemId = c.Int(nullable: false),
                        InventoryType = c.Byte(nullable: false),
                        Position = c.Byte(nullable: false),
                        Quantity = c.Short(nullable: false),
                        Owner = c.String(unicode: false),
                        ExpireDate = c.DateTime(precision: 0),
                        UniqueId = c.Int(nullable: false),
                        PetSlot = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Characters", t => t.CId, cascadeDelete: true)
                .Index(t => t.CId);
            
            CreateTable(
                "dbo.InventoryEquipments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UpgradeSlots = c.Byte(nullable: false),
                        Locked = c.Byte(nullable: false),
                        Level = c.Byte(nullable: false),
                        Str = c.Short(nullable: false),
                        Dex = c.Short(nullable: false),
                        Int = c.Short(nullable: false),
                        Luk = c.Short(nullable: false),
                        Hp = c.Short(nullable: false),
                        Mp = c.Short(nullable: false),
                        Watk = c.Short(nullable: false),
                        Matk = c.Short(nullable: false),
                        Wdef = c.Short(nullable: false),
                        Mdef = c.Short(nullable: false),
                        Acc = c.Short(nullable: false),
                        Avoid = c.Short(nullable: false),
                        Hands = c.Short(nullable: false),
                        Speed = c.Short(nullable: false),
                        Jump = c.Short(nullable: false),
                        Vicious = c.Short(nullable: false),
                        IsRing = c.Boolean(nullable: false),
                        Flag = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InventoryItems", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.CashShopGifts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AId = c.Int(nullable: false),
                        Sender = c.String(unicode: false),
                        ItemId = c.Int(nullable: false),
                        Sn = c.Int(nullable: false),
                        Quantity = c.Short(nullable: false),
                        ExpireDate = c.DateTime(precision: 0),
                        Message = c.String(unicode: false),
                        RingUniqueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CashShopInventories",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AId = c.Int(nullable: false),
                        Sender = c.String(unicode: false),
                        UniqueId = c.Int(nullable: false),
                        ItemId = c.Int(nullable: false),
                        Sn = c.Int(nullable: false),
                        Quantity = c.Short(nullable: false),
                        Message = c.String(unicode: false),
                        ExpireDate = c.DateTime(precision: 0),
                        IsGift = c.Boolean(nullable: false),
                        IsRing = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MonsterDropModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MonsterId = c.Int(nullable: false),
                        ItemId = c.Int(nullable: false),
                        Chance = c.Int(nullable: false),
                        QuestId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PetModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Level = c.Byte(nullable: false),
                        Closeness = c.Short(nullable: false),
                        Fullness = c.Byte(nullable: false),
                        UniqueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ShopMappings",
                c => new
                    {
                        ShopId = c.Int(nullable: false, identity: true),
                        Npcid = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ShopId);
            
            CreateTable(
                "dbo.Skills",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CId = c.Int(nullable: false),
                        SkillId = c.Int(nullable: false),
                        Level = c.Byte(nullable: false),
                        MasterLevel = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Storages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AId = c.Int(nullable: false),
                        Slots = c.Byte(nullable: false),
                        Meso = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Characters", "AId", "dbo.Accounts");
            DropForeignKey("dbo.InventoryItems", "CId", "dbo.Characters");
            DropForeignKey("dbo.InventoryEquipments", "Id", "dbo.InventoryItems");
            DropIndex("dbo.InventoryEquipments", new[] { "Id" });
            DropIndex("dbo.InventoryItems", new[] { "CId" });
            DropIndex("dbo.Characters", new[] { "AId" });
            DropTable("dbo.Storages");
            DropTable("dbo.Skills");
            DropTable("dbo.ShopMappings");
            DropTable("dbo.PetModels");
            DropTable("dbo.MonsterDropModels");
            DropTable("dbo.CashShopInventories");
            DropTable("dbo.CashShopGifts");
            DropTable("dbo.InventoryEquipments");
            DropTable("dbo.InventoryItems");
            DropTable("dbo.Characters");
            DropTable("dbo.Accounts");
        }
    }
}
