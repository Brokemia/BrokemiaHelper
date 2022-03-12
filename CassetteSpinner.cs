using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper
{
    public class CassetteSpinner : CrystalStaticSpinner, CassetteEntity
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

        protected static FieldInfo filler = typeof(CrystalStaticSpinner).GetField("filler", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

        protected static FieldInfo border = typeof(CrystalStaticSpinner).GetField("border", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

        protected static FieldInfo offset = typeof(CrystalStaticSpinner).GetField("offset", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

        protected static MethodInfo ClearSprites = typeof(CrystalStaticSpinner).GetMethod("ClearSprites", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);

        EntityID CassetteEntity.ID { get => ID; set => ID = value; }
        int CassetteEntity.Index { get => Index; set => Index = value; }
        bool CassetteEntity.Activated { get => Activated; set => Activated = value; }
        float CassetteEntity.Tempo { get => Tempo; set => Tempo = value; }

        public CassetteSpinner(Vector2 position, EntityID id, int index, float tempo, bool attachToSolid)
            : base(position, attachToSolid, CrystalColor.Rainbow)
        {
            Index = index;
            Tempo = tempo;
            Collidable = false;
            ID = id;
            color = GetColorFromIndex(Index);
            Color c = Calc.HexToColor("667da5");
            disabledColor = new Color((float)(int)c.R / 255f * ((float)(int)this.color.R / 255f), (float)(int)c.G / 255f * ((float)(int)this.color.G / 255f), (float)(int)c.B / 255f * ((float)(int)this.color.B / 255f), 1f);
        }

        public CassetteSpinner(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, id, data.Int("index"), data.Float("tempo", 1f), data.Bool("attachToSolid")) {}

        public CassetteSpinner(EntityData data, Vector2 offset)
            : this(data, offset, new EntityID(data.Level.Name, data.ID))
        {
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

        public void MoveVExact(int move)
        {
            Y += move;
            if(filler.GetValue(this) != null)
            {
                ((Entity)filler.GetValue(this)).Y += move;
            }
        }

        private void UpdateHue(Color c)
        {
            foreach (Component component in Components)
            {
                Image image = component as Image;
                if (image != null)
                {
                    image.Color = c;
                }
            }
            if (filler.GetValue(this) != null)
            {
                foreach (Component component2 in ((Entity)filler.GetValue(this)).Components)
                {
                    Image image2 = component2 as Image;
                    if (image2 != null)
                    {
                        image2.Color = c;
                    }
                }
            }
        }

        private void UpdateDepth(int d)
        {
            if (filler.GetValue(this) != null)
            {
                ((Entity)filler.GetValue(this)).Depth = d + 1;
            }
            if (border.GetValue(this) != null)
            {
                ((Entity)border.GetValue(this)).Depth = d + 2;
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

        public override void Update()
        {
            // Spinners mess with Collidable so we need to address that
            bool collidable = Collidable;
            base.Update();
            Collidable = collidable;

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
            UpdateDepth(Depth);
            UpdateHue(Collidable ? color : disabledColor);
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
