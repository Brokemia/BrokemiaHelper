using Microsoft.Xna.Framework;

namespace BrokemiaHelper.PixelRendered {
    /// <summary>
    /// This is poorly named. I just use this to edit Color data and then create a Texture2D out of it.
    /// </summary>
    public class ImageData {
        private int width, height;
        private Color[] data;

        public Color this[int x, int y] {
            get => data[y * width + x];
            set {
                data[y * width + x] = value;
            }
        }

        public ImageData(int width, int height) {
            this.width = width;
            this.height = height;
            data = new Color[width * height];
        }

        public Color[] GetData() {
            return data;
        }

        public void ClearData() {
            for(int i = 0; i < data.Length; i++) {
                data[i] = default;
            }
        }
    }
}
