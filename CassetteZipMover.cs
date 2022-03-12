using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper
{
    public class CassetteZipMover : ZipMover, CassetteEntity
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

        private static FieldInfo innerCogsInfo = typeof(ZipMover).GetField("innerCogs", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo edgesInfo = typeof(ZipMover).GetField("edges", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo percentInfo = typeof(ZipMover).GetField("percent", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private static FieldInfo tempInfo = typeof(ZipMover).GetField("temp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        EntityID CassetteEntity.ID { get => ID; set => ID = value; }
        int CassetteEntity.Index { get => Index; set => Index = value; }
        bool CassetteEntity.Activated { get => Activated; set => Activated = value; }
        float CassetteEntity.Tempo { get => Tempo; set => Tempo = value; }

        public CassetteZipMover(EntityData data, Vector2 offset) : base(data, offset)
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
            UpdateVisualState();
        }

        public bool BlockedCheck()
        {
            TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
            if (theoCrystal != null && !TryActorWiggleUp(theoCrystal))
            {
                return true;
            }
            Player player = CollideFirst<Player>();
            if (player != null && !TryActorWiggleUp(player))
            {
                return true;
            }
            return false;
        }

        private bool TryActorWiggleUp(Entity actor)
        {
            bool collidable = Collidable;
            Collidable = true;
            for (int i = 1; i <= 4; i++)
            {
                if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
                {
                    actor.Position -= Vector2.UnitY * i;
                    Collidable = collidable;
                    return true;
                }
            }
            Collidable = collidable;
            return false;
        }

        public override void Update()
        {
            base.Update();
            if (Activated && !Collidable)
            {
                if (!BlockedCheck())
                {
                    Collidable = true;
                    wiggler.Start();
                    EnableStaticMovers();
                }
            }
            else if (!Activated && Collidable)
            {
                Collidable = false;
                DisableStaticMovers();
            }
            UpdateVisualState();
        }

        public override void Render()
        {
            List<MTexture> innerCogs = (List<MTexture>)innerCogsInfo.GetValue(this);
            MTexture[,] edges = (MTexture[,])edgesInfo.GetValue(this);
            float percent = (float)percentInfo.GetValue(this);
            MTexture temp = (MTexture)tempInfo.GetValue(this);

            Vector2 position = Position;
            Position += base.Shake;
            Draw.Rect(base.X + 1f, base.Y + 1f, base.Width - 2f, base.Height - 2f, Color.Black);
            int num = 1;
            float num2 = 0f;
            int count = innerCogs.Count;
            for (int i = 4; (float)i <= base.Height - 4f; i += 8)
            {
                int num3 = num;
                for (int j = 4; (float)j <= base.Width - 4f; j += 8)
                {
                    int index = (int)(mod((num2 + (float)num * percent * (float)Math.PI * 4f) / ((float)Math.PI / 2f), 1f) * (float)count);
                    MTexture mTexture = innerCogs[index];
                    Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
                    Vector2 zero = Vector2.Zero;
                    if (j <= 4)
                    {
                        zero.X = 2f;
                        rectangle.X = 2;
                        rectangle.Width -= 2;
                    }
                    else if ((float)j >= base.Width - 4f)
                    {
                        zero.X = -2f;
                        rectangle.Width -= 2;
                    }
                    if (i <= 4)
                    {
                        zero.Y = 2f;
                        rectangle.Y = 2;
                        rectangle.Height -= 2;
                    }
                    else if ((float)i >= Height - 4f)
                    {
                        zero.Y = -2f;
                        rectangle.Height -= 2;
                    }
                    mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
                    mTexture.DrawCentered(Position + new Vector2(j, i) + zero, (Collidable ? color: disabledColor) * ((num < 0) ? 0.5f : 1f));
                    num = -num;
                    num2 += (float)Math.PI / 3f;
                }
                if (num3 == num)
                {
                    num = -num;
                }
            }
            for (int k = 0; (float)k < Width / 8f; k++)
            {
                for (int l = 0; (float)l < Height / 8f; l++)
                {
                    int num4 = (k != 0) ? (((float)k != Width / 8f - 1f) ? 1 : 2) : 0;
                    int num5 = (l != 0) ? (((float)l != Height / 8f - 1f) ? 1 : 2) : 0;
                    if (num4 != 1 || num5 != 1)
                    {
                        edges[num4, num5].Draw(new Vector2(base.X + (float)(k * 8), base.Y + (float)(l * 8)), Vector2.Zero, (Collidable ? color : disabledColor));
                    }
                }
            }
            //typeof(Solid).GetMethod("Render").Invoke(this, null);// TODO fix
            base.Render();
            Position = position;
        }

        private void UpdateVisualState()
        {
            Depth = !Collidable ? 8990 : -9999;

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

        public void SetActivatedSilently(bool activated)
        {
            Activated = Collidable = activated;
            UpdateVisualState();
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
        }

        public void WillToggle()
        {
            UpdateVisualState();
        }

        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
