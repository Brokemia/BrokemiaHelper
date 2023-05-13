using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BrokemiaHelper {
    [Tracked(false)]
    public class PersistentMiniTextbox : MiniTextbox {
        public class SpecialTextNode {
            public int location;

            public virtual void Draw(FancyText.Text self, Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start, int end) {

            }
        }

        public class SwitchingTextNode : SpecialTextNode {
            public string[] entries;
            public int current;
            private const float startCycleTime = .4f;
            public float timer = 0;

            public override void Draw(FancyText.Text self, Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start, int end) {
                base.Draw(self, position, justify, scale, alpha, start, end);
                if (start <= location && end >= location) {
                    timer -= Engine.DeltaTime;
                }

                // TODO Make it more obvious when it's settled, maybe briefly turn up the scale or do a wiggle
                if (timer < 0 && (current / entries.Length < 3 || current % entries.Length != 0)) {
                    // This is kind of complicated because I wanted to make it always take a consistent amount of time
                    float newTimer = startCycleTime;
                    for (int i = 0; i < current / entries.Length; i++) {
                        newTimer *= 2;
                    }
                    newTimer += ((newTimer * 2 - newTimer) / entries.Length) * (current % entries.Length);
                    newTimer /= entries.Length;
                    timer = newTimer;
                    for (int i = location; i < location + entries[current % entries.Length].Length; i++) {
                        (self[i] as FancyText.Char).Character = ' ';
                    }
                    current++;
                    var language = Dialog.Language;
                    var size = Fonts.Get(language.FontFace).Get(language.FontFaceSize);
                    float currentPosition = (self[location] as FancyText.Char).Position;
                    for (int i = location; i < location + entries[current % entries.Length].Length; i++) {
                        var c = self[i] as FancyText.Char;
                        c.Character = entries[current % entries.Length][i - location];
                        if ((currentPosition == 0f && c.Character == ' ') || c.Character == '\\') {
                            continue;
                        }
                        PixelFontCharacter pixelFontCharacter = size.Get(c.Character);
                        if (pixelFontCharacter == null) {
                            continue;
                        }
                        c.Position = currentPosition;
                        c.IsPunctuation = language.CommaCharacters.IndexOf((char)c.Character) >= 0 || language.PeriodCharacters.IndexOf((char)c.Character) >= 0;
                        currentPosition += pixelFontCharacter.XAdvance * c.Scale;
                        if (i < location + entries[current % entries.Length].Length - 1 && pixelFontCharacter.Kerning.TryGetValue(c.Character, out var value)) {
                            currentPosition += value * c.Scale;
                        }
                    }
                }
            }
        }

        private static readonly Regex insertLate = new Regex("\\{BH\\+\\s*(.*?)\\}", RegexOptions.Compiled);
        // Note that switching text MUST be at least 2 characters long
        private static readonly Regex switchingText = new Regex("\\{BH\\!\\s*(\\d*?)!(.*?)\\}", RegexOptions.Compiled);

        // Ideally no one will ever want to use this character in dialog
        private static readonly char indicatorChar = '⁗';
        private static readonly int nodeIDOffset = 57344;

        private static int nextNodeID = 1;
        private static readonly Dictionary<int, string> nodeTexts = new();
        private static readonly Dictionary<string, int> nodeTextsReverse = new();
        private static readonly Random rand = new(2354362);

        public static string ProcessDialog(string dialogID) {
            string text = Dialog.Get(dialogID.Trim());
            MatchCollection matchCollection = null;
            bool anyChanges = false;
            // Repeat until no more matches are found, just in case there are inserts in the inserted text
            while (matchCollection == null || matchCollection.Count > 0) {
                matchCollection = insertLate.Matches(text);
                for (int i = 0; i < matchCollection.Count; i++) {
                    Match match = matchCollection[i];
                    string value = match.Groups[1].Value;
                    text = ((!Dialog.Language.Dialog.TryGetValue(value, out string value2)) ? text.Replace(match.Value, "[XXX]") : text.Replace(match.Value, value2));
                    anyChanges = true;
                }
            }
            matchCollection = switchingText.Matches(text);
            for (int i = 0; i < matchCollection.Count; i++) {
                Match match = matchCollection[i];
                string txt = match.Groups[2].Value;
                if (!nodeTextsReverse.TryGetValue(txt, out int id)) {
                    id = nextNodeID;
                    nextNodeID++;
                    nodeTextsReverse[txt] = id;
                    nodeTexts[id] = txt;
                }
                if (!Dialog.Language.FontSize.Characters.ContainsKey(indicatorChar)) {
                    Dialog.Language.FontSize.Characters[indicatorChar] = new(indicatorChar, GFX.Game["__fallback"], Emoji.FakeXML);
                }
                if (!Dialog.Language.FontSize.Characters.ContainsKey(nodeIDOffset + id)) {
                    Dialog.Language.FontSize.Characters[nodeIDOffset + id] = new(nodeIDOffset + id, GFX.Game["__fallback"], Emoji.FakeXML);
                }
                text = (!int.TryParse(match.Groups[1].Value, out int value)) ? text.Replace(match.Value, "[XXX]") : text.Replace(match.Value, indicatorChar.ToString() + (char)(nodeIDOffset + id) + new string(' ', value - 2));
                anyChanges = true;
            }

            if (anyChanges) {
                Dialog.Language.Dialog[dialogID + "_BrokemiaHelperProcessed"] = text;
                return dialogID + "_BrokemiaHelperProcessed";
            }
            return dialogID;
        }

        private FancyText.Anchors anchor;
        private DynamicData selfData;

        public event Action OnFinish;

        private bool pauseUpdate;

        private float time;

        public PersistentMiniTextbox(string dialogId, FancyText.Anchors anchor = FancyText.Anchors.Top, bool pauseUpdate = false, float time = -1) : base(dialogId) {
            this.anchor = anchor;
            this.pauseUpdate = pauseUpdate;
            this.time = time;
            selfData = DynamicData.For(this);
            if (pauseUpdate) {
                AddTag(Tags.PauseUpdate);
                AddTag(Tags.FrozenUpdate);
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
        }

        public override void Render() {
            float ease = selfData.Get<float>("ease");
            if (ease <= 0f) {
                return;
            }
            Level level = Scene as Level;
            if ((!level.FrozenOrPaused || pauseUpdate) && level.RetryPlayerCorpse == null && !level.SkippingCutscene) {
                MTexture box = selfData.Get<MTexture>("box");
                Sprite portrait = selfData.Get<Sprite>("portrait");
                FancyText.Text text = selfData.Get<FancyText.Text>("text");
                float portraitScale = selfData.Get<float>("portraitScale");
                float portraitSize = selfData.Get<float>("portraitSize");
                int index = selfData.Get<int>("index");

                Vector2 vector = new Vector2(Engine.Width / 2, 72f + (Engine.Width - 1688f) / 4f);
                if (anchor == FancyText.Anchors.Bottom) {
                    vector.Y = Engine.Height - vector.Y;
                } else if (anchor == FancyText.Anchors.Middle) {
                    vector.Y = Engine.Height / 2;
                }
                Vector2 value = vector + new Vector2(-828f, -56f);

                box.DrawCentered(vector, Color.White, new Vector2(1f, ease));
                if (portrait != null) {
                    portrait.Scale = new Vector2(1f, ease) * portraitScale;
                    portrait.RenderPosition = value + new Vector2(portraitSize / 2f, portraitSize / 2f);
                    portrait.Render();
                }
                text.Draw(new Vector2(value.X + portraitSize + 32f, vector.Y), new Vector2(0f, 0.5f), new Vector2(1f, ease) * 0.75f, 1f, 0, index);
            }
        }

        public static void Load() {
            On.Celeste.MiniTextbox.Routine += MiniTextbox_Routine;
            On.Celeste.MiniTextbox.ctor += MiniTextbox_ctor;
            On.Celeste.Textbox.ctor_string_Language_Func1Array += Textbox_ctor;
            On.Celeste.FancyText.Parse += FancyText_Parse;
            On.Celeste.FancyText.Text.Draw += Text_Draw;
        }
        
        private static void Text_Draw(On.Celeste.FancyText.Text.orig_Draw orig, FancyText.Text self, Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start, int end) {
            var selfData = DynamicData.For(self);
            if (selfData.TryGet("brokemiaHelperSpecialNodes", out List<SpecialTextNode> nodes)) {
                foreach (SpecialTextNode node in nodes) {
                    node.Draw(self, position, justify, scale, alpha, start, end);
                }
            }
            orig(self, position, justify, scale, alpha, start, end);
        }

        private static FancyText.Text FancyText_Parse(On.Celeste.FancyText.orig_Parse orig, FancyText self) {
            var res = orig(self);

            for (int i = 0; i < res.Count; i++) {
                if (res[i] is FancyText.Char c && c.Character == indicatorChar) {
                    c.Character = ' ';
                    var textData = new DynamicData(res);
                    if (!textData.TryGet("brokemiaHelperSpecialNodes", out List<SpecialTextNode> nodes)) {
                        nodes = new();
                        textData.Set("brokemiaHelperSpecialNodes", nodes);
                    }
                    var nodeID = (res[i + 1] as FancyText.Char).Character - nodeIDOffset;
                    (res[i + 1] as FancyText.Char).Character = ' ';
                    var entries = Dialog.Has(nodeTexts[nodeID]) ? Dialog.Get(nodeTexts[nodeID]).Split('|') : new string[] { "[???]" };
                    nodes.Add(new SwitchingTextNode() { location = i, entries = entries, current = rand.Next(entries.Length) });
                }
            }

            return res;
        }

        private static void MiniTextbox_ctor(On.Celeste.MiniTextbox.orig_ctor orig, MiniTextbox self, string dialogId) {
            orig(self, ProcessDialog(dialogId));
        }

        private static void Textbox_ctor(On.Celeste.Textbox.orig_ctor_string_Language_Func1Array orig, Textbox self, string dialog, Language language, Func<IEnumerator>[] events) {
            orig(self, ProcessDialog(dialog), language, events);
        }

        public static void Unload() {
            On.Celeste.MiniTextbox.Routine -= MiniTextbox_Routine;
            On.Celeste.MiniTextbox.ctor -= MiniTextbox_ctor;
            On.Celeste.Textbox.ctor_string_Language_Func1Array -= Textbox_ctor;
            On.Celeste.FancyText.Parse -= FancyText_Parse;
            On.Celeste.FancyText.Text.Draw -= Text_Draw;
        }

        private static IEnumerator MiniTextbox_Routine(On.Celeste.MiniTextbox.orig_Routine orig, MiniTextbox self) {
            if (self is PersistentMiniTextbox selfPersistent) {
                IEnumerator res = orig(self);
                while (res.MoveNext()) {
                    if (res.Current is float f && f == 3f) {
                        if (selfPersistent.time < 0) {
                            selfPersistent.OnFinish?.Invoke();
                            selfPersistent.OnFinish = null;
                            yield break;
                        } else {
                            yield return selfPersistent.time;
                            selfPersistent.OnFinish?.Invoke();
                            selfPersistent.OnFinish = null;
                        }
                    } else {
                        yield return res.Current;
                    }
                }                
            } else {
                yield return new SwapImmediately(orig(self));
            }
        }
    }
}
