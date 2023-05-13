//#define CARCINIZATION

#if CARCINIZATION

using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BrokemiaHelper {
    public class Carcinization : Entity {
        public static void Load() {
            Everest.Events.MainMenu.OnCreateButtons += OuiMainMenu_OnCreateButtons;
        }

        public static void Unload() {
            Everest.Events.MainMenu.OnCreateButtons -= OuiMainMenu_OnCreateButtons;
        }

        private static void OuiMainMenu_OnCreateButtons(OuiMainMenu menu, List<MenuButton> buttons) {
            if (!Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "CarcinizationHelper", Version = new(1, 0) })
                && DateTime.Now.Month == 4 && DateTime.Now.Day == 1) {
                Dialog.FallbackLanguage.Cleaned["BrokemiaHelper_crab"] = "CRAB";
                buttons.Insert(1, new MainMenuSmallButton("BrokemiaHelper_crab", "BrokemiaHelper/menu/crab", menu, Vector2.Zero, Vector2.Zero, () => OnCrabMenu(menu)));
            }
        }

        private static void OnCrabMenu(OuiMainMenu menu) {
            menu.Overworld.Goto<OuiCrabLoading>();
        }
    }

    public class OuiCrabLoading : Oui {
        private float alpha = 0;
        private bool done;
        private bool failed;
        private bool doPostcard;
        private bool restartText;

        public override IEnumerator Enter(Oui from) {
            yield return null;
            Overworld.ShowInputUI = false;
            Visible = true;
            while (alpha < 1) {
                alpha = Math.Min(1, alpha + Engine.DeltaTime * 2);
                yield return null;
            }

            yield return 3;

            var zipPath = Path.Combine(Everest.Loader.PathMods, "CarcinizationHelper.zip");
            if(File.Exists(zipPath)) {
                failed = true;
                Overworld.ShowInputUI = true;
                yield break;
            }

            using (var src = Assembly.GetAssembly(GetType()).GetManifestResourceStream("BrokemiaHelper.CarcinizationHelper.zip")) {
                using var dst = new FileStream(zipPath, FileMode.CreateNew);
                if( src == null || dst == null || !dst.CanWrite) {
                    failed = true;
                    Overworld.ShowInputUI = true;
                    yield break;
                }
                var task = src.CopyToAsync(dst);
                while (!task.IsCompleted) {
                    yield return null;
                }
            }

            done = true;
            Overworld.ShowInputUI = true;

            while(!doPostcard) {
                yield return null;
            }

            var postcard = new Postcard("Ready for a real challenge?{n}{n}{# F94A4A}Crab-Sides{#} have been unlocked!{n}{n}Good luck!", "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out");
            Add(new BeforeRenderHook(postcard.BeforeRender));
            Scene.Add(postcard);
            yield return postcard.DisplayRoutine();
            restartText = true;
            yield return 3f;
            Everest.QuickFullRestart();
        }

        public override void Update() {
            base.Update();
            if ((failed || done) && (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed)) {
                if(done) {
                    doPostcard = true;
                } else {
                    MainThreadHelper.Do(() => Overworld.GetUI<OuiMainMenu>()?.RebuildMainAndTitle());
                    Overworld.Goto<OuiMainMenu>();
                }
            }
        }

        public override void Render() {
            base.Render();
            var text = restartText ? "Restarting Celeste. Prepare for crab." : (failed ? "Failed installing Carcinization Helper." : (done ? "Carcinization Helper Installed." : "Installing Carcinization Helper..."));
            ActiveFont.Draw(text, new Vector2(960, 540) - ActiveFont.Measure(text) / 2, Color.White * alpha);
        }

        public override IEnumerator Leave(Oui next) {
            while (alpha > 0) {
                alpha = Math.Max(0, alpha - Engine.DeltaTime * 2);
                yield return null;
            }
            Visible = false;
        }
    }
}

#endif
