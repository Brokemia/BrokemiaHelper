using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/neutralDeathPenalty")]
    public class NeutralDeathPenaltyTrigger : Trigger {
        private string flag;
        private bool enable;

        public NeutralDeathPenaltyTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            flag = data.Attr("flag", "");
            enable = data.Bool("enable", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (string.IsNullOrWhiteSpace(flag) || SceneAs<Level>().Session.GetFlag(flag)) {
                BrokemiaHelperModule.Session.NeutralDeathPenalty = enable;
            }
        }

        public static void Load() {
            On.Celeste.Player.WallJump += Player_WallJump;
        }

        public static void Unload() {
            On.Celeste.Player.WallJump -= Player_WallJump;
        }

        private static void Player_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
            orig(self, dir);
            if (BrokemiaHelperModule.Session.NeutralDeathPenalty && DynamicData.For(self).Get<int>("moveX") == 0) {
                self.Die(Vector2.Zero);
            }
        }
    }
}
