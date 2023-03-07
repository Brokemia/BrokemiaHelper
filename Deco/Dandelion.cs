using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrokemiaHelper.Deco {
    [CustomEntity("BrokemiaHelper/dandelion")]
    public class Dandelion : Entity {
        private class Seed {
            public Vector2 position;
            public Vector2 velocity;
            public bool flipH;
            public bool flipV;
            public float timer;
        }

        private MTexture seedTexture;
        private List<Seed> seeds = new();
        private List<Seed> seedsToRemove = new();
        private int seedCount;
        private Vector2 seedStart;
        private Sprite sprite;
        private PlayerCollider playerCollider;
        private Random rand;
        private bool scattered;
        
        public Dandelion(
                Vector2 pos,
                int seedCount,
                Vector2 seedStart,
                MTexture seedTexture,
                float scatterWidth,
                float scatterHeight,
                string spriteName,
                bool flipH,
                bool flipV,
                int depth
            ) : base(pos) {
            Depth = depth;
            this.seedCount = seedCount;
            this.seedStart = seedStart;
            this.seedTexture = seedTexture;
            Collider = new Hitbox(scatterWidth, scatterHeight, -scatterWidth / 2, -scatterHeight);
            Add(sprite = GFX.SpriteBank.Create(spriteName));
            sprite.FlipX = flipH;
            sprite.FlipY = flipV;
            sprite.Play("idle", true, true);
            Add(playerCollider = new PlayerCollider(OnPlayer));
        }

        public Dandelion(EntityData data, Vector2 offset) : this(
                data.Position + offset,
                data.Int("seeds", 5),
                new(data.Float("seedStartX"), data.Float("seedStartY", -5)),
                GFX.Game[data.Attr("seedTexture", "deco/BrokemiaHelper/dandelion/seed")],
                data.Float("flyAwayWidth", 8),
                data.Float("flyAwayHeight", 16),
                data.Attr("sprite", "BrokemiaHelper_DandelionSmall"),
                data.Bool("flipH"),
                data.Bool("flipV"),
                data.Int("depth", Depths.BGDecals)
            ) {
            
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            rand = new Random(Calc.Random.Next());
        }

        private void OnPlayer(Player player) {
            for(int i = 0; i < seedCount; i++) {
                seeds.Add(new Seed {
                    position = seedStart,
                    velocity = player.Speed * 0.5f + Calc.AngleToVector(rand.NextAngle(), player.Speed.Length() * 0.1f),
                    flipH = rand.Next(2) == 0,
                    flipV = rand.Next(2) == 0,
                    timer = rand.NextFloat(4) + 2
                });
            }
            sprite.Play("scatter", false, true);
            scattered = true;
        }

        public override void Update() {
            base.Update();
            if(scattered && playerCollider != null) {
                Remove(playerCollider);
            }

            for (int i = 0; i < seeds.Count; i++) {
                var seed = seeds[i];
                seed.position += seed.velocity * Engine.DeltaTime;
                seed.velocity *= 0.995f;
                seed.velocity += Calc.AngleToVector(rand.NextAngle(), 2);
                seed.timer -= Engine.DeltaTime;
                if (seed.timer < 0) {
                    seedsToRemove.Add(seed);
                }
            }
            foreach(var seed in seedsToRemove) {
                seeds.Remove(seed);
            }
            seedsToRemove.Clear();
        }

        public override void Render() {
            base.Render();
            foreach(var seed in seeds) {
                seedTexture.DrawCentered(Position + seed.position, Color.White * (seed.timer < 1 ? seed.timer : 1), 1, 0,
                    (seed.flipH ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (seed.flipV ? SpriteEffects.FlipVertically : SpriteEffects.None));
            }
        }
    }
}
