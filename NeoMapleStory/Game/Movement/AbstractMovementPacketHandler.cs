using NeoMapleStory.Core.IO;
using NeoMapleStory.Game.Map;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NeoMapleStory.Game.Movement
{
    public static class AbstractMovementPacketHandler
    {
       public static List<ILifeMovementFragment> ParseMovement(InPacket lea)
        {
            List<ILifeMovementFragment> res = new List<ILifeMovementFragment>();
            byte numCommands = lea.ReadByte();
            for (int i = 0; i < numCommands; i++)
            {
                byte command = lea.ReadByte();
                switch (command)
                {
                    case 0: // normal move
                    case 5:
                    case 17: // Float
                        {
                            short xpos = lea.ReadShort();
                            short ypos = lea.ReadShort();
                            short xwobble = lea.ReadShort();
                            short ywobble = lea.ReadShort();
                            short unk = lea.ReadShort();
                            byte newstate = lea.ReadByte();
                            short duration = lea.ReadShort();
                            AbsoluteLifeMovement alm = new AbsoluteLifeMovement(command, new Point(xpos, ypos), duration, newstate);
                            alm.Unk = unk;
                            alm.PixelsPerSecond = new Point(xwobble, ywobble);
                            res.Add(alm);
                            break;
                        }
                    case 1:
                    case 2:
                    case 6: // fj
                    case 12:
                    case 13: // Shot-jump-back thing
                    case 16: // Float
                        {
                            short xmod = lea.ReadShort();
                            short ymod = lea.ReadShort();
                            byte newstate = lea.ReadByte();
                            short duration = lea.ReadShort();
                            RelativeLifeMovement rlm = new RelativeLifeMovement(command, new Point(xmod, ymod), duration, newstate);
                            res.Add(rlm);
                            // log.trace("Relative move {},{} state {}, duration {}", new Object[] { xmod, ymod, newstate, duration });
                            break;
                        }
                    case 3:
                    case 4: // tele... -.-
                    case 7: // assaulter
                    case 8: // assassinate
                    case 9: // rush
                    case 14:
                        {
                            short xpos = lea.ReadShort();
                            short ypos = lea.ReadShort();
                            short xwobble = lea.ReadShort();
                            short ywobble = lea.ReadShort();
                            byte newstate = lea.ReadByte();
                            TeleportMovement tm = new TeleportMovement(command, new Point(xpos, ypos), newstate);
                            tm.PixelsPerSecond = new Point(xwobble, ywobble);
                            res.Add(tm);
                            break;
                        }
                    case 10: // change equip ???
                        {
                            res.Add(new ChangeEquipSpecialAwesome(lea.ReadByte()));
                            break;
                        }
                    case 11: // chair
                        {
                            short xpos = lea.ReadShort();
                            short ypos = lea.ReadShort();
                            short unk = lea.ReadShort();
                            byte newstate = lea.ReadByte();
                            short duration = lea.ReadShort();
                            ChairMovement cm = new ChairMovement(command, new Point(xpos, ypos), duration, newstate);
                            cm.Unk = unk;
                            res.Add(cm);
                            break;
                        }
                    case 15:
                        {
                            short xpos = lea.ReadShort();
                            short ypos = lea.ReadShort();
                            short xwobble = lea.ReadShort();
                            short ywobble = lea.ReadShort();
                            short unk = lea.ReadShort();
                            short fh = lea.ReadShort();
                            byte newstate = lea.ReadByte();
                            short duration = lea.ReadShort();
                            JumpDownMovement jdm = new JumpDownMovement(command, new Point(xpos, ypos), duration, newstate);
                            jdm.Unk = unk;
                            jdm.PixelsPerSecond = new Point(xwobble, ywobble);
                            jdm.Fh = fh;
                            res.Add(jdm);
                            break;
                        }
                    case 20:
                    case 21:
                    case 22:
                        {
                            short unk = lea.ReadShort();
                            byte newstate = lea.ReadByte();
                            ArasMovement acm = new ArasMovement(command, new Point(0, 0), unk, newstate);
                            res.Add(acm);
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Unhandeled movement command {0} received", command);
                            //Console.WriteLine("Movement packet: {0}" lea.ToArray());
                            return null;
                        }
                }
            }
            if (numCommands != res.Count)
            {
                Console.WriteLine("numCommands ({0}) does not match the number of deserialized movement commands ({1})", numCommands, res.Count);
            }
            return res;
        }

       public static void UpdatePosition(List<ILifeMovementFragment> movement, IAnimatedMapleMapObject target, int yoffset)
        {
            foreach (ILifeMovementFragment move in movement)
            {
                if (move is ILifeMovement)
                {
                    if (move is AbsoluteLifeMovement)
                    {
                        Point position = ((ILifeMovement)move).Position;
                        position.Y += yoffset;
                        target.Position = position;
                    }
                    target.Stance = ((ILifeMovement)move).Newstate;
                }
            }
        }
    }
}
