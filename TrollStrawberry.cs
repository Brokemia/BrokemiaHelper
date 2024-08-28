using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/trollStrawberry")]
    [RegisterStrawberry(false, false)]
    public class TrollStrawberry : Entity, IStrawberrySeeded {

        public EntityID ID;

        private Sprite sprite;

        public Follower Follower;

        private Wiggler wiggler;

        private Wiggler rotateWiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Tween lightTween;

        private float wobble;

        private Vector2 start;

        private float collectTimer;

        private bool collected;

        private bool flyingAway;

        private float flapSpeed;

        public bool ReturnHomeWhenLost = true;

        private bool reappear;

        protected bool waitingOnSeeds;

        protected List<GenericStrawberrySeed> seeds;

        private bool hadWings;
        
        public bool Winged {
            get;
            private set;
        }

        public bool Moon {
            get;
            private set;
        }

        public List<GenericStrawberrySeed> Seeds => seeds;

        public string gotSeedFlag => "collected_seeds_of_" + ID.ToString();

        public bool WaitingOnSeeds => waitingOnSeeds;

        public TrollStrawberry(EntityData data, Vector2 offset, EntityID gid) {
            ID = gid;
            Position = (start = data.Position + offset);
            hadWings = Winged = data.Bool("winged", false);
            Moon = data.Bool("moon");
            reappear = data.Bool("reappear", false);
            Depth = -100;
            Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer), null, null));
            Add(new MirrorReflection());
            Add(Follower = new Follower(ID, null, new Action(OnLoseLeader)));
            Follower.FollowDelay = 0.3f;
            if (Winged) {
                Add(new DashListener {
                    OnDash = new Action<Vector2>(OnDash)
                });
            }
            if (data.Nodes != null && data.Nodes.Length != 0) {
                seeds = new List<GenericStrawberrySeed>();
                for (int i = 0; i < data.Nodes.Length; i++) {
                    Seeds.Add(new GenericStrawberrySeed(this, offset + data.Nodes[i], i, false));
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (Moon) {
                sprite = GFX.SpriteBank.Create("moonberry");
            } else {
                sprite = GFX.SpriteBank.Create("strawberry");
            }
            Add(sprite);

            if (Winged) {
                sprite.Play("flap");
            }
            sprite.OnFrameChange = OnAnimate;
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate (float v) {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(rotateWiggler = Wiggler.Create(0.5f, 4f, delegate (float v) {
                sprite.Rotation = v * 30f * 0.0174532924f;
            }));
            Add(bloom = new BloomPoint(Moon ? 0.5f : 1f, 12f));
            Add(light = new VertexLight(Color.White, 1f, 16, 24));
            Add(lightTween = light.CreatePulseTween());
            if (Seeds != null && Seeds.Count > 0 && !(scene as Level).Session.GetFlag(gotSeedFlag)) {
                foreach (GenericStrawberrySeed seed in Seeds) {
                    scene.Add(seed);
                }
                Visible = false;
                Collidable = false;
                waitingOnSeeds = true;
                bloom.Visible = (light.Visible = false);
            }
            if ((scene as Level).Session.BloomBaseAdd > 0.1f) {
                bloom.Alpha *= 0.5f;
            }
        }

        public override void Update() {
            if(WaitingOnSeeds) {
                return;
            }
            if (!collected) {
                if (!Winged) {
                    wobble += Engine.DeltaTime * 4f;
                    sprite.Y = (bloom.Y = (light.Y = (float)Math.Sin(wobble) * 2f));
                }
                int followIndex = Follower.FollowIndex;
                if (Follower.Leader != null && Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this)) {
                    bool collectable = false;
                    if (Follower.Leader.Entity is Player player && player.Scene != null && !player.StrawberriesBlocked) {
                        collectable = player.OnSafeGround && (!Moon || player.StateMachine.State != 13);
                    }
                    if (collectable) {
                        collectTimer += Engine.DeltaTime;
                        if (collectTimer > 0.15f) {
                            OnCollect();
                        }
                    } else {
                        collectTimer = Math.Min(collectTimer, 0f);
                    }
                } else {
                    if (followIndex > 0) {
                        collectTimer = -0.15f;
                    }
                    if (Winged) {
                        Y += flapSpeed * Engine.DeltaTime;
                        if (flyingAway) {
                            if (Y < (SceneAs<Level>().Bounds.Top - 16)) {
                                RemoveSelf();
                            }
                        } else {
                            flapSpeed = Calc.Approach(flapSpeed, 20f, 170f * Engine.DeltaTime);
                            if (Y < start.Y - 5f) {
                                Y = start.Y - 5f;
                            } else if (Y > start.Y + 5f) {
                                Y = start.Y + 5f;
                            }
                        }
                    }
                }
            }
            base.Update();
            if (Follower.Leader != null && Scene.OnInterval(0.08f)) {
                ParticleType type = Moon ? Strawberry.P_MoonGlow : Strawberry.P_Glow;
                SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
            }
        }

        private void OnDash(Vector2 dir) {
            if (!flyingAway && Winged && !WaitingOnSeeds) {
                Depth = -1000000;
                Add(new Coroutine(FlyAwayRoutine(), true));
                flyingAway = true;
            }
        }

        private void OnAnimate(string id) {
            if (!flyingAway && id == "flap" && sprite.CurrentAnimationFrame % 9 == 4) {
                Audio.Play("event:/game/general/strawberry_wingflap", Position);
                flapSpeed = -50f;
            }
            int pulseFrame = (id == "flap") ? 25 : ((!Moon) ? 35 : 30);
            if (sprite.CurrentAnimationFrame == pulseFrame) {
                lightTween.Start();
                if (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>())) {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f, null, null);
                    return;
                }
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f, null, null);
            }
        }

        public void OnPlayer(Player player) {
            if (Follower.Leader == null && !collected && !WaitingOnSeeds) {
                ReturnHomeWhenLost = true;
                if (Winged) {
                    Level level = SceneAs<Level>();
                    Winged = false;
                    sprite.Rate = 0f;
                    Alarm.Set(this, Follower.FollowDelay, delegate {
                        sprite.Rate = 1f;
                        sprite.Play("idle", false, false);
                        level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position + new Vector2(8f, 0f), new Vector2(4f, 2f));
                        level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position - new Vector2(8f, 0f), new Vector2(4f, 2f));
                    }, Alarm.AlarmMode.Oneshot);
                }
                Audio.Play("event:/game/general/strawberry_touch", Position);
                player.Leader.GainFollower(Follower);
                wiggler.Start();
                Depth = -1000000;
            }
        }

        public void OnCollect() {
            if (collected) {
                return;
            }
            int collectIndex = 0;
            collected = true;
            if (Follower.Leader != null) {
                Player p = Follower.Leader.Entity as Player;
                collectIndex = p.StrawberryCollectIndex;
                p.StrawberryCollectIndex++;
                p.StrawberryCollectResetTimer = 2.5f;
                Follower.Leader.LoseFollower(Follower);
            }
            Session session = (Scene as Level).Session;
            if (!reappear) {
                session.DoNotLoad.Add(ID);
            }
            session.UpdateLevelStartDashes();
            Add(new Coroutine(CollectRoutine(collectIndex), true));
        }

        private IEnumerator FlyAwayRoutine() {
            rotateWiggler.Start();
            flapSpeed = -200f;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.25f, start: true);
            tween.OnUpdate = delegate (Tween t) {
                flapSpeed = MathHelper.Lerp(-200f, 0f, t.Eased);
            };
            Add(tween);
            yield return 0.1f;
            Audio.Play("event:/game/general/strawberry_laugh", Position);
            yield return 0.2f;
            if (!Follower.HasLeader) {
                Audio.Play("event:/game/general/strawberry_flyaway", Position);
            }
            tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
            tween.OnUpdate = delegate (Tween t) {
                flapSpeed = MathHelper.Lerp(0f, -200f, t.Eased);
            };
            Add(tween);
        }

        public void CollectedSeeds() {
            waitingOnSeeds = false;
            Visible = true;
            Collidable = true;
            bloom.Visible = (light.Visible = true);
            (Scene as Level).Session.SetFlag(gotSeedFlag);
        }

        private IEnumerator CollectRoutine(int collectIndex) {
            Level level = SceneAs<Level>();
            Tag = Tags.TransitionUpdate;
            Depth = -2000010;
            Audio.Play("event:/game/general/seed_poof", Position);
            Collidable = false;
            sprite.Scale = Vector2.One * 2f;
            yield return 0.05f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            for (int i = 0; i < 6; i++) {
                float num = Calc.Random.NextFloat((float)Math.PI * 2f);
                level.ParticlesFG.Emit(StrawberrySeed.P_Burst, 1, Position + Calc.AngleToVector(num, 4f), Vector2.Zero, num);
            }
            
            var bloomWasVisible = bloom.Visible;
            var lightWasVisible = light.Visible;
            bloom.Visible = false;
            light.Visible = false;
            Visible = false;
            // Yes I know it's invisible, I just don't want to work out the timing manually
            sprite.Play("collect");
            while (sprite.Animating) {
                yield return null;
            }
            if (!reappear || ID.Level != level.Session.Level) {
                RemoveSelf();
                yield break;
            }

            // Reappear            
            yield return 0.2f;
            bloom.Visible = bloomWasVisible;
            light.Visible = lightWasVisible;
            Position = start;
            sprite.Reverse("collect", true);
            while (sprite.Animating) {
                yield return null;
            }
            Winged = hadWings;
            if (Winged) {
                sprite.Play("flap");
            } else {
                sprite.Play("idle");
            }
            Audio.Play("event:/game/general/seed_reappear", Position, "count", 0);
            sprite.Scale = Vector2.One;
            Visible = true;
            Collidable = true;
            collected = false;
            Follower.Leader = null;
            Depth = -100;
            bloom.Visible = bloomWasVisible;
            light.Visible = lightWasVisible;
            level.Displacement.AddBurst(Position, 0.2f, 8f, 28f, 0.2f);
        }

        private void OnLoseLeader() {
            if (!collected && ReturnHomeWhenLost) {
                Alarm.Set(this, 0.15f, delegate {
                    Vector2 displacement = (start - Position).SafeNormalize();
                    float dist = Vector2.Distance(Position, start);
                    float scaleFactor = Calc.ClampedMap(dist, 16f, 120f, 16f, 96f);
                    Vector2 control = start + displacement * 16f + displacement.Perpendicular() * scaleFactor * Calc.Random.Choose(1, -1);
                    SimpleCurve curve = new SimpleCurve(Position, start, control);
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(dist / 100f, 0.4f), start: true);
                    tween.OnUpdate = delegate (Tween f) {
                        Position = curve.GetPoint(f.Eased);
                    };
                    tween.OnComplete = delegate {
                        Depth = 0;
                    };
                    Add(tween);
                });
            }
        }
    }
}