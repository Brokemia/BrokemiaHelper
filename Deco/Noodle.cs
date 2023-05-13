using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace BrokemiaHelper.Deco {
    [Pooled]
    [Tracked]
    public class Noodle : Entity {
        public struct NoodleParams {
            public NoodleParams() { }

            public int Depth = -13010;
            public int Length = 15;
            public float FastSpeed = 70;
            public float SlowSpeed = 30;
            public float Acceleration = 50;
            public float TailTaper = 0.5f;
            public float WanderStrength = 0.4f;
            public float PlayerInterest = 0.07f;
            public float FriendInterest = 0.02f;
            public float HomingDistSq = 36;
            public float Homing = 0.25f;
            public float JourneyMax = 15;
            public float JourneyMin = 5;
            public float FocusMax = 0.15f;
            public float FocusIncrease = 0.02f;
            public float SightDistanceSq = 640;
            public float Bloodthirst = 0f;
        }

        private const float BLOODTHIRST_SCALE = 0.1f;
        private const float BLOODTHIRST_ACC_SCALE = 0.015f;
        private const float KILL_DIST_SQ = 16;
        private const float END_DISTANCE_SQ = 16;
        
        private MTexture circle;
        private Random rand;

        // Noodle parameters
        private Vector2 end;
        private NoodleParams noodleParams;

        // Variables during execution
        private List<Vector2> pastPoints = new();
        private Vector2 direction;
        private float speed;
        private bool goingSlow;
        private float journey;
        private float focusStrength;
        private float killCircle;
        private bool killSignaled;

        public Noodle() {
            circle = GFX.Game["particles/circle"];
            Add(new PostUpdateHook(AfterUpdate));
        }

        public Noodle Init(Vector2 start, Vector2 end, int seed, NoodleParams noodleParams) {
            rand = new(seed);
            Position = start;
            pastPoints.Clear();
            this.end = end;
            direction = Vector2.Zero;
            speed = noodleParams.SlowSpeed;
            goingSlow = true;
            journey = noodleParams.JourneyMin + rand.NextFloat(noodleParams.JourneyMax - noodleParams.JourneyMin);
            focusStrength = 0;
            killCircle = 0;
            killSignaled = false;
            this.noodleParams = noodleParams;
            Depth = noodleParams.Depth;
            return this;
        }

        public override void Update() {
            base.Update();
            pastPoints.Add(Position);
            if(pastPoints.Count >= noodleParams.Length) {
                pastPoints.RemoveAt(0);
            }

            float targetSpeed = goingSlow ? noodleParams.SlowSpeed : noodleParams.FastSpeed;
            speed = Calc.Approach(speed, targetSpeed, noodleParams.Acceleration * Engine.DeltaTime);
            if (Math.Abs(speed - targetSpeed) < 0.01f) {
                goingSlow = !goingSlow;
            }
            Position += direction * Engine.DeltaTime * speed;

            // Wander away
            direction += Calc.AngleToVector(rand.NextAngle(), noodleParams.WanderStrength);
            direction.Normalize();

            if (journey > 0) {
                journey -= Engine.DeltaTime;

                bool killSignal = false;
                if (Scene.Tracker.GetEntity<Player>() is { } player) {
                    // Steer towards player
                    Vector2 playerDir = player.Center - Position;
                    if (playerDir.LengthSquared() < noodleParams.SightDistanceSq) {
                        if (!killSignaled && noodleParams.Bloodthirst > 0.5f) {
                            if (!killSignaled) {
                                killSignal = true;
                            }
                            if (Math.Abs(killCircle - noodleParams.Bloodthirst * BLOODTHIRST_SCALE) < 0.01f && playerDir.LengthSquared() < KILL_DIST_SQ) {
                                player.Die(Vector2.Zero);
                            }
                        }
                        direction += playerDir.SafeNormalize() * (noodleParams.PlayerInterest + killCircle);
                    }
                } else {
                    // Cool off if they can't see the player
                    killCircle = Math.Max(killCircle - noodleParams.Bloodthirst * BLOODTHIRST_ACC_SCALE * Engine.DeltaTime, 0);
                }

                Vector2 closestNoodleDir = Vector2.Zero;
                float closestNoodleDistSq = float.MaxValue;
                foreach (Noodle noodle in Scene.Tracker.GetEntities<Noodle>()) {
                    if(killSignal) {
                        noodle.SignalKill();
                    }
                    if (noodle == this) continue;
                    // Steer towards other noodles
                    Vector2 noodleDir = noodle.Position - Position;
                    if (noodleDir.LengthSquared() < closestNoodleDistSq) {
                        closestNoodleDir = noodleDir;
                        closestNoodleDistSq = noodleDir.LengthSquared();
                    }
                }
                if (closestNoodleDistSq < noodleParams.SightDistanceSq) {
                    direction += closestNoodleDir.SafeNormalize() * noodleParams.FriendInterest;
                }
            } else {
                focusStrength = Math.Min(focusStrength + noodleParams.FocusIncrease * Engine.DeltaTime, noodleParams.FocusMax);
                Vector2 targetDir = end - Position;
                // Steer back
                direction += targetDir.SafeNormalize() * focusStrength;
                // Steer back strongly when near target
                if (noodleParams.HomingDistSq >= targetDir.LengthSquared()) {
                    direction += targetDir.SafeNormalize() * (noodleParams.HomingDistSq - targetDir.LengthSquared()) / noodleParams.HomingDistSq * noodleParams.Homing;
                }

                if (targetDir.LengthSquared() <= END_DISTANCE_SQ) {
                    RemoveSelf();
                }
            }
            
            direction.Normalize();
        }

        public void AfterUpdate() {
            killSignaled = false;
        }
        
        public void SignalKill() {
            if (killSignaled) return;
            killCircle = Math.Min(killCircle + noodleParams.Bloodthirst * BLOODTHIRST_ACC_SCALE * Engine.DeltaTime, noodleParams.Bloodthirst * BLOODTHIRST_SCALE);
            killSignaled = true;
        }

        public override void Render() {
            base.Render();
            circle.DrawCentered(Position, Color.Wheat);
            for (int i = 0; i < pastPoints.Count; i++) {
                circle.DrawCentered(pastPoints[i], Color.Wheat, MathHelper.Lerp(noodleParams.TailTaper, 1, (i + 1) / (float)noodleParams.Length));
            }
        }

    }
}
