using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BrokemiaHelper
{
    public class FakeBooster : Booster
    {

        // The booster that the player is actually in
        public WhiteBooster realBooster;

        private Coroutine dashRoutine;

        private DashListener dashListener;
        
        public new bool BoostingPlayer
        {
            get;
            private set;
        }

        public FakeBooster(Vector2 position, WhiteBooster real) : base(position, true)
        {
            realBooster = real;
            base.Add(this.dashRoutine = new Coroutine(false));
            base.Add(this.dashListener = new DashListener());
            this.dashListener.OnDash = new Action<Vector2>(this.OnPlayerDashed);
        }

        public FakeBooster(EntityData data, Vector2 offset) : this(data.Position + offset, null)
        {
        }

        public override void Added(Scene scene)
        {
            //base.Added(scene);
        }

        private void AppearParticles()
        {}

        private void OnPlayer(Player player)
        {
            //if (this.respawnTimer <= 0f && this.cannotUseTimer <= 0f && !this.BoostingPlayer)
            //{
            //    this.cannotUseTimer = 0.45f;
            //    player.StateMachine.State = 4;
            //    player.Speed = Vector2.Zero;
            //    player.boostTarget = this.Center;
            //    player.boostRed = true;
            //    player.CurrentBooster = this;
            //    player.LastBooster = this;

            //    // TODO More sound effect
            //    Audio.Play("event:/game/04_cliffside/whitebooster_enter", this.Position);
            //    this.wiggler.Start();
            //    this.sprite.Play("inside", false, false);
            //    this.sprite.FlipX = (player.Facing == Facings.Left);
            //}
            realBooster.OnPlayer(player);
        }

        public new void PlayerBoosted(Player player, Vector2 direction)
        {
            realBooster.PlayerBoosted(player, direction);
            this.BoostingPlayer = true;
            base.Tag = (Tags.Persistent | Tags.TransitionUpdate);

            this.dashRoutine.Replace(realBooster.BoostRoutine(player, direction));
        }

        // TODO Figure out this
        private IEnumerator BoostRoutine(Player player, Vector2 dir)
        {
            yield return realBooster.BoostRoutine(player, dir);
        }

        public new void OnPlayerDashed(Vector2 direction)
        {
            if (this.BoostingPlayer)
            {
                this.BoostingPlayer = false;
            }
            realBooster.OnPlayerDashed(direction);
        }

        public new void PlayerReleased()
        {
            realBooster.PlayerReleased();
            this.BoostingPlayer = false;
        }

        public new void PlayerDied()
        {
            realBooster.PlayerDied();
            if (this.BoostingPlayer)
            {
                this.dashRoutine.Active = false;
                base.Tag = 0;
            }
        }

        public new void Respawn()
        {

        }

        public override void Update()
        {

        }

        public override void Render()
        {

        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        // Forces Booster.whatever to call the method in FakeBooster rather than the one in Booster when appropriate
        public static void Forwarding()
        {
            On.Celeste.Booster.PlayerBoosted += (orig, self, player, direction) =>
            {
                if (self.GetType() == typeof(FakeBooster))
                {
                    ((FakeBooster)self).PlayerBoosted(player, direction);
                }
                else
                {
                    orig(self, player, direction);
                }
            };
            On.Celeste.Booster.OnPlayerDashed += (orig, self, direction) =>
            {
                if (self.GetType() == typeof(FakeBooster))
                {
                    ((FakeBooster)self).OnPlayerDashed(direction);
                }
                else
                {
                    orig(self, direction);
                }
            };
            On.Celeste.Booster.PlayerReleased += (orig, self) =>
            {
                if (self.GetType() == typeof(FakeBooster))
                {
                    ((FakeBooster)self).PlayerReleased();
                }
                else
                {
                    orig(self);
                }
            };
            On.Celeste.Booster.PlayerDied += (orig, self) =>
            {
                if (self.GetType() == typeof(FakeBooster))
                {
                    ((FakeBooster)self).PlayerDied();
                }
                else
                {
                    orig(self);
                }
            };
            On.Celeste.Booster.Respawn += (orig, self) =>
            {
                if (self.GetType() == typeof(FakeBooster))
                {
                    ((FakeBooster)self).Respawn();
                }
                else
                {
                    orig(self);
                }
            };
        }

    }
}
