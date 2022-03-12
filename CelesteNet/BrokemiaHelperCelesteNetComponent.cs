using Celeste;
using Celeste.Mod.CelesteNet;
using Celeste.Mod.CelesteNet.Client;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokemiaHelper.CelesteNet {
    class BrokemiaHelperCelesteNetComponent : CelesteNetGameComponent {

        public BrokemiaHelperCelesteNetComponent(CelesteNetClientContext context, Game game) : base(context, game) {
            Visible = false;
        }

        public void Handle(CelesteNetConnection con, FlagData data) {
            Level level = Engine.Scene as Level;
            // Deliberately not using SetFlag because it'll try to send a data packet back and make a loop
            if (data.value) {
                level?.Session.Flags.Add(data.flag);
            } else {
                level?.Session.Flags.Remove(data.flag);
            }
        }
    }
}
