using System;
using System.Collections.Generic;
using System.Drawing;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Life;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.World;

namespace NeoMapleStory.Game.Mob
{
    public class MobSkill
    {
        private const double Epsinon = 0.00000001;

        public MobSkill(byte skillId, byte level)
        {
            this.SkillId = skillId;
            SkillLevel = level;
        }

        public byte SkillId { get; }
        public byte SkillLevel { get; }
        public int MpCon { get; set; }
        public List<int> ToSummon { get; } = new List<int>();
        public int SpawnEffect { get; set; }
        public int Hp { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Duration { get; set; }
        public int Cooltime { get; set; }
        public float Prop { get; set; }
        public Point Lt { get; private set; }
        public Point Rb { get; private set; }
        public int Limit { get; set; }
        public int Count { get; set; }

        public void SetLtRb(Point lt, Point rb)
        {
            this.Lt = lt;
            this.Rb = rb;
        }

        public void ApplyEffect(MapleCharacter player, MapleMonster monster, bool skill)
        {
            MonsterStatus? monStat = null;
            MapleDisease? disease = null;
            var heal = false;
            var dispel = false;
            var banish = false;

            switch (SkillId)
            {
                case 100:
                case 110:
                    monStat = MonsterStatus.WeaponAttackUp;
                    break;
                case 101:
                case 111:
                    monStat = MonsterStatus.MagicAttackUp;
                    break;
                case 102:
                case 112:
                    monStat = MonsterStatus.WeaponDefenseUp;
                    break;
                case 103:
                case 113:
                    monStat = MonsterStatus.MagicDefenseUp;
                    break;
                case 114: // Heal
                    heal = true;
                    break;
                case 120:
                    disease = MapleDisease.Seal;
                    break;
                case 121:
                    disease = MapleDisease.Darkness;
                    break;
                case 122:
                    disease = MapleDisease.Weaken;
                    break;
                case 123:
                    disease = MapleDisease.Stun;
                    break;
                case 124:
                    disease = MapleDisease.Curse;
                    break;
                case 125:
                    //disease = MapleDisease.POISON;
                    break;
                case 126:
                    disease = MapleDisease.Slow;
                    break;
                case 127:
                    dispel = true;
                    break;
                case 128: // Seduce
                    if (MakeChanceResult())
                    {
                        // HT is 100%, yet some others mobs aren't - we can handle those later
                        var htSquad = player.Client.ChannelServer.GetMapleSquad(MapleSquadType.Horntail);
                        if (htSquad != null && htSquad.ContainsMember(player))
                        {
                            var i = 0;
                            foreach (var htMember in htSquad.Members)
                            {
                                if (htMember.IsAlive && htMember.Map == player.Map)
                                {
                                    if (i < Count)
                                    {
                                        htMember.GiveDebuff(MapleDisease.Seduce, this);
                                        i++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 129: // Banish
                    if (Lt != Point.Empty && Rb != Point.Empty && skill)
                        foreach (var chr in GetPlayersInRange(monster, player))
                            chr.ChangeMapBanish(monster.Stats.Banish.MapId, monster.Stats.Banish.Portal,
                                monster.Stats.Banish.Msg);
                    else
                        player.ChangeMapBanish(monster.Stats.Banish.MapId, monster.Stats.Banish.Portal,
                            monster.Stats.Banish.Msg);
                    break;
                case 131: // Poison Mist
                    // TODO: make this work
                    break;
                case 140:
                    if (MakeChanceResult() && !monster.MonsterBuffs.Contains(MonsterStatus.MagicImmunity))
                    {
                        monStat = MonsterStatus.WeaponImmunity;
                    }
                    break;
                case 141:
                    if (MakeChanceResult() && !monster.MonsterBuffs.Contains(MonsterStatus.WeaponImmunity))
                    {
                        monStat = MonsterStatus.MagicImmunity;
                    }
                    break;
                case 200:
                    var canSpawn = true;
                    //if (player.getEventInstance() != null)
                    //{
                    //    if (player.getEventInstance().getName().indexOf("FoJ", 0) != -1)
                    //    {
                    //        canSpawn = false;
                    //    }
                    //    else if (player.getEventInstance().getName().indexOf("BossQuest", 0) != -1 && monster.getId() == 8500001)
                    //    {
                    //        canSpawn = false;
                    //    }
                    //}
                    if (monster.Map.SpawnedMonstersOnMap.Value < 50 && canSpawn)
                    {
                        foreach (var mobId in ToSummon)
                        {
                            var toSpawn = MapleLifeFactory.GetMonster(mobId);
                            toSpawn.Position = monster.Position;
                            var xpos = monster.Position.X;
                            var ypos = monster.Position.Y;
                            switch (mobId)
                            {
                                case 8500003: // Pap bomb high
                                    toSpawn.Fh = (int) Math.Ceiling(Randomizer.NextDouble()*19.0);
                                    ypos = -590;
                                    goto case 8500004;
                                case 8500004: // Pap bomb
                                    //Spawn between -500 and 500 from the monsters X position
                                    xpos =
                                        (int) (monster.Position.X + Math.Ceiling(Randomizer.NextDouble()*1000.0) - 500);
                                    if (ypos != -590)
                                    {
                                        ypos = monster.Position.Y;
                                    }
                                    break;
                                case 8510100: //Pianus bomb
                                    if (Math.Abs(Math.Ceiling(Randomizer.NextDouble()*5) - 1) < Epsinon)
                                    {
                                        ypos = 78;
                                        xpos = (int) (0 + Math.Ceiling(Randomizer.NextDouble()*5)) +
                                               (Math.Abs(Math.Ceiling(Randomizer.NextDouble()*2) - 1) < Epsinon
                                                   ? 180
                                                   : 0);
                                    }
                                    else
                                    {
                                        xpos =
                                            (int)
                                                (monster.Position.X + Math.Ceiling(Randomizer.NextDouble()*1000.0) - 500);
                                    }
                                    break;
                            }
                            // Get spawn coordinates (This fixes monster lock)
                            // TODO get map left and right wall. Any suggestions? PM TheRamon
                            switch (monster.Map.MapId)
                            {
                                case 220080001: //Pap map
                                    if (xpos < -890)
                                    {
                                        xpos = (int) (-890 + Math.Ceiling(Randomizer.NextDouble()*150));
                                    }
                                    else if (xpos > 230)
                                    {
                                        xpos = (int) (230 - Math.Ceiling(Randomizer.NextDouble()*150));
                                    }
                                    break;
                                case 230040420: // Pianus map
                                    if (xpos < -239)
                                    {
                                        xpos = (int) (-239 + Math.Ceiling(Randomizer.NextDouble()*150));
                                    }
                                    else if (xpos > 371)
                                    {
                                        xpos = (int) (371 - Math.Ceiling(Randomizer.NextDouble()*150));
                                    }
                                    break;
                            }
                            toSpawn.Position = new Point(xpos, ypos);
                            monster.Map.SpawnMonsterWithEffect(toSpawn, SpawnEffect, toSpawn.Position);
                        }
                    }
                    break;
            }

            if (monStat != null || heal)
            {
                if (Lt != Point.Empty && Rb != Point.Empty && skill)
                {
                    var objects = GetObjectsInRange(monster, MapleMapObjectType.Monster);
                    if (heal)
                    {
                        foreach (var mons in objects)
                        {
                            ((MapleMonster) mons).Heal(X, Y);
                        }
                    }
                    else
                    {
                        foreach (var mons in objects)
                        {
                            if (!monster.MonsterBuffs.Contains(monStat.Value))
                            {
                                ((MapleMonster) mons).ApplyMonsterBuff(monStat.Value, X, SkillId, Duration, this);
                            }
                        }
                    }
                }
                else
                {
                    if (heal)
                    {
                        monster.Heal(X, Y);
                    }
                    else
                    {
                        if (!monster.MonsterBuffs.Contains(monStat.Value))
                        {
                            monster.ApplyMonsterBuff(monStat.Value, X, SkillId, Duration, this);
                        }
                    }
                }
            }

            if (disease != null || dispel || banish)
            {
                if (skill && Lt != Point.Empty && Rb != Point.Empty)
                {
                    var characters = GetPlayersInRange(monster, player);
                    foreach (var character in characters)
                    {
                        if (dispel)
                        {
                            character.Dispel();
                        }
                        else if (banish)
                        {
                            if (player.EventInstanceManager == null)
                            {
                                var to = player.Map.ReturnMap;
                                var pto = to.GetPortal((short) (0 + 10*Randomizer.NextDouble()));
                                character.ChangeMap(to, pto);
                            }
                        }
                        else
                        {
                            character.GiveDebuff(disease.Value, this);
                        }
                    }
                }
                else
                {
                    if (dispel)
                    {
                        player.Dispel();
                    }
                    else
                    {
                        player.GiveDebuff(disease.Value, this);
                    }
                }
            }
            monster.UsedSkill(SkillId, SkillLevel, Cooltime);
            monster.Mp -= MpCon;
        }

        public bool MakeChanceResult()
        {
            return Math.Abs(Prop - 1.0) < Epsinon || Randomizer.NextDouble() < Prop;
        }

        private Rectangle CalculateBoundingBox(Point posFrom, bool facingLeft)
        {
            Point mylt;
            Point myrb;
            if (facingLeft)
            {
                mylt = new Point(Lt.X + posFrom.X, Lt.Y + posFrom.Y);
                myrb = new Point(Rb.X + posFrom.X, Rb.Y + posFrom.Y);
            }
            else
            {
                myrb = new Point(Lt.X*-1 + posFrom.X, Rb.Y + posFrom.Y);
                mylt = new Point(Rb.X*-1 + posFrom.X, Lt.Y + posFrom.Y);
            }
            var bounds = new Rectangle(mylt.X, mylt.Y, myrb.X - mylt.X, myrb.Y - mylt.Y);
            return bounds;
        }

        private List<MapleCharacter> GetPlayersInRange(MapleMonster monster, MapleCharacter player)
        {
            var bounds = CalculateBoundingBox(monster.Position, monster.IsFacingLeft);
            var players = new List<MapleCharacter>();
            players.Add(player);
            return monster.Map.GetPlayersInRect(bounds, players);
        }

        private List<IMapleMapObject> GetObjectsInRange(MapleMonster monster, MapleMapObjectType objectType)
        {
            var bounds = CalculateBoundingBox(monster.Position, monster.IsFacingLeft);
            var objectTypes = new List<MapleMapObjectType>();
            objectTypes.Add(objectType);
            return monster.Map.GetMapObjectsInRect(bounds, objectTypes);
        }
    }
}