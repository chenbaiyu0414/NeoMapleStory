using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using NeoMapleStory.Core;
using NeoMapleStory.Core.Database;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Buff;
using NeoMapleStory.Game.Client.AntiCheat;
using NeoMapleStory.Game.Inventory;
using NeoMapleStory.Game.Job;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.Mob;
using NeoMapleStory.Game.Movement;
using NeoMapleStory.Game.Quest;
using NeoMapleStory.Game.Script.Event;
using NeoMapleStory.Game.Shop;
using NeoMapleStory.Game.Skill;
using NeoMapleStory.Game.World;
using NeoMapleStory.Packet;
using NeoMapleStory.Server;
using Quartz;
using NeoMapleStory.Core.Database.Models;
using System.Data.Entity;

namespace NeoMapleStory.Game.Client
{
    public class MapleCharacter : AbstractAnimatedMapleMapObject
    {
        public const double MaxViewRangeSq = 850 * 850;

        private  CharacterModel m_characterInfo = new CharacterModel();

        private readonly Action<MapleCharacter, int> m_cancelCooldownAction = (target, skillId) =>
        {
            var realTarget = (MapleCharacter)new WeakReference(target).Target;
            realTarget?.RemoveCooldown(skillId);
        };

        //private IPlayerInteractionManager interaction = null;
        private readonly List<MapleMonster> m_controlled = new List<MapleMonster>();

        private readonly Dictionary<int, MapleCoolDownValueHolder> m_coolDowns =
            new Dictionary<int, MapleCoolDownValueHolder>();

        private readonly Dictionary<MapleBuffStat, MapleBuffStatValueHolder> m_effects =
            new Dictionary<MapleBuffStat, MapleBuffStatValueHolder>();

        private readonly List<string> m_mapletips = new List<string>();

        private readonly Dictionary<MapleQuest, MapleQuestStatus> m_quests =
            new Dictionary<MapleQuest, MapleQuestStatus>();

        private readonly SkillMacro[] m_skillMacros = new SkillMacro[5];
        public readonly List<MapleDisease> Diseases = new List<MapleDisease>();

        public Dictionary<long, MapleStatEffect> BuffsToCancel { get; private set; } = new Dictionary<long, MapleStatEffect>();


        private int m_apqScore;
        private string m_chalkboardtext;
        private int m_cp;

        public bool InCashShop = false;

        //Character Info Begin
        public AccountModel Account => m_characterInfo.Account;
        public int NexonPoint
        {
            get { return Account.NexonPoint; }
            set { Account.NexonPoint = value; }
        }
        public int MaplePoint
        {
            get { return Account.MaplePoint; }
            set { Account.MaplePoint = value; }
        }
        public int ShoppoingPoint
        {
            get { return Account.ShoppingPoint; }
            set { Account.ShoppingPoint = value; }
        }
        public bool IsGm
        {
            get { return Account.IsGm; }
            set { Account.IsGm = value; }
        }

        public int Id => m_characterInfo.Id;
        public string Name
        {
            get { return m_characterInfo.Name; }
            set { m_characterInfo.Name = value; }
        }

        public bool IsMarried => m_characterInfo.IsMarried;
        public byte WorldId => m_characterInfo.WorldId;


        public byte GmLevel
        {
            get { return m_characterInfo.GmLevel; }
            set { m_characterInfo.GmLevel = value; }
        }

        public int Face
        {
            get { return m_characterInfo.Face; }
            set { m_characterInfo.Face = value; }
        }

        public int Hair
        {
            get { return m_characterInfo.Hair; }
            set { m_characterInfo.Hair = value; }
        }

        public short RemainingAp
        {
            get { return m_characterInfo.RemainingAp; }
            set { m_characterInfo.RemainingAp = value; }
        }

        public short RemainingSp
        {
            get { return m_characterInfo.RemainingSp; }
            set { m_characterInfo.RemainingSp = value; }
        }

        public short Fame
        {
            get { return m_characterInfo.Fame; }
            set { m_characterInfo.Fame = value; }
        }

        public short Hp
        {
            get { return m_characterInfo.Hp; }
            set
            {
                var oldhp = m_characterInfo.Hp;
                var tmpHp = value;
                if (tmpHp < 0)
                    tmpHp = 0;
                if (tmpHp > Localmaxhp)
                    tmpHp = Localmaxhp;
                m_characterInfo.Hp = tmpHp;
                UpdatePartyMemberHp();
                if (oldhp > m_characterInfo.Hp && !IsAlive)
                    PlayerDead();
                CheckBerserk();
            }
        }

        public short Mp
        {
            get { return m_characterInfo.Mp; }
            set
            {
                var tmp = value;
                if (tmp < 0)
                {
                    tmp = 0;
                }
                if (tmp > Localmaxmp)
                {
                    tmp = Localmaxmp;
                }
                m_characterInfo.Mp = tmp;
            }
        }

        public short MaxHp
        {
            get { return m_characterInfo.MaxHp; }
            set { m_characterInfo.MaxHp = value; }
        }

        public short MaxMp
        {
            get { return m_characterInfo.MaxMp; }
            set { m_characterInfo.MaxMp = value; }
        }

        public byte Level
        {
            get { return m_characterInfo.Level; }
            set { m_characterInfo.Level = value; }
        }

        public MapleSkinColor Skin
        {
            get { return MapleSkinColor.GetByColorId(m_characterInfo.Skin); }
            set { m_characterInfo.Skin = value.ColorId; }
        }

        public short Str
        {
            get { return m_characterInfo.Str; }
            set { m_characterInfo.Str = value; }
        }

        public short Dex
        {
            get { return m_characterInfo.Dex; }
            set { m_characterInfo.Dex = value; }
        }

        public short Int
        {
            get { return m_characterInfo.Int; }
            set { m_characterInfo.Int = value; }
        }

        public short Luk
        {
            get { return m_characterInfo.Luk; }
            set { m_characterInfo.Luk = value; }
        }

        public int AutoHpPot
        {
            get { return m_characterInfo.AutoHpPot; }
            set { m_characterInfo.AutoHpPot = value; }
        }

        public int AutoMpPot
        {
            get { return m_characterInfo.AutoMpPot; }
            set { m_characterInfo.AutoMpPot = value; }
        }

        public byte InitialSpawnPoint
        {
            get { return m_characterInfo.SpawnPoint; }
            set { m_characterInfo.SpawnPoint = value; }
        }

        public bool Gender
        {
            get { return m_characterInfo.Gender; }
            set { m_characterInfo.Gender = value; }
        }
        //Character Info End

        //计算出的属性
        public short Localmaxhp { get; set; }
        public short Localmaxmp { get; set; }
        public short Localdex { get; set; }
        public short Localint { get; set; }
        public short Localstr { get; set; }
        public short Localluk { get; set; }
        public int Watk { get; set; }
        public int Matk { get; set; }
        public int Wdef { get; set; }
        public int Mdef { get; set; }
        public int Magic { get; set; }
        public double SpeedMod { get; set; }
        public double JumpMod { get; set; }


        public int LocalMaxBasedDamage { get; set; }
        public Dictionary<ISkill, SkillEntry> Skills { get; } = new Dictionary<ISkill, SkillEntry>();

        public int JobType => Job.JobId / 2000;
        public bool IsAlive => Hp > 0;

        public InterLockedInt Exp { get; set; } = new InterLockedInt(0);

        public InterLockedInt Meso { get; set; } = new InterLockedInt(0);

        public MapleClient Client { get; set; }

        public MapleMap Map { get; set; }

        public MapleJob Job { get; set; }

        public MapleInventory[] Inventorys { get; set; }

        public MapleParty Party { get; set; }

        public List<MapleDoor> Doors { get; set; }

        public MapleMessenger Messenger { get; set; }

        public bool IsHidden { get; set; }

        public MapleShop Shop { get; set; }

        public MapleBuddyList Buddies { get; set; } = new MapleBuddyList(20);

        public int ItemEffect { get; set; }

        public int Chair { get; set; }

        public List<MaplePet> Pets { get; set; } = new List<MaplePet>();
        private TriggerKey m_fullnessSchedule;
        private TriggerKey m_fullnessSchedule1;
        private TriggerKey m_fullnessSchedule2;

        public Dictionary<int, MapleSummon> Summons { get; set; } = new Dictionary<int, MapleSummon>();

        public List<IMapleMapObject> VisibleMapObjects { get; set; } = new List<IMapleMapObject>();

        public List<ILifeMovementFragment> Lastres { get; set; } = new List<ILifeMovementFragment>();

        public List<string> BlockedPortals { get; } = new List<string>();

        public string ChalkBoardText
        {
            get { return m_chalkboardtext; }
            set
            {
                //if (interaction != null)
                //{
                //    return;
                //}
                m_chalkboardtext = value;
                Map.BroadcastMessage(m_chalkboardtext == null
                    ? PacketCreator.UseChalkboard(this, true)
                    : PacketCreator.UseChalkboard(this, false));
            }
        }

        private long m_afkTimer;
        private readonly int[] m_savedLocations = new int[7];

        public int DojoEnergy { get; set; } = 0;

        public int Energybar { get; set; } = 0;

        private MapleCashShopInventory m_cashshopInventory;
        public MapleCashShopInventory CashShopInventory => m_cashshopInventory ?? (m_cashshopInventory = new MapleCashShopInventory(this));

        public EventInstanceManager EventInstanceManager { get; set; }

        public CheatTracker AntiCheatTracker { get; private set; }
        public bool CanDoor { get; private set; } = true;

        public enum SavedLocationType
        {
            FreeMarket,
            Worldtour,
            Florina,
            Cygnusintro,
            Dojo,
            Pvp,
            PachinkoPort
        }

    public MapleCharacter()
        {
        Stance = 0;
            Inventorys = new MapleInventory[MapleInventoryType.TypeList.Length];
            foreach (var type in MapleInventoryType.TypeList)
            {
                Inventorys[type.Value] = new MapleInventory(type, 100);
            }
            Position = Point.Empty;
            AntiCheatTracker = new CheatTracker(this);

        }

        public MapleCharacter(int charid, MapleClient client, bool isInChannel) : this()
        {
            Client = client;
            Load(charid, isInChannel);
        }

        public void Create(MapleClient client,int job,int top,int bottom,int shoes,int weapon)
        {
            Client = client;
            m_characterInfo.Account = client.Account;
            m_characterInfo.AId = Account.Id;

            Inventorys[MapleInventoryType.Equip.Value] = new MapleInventory(MapleInventoryType.Equip, 24);
            Inventorys[MapleInventoryType.Use.Value] = new MapleInventory(MapleInventoryType.Use, 24);
            Inventorys[MapleInventoryType.Setup.Value] = new MapleInventory(MapleInventoryType.Setup, 24);
            Inventorys[MapleInventoryType.Etc.Value] = new MapleInventory(MapleInventoryType.Etc, 24);
            Inventorys[MapleInventoryType.Cash.Value] = new MapleInventory(MapleInventoryType.Cash, 50);

            if (job == 2)
            {
                Str = 11;
                Dex = 6;
                Int = 4;
               Luk = 4;
                RemainingAp = 0;
            }
            else
            {
                Str = 4;
                Dex = 4;
                Int = 4;
                Luk = 4;
                RemainingAp = 9;
            }

            if (job == 1)
            {
                Job = MapleJob.Beginner;
                Inventorys[MapleInventoryType.Etc.Value].AddItem(new Item(4161001, 0, 1));
            }
            else if (job == 0)
            {
                Job = MapleJob.Knight;
                Inventorys[MapleInventoryType.Etc.Value].AddItem(new Item(4161047, 0, 1));
            }
            else if (job == 2)
            {
                Job = MapleJob.Ares;
                Inventorys[MapleInventoryType.Etc.Value].AddItem(new Item(4161048, 0, 1));
            }

            var equip = Inventorys[MapleInventoryType.Equipped.Value];

            var equipTop = new Equip(top, 0xFB)
            {
                Wdef = 3,
                UpgradeSlots = 7
            };
            equip.AddFromDb(equipTop.Copy());

            var equipBottom = new Equip(bottom, 0xFA)
            {
                Wdef = 2,
                UpgradeSlots = 7
            };
            equip.AddFromDb(equipBottom.Copy());

            var equipShoes = new Equip(shoes, 0xF9)
            {
                Wdef = 2,
                UpgradeSlots = 7
            };
            equip.AddFromDb(equipShoes.Copy());

            var equipWeapon = new Equip(weapon, 0xF5)
            {
                Watk = 15,
                UpgradeSlots = 7
            };
            equip.AddFromDb(equipWeapon.Copy());

            MaxHp = 50;
            MaxMp = 50;

            RecalcLocalStats();

            Hp = 50;
            Mp = 50;
            GmLevel = 0;
            Level = 1;
            //BookCover = 0;
            //Maplemount = null;
            //TotalCp = 0;
            //Team = -1;
            //Incs = false;
            //Inmts = false;
            //AllianceRank = 5;


            Map = null;

            Buddies = new MapleBuddyList(20);

            m_cp = 0;

            m_apqScore = 0;

            Exp.Reset();
            Meso.Exchange(10000);


            //ret.keymap.put(Integer.valueOf(2), new MapleKeyBinding(4, 10));
            //ret.keymap.put(Integer.valueOf(3), new MapleKeyBinding(4, 12));
            //ret.keymap.put(Integer.valueOf(4), new MapleKeyBinding(4, 13));
            //ret.keymap.put(Integer.valueOf(5), new MapleKeyBinding(4, 18));
            //ret.keymap.put(Integer.valueOf(6), new MapleKeyBinding(4, 24));
            //ret.keymap.put(Integer.valueOf(7), new MapleKeyBinding(4, 21));
            //ret.keymap.put(Integer.valueOf(16), new MapleKeyBinding(4, 8));
            //ret.keymap.put(Integer.valueOf(17), new MapleKeyBinding(4, 5));
            //ret.keymap.put(Integer.valueOf(18), new MapleKeyBinding(4, 0));
            //ret.keymap.put(Integer.valueOf(19), new MapleKeyBinding(4, 4));
            //ret.keymap.put(Integer.valueOf(23), new MapleKeyBinding(4, 1));
            //ret.keymap.put(Integer.valueOf(25), new MapleKeyBinding(4, 19));
            //ret.keymap.put(Integer.valueOf(26), new MapleKeyBinding(4, 14));
            //ret.keymap.put(Integer.valueOf(27), new MapleKeyBinding(4, 15));
            //ret.keymap.put(Integer.valueOf(29), new MapleKeyBinding(5, 52));
            //ret.keymap.put(Integer.valueOf(31), new MapleKeyBinding(4, 2));
            //ret.keymap.put(Integer.valueOf(34), new MapleKeyBinding(4, 17));
            //ret.keymap.put(Integer.valueOf(35), new MapleKeyBinding(4, 11));
            //ret.keymap.put(Integer.valueOf(37), new MapleKeyBinding(4, 3));
            //ret.keymap.put(Integer.valueOf(38), new MapleKeyBinding(4, 20));
            //ret.keymap.put(Integer.valueOf(40), new MapleKeyBinding(4, 16));
            //ret.keymap.put(Integer.valueOf(41), new MapleKeyBinding(4, 23));
            //ret.keymap.put(Integer.valueOf(43), new MapleKeyBinding(4, 9));
            //ret.keymap.put(Integer.valueOf(44), new MapleKeyBinding(5, 50));
            //ret.keymap.put(Integer.valueOf(45), new MapleKeyBinding(5, 51));
            //ret.keymap.put(Integer.valueOf(46), new MapleKeyBinding(4, 6));
            //ret.keymap.put(Integer.valueOf(48), new MapleKeyBinding(4, 22));
            //ret.keymap.put(Integer.valueOf(50), new MapleKeyBinding(4, 7));
            //ret.keymap.put(Integer.valueOf(56), new MapleKeyBinding(5, 53));
            //ret.keymap.put(Integer.valueOf(57), new MapleKeyBinding(5, 54));
            //ret.keymap.put(Integer.valueOf(59), new MapleKeyBinding(6, 100));
            //ret.keymap.put(Integer.valueOf(60), new MapleKeyBinding(6, 101));
            //ret.keymap.put(Integer.valueOf(61), new MapleKeyBinding(6, 102));
            //ret.keymap.put(Integer.valueOf(62), new MapleKeyBinding(6, 103));
            //ret.keymap.put(Integer.valueOf(63), new MapleKeyBinding(6, 104));
            //ret.keymap.put(Integer.valueOf(64), new MapleKeyBinding(6, 105));
            //ret.keymap.put(Integer.valueOf(65), new MapleKeyBinding(6, 106));
        }

        private void RecalcLocalStats()
        {
            int oldmaxhp = Localmaxhp;
            Localmaxhp = MaxHp;
            Localmaxmp = MaxMp;
            Localdex = Dex;
            Localint = Int;
            Localstr = Str;
            Localluk = Luk;
            var speed = 100;
            var jump = 100;
            Magic = Localint;
            Watk = 0;

            foreach (var item in Inventorys[MapleInventoryType.Equipped.Value].Inventory.Values)
            {
                var equip = (IEquip)item;
                Localmaxhp += equip.Hp;
                Localmaxmp += equip.Mp;
                Localdex += equip.Dex;
                Localint += equip.Int;
                Localstr += equip.Str;
                Localluk += equip.Luk;
                Magic += equip.Matk + equip.Int;
                Watk += equip.Watk;
                speed += equip.Speed;
                jump += equip.Jump;
            }

            IMapleItem weapon;
            if (!Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(0xF5, out weapon) && Job == MapleJob.Pirate)
            {
                // Barefists
                Watk += 8;
            }
            Magic = Math.Min(Magic, 2000);

            var hbhp = GetBuffedValue(MapleBuffStat.Hyperbodyhp);
            if (hbhp.HasValue)
            {
                Localmaxhp += (short)(hbhp.Value / 100 * Localmaxhp);
            }
            var hbmp = GetBuffedValue(MapleBuffStat.Hyperbodymp);
            if (hbmp.HasValue)
            {
                Localmaxmp += (short)(hbmp.Value / 100 * Localmaxmp);
            }

            Localmaxhp = Math.Min((short)30000, Localmaxhp);
            Localmaxmp = Math.Min((short)30000, Localmaxmp);

            var watkbuff = GetBuffedValue(MapleBuffStat.Watk);
            if (watkbuff.HasValue)
            {
                Watk += watkbuff.Value;
            }
            if (Job == MapleJob.Bowman)
            {
                ISkill expert = null;
                if (Job == MapleJob.Crossbowmaster)
                {
                    expert = SkillFactory.GetSkill(3220004);
                }
                else if (Job == MapleJob.Bowmaster)
                {
                    expert = SkillFactory.GetSkill(3120005);
                }
                if (expert != null)
                {
                    var boostLevel = GetSkillLevel(expert);
                    if (boostLevel > 0)
                    {
                        Watk += expert.GetEffect(boostLevel).X;
                    }
                }
            }

            var matkbuff = GetBuffedValue(MapleBuffStat.Matk);
            if (matkbuff.HasValue)
            {
                Magic += matkbuff.Value;
            }

            var speedbuff = GetBuffedValue(MapleBuffStat.Speed);
            if (speedbuff.HasValue)
            {
                speed += speedbuff.Value;
            }

            var jumpbuff = GetBuffedValue(MapleBuffStat.Jump);
            if (jumpbuff.HasValue)
            {
                jump += jumpbuff.Value;
            }

            if (speed > 140)
            {
                speed = 140;
            }
            if (jump > 123)
            {
                jump = 123;
            }

            SpeedMod = speed / 100.0;
            JumpMod = jump / 100.0;

            var mount = GetBuffedValue(MapleBuffStat.MonsterRiding);
            if (mount.HasValue)
            {
                JumpMod = 1.23;
                switch (mount.Value)
                {
                    case 1:
                        SpeedMod = 1.5;
                        break;
                    case 2:
                        SpeedMod = 1.7;
                        break;
                    case 3:
                        SpeedMod = 1.8;
                        break;
                    case 5:
                        SpeedMod = 1.0;
                        JumpMod = 1.0;
                        break;
                    default:
                        SpeedMod = 2.0;
                        break;
                }
            }


            var mWarrior = GetBuffedValue(MapleBuffStat.MapleWarrior);
            if (mWarrior.HasValue)
            {
                Localstr += (short)(mWarrior.Value / 100 * Localstr);
                Localdex += (short)(mWarrior.Value / 100 * Localdex);
                Localint += (short)(mWarrior.Value / 100 * Localint);
                Localluk += (short)(mWarrior.Value / 100 * Localluk);
            }

            LocalMaxBasedDamage = CalculateMaxBaseDamage(Watk);
            if (oldmaxhp != 0 && oldmaxhp != Localmaxhp)
            {
                UpdatePartyMemberHp();
            }
        }

        public int CalculateMaxBaseDamage(int watk)
        {
            int maxbasedamage;
            if (watk == 0)
            {
                maxbasedamage = 1;
            }
            else
            {
                IMapleItem weaponItem;
                var barefists = !Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(0xF5, out weaponItem) && (Job == MapleJob.Pirate || Job == MapleJob.ThiefKnight);

                if (weaponItem != null || Job == MapleJob.Pirate || Job == MapleJob.ThiefKnight)
                {
                    var weapon = barefists
                        ? MapleWeaponType.Knuckle
                        : MapleItemInformationProvider.Instance.GetWeaponType(weaponItem.ItemId);
                    int mainstat;
                    int secondarystat;
                    if (weapon == MapleWeaponType.Bow || weapon == MapleWeaponType.Crossbow)
                    {
                        mainstat = Localdex;
                        secondarystat = Localstr;
                    }
                    else if ((Job == MapleJob.Thief || Job == MapleJob.NightKnight) &&
                             (weapon == MapleWeaponType.Claw || weapon == MapleWeaponType.Dagger))
                    {
                        mainstat = Localluk;
                        secondarystat = Localdex + Localstr;
                    }
                    else if ((Job == MapleJob.Pirate || Job == MapleJob.ThiefKnight) &&
                             (weapon == MapleWeaponType.Gun))
                    {
                        mainstat = Localdex;
                        secondarystat = Localstr;
                    }
                    else if ((Job == MapleJob.Pirate || Job == MapleJob.ThiefKnight) &&
                             (weapon == MapleWeaponType.Knuckle))
                    {
                        mainstat = Localstr;
                        secondarystat = Localdex;
                    }
                    else
                    {
                        mainstat = Localstr;
                        secondarystat = Localdex;
                    }
                    maxbasedamage = (int)((weapon.DamageMultiplier * mainstat + secondarystat) / 100.0 * watk);
                    maxbasedamage += 10;
                }
                else
                {
                    maxbasedamage = 0;
                }
            }
            return maxbasedamage;
        }

        public void GainNexonPoint(int value)
        {
            /*Account.NexonPoint += value;*/
        }


        public void Save()
        {
            using (var db = new NeoMapleStoryDatabase())
            {
                var update = db.Characters.Where(x => x.Id == Id).Select(x => x).Any();

                db.Entry(m_characterInfo).State = update ? EntityState.Modified : EntityState.Added;
                db.Entry(m_characterInfo.Account).State = EntityState.Modified;

                m_characterInfo.Exp = Exp.Value;
                m_characterInfo.Skin = Skin.ColorId;
                m_characterInfo.JobId = Job.JobId;
                m_characterInfo.Meso = Meso.Value;

                //ps.setInt(22, hpApUsed);
                //ps.setInt(23, mpApUsed);


                int mapId;
                if (Map == null)
                {
                    if (!update && Job.JobId == 1000)
                        mapId = 130030000;
                    else if (!update && Job.JobId == 2000)
                        mapId = 914000000;
                    else
                        mapId = 0;
                }
                else if (Map.MapId == 677000013 || Map.MapId == 677000012)
                {
                    mapId = 970030002;
                }
                else if (Map.ForcedReturnMapId != 999999999)
                {
                    mapId = Map.ForcedReturnMapId;
                }
                else
                {
                    mapId = Map.MapId;
                }

                m_characterInfo.MapId = mapId;


                if (Map == null || Map.MapId == 610020000 || Map.MapId == 610020001)
                {
                    m_characterInfo.SpawnPoint = 0;
                }
                else
                {
                    m_characterInfo.SpawnPoint = Map.FindClosestSpawnpoint(Position)?.PortalId ?? 0;
                }

                m_characterInfo.PartyId = Party?.PartyId ?? 0;
                m_characterInfo.AutoHpPot = AutoHpPot != 0 && GetItemAmount(AutoHpPot) >= 1 ? AutoHpPot : 0;
                m_characterInfo.AutoMpPot = AutoMpPot != 0 && GetItemAmount(AutoMpPot) >= 1 ? AutoMpPot : 0;
                m_characterInfo.IsMarried = IsMarried;


                //if (Messenger == null)
                //{
                //    MessengerId = 0;
                //    MessengerPosition = 4;
                //}


                //if (ZakumLvl > 2)
                //    ZakumLvl = 2;


                //if (maplemount != null)
                //{
                //    ps.setInt(49, maplemount.getLevel());
                //    ps.setInt(50, maplemount.getExp());
                //    ps.setInt(51, maplemount.getTiredness());
                //}
                //else {
                //    ps.setInt(49, 1);
                //    ps.setInt(50, 0);
                //    ps.setInt(51, 0);
                //}

                m_characterInfo.EquipSlots = Inventorys[MapleInventoryType.Equip.Value].SlotLimit;
                m_characterInfo.UseSlots = Inventorys[MapleInventoryType.Equip.Value].SlotLimit;
                m_characterInfo.SetupSlots = Inventorys[MapleInventoryType.Setup.Value].SlotLimit;
                m_characterInfo.EtcSlots = Inventorys[MapleInventoryType.Etc.Value].SlotLimit;
                m_characterInfo.CashSlots = Inventorys[MapleInventoryType.Cash.Value].SlotLimit;
                
                CashShopInventory?.Save();


                //foreach (MaplePet pettt in Pets)
                //{
                //    pettt.SaveToDb();
                //}

                //技能宏指令

                //ps = con.prepareStatement("DELETE FROM skillmacros WHERE characterid = ?");
                //ps.setInt(1, id);
                //ps.executeUpdate();
                //ps.close();

                //for (int i = 0; i < 5; i++)
                //{
                //    SkillMacro macro = skillMacros[i];
                //    if (macro != null)
                //    {
                //        ps = con.prepareStatement("INSERT INTO skillmacros (characterid, skill1, skill2, skill3, name, shout, position) VALUES (?, ?, ?, ?, ?, ?, ?)");

                //        ps.setInt(1, id);
                //        ps.setInt(2, macro.getSkill1());
                //        ps.setInt(3, macro.getSkill2());
                //        ps.setInt(4, macro.getSkill3());
                //        ps.setString(5, macro.getName());
                //        ps.setInt(6, macro.getShout());
                //        ps.setInt(7, i);

                //        ps.executeUpdate();
                //        ps.close();
                //    }
                //}


                var deleteItems = db.InventoryItems.Where(x => x.CId == Id).Select(x => x);
                db.InventoryItems.RemoveRange(deleteItems);

                foreach (var inventory in Inventorys)
                {
                    foreach (var item in inventory.Inventory.Values)
                    {
                        Guid guid = Guid.NewGuid();
                        var inventoryItem = new InventoryItemModel
                        {
                            Id = guid,
                            CId = Id,
                            ItemId = item.ItemId,
                            InventoryType = inventory.Type.Value,
                            Position = item.Position,
                            Quantity = item.Quantity,
                            Owner = item.Owner,
                            UniqueId = item.UniqueId,
                            ExpireDate = item.Expiration,
                            //PetSlot = /*getPetByUniqueId(item.UniqueID) > -1 ? getPetByUniqueId(item.UniqueID) + 1 :*/ 0,
                        };
                        db.InventoryItems.Add(inventoryItem);

                        if (inventory.Type == MapleInventoryType.Equip || inventory.Type == MapleInventoryType.Equipped)
                        {

                            var equip = (IEquip)item;
                            var inventoryEquip = new InventoryEquipmentModel
                            {
                                Id = inventoryItem.Id,
                                Acc = equip.Acc,
                                Avoid = equip.Avoid,
                                Dex = equip.Dex,
                                Hands = equip.Hands,
                                Hp = equip.Hp,
                                Mp = equip.Mp,
                                Int = equip.Int,
                                IsRing = equip.IsRing,
                                Jump = equip.Jump,
                                Level = equip.Level,
                                Locked = equip.Locked,
                                Luk = equip.Luk,
                                Matk = equip.Matk,
                                Mdef = equip.Mdef,
                                Watk = equip.Watk,
                                Wdef = equip.Wdef,
                                Speed = equip.Speed,
                                Str = equip.Str,
                                UpgradeSlots = equip.UpgradeSlots,
                                Vicious = equip.Vicious,
                                Flag = equip.Flag,
                            };
                            db.InventoryEquipments.Add(inventoryEquip);
                        }
                    }
                }

                //deleteWhereCharacterId(con, "DELETE FROM queststatus WHERE characterid = ?");
                //ps = con.prepareStatement("INSERT INTO queststatus (`queststatusid`, `characterid`, `quest`, `status`, `time`, `forfeited`) VALUES (DEFAULT, ?, ?, ?, ?, ?)", Statement.RETURN_GENERATED_KEYS);
                //pse = con.prepareStatement("INSERT INTO queststatusmobs VALUES (DEFAULT, ?, ?, ?)");
                //ps.setInt(1, id);
                //for (MapleQuestStatus q : quests.values())
                //{
                //    ps.setInt(2, q.getQuest().getId());
                //    ps.setInt(3, q.getStatus().getId());
                //    ps.setInt(4, (int)(q.getCompletionTime() / 1000));
                //    ps.setInt(5, q.getForfeited());
                //    ps.executeUpdate();
                //    ResultSet rs = ps.getGeneratedKeys();
                //    rs.next();
                //    for (int mob : q.getMobKills().keySet())
                //    {
                //        pse.setInt(1, rs.getInt(1));
                //        pse.setInt(2, mob);
                //        pse.setInt(3, q.getMobKills(mob));
                //        pse.executeUpdate();
                //    }
                //    rs.close();
                //}
                //ps.close();
                //pse.close();

                var deleteSkills = db.Skills.Where(x => x.CId == Id).Select(x => x);
                db.Skills.RemoveRange(deleteSkills);


                var addSkills = Skills.Select(skill => new SkillModel
                {
                    CId = Id,
                    SkillId = skill.Key.SkillId,
                    Level = skill.Value.SkilLevel,
                    MasterLevel = skill.Value.MasterLevel
                }).ToList();
                db.Skills.AddRange(addSkills);

                //deleteWhereCharacterId(con, "DELETE FROM keymap WHERE characterid = ?");
                //ps = con.prepareStatement("INSERT INTO keymap (characterid, `key`, `type`, `action`) VALUES (?, ?, ?, ?)");
                //ps.setInt(1, id);
                //for (Entry<Integer, MapleKeyBinding> keybinding : keymap.entrySet())
                //{
                //    ps.setInt(2, keybinding.getKey().intValue());
                //    ps.setInt(3, keybinding.getValue().getType());
                //    ps.setInt(4, keybinding.getValue().getAction());
                //    ps.executeUpdate();
                //}
                //ps.close();

                //deleteWhereCharacterId(con, "DELETE FROM savedlocations WHERE characterid = ?");
                //ps = con.prepareStatement("INSERT INTO savedlocations (characterid, `locationtype`, `map`) VALUES (?, ?, ?)");
                //ps.setInt(1, id);
                //for (SavedLocationType savedLocationType : SavedLocationType.values())
                //{
                //    if (savedLocations[savedLocationType.ordinal()] != -1)
                //    {
                //        ps.setString(2, savedLocationType.name());
                //        ps.setInt(3, savedLocations[savedLocationType.ordinal()]);
                //        ps.executeUpdate();
                //    }
                //}
                //ps.close();

                //deleteWhereCharacterId(con, "DELETE FROM buddies WHERE characterid = ? AND pending = 0");
                //ps = con.prepareStatement("INSERT INTO buddies (characterid, `buddyid`, `group`, `pending`) VALUES (?, ?, ?, 0)");
                //ps.setInt(1, id);
                //for (BuddylistEntry entry : buddylist.getBuddies())
                //{
                //    if (entry.isVisible())
                //    {
                //        ps.setInt(2, entry.getCharacterId());
                //        ps.setString(3, entry.getGroup());
                //        ps.executeUpdate();
                //    }
                //}
                //ps.close();

                //ps = con.prepareStatement("UPDATE accounts SET `paypalNX` = ?, `mPoints` = ?, `cardNX` = ?, `Present` = ? WHERE id = ?");
                //ps.setInt(1, paypalnx);
                //ps.setInt(2, maplepoints);
                //ps.setInt(3, cardnx);
                //ps.setInt(4, Present);
                //ps.setInt(5, client.getAccID());
                //ps.executeUpdate();
                //ps.close();

                //if (storage != null)
                //{
                //    storage.saveToDB();
                //}

                //if (update)
                //{
                //    ps = con.prepareStatement("DELETE FROM achievements WHERE accountid = ?");
                //    ps.setInt(1, accountid);
                //    ps.executeUpdate();
                //    ps.close();

                //    for (Integer achid : finishedAchievements)
                //    {
                //        ps = con.prepareStatement("INSERT INTO achievements(charid, achievementid, accountid) VALUES(?, ?, ?)");
                //        ps.setInt(1, id);
                //        ps.setInt(2, achid);
                //        ps.setInt(3, accountid);
                //        ps.executeUpdate();
                //        ps.close();
                //    }
                //}


                db.SaveChanges();
            }
        }

        public void StartMapEffect(string msg, int itemId, int duration = 30000)
        {
            var mapEffect = new MapleMapEffect(msg, itemId);
            Client.Send(mapEffect.CreateStartData());
            TimerManager.Instance.RunOnceTask(() => { Client.Send(mapEffect.CreateDestroyData()); }, duration);
        }

        private void Load(int charid, bool channelserver)
        {
            using (var db = new NeoMapleStoryDatabase())
            {
                db.Configuration.LazyLoadingEnabled = false;

                var charInfo = db.Characters.Where(x => x.Id == charid).Select(x => x)
                    .Include(x => x.Account)
                    .Include(x => x.InventoryItems)
                    .Include(x => x.InventoryItems.Select(y => y.InventoryEquipments))//"InventoryItems.InventoryEquipments"
                    .FirstOrDefault();

                if (charInfo == null)
                    throw new Exception("加载角色失败（未找到角色）");

                m_characterInfo = charInfo;

                Exp.Exchange(charInfo.Exp);
                Meso.Exchange(charInfo.Meso);
                Job = new MapleJob(charInfo.JobId);


                Inventorys[MapleInventoryType.Equip.Value] = new MapleInventory(MapleInventoryType.Equip, charInfo.EquipSlots);
                Inventorys[MapleInventoryType.Use.Value] = new MapleInventory(MapleInventoryType.Use, charInfo.UseSlots);
                Inventorys[MapleInventoryType.Setup.Value] = new MapleInventory(MapleInventoryType.Setup, charInfo.SetupSlots);
                Inventorys[MapleInventoryType.Etc.Value] = new MapleInventory(MapleInventoryType.Etc, charInfo.EtcSlots);
                Inventorys[MapleInventoryType.Cash.Value] = new MapleInventory(MapleInventoryType.Cash, charInfo.CashSlots);

                //int mountexp = rs.getInt("mountexp");
                //int mountlevel = rs.getInt("mountlevel");
                //int mounttiredness = rs.getInt("mounttiredness");

                if (channelserver)
                {
                    var mapFactory = MasterServer.Instance.ChannelServers[Client.ChannelId].MapFactory;
                    Map = mapFactory.GetMap(charInfo.MapId);
                    if (Map == null)
                    {
                        Map = mapFactory.GetMap(100000000);
                    }
                    else if (Map.ForcedReturnMapId != 999999999)
                    {
                        Map = mapFactory.GetMap(Map.ForcedReturnMapId);
                    }

                    var portal = Map.GetPortal(InitialSpawnPoint);
                    if (portal == null)
                    {
                        portal = Map.GetPortal(0);
                        // char is on a spawnpoint that doesn't exist - select the first spawnpoint instead
                        InitialSpawnPoint = 0;
                    }
                    Position = portal.Position;
                }

                //if (channelserver)
                //{
                //    int partyid = rs.getInt("party");
                //    if (partyid >= 0)
                //    {
                //        try
                //        {
                //            MapleParty party = client.getChannelServer().getWorldInterface().getParty(partyid);
                //            if (party != null && party.getMemberById(ret.id) != null)
                //            {
                //                ret.party = party;
                //            }
                //        }
                //        catch (RemoteException e)
                //        {
                //            client.getChannelServer().reconnectWorld();
                //        }
                //    }

                //    int messengerid = rs.getInt("messengerid");
                //    int position = rs.getInt("messengerposition");
                //    if (messengerid > 0 && position < 4 && position > -1)
                //    {
                //        try
                //        {
                //            WorldChannelInterface wci = ChannelServer.getInstance(client.getChannel()).getWorldInterface();
                //            MapleMessenger messenger = wci.getMessenger(messengerid);
                //            if (messenger != null)
                //            {
                //                ret.messenger = messenger;
                //                ret.messengerposition = position;
                //            }
                //        }
                //        catch (RemoteException e)
                //        {
                //            client.getChannelServer().reconnectWorld();
                //        }
                //    }
                //}

                foreach (var item in m_characterInfo.InventoryItems)
                {
                    var type = MapleInventoryType.GetByType(item.InventoryType);
                    var expiration = item.ExpireDate;

                    if (type == MapleInventoryType.Equip || type == MapleInventoryType.Equipped)
                    {
                        var itemid = item.ItemId;
                        var equip = new Equip(itemid, item.Position);

                        if (item.InventoryEquipments.IsRing)
                        {
                            //equip = MapleRing.loadFromDb(itemid, (byte)rs.getInt("position"), rs.getInt("uniqueid"));
                        }
                        else
                        {
                            equip.Owner = item.Owner;
                            equip.Quantity = item.Quantity;
                            equip.UniqueId = item.UniqueId;
                            equip.Acc = item.InventoryEquipments.Acc;
                            equip.Avoid = item.InventoryEquipments.Avoid;
                            equip.Dex = item.InventoryEquipments.Dex;
                            equip.Hands = item.InventoryEquipments.Hands;
                            equip.Hp = item.InventoryEquipments.Hp;
                            equip.Int = item.InventoryEquipments.Int;
                            equip.Jump = item.InventoryEquipments.Jump;
                            equip.Luk = item.InventoryEquipments.Luk;
                            equip.Matk = item.InventoryEquipments.Matk;
                            equip.Mdef = item.InventoryEquipments.Mdef;
                            equip.Mp = item.InventoryEquipments.Mp;
                            equip.Speed = item.InventoryEquipments.Speed;
                            equip.Str = item.InventoryEquipments.Str;
                            equip.Watk = item.InventoryEquipments.Watk;
                            equip.Wdef = item.InventoryEquipments.Wdef;
                            equip.UpgradeSlots = item.InventoryEquipments.UpgradeSlots;
                            equip.Locked = item.InventoryEquipments.Locked;
                            equip.Level = item.InventoryEquipments.Level;
                            equip.Flag = item.InventoryEquipments.Flag;
                            equip.Vicious = item.InventoryEquipments.Vicious;

                        }
                        if (expiration.HasValue)
                        {
                            if (expiration > DateTime.Now)
                            {
                                equip.Expiration = expiration;
                                Inventorys[type.Value].AddFromDb(equip);
                            }
                            else
                            {
                                var name = MapleItemInformationProvider.Instance.GetName(itemid);
                                m_mapletips.Add($"现金道具[{name}]由于过期已经被清除了");
                            }
                        }
                        else
                        {
                            Inventorys[type.Value].AddFromDb(equip);
                        }
                    }
                    else
                    {
                        var itemid = item.ItemId;
                        var newitem = new Item(itemid, item.Position, item.Quantity)
                        {
                            Owner = item.Owner,
                            UniqueId = item.UniqueId
                        };

                        if (expiration.HasValue)
                        {
                            if (expiration > DateTime.Now)
                            {
                                newitem.Expiration = expiration;
                                Inventorys[type.Value].AddFromDb(newitem);
                                if (itemid >= 5000000 && itemid <= 5000100)
                                {
                                    //if (item.items.PetSlot > 0)
                                    //{
                                    //    int index = item.items.PetSlot - 1;
                                    //    // MaplePet pet = MaplePet.loadFromDb(newitem.getItemId(), newitem.getPosition(), newitem.getUniqueId());
                                    //    //Point pos = ret.Position;
                                    //    //pos.Y -= 12;
                                    //    //pet.setPos(pos);
                                    //    //pet.setFh(ret.getMap().getFootholds().findBelow(pet.getPos()).getId());
                                    //    //pet.setStance(0);
                                    //    //int hunger = PetDataFactory.getHunger(pet.getItemId());
                                    //    //if (index > ret.getPets().size())
                                    //    //{
                                    //    //    ret.getPets().add(pet);
                                    //    //    ret.startFullnessSchedule(hunger, pet, ret.getPetSlot(pet));
                                    //    //}
                                    //    //else {
                                    //    //    ret.getPets().add(index, pet);
                                    //    //    ret.startFullnessSchedule(hunger, pet, ret.getPetSlot(pet));
                                    //    //}
                                    //}
                                }
                            }
                            else
                            {
                                var name = MapleItemInformationProvider.Instance.GetName(itemid);
                                m_mapletips.Add($"现金道具[{name}]由于过期已经被清除了");
                            }
                        }
                        else
                        {
                            Inventorys[type.Value].AddFromDb(newitem);
                        }
                    }
                }

                if (channelserver)
                {
                    //    ps = con.prepareStatement("SELECT * FROM queststatus WHERE characterid = ?");
                    //    ps.setInt(1, charid);
                    //    rs = ps.executeQuery();
                    //    PreparedStatement pse = con.prepareStatement("SELECT * FROM queststatusmobs WHERE queststatusid = ?");
                    //    while (rs.next())
                    //    {
                    //        MapleQuest q = MapleQuest.getInstance(rs.getInt("quest"));
                    //        MapleQuestStatus status = new MapleQuestStatus(q, MapleQuestStatus.Status.getById(rs.getInt("status")));
                    //        long cTime = rs.getLong("time");
                    //        if (cTime > -1)
                    //        {
                    //            status.setCompletionTime(cTime * 1000);
                    //        }
                    //        status.setForfeited(rs.getInt("forfeited"));
                    //        ret.quests.put(q, status);
                    //        pse.setInt(1, rs.getInt("queststatusid"));
                    //        ResultSet rsMobs = pse.executeQuery();
                    //        while (rsMobs.next())
                    //        {
                    //            status.setMobKills(rsMobs.getInt("mob"), rsMobs.getInt("count"));
                    //        }
                    //        rsMobs.close();
                    //    }
                    //    rs.close();
                    //    ps.close();
                    //    pse.close();

                    var skills = db.Skills.Where(x => x.CId == charid).Select(x => x);
                    foreach (var skill in skills)
                    {
                        var iskill = SkillFactory.GetSkill(skill.SkillId);
                        var skillEntry = new SkillEntry(skill.Level, skill.MasterLevel);

                        if (Skills.ContainsKey(iskill))
                            Skills[iskill] = skillEntry;
                        else
                            Skills.Add(iskill, skillEntry);
                    }

                    //    ps = con.prepareStatement("SELECT * FROM skillmacros WHERE characterid = ?");
                    //    ps.setInt(1, charid);
                    //    rs = ps.executeQuery();
                    //    while (rs.next())
                    //    {
                    //        int skill1 = rs.getInt("skill1");
                    //        int skill2 = rs.getInt("skill2");
                    //        int skill3 = rs.getInt("skill3");
                    //        String name = rs.getString("name");
                    //        int shout = rs.getInt("shout");
                    //        int position = rs.getInt("position");
                    //        SkillMacro macro = new SkillMacro(skill1, skill2, skill3, name, shout, position);
                    //        ret.skillMacros[position] = macro;
                    //    }
                    //    rs.close();
                    //    ps.close();

                    //    ps = con.prepareStatement("SELECT `key`,`type`,`action` FROM keymap WHERE characterid = ?");
                    //    ps.setInt(1, charid);
                    //    rs = ps.executeQuery();
                    //    while (rs.next())
                    //    {
                    //        int key = rs.getInt("key");
                    //        int type = rs.getInt("type");
                    //        int action = rs.getInt("action");
                    //        ret.keymap.put(key, new MapleKeyBinding(type, action));
                    //    }
                    //    rs.close();
                    //    ps.close();

                    //    ps = con.prepareStatement("SELECT `locationtype`,`map` FROM savedlocations WHERE characterid = ?");
                    //    ps.setInt(1, charid);
                    //    rs = ps.executeQuery();
                    //    while (rs.next())
                    //    {
                    //        String locationType = rs.getString("locationtype");
                    //        int mapid = rs.getInt("map");
                    //        ret.savedLocations[SavedLocationType.valueOf(locationType).ordinal()] = mapid;
                    //    }
                    //    rs.close();
                    //    ps.close();

                    //    ps = con.prepareStatement("SELECT `characterid_to`,`when` FROM famelog WHERE characterid = ? AND DATEDIFF(NOW(),`when`) < 30");
                    //    ps.setInt(1, charid);
                    //    rs = ps.executeQuery();
                    //    ret.lastfametime = 0;
                    //    ret.lastmonthfameids = new ArrayList<>(31);
                    //    while (rs.next())
                    //    {
                    //        ret.lastfametime = Math.max(ret.lastfametime, rs.getTimestamp("when").getTime());
                    //        ret.lastmonthfameids.add(rs.getInt("characterid_to"));
                    //    }
                    //    rs.close();
                    //    ps.close();

                    //    ps = con.prepareStatement("SELECT ares_data FROM char_ares_info WHERE charid = ?");
                    //    ps.setInt(1, charid);
                    //    rs = ps.executeQuery();
                    //    while (rs.next())
                    //    {
                    //        ret.ares_data.add(rs.getString("ares_data"));
                    //    }
                    //    rs.close();
                    //    ps.close();
                    //    ret.buddylist.loadFromDb(charid);
                    //    ret.storage = MapleStorage.loadOrCreateFromDB(ret.accountid);
                }

                //String achsql = "SELECT * FROM achievements WHERE accountid = ?";
                //ps = con.prepareStatement(achsql);
                //ps.setInt(1, ret.accountid);
                //rs = ps.executeQuery();
                //while (rs.next())
                //{
                //    ret.finishedAchievements.add(rs.getInt("achievementid"));
                //}
                //rs.close();
                //ps.close();

                //int mountid = ret.getJobType() * 20000000 + 1004;
                //if (ret.getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-18) != null)
                //{
                //    ret.maplemount = new MapleMount(ret, ret.getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-18).getItemId(), mountid);
                //    ret.maplemount.setExp(mountexp);
                //    ret.maplemount.setLevel(mountlevel);
                //    ret.maplemount.setTiredness(mounttiredness);
                //    ret.maplemount.setActive(false);
                //}
                //else {
                //    ret.maplemount = new MapleMount(ret, 0, mountid);
                //    ret.maplemount.setExp(mountexp);
                //    ret.maplemount.setLevel(mountlevel);
                //    ret.maplemount.setTiredness(mounttiredness);
                //    ret.maplemount.setActive(false);
                //}
            }
            RecalcLocalStats();
            //SilentEnforceMaxHpMp();
            //ISkill ship = SkillFactory.getSkill(5221006);
            //ret.battleshipHP = (ret.getSkillLevel(ship) * 4000 + (ret.getLevel() - 120) * 2000);
            //ret.loggedInTimer = System.currentTimeMillis();
        }

        public void CancelBuffStats(MapleBuffStat stat)
        {
            var buffStatList = new List<MapleBuffStat> { stat };
            DeregisterBuffStats(buffStatList);
            CancelPlayerBuffs(buffStatList);
        }

        private void CancelPlayerBuffs(List<MapleBuffStat> buffstats)
        {
            if (Client.ChannelServer.Characters.FirstOrDefault(x => x.Id == Id) != null)
            {
                // are we still connected ?
                RecalcLocalStats();
                EnforceMaxHpMp();
                Client.Send(PacketCreator.CancelBuff(buffstats));
                Map.BroadcastMessage(this, PacketCreator.CancelForeignBuff(Id, buffstats), false);
            }
        }

        private void DeregisterBuffStats(List<MapleBuffStat> stats)
        {
            var effectsToCancel = new List<MapleBuffStatValueHolder>(stats.Count);
            foreach (var stat in stats)
            {
                MapleBuffStatValueHolder mbsvh;
                if (m_effects.TryGetValue(stat, out mbsvh))
                {
                    m_effects.Remove(stat);
                    var addMbsvh = true;
                    foreach (var contained in effectsToCancel)
                    {
                        if (mbsvh.StartTime == contained.StartTime && contained.Effect == mbsvh.Effect)
                        {
                            addMbsvh = false;
                        }
                    }
                    if (addMbsvh)
                    {
                        effectsToCancel.Add(mbsvh);
                    }
                    if (stat == MapleBuffStat.Summon || stat == MapleBuffStat.Puppet)
                    {
                        var summonId = mbsvh.Effect.GetSourceId();
                        MapleSummon summon;
                        if (Summons.TryGetValue(summonId, out summon))
                        {
                            Map.BroadcastMessage(PacketCreator.RemoveSpecialMapObject(summon, true));
                            Map.RemoveMapObject(summon);
                            VisibleMapObjects.Remove(summon);
                            Summons.Remove(summonId);
                        }
                        if (summon.SkillId == 1321007)
                        {
                            //if (beholderHealingSchedule != null)
                            //{
                            //    beholderHealingSchedule.cancel(false);
                            //    beholderHealingSchedule = null;
                            //}
                            //if (beholderBuffSchedule != null)
                            //{
                            //    beholderBuffSchedule.cancel(false);
                            //    beholderBuffSchedule = null;
                            //}
                        }
                    }
                    else if (stat == MapleBuffStat.Dragonblood)
                    {
                        //dragonBloodSchedule.cancel(false);
                        //dragonBloodSchedule = null;
                    }
                }
            }
            foreach (var cancelEffectCancelTasks in effectsToCancel)
            {
                if (!GetBuffStats(cancelEffectCancelTasks.Effect, cancelEffectCancelTasks.StartTime).Any())
                {
                    //cancelEffectCancelTasks.Schedule.cancel(false);
                }
            }
        }

        private List<MapleBuffStat> GetBuffStats(MapleStatEffect effect, long startTime)
        {
            var stats = new List<MapleBuffStat>();
            foreach (var stateffect in m_effects)
            {
                var mbsvh = stateffect.Value;
                if (mbsvh.Effect.SameSource(effect) && (startTime == -1 || startTime == mbsvh.StartTime))
                {
                    stats.Add(stateffect.Key);
                }
            }
            return stats;
        }

        public bool IsBuffFrom(MapleBuffStat stat, ISkill skill)
        {
            MapleBuffStatValueHolder mbsvh;
            if (!m_effects.TryGetValue(stat, out mbsvh))
            {
                return false;
            }
            return mbsvh.Effect.IsSkill() && mbsvh.Effect.GetSourceId() == skill.SkillId;
        }

        public int? GetBuffedValue(MapleBuffStat effect)
        {
            MapleBuffStatValueHolder mbsvh;
            if (!m_effects.TryGetValue(effect, out mbsvh))
            {
                return null;
            }
            return mbsvh.Value;
        }

        public void Dispel()
        {
            var allBuffs = new List<MapleBuffStatValueHolder>(m_effects.Values);
            foreach (var mbsvh in allBuffs)
            {
                if (mbsvh.Effect.IsSkill())
                {
                    CancelEffect(mbsvh.Effect, false, mbsvh.StartTime);
                }
            }
        }

        public bool IsActiveBuffedValue(int skillid)
        {
            var allBuffs = new List<MapleBuffStatValueHolder>(m_effects.Values);
            return allBuffs.Any(mbsvh => mbsvh.Effect.IsSkill() && mbsvh.Effect.GetSourceId() == skillid && !IsGm);
        }

        public void GiveDebuff(MapleDisease disease, MobSkill skill)
        {
            lock (Diseases)
            {
                if (IsAlive && !IsActiveBuffedValue(2321005) && !Diseases.Contains(disease) && Diseases.Count < 2)
                {
                    Diseases.Add(disease);
                    var debuff = new List<Tuple<MapleDisease, int>>
                    {
                        new Tuple<MapleDisease, int>(disease, skill.X)
                    };
                    var mask = debuff.Aggregate<Tuple<MapleDisease, int>, long>(0,
                        (current, statup) => current | (long)statup.Item1);

                    Client.Send(PacketCreator.GiveDebuff(mask, debuff, skill));
                    Map.BroadcastMessage(this, PacketCreator.GiveForeignDebuff(Id, mask, skill), false);

                    if (IsAlive && Diseases.Contains(disease))
                    {
                        var character = this;
                        var disease_ = disease;
                        TimerManager.Instance.RunOnceTask(() =>
                        {
                            if (character.Diseases.Contains(disease_))
                            {
                                DispelDebuff(disease_);
                            }
                        }, skill.Duration);
                    }
                }
            }
        }

        public void DispelDebuff(MapleDisease debuff)
        {
            if (Diseases.Contains(debuff))
            {
                Diseases.Remove(debuff);
                var mask = (long)debuff;
                //getClient().getSession().write(MaplePacketCreator.cancelDebuff(mask));
                //getMap().broadcastMessage(this, MaplePacketCreator.cancelForeignDebuff(id, mask), false);
            }
        }

        public void DispelDebuffs()
        {
            var ret = new List<MapleDisease>();
            foreach (var disease in ret)
            {
                if (disease != MapleDisease.Seduce && disease != MapleDisease.Stun)
                {
                    Diseases.Remove(disease);
                    var mask = (long)disease;
                    //getClient().getSession().write(MaplePacketCreator.cancelDebuff(mask));
                    //getMap().broadcastMessage(this, MaplePacketCreator.cancelForeignDebuff(id, mask), false);
                }
            }
        }

        public void DispelDebuffsi()
        {
            var ret = new List<MapleDisease>();
            foreach (var disease in ret)
            {
                if (disease != MapleDisease.Seal)
                {
                    Diseases.Remove(disease);
                    var mask = (long)disease;
                    //getClient().getSession().write(MaplePacketCreator.cancelDebuff(mask));
                    //getMap().broadcastMessage(this, MaplePacketCreator.cancelForeignDebuff(id, mask), false);
                }
            }
        }

        public void CancelSavedBuffs()
        {
            foreach (long key in BuffsToCancel.Keys)
            {
                CancelEffect(BuffsToCancel[key], false, key);
            }
            BuffsToCancel.Clear();
        }

        public void DispelSkill(int skillid)
        {
            var allBuffs = new LinkedList<MapleBuffStatValueHolder>(m_effects.Values);
            foreach (var mbsvh in allBuffs)
            {
                if (skillid == 0)
                {
                    if (mbsvh.Effect.IsSkill() &&
                        (mbsvh.Effect.GetSourceId() % 20000000 == 1004 || DispelSkills(mbsvh.Effect.GetSourceId())))
                    {
                        //cancelEffect(mbsvh.Effect, false, mbsvh.StartTime);
                    }
                }
                else if (mbsvh.Effect.IsSkill() && mbsvh.Effect.GetSourceId() == skillid)
                {
                    // cancelEffect(mbsvh.Effect, false, mbsvh.StartTime);
                }
            }
        }

        private bool DispelSkills(int skillid)
        {
            switch (skillid)
            {
                case 1004:
                case 1321007:
                case 2121005:
                case 2221005:
                case 2311006:
                case 2321003:
                case 3111002:
                case 3111005:
                case 3211002:
                case 3211005:
                case 4111002:
                case 11001004:
                case 12001004:
                case 13001004:
                case 14001005:
                case 15001004:
                case 12111004:
                case 20001004:
                    return true;
                default:
                    return false;
            }
        }

        public void CancelAllDebuffs()
        {
            var ret = new List<MapleDisease>();
            foreach (var disease in ret)
            {
                Diseases.Remove(disease);
                var mask = (long)disease;
                //getClient().getSession().write(MaplePacketCreator.cancelDebuff(mask));
                //getMap().broadcastMessage(this, MaplePacketCreator.cancelForeignDebuff(id, mask), false);
            }
        }

        public int GetSkillLevel(int skill)
        {
            SkillEntry ret;
            if (!Skills.TryGetValue(SkillFactory.GetSkill(skill), out ret))
            {
                return 0;
            }
            return ret.SkilLevel;
        }

        public byte GetSkillLevel(ISkill skill)
        {
            SkillEntry ret;

            if (skill != null &&
                (skill.SkillId == 1009 || skill.SkillId == 10001009 || skill.SkillId == 1010 || skill.SkillId == 1011 ||
                 skill.SkillId == 10001010 || skill.SkillId == 10001011))
            {
                return 1;
            }
            if (!Skills.TryGetValue(skill, out ret))
            {
                return 0;
            }
            return ret.SkilLevel;
        }

        public void ChangeMap(MapleMap to) => ChangeMap(to, to.GetPortal(0));

        public void ChangeMap(MapleMap to, IMaplePortal pto)
        {
            if (to.MapId == 100000200 || to.MapId == 211000100 || to.MapId == 220000300)
            {
                ChangeMapInternal(to, pto.Position, PacketCreator.GetWarpToMap(to, (byte)(pto.PortalId - 2), this));
            }
            else
            {
                ChangeMapInternal(to, pto.Position, PacketCreator.GetWarpToMap(to, pto.PortalId, this));
            }
        }

        public void ChangeMap(MapleMap to, Point pos)
        {
            ChangeMapInternal(to, pos, PacketCreator.GetWarpToMap(to, 0x80, this));
        }

        private void ChangeMapInternal(MapleMap to, Point pos, OutPacket warpPacket)
        {
            Client.Send(warpPacket);

            //IPlayerInteractionManager interaction = MapleCharacter.this.getInteraction();
            //if (interaction != null)
            //{
            //    if (interaction.isOwner(MapleCharacter.this))
            //    {
            //        if (interaction.getShopType() == 2)
            //        {
            //            interaction.removeAllVisitors(3, 1);
            //            interaction.closeShop(((MaplePlayerShop)interaction).returnItems(getClient()));
            //        }
            //        else if (interaction.getShopType() == 1)
            //        {
            //            getClient().getSession().write(MaplePacketCreator.shopVisitorLeave(0));
            //            if (interaction.getItems().size() == 0)
            //            {
            //                interaction.removeAllVisitors(3, 1);
            //                interaction.closeShop(((HiredMerchant)interaction).returnItems(getClient()));
            //            }
            //        }
            //        else if (interaction.getShopType() == 3 || interaction.getShopType() == 4)
            //        {
            //            interaction.removeAllVisitors(3, 1);
            //        }
            //    }
            //    else {
            //        interaction.removeVisitor(MapleCharacter.this);
            //    }
            //}
            //MapleCharacter.this.setInteraction(null);

            Map.RemovePlayer(this);
            if (Client.ChannelServer.Characters.Contains(this))
            {
                Map = to;
                Position = pos;
                to.AddPlayer(this);
                //lastPortalPoint = getPosition();
                //if (Party != null)
                //{
                //    silentPartyUpdate();
                //    getClient().getSession().write(MaplePacketCreator.updateParty(getClient().getChannel(), party, PartyOperation.SILENT_UPDATE, null));
                //    updatePartyMemberHP();
                //}
                //if (getMap().getHPDec() > 0)
                //{
                //    hpDecreaseTask = TimerManager.Instance.Schedule(() =>
                //    {
                //        doHurtHp();
                //    }, 10000);
                //}
                //if (to.MapID == 980000301) { //todo: all CPq map id's
                //    setTeam(rand(0, 1));
                //    Client.Send(PacketCreator.startMonsterCarnival(getTeam()));
                //}
            }
        }

        public void ChangeMapBanish(int mapid, string portal, string msg)
        {
            DropMessage(PacketCreator.ServerMessageType.PinkText, msg);
            var map = Client.ChannelServer.MapFactory.GetMap(mapid);
            ChangeMap(map, map.GetPortal(portal));
        }

        public void ChangeCashShopPoints(byte type, int quantity)
        {
            switch (type)
            {
                case 0:
                    NexonPoint += quantity;
                    break;
                case 1:
                    MaplePoint += quantity;
                    break;
                    //case 4:
                    //    this.cardnx += quantity;
                    //    break;
            }
        }

        public int GetCashShopPoints(byte type)
        {
            switch (type)
            {
                case 0:
                    return NexonPoint;
                case 1:
                    return MaplePoint;
                //case 4:
                //    return this.cardnx;
                default:
                    return 0;
            }
        }

        public static int GetNextUniqueId()
        {
            int nextid;
            using (var db = new NeoMapleStoryDatabase())
            {
                var itemUid = db.InventoryItems.Max(x => (int?)x.UniqueId) + 1 ?? 0;
                var cashItemUid = db.CashShopInventories.Max(x => (int?)x.UniqueId) + 1 ?? 0;
                nextid = cashItemUid > itemUid ? cashItemUid : itemUid;
            }
            return nextid;
        }

        public void EquipChanged()
        {
            Map.BroadcastMessage(this, PacketCreator.UpdateCharLook(this), false);
            RecalcLocalStats();
            EnforceMaxHpMp();
            if (Messenger != null)
            {
                //WorldChannelInterface wci = ChannelServer.getInstance(getClient().getChannel()).getWorldInterface();
                //try
                //{
                //    wci.updateMessenger(getClient().getPlayer().getMessenger().getId(), getClient().getPlayer().getName(), getClient().getChannel());
                //}
                //catch 
                //{
                //    //getClient().getChannelServer().reconnectWorld();
                //}
            }
        }

        public void BlockPortal(string scriptName)
        {
            if (!BlockedPortals.Contains(scriptName) && scriptName != null)
            {
                BlockedPortals.Add(scriptName);
            }
            Client.Send(PacketCreator.BlockedPortal());
        }

        public void UnblockPortal(string scriptName)
        {
            if (BlockedPortals.Contains(scriptName) && scriptName != null)
            {
                BlockedPortals.Remove(scriptName);
            }
        }

        public int GetItemAmount(int itemid)
        {
            var type = MapleItemInformationProvider.Instance.GetInventoryType(itemid);
            var iv = Inventorys[type.Value];
            return iv.CountById(itemid);
        }

        public override void SendDestroyData(MapleClient client)
        {
            client.Send(PacketCreator.RemovePlayerFromMap(ObjectId));
        }

        public override void SendSpawnData(MapleClient client)
        {
            if ((IsHidden && client.Player.GmLevel > 0) || !IsHidden)
            {
                client.Send(PacketCreator.SpawnPlayerMapobject(this));
                foreach (var pet in Pets)
                {
                    Map.BroadcastMessage(this, PacketCreator.ShowPet(this, pet, false, false), false);
                }
            }
        }

        public override MapleMapObjectType GetType() => MapleMapObjectType.Player;


        public void GainMeso(int gain, bool show, bool enableActions)
        {
            GainMeso(gain, show, enableActions, false);
        }

        public void GainMeso(int gain, bool show, bool enableActions = false, bool inChat = false)
        {
            if (Meso.Value + gain < 0)
            {
                Client.Send(PacketCreator.EnableActions());
                return;
            }
            var newVal = Meso.Add(gain);
            UpdateSingleStat(MapleStat.Meso, newVal, enableActions);
            if (show)
            {
                Client.Send(PacketCreator.GetShowMesoGain(gain, inChat));
            }
        }

        public void AddCooldown(int skillId, long startTime, long length, TriggerKey triggerKey)
        {
            if (GmLevel < 5)
            {
                if (m_coolDowns.ContainsKey(skillId))
                {
                    m_coolDowns.Remove(skillId);
                }
                m_coolDowns.Add(skillId, new MapleCoolDownValueHolder(skillId, startTime, length, triggerKey));
            }
            else
            {
                Client.Send(PacketCreator.SkillCooldown(skillId, 0));
            }
        }

        public void GiveCoolDowns(int skillid, long starttime, long length)
        {
            var time = length + starttime - DateTime.Now.GetTimeMilliseconds();
            var triggerKey = TimerManager.Instance.RunOnceTask(() => m_cancelCooldownAction(this, skillid), time);
            AddCooldown(skillid, DateTime.Now.GetTimeMilliseconds(), time, triggerKey);
        }

        public void RemoveCooldown(int skillId)
        {
            if (m_coolDowns.ContainsKey(skillId))
            {
                m_coolDowns.Remove(skillId);
            }
            Client.Send(PacketCreator.SkillCooldown(skillId, 0));
        }

        public bool SkillisCooling(int skillId)
        {
            return m_coolDowns.ContainsKey(skillId);
        }

        public byte GetMasterLevel(ISkill skill)
        {
            SkillEntry ret;
            if (!Skills.TryGetValue(skill, out ret))
            {
                return 0;
            }
            return ret.MasterLevel;
        }

        public List<PlayerCoolDownValueHolder> GetAllCooldowns()
        {
            var ret = new List<PlayerCoolDownValueHolder>();
            foreach (var mcdvh in m_coolDowns.Values)
            {
                ret.Add(new PlayerCoolDownValueHolder(mcdvh.SkillId, mcdvh.StartTime, mcdvh.Duration));
            }
            return ret;
        }

        public List<int> GetTRockMaps(int type)
        {
            var rockmaps = new List<int>();

            //using (var db = new NeoDatabase())
            //{
            //    var result = db.RockLocations.Where(x => x.CharacterId == Id && x.Type == type).Select(x => x.MapId).Take(type == 1 ? 10 : 5);
            //    foreach (var mapid in result)
            //    {
            //        rockmaps.Add(mapid);
            //    }
            //    return rockmaps;
            //}
            return rockmaps;
        }

        public void DropMessage(string message)
        {
            Client.Send(
                PacketCreator.ServerNotice(
                    GmLevel > 0
                        ? PacketCreator.ServerMessageType.LightBlueText
                        : PacketCreator.ServerMessageType.PinkText, message));
        }

        public void DropMessage(PacketCreator.ServerMessageType type, string message)
        {
            Client.Send(PacketCreator.ServerNotice(type, message));
        }

        public MapleQuestStatus GetQuest(MapleQuest quest)
        {
            if (!m_quests.ContainsKey(quest))
            {
                return new MapleQuestStatus(quest, MapleQuestStatusType.NotStarted);
            }
            return m_quests[quest];
        }

        public int GetNumQuest()
        {
            var i = 0;
            foreach (var q in m_quests.Values)
            {
                if (q.Status == MapleQuestStatusType.Completed)
                {
                    i++;
                }
            }
            return i;
        }

        public void GainExp(int gain, bool show, bool inChat)
        {
            GainExp(gain, show, inChat, true);
        }

        public void GainExp(int gain, bool show, bool inChat, bool white)
        {
            if ((Level < 200 && (Math.Floor(Job.JobId / 100D) < 10 || Math.Floor(Job.JobId / 1000D) == 2)) || Level < 180)
            {
                if (Exp.Value + gain >= ExpTable.GetExpNeededForLevel(Level + 1))
                {
                    Exp.Add(gain);
                    LevelUp();
                    if (Exp.Value > ExpTable.GetExpNeededForLevel(Level + 1))
                    {
                        Exp.Exchange(ExpTable.GetExpNeededForLevel(Level + 1));
                    }
                }
                else
                {
                    Exp.Add(gain);
                }
            }
            else if (Exp.Value != 0)
            {
                Exp.Reset();
            }
            /**
          * @112208 - 精灵吊坠
          * @112207 - 温暖的围脖
          * @以上装备打怪可以多获得2倍的经验
          */
            UpdateSingleStat(MapleStat.Exp, Exp.Value);
            if (show && gain != 0)
            {
                IMapleItem eqp;
                if ((Inventorys[MapleInventoryType.Equipped.Value].Inventory.TryGetValue(17, out eqp) &&
                     eqp.ItemId == 1122018) || (eqp != null && eqp.ItemId == 1122017))
                {
                    //围脖吊坠
                    if (Level >= 1)
                    {
                        gain *= 2; //**经验倍数X2
                    }
                    Client.Send(PacketCreator.GetShowExpGain(gain, inChat, white, 1));
                }
                else
                {
                    Client.Send(PacketCreator.GetShowExpGain(gain, inChat, white));
                }
            }
        }

        private static short Rand(int lbound, int ubound)
            => (short)(Randomizer.NextDouble() * (ubound - lbound + 1) + lbound);

        public void LevelUp()
        {
            ISkill improvingMaxHp = null;
            var improvingMaxHpLevel = 0;

            ISkill improvingMaxMp = null;
            var improvingMaxMpLevel = 0;

            if (Job.JobId >= 1000 && Job.JobId <= 1511 && Level < 70)
            {
                RemainingAp += 1;
            }
            RemainingAp += 5;

            if (Job == MapleJob.Ares || Job == MapleJob.Ares1 || Job == MapleJob.Ares2 || Job == MapleJob.Ares3 ||
                Job == MapleJob.Ares4)
            {
                MaxHp += Rand(24, 28);
                MaxMp += Rand(4, 6);
            }
            if (Job == MapleJob.Beginner || Job == MapleJob.Knight)
            {
                MaxHp += Rand(12, 16);
                MaxMp += Rand(10, 12);
            }
            else if (Job == MapleJob.Warrior || Job == MapleJob.GhostKnight || Job == MapleJob.Ares1)
            {
                improvingMaxHp = SkillFactory.GetSkill(1000001);
                improvingMaxHpLevel = GetSkillLevel(improvingMaxHp);
                if (Job == MapleJob.GhostKnight)
                {
                    improvingMaxHp = SkillFactory.GetSkill(11000000);
                    improvingMaxHpLevel = GetSkillLevel(improvingMaxHp);
                }
                MaxHp += Rand(24, 28);
                MaxMp += Rand(4, 6);
            }
            else if (Job == MapleJob.Magician || Job == MapleJob.FireKnight)
            {
                improvingMaxMp = SkillFactory.GetSkill(2000001);
                improvingMaxMpLevel = GetSkillLevel(improvingMaxMp);
                if (Job == MapleJob.FireKnight)
                {
                    improvingMaxMp = SkillFactory.GetSkill(12000000);
                    improvingMaxMpLevel = GetSkillLevel(improvingMaxMp);
                }
                MaxHp += Rand(10, 14);
                MaxMp += Rand(22, 24);
            }
            else if (Job == MapleJob.Bowman || Job == MapleJob.Thief || Job == MapleJob.Gm || Job == MapleJob.WindKnight ||
                     Job == MapleJob.NightKnight)
            {
                MaxHp += Rand(20, 24);
                MaxMp += Rand(14, 16);
            }
            else if (Job == MapleJob.Pirate || Job == MapleJob.ThiefKnight)
            {
                improvingMaxHp = SkillFactory.GetSkill(5100000);
                improvingMaxHpLevel = GetSkillLevel(improvingMaxHp);
                if (Job == MapleJob.GhostKnight)
                {
                    improvingMaxHp = SkillFactory.GetSkill(15100000);
                    improvingMaxHpLevel = GetSkillLevel(improvingMaxHp);
                }
                MaxHp += Rand(22, 26);
                MaxMp += Rand(18, 23);
            }
            if (improvingMaxHpLevel > 0 && improvingMaxHp != null)
            {
                MaxHp += (short)improvingMaxHp.GetEffect(improvingMaxHpLevel).X;
            }
            if (improvingMaxMpLevel > 0 && improvingMaxMp != null)
            {
                MaxMp += (short)improvingMaxMp.GetEffect(improvingMaxMpLevel).X;
            }

            MaxMp += (short)(Localint / 10);
            Exp.Add(-ExpTable.GetExpNeededForLevel(Level + 1));
            Level++;

            DropMessage($"[系统信息] 升级了！恭喜你升到 {Level} 级!");
            if (Exp.Value > 0)
            {
                Exp.Reset();
            }
            if (Level == 200)
            {
                Exp.Reset();
                var packet = PacketCreator.ServerNotice(0, $"祝贺 {Name} 到达200级！");
                try
                {
                    // Client.ChannelServer.getWorldInterface().broadcastMessage(CharacterName, packet);
                }
                catch
                {
                    //getClient().getChannelServer().reconnectWorld();
                }
            }

            MaxHp = Math.Min((short)30000, MaxHp);
            MaxMp = Math.Min((short)30000, MaxMp);

            var statup = new List<Tuple<MapleStat, int>>(8)
            {
                new Tuple<MapleStat, int>(MapleStat.Availableap, RemainingAp),
                new Tuple<MapleStat, int>(MapleStat.Maxhp, MaxHp),
                new Tuple<MapleStat, int>(MapleStat.Maxmp, MaxMp),
                new Tuple<MapleStat, int>(MapleStat.Hp, MaxHp),
                new Tuple<MapleStat, int>(MapleStat.Mp, MaxMp),
                new Tuple<MapleStat, int>(MapleStat.Exp, Exp.Value),
                new Tuple<MapleStat, int>(MapleStat.Level, Level)
            };

            if (Job != MapleJob.Beginner)
            {
                RemainingSp += 3;
                statup.Add(new Tuple<MapleStat, int>(MapleStat.Availablesp, RemainingSp));
            }

            Hp = MaxHp;
            Mp = MaxMp;

            if (Job == MapleJob.GhostKnight && Level <= 10)
            {
                Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, "骑士团职业已经需求"));
            }

            if (Level >= 30)
            {
                //finishAchievement(3);
            }
            if (Level >= 70)
            {
                // finishAchievement(4);
            }
            if (Level >= 120)
            {
                //finishAchievement(5);
            }
            if (Level == 200)
            {
                //finishAchievement(22);
            }

            /*@升级送点
             *@10.30.70.120.200可以送
             *@全程为7000点卷.
             */

            if (Level == 30)
            {
                GainNexonPoint(1000);
                DropMessage($"恭喜您等级达到 {Level} 级!得到了{1000}点卷奖励!");
            }
            if (Level == 70)
            {
                GainNexonPoint(1500);
                DropMessage($"恭喜您等级达到 {Level} 级!得到了{1500}点卷奖励!");
            }
            if (Level == 120)
            {
                GainNexonPoint(2000);
                DropMessage($"恭喜您等级达到 {Level} 级!得到了{2000}点卷奖励!");
            }
            if (Level == 200)
            {
                GainNexonPoint(2500);
                DropMessage($"恭喜您等级达到 {Level} 级!得到了{2500}点卷奖励!");
            }


            Client.Send(PacketCreator.UpdatePlayerStats(statup));
            Map.BroadcastMessage(this, PacketCreator.ShowLevelup(Id), false);
            RecalcLocalStats();
            SilentPartyUpdate();
            GuildUpdate();
        }

        public void ChangeSkillLevel(ISkill skill, byte newLevel, byte newMasterlevel)
        {
            if (Skills.ContainsKey(skill))
                Skills[skill] = new SkillEntry(newLevel, newMasterlevel);
            else
                Skills.Add(skill, new SkillEntry(newLevel, newMasterlevel));
            Client.Send(PacketCreator.UpdateSkill(skill.SkillId, newLevel, newMasterlevel));
        }


        public void UpdateQuest(MapleQuestStatus quest)
        {
            m_quests.Add(quest.Quest, quest);
            var questId = quest.Quest.GetQuestId();
            if (questId == 4760 || questId == 4761 || questId == 4762 || questId == 4763 || questId == 4764 ||
                questId == 4765 || questId == 4766 || questId == 4767 || questId == 4768 || questId == 4769 ||
                questId == 4770 || questId == 4771)
            {
                Client.Send(PacketCreator.CompleteQuest(this, (short)questId));
                Client.Send(PacketCreator.UpdateQuestInfo(this, (short)questId, quest.Npcid, 8));
            }
            else
            {
                if (quest.Status == MapleQuestStatusType.Started)
                {
                    Client.Send(PacketCreator.StartQuest(this, (short)questId));
                    Client.Send(PacketCreator.UpdateQuestInfo(this, (short)questId, quest.Npcid, 8));
                }
                else if (quest.Status == MapleQuestStatusType.Completed)
                {
                    Client.Send(PacketCreator.CompleteQuest(this, (short)questId));
                }
                else if (quest.Status == MapleQuestStatusType.NotStarted)
                {
                    Client.Send(PacketCreator.ForfeitQuest(this, (short)questId));
                }
            }
        }

        public List<MapleQuestStatus> GetStartedQuests()
        {
            var ret = new List<MapleQuestStatus>();
            foreach (var questStatus in m_quests.Values)
            {
                if (questStatus.Status == MapleQuestStatusType.Completed)
                {
                }
                else if (questStatus.Status == MapleQuestStatusType.Started)
                {
                    ret.Add(questStatus);
                }
            }
            return ret;
        }

        public List<MapleQuestStatus> GetCompletedQuests()
        {
            var ret = new List<MapleQuestStatus>();
            foreach (var questStatus in m_quests.Values)
            {
                if (questStatus.Status == MapleQuestStatusType.Completed)
                {
                    ret.Add(questStatus);
                }
            }
            return ret;
        }

        public void UpdateSingleStat(MapleStat stat, int newval, bool itemReaction = false)
        {
            var statpair = new Tuple<MapleStat, int>(stat, newval);
            Client.Send(PacketCreator.UpdatePlayerStats(new List<Tuple<MapleStat, int>> { statpair }, itemReaction));
        }

        public void SilentPartyUpdate()
        {
            //if (Party != null)
            //{
            //    try
            //    {
            //        getClient().getChannelServer().getWorldInterface().updateParty(Party.PartyID, PartyOperation.SILENT_UPDATE, new MaplePartyCharacter(MapleCharacter.this));
            //    }
            //    catch
            //    {
            //        log.error("REMOTE THROW", e);
            //        getClient().getChannelServer().reconnectWorld();
            //    }
            //}
        }

        public void GuildUpdate()
        {
            //if (this.guildid <= 0)
            //{
            //    return;
            //}

            //mgc.setLevel(this.level);
            //mgc.setJobId(this.job.getId());

            //try
            //{
            //    this.client.getChannelServer().getWorldInterface().memberLevelJobUpdate(this.mgc);
            //}
            //catch (RemoteException re)
            //{
            //    log.error("RemoteExcept while trying to update level/job in guild.", re);
            //}
        }

        public void SendMacros()
        {
            var macros = false;
            for (var i = 0; i < 5; i++)
            {
                if (m_skillMacros[i] != null)
                {
                    macros = true;
                }
            }
            if (macros)
            {
                Client.Send(PacketCreator.GetMacros(m_skillMacros));
            }
        }

        public void ShowNote()
        {
            //Connection con = DatabaseConnection.getConnection();
            //PreparedStatement ps = con.prepareStatement("SELECT * FROM notes WHERE `to`=?", ResultSet.TYPE_SCROLL_SENSITIVE, ResultSet.CONCUR_UPDATABLE);
            //ps.setString(1, this.getName());
            //ResultSet rs = ps.executeQuery();

            //rs.last();
            //int count = rs.getRow();
            //rs.first();
            Client.Send(PacketCreator.ShowNotes(0));
            //ps.close();
        }

        public byte GetPetSlot(MaplePet pet)
        {
            if (pet == null) return 0xFF;
            if (!Pets.Any()) return 0xFF;
            for (byte i = 0; i < Pets.Count; i++)
            {
                if (Pets[i] == null) continue;
                if (Pets[i].UniqueId == pet.UniqueId)
                {
                    return i;
                }
            }
            return 0xFF;
        }

        public byte GetPetByUniqueId(int uniqueid)
        {
            if (!Pets.Any()) return 0xFF;
            for (byte i = 0; i < Pets.Count; i++)
            {
                if (Pets[i] == null) continue;
                if (Pets[i].UniqueId == uniqueid)
                {
                    return i;
                }
            }
            return 0xFF;
        }

        public void UnequipPet(MaplePet pet, bool shiftLeft, bool hunger=false)
        {
            CancelFullnessSchedule(GetPetSlot(pet));
            pet.Save();
            Map.BroadcastMessage(this, PacketCreator.ShowPet(this, pet, true, hunger), true);
            var stats = new List<Tuple<MapleStat, int>> {Tuple.Create(MapleStat.Pet, 0)};
            Client.Send(PacketCreator.PetStatUpdate(this));
            Client.Send(PacketCreator.EnableActions());
            RemovePet(pet, shiftLeft);
        }

        public void RemovePet(MaplePet pet, bool shiftLeft)
        {
            int petslot = GetPetSlot(pet);
            Pets.RemoveAt(petslot);
            Client.Send(PacketCreator.PetStatUpdate(this));
        }

        public void AddPet(MaplePet pet, bool lead)
        {
            if (Pets.Count < 3)
            {
                if (lead)
                {
                    List<MaplePet> newpets = new List<MaplePet>();
                    newpets.Add(pet);
                    foreach (MaplePet oldpet in Pets)
                    {
                        newpets.Add(oldpet);
                    }
                    Pets = newpets;
                    //fullnessSchedule_2 = fullnessSchedule_1;
                    //fullnessSchedule_1 = fullnessSchedule;
                }
                else
                {
                    Pets.Add(pet);
                }
            }
        }

        public void StartFullnessSchedule(byte decrease, MaplePet pet, int petSlot)
        {
            var schedule = TimerManager.Instance.RepeatTask(() =>
            {

                byte newFullness = (byte)(pet.PetInfo.Fullness - decrease);
                if (newFullness <= 5)
                {
                    pet.PetInfo.Fullness = 15;
                    UnequipPet(pet, true, true);
                }
                else
                {
                    pet.PetInfo.Fullness = newFullness;
                    Client.Send(PacketCreator.UpdatePet(pet, true));
                }

            }, 60000, 60000);

            switch (petSlot)
            {
                case 0:
                    m_fullnessSchedule = schedule;
                    break;
                case 1:
                    m_fullnessSchedule1 = schedule;
                    break;
                case 2:
                    m_fullnessSchedule2 = schedule;
                    break;
            }
        }

        public void CancelFullnessSchedule(int petSlot)
        {
            switch (petSlot)
            {
                case 0:
                    TimerManager.Instance.CancelTask(m_fullnessSchedule);
                    break;
                case 1:
                    TimerManager.Instance.CancelTask(m_fullnessSchedule1);
                    break;
                case 2:
                    TimerManager.Instance.CancelTask(m_fullnessSchedule2);
                    break;
            }
        }

        public MapleStatEffect GetStatForBuff(MapleBuffStat effect)
        {
            MapleBuffStatValueHolder mbsvh;
            return !m_effects.TryGetValue(effect, out mbsvh) ? null : mbsvh.Effect;
        }

        public void UpdatePartyMemberHp()
        {
            if (Party != null)
            {
                int channel = Client.ChannelId;
                Party.GetMembers().ForEach(partychr =>
                {
                    if (partychr.MapId == Map.MapId && partychr.ChannelId == channel)
                    {
                        var other =
                            MasterServer.Instance.ChannelServers[channel].Characters.FirstOrDefault(
                                x => x.Name == partychr.CharacterName);
                        other?.Client.Send(PacketCreator.UpdatePartyMemberHp(Id, Hp, Localmaxhp));
                    }
                });
            }
        }

        public void RegisterEffect(MapleStatEffect effect, long starttime, TriggerKey triggerKey)
        {
            //if (effect.IsHide())
            //{
            //    this.IsHidden = true;
            //    Map.BroadcastNonGmMessage(this, PacketCreator.RemovePlayerFromMap(Id));
            //    // getMap().broadcastMessage(this, MaplePacketCreator.removePlayerFromMap(getId()), false);
            //}
            //else if (effect.IsDragonBlood())
            //{
            //    prepareDragonBlood(effect);
            //}
            //else if (effect.IsBerserk())
            //{
            //    checkBerserk();
            //}
            //else if (effect.IsBeholder())
            //{
            //    prepareBeholderEffect();
            //}

            foreach (var statup in effect.GetStatups())
            {
                var value = new MapleBuffStatValueHolder(effect, starttime, triggerKey, statup.Item2);
                if (m_effects.ContainsKey(statup.Item1))
                    m_effects[statup.Item1] = value;
                else
                    m_effects.Add(statup.Item1, value);
            }
            RecalcLocalStats();
        }

        public void ReceivePartyMemberHp()
        {
            if (Party != null)
            {
                int channel = Client.ChannelId;
                Party.GetMembers().ForEach(partychr =>
                {
                    if (partychr.MapId == Map.MapId && partychr.ChannelId == channel)
                    {
                        var other =
                            MasterServer.Instance.ChannelServers[channel].Characters.FirstOrDefault(
                                x => x.Name == partychr.CharacterName);
                        if (other != null)
                        {
                            Client.Send(PacketCreator.UpdatePartyMemberHp(other.Id, other.Hp, other.Localmaxhp));
                        }
                    }
                });
            }
        }


        public void LeaveMap()
        {
            m_controlled.Clear();
            VisibleMapObjects.Clear();
            if (Chair != 0)
            {
                Chair = 0;
            }
            //if (hpDecreaseTask != null)
            //{
            //    hpDecreaseTask.cancel(false);
            //}
        }

        public void CheckMonsterAggro(MapleMonster monster)
        {
            if (monster.ControllerHasAggro) return;
            if (monster.GetController() == this)
            {
                monster.ControllerHasAggro = true;
            }
            else
            {
                monster.SwitchController(this, true);
            }
        }

        public int GetBuffSource(MapleBuffStat stat)
        {
            MapleBuffStatValueHolder mbsvh;
            if (!m_effects.TryGetValue(stat, out mbsvh))
            {
                return -1;
            }

            return mbsvh.Effect.GetSourceId();
        }

        public void MobKilled(int id)
        {
            foreach (var q in m_quests.Values)
            {
                if (MapleQuest.GetInstance(q.Quest.QuestId).NullCompleteQuestData())
                {
                    //reloadQuest(MapleQuest.getInstance(q.getQuest().getId()));
                }
                if (q.Status == MapleQuestStatusType.Completed || q.Quest.CanComplete(this, null))
                {
                    continue;
                }

                if (!q.MobKilled(id) /*&& !q.Quest is maplecustomQuest*/) continue;

                //Client.Send(PacketCreator.updateQuestMobKills(q));
                if (q.Quest.CanComplete(this, null))
                {
                    //Client.Send(PacketCreator.getShowQuestCompletion(q.Quest.QuestId));
                }
            }
        }

        public void CheckBerserk()
        {
            //if (BerserkSchedule != null)
            //{
            //    BerserkSchedule.cancel(false);
            //}

            //MapleCharacter chr = this;
            //ISkill berserkX = SkillFactory.GetSkill(1320006);
            //int skilllevel = GetSkillLevel(berserkX);
            //if (chr.Job == MapleJob.Darkknight && skilllevel >= 1)
            //{
            //    MapleStatEffect ampStat = berserkX.GetEffect(skilllevel);
            //    int x = ampStat.X;
            //    int hp = chr.Hp;
            //    int mhp = chr.MaxHp;
            //    int ratio = hp * 100 / mhp;
            //    if (ratio > x)
            //    {
            //        //Berserk = false;
            //    }
            //    else
            //    {
            //        //Berserk = true;
            //    }

            //            BerserkSchedule = TimerManager.getInstance().register(new Runnable() {

            //            @Override
            //            public void run()
            //    {
            //        //getClient().getSession().write(MaplePacketCreator.showOwnBerserk(skilllevel, Berserk));
            //        //getMap().broadcastMessage(MapleCharacter.this, MaplePacketCreator.showBerserk(getId(), skilllevel, Berserk), false);
            //    }
            //}, 5000, 3000);
            //}
        }

        private void PlayerDead()
        {
            //if (getEventInstance() != null)
            //{
            //    getEventInstance().playerKilled(this);
            //}

            //dispelSkill(0);
            //cancelAllDebuffs();
            //cancelMorphs();

            //int[] charmID = { 5130000, 5130002, 5131000, 4031283, 4140903 };
            //MapleCharacter player = Client.Character;
            //int possesed = 0;
            //int i;

            ////Check for charms
            //for (i = 0; i < charmID.Length; i++)
            //{
            //    int quantity = getItemQuantity(charmID[i], false);
            //    if (possesed == 0 && quantity > 0)
            //    {
            //        possesed = quantity;
            //        break;
            //    }
            //}

            //if (possesed > 0 && !getMap().hasEvent())
            //{
            //    possesed -= 1;
            //    getClient().getSession().write(MaplePacketCreator.serverNotice(5, "��ʹ���� [�����] ���������ľ��鲻����٣�ʣ�� (" + possesed + " ��)"));
            //    MapleInventoryManipulator.removeById(getClient(), MapleItemInformationProvider.getInstance().getInventoryType(charmID[i]), charmID[i], 1, true, false);
            //}
            //else if (getMap().hasEvent())
            //{
            //    getClient().getSession().write(MaplePacketCreator.serverNotice(5, "�������ͼ�����������ľ���ֵ������١�"));
            //}
            //else if (player.getJob() != MapleJob.BEGINNER || player.getJob() != MapleJob.KNIGHT || player.getJob() != MapleJob.Ares)
            //{
            //    //Lose XP
            //    int XPdummy = ExpTable.getExpNeededForLevel(player.getLevel() + 1);
            //    if (player.getMap().isTown())
            //    {
            //        XPdummy *= 0.01;
            //    }
            //    if (XPdummy == ExpTable.getExpNeededForLevel(player.getLevel() + 1))
            //    {
            //        if (player.getLuk() <= 100 && player.getLuk() > 8)
            //        {
            //            XPdummy *= 0.10 - (player.getLuk() * 0.0005);
            //        }
            //        else if (player.getLuk() < 8)
            //        {
            //            XPdummy *= 0.10; //Otherwise they lose about 9 percent
            //        }
            //        else {
            //            XPdummy *= 0.10 - (100 * 0.0005);
            //        }
            //    }
            //    if ((player.getExp() - XPdummy) > 0)
            //    {
            //        player.gainExp(-XPdummy, false, false);
            //    }
            //    else {
            //        player.gainExp(-player.getExp(), false, false);
            //    }
            //}
            //if (getBuffedValue(MapleBuffStat.MORPH) != null)
            //{
            //    cancelEffectFromBuffStat(MapleBuffStat.MORPH);
            //}
            //if (getBuffedValue(MapleBuffStat.MONSTER_RIDING) != null)
            //{
            //    cancelEffectFromBuffStat(MapleBuffStat.MONSTER_RIDING);
            //}
            //client.getSession().write(PacketCreator.EnableActions());
        }

        private void EnforceMaxHpMp()
        {
            var stats = new List<Tuple<MapleStat, int>>(2);
            if (Mp > Localmaxmp)
            {
                Mp = Localmaxmp;
                stats.Add(new Tuple<MapleStat, int>(MapleStat.Mp, Mp));
            }
            if (Hp > Localmaxhp)
            {
                Hp = Localmaxhp;
                stats.Add(new Tuple<MapleStat, int>(MapleStat.Hp, Hp));
            }
            if (stats.Any())
            {
                Client.Send(PacketCreator.UpdatePlayerStats(stats));
            }
        }

        public void CancelAllBuffs()
        {
            var allBuffs = new List<MapleBuffStatValueHolder>(m_effects.Count);
            foreach (var mbsvh in allBuffs)
            {
                CancelEffect(mbsvh.Effect, false, mbsvh.StartTime);
            }
        }
        public void CancelEffectFromBuffStat(MapleBuffStat stat)
        {
            MapleBuffStatValueHolder value;
            if (m_effects.TryGetValue(stat, out value))
            {
                CancelEffect(value.Effect, false, -1);
            }
        }

        public void CancelEffect(MapleStatEffect effect, bool overwrite, long startTime)
        {
            List<MapleBuffStat> buffstats;
            if (!overwrite)
            {
                buffstats = GetBuffStats(effect, startTime);
            }
            else
            {
                var statups = effect.GetStatups();
                buffstats = new List<MapleBuffStat>(statups.Count);
                foreach (var statup in statups)
                {
                    buffstats.Add(statup.Item1);
                }
            }
            DeregisterBuffStats(buffstats);
            if (effect.IsMagicDoor())
            {
                // remove for all on maps
                if (Doors.Any())
                {
                    Doors.GetEnumerator().MoveNext();
                    var door = Doors.GetEnumerator().Current;
                    foreach (var chr in door.TargetMap.Characters)
                    {
                        door.SendDestroyData(chr.Client);
                    }
                    foreach (var chr in door.Town.Characters)
                    {
                        door.SendDestroyData(chr.Client);
                    }
                    foreach (var destroyDoor in Doors)
                    {
                        door.TargetMap.RemoveMapObject(destroyDoor);
                        door.Town.RemoveMapObject(destroyDoor);
                    }
                    Doors.Clear();
                    SilentPartyUpdate();
                }
            }
            if (!overwrite)
            {
                CancelPlayerBuffs(buffstats);
                if (effect.IsHide() && Map.Mapobjects.ContainsKey(ObjectId))
                {
                    IsHidden = false;
                    Map.BroadcastMessage(this, PacketCreator.SpawnPlayerMapobject(this), false);
                    foreach (var pett in Pets)
                    {
                        Map.BroadcastMessage(this, PacketCreator.ShowPet(this, pett, false, false), false);
                    }
                }
            }
        }

        public bool HaveItem(int itemid, int quantity, bool checkEquipped, bool exact)
        {
            // if exact is true, then possessed must be EXACTLY equal to quantity. else, possessed can be >= quantity
            var type = MapleItemInformationProvider.Instance.GetInventoryType(itemid);
            var iv = Inventorys[type.Value];
            var possessed = iv.CountById(itemid);
            if (checkEquipped)
            {
                possessed += Inventorys[MapleInventoryType.Equipped.Value].CountById(itemid);
            }
            if (exact)
            {
                return possessed == quantity;
            }
            return possessed >= quantity;
        }

        public bool HaveItem(int[] itemids, int quantity, bool exact)
        {
            foreach (var itemid in itemids)
            {
                var type = MapleItemInformationProvider.Instance.GetInventoryType(itemid);
                var iv = Inventorys[type.Value];
                var possessed = iv.CountById(itemid);
                possessed += Inventorys[MapleInventoryType.Equipped.Value].CountById(itemid);
                if (possessed >= quantity)
                {
                    if (exact)
                    {
                        if (possessed == quantity)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetSavedLocation(SavedLocationType type)=> m_savedLocations[(int)type];


        public void SaveLocation(SavedLocationType type)=> m_savedLocations[(int)type] = Map.MapId;


        public void ClearSavedLocation(SavedLocationType type)=> m_savedLocations[(int)type] = -1;


        public void ControlMonster(MapleMonster monster, bool aggro)
        {
            monster.SetController(this);
            m_controlled.Add(monster);
            Client.Send(PacketCreator.ControlMonster(monster, false, aggro));
        }

        public void StopControllingMonster(MapleMonster monster)
        {
            m_controlled.Remove(monster);
        }

        public List<MapleMonster> GetControlledMonsters()
        {
            return m_controlled;
        }

        public void ResetAfkTimer()
        {
            m_afkTimer = DateTime.Now.GetTimeMilliseconds();
        }
        public long GetAfkTimer()
        {
            return DateTime.Now.GetTimeMilliseconds() - m_afkTimer;
        }

        public void ChangeJob(MapleJob newJob)
        {
            Job = newJob;
            RemainingSp++;
            if (newJob.JobId % 10 == 2)
            {
                RemainingSp += 2;
            }
            UpdateSingleStat(MapleStat.Availablesp, RemainingSp);
            UpdateSingleStat(MapleStat.Job, newJob.JobId);
            switch (Job.JobId)
            {
                case 100:
                    MaxHp += Rand(200, 250);
                    break;
                case 110:
                case 111:
                case 112:
                    MaxHp += Rand(300, 350);
                    break;
                case 120:
                case 121:
                case 122:
                case 130:
                case 131:
                case 132:
                case 200:
                    MaxMp += Rand(100, 150);
                    break;
                case 210:
                case 211:
                case 212:
                case 220:
                case 221:
                case 222:
                case 230:
                case 231:
                case 232:
                    MaxMp += Rand(450, 500);
                    break;
                case 300:
                case 400:
                case 500:
                    MaxHp += Rand(100, 150);
                    MaxMp += Rand(30, 50);
                    break;
                case 310:
                case 311:
                case 312:
                case 320:
                case 321:
                case 322:
                case 410:
                case 411:
                case 412:
                case 420:
                case 421:
                case 422:
                case 510:
                case 511:
                case 512:
                case 520:
                case 521:
                case 522:
                    MaxHp += Rand(300, 350);
                    MaxMp += Rand(150, 200);
                    break;
            }
            if (MaxHp > 30000)
            {
                MaxHp = 30000;
            }
            if (MaxMp > 30000)
            {
                MaxMp = 30000;
            }
            /*if (((MapleJob.SNIPER)) || (this.level >= 10)) {
          if (this.level == 10) {
            changeJob(MapleJob.SNIPER);
                   }
            }*/
            Hp = MaxHp;
            Mp = MaxMp;
            List<Tuple<MapleStat, int>> statup = new List<Tuple<MapleStat, int>>(2);
            statup.Add(new Tuple<MapleStat, int>(MapleStat.Maxhp, MaxHp));
            statup.Add(new Tuple<MapleStat, int>(MapleStat.Maxmp, MaxMp));
            RecalcLocalStats();
            Client.Send(PacketCreator.UpdatePlayerStats(statup));
            Map.BroadcastMessage(this, PacketCreator.ShowJobChange(Id), false);
            SilentPartyUpdate();
            GuildUpdate();
        }

        public void HandleOrbconsume()
        {
            ISkill combo = SkillFactory.GetSkill(1111002);
            if (GetSkillLevel(combo) == 0)
            {
                combo = SkillFactory.GetSkill(11111001);
            }
            MapleStatEffect ceffect = combo.GetEffect(GetSkillLevel(combo));
            List<Tuple<MapleBuffStat, int>> stat = new List<Tuple<MapleBuffStat, int>> { new Tuple<MapleBuffStat, int>(MapleBuffStat.Combo, 1) };
            SetBuffedValue(MapleBuffStat.Combo, 1);
            int duration = ceffect.Duration;
            duration += (int)(GetBuffedStarttime(MapleBuffStat.Combo) ?? DateTime.Now.GetTimeMilliseconds() - DateTime.Now.GetTimeMilliseconds());
            if (GetSkillLevel(combo) == 0)
            {
                Client.Send(PacketCreator.GiveBuff(this, 11111001, duration, stat));
            }
            else
            {
                Client.Send(PacketCreator.GiveBuff(this, 1111002, duration, stat));
            }
            Map.BroadcastMessage(this, PacketCreator.GiveForeignBuff(this, stat, ceffect), false);
        }

        public void SetBuffedValue(MapleBuffStat effect, int value)
        {
            MapleBuffStatValueHolder mbsvh;
            if (!m_effects.TryGetValue(effect, out mbsvh))
            {
                return;
            }
            mbsvh.Value = value;
        }
        public long? GetBuffedStarttime(MapleBuffStat effect)
        {
            MapleBuffStatValueHolder mbsvh;
            if (!m_effects.TryGetValue(effect, out mbsvh))
            {
                return null;
            }
            return mbsvh.StartTime;
        }
    }

    internal struct MapleBuffStatValueHolder
    {
        public MapleStatEffect Effect { get; }
        public long StartTime { get; }
        public int Value { get; set; }
        public TriggerKey TriggerKey { get; private set; }

        public MapleBuffStatValueHolder(MapleStatEffect effect, long startTime, TriggerKey triggerKey, int value)
        {
            Effect = effect;
            StartTime = startTime;
            TriggerKey = triggerKey;
            Value = value;
        }
    }

    public class CancelCooldownAction
    {
        private readonly WeakReference<MapleCharacter> m_char;
        private readonly int m_skillId;

        public CancelCooldownAction(MapleCharacter target, int skillid)
        {
            m_char = new WeakReference<MapleCharacter>(target);
            m_skillId = skillid;
        }
        public void Run()
        {
            MapleCharacter realTarget;
            if (m_char.TryGetTarget(out realTarget))
            {
                realTarget.RemoveCooldown(m_skillId);
            }
        }
    }
}