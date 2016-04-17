namespace NeoMapleStory.Game.Job
{
    public class MapleJob
    {

        public static MapleJob Beginner { get; } = new MapleJob(0);
        public static MapleJob Warrior { get; } = new MapleJob(100);
        public static MapleJob Fighter { get; } = new MapleJob(110);
        public static MapleJob Crusader { get; } = new MapleJob(111);
        public static MapleJob Hero { get; } = new MapleJob(112);
        public static MapleJob Page { get; } = new MapleJob(120);
        public static MapleJob Whiteknight { get; } = new MapleJob(121);
        public static MapleJob Paladin { get; } = new MapleJob(122);
        public static MapleJob Spearman { get; } = new MapleJob(130);
        public static MapleJob Dragonknight { get; } = new MapleJob(131);
        public static MapleJob Darkknight { get; } = new MapleJob(132);
        public static MapleJob Magician { get; } = new MapleJob(200);
        public static MapleJob FpWizard { get; } = new MapleJob(210);
        public static MapleJob FpMage { get; } = new MapleJob(211);
        public static MapleJob FpArchmage { get; } = new MapleJob(212);
        public static MapleJob IlWizard { get; } = new MapleJob(220);
        public static MapleJob IlMage { get; } = new MapleJob(221);
        public static MapleJob IlArchmage { get; } = new MapleJob(222);
        public static MapleJob Cleric { get; } = new MapleJob(230);
        public static MapleJob Priest { get; } = new MapleJob(231);
        public static MapleJob Bishop { get; } = new MapleJob(232);
        public static MapleJob Bowman { get; } = new MapleJob(300);
        public static MapleJob Hunter { get; } = new MapleJob(310);
        public static MapleJob Ranger { get; } = new MapleJob(311);
        public static MapleJob Bowmaster { get; } = new MapleJob(312);
        public static MapleJob Crossbowman { get; } = new MapleJob(320);
        public static MapleJob Sniper { get; } = new MapleJob(321);
        public static MapleJob Crossbowmaster { get; } = new MapleJob(322);
        public static MapleJob Thief { get; } = new MapleJob(400);
        public static MapleJob Assassin { get; } = new MapleJob(410);
        public static MapleJob Hermit { get; } = new MapleJob(411);
        public static MapleJob Nightlord { get; } = new MapleJob(412);
        public static MapleJob Bandit { get; } = new MapleJob(420);
        public static MapleJob Chiefbandit { get; } = new MapleJob(421);
        public static MapleJob Shadower { get; } = new MapleJob(422);
        public static MapleJob Pirate { get; } = new MapleJob(500);
        public static MapleJob Brawler { get; } = new MapleJob(510);
        public static MapleJob Marauder { get; } = new MapleJob(511);
        public static MapleJob Buccaneer { get; } = new MapleJob(512);
        public static MapleJob Gunslinger { get; } = new MapleJob(520);
        public static MapleJob Outlaw { get; } = new MapleJob(521);
        public static MapleJob Corsair { get; } = new MapleJob(522);
        public static MapleJob Gm { get; } = new MapleJob(900);
        public static MapleJob Knight { get; } = new MapleJob(1000);
        public static MapleJob GhostKnight { get; } = new MapleJob(1100);
        public static MapleJob GhostKnight2 { get; } = new MapleJob(1110);
        public static MapleJob GhostKnight3 { get; } = new MapleJob(1111);
        public static MapleJob FireKnight { get; } = new MapleJob(1200);
        public static MapleJob FireKnight2 { get; } = new MapleJob(1210);
        public static MapleJob FireKnight3 { get; } = new MapleJob(1211);
        public static MapleJob WindKnight { get; } = new MapleJob(1300);
        public static MapleJob WindKnight2 { get; } = new MapleJob(1310);
        public static MapleJob WindKnight3 { get; } = new MapleJob(1311);
        public static MapleJob NightKnight { get; } = new MapleJob(1400);
        public static MapleJob NightKnight2 { get; } = new MapleJob(1410);
        public static MapleJob NightKnight3 { get; } = new MapleJob(1411);
        public static MapleJob ThiefKnight { get; } = new MapleJob(1500);
        public static MapleJob ThiefKnight2 { get; } = new MapleJob(1510);
        public static MapleJob ThiefKnight3 { get; } = new MapleJob(1511);
        public static MapleJob Ares { get; } = new MapleJob(2000);
        public static MapleJob Ares1 { get; } = new MapleJob(2100);
        public static MapleJob Ares2 { get; } = new MapleJob(2110);
        public static MapleJob Ares3 { get; } = new MapleJob(2111);
        public static MapleJob Ares4 { get; } = new MapleJob(2112);

        public short JobId { get; private set; }

        public MapleJob(short jobId)
        {
            JobId = jobId;
        }

        public static MapleJob GetByJobId(short id)
        {
            var props = typeof(MapleJob).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach (var prop in props)
            {
                MapleJob job = (MapleJob)prop.GetValue(prop, null);
                if (job.JobId == id)
                    return job;
            }
            return null;
        }

        public static bool operator ==(MapleJob job, MapleJob otherjob)
        {
            return job.JobId >= otherjob.JobId && job.JobId / 100 == otherjob.JobId / 100;
        }

        public static bool operator !=(MapleJob job, MapleJob otherjob)
        {
            return job.JobId < otherjob.JobId || job.JobId / 100 != otherjob.JobId / 100;
        }

        public override bool Equals(object obj)
        {
            return this == (MapleJob)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return GetJobName();
        }

        public string GetJobName()
        {
            MapleJob job = this;
            if (job == Beginner)
            {
                return "新手";
            }
            else if (job == Thief)
            {
                return "飞侠";
            }
            else if (job == Warrior)
            {
                return "战士";
            }
            else if (job == Magician)
            {
                return "魔法师";
            }
            else if (job == Bowman)
            {
                return "弓箭手";
            }
            else if (job == Pirate)
            {
                return "海盗";
            }
            else if (job == Bandit)
            {
                return "侠客";
            }
            else if (job == Assassin)
            {
                return "刺客";
            }
            else if (job == Spearman)
            {
                return "枪战士";
            }
            else if (job == Page)
            {
                return "准骑士";
            }
            else if (job == Fighter)
            {
                return "剑客";
            }
            else if (job == Cleric)
            {
                return "牧师";
            }
            else if (job == IlWizard)
            {
                return "冰雷法师";
            }
            else if (job == FpWizard)
            {
                return "火毒法师";
            }
            else if (job == Hunter)
            {
                return "猎人";
            }
            else if (job == Crossbowman)
            {
                return "弩弓手";
            }
            else if (job == Gunslinger)
            {
                return "Gunslinger";
            }
            else if (job == Brawler)
            {
                return "Brawler";
            }
            else if (job == Chiefbandit)
            {
                return "独行客";
            }
            else if (job == Hermit)
            {
                return "无影人";
            }
            else if (job == Dragonknight)
            {
                return "黑骑士";
            }
            else if (job == Whiteknight)
            {
                return "骑士";
            }
            else if (job == Crusader)
            {
                return "勇士";
            }
            else if (job == Paladin)
            {
                return "圣骑士";
            }
            else if (job == Priest)
            {
                return "祭祀";
            }
            else if (job == IlMage)
            {
                return "冰雷/巫师";
            }
            else if (job == FpMage)
            {
                return "火毒/巫师";
            }
            else if (job == Ranger)
            {
                return "射手";
            }
            else if (job == Sniper)
            {
                return "游侠";
            }
            else if (job == Marauder)
            {
                return "Marauder";
            }
            else if (job == Outlaw)
            {
                return "Outlaw";
            }
            else if (job == Shadower)
            {
                return "侠盗";
            }
            else if (job == Nightlord)
            {
                return "隐士";
            }
            else if (job == Darkknight)
            {
                return "Dark Knight";
            }
            else if (job == Hero)
            {
                return "英雄";
            }
            else if (job == Paladin)
            {
                return "圣骑士";
            }
            else if (job == IlArchmage)
            {
                return "魔导师/冰雷";
            }
            else if (job == FpArchmage)
            {
                return "魔导师/火毒";
            }
            else if (job == Bowmaster)
            {
                return "神射手";
            }
            else if (job == Crossbowmaster)
            {
                return "箭神";
            }
            else if (job == Buccaneer)
            {
                return "Buccaneer";
            }
            else if (job == Corsair)
            {
                return "Corsair";
            }
            else {
                return "管理员";
            }
        }

        public static MapleJob GetBy5ByteEncoding(int encoded)
        {
            switch (encoded)
            {
                case 2:
                    return Warrior;
                case 4:
                    return Magician;
                case 8:
                    return Bowman;
                case 16:
                    return Thief;
                case 32:
                    return Pirate;
                default:
                    return Beginner;
            }
        }
    }
}
