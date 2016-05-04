using System.Collections.Generic;
using NeoMapleStory.Server;
using System.Drawing;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.World;
using NeoMapleStory.Packet;

namespace NeoMapleStory.Game.Map
{
    public class MapleDoor : AbstractMapleMapObject
    {
        public MapleCharacter Owner { get; private set; }
        public MapleMap Town { get; private set; }
        public IMaplePortal TownPortal { get; private set; }
        public MapleMap TargetMap { get; private set; }
        public Point TargetMapPosition { get; private set; }

        public MapleDoor(MapleCharacter owner, Point targetPosition)
        {
            Owner = owner;
            TargetMap = owner.Map;
            TargetMapPosition = targetPosition;
            Position = TargetMapPosition;
            Town = TargetMap.ReturnMap;
            TownPortal = GetFreePortal();
        }

        public MapleDoor(MapleDoor origDoor)
        {
            Owner = origDoor.Owner;
            Town = origDoor.Town;
            TownPortal = origDoor.TownPortal;
            TargetMap = origDoor.TargetMap;
            TargetMapPosition = origDoor.TargetMapPosition;
            TownPortal = origDoor.TownPortal;
            Position = TownPortal.Position;
        }

        private IMaplePortal GetFreePortal()
        {
            List<IMaplePortal> freePortals = new List<IMaplePortal>();

            foreach (IMaplePortal port in Town.Portals.Values)
            {
                if (port.Type ==  PortalType.DoorPortal)
                {
                    freePortals.Add(port);
                }
            }

            freePortals.Sort((obj1, obj2) =>
            {
                if (obj1.PortalId < obj2.PortalId)
                    return -1;
                else if (obj1.PortalId == obj2.PortalId)
                    return 0;
                else
                    return 1;
            });

            foreach (IMapleMapObject obj in Town.Mapobjects.Values)
            {
                if (obj is MapleDoor)
                {
                    MapleDoor door = (MapleDoor)obj;
                    if (door.Owner.Party != null &&
                            Owner.Party.ContainsMember(new MaplePartyCharacter(door.Owner)))
                    {
                        freePortals.Remove(door.TownPortal);
                    }
                }
            }
            freePortals.GetEnumerator().MoveNext();
            return freePortals.GetEnumerator().Current;
        }

        public void Warp(MapleCharacter chr, bool toTown)
        {
            if (chr == Owner || Owner.Party != null && Owner.Party.ContainsMember(new MaplePartyCharacter(chr)))
            {
                if (!toTown)
                {
                    if (!chr.Map.CanExit && chr.GmLevel == 0)
                    {
                        chr.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, "您被禁止离开此地图"));
                        chr.Client.Send(PacketCreator.EnableActions());
                        return;
                    }
                    if (!TargetMap.CanEnter && chr.GmLevel == 0)
                    {
                        chr.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, $"您不能进入 {TargetMap.StreetName} : {TargetMap.MapName}"));
                        chr.Client.Send(PacketCreator.EnableActions());
                        return;
                    }
                    chr.changeMap(TargetMap, TargetMapPosition);
                }
                else {
                    if (!chr.Map.CanExit && chr.GmLevel == 0)
                    {
                        chr.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, "您被禁止离开此地图"));
                        chr.Client.Send(PacketCreator.EnableActions());
                        return;
                    }
                    if (!Town.CanEnter && chr.GmLevel == 0)
                    {
                        chr.Client.Send(PacketCreator.ServerNotice(PacketCreator.ServerMessageType.PinkText, $"您不能进入 {Town.StreetName} : {Town.MapName}"));
                        chr.Client.Send(PacketCreator.EnableActions());
                        return;
                    }
                    chr.changeMap(Town, TownPortal);
                }
            }
            else {
                chr.Client.Send(PacketCreator.EnableActions());
            }
        }

        public override MapleMapObjectType GetType() => MapleMapObjectType.Door;

        public override void SendDestroyData(MapleClient client)
        {
            if (TargetMap.MapId == client.Player.Map.MapId || Owner == client.Player || Owner.Party != null && Owner.Party.ContainsMember(new MaplePartyCharacter(client.Player)))
            {
                if (Owner.Party != null && (Owner == client.Player || Owner.Party.ContainsMember(new MaplePartyCharacter(client.Player))))
                {
                    client.Send(PacketCreator.PartyPortal(999999999, 999999999, new Point(-1, -1)));
                }
                client.Send(PacketCreator.RemoveDoor(Owner.Id, false));
                client.Send(PacketCreator.RemoveDoor(Owner.Id, true));
            }
        }

        public override void SendSpawnData(MapleClient client)
        {
            if (TargetMap.MapId == client.Player.Map.MapId || Owner == client.Player && Owner.Party == null)
            {
                client.Send(PacketCreator.SpawnDoor(Owner.Id, Town.MapId == client.Player.Map.MapId ? TownPortal.Position : TargetMapPosition, true));
                if (Owner.Party != null && (Owner == client.Player || Owner.Party.ContainsMember(new MaplePartyCharacter(client.Player))))
                {
                    client.Send(PacketCreator.PartyPortal(Town.MapId, TargetMap.MapId, TargetMapPosition));
                }
                client.Send(PacketCreator.SpawnPortal(Town.MapId, TargetMap.MapId, TargetMapPosition));
            }
        }
    }
}
