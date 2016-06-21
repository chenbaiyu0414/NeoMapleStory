using System.Data.Entity;
using NeoMapleStory.Core.Database.Models;

namespace NeoMapleStory.Core.Database
{
    public class NeoMapleStoryDatabase:DbContext
    {
        public NeoMapleStoryDatabase():base("NeoMapleStory")
        {
            
        }

        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<CharacterModel> Characters { get; set; }
        public DbSet<InventoryItemModel> InventoryItems { get; set; }
        public DbSet<InventoryEquipmentModel> InventoryEquipments { get; set; }
        public DbSet<ShopMappingModel> ShopMappings { get; set; }
        public DbSet<SkillModel> Skills { get; set; }
        public DbSet<StorageModel> Storages { get; set; }
        public DbSet<CashShopInventoryModel> CashShopInventories { get; set; }
        public DbSet<CashShopGiftModel> CashShopGifts { get; set; }
        public DbSet<PetModel> Pets { get; set; }
        public DbSet<MonsterDropModel> MonsterDrops { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {           
            //Account=>Many(Character)
            modelBuilder.Entity<AccountModel>().HasMany(x => x.Characters).WithRequired(x => x.Account).HasForeignKey(x => x.AId);
            //Character=>Many(InventoryItem)
            modelBuilder.Entity<CharacterModel>().HasMany(x => x.InventoryItems).WithRequired(x => x.Character).HasForeignKey(x => x.CId);
            //InventoryItem=>one or null (InventoryEquipment)
            modelBuilder.Entity<InventoryItemModel>().HasOptional(x => x.InventoryEquipments).WithRequired(x => x.InventoryItem).WillCascadeOnDelete(true);
            base.OnModelCreating(modelBuilder);
        }
        
    }
}
