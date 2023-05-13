using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace BrokemiaHelper.Deco {
    [CustomEntity("BrokemiaHelper/noodleEmitter")]
    public class NoodleEmitter : Entity {
        private Sprite sprite;
        private readonly Vector2[] nodes;
        private float noodleTimer = 0;
        private Noodle.NoodleParams noodleParams;

        private int? randSeed;
        private Random rand;
        
        public NoodleEmitter(EntityData data, Vector2 offset) : base(data.Position + offset) {
            //Add(sprite = GFX.SpriteBank.Create(data.Attr("sprite", " ")));
            nodes = data.NodesOffset(offset);
            randSeed = data.Int("seed");
            if (randSeed == 0) randSeed = null;

            noodleParams = new() {
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
                Bloodthirst = data.Float("noodleBloodthirst")
            };
            noodleParams.HomingDistSq *= noodleParams.HomingDistSq;
            noodleParams.SightDistanceSq *= noodleParams.SightDistanceSq;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            rand = new(randSeed ?? Calc.Random.Next());
            noodleTimer = 1 + rand.NextFloat(5);
        }

        public override void Update() {
            base.Update();
            noodleTimer -= Engine.DeltaTime;
            if(noodleTimer < 0) {
                noodleTimer = 1 + rand.NextFloat(5);
                Scene.Add(Engine.Pooler.Create<Noodle>().Init(nodes[rand.Next(nodes.Length)], nodes[rand.Next(nodes.Length)], rand.Next(), noodleParams));
            }
        }
    }
}
