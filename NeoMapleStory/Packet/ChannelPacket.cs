using System;
using NeoMapleStory.Core;
using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Client;

namespace NeoMapleStory.Packet
{
    public static class ChannelPacket
    {
        public static OutPacket GetCharInfo(MapleCharacter chr)
        {
            using (var packet = new OutPacket(SendOpcodes.WarpToMap))
            {
                packet.WriteInt(chr.Client.ChannelId);
                packet.WriteByte(0x00);
                packet.WriteByte(0x01);
                packet.WriteByte(0x01);
                packet.WriteShort(0);
                packet.WriteInt((int) (Randomizer.NextDouble()*10));
                packet.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
                packet.WriteLong(-1);
                packet.WriteByte(0x00);
                LoginPacket.AddCharStats(packet, chr);
                packet.WriteByte((byte) chr.Buddies.Capacity);
                PacketCreator.AddInventoryInfo(packet, chr);
                PacketCreator.AddSkillRecord(packet, chr);
                PacketCreator.AddQuestRecord(packet, chr);
                PacketCreator.AddRingInfo(packet, chr);
                PacketCreator.AddTeleportRockRecord(packet, chr);
                packet.WriteShort(0);
                packet.WriteLong(0);
                packet.WriteByte(0x00);
                packet.WriteInt(0);
                packet.WriteLong(DateUtiliy.GetFileTimestamp(DateTime.Now.GetTimeMilliseconds()));
                return packet;
            }
        }
    }
}