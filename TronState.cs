using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BrokemiaHelper {
    // Largely copied from vanilla feathers
    public class TronState {
        public static int TronStateID;
        private const float defaultSlowSpeed = Player.StarFlySlowSpeed;
        private const float defaultTargetSpeed = Player.StarFlyTargetSpeed;
        private const float defaultMaxSpeed = Player.StarFlyMaxSpeed;

        private static Color? lastColor;
        public static List<Vector2> trail;
        internal static float trailFade;
        public static readonly float pointSpacingSq = 90 * 90;

        public static event Action<Vector2> OnAddTrailPt;

        public static void Load() {
            On.Celeste.Player.ctor += Player_ctor;
            On.Celeste.Player.Render += Player_Render;
            On.Celeste.PlayerDeadBody.Render += PlayerDeadBody_Render;
            On.Celeste.PlayerDeadBody.Update += PlayerDeadBody_Update;
            On.Celeste.Player.UpdateHair += Player_UpdateHair;
            On.Celeste.Player.UpdateSprite += Player_UpdateSprite;
            On.Celeste.Player.OnCollideH += Player_OnCollideH;
            On.Celeste.Player.OnCollideV += Player_OnCollideV;
        }

        private static void PlayerDeadBody_Update(On.Celeste.PlayerDeadBody.orig_Update orig, PlayerDeadBody self) {
            orig(self);
            trailFade = Calc.Approach(trailFade, 0, 2 * Engine.DeltaTime);
        }

        private static void PlayerDeadBody_Render(On.Celeste.PlayerDeadBody.orig_Render orig, PlayerDeadBody self) {
            orig(self);
            TrailRender(null);
        }

        private static void Player_OnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            if (self.StateMachine.State == TronStateID) {
                self.StateMachine.state = Player.StStarFly;
                orig(self, data);
                self.StateMachine.state = TronStateID;
            } else {
                orig(self, data);
            }
        }

        private static void Player_OnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            if (self.StateMachine.State == TronStateID) {
                self.StateMachine.state = Player.StStarFly;
                orig(self, data);
                self.StateMachine.state = TronStateID;
            } else {
                orig(self, data);
            }
        }

        private static void Player_UpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self) {
            if (self.StateMachine.State == TronStateID) {
                self.StateMachine.state = Player.StStarFly;
                orig(self);
                self.StateMachine.state = TronStateID;
            } else {
                orig(self);
            }
        }

        private static void Player_UpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity) {
            if (self.StateMachine.State == TronStateID) {
                self.StateMachine.state = Player.StStarFly;
                orig(self, applyGravity);
                self.StateMachine.state = TronStateID;
            } else {
                orig(self, applyGravity);
            }
        }

        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self) {
            if (self.StateMachine.State == TronStateID) {
                self.StateMachine.state = Player.StStarFly;
                orig(self);
                self.StateMachine.state = TronStateID;
            } else {
                orig(self);
            }
            TrailRender(self);
        }

        private static void TrailRender(Player self) {
            if (trail != null) {
                if (self != null) {
                    lastColor = DynamicData.For(self).TryGet("BrokemiaHelperTronHairColor", out Color? color) ? color : self.starFlyColor;
                }
                if(lastColor == null) {
                    return;
                }
                lastColor *= trailFade;
                for (int j = 0; j < trail.Count - 1; j++) {
                    Draw.Line(trail[j], trail[j + 1], lastColor.Value, 3);
                }
            }
        }

        private static void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);
            TronStateID = self.StateMachine.AddState(() => TronUpdate(self), () => TronCoroutine(self), () => TronBegin(self), () => TronEnd(self));
            trailFade = 1;
            trail = new();
        }
        
        private static void AddTrail(Player self, Vector2 pt) {
            trail.Add(pt);
            OnAddTrailPt?.Invoke(pt);
        }

        public static bool StartTron(Player self) {
            self.RefillStamina();
            if (self.StateMachine.State == Player.StReflectionFall) {
                return false;
            }
            if (self.StateMachine.State == TronStateID) {
                self.starFlyTimer = float.MaxValue;
                self.Sprite.Color = DynamicData.For(self).TryGet("BrokemiaHelperTronHairColor", out Color? color) ? color.Value : self.starFlyColor;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            } else {
                self.StateMachine.State = TronStateID;
            }
            return true;
        }

        private static void TronBegin(Player self) {
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

        private static void TronEnd(Player self) {
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

        private static void TronReturnToNormalHitbox(Player self) {
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

        private static IEnumerator TronCoroutine(Player self) {
            while (self.Sprite.CurrentAnimationID == "startStarFly") {
                yield return null;
            }
            while (self.Speed != Vector2.Zero) {
                yield return null;
            }
            yield return 0.1f;
            trailFade = 1;
            trail.Clear();
            AddTrail(self, self.Center);
            self.Sprite.Color = DynamicData.For(self).TryGet("BrokemiaHelperTronHairColor", out Color? color) ? color.Value : self.starFlyColor;
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

        private static int TronUpdate(Player self) {
            // Allow other mods to configure the speed;
            var selfData = DynamicData.For(self);
            float? slowSpeed = selfData.TryGet("BrokemiaHelperTronSlowSpeed", out slowSpeed) ? slowSpeed : defaultSlowSpeed;
            float? targetSpeed = selfData.TryGet("BrokemiaHelperTronTargetSpeed", out targetSpeed) ? targetSpeed : defaultTargetSpeed;
            float? maxSpeed = selfData.TryGet("BrokemiaHelperTronMaxSpeed", out maxSpeed) ? maxSpeed : defaultMaxSpeed;

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
                    target = slowSpeed.Value;
                } else if (vector != Vector2.Zero && Vector2.Dot(vector, featherSteer) >= 0.45f) {
                    self.starFlySpeedLerp = Calc.Approach(self.starFlySpeedLerp, 1f, Engine.DeltaTime / 1f);
                    target = MathHelper.Lerp(targetSpeed.Value, maxSpeed.Value, self.starFlySpeedLerp);
                } else {
                    self.starFlySpeedLerp = 0f;
                    target = targetSpeed.Value;
                }
                self.starFlyLoopSfx.Param("feather_speed", (!flag) ? 1 : 0);
                float val = self.Speed.Length();
                val = Calc.Approach(val, target, 1000f * Engine.DeltaTime);
                self.Speed = vector * val;
                if (self.level.OnInterval(0.02f)) {
                    self.level.Particles.Emit(FlyFeather.P_Flying, 1, self.Center, Vector2.One * 2f, (-self.Speed).Angle());
                }
                float minDist = float.MaxValue;
                for(int i = 0; i < trail.Count - 1; i++) {
                    if((trail[i] - self.Center).LengthSquared() < minDist) {
                        minDist = (trail[i] - self.Center).LengthSquared();
                    }
                    if ((trail[i] - self.Center).LengthSquared() < pointSpacingSq * Engine.DeltaTime * Engine.DeltaTime) {
                        self.Die(Vector2.Zero);
                        break;
                    }
                }
                if ((self.Center - trail.Last()).LengthSquared() >= pointSpacingSq * Engine.DeltaTime * Engine.DeltaTime) {
                    AddTrail(self, self.Center);
                }
            }
            return TronStateID;
        }

        public static void Unload() {
            On.Celeste.Player.ctor -= Player_ctor;
            On.Celeste.Player.Render -= Player_Render;
        }
    }
}
