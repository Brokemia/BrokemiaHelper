using Monocle;
using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod.CelesteNet.Client;
using BrokemiaHelper.CelesteNet;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/CelesteNetFlagSynchronizer")]
    [Tracked]
    class CelesteNetFlagSynchronizer : Entity {
        public string flag;

        public CelesteNetFlagSynchronizer(EntityData data, Vector2 offset) {
            flag = data.Attr("flag");
        }

        public static void Load() {
            On.Celeste.Session.SetFlag += Session_SetFlag;
        }

        public static void Unload() {
            On.Celeste.Session.SetFlag -= Session_SetFlag;
        }

        private static void Session_SetFlag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool setTo) {
            bool oldValue = self.GetFlag(flag);
            orig(self, flag, setTo);
            if(oldValue != self.GetFlag(flag) && BrokemiaHelperModule.CelesteNetConnected()) {
                foreach(CelesteNetFlagSynchronizer sync in Engine.Scene.Tracker.GetEntities<CelesteNetFlagSynchronizer>()) {
                    if(sync.flag.Equals(flag)) {
                        CelesteNetSendFlagData(flag, setTo);
                    }
                }
            }
        }

        private static void CelesteNetSendFlagData(string flag, bool value) {
            CelesteNetClientModule.Instance.Client?.Send(new FlagData {
                flag = flag,
                value = value,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
