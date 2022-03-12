using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokemiaHelper {
    [CustomEntity("brokemiahelper/shadowCrushBlock")]
    class ShadowCrushBlock : CrushBlock {

        public ShadowCrushBlock(EntityData data, Vector2 offset) : base(data, offset) {

        }

    }
}
