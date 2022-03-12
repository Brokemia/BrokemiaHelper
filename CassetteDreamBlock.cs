using System;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper
{
    // Removed ShiftSize because I'd have to mess with an already created Tween to get it to work right with nodes
    public class CassetteDreamBlock : DreamBlock, CassetteEntity
    {
        public int Index;

        public float Tempo;

        public bool Activated;

        public CassetteModes Mode;

        public EntityID ID;
        
        protected Color color;
        protected Color disabledColor;

        protected Color defaultImageColor = new Color(255, 255, 255, 255);

        private Wiggler wiggler;

        private Vector2 wigglerScaler;

        private static FieldInfo shakeInfo = typeof(DreamBlock).GetField("shake", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo playerHasDreamDashInfo = typeof(DreamBlock).GetField("playerHasDreamDash", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo activeBackColorInfo = typeof(DreamBlock).GetField("activeBackColor", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField);
        private static FieldInfo disabledBackColorInfo = typeof(DreamBlock).GetField("disabledBackColor", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField);
        private static FieldInfo whiteFillInfo = typeof(DreamBlock).GetField("whiteFill", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo wobbleFromInfo = typeof(DreamBlock).GetField("wobbleFrom", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo wobbleToInfo = typeof(DreamBlock).GetField("wobbleTo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo wobbleEaseInfo = typeof(DreamBlock).GetField("wobbleEase", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        private static FieldInfo playerDreamBlockInfo = typeof(Player).GetField("dreamBlock", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        private static MethodInfo LerpInfo = typeof(DreamBlock).GetMethod("Lerp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
        private static MethodInfo LineAmplitudeInfo = typeof(DreamBlock).GetMethod("LineAmplitude", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

        EntityID CassetteEntity.ID { get => ID; set => ID = value; }
        int CassetteEntity.Index { get => Index; set => Index = value; }
        bool CassetteEntity.Activated { get => Activated; set => Activated = value; }
        float CassetteEntity.Tempo { get => Tempo; set => Tempo = value; }

        public CassetteDreamBlock(EntityData data, Vector2 offset) : base(data, offset)
        {
            Index = data.Int("index");
            Tempo = data.Float("tempo", 1f);
            Collidable = false;
            ID = new EntityID(data.Level.Name, data.ID);
            color = GetColorFromIndex(Index);
            Color c = Calc.HexToColor("667da5");
            disabledColor = new Color((float)(int)c.R / 255f * ((float)(int)this.color.R / 255f), (float)(int)c.G / 255f * ((float)(int)this.color.G / 255f), (float)(int)c.B / 255f * ((float)(int)this.color.B / 255f), 1f);
        }

        public static Color GetColorFromIndex(int index)
        {
            switch (index)
            {
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

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            float num = Left;
            float num2 = Right;
            float num3 = Bottom;
            float num4 = Top;
            color = GetColorFromIndex(Index);
            Color c = Calc.HexToColor("667da5");
            disabledColor = new Color((float)(int)c.R / 255f * ((float)(int)this.color.R / 255f), (float)(int)c.G / 255f * ((float)(int)this.color.G / 255f), (float)(int)c.B / 255f * ((float)(int)this.color.B / 255f), 1f);
            Vector2 gOrigin = new Vector2((int)(num + (num2 - num) / 2f), (int)num4);
            wigglerScaler = new Vector2(Calc.ClampedMap(num2 - num, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(num4 - num3, 32f, 96f, 1f, 0.2f));
            Add(wiggler = Wiggler.Create(0.3f, 3f));
            foreach (StaticMover staticMover in staticMovers)
            {
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null)
                {
                    spikes.EnabledColor = color;
                    spikes.DisabledColor = disabledColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(color);
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null)
                {
                    spring.DisabledColor = disabledColor;
                    spring.VisibleWhenDisabled = true;
                }
            }
            Vector2 groupOrigin = new Vector2((int)(num + (num2 - num) / 2f), (int)num4);
            foreach (StaticMover staticMover2 in staticMovers)
            {
                (staticMover2.Entity as Spikes)?.SetOrigins(groupOrigin);
            }
            playerHasDreamDashInfo.SetValue(this, Collidable);
            UpdateVisualState();
        }

        public override void Update()
        {
            base.Update();
            if (Activated && !Collidable)
            {
                if (!BlockedCheck())
                {
                    Collidable = true;
                    playerHasDreamDashInfo.SetValue(this, true);
                    wiggler.Start();
                    EnableStaticMovers();
                }
            }
            else if (!Activated && Collidable)
            {
                Player player = SceneAs<Level>().Entities.FindFirst<Player>();
                if (!NoWiggleBlockedCheck() && (player == null || !Equals(playerDreamBlockInfo.GetValue(player))))
                {
                    Collidable = false;
                    playerHasDreamDashInfo.SetValue(this, false);
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

        private void UpdateVisualState()
        {
            Depth = !Collidable ? 5000 : -11000;
            foreach (StaticMover staticMover in staticMovers)
            {
                staticMover.Entity.Depth = base.Depth + 1;
            }
            Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
            foreach (StaticMover staticMover2 in staticMovers)
            {
                Spikes spikes = staticMover2.Entity as Spikes;
                if (spikes != null)
                {
                    foreach (Component component in spikes.Components)
                    {
                        Image image = component as Image;
                        if (image != null)
                        {
                            image.Scale = scale;
                        }
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            // Draw correctly colored border lines over the old ones
            Vector2 shake = (Vector2)shakeInfo.GetValue(this);
            WobbleLine(shake + new Vector2(X, Y), shake + new Vector2(X + Width, Y), 0f);
            WobbleLine(shake + new Vector2(X + Width, Y), shake + new Vector2(X + Width, Y + Height), 0.7f);
            WobbleLine(shake + new Vector2(X + Width, Y + Height), shake + new Vector2(X, Y + Height), 1.5f);
            WobbleLine(shake + new Vector2(X, Y + Height), shake + new Vector2(X, Y), 2.5f);
            Draw.Rect(shake + new Vector2(X, Y), 2f, 2f, Collidable ? color : disabledColor);
            Draw.Rect(shake + new Vector2(X + Width - 2f, Y), 2f, 2f, Collidable ? color : disabledColor);
            Draw.Rect(shake + new Vector2(X, Y + Height - 2f), 2f, 2f, Collidable ? color : disabledColor);
            Draw.Rect(shake + new Vector2(X + Width - 2f, Y + Height - 2f), 2f, 2f, Collidable ? color : disabledColor);
        }

        protected void WobbleLine(Vector2 from, Vector2 to, float offset)
        {
            float num = (to - from).Length();
            Vector2 value = Vector2.Normalize(to - from);
            Vector2 vector = new Vector2(value.Y, 0f - value.X);
            Color lineColor = Collidable ? color : disabledColor;
            Color color2 = (bool)playerHasDreamDashInfo.GetValue(this) ? (Color)activeBackColorInfo.GetValue(null) : (Color)disabledBackColorInfo.GetValue(null);
            float whiteFill = (float)whiteFillInfo.GetValue(this);
            if (whiteFill > 0f)
            {
                lineColor = Color.Lerp(lineColor, Color.White, whiteFill);
                color2 = Color.Lerp(color2, Color.White, whiteFill);
            }
            float wobbleFrom = (float)wobbleFromInfo.GetValue(this);
            float wobbleTo = (float)wobbleToInfo.GetValue(this);
            float wobbleEase = (float)wobbleEaseInfo.GetValue(this);
            float scaleFactor = 0f;
            int num2 = 16;
            for (int i = 2; (float)i < num - 2f; i += num2)
            {
                float num3 = (float)LerpInfo.Invoke(this, new object[] { LineAmplitudeInfo.Invoke(this, new object[] { wobbleFrom + offset, i }), LineAmplitudeInfo.Invoke(this, new object[] { wobbleTo + offset, i }), wobbleEase });
                if ((float)(i + num2) >= num)
                {
                    num3 = 0f;
                }
                float num4 = Math.Min(num2, num - 2f - (float)i);
                Vector2 vector2 = from + value * i + vector * scaleFactor;
                Vector2 vector3 = from + value * ((float)i + num4) + vector * num3;
                Draw.Line(vector2 - vector, vector3 - vector, color2);
                Draw.Line(vector2 - vector * 2f, vector3 - vector * 2f, color2);
                Draw.Line(vector2, vector3, lineColor);
                scaleFactor = num3;
            }
        }

        public void SetActivatedSilently(bool activated)
        {
            Activated = Collidable = activated;
            UpdateVisualState();
            playerHasDreamDashInfo.SetValue(this, activated);
            if (activated)
            {
                EnableStaticMovers();
                return;
            }
            DisableStaticMovers();
        }

        public void Finish()
        {
            Activated = false;
            playerHasDreamDashInfo.SetValue(this, Activated);
        }

        public void WillToggle()
        {
            UpdateVisualState();
        }

    }
}
