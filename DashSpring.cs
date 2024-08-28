using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/dashSpring", "BrokemiaHelper/wallDashSpringRight", "BrokemiaHelper/wallDashSpringLeft", "BrokemiaHelper/dashSpringDown")]
    public class DashSpring : Spring {

        private static FieldInfo playerCanUseInfo = typeof(Spring).GetField("playerCanUse", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo spriteInfo = typeof(Spring).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        private static MethodInfo BounceAnimateInfo = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

        private bool ignoreRedBoosters;

        public DashSpring(Vector2 position, Orientations orientation, string spritePath, bool playerCanUse, bool ignoreHoldables, bool ignoreRedBubble)
            : base(position, orientation == (Orientations)3 ? Orientations.Floor : orientation, playerCanUse) {
            Orientation = orientation;
            ignoreRedBoosters = ignoreRedBubble;
            DynData<Spring> selfData = new DynData<Spring>(this);

            if(ignoreHoldables) {
                Remove(Get<HoldableCollider>());
            }

            // Only one other player collider is added so it can easily be removed
            Remove(Get<PlayerCollider>());
            Add(new PlayerCollider(OnCollide));
            Sprite sprite = (Sprite)spriteInfo.GetValue(this);
            sprite.Reset(GFX.Game, spritePath);
            sprite.Add("idle", "", 0f, default(int));
            sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            sprite.Add("disabled", "white", 0.07f);
            sprite.Play("idle");
            sprite.Origin.X = sprite.Width / 2f;
            sprite.Origin.Y = sprite.Height;
            if (orientation == (Orientations)3) {
                selfData.Get<StaticMover>("staticMover").SolidChecker = ((Solid s) => CollideCheck(s, Position - Vector2.UnitY));
                selfData.Get<StaticMover>("staticMover").JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY));
                Collider = new Hitbox(16f, 6f, -8f, 0f);
                Get<PufferCollider>().Collider = new Hitbox(16f, 10f, -8f, 0f);
                sprite.Rotation = (float)Math.PI;
            }

        }

        public DashSpring(EntityData data, Vector2 offset)
            : this(data.Position + offset, GetOrientationFromName(data.Name), data.Attr("spritePath", "objects/BrokemiaHelper/dashSpring/"), data.Bool("playerCanUse", true), data.Bool("ignoreHoldables", false), data.Bool("ignoreRedBoosters", false)) {
        }

        public static Orientations GetOrientationFromName(string name) {
            switch (name) {
                case "BrokemiaHelper/dashSpring":
                    return Orientations.Floor;
                case "BrokemiaHelper/wallDashSpringRight":
                    return Orientations.WallRight;
                case "BrokemiaHelper/wallDashSpringLeft":
                    return Orientations.WallLeft;
                case "BrokemiaHelper/dashSpringDown":
                    return (Orientations)3;
                default:
                    throw new Exception("Dash Spring name doesn't correlate to a valid Orientation!");
            }
        }

        protected new void OnCollide(Player player) {
            if (player.StateMachine.State == 9 || !(bool)playerCanUseInfo.GetValue(this) || !player.DashAttacking) {
                return;
            }
            if(ignoreRedBoosters && player.StateMachine.State == Player.StRedDash) {
                return;
            }
            if (Orientation == Orientations.Floor) {
                if (player.Speed.Y >= 0f) {
                    BounceAnimateInfo.Invoke(this, null);
                    player.SuperBounce(Top);
                }
                return;
            }
            if (Orientation == Orientations.WallLeft) {
                if (player.SideBounce(1, Right, CenterY)) {
                    BounceAnimateInfo.Invoke(this, null);
                }
                return;
            }
            if (Orientation == Orientations.WallRight) {
                if (player.SideBounce(-1, Left, CenterY)) {
                    BounceAnimateInfo.Invoke(this, null);
                }
                return;
            }
            if (Orientation == (Orientations)3) {
                if (player.Speed.Y <= 0f) {
                    BounceAnimateInfo.Invoke(this, null);
                    player.SuperBounce(Bottom + player.Height);
                    
                    DynData<Player> playerData = new DynData<Player>(player);
                    playerData["varJumpSpeed"] = player.Speed.Y = 185f;
                    SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.1f);
                }
                return;
            }
            throw new Exception("Orientation not supported!");
        }
    }
}
