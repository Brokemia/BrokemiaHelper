using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Linq;

namespace BrokemiaHelper.PixelRendered {
    public class PixelComponent : Component {
        private readonly int chunkSize = 128;
        private VirtualMap<ImageData> chunks;
        private VirtualMap<bool> dirtyChunks;
        private VirtualMap<Texture2D> textureChunks;

        private Level level;
        private Vector2 levelOffset;

        public PixelComponent() : base(false, true) {

        }

        public override void EntityAdded(Scene scene) {
            base.EntityAdded(scene);
            level = SceneAs<Level>();
            levelOffset = level.Bounds.Location.ToVector2();
            chunks = new((int)Math.Ceiling(level.Bounds.Width / (float)chunkSize) + 2, (int)Math.Ceiling(level.Bounds.Height / (float)chunkSize) + 2);
            dirtyChunks = new(chunks.Columns, chunks.Rows);
        }

        public void CommitChunks() {
            if (textureChunks == null) {
                textureChunks = new(chunks.Columns, chunks.Rows);
            }

            for (int i = 0; i < chunks.Columns; i++) {
                for (int j = 0; j < chunks.Rows; j++) {
                    if (!dirtyChunks[i, j]) continue;
                    if (chunks[i, j] is { } chunk) {
                        if (textureChunks[i, j] is not { } tex) {
                            tex = new Texture2D(Engine.Graphics.GraphicsDevice, chunkSize, chunkSize);
                            textureChunks[i, j] = tex;
                        }
                        tex.SetData(chunk.GetData());
                    }
                    dirtyChunks[i, j] = false;
                }
            }
        }

        public void ClearChunks() {
            if (chunks == null) return;

            for (int i = 0; i < chunks.Columns; i++) {
                for (int j = 0; j < chunks.Rows; j++) {
                    if (chunks[i, j] is { } chunk) {
                        chunk.ClearData();
                    }
                    dirtyChunks[i, j] = true;
                }
            }
        }

        public override void DebugRender(Camera camera) {
            base.DebugRender(camera);
            for (int i = 0; i < textureChunks.Columns; i++) {
                for (int j = 0; j < textureChunks.Rows; j++) {
                    if (textureChunks[i, j] is { } tex) {
                        Draw.HollowRect(new Vector2((i - 1) * chunkSize, (j - 1) * chunkSize) + levelOffset, chunkSize, chunkSize, new Color(0, 0.3f, 0, 0.1f));
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

        public void SetPixel(int x, int y, Color color) {
            x += chunkSize;
            y += chunkSize;
            int chunkX = (int)Math.Floor(x / (float)chunkSize);
            int chunkY = (int)Math.Floor(y / (float)chunkSize);
            if (chunkX < 0 || chunkX >= chunks.Columns || chunkY < 0 || chunkY >= chunks.Rows) return;
            if (chunks[chunkX, chunkY] == null) {
                chunks[chunkX, chunkY] = new ImageData(chunkSize, chunkSize);
            }
            chunks[chunkX, chunkY][x % chunkSize, y % chunkSize] = color;
            dirtyChunks[chunkX, chunkY] = true;
        }

        public void DrawCircle(int x, int y, float radius, Color color) {
            int pxlRadius = (int)Math.Ceiling(radius);
            float radSq = radius * radius;
            for (int i = -pxlRadius; i <= pxlRadius; i++) {
                for (int j = -pxlRadius; j <= +pxlRadius; j++) {
                    if (i * i + j * j < radSq) {
                        SetPixel(x + i, y + j, color);
                    }
                }
            }
        }

        public void DrawSquare(int x, int y, int width, Color color) {
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < width; j++) {
                    SetPixel(x + i, y + j, color);
                }
            }
        }

        // Draws a quadratic bezier curve
        // Might be kind of expensive, we'll see
        public void DrawArc(int sx, int sy, int dx, int dy, int cx, int cy, Color color, int LUTRes = 30) {
            SimpleCurve curve = new(new(sx, sy), new(dx, dy), new(cx, cy));

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
            if (dist < 0) {
                return 0;
            }
            if (dist >= len) {
                return 1;
            }

            for (int i = 0; i < LUT.Length - 1; i++) {
                if (dist >= LUT[i] && dist < LUT[i + 1]) {
                    float lastT = i / (float)(LUT.Length - 1);
                    float nextT = (i + 1) / (float)(LUT.Length - 1);
                    return lastT + (nextT - lastT) * (dist - LUT[i]) / (LUT[i + 1] - LUT[i]);
                }
            }
            throw new ArgumentException("Something is wrong with the lookup table provided");
        }

        public override void EntityRemoved(Scene scene) {
            base.EntityRemoved(scene);
            for (int i = 0; i < textureChunks.Columns; i++) {
                for (int j = 0; j < textureChunks.Rows; j++) {
                    textureChunks[i, j]?.Dispose();
                    textureChunks[i, j] = null;
                }
            }
        }
    }
}
