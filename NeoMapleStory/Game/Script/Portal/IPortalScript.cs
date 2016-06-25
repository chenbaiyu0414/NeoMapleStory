using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoMapleStory.Game.Script.Portal
{
    public interface IPortalScript
    {
        bool Enter(PortalPlayerInteraction p);
    }
}
