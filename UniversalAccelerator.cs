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
            var selfData = DynamicData.For(self);
            if (selfData.Get<float?>("BrokemiaHelper_universallyAccelerated") is float acc && acc != 0
                && selfData.Get<float?>("BrokemiaHelper_universallyAcceleratedTerminalVelocity") is float maxVel) {
                Vector2 speed = default;
                Action<Vector2> speedSetter = null;
                if (self is Player player) {
                    speed = player.Speed;
                    speedSetter = (speed) => player.Speed = speed;
                } else if (self.Get<Holdable>() is { } holdable) {
                    speed = holdable.GetSpeed();
                    speedSetter = holdable.SpeedSetter;
                }

                if (maxVel < 0 || Math.Sign(speed.Y) != Math.Sign(acc) || Math.Abs(speed.Y) <= maxVel) {
                    var newSpeed = speed.Y + acc * Engine.DeltaTime;
                    speedSetter?.Invoke(new(speed.X, acc > 0 ? Math.Max(newSpeed, maxVel) : Math.Min(newSpeed, maxVel)));
                }
            }
            orig(self);
        }

        private float acceleration;
        private float terminalVelocity;

        public UniversalAccelerator(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            acceleration = data.Float("acceleration", 900f);
            terminalVelocity = data.Float("terminalVelocity", 160f);
            Add(new PlayerCollider(Accelerate));
            Add(new HoldableCollider((Holdable h) => Accelerate(h.Entity)));
        }

        private void Accelerate(Entity actor) {
            var actorData = DynamicData.For(actor);
            actorData.Set("BrokemiaHelper_universallyAccelerated", acceleration);
            actorData.Set("BrokemiaHelper_universallyAcceleratedTerminalVelocity", terminalVelocity);
        }
    }
}
