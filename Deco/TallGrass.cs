using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace BrokemiaHelper.Deco {
    [CustomEntity("BrokemiaHelper/tallGrass")]
    public class TallGrass : Entity {
        private MTexture grass, grassSpread;
        private int width;
        private float[] wiggles;
        private float wiggleSpeed;
        private float wiggleFrequency;
        private float minPlayerSpeedSq;
        private float scaleMultiplier;
        private float rotationMultiplier;

        public TallGrass(EntityData data, Vector2 offset) : base(data.Position + offset) {
            grass = GFX.Game[data.Attr("grassTexture", "deco/BrokemiaHelper/grass")];
            grassSpread = GFX.Game[data.Attr("grassSpreadTexture", "deco/BrokemiaHelper/grassSpread")];
            width = (data.Width / grass.Width) * grass.Width;
            Depth = data.Int("depth", Depths.Above);
            wiggles = new float[width / grass.Width];
            wiggleSpeed = data.Float("wiggleSpeed", 3f);
            wiggleFrequency = data.Float("wiggleFrequency", 1.5f);
            minPlayerSpeedSq = data.Float("minPlayerSpeed", 10f);
            minPlayerSpeedSq *= minPlayerSpeedSq;
            scaleMultiplier = data.Float("scaleMultiplier", 0.1f);
            rotationMultiplier = data.Float("rotationMultiplier", 0.15f);
        }

        public override void Update() {
            base.Update();
            for (int i = 0; i < wiggles.Length; i++) {
                wiggles[i] = Calc.Approach(wiggles[i], 0, Engine.DeltaTime * wiggleSpeed);
            }
            if (Scene.Tracker.GetEntity<Player>() is { } player && player.Speed.LengthSquared() > minPlayerSpeedSq
                && player.Right >= X && player.Left <= X + width && player.Bottom > Y - grass.Height / 2 && player.Top < Y) {
                wiggles[Calc.Clamp((int)((player.Center.X - X) / grass.Width), 0, wiggles.Length - 1)] = 1;
            }
        }

        public override void Render() {
            base.Render();
            var player = Scene.Tracker.GetEntity<Player>();
            for (int i = 0; i < width / grass.Width; i++) {
                var pos = Position + new Vector2(i * grass.Width, 0);
                var halfGrassOffset = new Vector2(grass.Width / 2f, 0);

                var wiggleEased = Ease.CubeOut(wiggles[i]) * (float)Math.Sin(wiggles[i] * 6.28 * wiggleFrequency);
                var scale = new Vector2(1, 1f + scaleMultiplier * wiggleEased);
                var rotation = wiggleEased * rotationMultiplier;

                if (player == null || player.Bottom <= Y - grass.Height / 2 || player.Top >= Y) {
                    grass.DrawJustified(pos + halfGrassOffset, new(0.5f, 1), Color.White, scale, rotation);
                    continue;
                }
                
                if (player.Left > pos.X) {
                    grass.DrawJustifiedClipped(pos + halfGrassOffset, new(0.5f, 1),
                        new Rectangle(0, 0, Math.Min(grass.Width, (int)(player.Left - pos.X)), grass.Height), scale, rotation);
                }
                if (player.Right < pos.X + grass.Width) {
                    var newX = Math.Max(0, (int)(player.Right - pos.X));
                    grass.DrawJustifiedClipped(pos + halfGrassOffset + new Vector2(newX, 0), new(0.5f, 1),
                        new Rectangle(Math.Max(0, newX), 0,
                            Math.Min(grass.Width, (int)(pos.X + grass.Width - player.Right)), grass.Height), scale, rotation);
                }
            }
            if (player != null) {
                if (player.Right >= X && player.Left <= X + width && player.Bottom > Y - grass.Height / 2 && player.Top < Y) {
                    grassSpread.DrawJustifiedClipped(new(Math.Max(player.Left, X), Y), new(0, 1),
                        new Rectangle(Math.Max(0, (int)(X - player.Left)), 0, (int)Calc.Min(player.Right - X, X + width - player.Left, grassSpread.Width), grassSpread.Height));
                }
            }
        }
    }
}
