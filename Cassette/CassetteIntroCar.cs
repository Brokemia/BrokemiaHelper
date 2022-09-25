using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace BrokemiaHelper
{
    public class CassetteIntroCar : IntroCar, CassetteEntity
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

        private List<Image> pressed = new List<Image>();

        private List<Image> solid = new List<Image>();

        private List<Image> all = new List<Image>();
        
        private Wiggler wiggler;

        private Vector2 wigglerScaler;

        private FieldInfo wheelsInfo = typeof(IntroCar).GetField("wheels", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
        private FieldInfo bodySprInfo = typeof(IntroCar).GetField("bodySprite", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

        EntityID CassetteEntity.ID { get => ID; set => ID = value; }
        int CassetteEntity.Index { get => Index; set => Index = value; }
        bool CassetteEntity.Activated { get => Activated; set => Activated = value; }
        float CassetteEntity.Tempo { get => Tempo; set => Tempo = value; }

        public CassetteIntroCar(Vector2 position, EntityID id, int index, float tempo)
            : base(position)
        {
            Index = index;
            Tempo = tempo;
            Collidable = false;
            ID = id;
            // Colors were lightened because the darker versions didn't work as well with the intro car
            // Still might need fine tuning
            switch (Index)
            {
                default:
                    color = Calc.HexToColor("59baff");
                    break;
                case 1:
                    color = Calc.HexToColor("ff59ce");
                    break;
                case 2:
                    color = Calc.HexToColor("ffec4a");
                    break;
                case 3:
                    color = Calc.HexToColor("48f05e");
                    break;
            }

        }

        public CassetteIntroCar(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, data.Int("index"), data.Float("tempo", 1f))
        {
        }

        public CassetteIntroCar(EntityData data, Vector2 offset)
            : this(data, offset, new EntityID(data.Level.Name, data.ID))
        {
        }

        public EntityID GetID()
        {
            return ID;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            (bodySprInfo.GetValue(this) as Image).Color = this.color;
            Color color = Calc.HexToColor("667da5");
            disabledColor = new Color((float)(int)color.R / 255f * ((float)(int)this.color.R / 255f), (float)(int)color.G / 255f * ((float)(int)this.color.G / 255f), (float)(int)color.B / 255f * ((float)(int)this.color.B / 255f), 1f);
            foreach (StaticMover staticMover in staticMovers)
            {
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null)
                {
                    spikes.EnabledColor = this.color;
                    spikes.DisabledColor = disabledColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(this.color);
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null)
                {
                    spring.DisabledColor = disabledColor;
                    spring.VisibleWhenDisabled = true;
                }
            }
            float num = Left;
            float num2 = Right;
            float num3 = Bottom;
            float num4 = Top;
            Vector2 gOrigin = new Vector2((int)(num + (num2 - num) / 2f), (int)num4);

            wigglerScaler = new Vector2(Calc.ClampedMap(num2 - num, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(num4 - num3, 32f, 96f, 1f, 0.2f));
            Add(wiggler = Wiggler.Create(0.3f, 3f));
            foreach (StaticMover staticMover2 in staticMovers)
            {
                (staticMover2.Entity as Spikes)?.SetOrigins(gOrigin);
            }
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
                    EnableStaticMovers();
                    ShiftSize(-1);
                    wiggler.Start();
                }
            }
            else if (!Activated && Collidable)
            {
                ShiftSize(1);
                Collidable = false;
                DisableStaticMovers();
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

        private void UpdateVisualState()
        {
            Depth = !Collidable ? 8990 : 1;
            Entity wheels = wheelsInfo.GetValue(this) as Entity;
            wheels.Depth = Depth + 2;
            Image bodySpr = bodySprInfo.GetValue(this) as Image;
            bodySpr.Color = Collidable ? color : disabledColor;
            foreach (StaticMover staticMover in staticMovers)
            {
                staticMover.Entity.Depth = base.Depth + 1;
            }
            foreach (Image item in solid)
            {
                item.Visible = Collidable;
            }
            foreach (Image item2 in pressed)
            {
                item2.Visible = !Collidable;
            }
            Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
            foreach (Image item4 in all)
            {
                item4.Scale = scale;
            }
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
            ShiftSize(2);
            DisableStaticMovers();
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
            MoveV(amount);
            blockHeight -= amount;
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
    }

}