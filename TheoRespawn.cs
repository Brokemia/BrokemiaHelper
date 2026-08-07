using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/theoRespawn")]
    [Tracked]
    class TheoRespawn : Entity {
        private const string TheoCrystalName = "theoCrystal";

        public string flag;
        private Vector2 positionBeforeOffset;
        private Vector2 offset;
        private string entityToSpawn;
        private Dictionary<string, object> entityDataValues;

        public TheoRespawn(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flag = data.Attr("flag");
            entityToSpawn = data.Attr("entityToSpawn", TheoCrystalName);
            positionBeforeOffset = data.Position;
            this.offset = offset;

            entityDataValues = new Dictionary<string, object>();
            foreach (string entityDataEntry in data.Attr("entityData").Split(","))
            {
                var entityDataPair = entityDataEntry.Split(":");
                if (entityDataPair.Length == 2)
                {
                    entityDataValues[entityDataPair[0]] = entityDataPair[1];
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            Level level = SceneAs<Level>();
            // Remove this if a flag is defined but not active in the session
            if (!string.IsNullOrWhiteSpace(flag) && !level.Session.GetFlag(flag)) {
                RemoveSelf();
                return;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            Level level = SceneAs<Level>();

            float thisDist = Vector2.Distance(Position, level.Session.RespawnPoint.Value);

            foreach (TheoRespawn respawn in scene.Tracker.GetEntities<TheoRespawn>()) {
                float dist = Vector2.Distance(respawn.Position, level.Session.RespawnPoint.Value);
                // If another TheoRespawn is closer than this one, remove this
                if (dist < thisDist) {
                    RemoveSelf();
                    return;
                }
            }

            var crystal = entityToSpawn == TheoCrystalName
                ? new TheoCrystal(Position)
                // Allow for spawning arbitrary modded entities
                : Level.EntityLoaders[entityToSpawn](level, level.Session.LevelData, offset, new EntityData {
                    Name = entityToSpawn,
                    Position = positionBeforeOffset,
                    Values = entityDataValues
                });
            scene.Add(crystal);
            RemoveSelf();
        }

    }
}
