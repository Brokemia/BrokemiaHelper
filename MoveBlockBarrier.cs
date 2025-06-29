using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/moveBlockBarrier")]
    [Tracked(false)]
    public class MoveBlockBarrier : SeekerBarrier {

        private static FieldInfo particlesInfo = typeof(SeekerBarrier).GetField("particles", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);

        public bool blockDebris = false;

        // TODO mod interop to add to this
        public static List<string> ExtraMoveBlockTypes = new(new string[]{
            // Communal Helper
            "Celeste.Mod.CommunalHelper.Entities.MoveSwapBlock, CommunalHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Celeste.Mod.CommunalHelper.Entities.CassetteMoveBlock, CommunalHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Celeste.Mod.CommunalHelper.Entities.DreamMoveBlock, CommunalHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Celeste.Mod.CommunalHelper.Entities.EquationMoveBlock, CommunalHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Celeste.Mod.CommunalHelper.Entities.ConnectedMoveBlock, CommunalHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            // Crystalline Helper
            "vitmod.VitMoveBlock, vitmod, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            // Dzhake Helper
            "Celeste.Mod.DzhakeHelper.Entities.CustomMoveBlock, DzhakeHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
        });

        static List<Hook> ExtraMoveBlockHooks = new();

        public MoveBlockBarrier(EntityData data, Vector2 offset) : base(data, offset) {
            blockDebris = data.Bool("blockDebris");
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            scene.Tracker.GetEntity<SeekerBarrierRenderer>().Untrack(this);
            scene.Tracker.GetEntity<MoveBlockBarrierRenderer>().Track(this);
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            scene.Tracker.GetEntity<MoveBlockBarrierRenderer>().Untrack(this);
        }

        public override void Render() {
            List<Vector2> particles = (List<Vector2>)particlesInfo.GetValue(this);
            Color color = Color.Pink * 0.5f;
            foreach (Vector2 particle in particles) {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
            }
            if (Flashing) {
                Draw.Rect(base.Collider, Color.Pink * Flash * 0.5f);
            }
        }

        public static void Load() {
            On.Celeste.MoveBlock.MoveCheck += MoveBlock_MoveCheck;
            On.Celeste.Platform.MoveHCollideSolids += Platform_MoveHCollideSolids;
            On.Celeste.Platform.MoveVCollideSolids += Platform_MoveVCollideSolids;
            On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
            On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
        }

        public static void Initialize() {
            // Automatically hook the MoveCheck method of other MoveBlock variants
            foreach (string moveBlockType in ExtraMoveBlockTypes) {
                MethodInfo moveCheckInfo;
                var blockClass = Type.GetType(moveBlockType);
                if ((moveCheckInfo = blockClass?.GetMethod("MoveCheck", BindingFlags.NonPublic | BindingFlags.Instance)) != null
                    // DzhakeHelper seems to have copied from the publicizer
                    || (moveCheckInfo = blockClass?.GetMethod("MoveCheck", BindingFlags.Public | BindingFlags.Instance)) != null) {
                    ExtraMoveBlockHooks.Add(new Hook(moveCheckInfo, ((Func<Func<Entity, Vector2, bool>, Entity, Vector2, bool>)ExtraMoveBlock_MoveCheck).Method));
                }
            }
        }

        public static void Unload() {
            On.Celeste.MoveBlock.MoveCheck -= MoveBlock_MoveCheck;
            On.Celeste.Platform.MoveHCollideSolids -= Platform_MoveHCollideSolids;
            On.Celeste.Platform.MoveVCollideSolids -= Platform_MoveVCollideSolids;
            On.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
            On.Celeste.Actor.MoveVExact -= Actor_MoveVExact;

            // Dispose the automatically made hooks
            foreach (Hook hook in ExtraMoveBlockHooks) {
                hook.Dispose();
            }
        }

        static bool ExtraMoveBlock_MoveCheck(Func<Entity, Vector2, bool> orig, Entity self, Vector2 speed) {
            if (self.Scene == null || self.Scene.Tracker == null)
                return orig(self, speed);
            foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                barrier.Collidable = true;
            }

            bool result = orig(self, speed);

            foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                barrier.Collidable = false;
            }

            return result;
        }

        static bool Platform_MoveHCollideSolids(On.Celeste.Platform.orig_MoveHCollideSolids orig, Platform self, float moveH, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide) {
            if (self.Scene == null || self.Scene.Tracker == null)
                return orig(self, moveH, thruDashBlocks, onCollide);
            bool barrierWasCollidable = self.Scene.Tracker.GetEntity<MoveBlockBarrier>()?.Collidable ?? false;
            if (self is MoveBlock || ExtraMoveBlockTypes.Contains(self.GetType().AssemblyQualifiedName)) {
                foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                    barrier.Collidable = true;
                }

                bool result = orig(self, moveH, thruDashBlocks, onCollide);

                foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                    barrier.Collidable = barrierWasCollidable;
                }

                return result;
            }
            return orig(self, moveH, thruDashBlocks, onCollide);
        }

        static bool Platform_MoveVCollideSolids(On.Celeste.Platform.orig_MoveVCollideSolids orig, Platform self, float moveV, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide) {
            if (self.Scene == null || self.Scene.Tracker == null)
                return orig(self, moveV, thruDashBlocks, onCollide);
            bool barrierWasCollidable = self.Scene.Tracker.GetEntity<MoveBlockBarrier>()?.Collidable ?? false;
            if (self is MoveBlock || ExtraMoveBlockTypes.Contains(self.GetType().AssemblyQualifiedName)) {
                foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                    barrier.Collidable = true;
                }

                bool result = orig(self, moveV, thruDashBlocks, onCollide);

                foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                    barrier.Collidable = barrierWasCollidable;
                }

                return result;
            }
            return orig(self, moveV, thruDashBlocks, onCollide);
        }


        static bool Actor_MoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher) {
            if (self.Scene == null || self.Scene.Tracker == null)
                return orig(self, moveH, onCollide, pusher);
            bool isDebris = self.GetType().FullName.Equals("Celeste.MoveBlock+Debris");
            bool barrierWasCollidable = self.Scene.Tracker.GetEntity<MoveBlockBarrier>()?.Collidable ?? false;
            foreach (MoveBlockBarrier barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                barrier.Collidable = isDebris && barrier.blockDebris;
            }

            bool result = orig(self, moveH, onCollide, pusher);

            foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                barrier.Collidable = barrierWasCollidable;
            }

            return result;
        }

        static bool Actor_MoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher) {
            if (self.Scene == null || self.Scene.Tracker == null)
                return orig(self, moveV, onCollide, pusher);
            bool isDebris = self.GetType().FullName.Equals("Celeste.MoveBlock+Debris");
            bool barrierWasCollidable = self.Scene.Tracker.GetEntity<MoveBlockBarrier>()?.Collidable ?? false;
            foreach (MoveBlockBarrier barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                barrier.Collidable = isDebris && barrier.blockDebris;
            }

            bool result = orig(self, moveV, onCollide, pusher);

            foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                barrier.Collidable = barrierWasCollidable;
            }

            return result;
        }

        // Make the barrier temporarily collidable while checking MoveBlock collisions
        static bool MoveBlock_MoveCheck(On.Celeste.MoveBlock.orig_MoveCheck orig, MoveBlock self, Vector2 speed) {
            if (self.Scene == null || self.Scene.Tracker == null)
                return orig(self, speed);
            foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                barrier.Collidable = true;
            }

            bool result = orig(self, speed);

            foreach (Entity barrier in self.Scene.Tracker.GetEntities<MoveBlockBarrier>()) {
                barrier.Collidable = false;
            }

            return result;
        }

    }
}
