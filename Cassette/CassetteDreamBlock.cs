using System;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper {
    // I originally removed ShiftSize because I'd have to mess with an already created Tween to get it to work right with nodes
    // At this point it would probably have backwards compatibility issues if tried to add it back in
    [CustomEntity("brokemiahelper/cassetteDreamBlock")]
    public class CassetteDreamBlock : DreamBlock {
        protected Color color;
        protected Color disabledColor;

        protected Color defaultImageColor = new Color(255, 255, 255, 255);

        private Wiggler wiggler;

        private Vector2 wigglerScaler;

        private CassetteListener cassetteListener;

        public CassetteDreamBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset) {
            Collidable = false;
            Add(cassetteListener = new CassetteListener(
                id,
                data.Int("index"),
                data.Float("tempo", 1f)
            ) {
                OnFinish = Finish,
                OnStart = OnStart
            });
        }

        public static Color GetColorFromIndex(int index) {
            switch (index) {
                default:
                    return Calc.HexToColor("49aaf0");
                case 1:
                    return Calc.HexToColor("f049be");
                case 2:
                    return Calc.HexToColor("fcdc3a");
                case 3:
                    return Calc.HexToColor("38e04e");
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            color = GetColorFromIndex(cassetteListener.Index);
            Color c = Calc.HexToColor("667da5");
            disabledColor = new Color(c.R / 255f * (color.R / 255f), c.G / 255f * (color.G / 255f), c.B / 255f * (color.B / 255f), 1f);
            
            wigglerScaler = new Vector2(Calc.ClampedMap(Right - Left, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(Bottom - Top, 32f, 96f, 1f, 0.2f));
            Add(wiggler = Wiggler.Create(0.3f, 3f));
            foreach (StaticMover staticMover in staticMovers) {
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null) {
                    spikes.EnabledColor = color;
                    spikes.DisabledColor = disabledColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(color);
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null) {
                    spring.DisabledColor = disabledColor;
                    spring.VisibleWhenDisabled = true;
                }
            }
            Vector2 groupOrigin = new Vector2((int)(Left + (Right - Left) / 2f), (int)Bottom);
            foreach (StaticMover staticMover2 in staticMovers) {
                (staticMover2.Entity as Spikes)?.SetOrigins(groupOrigin);
            }
            playerHasDreamDash = Collidable;
            UpdateVisualState();
        }

        public override void Update() {
            base.Update();
            // Note: we need to check every frame in order to update as soon as the player leaves
            if (cassetteListener.Activated && !Collidable) {
                if (!BlockedCheck()) {
                    Collidable = true;
                    playerHasDreamDash = true;
                    wiggler.Start();
                    EnableStaticMovers();
                }
            } else if (!cassetteListener.Activated && Collidable) {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (!NoWiggleBlockedCheck() && (player == null || this != player.dreamBlock)) {
                    Collidable = false;
                    playerHasDreamDash = false;
                    DisableStaticMovers();
                }
            }
            UpdateVisualState();
        }

        // Used to stop the player from being wiggled up and then immediately re-entering the dream block
        public bool NoWiggleBlockedCheck() {
            TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
            if (theoCrystal != null) {
                return true;
            }
            Player player = CollideFirst<Player>();
            if (player != null) {
                return true;
            }
            return false;
        }

        private void UpdateVisualState() {
            Depth = !Collidable ? 5000 : -11000;
            foreach (StaticMover staticMover in staticMovers) {
                staticMover.Entity.Depth = Depth + 1;
            }
            Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
            foreach (StaticMover staticMover2 in staticMovers) {
                if (staticMover2.Entity is Spikes spikes) {
                    foreach (Component component in spikes.Components) {
                        if (component is Image image) {
                            image.Scale = scale;
                        }
                    }
                }
            }
        }

        public override void Render() {
            base.Render();
            // Draw correctly colored border lines over the old ones
            WobbleLine(shake + new Vector2(X, Y), shake + new Vector2(X + Width, Y), 0f);
            WobbleLine(shake + new Vector2(X + Width, Y), shake + new Vector2(X + Width, Y + Height), 0.7f);
            WobbleLine(shake + new Vector2(X + Width, Y + Height), shake + new Vector2(X, Y + Height), 1.5f);
            WobbleLine(shake + new Vector2(X, Y + Height), shake + new Vector2(X, Y), 2.5f);
            Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, Collidable ? color : disabledColor);
            Draw.Rect(shake + new Vector2(X + Width - 2f, Y), 2f, 2f, Collidable ? color : disabledColor);
            Draw.Rect(shake + new Vector2(X, Y + Height - 2f), 2f, 2f, Collidable ? color : disabledColor);
            Draw.Rect(shake + new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, Collidable ? color : disabledColor);
        }

        protected new void WobbleLine(Vector2 from, Vector2 to, float offset) {
            float lineLength = (to - from).Length();
            Vector2 direction = Vector2.Normalize(to - from);
            // This looks like a rotation by 90 degrees?
            Vector2 perp = new Vector2(direction.Y, -direction.X);
            Color lineColor = Collidable ? color : disabledColor;
            Color color2 = playerHasDreamDash ? activeBackColor : disabledBackColor;
            if (whiteFill > 0f) {
                lineColor = Color.Lerp(lineColor, Color.White, whiteFill);
                color2 = Color.Lerp(color2, Color.White, whiteFill);
            }
            
            float scaleFactor = 0f;
            int segmentLength = 16;
            for (int i = 2; i < lineLength - 2f; i += segmentLength) {
                float num3 = Lerp(LineAmplitude(wobbleFrom + offset, i ), LineAmplitude(wobbleTo + offset, i ), wobbleEase);
                if ((i + segmentLength) >= lineLength) {
                    num3 = 0f;
                }
                float actualLength = Math.Min(segmentLength, lineLength - 2f - i);
                Vector2 vector2 = from + direction * i + perp * scaleFactor;
                Vector2 vector3 = from + direction * (i + actualLength) + perp * num3;
                Draw.Line(vector2 - perp, vector3 - perp, color2);
                Draw.Line(vector2 - perp * 2f, vector3 - perp * 2f, color2);
                Draw.Line(vector2, vector3, lineColor);
                scaleFactor = num3;
            }
        }

        public void OnStart(bool activated) {
            Collidable = activated;
            UpdateVisualState();
            playerHasDreamDash = activated;
            if (activated) {
                EnableStaticMovers();
            } else {
                DisableStaticMovers();
            }
        }

        public void Finish() {
            playerHasDreamDash = cassetteListener.Activated;
        }

    }
}
