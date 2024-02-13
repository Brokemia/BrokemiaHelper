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
            var tron = player.Get<TronState>();
            if(tron != null && player.StateMachine.State == tron.TronStateID) {
                player.StateMachine.State = Player.StNormal;
                if (onlyOnce) {
                    RemoveSelf();
                }
            }
        }
    }
}
