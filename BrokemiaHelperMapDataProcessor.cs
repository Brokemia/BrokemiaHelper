using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;

namespace BrokemiaHelper {
    public class BrokemiaHelperMapDataProcessor : EverestMapDataProcessor {
        public int Checkpoint;

        public override void End() {
        }

        public override Dictionary<string, Action<BinaryPacker.Element>> Init() {
            return new Dictionary<string, Action<BinaryPacker.Element>> {
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
                }
            };
        }

        public override void Reset() {
            Checkpoint = 0;
        }
    }
}