using Celeste;
using Monocle;
using MonoMod.Utils;
using System;

namespace BrokemiaHelper {
    public class DecalCassetteAnimator : Component {

        private Decal Decal => (Decal)Entity;
        private DynamicData decalData, cassetteManagerData;
        private int[] beatFrames;
        private bool firstUpdate = true;

        public DecalCassetteAnimator(int[] frames) : base(true, false) {
            beatFrames = frames;
        }

        public override void Added(Entity entity) {
            base.Added(entity);
            decalData = new(Entity);
        }

        public override void Update() {
            if(firstUpdate) {
                if (Scene.Tracker.GetEntity<CassetteBlockManager>() is CassetteBlockManager manager) {
                    cassetteManagerData = new(manager);
                }
                firstUpdate = false;
            }
            base.Update();
            if(cassetteManagerData != null) {
                int index = cassetteManagerData.Get<int>("currentIndex");
                if(((int)decalData.Get<float>("frame")) != beatFrames[index]) {
                    decalData.Set("animated", true);
                } else {
                    decalData.Set("animated", false);
                    decalData.Set("frame", (float)beatFrames[index]);
                }
            } else {
                decalData.Set("animated", false);
            }
        }

    }
}
