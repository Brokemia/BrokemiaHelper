using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BrokemiaHelper {
    // Largely copied from vanilla feathers
    public class TronState : Component {
        private const float defaultSlowSpeed = Player.StarFlySlowSpeed;
        private const float defaultTargetSpeed = Player.StarFlyTargetSpeed;
        private const float defaultMaxSpeed = Player.StarFlyMaxSpeed;
        public static readonly float pointSpacingSq = 90 * 90;

        public event Action<Vector2> OnAddTrailPt;

        public int TronStateID { get; private set; }
        public List<Vector2> trail = new();
        public Color? HairColor { get; set; }
        public float SlowSpeed { get; set; } = defaultSlowSpeed;
        public float TargetSpeed { get; set; } = defaultTargetSpeed;
        public float MaxSpeed { get; set; } = defaultMaxSpeed;
        public bool LeaveTrail { get; set; } = true;


        private Color? lastColor;
        private float trailFade = 1;

        public TronState() : base(true, true) {
        }

        public override void Added(Entity entity) {
            base.Added(entity);
            if (entity is Player player) {
                TronStateID = player.StateMachine.AddState(TronUpdate, TronCoroutine, TronBegin, TronEnd);
            }
        }

        public override void Update() {
            base.Update();
            if (Entity is PlayerDeadBody || (Entity is Player player && player.StateMachine.State != TronStateID)) {
                trailFade = Calc.Approach(trailFade, 0, 2 * Engine.DeltaTime);
            }
        }

        public override void Render() {
            if (trail != null) {
                if (Entity is Player player) {
                    lastColor = HairColor ?? player.starFlyColor;
                }
                if (lastColor == null) {
                    return;
                }
                lastColor *= trailFade;
                for (int j = 0; j < trail.Count - 1; j++) {
                    Draw.Line(trail[j], trail[j + 1], lastColor.Value, 3);
                }
            }
        }

        private void AddTrail(Vector2 pt) {
            if (LeaveTrail) {
                trail.Add(pt);
                OnAddTrailPt?.Invoke(pt);
            }
        }

        public bool StartTron() {
            if (Entity is not Player self) return false;
            self.RefillStamina();
            if (self.StateMachine.State == Player.StReflectionFall) {
                return false;
            }

            if (self.StateMachine.State == TronStateID) {
                self.starFlyTimer = float.MaxValue;
                self.Sprite.Color = HairColor ?? self.starFlyColor;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            } else {
                self.StateMachine.State = TronStateID;
            }
            return true;
        }

        private void TronBegin() {
            var self = Entity as Player;
            self.Sprite.Play("startStarFly");
            self.starFlyTransforming = true;
            self.starFlyTimer = float.MaxValue;
            self.starFlySpeedLerp = 0f;
            self.jumpGraceTimer = 0f;
            if (self.starFlyBloom == null) {
                self.Add(self.starFlyBloom = new BloomPoint(new Vector2(0f, -6f), 0f, 16f));
            }
            self.starFlyBloom.Visible = true;
            self.starFlyBloom.Alpha = 0f;
            self.Collider = self.starFlyHitbox;
            self.hurtbox = self.starFlyHurtbox;
            if (self.starFlyLoopSfx == null) {
                self.Add(self.starFlyLoopSfx = new SoundSource());
                self.starFlyLoopSfx.DisposeOnTransition = false;
                self.Add(self.starFlyWarningSfx = new SoundSource());
                self.starFlyWarningSfx.DisposeOnTransition = false;
            }
            self.starFlyLoopSfx.Play("event:/game/06_reflection/feather_state_loop", "feather_speed", 1f);
            self.starFlyWarningSfx.Stop();
        }

        private void TronEnd() {
            var self = Entity as Player;
            self.Play("event:/game/06_reflection/feather_state_end");
            self.starFlyWarningSfx.Stop();
            self.starFlyLoopSfx.Stop();
            self.Hair.DrawPlayerSpriteOutline = false;
            self.Sprite.Color = Color.White;
            self.level.Displacement.AddBurst(self.Center, 0.25f, 8f, 32f);
            self.starFlyBloom.Visible = false;
            self.Sprite.HairCount = self.startHairCount;
            TronReturnToNormalHitbox(self);
            if (self.StateMachine.State != Player.StDash) {
                self.level.Particles.Emit(FlyFeather.P_Boost, 12, self.Center, Vector2.One * 4f, (-self.Speed).Angle());
            }
        }

        private void TronReturnToNormalHitbox(Player self) {
            self.Collider = self.normalHitbox;
            self.hurtbox = self.normalHurtbox;
            if (!self.CollideCheck<Solid>()) {
                return;
            }
            Vector2 position = self.Position;
            self.Y -= self.normalHitbox.Bottom - self.starFlyHitbox.Bottom;
            if (self.CollideCheck<Solid>()) {
                self.Position = position;
                self.Ducking = true;
                self.Y -= self.duckHitbox.Bottom - self.starFlyHitbox.Bottom;
                if (self.CollideCheck<Solid>()) {
                    self.Position = position;
                    self.Die(Vector2.Zero);
                }
            }
        }

        private IEnumerator TronCoroutine() {
            var self = Entity as Player;
            while (self.Sprite.CurrentAnimationID == "startStarFly") {
                yield return null;
            }
            while (self.Speed != Vector2.Zero) {
                yield return null;
            }
            yield return 0.1f;
            trailFade = 1;
            trail.Clear();
            AddTrail(self.Center);
            self.Sprite.Color = HairColor ?? self.starFlyColor;
            self.Sprite.HairCount = 7;
            self.Hair.DrawPlayerSpriteOutline = true;
            self.level.Displacement.AddBurst(self.Center, 0.25f, 8f, 32f);
            self.starFlyTransforming = false;
            self.starFlyTimer = 2f;
            self.RefillDash();
            self.RefillStamina();
            Vector2 vector = Input.Feather.Value;
            if (vector == Vector2.Zero) {
                vector = Vector2.UnitX * (float)self.Facing;
            }
            self.Speed = vector * 250f;
            self.starFlyLastDir = vector;
            self.level.Particles.Emit(FlyFeather.P_Boost, 12, self.Center, Vector2.One * 4f, (-vector).Angle());
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            self.level.DirectionalShake(self.starFlyLastDir);
            while (self.starFlyTimer > 0.5f) {
                yield return null;
            }
            self.starFlyWarningSfx.Play("event:/game/06_reflection/feather_state_warning");
        }

        private int TronUpdate() {
            var self = Entity as Player;

            self.Sprite.HairCount = 7;
            self.starFlyBloom.Alpha = Calc.Approach(self.starFlyBloom.Alpha, 0.7f, Engine.DeltaTime * 2f);
            Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
            if (self.starFlyTransforming) {
                self.Speed = Calc.Approach(self.Speed, Vector2.Zero, 1000f * Engine.DeltaTime);
            } else {
                Vector2 featherSteer = Input.Feather.Value;
                bool flag = false;
                if (featherSteer == Vector2.Zero) {
                    flag = true;
                    featherSteer = self.starFlyLastDir;
                }
                Vector2 vector = self.Speed.SafeNormalize(Vector2.Zero);
                vector = (self.starFlyLastDir = ((!(vector == Vector2.Zero)) ? vector.RotateTowards(featherSteer.Angle(), 5.58505344f * Engine.DeltaTime) : featherSteer));
                float target;
                if (flag) {
                    self.starFlySpeedLerp = 0f;
                    target = SlowSpeed;
                } else if (vector != Vector2.Zero && Vector2.Dot(vector, featherSteer) >= 0.45f) {
                    self.starFlySpeedLerp = Calc.Approach(self.starFlySpeedLerp, 1f, Engine.DeltaTime / 1f);
                    target = MathHelper.Lerp(TargetSpeed, MaxSpeed, self.starFlySpeedLerp);
                } else {
                    self.starFlySpeedLerp = 0f;
                    target = TargetSpeed;
                }
                self.starFlyLoopSfx.Param("feather_speed", (!flag) ? 1 : 0);
                float val = self.Speed.Length();
                val = Calc.Approach(val, target, 1000f * Engine.DeltaTime);
                self.Speed = vector * val;
                if (self.level.OnInterval(0.02f)) {
                    self.level.Particles.Emit(FlyFeather.P_Flying, 1, self.Center, Vector2.One * 2f, (-self.Speed).Angle());
                }
                float minDist = float.MaxValue;
                for (int i = 0; i < trail.Count - 1; i++) {
                    if ((trail[i] - self.Center).LengthSquared() < minDist) {
                        minDist = (trail[i] - self.Center).LengthSquared();
                    }
                    if ((trail[i] - self.Center).LengthSquared() < pointSpacingSq * Engine.DeltaTime * Engine.DeltaTime) {
                        self.Die(Vector2.Zero);
                        break;
                    }
                }
                if (trail.Count > 0 && (self.Center - trail.Last()).LengthSquared() >= pointSpacingSq * Engine.DeltaTime * Engine.DeltaTime) {
                    AddTrail(self.Center);
                }
            }
            return TronStateID;
        }

        public static void Load() {
            On.Celeste.Player.ctor += Player_ctor;
            On.Celeste.Player.Render += Player_Render;
            On.Celeste.PlayerDeadBody.Awake += PlayerDeadBody_Awake;
            On.Celeste.Player.UpdateHair += Player_UpdateHair;
            On.Celeste.Player.UpdateSprite += Player_UpdateSprite;
            On.Celeste.Player.OnCollideH += Player_OnCollideH;
            On.Celeste.Player.OnCollideV += Player_OnCollideV;
        }

        private static void PlayerDeadBody_Awake(On.Celeste.PlayerDeadBody.orig_Awake orig, PlayerDeadBody self, Scene scene) {
            orig(self, scene);
            // Move the component to the corpse
            var state = self.player?.Get<TronState>();
            if (state != null) {
                self.Add(state);
            }
        }

        private static void Player_OnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            var id = self.Get<TronState>()?.TronStateID;
            if (self.StateMachine.State == id) {
                self.StateMachine.state = Player.StStarFly;
                orig(self, data);
                self.StateMachine.state = id.Value;
            } else {
                orig(self, data);
            }
        }

        private static void Player_OnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            var id = self.Get<TronState>()?.TronStateID;
            if (self.StateMachine.State == id) {
                self.StateMachine.state = Player.StStarFly;
                orig(self, data);
                self.StateMachine.state = id.Value;
            } else {
                orig(self, data);
            }
        }

        private static void Player_UpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self) {
            var id = self.Get<TronState>()?.TronStateID;
            if (self.StateMachine.State == id) {
                self.StateMachine.state = Player.StStarFly;
                orig(self);
                self.StateMachine.state = id.Value;
            } else {
                orig(self);
            }
        }

        private static void Player_UpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity) {
            var id = self.Get<TronState>()?.TronStateID;
            if (self.StateMachine.State == id) {
                self.StateMachine.state = Player.StStarFly;
                orig(self, applyGravity);
                self.StateMachine.state = id.Value;
            } else {
                orig(self, applyGravity);
            }
        }

        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self) {
            var id = self.Get<TronState>()?.TronStateID;
            if (self.StateMachine.State == id) {
                self.StateMachine.state = Player.StStarFly;
                orig(self);
                self.StateMachine.state = id.Value;
            } else {
                orig(self);
            }
        }

        private static void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);
            self.Add(new TronState());
        }

        public static void Unload() {
            On.Celeste.Player.ctor -= Player_ctor;
            On.Celeste.Player.Render -= Player_Render;
            On.Celeste.PlayerDeadBody.Awake -= PlayerDeadBody_Awake;
            On.Celeste.Player.UpdateHair -= Player_UpdateHair;
            On.Celeste.Player.UpdateSprite -= Player_UpdateSprite;
            On.Celeste.Player.OnCollideH -= Player_OnCollideH;
            On.Celeste.Player.OnCollideV -= Player_OnCollideV;
        }
    }
}
