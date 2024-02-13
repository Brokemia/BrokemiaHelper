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
            public bool ColorGradient = true;
            public Color HeadColor = Color.Purple;
            public Color TailColor = Color.MediumPurple;
        }

        private const float MAX_KILL_CIRCLE_TIME = 0.1f;
        private const float BLOODTHIRST_ACC_SCALE = 0.05f;
        private const float KILL_DIST_SQ = 36;
        private const float END_DISTANCE_SQ = 9;
        
        private MTexture circle;
        private Random rand;

        // Noodle parameters
        private Action<Noodle> onReachTarget;
        private Vector2 end;
        public NoodleParams Params { get; private set; }

        // Variables during execution
        private List<Vector2> pastPoints = new();
        private Vector2 direction;
        private float speed;
        private bool goingSlow;
        private float journey;
        private float focusStrength;
        private float killCircle;
        private bool killSignaled;
        private float actualKillTimer;
        private int disappearingOffset;

        public Noodle() {
            circle = GFX.Game["particles/circle"];
            Add(new PostUpdateHook(AfterUpdate));
        }

        public Noodle Init(Action<Noodle> onReachTarget, Vector2 start, Vector2 end, int seed, NoodleParams noodleParams) {
            this.onReachTarget = onReachTarget;
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
            actualKillTimer = 0;
            disappearingOffset = -1;
            Params = noodleParams;
            Depth = noodleParams.Depth;
            return this;
        }

        public override void Update() {
            base.Update();
            pastPoints.Add(Position);
            if(pastPoints.Count >= Params.Length) {
                pastPoints.RemoveAt(0);
            }

            if (disappearingOffset >= 0) {
                disappearingOffset++;
                if (disappearingOffset >= pastPoints.Count) {
                    onReachTarget?.Invoke(this);
                    RemoveSelf();
                }
                return;
            }

            float targetSpeed = goingSlow ? Params.SlowSpeed : Params.FastSpeed;
            speed = Calc.Approach(speed, targetSpeed, Params.Acceleration * Engine.DeltaTime);
            if (Math.Abs(speed - targetSpeed) < 0.01f) {
                goingSlow = !goingSlow;
            }
            Position += direction * Engine.DeltaTime * speed;

            // Wander away
            direction += Calc.AngleToVector(rand.NextAngle(), Params.WanderStrength);
            direction.Normalize();

            if (journey > 0) {
                journey -= Engine.DeltaTime;

                bool killSignal = false;
                if (Scene.Tracker.GetEntity<Player>() is { } player) {
                    // Steer towards player
                    Vector2 playerDir = player.Center - Position;
                    if (playerDir.LengthSquared() < Params.SightDistanceSq) {
                        if (!killSignaled && Params.Bloodthirst > 0.5f) {
                            if (!killSignaled) {
                                killSignal = true;
                            }
                            if (Math.Abs(killCircle - MAX_KILL_CIRCLE_TIME / Params.Bloodthirst) < 0.01f && playerDir.LengthSquared() < KILL_DIST_SQ) {
                                actualKillTimer += Engine.DeltaTime;
                                if (actualKillTimer > (1 - Params.Bloodthirst) / 2 + 0.2f) {
                                    player.Die(Vector2.Zero);
                                }
                                Console.WriteLine(actualKillTimer);
                            } else {
                                actualKillTimer = Calc.Approach(actualKillTimer, 0, Engine.DeltaTime / 4);
                            }
                        }
                        direction += playerDir.SafeNormalize() * (Params.PlayerInterest + killCircle);
                    }
                } else {
                    // Cool off if they can't see the player
                    killCircle = Math.Max(killCircle - Params.Bloodthirst * BLOODTHIRST_ACC_SCALE * Engine.DeltaTime, 0);
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
                if (closestNoodleDistSq < Params.SightDistanceSq) {
                    direction += closestNoodleDir.SafeNormalize() * Params.FriendInterest;
                }
            } else {
                focusStrength = Math.Min(focusStrength + Params.FocusIncrease * Engine.DeltaTime, Params.FocusMax);
                Vector2 targetDir = end - Position;
                // Steer back
                direction += targetDir.SafeNormalize() * focusStrength;
                // Steer back strongly when near target
                if (Params.HomingDistSq >= targetDir.LengthSquared()) {
                    direction += targetDir.SafeNormalize() * (Params.HomingDistSq - targetDir.LengthSquared()) / Params.HomingDistSq * Params.Homing;
                }

                if (targetDir.LengthSquared() <= END_DISTANCE_SQ) {
                    disappearingOffset = 0;
                }
            }
            
            direction.Normalize();
        }

        public void AfterUpdate() {
            killSignaled = false;
        }
        
        public void SignalKill() {
            if (killSignaled) return;
            killCircle = Math.Min(killCircle + Params.Bloodthirst * BLOODTHIRST_ACC_SCALE * Engine.DeltaTime, MAX_KILL_CIRCLE_TIME / Params.Bloodthirst);
            killSignaled = true;
        }

        public override void Render() {
            base.Render();
            
            int offset = Math.Max(0, disappearingOffset);
            for (int i = pastPoints.Count - 1; i >= offset; i--) {
                circle.DrawCentered(pastPoints[i - offset], Params.ColorGradient ? Color.Lerp(Params.TailColor, Params.HeadColor, (i + 1) / (float) (Params.Length - 1)) : Params.TailColor, MathHelper.Lerp(Params.TailTaper, 1, (i + 1) / (float)Params.Length));
            }
            circle.DrawCentered(Position, Params.HeadColor);
        }

    }
}
