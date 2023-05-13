using Celeste;
using Monocle;

namespace BrokemiaHelper {
    public class ImagineAWorldWhereTextboxesDoNotHavePortraitsThisIsTheTwilightZone {
        public static void Load() {
            On.Celeste.Textbox.Update += Textbox_Update;
        }

        private static void Textbox_Update(On.Celeste.Textbox.orig_Update orig, Textbox self) {
            orig(self);
            if (self.index >= self.Nodes.Count)
                return;
            if (self.Nodes[self.index] is not FancyText.Portrait portrait)
                return;
            if (string.IsNullOrEmpty(portrait.Sprite))
                return;
            if (!GFX.PortraitsSpriteBank.Has(portrait.SpriteId))
                return;
            if (!GFX.PortraitsSpriteBank.SpriteData[portrait.SpriteId].Sources[0].XML.AttrBool("BrokemiaHelper_noPortrait", false))
                return;
            
            self.portrait = null;
            self.portraitExists = false;
        }

        public static void Unload() {
            On.Celeste.Textbox.Update -= Textbox_Update;
        }
    }
}
