using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/floatierSpaceBlock")]
    public class FloatierSpaceBlock : FloatySpaceBlock {
        public float floatinessBoost;
        public float dashEaseMultiplier;
        public float dashOffsetMultiplier;
        private float naturalFloatiness;
        private int sinkAmount;
        private float unsinkDelay;
        private float sinkSpeed;
        private float unsinkSpeed;

        private static FieldInfo sinkTimerInfo = typeof(FloatySpaceBlock).GetField("sinkTimer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo yLerpInfo = typeof(FloatySpaceBlock).GetField("yLerp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo sineWaveInfo = typeof(FloatySpaceBlock).GetField("sineWave", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo dashEaseInfo = typeof(FloatySpaceBlock).GetField("dashEase", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo dashDirectionInfo = typeof(FloatySpaceBlock).GetField("dashDirection", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        private static PropertyInfo MasterOfGroupInfo = typeof(FloatySpaceBlock).GetProperty("MasterOfGroup", BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

        public FloatierSpaceBlock(EntityData data, Vector2 offset) : base(data, offset) {
            // Just in case someone was relying on the old default, don't mess with it unless they're using new fields
            if (data.Has("naturalFloatiness")) {
                floatinessBoost = data.Float("floatinessMultiplier", 0);
            } else {
                floatinessBoost = data.Float("floatinessMultiplier", 1);
            }
            dashEaseMultiplier = data.Float("bounceBackMultiplier", 1);
            dashOffsetMultiplier = data.Float("dashOffsetMultiplier", floatinessBoost);
            naturalFloatiness = data.Float("naturalFloatiness", 0);
            sinkAmount = data.Int("sinkAmount", 0);
            unsinkDelay = data.Float("unsinkDelay", 0);
            sinkSpeed = data.Float("sinkSpeed", 1);
            unsinkSpeed = data.Float("unsinkSpeed", 1);
        }

        public static void Load() {
            On.Monocle.Tracker.Initialize += InitializeTracker;
        }

        public static void Unload() {
            On.Monocle.Tracker.Initialize -= InitializeTracker;
        }

        private static void InitializeTracker(On.Monocle.Tracker.orig_Initialize orig) {
            orig();
            Tracker.TrackedEntityTypes[typeof(FloatierSpaceBlock)].Add(typeof(FloatySpaceBlock));
        }

        public override void Update() {
            bool masterOfGroup = MasterOfGroup;
            // If MasterOfGroup is false, FloatySpaceBlock does almost nothing in Update()
            MasterOfGroupInfo.SetValue(this, false);
            base.Update();
            MasterOfGroupInfo.SetValue(this, masterOfGroup);

            if (MasterOfGroup) {
                bool playerRiding = false;
                foreach (FloatySpaceBlock item in Group) {
                    if (item.HasPlayerRider()) {
                        playerRiding = true;
                        break;
                    }
                }
                if (!playerRiding) {
                    foreach (JumpThru jumpthru in Jumpthrus) {
                        if (jumpthru.HasPlayerRider()) {
                            playerRiding = true;
                            break;
                        }
                    }
                }
                float sinkTimerVal = (float)sinkTimerInfo.GetValue(this);
                // The block will keep sinking for 0.3 * floatinessBoost seconds after the player leaves
                if (playerRiding) {
                    sinkTimerInfo.SetValue(this, sinkTimerVal = 0.3f * floatinessBoost + unsinkDelay);
                } else if (sinkTimerVal > 0f) {
                    sinkTimerInfo.SetValue(this, sinkTimerVal -= Engine.DeltaTime);
                }
                float yLerpVal = (float)yLerpInfo.GetValue(this);
                // If the player is or just was standing on the block, make the block sink
                if (sinkTimerVal > 0f) {
                    yLerpInfo.SetValue(this, Calc.Approach(yLerpVal, 1f, sinkSpeed * Engine.DeltaTime));
                } else {
                    yLerpInfo.SetValue(this, Calc.Approach(yLerpVal, 0f, unsinkSpeed * Engine.DeltaTime));
                }
                sineWaveInfo.SetValue(this, (float)sineWaveInfo.GetValue(this) + Engine.DeltaTime);
                dashEaseInfo.SetValue(this, Calc.Approach((float)dashEaseInfo.GetValue(this), 0f, Engine.DeltaTime * 1.5f * dashEaseMultiplier));
                MoveToTarget();
            }
            LiftSpeed = Vector2.Zero;
        }

        private void MoveToTarget() {
            float sineWavePos = (float)Math.Sin((float)sineWaveInfo.GetValue(this)) * 4f;
            Vector2 dashOffset = Calc.YoYo(Ease.QuadIn((float)dashEaseInfo.GetValue(this))) * (Vector2)dashDirectionInfo.GetValue(this) * 8f * dashOffsetMultiplier;
            for (int i = 0; i < 2; i++) {
                foreach (KeyValuePair<Platform, Vector2> move in Moves) {
                    Platform key = move.Key;
                    bool flag = (key is JumpThru jumpThru && jumpThru.HasRider()) || (key is Solid solid && solid.HasRider());
                    if ((flag || i != 0) && (!flag || i != 1)) {
                        Vector2 intialPos = move.Value;
                        float sinkDestination = intialPos.Y + 12f * floatinessBoost + sinkAmount;
                        // This is the important line
                        float bobbingOffset = MathHelper.Lerp(intialPos.Y, sinkDestination, Ease.SineInOut((float)yLerpInfo.GetValue(this))) + sineWavePos * floatinessBoost + sineWavePos * naturalFloatiness;
                        key.MoveToY(bobbingOffset + dashOffset.Y);
                        key.MoveToX(intialPos.X + dashOffset.X);
                    }
                }
            }
        }
    }
}