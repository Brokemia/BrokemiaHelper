using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace BrokemiaHelper {
    //[CustomEntity("brokemiahelper/cursedTeleportTrigger")]
    public class CursedTeleportTrigger : Trigger {
        string endRoom;

        public CursedTeleportTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            endRoom = data.Attr("endRoom");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            Level level = player.SceneAs<Level>();
            LevelData end = level.Session.MapData.Get(endRoom);
            Vector2 shiftAmount = level.Session.LevelData.Position - end.Position;
            foreach(LevelData ld in level.Session.MapData.Levels) {
                ld.Bounds.Offset(new Point((int)shiftAmount.X, (int)shiftAmount.Y));
            }
            RemoveSelf();
        }
    }
}
