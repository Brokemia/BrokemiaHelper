using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace BrokemiaHelper
{
    [CustomEntity("brokemiahelper/cassetteIntroCar")]
    public class CassetteIntroCar : IntroCar
    {
        private Color color;
        private Color disabledColor;

        protected Color defaultImageColor = new Color(255, 255, 255, 255);
        
        private Wiggler wiggler;

        private Vector2 wigglerScaler;

        private CassetteListener cassetteListener;

        public CassetteIntroCar(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            Add(cassetteListener = new CassetteListener(
                id,
                data.Int("index"),
                data.Float("tempo", 1f)
            ) {
                OnFinish = Finish,
                OnWillActivate = WillToggle,
                OnWillDeactivate = WillToggle,
                OnStart = OnStart
            });
            Collidable = false;
            
            // Colors were lightened because the darker versions didn't work as well with the intro car
            // Still might need fine tuning
            switch (cassetteListener.Index) {
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

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            bodySprite.Color = this.color;
            Color color = Calc.HexToColor("667da5");
            disabledColor = new Color(color.R / 255f * (color.R / 255f), color.G / 255f * (color.G / 255f), color.B / 255f * (color.B / 255f), 1f);
            foreach (StaticMover staticMover in staticMovers) {
                if (staticMover.Entity is Spikes spikes)
                {
                    spikes.EnabledColor = this.color;
                    spikes.DisabledColor = disabledColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(this.color);
                }
                if (staticMover.Entity is Spring spring)
                {
                    spring.DisabledColor = disabledColor;
                    spring.VisibleWhenDisabled = true;
                }
            }
            
            Vector2 gOrigin = new Vector2((int)(Left + (Right - Left) / 2f), (int)Top);

            wigglerScaler = new Vector2(Calc.ClampedMap(Right - Left, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(Top - Bottom, 32f, 96f, 1f, 0.2f));
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
            if (cassetteListener.Activated && !Collidable)
            {
                if (!BlockedCheck())
                {
                    Collidable = true;
                    EnableStaticMovers();
                    ShiftSize(-1);
                    wiggler.Start();
                }
            }
            else if (!cassetteListener.Activated && Collidable)
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
            wheels.Depth = Depth + 2;
            bodySprite.Color = Collidable ? color : disabledColor;
            foreach (StaticMover staticMover in staticMovers)
            {
                staticMover.Entity.Depth = Depth + 1;
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

        private void OnStart(bool activated)
        {
            Collidable = activated;
            UpdateVisualState();
            if (activated)
            {
                EnableStaticMovers();
                return;
            }
            ShiftSize(2);
            DisableStaticMovers();
        }

        private void Finish()
        {
            cassetteListener.Activated = false;
        }

        private void WillToggle()
        {
            ShiftSize(Collidable ? 1 : (-1));
            UpdateVisualState();
        }

        private void ShiftSize(int amount)
        {
            MoveV(amount);
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
