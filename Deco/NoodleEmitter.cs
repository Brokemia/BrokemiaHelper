using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrokemiaHelper.Deco {
    [CustomEntity("BrokemiaHelper/noodleEmitter")]
    public class NoodleEmitter : Entity {
        public struct NoodleEmitterDetails {
            public string ID;
            public Vector2 Position;
            public int NoodleLimit;
        }

        // Maps the area to a list of noodle emitter IDs (using EntityID.Key) and positions
        public static readonly Dictionary<(int, AreaMode), List<NoodleEmitterDetails>> EmittersByLevel = new();
        
        private Sprite sprite;
        private readonly Vector2[] nodes;
        private float noodleTimer = 0;
        private Noodle.NoodleParams noodleParams;
        private bool noodleGenetics;
        private List<Noodle.NoodleParams> geneticNoodles => BrokemiaHelperModule.Session.Noodles.TryGetValue(id.Key, out var res) ? res : null;
        private List<Noodle.NoodleParams> geneticNoodlesAtHome = new();
        private int noodleCount;
        private int noodleLimit;
        private float minEmitTime;
        private float emitTimeRange;
        private bool waitingOnReturn;

        private int? randSeed;
        private Random rand;

        private EntityID id;
        private Level level;
        
        public NoodleEmitter(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
            this.id = id;
            Add(sprite = GFX.SpriteBank.Create(data.Attr("sprite", "")));
            Depth = data.Int("depth");
            nodes = data.NodesOffset(offset);
            randSeed = data.Int("seed");
            if (randSeed == 0) randSeed = null;
            noodleCount = data.Int("noodleCount", 4);
            noodleLimit = data.Int("noodleLimit", 8);
            minEmitTime = data.Float("minEmitTime", 1);
            emitTimeRange = data.Float("maxEmitTime", 6) - minEmitTime;

            noodleGenetics = data.Bool("noodleGenetics", false);
            noodleParams = GetNoodleParams(data);
        }

        public static Noodle.NoodleParams GetNoodleParams(EntityData data) {
            var res = new Noodle.NoodleParams() {
                Depth = data.Int("noodleDepth"),
                Length = data.Int("noodleLength"),
                FastSpeed = data.Float("noodleFastSpeed"),
                SlowSpeed = data.Float("noodleSlowSpeed"),
                Acceleration = data.Float("noodleAcceleration"),
                TailTaper = data.Float("noodleTailTaper"),
                WanderStrength = data.Float("noodleWanderStrength"),
                PlayerInterest = data.Float("noodlePlayerInterest"),
                FriendInterest = data.Float("noodleFriendInterest"),
                HomingDistSq = data.Float("noodleHomingDistance"),
                Homing = data.Float("noodleHoming"),
                JourneyMax = data.Float("noodleJourneyMax"),
                JourneyMin = data.Float("noodleJourneyMin"),
                FocusMax = data.Float("noodleFocusMax"),
                FocusIncrease = data.Float("noodleFocusIncrease"),
                SightDistanceSq = data.Float("noodleSightDistance"),
                Bloodthirst = data.Float("noodleBloodthirst"),
                ColorGradient = data.Bool("noodleColorGradient", true),
                HeadColor = Calc.HexToColor(data.Attr("noodleHeadColor", "800080")),
                TailColor = Calc.HexToColor(data.Attr("noodleTailColor", "9370DB"))
            };
            res.HomingDistSq *= res.HomingDistSq;
            res.SightDistanceSq *= res.SightDistanceSq;

            return res;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
            rand = new(randSeed ?? Calc.Random.Next());
            noodleTimer = minEmitTime + rand.NextFloat(emitTimeRange);

            // Add the initial noodles
            if (noodleGenetics) {
                var startingNoodles = geneticNoodles;
                if (startingNoodles == null) {
                    startingNoodles = new();
                    for (int i = 0; i < noodleCount; i++) {
                        startingNoodles.Add(noodleParams);
                    }
                    BrokemiaHelperModule.Session.Noodles[id.Key] = startingNoodles;
                }
                geneticNoodlesAtHome.AddRange(startingNoodles);
            }
        }

        private bool ContainsNoodles() {
            return noodleGenetics ? (geneticNoodlesAtHome?.Count ?? 0) > 0 : noodleCount > 0;
        }

        public override void Update() {
            base.Update();
            noodleTimer -= Engine.DeltaTime;
            
            if (noodleTimer < 0 && ContainsNoodles()) {
                EmitNoodle();
                if (ContainsNoodles()) {
                    noodleTimer = minEmitTime + rand.NextFloat(emitTimeRange);
                } else {
                    waitingOnReturn = true;
                }
            }
        }

        public void EmitNoodle() {
            noodleCount--;
            var noodle = noodleParams;
            var onReturn = OnNoodleReturn;
            if (noodleGenetics) {
                int noodleIdx = rand.Next(geneticNoodlesAtHome.Count);
                noodle = geneticNoodlesAtHome[noodleIdx];
                geneticNoodlesAtHome.RemoveAt(noodleIdx);

                var validTargets = EmittersByLevel[(level.Session.Area.ID, level.Session.Area.Mode)]
                    .Where(e => BrokemiaHelperModule.Session.Noodles.TryGetValue(e.ID, out var noodles) && noodles.Count < e.NoodleLimit)
                    .OrderBy(e => (e.Position - Position).LengthSquared());
                // TODO choose with odds based on distance (index in array or maybe actually proportional to lengthsquared)
                // Make emitters in the same room address nodes also, maybe with "master" emitter that holds list of others
                var target = validTargets.Take((int)(Ease.QuadIn(rand.NextFloat()) * validTargets.Count()) + 1).Last();
                onReturn = _ => {

                };
            }
            int startIndex = rand.Next(nodes.Length);
            sprite.Play($"emit{startIndex}");
            Scene.Add(Engine.Pooler.Create<Noodle>().Init(onReturn, nodes[startIndex], nodes[rand.Next(nodes.Length)], rand.Next(), noodle));
        }

        public void OnNoodleReturn(Noodle noodle) {
            noodleCount++;
            if (noodleGenetics) {
                geneticNoodlesAtHome.Add(noodle.Params);
            }
            if (waitingOnReturn && ContainsNoodles()) {
                noodleTimer = minEmitTime + rand.NextFloat(emitTimeRange);
                waitingOnReturn = false;
            }
        }
    }
}
