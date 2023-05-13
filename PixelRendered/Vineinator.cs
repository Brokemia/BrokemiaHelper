using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

// TODO add ability to do custom "features" (like flowers) that are images and can be placed with a given frequency and option for random flipping/rotation/color/etc

namespace BrokemiaHelper.PixelRendered {
    [CustomEntity("BrokemiaHelper/vineinator")]
    public class Vineinator : Entity {
        private PixelComponent pixelComponent;

        private EntityID id;
        private string seedString;
        private int seed;
        private Random rand;
        private bool legacyRNG;

        private Vector2[] waypoints;
        private Color highlightColor;
        private Color bodyColor;
        private Color shadowColor;
        private Color thornColor;
        private Color hangingVineColor;

        private int maxLength;

        // Size is effectively used as radius
        private float minSize;
        private float maxSize;
        private float sizeSpeed;

        private float minHighlightProportion;
        private float maxHighlightProportion;
        private float highlightProportionSpeed;

        private float wanderStrength;
        private float focusStrength;
        private float homing;
        private float homingDistSq;

        private bool thorns;
        private float thornFrequency;
        private float minThornSize;
        private float maxThornSize;
        private float thornShrinkSpeed;

        private bool hangingVines;
        private float hangingVineFrequency;
        private int hangingVineSlack;

        private bool wiggles;
        private bool animateWiggles;
        private float wiggleAmount;
        private float wiggleFrequency;
        private float wiggleSpeed;

        private bool animateSnake;
        private float snakeSpeed;
        private int snakeLength;

        public Vineinator(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
            waypoints = data.NodesWithPosition(new());
            this.id = id;
            seedString = data.Attr("seed", "");
            minSize = data.Float("minSize", 3) / 2;
            maxSize = data.Float("maxSize", 8) / 2;
            sizeSpeed = data.Float("sizeSpeed", 0.2f) / 2;
            minHighlightProportion = data.Float("minHighlightProportion", 0.1f);
            maxHighlightProportion = data.Float("maxHighlightProportion", 0.6f);
            highlightProportionSpeed = data.Float("highlightProportionSpeed", 0.2f);
            maxLength = data.Int("maxLength", 100000);
            wanderStrength = data.Float("wanderStrength", 0.4f);
            focusStrength = data.Float("focusStrength", 0.2f);
            homing = data.Float("homing", 0.25f);
            homingDistSq = data.Float("homingDistance", 6);
            homingDistSq *= homingDistSq;
            thorns = data.Bool("thorns", true);
            thornFrequency = data.Float("thornFrequency", 0.12f);
            minThornSize = data.Float("minThornSize", 1);
            maxThornSize = data.Float("maxThornSize", 2f);
            thornShrinkSpeed = data.Float("thornShrinkSpeed", 0.5f);
            hangingVines = data.Bool("hangingVines", true);
            hangingVineFrequency = data.Float("hangingVineFrequency", 0.02f);
            hangingVineSlack = data.Int("hangingVineSlack", 30);
            wiggles = data.Bool("wiggles", false);
            animateWiggles = data.Bool("animateWiggles", true);
            wiggleAmount = data.Float("wiggleAmount", 2.0f);
            wiggleFrequency = data.Float("wiggleFrequency", 0.125f);
            wiggleSpeed = data.Float("wiggleSpeed", 1.0f);
            animateSnake = data.Bool("animateSnake", false);
            snakeSpeed = data.Float("snakeSpeed", 20f);
            snakeLength = data.Int("snakeLength", 50);
            highlightColor = Calc.HexToColor(data.Attr("highlightColor", "1a3236"));
            bodyColor = Calc.HexToColor(data.Attr("bodyColor", "171f28"));
            shadowColor = Calc.HexToColor(data.Attr("shadowColor", "040815"));
            thornColor = Calc.HexToColor(data.Attr("thornColor", "1d5455"));
            hangingVineColor = Calc.HexToColor(data.Attr("hangingVineColor", "040815"));
            Depth = data.Int("depth", Depths.BGDecals);
            legacyRNG = data.Bool("legacyRNG", false);

            Add(pixelComponent = new PixelComponent());
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            seed = GetSeed();
            DrawVines();
            pixelComponent.CommitChunks();
        }

        public override void Render() {
            base.Render();
            if ((wiggles && animateWiggles) || animateSnake) {
                pixelComponent.ClearChunks();
                DrawVines();
                pixelComponent.CommitChunks();
            }
        }

        private void DrawVines() {
            // Measure length of the vine
            if (vineLength < 0) {
                bool oldSnake = animateSnake;
                animateSnake = false;
                vineLength = 0;
                DrawPass((x, y, size, dir) => vineLength++);
                animateSnake = oldSnake;
            }

            Random featureRand = new(seed + 1);

            Point? vineStart = null;
            DrawPass((x, y, size, dir) => {
                pixelComponent.DrawCircle(x, y + 1, size - .5f, shadowColor);
                if (hangingVines) {
                    if (featureRand.NextFloat() > hangingVineFrequency) return;
                    if (vineStart.HasValue) {
                        Point control = new((vineStart.Value.X + x) / 2, Math.Max(vineStart.Value.Y, y) + hangingVineSlack);
                        pixelComponent.DrawArc(vineStart.Value.X, vineStart.Value.Y, x, y, control.X, control.Y, hangingVineColor);
                        vineStart = null;
                    } else {
                        vineStart = new(x, y);
                    }
                }
            });

            DrawPass((x, y, size, dir) => {
                pixelComponent.DrawCircle(x, y, size - .5f, bodyColor);
            });

            float sizeMult = (minHighlightProportion + maxHighlightProportion) / 2;
            DrawPass((x, y, size, dir) => {
                Vector2 sunward = dir.Rotate((float)Math.PI / 2);
                if (sunward.Y > 0) {
                    sunward *= -1;
                }
                int highlightOffset = (int)Math.Ceiling(size - size * sizeMult);
                pixelComponent.DrawCircle(x + (int)(sunward.X * highlightOffset), y + (int)(sunward.Y * highlightOffset), size * sizeMult, highlightColor);

                // Vary size;
                if (featureRand.Next(2) == 0) {
                    sizeMult = Math.Min(maxHighlightProportion, sizeMult + highlightProportionSpeed);
                } else {
                    sizeMult = Math.Max(minHighlightProportion, sizeMult - highlightProportionSpeed);
                }
            });

            if (thorns) {
                DrawPass((x, y, size, dir) => {
                    if (featureRand.NextFloat() > thornFrequency) return;
                    float thornSize = minThornSize + featureRand.NextFloat(maxThornSize - minThornSize);
                    // Rotated randomly up to 45 degrees off a tangent
                    Vector2 thornDir = dir.Rotate((featureRand.Next(2) == 0 ? -1 : 1) * (float)Math.PI / 2 + featureRand.NextAngle() / 8);
                    for (int thornProgress = 0; thornSize > 0; thornSize -= thornShrinkSpeed, thornProgress++) {
                        pixelComponent.DrawSquare((int)(x + thornDir.X * (size - .3f + thornProgress) - thornSize / 2), (int)(y + thornDir.Y * (size - .3f + thornProgress) - thornSize / 2), (int)Math.Ceiling(thornSize), thornColor);
                    }
                });
            }
        }

        private int vineLength = -1;

        /// <summary>
        /// Meander through the vine path, calling drawAction at each pixel
        /// </summary>
        /// <param name="drawAction">Takes x, y, size, and direction of travel</param>
        private void DrawPass(Action<int, int, float, Vector2> drawAction) {
            rand = new(seed);
            float time = SceneAs<Level>().TimeActive;
            float wiggleOffset = rand.NextAngle();
            int snakeStart = rand.Next(vineLength < 0 ? 0 : vineLength) + (int)(time * snakeSpeed);
            snakeStart %= vineLength == 0 ? 1 : vineLength;
            int snakeEnd = snakeStart + snakeLength;
            int target = 1;
            Vector2 pos = waypoints[0];
            Vector2 direction = waypoints[target] - waypoints[target - 1];
            direction.Normalize();
            float size = (minSize + maxSize) / 2;
            int progress = 0;

            while (target < waypoints.Length) {
                int iterations = 0;
                while ((pos - waypoints[target]).LengthSquared() > 2) {
                    if (!animateSnake || (progress >= snakeStart && progress < snakeEnd)) {
                        Vector2 wiggle = wiggles ? direction.Rotate((float)Math.PI / 2) * (float)Math.Sin(progress * wiggleFrequency + time * wiggleSpeed + wiggleOffset) * wiggleAmount : default;
                        drawAction((int)(pos.X + wiggle.X), (int)(pos.Y + wiggle.Y), size, direction);
                    }
                    pos += direction;

                    // Wander away
                    direction += Calc.AngleToVector(rand.NextAngle(), wanderStrength);
                    direction.Normalize();

                    Vector2 targetDir = waypoints[target] - pos;
                    // Steer back
                    direction += targetDir.SafeNormalize() * focusStrength;
                    // Steer back strongly when near target
                    if (homingDistSq >= targetDir.LengthSquared()) {
                        direction += targetDir.SafeNormalize() * (homingDistSq - targetDir.LengthSquared()) / homingDistSq * homing;
                    }
                    direction.Normalize();

                    // Vary size;
                    if (rand.Next(2) == 0) {
                        size = Math.Min(maxSize, size + sizeSpeed);
                    } else {
                        size = Math.Max(minSize, size - sizeSpeed);
                    }

                    progress++;
                    // Safety to avoid infinite loop
                    iterations++;
                    if (iterations >= maxLength) return;
                }
                // Choose next target
                target++;
            }
        }

        private int GetSeed() {
            if(legacyRNG) {
                return string.IsNullOrWhiteSpace(seedString) ? (SceneAs<Level>().Session.Level + "_vine").WindowsHashCode() + id.ID : seedString.WindowsHashCode();
            }
            return string.IsNullOrWhiteSpace(seedString) ? Calc.Random.Next() :
                seedString.SimpleHash();
        }
    }
}
