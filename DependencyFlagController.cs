using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/dependencyFlagController")]
    public class DependencyFlagController : Trigger {
        public EverestModuleMetadata dependency;
        public string flag;
        
        public DependencyFlagController(EntityData data, Vector2 offset) : base(data, offset) {
            dependency = new EverestModuleMetadata { Name = data.Attr("name"), VersionString = data.Attr("version") };
            flag = data.Attr("flag");
            if (string.IsNullOrWhiteSpace(flag)) {
                flag = "Dependency_" + dependency.Name + "_" + dependency.VersionString;
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (Everest.Loader.DependencyLoaded(dependency)) {
                SceneAs<Level>().Session.SetFlag(flag, true);
            } else {
                SceneAs<Level>().Session.SetFlag(flag, false);
            }
            RemoveSelf();
        }
    }
}
