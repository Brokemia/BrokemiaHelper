using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokemiaHelper.PixelRendered {
    [CustomEntity("BrokemiaHelper/rwLizard")]
    public class RWLizard : Actor {
        public enum States {
            Idle = 0,
            Walk = 1,
            Climb = 2
        }

        private Dictionary<States, Color> stateColors = new() {
            { States.Idle, Color.Gray },
            { States.Walk, Color.Blue },
            { States.Climb, Color.DarkBlue }
        };

        private PixelComponent pixelComponent;
        private Level level;
        private Vector2 levelOffset;
        private Random rand;

        private Color tailColor = Color.Gray;

        private Vector2[] tailPts;

        private int tailSegments;
        private float tailSegmentLength;
        private float tailStartWidth;
        private float tailGravity;

        private float randomTargetChance;

        private float walkSpeed;
        private float walkAccel;

        private float climbSpeed;
        private float climbAccel;

        private float wallGrabDist = 3;

        private float gravity;
        private float groundFriction;
        private float airFriction;

        private Vector2? target;
        private float randomTargetTime;
        private float minRandomTargetTime;
        private float maxRandomTargetTime;

        private bool onGround;
        private bool wasOnGround;

        private Platform adjacentWall;
        private Platform grabbedPlatform;
        private int climbDir;

        public Vector2 Speed;
        private Vector2 prevLiftSpeed;
        private float noGravityTimer;
        private float hardVerticalHitSoundCooldown;

        public StateMachine stateMachine;

        public RWLizard(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
			Depth = Depths.Seeker;
            levelOffset = offset;
            Collider = new Hitbox(8f, 8f, -4f, -8f);
            tailSegments = data.Int("tailSegments", 10);
            tailSegmentLength = data.Float("tailSegmentLength", 5);
            tailStartWidth = data.Float("tailStartWidth", 7f);
            tailGravity = data.Float("tailGravity", 20f);

            randomTargetChance = data.Float("randomTargetChance", 0.003f);
            minRandomTargetTime = data.Float("minRandomTargetTime", 1f);
            maxRandomTargetTime = data.Float("maxRandomTargetTime", 8f);

            walkSpeed = data.Float("walkSpeed", 40f);
            walkAccel = data.Float("walkAccel", 70f);

            climbSpeed = data.Float("climbSpeed", 30f);
            climbAccel = data.Float("climbAccel", 100f);
            wallGrabDist = data.Float("walkGrabDist", 3);

            gravity = data.Float("gravity", 300f);
            groundFriction = data.Float("groundFriction", 50f);
            airFriction = data.Float("airFriction", 50f);

            string seed = data.Attr("seed", "");
            rand = data.Bool("noSetSeed", false) ? new() : new(string.IsNullOrWhiteSpace(seed) ? id.Key.GetHashCode() : seed.GetHashCode());

            tailPts = new Vector2[tailSegments];

            Add(pixelComponent = new PixelComponent());

            Add(stateMachine = new());
            stateMachine.SetCallbacks((int)States.Idle, _idleUpdate);
            stateMachine.SetCallbacks((int)States.Walk, _walkUpdate);
            stateMachine.SetCallbacks((int)States.Climb, _climbUpdate);

            //AddTag(Tags.Persistent);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            level = SceneAs<Level>();
            for(int i = 0; i < tailPts.Length; i++) {
                tailPts[i] = Position - levelOffset + new Vector2(tailSegmentLength, 0) * (i+1);
            }
        }

        private int _idleUpdate() => (int)IdleUpdate();
        private int _walkUpdate() => (int)WalkUpdate();
        private int _climbUpdate() => (int)ClimbUpdate();

        private States IdleUpdate() {
            if(target != null) {
                if(grabbedPlatform != null) {
                    return States.Climb;
                }
                return States.Walk;
            }
            return States.Idle;
        }

        private States WalkUpdate() {
            if (!target.HasValue) return States.Idle;
            int dir = Math.Sign((target - Position).Value.X);
            // If we've just walked off an edge
            if (!onGround && wasOnGround && CollideFirst<Platform>(Position + new Vector2(-dir * wallGrabDist, 1)) is { } p) {
                Speed.X = 0;
                grabbedPlatform = p;
                climbDir = 1;
                return States.Climb;
            }

            if (adjacentWall != null) {
                grabbedPlatform = adjacentWall;
                adjacentWall = null;
            } else {
                Speed.X = Calc.Approach(Speed.X, walkSpeed * dir, walkAccel * Engine.DeltaTime);
            }

            if (grabbedPlatform != null) {
                climbDir = -1;
                return States.Climb;
            }
            return States.Walk;
        }

        private States ClimbUpdate() {
            // Make sure still next to a wall
            if(!CollideCheck<Platform>(Position + new Vector2(wallGrabDist, 1)) && !CollideCheck<Platform>(Position + new Vector2(-wallGrabDist, 1))) {
                grabbedPlatform = null;
                Speed.Y /= 2;
                return States.Walk;
            }

            if (target == null) {
                climbDir = 0;
                Speed.Y = 0;
                return States.Climb;
            } else if(climbDir == 0) {
                int targetDir = Math.Sign((target - Position).Value.X);
                if(CollideCheck<Platform>(Position + new Vector2(wallGrabDist, 1))) {
                    climbDir = -1;
                } else if (CollideCheck<Platform>(Position + new Vector2(-wallGrabDist, 1))) {
                    climbDir = 1;
                }
                climbDir *= targetDir;
            }

            Speed.Y = Calc.Approach(Speed.Y, climbSpeed * climbDir, climbAccel * Engine.DeltaTime);
            // Hit the ceiling or floor
            if(CollideCheck<Platform>(Position + Vector2.UnitY * climbDir)) {
                grabbedPlatform = null;
                return States.Walk;
            }
            return States.Climb;
        }

        private void UpdateGoals() {
            if(target == null) {
                if (rand.NextFloat() < randomTargetChance) {
                    target = Position + new Vector2(600, 0);
                    randomTargetTime = minRandomTargetTime + rand.NextFloat(maxRandomTargetTime - minRandomTargetTime);
                }
            } else {
                randomTargetTime -= Engine.DeltaTime;
                if(randomTargetTime < 0) {
                    target = null;
                }
            }
        }

        public override void Update() {
            base.Update();

            UpdateGoals();

			hardVerticalHitSoundCooldown -= Engine.DeltaTime;
			if (onGround || grabbedPlatform != null) {
                // Slip off the edges of solids
                if (grabbedPlatform == null) {
                    if (Math.Abs(Speed.X) < 1) {
                        float slipTargetSpeed = (!OnGround(Position + Vector2.UnitX * 2f)) ? 20f : (OnGround(Position - Vector2.UnitX * 2f) ? 0f : (-20f));
                        Speed.X = Calc.Approach(Speed.X, slipTargetSpeed, 800f * Engine.DeltaTime);
                    } else { // Apply ground friction
                        Speed.X = Calc.Approach(Speed.X, 0, groundFriction * Engine.DeltaTime);
                    }
                }

                // Get launched by stuff once it stops moving
				if (LiftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero) {
					Speed = prevLiftSpeed;
					prevLiftSpeed = Vector2.Zero;
                    // Prevent downwards lifting and limit upwards lifting to 60%
					Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                    // When thrown purely horizontally, get sent up a bit to make an arc
					if (Speed.X != 0f && Speed.Y == 0f) {
						Speed.Y = -60f;
					}
                    // Reduce gravity for a brief bit after launching with any upwards velocity
					if (Speed.Y < 0f) {
						noGravityTimer = 0.15f;
					}
				} else {
                    // While being lifted, keep tracking the last lift speed
					prevLiftSpeed = LiftSpeed;
					if (LiftSpeed.Y < 0f && Speed.Y < 0f) {
						Speed.Y = 0f;
					}
				}
			} else { // Handle normal gravity stuff
                float gravity = this.gravity;
                // While going slowly in Y (aka near the top of an arc)
                // Half the effects of gravity
				if (Math.Abs(Speed.Y) <= 30f) {
					gravity *= 0.5f;
				}
                // Air friction to slow down X
				float airFric = airFriction;
                // Halved while going up
				if (Speed.Y < 0f) {
					airFric *= 0.5f;
				}
				Speed.X = Calc.Approach(Speed.X, 0f, airFric * Engine.DeltaTime);
				if (noGravityTimer > 0f) {
					noGravityTimer -= Engine.DeltaTime;
				} else {
					Speed.Y = Calc.Approach(Speed.Y, 200f, gravity * Engine.DeltaTime);
				}
			}

            wasOnGround = onGround;
			MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
			MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
            onGround = OnGround();

            TailUpdate();
        }

        private void TailUpdate() {
            float tailSegmentLengthSq = tailSegmentLength * tailSegmentLength;
            Vector2 last = Position - levelOffset;
            float scaledTailGravity = tailGravity * Engine.DeltaTime;
            for (int i = 0; i < tailPts.Length; i++) {
                // Fall due to gravity
                for (int j = 0; j < (int)scaledTailGravity; j++) {
                    if (CollidePoint<Solid>(levelOffset + tailPts[i] + Vector2.UnitY)) {
                        break;
                    }
                    tailPts[i].Y++;
                }
                if (!CollidePoint<Solid>(levelOffset + tailPts[i] + new Vector2(0, scaledTailGravity - (int)scaledTailGravity))) {
                    tailPts[i].Y += scaledTailGravity - (int)scaledTailGravity;
                }

                // Shrink back to maintain appropriate length
                Vector2 shortened = tailPts[i];
                int tooLong = 0;
                while ((shortened - last).LengthSquared() > tailSegmentLengthSq) {
                    Vector2 next = Calc.Approach(shortened, last, 0.5f);
                    if (CollidePoint<Solid>(levelOffset + next)) {
                        if (Math.Abs(next.X - shortened.X) > .0001f && !CollidePoint<Solid>(levelOffset + new Vector2(next.X, shortened.Y))) {
                            next.Y = shortened.Y;
                        } else if (Math.Abs(next.Y - shortened.Y) > .0001f && !CollidePoint<Solid>(levelOffset + new Vector2(shortened.X, next.Y))) {
                            next.X = shortened.X;
                        }
                    }
                    shortened = next;
                    // TODO try to stop this being required
                    tooLong++;
                    if (tooLong > 200) {
                        break;
                    }
                }
                tailPts[i] = shortened;

                last = tailPts[i];
            }
        }

		private void OnCollideH(CollisionData data) {
			//if (data.Hit is DashSwitch) {
			//	(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
			//}
			//Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
			if (Math.Abs(Speed.X) > 100f) {
				ImpactParticles(data.Direction);
			}
            if(Math.Abs(Speed.X) > 70f) {
                Speed.X *= -0.4f;
            } else {
                adjacentWall = data.Hit;
            }
		}

		private void OnCollideV(CollisionData data) {
			if (data.Hit is DashSwitch) {
				(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
			}
			if (Speed.Y > 0f) {
				if (hardVerticalHitSoundCooldown <= 0f) {
					Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
					hardVerticalHitSoundCooldown = 0.5f;
				} else {
					Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
				}
			}
			if (Speed.Y > 160f) {
				ImpactParticles(data.Direction);
			}
            //if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch)) {
            //	Speed.Y *= -0.6f;
            //} else {
            //	Speed.Y = 0f;
            //}
            Speed.Y = 0f;
        }

        private void ImpactParticles(Vector2 dir) {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f) {
                direction = (float)Math.PI;
                position = new Vector2(Right, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if (dir.X < 0f) {
                direction = 0f;
                position = new Vector2(Left, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if (dir.Y > 0f) {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(X, Bottom);
                positionRange = Vector2.UnitX * 6f;
            } else {
                direction = (float)Math.PI / 2f;
                position = new Vector2(X, Top);
                positionRange = Vector2.UnitX * 6f;
            }
            level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        // Allows the lizard to get launched automatically
        public override bool IsRiding(Solid solid) {
            if (Speed.Y == 0f) {
                return base.IsRiding(solid);
            }
            return false;
        }

        private bool CollidePoint<T>(Vector2 pt) where T : Entity {
            return Scene.Tracker.Entities[typeof(T)].Any(e => e.CollidePoint(pt));
        }

        public override void DebugRender(Camera camera) {
            base.DebugRender(camera);
            Draw.HollowRect(Center - Vector2.One * 2, 4, 4, GetStateColor());
            if(target != null) {
                Draw.Point(Center - Vector2.One, Color.Green);
            }
        }

        private Color GetStateColor() {
            if (stateColors.TryGetValue((States)stateMachine.State, out Color res)) {
                return res;
            }
            return Color.Transparent;
        }

        public override void Render() {
            base.Render();
            pixelComponent.ClearChunks();
            DrawLizard();
            pixelComponent.CommitChunks();
        }

        private void DrawLizard() {
            Vector2 last = Position - levelOffset;
            for(int i = 0; i < tailPts.Length; i++) {
                Vector2 dir = tailPts[i] - last;
                dir.Normalize();
                for (float d = 0; d < tailSegmentLength; d++) {
                    float tailScale = 1 - (i * tailSegmentLength + d) / (tailSegmentLength * tailSegments);
                    pixelComponent.DrawCircle((int)(last.X + dir.X * d), (int)(last.Y + dir.Y * d), tailStartWidth * tailScale, tailColor);
                }
                
                last = tailPts[i];
            }
        }
    }
}
