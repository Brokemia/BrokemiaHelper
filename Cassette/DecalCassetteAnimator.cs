using Celeste;
using Monocle;
using MonoMod.Utils;
using System;

namespace BrokemiaHelper {
    public class DecalCassetteAnimator : Component {

        private Decal Decal => (Decal)Entity;
        private CassetteBlockManager manager;
        private int[] beatFrames;
        private bool firstUpdate = true;

        public DecalCassetteAnimator(int[] frames) : base(true, false) {
            beatFrames = frames;
        }

        public override void Update() {
            if(firstUpdate) {
                if (Scene.Tracker.GetEntity<CassetteBlockManager>() is CassetteBlockManager manager) {
                    this.manager = manager;
                }
                firstUpdate = false;
            }
            base.Update();
            if(manager != null) {
                if(((int)Decal.frame) != beatFrames[manager.currentIndex]) {
                    Decal.animated = true;
                } else {
                    Decal.animated = false;
                    Decal.frame = beatFrames[manager.currentIndex];
                }
            } else {
                Decal.animated = false;
            }
        }

    }
}
