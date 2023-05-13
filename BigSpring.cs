using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/bigSpring")]
    public class BigSpring : Spring {

        private static FieldInfo playerCanUseInfo = typeof(Spring).GetField("playerCanUse", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo spriteInfo = typeof(Spring).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        private static MethodInfo BounceAnimateInfo = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

        private bool dashSpring;

        private int width;

        private int horizontalTexturePadding;

        private bool slicedSprite;

        // left, center, right
        private int[] sliceWidths;

        private bool ignoreRedBoosters;

        public BigSpring(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Int("orientation", 0) == 3 ? Orientations.Floor : (Orientations)data.Int("orientation", 0), data.Bool("playerCanUse", true)) {
            Orientation = (Orientations)data.Int("orientation", 0);
            dashSpring = data.Bool("dashSpring", false);
            ignoreRedBoosters = data.Bool("ignoreRedBoosters", false);
            width = data.Width;
            horizontalTexturePadding = data.Int("horizontalTexturePadding", 4);
            slicedSprite = data.Bool("slicedSprite", false);
            DynData<Spring> selfData = new(this);

            if (data.Bool("ignoreHoldables", false)) {
                Remove(Get<HoldableCollider>());
            }

            // Only one other player collider is added so it can easily be removed
            Remove(Get<PlayerCollider>());
            Add(new PlayerCollider(OnCollide));
            Sprite sprite = (Sprite)spriteInfo.GetValue(this);
            string defaultSpritePath = dashSpring ? "objects/BrokemiaHelper/dashSpring/" : "objects/spring/";
            string spritePath = data.Attr("sprite", defaultSpritePath);
            sprite.Reset(GFX.Game, spritePath == "" ? defaultSpritePath : spritePath);
            sprite.Add("idle", "", 0f, default(int));
            sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            sprite.Add("disabled", "white", 0.07f);
            sprite.Play("idle");
            sprite.Origin.X = sprite.Width / 2f;
            sprite.Origin.Y = sprite.Height;
            sprite.Scale = new((width - horizontalTexturePadding) / (sprite.Width - horizontalTexturePadding), 1);

            if(slicedSprite) {
                sprite.Visible = false;
            }

            switch (Orientation) {
                case Orientations.Floor:
                    Collider = new Hitbox(width, 6f, -width / 2, -6f);
                    Get<PufferCollider>().Collider = new Hitbox(width, 10f, -width / 2, -10f);
                    break;
                case Orientations.WallLeft:
                    Collider = new Hitbox(6f, width, 0f, -width / 2);
                    Get<PufferCollider>().Collider = new Hitbox(12f, width, 0f, -width / 2);
                    break;
                case Orientations.WallRight:
                    Collider = new Hitbox(6f, width, -6f, -width / 2);
                    Get<PufferCollider>().Collider = new Hitbox(12f, width, -12f, -width / 2);
                    break;
                case (Orientations)3:
                    selfData.Get<StaticMover>("staticMover").SolidChecker = ((Solid s) => CollideCheck(s, Position - Vector2.UnitY));
                    selfData.Get<StaticMover>("staticMover").JumpThruChecker = ((JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY));
                    Collider = new Hitbox(width, 6f, -width / 2, 0f);
                    Get<PufferCollider>().Collider = new Hitbox(width, 10f, -width / 2, 0f);
                    sprite.Rotation = (float)Math.PI;
                    break;
                default:
                    throw new Exception("Orientation not supported!");
            }
        }

        protected void OnCollide(Player player) {
            if (dashSpring && (player.StateMachine.State == 9 || !(bool)playerCanUseInfo.GetValue(this) || !player.DashAttacking)) {
                return;
            }
            if (ignoreRedBoosters && player.StateMachine.State == Player.StRedDash) {
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

        public override void Render() {
            base.Render();
            if(slicedSprite) {
                Sprite sprite = (Sprite)spriteInfo.GetValue(this);
                sprite.DrawSubrect(Vector2.Zero, new Rectangle(horizontalTexturePadding, 0, sliceWidths[0], (int)sprite.Height));
            }
        }
    }
}
