using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Security.AccessControl;

namespace BrokemiaHelper.Deco {
    [CustomEntity("BrokemiaHelper/thornGrowers")]
    public class ThornGrowers : Entity {
        private static DynamicData easeData = new(typeof(Ease));

        private int width;
        private int height;

        private MTexture circle;

        private int randomSeed;

        private int minGrowers;
        private int growerCountRange;
        private bool clampGrowerPos;

        private float heightFractionMin;
        private float centerHeightFraction;
        private float centerHeightVariation;

        private float centerAngleRange;
        private float edgeAngleRange;
        private float angleFractionMin;
        private float angleFractionRange;

        private Color stalkBaseColor;
        private Color stalkTopColor;

        private float stalkWanderMax;
        private float stalkWanderAcceleration;

        private float stalkBaseThicknessMin;
        private float stalkBaseThicknessRange;
        private float stalkTopThicknessMin;
        private float stalkTopThicknessRange;
        private Ease.Easer stalkThicknessDistribution;

        private bool hasThorns;

        private Color baseThornColor;
        private Color topThornColor;
        
        private float thornExtent;
        private int thornMaxLength;
        private int thornMinLength;
        private int thornLengthRange;
        private int thornBaseDist;
        private int thornMinSpacing;
        private int thornSpacingRange;
        private float thornThickness;
        private float thornDirection;

        public ThornGrowers(EntityData data, Vector2 offset) : base(data.Position + offset) {
            width = data.Width;
            height = data.Height;

            randomSeed = data.Int("randomSeed", 0);
            Depth = data.Int("depth", Depths.BGDecals);

            minGrowers = data.Int("minGrowers", 6);
            growerCountRange = data.Int("maxGrowers", 10) - minGrowers;
            clampGrowerPos = data.Bool("clampGrowerPos", true);

            heightFractionMin = data.Float("heightFractionMin", 0.7f);
            centerHeightFraction = data.Float("centerHeightFraction", 0.25f);
            centerHeightVariation = data.Float("centerHeightVariation", 0.25f);

            centerAngleRange = data.Float("centerAngleRange", (float)Math.PI / 8f);
            edgeAngleRange = data.Float("edgeAngleRange", (float)Math.PI / 4f);
            angleFractionMin = data.Float("angleFractionMin", 0.2f);
            angleFractionRange = data.Float("angleFractionMax", 1f) - angleFractionMin;

            stalkBaseColor = Calc.HexToColor(data.Attr("stalkBaseColor", "1d5a10"));
            stalkTopColor = Calc.HexToColor(data.Attr("stalkTopColor", "2d8e19"));

            stalkWanderMax = data.Float("stalkWanderMax", (float)Math.PI / 16);
            stalkWanderAcceleration = data.Float("stalkWanderAcceleration", (float)Math.PI / 20);

            stalkBaseThicknessMin = data.Float("stalkBaseThicknessMin", 0.5f);
            stalkBaseThicknessRange = data.Float("stalkBaseThicknessMax", 1.2f) - stalkBaseThicknessMin;
            stalkTopThicknessMin = data.Float("stalkTopThicknessMin", 0.35f);
            stalkTopThicknessRange = data.Float("stalkTopThicknessMax", 0.5f) - stalkTopThicknessMin;
            stalkThicknessDistribution = easeData.Get<Ease.Easer>(data.Attr("stalkThicknessDistribution", "QuadIn"));

            hasThorns = data.Bool("hasThorns", true);

            baseThornColor = Calc.HexToColor(data.Attr("baseThornColor", "249c7e"));
            topThornColor = Calc.HexToColor(data.Attr("topThornColor", "249c7e"));

            thornExtent = data.Float("thornExtent", 4);
            thornMaxLength = data.Int("thornMaxLength", 8);
            thornMinLength = data.Int("thornMinLength", 3);
            thornLengthRange = thornMaxLength - thornMinLength;
            thornBaseDist = data.Int("thornBaseDist", 10);
            thornMinSpacing = data.Int("thornMinSpacing", 3);
            thornSpacingRange = data.Int("thornMaxSpacing", 14) - thornMinSpacing;
            thornThickness = data.Float("thornThickness", 0.3f);
            thornDirection = data.Float("thornDirection", (float)Math.PI / 2);

            circle = GFX.Game["particles/circle"];
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            randomSeed = randomSeed == 0 ? Calc.Random.Next() : randomSeed;
        }

        public override void Render() {
            base.Render();

            var rand = new Random(randomSeed);
            var meanPos = width / 2f;
            // There should be about a 98% chance for a grower to be inside the actual rectangle, with the outside ones clamped to the edges
            var posStdDev = width / 6f;
            var growers = minGrowers + rand.Next(growerCountRange);

            for (int i = 0; i < growers; i++) {
                var growerX = clampGrowerPos ? rand.NextGaussianClamped(meanPos, posStdDev, 0, width) : rand.NextGaussian(meanPos, posStdDev);
                var edgeness = Math.Abs(growerX - meanPos) / width * 2;
                // Curve away from the center at random angles
                var growerEndAngle = Calc.Up + (growerX > meanPos ? 1 : -1) * (angleFractionMin + rand.NextFloat(angleFractionRange)) * Calc.LerpClamp(centerAngleRange, edgeAngleRange, edgeness);
                var growerHeight = (int)(height * ((rand.NextFloat(centerHeightVariation) + centerHeightFraction) * (1 - edgeness) + heightFractionMin));
                RenderVine(rand, Position + new Vector2(growerX, height), growerEndAngle, growerHeight);
            }
        }

        public void RenderVine(Random rand, Vector2 pos, float endAngle, int height) {
            var offsetDir = 0f;

            var stalkBaseThickPercentile = stalkThicknessDistribution(rand.NextFloat());
            var stalkBaseThickness = stalkBaseThicknessMin + stalkBaseThickPercentile * stalkBaseThicknessRange;
            var stalkTopThickPercentile = stalkThicknessDistribution(rand.NextFloat());
            var stalkTopThickness = stalkTopThicknessMin + stalkTopThickPercentile * stalkTopThicknessRange;

            var nextThornLeft = rand.Next(thornMaxLength) + thornBaseDist;
            var nextThornRight = rand.Next(thornMaxLength) + thornBaseDist;

            for (int i = 0; i < height; i++) {
                var percent = i / (float)height;
                circle.DrawCentered(pos, Color.Lerp(stalkBaseColor, stalkTopColor, percent), Calc.LerpClamp(stalkBaseThickness, stalkTopThickness, percent));

                var dir = Calc.AngleToVector(Calc.LerpClamp(Calc.Up, endAngle, i / (float)height) + offsetDir, 1);

                offsetDir = Calc.Clamp(offsetDir + (rand.NextFloat() - 0.5f) * stalkWanderAcceleration, -stalkWanderMax, stalkWanderMax);


                var thornColor = Color.Lerp(baseThornColor, topThornColor, percent);
                if (nextThornLeft < 0) {
                    nextThornLeft = rand.Next(thornLengthRange) + thornMinLength;
                    RenderThorn(pos, dir, true, thornExtent, nextThornLeft, thornColor);
                    nextThornLeft += rand.Next(thornSpacingRange) + thornMinSpacing;
                }
                if (nextThornRight < 0) {
                    nextThornRight = rand.Next(thornLengthRange) + thornMinLength;
                    RenderThorn(pos, dir, false, thornExtent, nextThornRight, thornColor);
                    nextThornRight += rand.Next(thornSpacingRange) + thornMinSpacing;
                }
                if (height - i > thornMaxLength / 2) {
                    nextThornLeft--;
                    nextThornRight--;
                }
                
                pos += dir;
            }
        }

        private void RenderThorn(Vector2 start, Vector2 direction, bool left, float extent, float size, Color color) {
            if (!hasThorns) return;
            
            var stepAmount = 0.5f;
            var pos = start;
            var outDir = direction.Rotate((left ? -1f : 1f) * thornDirection);
            for (float i = 0; i < extent; i += stepAmount) {
                pos += outDir * stepAmount;

                circle.DrawCentered(pos, color, thornThickness);
            }

            for (float i = 0; i < size; i += stepAmount) {
                pos += direction * stepAmount;

                circle.DrawCentered(pos, color, thornThickness);
            }
        }
    }
}
