using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokemiaHelper.Deco {
    [CustomEntity("BrokemiaHelper/flowerField")]
    public class FlowerField : Entity {
        private List<Action<Vector2, bool>> spawners = new();
        private bool randomFlipH;
        private int width;
        private float flowerDensity;
        private string randomSeed;
        private Random rand;

        public FlowerField(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Depth = data.Int("depth", Depths.BGDecals);
            randomFlipH = data.Bool("randomFlipH", true);
            width = data.Width;
            flowerDensity = data.Float("flowerDensity", 0.25f);
            randomSeed = data.Attr("randomSeed");

            if (data.Bool("smallDandelions", true)) {
                spawners.Add(SpawnSmallDandelion);
            }
            if (data.Bool("largeDandelions", true)) {
                spawners.Add(SpawnLargeDandelion);
            }
            foreach (var decal in data.Attr("decals", "").Split(',')) {
                if (!string.IsNullOrWhiteSpace(decal)) {
                    spawners.Add((pos, flip) => SpawnDecal(pos, flip, decal));
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if(spawners.Count == 0) {
                RemoveSelf();
                return;
            }
            
            rand = new Random(string.IsNullOrWhiteSpace(randomSeed) ? Calc.Random.Next() :
                randomSeed.SimpleHash());
            
            for (int i = 0; i < width; i++) {
                if(rand.NextFloat() < flowerDensity) {
                    spawners[rand.Next(spawners.Count)](Position + new Vector2(i, 0), randomFlipH && (rand.Next(2) == 0));
                }
            }

            RemoveSelf();
        }

        private void SpawnSmallDandelion(Vector2 pos, bool flipH) {
            var data = new EntityData {
                Position = pos,
                Values = new()
            };
            data.Values["flipH"] = flipH;
            data.Values["depth"] = Depth;
            Scene.Add(new Dandelion(data, Vector2.Zero));
        }

        private void SpawnLargeDandelion(Vector2 pos, bool flipH) {
            var data = new EntityData {
                Position = pos,
                Values = new()
            };
            data.Values["flipH"] = flipH;
            data.Values["sprite"] = "BrokemiaHelper_DandelionLarge";
            data.Values["seedStartY"] = -8;
            data.Values["depth"] = Depth;
            Scene.Add(new Dandelion(data, Vector2.Zero));
        }

        private void SpawnDecal(Vector2 pos, bool flipH, string decal) {
            var entity = new Decal(decal, pos, new(flipH ? -1 : 1, 1), Depth);
            entity.Position.Y -= entity.textures[0].Height / 2;
            Scene.Add(entity);
        }
    }
}
