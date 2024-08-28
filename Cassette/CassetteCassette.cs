using System;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace BrokemiaHelper
{
    [CustomEntity("brokemiahelper/cassetteCassette")]
    public class CassetteCassette : Cassette
    {
        private Color color;
        private Color disabledColor;

        protected Color defaultImageColor = new Color(255, 255, 255, 255);

        private CassetteListener cassetteListener;

        public CassetteCassette(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
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
            color = GetColorFromIndex(cassetteListener.Index);
            Color c = Calc.HexToColor("667da5");
            disabledColor = new Color(c.R / 255f * (color.R / 255f), c.G / 255f * (color.G / 255f), c.B / 255f * (color.B / 255f), 1f);
            UpdateVisualState();
        }

        public override void Update() {
            base.Update();

            if (cassetteListener.Activated && !Collidable) {
                Collidable = true;
                ShiftSize(-1);
            } else if (!cassetteListener.Activated && Collidable) {
                ShiftSize(1);
                Collidable = false;
            }
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            Depth = !Collidable ? 8990 : -8500;
            if(sprite != null)
                sprite.Color = Collidable ? color : disabledColor;
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

        public void Finish()
        {
            cassetteListener.Activated = false;
        }

        public void WillToggle()
        {
            ShiftSize(Collidable ? 1 : (-1));
            UpdateVisualState();
        }

        private void ShiftSize(int amount)
        {
            Y += amount;
        }
    }
}
