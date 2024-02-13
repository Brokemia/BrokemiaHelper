using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/tronStateTrigger")]
    public class TronStateTrigger : Trigger {
        private bool onlyOnce;
        private Color? color;
        private float? slowSpeed;
        private float? targetSpeed;
        private float? maxSpeed;

        public TronStateTrigger(EntityData data, Vector2 offset) : base(data,offset) {
            onlyOnce = data.Bool("onlyOnce", false);
            color = data.HexColor("color");
            slowSpeed = data.Float("slowSpeed");
            targetSpeed = data.Float("targetSpeed");
            maxSpeed = data.Float("maxSpeed");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            var tron = player.Get<TronState>();
            if(tron != null && player.StateMachine.State != tron.TronStateID) {
                var pData = DynamicData.For(player);
                pData.Set("BrokemiaHelperTronHairColor", color);
                pData.Set("BrokemiaHelperTronSlowSpeed", slowSpeed);
                pData.Set("BrokemiaHelperTronTargetSpeed", targetSpeed);
                pData.Set("BrokemiaHelperTronMaxSpeed", maxSpeed);
                tron.StartTron();
                if(onlyOnce) {
                    RemoveSelf();
                }
            }
        }

    }
}
