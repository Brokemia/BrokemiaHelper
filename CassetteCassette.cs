using System;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper
{
    public class CassetteCassette : Cassette, CassetteEntity
    {
        public int Index;

        public float Tempo;

        public bool Activated;

        public CassetteModes Mode;

        public EntityID ID;

        private int blockHeight = 2;

        private Color color;
        private Color disabledColor;

        protected Color defaultImageColor = new Color(255, 255, 255, 255);

        private Wiggler wiggler;

        private Vector2 wigglerScaler;

        protected FieldInfo spriteInfo = typeof(Cassette).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        EntityID CassetteEntity.ID { get => ID; set => ID = value; }
        int CassetteEntity.Index { get => Index; set => Index = value; }
        bool CassetteEntity.Activated { get => Activated; set => Activated = value; }
        float CassetteEntity.Tempo { get => Tempo; set => Tempo = value; }

        public CassetteCassette(EntityData data, Vector2 offset) : base(data, offset)
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
            UpdateVisualState();
        }

        public void MoveVExact(int move)
        {
            Y += move;
        }

        public override void Update()
        {
            base.Update();

            if (Activated && !Collidable)
            {
                Collidable = true;
                ShiftSize(-1);
                wiggler.Start();
            }
            else if (!Activated && Collidable)
            {
                ShiftSize(1);
                Collidable = false;
            }
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            Depth = !Collidable ? 8990 : -8500;
            if(spriteInfo.GetValue(this) != null)
                ((Sprite)spriteInfo.GetValue(this)).Color = Collidable ? color : disabledColor;
        }

        public void SetActivatedSilently(bool activated)
        {
            Activated = Collidable = activated;
            UpdateVisualState();
            if (activated)
            {
                return;
            }
            ShiftSize(2);
        }

        public void Finish()
        {
            Activated = false;
        }

        public void WillToggle()
        {
            ShiftSize(Collidable ? 1 : (-1));
            UpdateVisualState();
        }

        private void ShiftSize(int amount)
        {
            MoveVExact(amount);
            blockHeight -= amount;
        }

    }
}
