using System;
using System.Collections.Generic;
using BrokemiaHelper.Deco;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;

namespace BrokemiaHelper {
    public class BrokemiaHelperMapDataProcessor : EverestMapDataProcessor {
        public int Checkpoint;

        public string LevelName;
        public Vector2 LevelOffset;

        public List<NoodleEmitter.NoodleEmitterDetails> NoodleEmitterIDs;

        public override void End() {
            NoodleEmitter.EmittersByLevel[(AreaKey.ID, AreaKey.Mode)] = NoodleEmitterIDs;
        }

        public override Dictionary<string, Action<BinaryPacker.Element>> Init() {
            return new Dictionary<string, Action<BinaryPacker.Element>> {
                {
                    "level",
                    delegate(BinaryPacker.Element levelData) {
                        LevelName = levelData.Attr("name").Split(':')[0];
                        if (LevelName.StartsWith("lvl_")) {
                            LevelName = LevelName.Substring(4);
                        }
                        LevelOffset = new(levelData.AttrFloat("x"), levelData.AttrFloat("y"));
                    }
                },
                {
                    "entities",
                    delegate(BinaryPacker.Element levelChild) {
                        foreach(BinaryPacker.Element child in levelChild.Children) {
                            if(child.Name == "checkpoint") {
                                Checkpoint++;
                            }
                        }
                    }
                },
                {
                    "entity:brokemiahelper/cassetteCassette",
                    delegate(BinaryPacker.Element root) {
                        if(AreaData.CassetteCheckpointIndex < 0) {
                            AreaData.CassetteCheckpointIndex = Checkpoint;
                        }
                        if(ParentAreaData.CassetteCheckpointIndex < 0) {
                            ParentAreaData.CassetteCheckpointIndex = Checkpoint + ((ParentMode.Checkpoints != null) ? ParentMode.Checkpoints.Length : 0);
                        }
                        MapData.DetectedCassette = true;
                        ParentMapData.DetectedCassette = true;
                    }
                },
                {
                    "entity:BrokemiaHelper/noodleEmitter",
                    (emitter) => {
                        var id = emitter.AttrInt("id");
                        if (id >= 0 && emitter.AttrBool("noodleGenetics")) {
                            NoodleEmitterIDs.Add(new NoodleEmitter.NoodleEmitterDetails {
                                ID = $"{LevelName}:{id}",
                                Position = new Vector2(emitter.AttrFloat("x"), emitter.AttrFloat("y")) + LevelOffset,
                                NoodleLimit = emitter.AttrInt("noodleLimit"),
                            });
                        }
                    }
                }
            };
        }

        public override void Reset() {
            Checkpoint = 0;
            NoodleEmitterIDs = new();
        }
    }
}