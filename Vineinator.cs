using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Linq;

// TODO add ability to do custom "features" (like flowers) that are images and can be placed with a given frequency and option for random flipping/rotation/color/etc

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/vineinator")]
    public class Vineinator : Entity {
        private readonly int chunkSize = 128;
        private VirtualMap<ImageData> chunks;
        private VirtualMap<Texture2D> textureChunks;

        private Level level;
        private Vector2 levelOffset;
        private EntityID id;
        private string seed;
        private Random rand;

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

        public Vineinator(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
            waypoints = data.NodesWithPosition(new());
            this.id = id;
            levelOffset = offset;
            seed = data.Attr("seed", "");
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
            highlightColor = Calc.HexToColor(data.Attr("highlightColor", "1a3236"));
            bodyColor = Calc.HexToColor(data.Attr("bodyColor", "171f28"));
            shadowColor = Calc.HexToColor(data.Attr("shadowColor", "040815"));
            thornColor = Calc.HexToColor(data.Attr("thornColor", "1d5455"));
            hangingVineColor = Calc.HexToColor(data.Attr("hangingVineColor", "040815"));
            Depth = data.Int("depth", Depths.BGDecals);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            level = SceneAs<Level>();
            chunks = new((int)Math.Ceiling(level.Bounds.Width / (float)chunkSize) + 2, (int)Math.Ceiling(level.Bounds.Width / (float)chunkSize) + 2);

            DrawVines();

            textureChunks = new(chunks.Columns, chunks.Rows);
            for(int i = 0; i < chunks.Columns; i++) {
                for(int j = 0; j < chunks.Rows; j++) {
                    if(chunks[i, j] is { } chunk) {
                        var tex = new Texture2D(Engine.Graphics.GraphicsDevice, chunkSize, chunkSize);
                        tex.SetData(chunk.GetData().Cast<Color>().ToArray());
                        textureChunks[i, j] = tex;
                    }
                } 
            }
        }

        public override void Render() {
            base.Render();
            if (textureChunks == null) return;
            for (int i = 0; i < textureChunks.Columns; i++) {
                for (int j = 0; j < textureChunks.Rows; j++) {
                    if (textureChunks[i, j] is { } tex) {
                        Draw.SpriteBatch.Draw(tex, new Vector2((i - 1) * chunkSize, (j - 1) * chunkSize) + levelOffset, Color.White);
                    }
                }
            }
        }

        private void SetPixel(int x, int y, Color color) {
            x += chunkSize;
            y += chunkSize;
            int chunkX = (int)Math.Floor(x / (float)chunkSize);
            int chunkY = (int)Math.Floor(y / (float)chunkSize);
            if (chunkX < 0 || chunkX >= chunks.Columns || chunkY < 0 || chunkY >= chunks.Columns) return;
            if(chunks[chunkX, chunkY] == null) {
                chunks[chunkX, chunkY] = new ImageData(chunkSize, chunkSize);
            }
            chunks[chunkX, chunkY][x % chunkSize, y % chunkSize] = color;
        }

        private void DrawCircle(int x, int y, float radius, Color color) {
            int pxlRadius = (int)Math.Ceiling(radius);
            float radSq = radius * radius;
            for(int i = -pxlRadius; i <= pxlRadius; i++) {
                for (int j = -pxlRadius; j <= +pxlRadius; j++) {
                    if(i * i + j * j < radSq) {
                        SetPixel(x + i, y + j, color);
                    }
                }
            }
        }

        private void DrawSquare(int x, int y, int width, Color color) {
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < width; j++) {
                    SetPixel(x + i, y + j, color);
                }
            }
        }

        // Draws a quadratic bezier curve
        // Might be kind of expensive, we'll see
        private void DrawArc(int sx, int sy, int dx, int dy, int cx, int cy, Color color) {
            SimpleCurve curve = new(new(sx, sy), new(dx, dy), new(cx, cy));
            int LUTRes = 30;

            float[] LUT = new float[LUTRes + 1];
            LUT[0] = 0;
            Vector2 last = curve.Begin;
            float length = 0f;
            for (int i = 1; i <= LUTRes; i++) {
                Vector2 point = curve.GetPoint(i / (float)LUTRes);
                length += (point - last).Length();
                LUT[i] = length;
                last = point;
            }

            for (float d = 0; d - 1 < LUT[LUT.Length - 1]; d += 0.5f) {
                float t = BezierDistToT(LUT, d);
                Vector2 pt = curve.GetPoint(t);
                SetPixel((int)pt.X, (int)pt.Y, color);
            }
        }

        // https://youtu.be/aVwxzDHniEw?t=1099
        private float BezierDistToT(float[] LUT, float dist) {
            float len = LUT[LUT.Length - 1];
            if(dist < 0) {
                return 0;
            }
            if(dist >= len) {
                return 1;
            }

            for(int i = 0; i < LUT.Length - 1; i++) {
                if(dist >= LUT[i] && dist < LUT[i + 1]) {
                    float lastT = i / (float)(LUT.Length - 1);
                    float nextT = (i + 1) / (float)(LUT.Length - 1);
                    return lastT + (nextT - lastT) * (dist - LUT[i]) / (LUT[i + 1] - LUT[i]);
                }
            }
            throw new ArgumentException("Something is wrong with the lookup table provided");
        }

        private void DrawVines() {
            Random featureRand = new(GetSeed() + 1);

            Point? vineStart = null;
            DrawPass((x, y, size, dir) => {
                DrawCircle(x, y + 1, size - .5f, shadowColor);
                if (hangingVines) {
                    if (featureRand.NextFloat() > hangingVineFrequency) return;
                    if (vineStart.HasValue) {
                        Point control = new Point((vineStart.Value.X + x) / 2, Math.Max(vineStart.Value.Y, y) + hangingVineSlack);
                        DrawArc(vineStart.Value.X, vineStart.Value.Y, x, y, control.X, control.Y, hangingVineColor);
                        vineStart = null;
                    } else {
                        vineStart = new(x, y);
                    }
                }
            });

            DrawPass((x, y, size, dir) => {
                DrawCircle(x, y, size - .5f, bodyColor);
            });

            float sizeMult = (minHighlightProportion + maxHighlightProportion) / 2;
            DrawPass((x, y, size, dir) => {
                Vector2 sunward = dir.Rotate((float)Math.PI / 2);
                if(sunward.Y > 0) {
                    sunward *= -1;
                }
                int highlightOffset = (int)Math.Ceiling(size - size * sizeMult);
                DrawCircle(x + (int)(sunward.X * highlightOffset), y + (int)(sunward.Y * highlightOffset), size * sizeMult , highlightColor);

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
                    for(int thornProgress = 0; thornSize > 0; thornSize -= thornShrinkSpeed, thornProgress++) {
                        DrawSquare((int)(x + thornDir.X * (size - .3f + thornProgress) - thornSize / 2), (int)(y + thornDir.Y * (size - .3f + thornProgress) - thornSize / 2), (int)Math.Ceiling(thornSize), thornColor);
                    }
                });
            }
        }

        /// <summary>
        /// Meander through the vine path, calling drawAction at each pixel
        /// </summary>
        /// <param name="drawAction">Takes x, y, size, and direction of travel</param>
        private void DrawPass(Action<int, int, float, Vector2> drawAction) {
            rand = new(GetSeed());
            int target = 1;
            Vector2 pos = waypoints[0];
            Vector2 direction = waypoints[target] - waypoints[target - 1];
            direction.Normalize();
            float size = (minSize + maxSize) / 2;

            while (target < waypoints.Length) {
                int iterations = 0;
                while ((pos - waypoints[target]).LengthSquared() > 2) {
                    drawAction((int)pos.X, (int)pos.Y, size, direction);
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
                    if(rand.Next(2) == 0) {
                        size = Math.Min(maxSize, size + sizeSpeed);
                    } else {
                        size = Math.Max(minSize, size - sizeSpeed);
                    }

                    // Safety to avoid infinite loop
                    iterations++;
                    if (iterations >= maxLength) return;
                }
                // Choose next target
                target++;
            }
        }

        private int GetSeed() {
            return string.IsNullOrWhiteSpace(seed) ? (SceneAs<Level>().Session.Level + "_vine").GetHashCode() + id.ID : seed.GetHashCode();
        }
    }
}
