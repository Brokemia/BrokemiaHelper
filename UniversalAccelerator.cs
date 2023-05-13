using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/universalAccelerator")]
    public class UniversalAccelerator : Entity {

        public static void Load() {
            On.Celeste.Actor.Update += Actor_Update;
        }

        public static void Unload() {
            On.Celeste.Actor.Update -= Actor_Update;
        }

        private static void Actor_Update(On.Celeste.Actor.orig_Update orig, Actor self) {
            if(DynamicData.For(self).Get<bool?>("BrokemiaHelper_universallyAccelerated") ?? false) {
                if(self is Player player) {
                    player.Speed.Y -= Player.Gravity * Engine.DeltaTime;
                }/* else if(self.Get<Holdable>() is { } holdable) {
                    holdable.
                }*/
            }
            orig(self);
        }

        public UniversalAccelerator(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            Add(new PlayerCollider(Accelerate));
        }

        private void Accelerate(Player player) {
            DynamicData.For(player).Set("BrokemiaHelper_universallyAccelerated", true);
        }
    }
}
