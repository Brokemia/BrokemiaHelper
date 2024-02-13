using Celeste;
using Celeste.Mod;
using Monocle;
using System;
using System.Linq;

namespace BrokemiaHelper.Deco {
    public class PlayerMotionComponent : Component {

        public static void Load() {
            DecalRegistry.AddPropertyHandler("BrokemiaHelper_playerMotion", (decal, attrs) => {
                decal.Add(new PlayerMotionComponent(
                    Calc.ReadCSVIntWithTricks(attrs["idle"]?.Value ?? "0"),
                    Calc.ReadCSVIntWithTricks(attrs["active"]?.Value ?? "0"),
                    attrs["sfx"]?.Value,
                    float.Parse(attrs["delay"]?.Value ?? "0.1"),
                    float.Parse(attrs["minPlayerSpeed"]?.Value ?? "10"),
                    int.Parse(attrs["hitboxWidth"]?.Value ?? "8"),
                    int.Parse(attrs["hitboxHeight"]?.Value ?? "8"),
                    int.Parse(attrs["hitboxOffsetX"]?.Value ?? "-4"),
                    int.Parse(attrs["hitboxOffsetY"]?.Value ?? "-4")
                ));
            });
        }

        private int[] idleFrames;
        private int[] activeFrames;
        private string[] sfx;
        private float delay;
        private float minPlayerSpeedSq;
        private Random rand;
        private Sprite sprite;
        private Hitbox hitbox;

        public PlayerMotionComponent(int[] idleFrames, int[] activeFrames, string sfx, float delay, float minPlayerSpeed, int hitboxW, int hitboxH, int hitboxX, int hitboxY) : base(false, false) {
            this.idleFrames = idleFrames;
            this.activeFrames = activeFrames;
            this.sfx = string.IsNullOrWhiteSpace(sfx) ? null : sfx.Split(',');
            this.delay = delay;
            minPlayerSpeedSq = minPlayerSpeed * minPlayerSpeed;
            hitbox = new Hitbox(hitboxW, hitboxH, hitboxX, hitboxY);
        }

        public override void Added(Entity entity) {
            base.Added(entity);
            var decal = entity as Decal;

            sprite = (Sprite)(decal.image = new Sprite(null, null));
            sprite.AddLoop("idle", delay, idleFrames.Select(i => decal.textures[i]).ToArray());
            sprite.Add("active", delay, "idle", activeFrames.Select(i => decal.textures[i]).ToArray());
            sprite.Play("idle", restart: true);
            sprite.Scale = decal.scale;
            sprite.Rotation = decal.Rotation;
            sprite.CenterOrigin();
            entity.Collider = hitbox;
            decal.Add(sprite);
            decal.Add(new PlayerCollider(OnPlayer));
        }

        public override void EntityAwake() {
            base.EntityAwake();
            rand = new Random(Calc.Random.Next());
        }

        private void OnPlayer(Player player) {
            if (sprite.CurrentAnimationID == "idle" && player.Speed.LengthSquared() >= minPlayerSpeedSq) {
                sprite.Play("active");
                if (sfx != null) {
                    Audio.Play(sfx[rand.Next(sfx.Length)]);
                }
            }
        }
    }
}
