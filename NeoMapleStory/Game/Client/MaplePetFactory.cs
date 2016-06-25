using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoMapleStory.Game.Data;

namespace NeoMapleStory.Game.Client
{
    public class PetCommand
    {
        public int PetId { get; private set; }
        public int SkillId { get; private set; }
        public int Probability { get; private set; }
        public int Increase { get; private set; }

        public PetCommand(int petId, int skillId, int prob, int inc)
        {
            PetId = petId;
            SkillId = skillId;
            Probability = prob;
            Increase = inc;
        }
    }

    public class MaplePetFactory
    {
        private static readonly IMapleDataProvider DataRoot = MapleDataProviderFactory.GetDataProvider("Item.wz");
        private static readonly Dictionary<Tuple<int, int>, PetCommand> PetCommands = new Dictionary<Tuple<int, int>, PetCommand>();
        private static readonly Dictionary<int, byte> PetHunger = new Dictionary<int, byte>();

        public static PetCommand GetPetCommand(int petId, int skillId)
        {
            PetCommand ret;
            var key = Tuple.Create(petId, skillId);

            if (PetCommands.TryGetValue(key, out ret))
            {
                return ret;
            }
            lock (PetCommands)
            {
                // see if someone else that's also synchronized has loaded the skill by now
                PetCommands.TryGetValue(key, out ret);
                if (ret == null)
                {
                    IMapleData skillData = DataRoot.GetData("Pet/" + petId + ".img");
                    int prob = 0;
                    int inc = 0;
                    if (skillData != null)
                    {
                        prob = MapleDataTool.GetInt("interact/" + skillId + "/prob", skillData, 0);
                        inc = MapleDataTool.GetInt("interact/" + skillId + "/inc", skillData, 0);
                    }
                    ret = new PetCommand(petId, skillId, prob, inc);

                    if (PetCommands.ContainsKey(key))
                        PetCommands[key] = ret;
                    else
                        PetCommands.Add(key, ret);
                }
                return ret;
            }
        }

        public static byte GetHunger(int petId)
        {
            byte ret;

            if (PetHunger.TryGetValue(petId, out ret))
            {
                return ret;
            }
            lock (PetHunger)
            {
                if (PetHunger.TryGetValue(petId, out ret)) return ret;
                IMapleData hungerData = DataRoot.GetData("Pet/" + petId + ".img").GetChildByPath("info/hungry");
                ret = (byte)MapleDataTool.GetInt(hungerData, 1);

                if (PetHunger.ContainsKey(petId))
                    PetHunger[petId] = ret;
                else
                    PetHunger.Add(petId, ret);
                return ret;
            }
        }
    }
}
