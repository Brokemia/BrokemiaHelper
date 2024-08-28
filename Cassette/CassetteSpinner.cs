using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace BrokemiaHelper
{
    [CustomEntity("brokemiahelper/cassetteSpinner")]
    public class CassetteSpinner : CrystalStaticSpinner
    {
        private Color color;
        private Color disabledColor;

        protected Color defaultImageColor = new Color(255, 255, 255, 255);

        private Wiggler wiggler;

        private Vector2 wigglerScaler;

        private CassetteListener cassetteListener;

        public CassetteSpinner(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, data.Bool("attachToSolid"), CrystalColor.Rainbow)
        {
            Add(cassetteListener = new CassetteListener(
                id,
                data.Int("index"),
                data.Float("tempo", 1f)
            ) {
                OnWillActivate = WillToggle,
                OnWillDeactivate = WillToggle,
                OnStart = OnStart
            });
            
            Collidable = false;
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
            if(filler != null)
            {
                filler.Y += move;
            }
        }

        private void UpdateHue(Color c)
        {
            foreach (Component component in Components)
            {
                if (component is Image image)
                {
                    image.Color = c;
                }
            }
            if (filler != null)
            {
                foreach (Component component in filler.Components)
                {
                    if (component is Image image)
                    {
                        image.Color = c;
                    }
                }
            }
        }

        private void UpdateDepth(int d)
        {
            if (filler != null)
            {
                filler.Depth = d + 1;
            }
            if (border != null)
            {
                border.Depth = d + 2;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            color = GetColorFromIndex(cassetteListener.Index);
            Color c = Calc.HexToColor("667da5");
            disabledColor = new Color(c.R / 255f * (color.R / 255f), c.G / 255f * (color.G / 255f), c.B / 255f * (color.B / 255f), 1f);
            Vector2 gOrigin = new Vector2((int)(Left + (Right - Left) / 2f), (int)Top);
            wigglerScaler = new Vector2(Calc.ClampedMap(Right - Left, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(Top - Bottom, 32f, 96f, 1f, 0.2f));
            Add(wiggler = Wiggler.Create(0.3f, 3f));
            UpdateVisualState();
        }

        public override void Update()
        {
            // Spinners mess with Collidable so we need to address that
            bool collidable = Collidable;
            base.Update();
            Collidable = collidable;

            if (cassetteListener.Activated && !Collidable)
            {
                Collidable = true;
                ShiftSize(-1);
                wiggler.Start();
            }
            else if (!cassetteListener.Activated && Collidable)
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

        public void OnStart(bool activated)
        {
            Collidable = activated;
            UpdateVisualState();
            if (activated)
            {
                return;
            }
            ShiftSize(2);
        }

        public void WillToggle()
        {
            ShiftSize(Collidable ? 1 : (-1));
            UpdateVisualState();
        }

        private void ShiftSize(int amount)
        {
            MoveVExact(amount);
        }
    }
}
