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
        private const double EPSINON = 0.00000001;

        public byte skillId { get; private set; }
        public byte skillLevel { get; private set; }
        public int mpCon { get; set; }
        public List<int> toSummon { get; private set; } = new List<int>();
        public int spawnEffect { get; set; }
        public int hp { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int duration { get; set; }
        public int cooltime { get; set; }
        public float prop { get; set; }
        public Point lt { get; private set; }
        public Point rb { get; private set; }
        public int limit { get; set; }
        public int count { get; set; }

        public MobSkill(byte skillId, byte level)
        {
            this.skillId = skillId;
            this.skillLevel = level;
        }

        public void SetLtRb(Point lt, Point rb)
        {
            this.lt = lt;
            this.rb = rb;
        }

        public void applyEffect(MapleCharacter player, MapleMonster monster, bool skill)
        {
            MonsterStatus? monStat = null;
            MapleDisease? disease = null;
            bool heal = false;
            bool dispel = false;
            bool banish = false;

            switch (skillId)
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
                    if (makeChanceResult())
                    { // HT is 100%, yet some others mobs aren't - we can handle those later
                        MapleSquad htSquad = player.Client.ChannelServer.getMapleSquad(MapleSquadType.Horntail);
                        if (htSquad != null && htSquad.ContainsMember(player))
                        {
                            int i = 0;
                            foreach (MapleCharacter htMember in htSquad.Members)
                            {
                                if (htMember.IsAlive && htMember.Map == player.Map)
                                {
                                    if (i < count)
                                    {
                                        htMember.giveDebuff(MapleDisease.Seduce, this);
                                        i++;
                                    }
                                    else {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 129: // Banish
                    //if (lt != Point.Empty && rb != Point.Empty && skill)
                    //    foreach (MapleCharacter chr in getPlayersInRange(monster, player))
                    //        chr.changeMapBanish(monster.Stats.Banish.MapId, monster.Stats.Banish.Portal,
                    //            monster.Stats.Banish.Msg);
                    //else
                    //    player.changeMapBanish(monster.Stats.Banish.MapId, monster.Stats.Banish.Portal,
                    //        monster.Stats.Banish.Msg);
                    break;
                case 131: // Poison Mist
                          // TODO: make this work
                    break;
                case 140:
                    if (makeChanceResult() && !monster.MonsterBuffs.Contains(MonsterStatus.MagicImmunity))
                    {
                        monStat = MonsterStatus.WeaponImmunity;
                    }
                    break;
                case 141:
                    if (makeChanceResult() && !monster.MonsterBuffs.Contains(MonsterStatus.WeaponImmunity))
                    {
                        monStat = MonsterStatus.MagicImmunity;
                    }
                    break;
                case 200:
                    bool canSpawn = true;
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
                        foreach (int mobId in toSummon)
                        {
                            MapleMonster toSpawn = MapleLifeFactory.GetMonster(mobId);
                            toSpawn.Position = (monster.Position);
                            var xpos = monster.Position.X;
                            var ypos = monster.Position.Y;
                            switch (mobId)
                            {
                                case 8500003: // Pap bomb high
                                    toSpawn.Fh = ((int)Math.Ceiling(Randomizer.NextDouble() * 19.0));
                                    ypos = -590;
                                    goto case 8500004;
                                case 8500004: // Pap bomb
                                              //Spawn between -500 and 500 from the monsters X position
                                    xpos = (int)(monster.Position.X + Math.Ceiling(Randomizer.NextDouble() * 1000.0) - 500);
                                    if (ypos != -590)
                                    {
                                        ypos = monster.Position.Y;
                                    }
                                    break;
                                case 8510100: //Pianus bomb
                                    if (Math.Abs(Math.Ceiling(Randomizer.NextDouble() * 5) - 1) < EPSINON)
                                    {
                                        ypos = 78;
                                        xpos = (int)(0 + Math.Ceiling(Randomizer.NextDouble() * 5)) + ((Math.Abs(Math.Ceiling(Randomizer.NextDouble() * 2) - 1) < EPSINON) ? 180 : 0);
                                    }
                                    else {
                                        xpos = (int)(monster.Position.X + Math.Ceiling(Randomizer.NextDouble() * 1000.0) - 500);
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
                                        xpos = (int)(-890 + Math.Ceiling(Randomizer.NextDouble() * 150));
                                    }
                                    else if (xpos > 230)
                                    {
                                        xpos = (int)(230 - Math.Ceiling(Randomizer.NextDouble() * 150));
                                    }
                                    break;
                                case 230040420: // Pianus map
                                    if (xpos < -239)
                                    {
                                        xpos = (int)(-239 + Math.Ceiling(Randomizer.NextDouble() * 150));
                                    }
                                    else if (xpos > 371)
                                    {
                                        xpos = (int)(371 - Math.Ceiling(Randomizer.NextDouble() * 150));
                                    }
                                    break;
                            }
                            toSpawn.Position = (new Point(xpos, ypos));
                            monster.Map.spawnMonsterWithEffect(toSpawn, spawnEffect, toSpawn.Position);
                        }
                    }
                    break;
            }

            if (monStat != null || heal)
            {
                if (lt != Point.Empty && rb != Point.Empty && skill)
                {
                    List<IMapleMapObject> objects = getObjectsInRange(monster, MapleMapObjectType.Monster);
                    if (heal)
                    {
                        foreach (IMapleMapObject mons in objects)
                        {
                            ((MapleMonster)mons).heal(x, y);
                        }
                    }
                    else {
                        foreach (IMapleMapObject mons in objects)
                        {
                            if (!monster.MonsterBuffs.Contains(monStat.Value))
                            {
                                ((MapleMonster)mons).applyMonsterBuff(monStat.Value, x, skillId, duration, this);
                            }
                        }
                    }
                }
                else {
                    if (heal)
                    {
                        monster.heal(x, y);
                    }
                    else {
                        if (!monster.MonsterBuffs.Contains(monStat.Value))
                        {
                            monster.applyMonsterBuff(monStat.Value, x, skillId, duration, this);
                        }
                    }
                }
            }

            if (disease != null || dispel || banish)
            {
                if (skill && lt != Point.Empty && rb != Point.Empty)
                {
                    List<MapleCharacter> characters = getPlayersInRange(monster, player);
                    foreach (MapleCharacter character in characters)
                    {
                        if (dispel)
                        {
                            character.dispel();
                        }
                        else if (banish)
                        {
                            if (player.EventInstanceManager == null)
                            {
                                MapleMap to = player.Map.ReturnMap;
                                IMaplePortal pto = to.getPortal((short)(0 + 10 * Randomizer.NextDouble()));
                                character.changeMap(to, pto);
                            }
                        }
                        else {
                            character.giveDebuff(disease.Value, this);
                        }
                    }
                }
                else {
                    if (dispel)
                    {
                        player.dispel();
                    }
                    else {
                        player.giveDebuff(disease.Value, this);
                    }
                }
            }
            monster.usedSkill(skillId, skillLevel, cooltime);
            monster.Stats.Mp -= mpCon;
        }

        public bool makeChanceResult()
        {
            return Math.Abs(prop - 1.0) < EPSINON || Randomizer.NextDouble() < prop;
        }

        private Rectangle calculateBoundingBox(Point posFrom, bool facingLeft)
        {
            Point mylt;
            Point myrb;
            if (facingLeft)
            {
                mylt = new Point(lt.X + posFrom.X, lt.Y + posFrom.Y);
                myrb = new Point(rb.X + posFrom.X, rb.Y + posFrom.Y);
            }
            else {
                myrb = new Point(lt.X * -1 + posFrom.X, rb.Y + posFrom.Y);
                mylt = new Point(rb.X * -1 + posFrom.X, lt.Y + posFrom.Y);
            }
            Rectangle bounds = new Rectangle(mylt.X, mylt.Y, myrb.X - mylt.X, myrb.Y - mylt.Y);
            return bounds;
        }

        private List<MapleCharacter> getPlayersInRange(MapleMonster monster, MapleCharacter player)
        {
            Rectangle bounds = calculateBoundingBox(monster.Position, monster.IsFacingLeft);
            List<MapleCharacter> players = new List<MapleCharacter>();
            players.Add(player);
            return monster.Map.getPlayersInRect(bounds, players);
        }

        private List<IMapleMapObject> getObjectsInRange(MapleMonster monster, MapleMapObjectType objectType)
        {
            Rectangle bounds = calculateBoundingBox(monster.Position, monster.IsFacingLeft);
            List<MapleMapObjectType> objectTypes = new List<MapleMapObjectType>();
            objectTypes.Add(objectType);
            return monster.Map.getMapObjectsInRect(bounds, objectTypes);
        }
    }
}
