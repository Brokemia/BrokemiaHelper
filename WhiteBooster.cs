using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BrokemiaHelper
{
    public class WhiteBooster : Entity
    {
        private const float RespawnTime = 1f;

        public static ParticleType P_Burst;

        public static ParticleType P_Appear;

        public static readonly Vector2 playerOffset = new Vector2(0f, -2f);

        private Sprite sprite;

        private Entity outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Coroutine dashRoutine;

        private DashListener dashListener;

        private ParticleType particleType;

        private float respawnTimer;

        private float cannotUseTimer;

        private SoundSource loopingSfx;

        private FakeBooster fakeBooster;

        public bool BoostingPlayer
        {
            get;
            private set;
        }

        public WhiteBooster(Vector2 position) : base(position)
        {
            fakeBooster = new FakeBooster(position, this);
            base.Depth = -8500;
            base.Collider = new Circle(10f, 0f, 2f);
            //TODO Sprites
            base.Add(this.sprite = GFX.SpriteBank.Create("booster"));
            base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
            base.Add(this.light = new VertexLight(Color.White, 1f, 16, 32));
            base.Add(this.bloom = new BloomPoint(0.1f, 16f));
            base.Add(this.wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                this.sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }, false, false));
            base.Add(this.dashRoutine = new Coroutine(false));
            base.Add(this.dashListener = new DashListener());
            base.Add(new MirrorReflection());
            base.Add(this.loopingSfx = new SoundSource());
            this.dashListener.OnDash = new Action<Vector2>(this.OnPlayerDashed);
            this.particleType = Booster.P_Burst;
        }

        public WhiteBooster(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Image image = new Image(GFX.Game["objects/booster/outline"]);
            image.CenterOrigin();
            image.Color = Color.White * 0.75f;
            this.outline = new Entity(this.Position);
            this.outline.Depth = 8999;
            this.outline.Visible = false;
            this.outline.Add(image);
            this.outline.Add(new MirrorReflection());
            scene.Add(this.outline);
        }

        public void Appear()
        {
            // TODO appear sound
            Audio.Play("event:/game/04_cliffside/greenbooster_reappear", this.Position);
            this.sprite.Play("appear", false, false);
            this.wiggler.Start();
            this.Visible = true;
            this.AppearParticles();
        }

        private void AppearParticles()
        {
            ParticleSystem particlesBG = base.SceneAs<Level>().ParticlesBG;
            for (int i = 0; i < 360; i += 30)
            {
                particlesBG.Emit(Booster.P_Appear, 1, base.Center, Vector2.One * 2f, (float)i * 0.0174532924f);
            }
        }

        public void OnPlayer(Player player)
        {
            if (this.respawnTimer <= 0f && this.cannotUseTimer <= 0f && !this.BoostingPlayer)
            {
                this.cannotUseTimer = 0.45f;
                player.RedBoost(fakeBooster);

                // TODO More sound effect
                //Audio.Play("event:/game/04_cliffside/whitebooster_enter", this.Position);
                this.wiggler.Start();
                this.sprite.Play("inside", false, false);
                this.sprite.FlipX = (player.Facing == Facings.Left);
            }
        }

        public void PlayerBoosted(Player player, Vector2 direction)
        {
            // TODO Sound
            //Audio.Play("event:/game/04_cliffside/whitebooster_dash", this.Position);
            // TODO Sound
            this.loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move", null, 0f);
            this.loopingSfx.DisposeOnTransition = false;

            this.BoostingPlayer = true;
            base.Tag = (Tags.Persistent | Tags.TransitionUpdate);
            this.sprite.Play("spin", false, false);
            this.sprite.FlipX = (player.Facing == Facings.Left);
            this.outline.Visible = true;
            this.wiggler.Start();
            this.dashRoutine.Replace(this.BoostRoutine(player, direction));
        }


        public IEnumerator BoostRoutine(Player player, Vector2 dir)
        {
            float angle = (-dir).Angle();
            // State 2 is dashing. State 5 is traveling in a red bubble
            while ((player.StateMachine.State == 2 || player.StateMachine.State == 5) && BoostingPlayer)
            {
                sprite.RenderPosition = player.Center + playerOffset;
                loopingSfx.Position = sprite.Position;
                if (Scene.OnInterval(0.02f))
                {
                    (Scene as Level).ParticlesBG.Emit(particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), angle);
                }
                yield return null;
            }
            PlayerReleased();
            if (player.StateMachine.State == 4)
            {
                sprite.Visible = false;
            }
            while (SceneAs<Level>().Transitioning)
            {
                yield return null;
            }
            Tag = 0;
        }

        public void OnPlayerDashed(Vector2 direction)
        {
            if (this.BoostingPlayer)
            {
                this.BoostingPlayer = false;
            }
        }

        public void PlayerReleased()
        {
            // TODO sound
            Audio.Play("event:/game/04_cliffside/greenbooster_end", this.sprite.RenderPosition);
            this.sprite.Play("pop", false, false);

            this.cannotUseTimer = 0f;
            this.respawnTimer = 1f;
            this.BoostingPlayer = false;
            this.wiggler.Stop();
            this.loopingSfx.Stop(true);
        }

        public void PlayerDied()
        {
            if (this.BoostingPlayer)
            {
                this.PlayerReleased();
                this.dashRoutine.Active = false;
                base.Tag = 0;
            }
        }

        public void Respawn()
        {
            // TODO sound
            //Audio.Play("event:/game/04_cliffside/whitebooster_reappear", this.Position);
            this.sprite.Position = Vector2.Zero;
            this.sprite.Play("loop", true, false);
            this.wiggler.Start();
            this.sprite.Visible = true;
            this.outline.Visible = false;
            this.AppearParticles();
        }

        public override void Update()
        {
            base.Update();
            if (this.cannotUseTimer > 0f)
            {
                this.cannotUseTimer -= Engine.DeltaTime;
            }
            if (this.respawnTimer > 0f)
            {
                this.respawnTimer -= Engine.DeltaTime;
                if (this.respawnTimer <= 0f)
                {
                    this.Respawn();
                }
            }
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (!this.dashRoutine.Active && this.respawnTimer <= 0f)
            {
                Vector2 target = Vector2.Zero;

                if (entity != null && base.CollideCheck(entity))
                {
                    target = entity.Center + Booster.playerOffset - this.Position;
                }
                this.sprite.Position = Calc.Approach(this.sprite.Position, target, 80f * Engine.DeltaTime);

            }
            if (this.sprite.CurrentAnimationID == "inside" && !this.BoostingPlayer && !base.CollideCheck<Player>())
            {
                this.sprite.Play("loop", false, false);
            }
            if (this.sprite.CurrentAnimationID == "inside" && entity != null && entity.StateMachine.State == 4)
            {
                this.sprite.RenderPosition = entity.Center + playerOffset;
                loopingSfx.Position = this.sprite.Position;
            }

        }

        public override void Render()
        {
            Vector2 position = this.sprite.Position;
            this.sprite.Position = position.Floor();
            if (this.sprite.CurrentAnimationID != "pop" && this.sprite.Visible)
            {
                this.sprite.DrawOutline(1);
            }
            base.Render();
            this.sprite.Position = position;
        }

        public override void Removed(Scene scene)
        {
            Level level = scene as Level;
            using (IEnumerator<Backdrop> enumerator = level.Background.GetEach<Backdrop>("bright").GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Backdrop expr_30 = enumerator.Current;
                    expr_30.ForceVisible = false;
                    expr_30.FadeAlphaMultiplier = 1f;
                }
            }
            level.Bloom.Base = AreaData.Get(level).BloomBase + 0.25f;
            level.Session.BloomBaseAdd = 0.25f;
            base.Removed(scene);
        }
            
        }
    }