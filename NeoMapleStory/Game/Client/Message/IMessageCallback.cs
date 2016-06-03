using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoMapleStory.Game.Client.Message
{
    interface IMessageCallback
    {
        void DropMessage(string message);
    }
}
