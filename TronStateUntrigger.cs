using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/tronStateUntrigger")]
    public class TronStateUntrigger : Trigger {
        private bool onlyOnce;

        public TronStateUntrigger(EntityData data, Vector2 offset) : base(data, offset) {
            onlyOnce = data.Bool("onlyOnce", false);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if(player.StateMachine.State == TronState.TronStateID) {
                player.StateMachine.State = Player.StNormal;
                if (onlyOnce) {
                    RemoveSelf();
                }
            }
        }
    }
}
