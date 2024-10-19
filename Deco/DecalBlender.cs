using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BrokemiaHelper.Deco {
    [CustomEntity("BrokemiaHelper/decalBlender")]
    public class DecalBlender : Entity {
        private EntityID id;
        private Level level;
        private VirtualRenderTarget renderTarget;
        private Regex decalNameFilter;
        private List<Decal> decals;
        private BlendState blendState;
        // TODO this entity can be optimized in memory by limiting render target size to camera size, and then doing some offsets so it looks right
        public DecalBlender(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
            this.id = id;
            Collider = new Hitbox(data.Width, data.Height);
            Depth = data.Int("depth", Depths.BGDecals);
            var decalNameFilterStr = data.Attr("decalNameFilter", null);
            if (!string.IsNullOrWhiteSpace(decalNameFilterStr)) {
                decalNameFilter = new Regex(decalNameFilterStr, RegexOptions.Compiled);
            }
            blendState = new BlendState();
            blendState.AlphaBlendFunction = data.Enum("alphaBlendFunction", BlendFunction.Max);
            blendState.AlphaDestinationBlend = data.Enum("alphaDestinationBlend", Blend.One);
            blendState.AlphaSourceBlend = data.Enum("alphaSourceBlend", Blend.One);
            blendState.ColorBlendFunction = data.Enum("colorBlendFunction", BlendFunction.Max);
            blendState.ColorDestinationBlend = data.Enum("colorDestinationBlend", Blend.One);
            blendState.ColorSourceBlend = data.Enum("colorSourceBlend", Blend.One);
            // TODO support alpha?
            blendState.BlendFactor = data.HexColor("blendFactor", Color.White);

            Add(new BeforeRenderHook(BeforeRender));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            level = SceneAs<Level>();
            renderTarget = VirtualContent.CreateRenderTarget("decalRenderer", (int)Width, (int)Height);

            // Get all decals with positions inside the blender
            decals = scene.Entities.FindAll<Decal>()
                .Where(d => Collide.CheckPoint(this, d.Position) && (decalNameFilter?.IsMatch(d.Name) ?? true))
                .ToList();
            foreach (var decal in decals) {
                decal.Visible = false;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            renderTarget?.Dispose();
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            renderTarget?.Dispose();
        }

        private void BeforeRender() {
            Draw.SpriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
            Draw.SpriteBatch.GraphicsDevice.Clear(Color.Transparent);
            
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, DepthStencilState.Default,
                RasterizerState.CullNone, null, Matrix.CreateTranslation(new Vector3(-Position, 0)));
            foreach (var decal in decals) {
                decal.Render();
            }
            Draw.SpriteBatch.End();
        }

        public override void Render() {
            base.Render();
            Draw.SpriteBatch.Draw(renderTarget, Position, Color.White);
        }

    }
}
