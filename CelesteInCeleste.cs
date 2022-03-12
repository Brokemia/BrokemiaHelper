using System;
using System.Collections;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper
{
    public class CelesteInCeleste
    {
        private static FieldInfo picoConsoleTalkingInfo = typeof(PicoConsole).GetField("talking", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        public static bool Incepted;

        public static Scene ReturnTo;

        public static void Load()
        {
            On.Celeste.PicoConsole.InteractRoutine += PicoConsole_InteractRoutine;
            On.Celeste.OuiMainMenu.OnExit += OuiMainMenu_OnExit;
        }

        public static void Unload()
        {
            On.Celeste.PicoConsole.InteractRoutine -= PicoConsole_InteractRoutine;
            On.Celeste.OuiMainMenu.OnExit -= OuiMainMenu_OnExit;
        }

        static IEnumerator PicoConsole_InteractRoutine(On.Celeste.PicoConsole.orig_InteractRoutine orig, PicoConsole self, Player player)
        {
            if (Incepted)
            {
                yield return new SwapImmediately(orig(self, player));
            }
            else
            {
                player.StateMachine.State = 11;
                yield return player.DummyWalkToExact((int)self.X - 6);
                player.Facing = Facings.Right;
                bool wasUnlocked = Settings.Instance.Pico8OnMainMenu;
                Settings.Instance.Pico8OnMainMenu = true;
                if (!wasUnlocked)
                {
                    UserIO.SaveHandler(file: false, settings: true);
                    while (UserIO.Saving)
                    {
                        yield return null;
                    }
                }
                else
                {
                    yield return 0.5f;
                }
                bool done = false;
                SpotlightWipe.FocusPoint = player.Position - (self.Scene as Level).Camera.Position + new Vector2(0f, -8f);
#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
                new SpotlightWipe(self.Scene, wipeIn: false, delegate
                {
                    if (!wasUnlocked)
                    {
                        self.Scene.Add(new UnlockedPico8Message(delegate
                        {
                            done = true;
                        }));
                    }
                    else
                    {
                        done = true;
                    }
                    Incepted = true;
                    ReturnTo = self.Scene;
                    Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen, new HiresSnow());
                //Engine.Scene = new Emulator(self.Scene as Level);
            });
#pragma warning restore RECS0026 // Possible unassigned object created by 'new'
                while (!done)
                {
                    yield return null;
                }
                yield return 0.25f;
                picoConsoleTalkingInfo.SetValue(self, false);
                (self.Scene as Level).PauseLock = false;
                player.StateMachine.State = 0;
            }
        }

        static void OuiMainMenu_OnExit(On.Celeste.OuiMainMenu.orig_OnExit orig, OuiMainMenu self)
        {
            if(Incepted)
            {
                Audio.Play("event:/ui/main/button_select");
                self.Focused = false;
#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
                new FadeWipe(self.Scene, wipeIn: false, delegate
                {
                    if (ReturnTo is Level)
                    {
                        (ReturnTo as Level).Session.Audio.Apply(forceSixteenthNoteHack: false);
                        new FadeWipe(ReturnTo, wipeIn: true);
                    }
                    Engine.Scene = ReturnTo;
                });
                Incepted = false;
                ReturnTo = null;
#pragma warning restore RECS0026 // Possible unassigned object created by 'new'
            }
            else
            {
                orig(self);
            }
        }

    }
}
