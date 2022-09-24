using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokemiaHelper {
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
    }
}
