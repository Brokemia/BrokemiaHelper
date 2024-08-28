//#define CARCINIZATION

using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Celeste.Mod.CelesteNet.Client;
using System.Runtime.CompilerServices;
using BrokemiaHelper.Deco;
using Celeste.Mod.Helpers.LegacyMonoMod;

// WhiteBooster, CassetteZipMover, and CelesteInCeleste commented out for a mostly-functioning release
[assembly: IgnoresAccessChecksTo("Celeste")]
namespace BrokemiaHelper
{
    public class BrokemiaHelperModule : EverestModule
    {

        // Only one alive module instance can exist at any given time.
        public static BrokemiaHelperModule Instance;

        private static float whiteBoosterSpeed = 40f;
        // The multiplier to add to the booster vertical speed when holding up
        private static float whiteBoosterUpSpeedMultiplier = .75f;

        public static float shadownKevinTime = 0.2f;

        private bool isInShadowKevin = false;

        private static EverestModuleMetadata celesteNetDependency = new EverestModuleMetadata { Name = "CelesteNet.Client", Version = new Version(2, 0, 0) };

        private static bool celesteNetInstalled {
            get {
                return Everest.Loader.DependencyLoaded(celesteNetDependency);
            }
        }

        private static bool celesteNetConnected {
            get {
                return CelesteNetClientModule.Instance?.Client?.Con != null;
            }
        }

        public static bool CelesteNetConnected() {
            return celesteNetInstalled && celesteNetConnected;
        }

        public BrokemiaHelperModule()
        {
            Instance = this;
        }

        // If you don't need to store any settings, => null
        public override Type SessionType => typeof(BrokemiaHelperSession);

        public static BrokemiaHelperSession Session => (BrokemiaHelperSession)Instance._Session;

        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load()
        {
            // Stuff that runs orig(self) always
            /* ************************************************ */
            //CelesteInCeleste.Load();
            On.Monocle.Tracker.Initialize += (orig) => {
                orig();
                Tracker.TrackedEntityTypes[typeof(CassetteDreamBlock)].Add(typeof(DreamBlock));
            };
            Everest.Events.Level.OnLoadEntity += Level_OnLoadEntity;
            On.Celeste.Player.RedDashUpdate += (orig, self) =>
            {
                if (self.CurrentBooster is FakeBooster)
                {
                    FieldInfo lastAim = typeof(Player).GetField("lastAim", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                    lastAim.SetValue(self, new Vector2(0, -1));
                }

                return orig(self);
            };
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.DreamBlock.OneUseDestroy += DreamBlock_OneUseDestroy;

            On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
            On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
            On.Celeste.Solid.MoveHExact += Solid_MoveHExact;
            On.Celeste.Solid.MoveVExact += Solid_MoveVExact;

            MoveBlockBarrierRenderer.Load();
            MoveBlockBarrier.Load();
            FloatierSpaceBlock.Load();
            CelesteNetFlagSynchronizer.Load();
            CaveWall.Load();
            PersistentMiniTextbox.Load();
            TronState.Load();
            NeutralDeathPenaltyTrigger.Load();
            ImagineAWorldWhereTextboxesDoNotHavePortraitsThisIsTheTwilightZone.Load();
            PlayerMotionComponent.Load();
            UniversalAccelerator.Load();
#if CARCINIZATION
            Carcinization.Load();
#endif

            DecalRegistry.AddPropertyHandler("BrokemiaHelper_cassetteAnimated", (decal, attrs) => {
                int[] beatFrames = [0, 0, 0, 0];
                string[] attrNames = ["redFrame", "blueFrame", "yellowFrame", "greenFrame"];
                int last = 0;

                for(int i = 0; i < attrNames.Length; i++) {
                    if(attrs[attrNames[i]] != null) {
                        beatFrames[i] = int.Parse(attrs[attrNames[i]].Value);
                        last = beatFrames[i];
                    } else {
                        beatFrames[i] = last;
                    }
                }

                decal.Add(new DecalCassetteAnimator(beatFrames));
            });

            // Stuff that doesn't always run orig(self) and therefore should run after every hook
            /* ******************************************* */
            using (new LegacyDetourContext("BrokemiaHelper")
            {
                Before = { "*" }
            })
            {
                //On.Celeste.Player.RedDashCoroutine += Player_RedDashCoroutine;

                FakeBooster.Forwarding();
            }
        }

        private void Solid_MoveHExact(On.Celeste.Solid.orig_MoveHExact orig, Solid self, int move) {
            if(self is ShadowCrushBlock) {
                isInShadowKevin = true;
            }
            orig(self, move);
            isInShadowKevin = false;
        }

        private void Solid_MoveVExact(On.Celeste.Solid.orig_MoveVExact orig, Solid self, int move) {
            if (self is ShadowCrushBlock) {
                isInShadowKevin = true;
            }
            orig(self, move);
            isInShadowKevin = false;
        }

        private bool Actor_MoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher) {
            if (self is Player) {
                if (isInShadowKevin) {
                    DynData<Player> selfData = new DynData<Player>(self as Player);
                    selfData.Set<float?>("brokemiaHelperShadowTimer", shadownKevinTime);
                }
            }
            
            bool res = orig(self, moveV, onCollide, pusher);
            return res;
        }

        private bool Actor_MoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher) {
            if (self is Player) {
                if (isInShadowKevin) {
                    DynData<Player> selfData = new DynData<Player>(self as Player);
                    selfData.Set<float?>("brokemiaHelperShadowTimer", shadownKevinTime);
                }
            }
            bool res = orig(self, moveH, onCollide, pusher);
            return res;
        }

        void DreamBlock_OneUseDestroy(On.Celeste.DreamBlock.orig_OneUseDestroy orig, DreamBlock self) {
            if (self is CassetteDreamBlock) {
                DynData<DreamBlock> blockData = new DynData<DreamBlock>(self);
                foreach (StaticMover staticMover in blockData.Get<List<StaticMover>>("staticMovers")) {
                    Spikes spikes = staticMover.Entity as Spikes;
                    if (spikes != null) {
                        spikes.VisibleWhenDisabled = false;
                    }
                    Spring spring = staticMover.Entity as Spring;
                    if (spring != null) {
                        spring.VisibleWhenDisabled = false;
                    }
                }
            }
            orig(self);
        }


        void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            // FIXME Triggers activate inside the orig Player.Update, so this code broke invincibility variant triggers
            //DynData<Player> selfData = new DynData<Player>(self);
            //float? shadowTimer = selfData.Get<float?>("brokemiaHelperShadowTimer");
            //bool wasInvincible = SaveData.Instance.Assists.Invincible;
            //if (shadowTimer.HasValue && shadowTimer.Value > 0) {
            //    selfData.Set<float?>("brokemiaHelperShadowTimer", shadowTimer - Engine.DeltaTime);
            //    SaveData.Instance.Assists.Invincible = true;
            //}
            if (self.StateMachine.State == 5 && self.LastBooster is FakeBooster currBooster)
            {
                MethodInfo CorrectDashPrecision = typeof(Player).GetMethod("CorrectDashPrecision", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);

                Vector2 movement = new Vector2(0, 0);

                if (Input.Aim.Value.X < 0)
                {
                    if (self.CenterX > currBooster.realBooster.CenterX - 24)
                    {
                        movement.X = Input.Aim.Value.X * 50;
                    }
                }
                else if (Input.Aim.Value.X > 0)
                {
                    if (self.CenterX < currBooster.realBooster.CenterX + 24)
                    {
                        movement.X = Input.Aim.Value.X * 50;
                    }
                }
                else if (self.CenterX != currBooster.realBooster.CenterX)
                {
                    movement.X = Math.Sign(currBooster.realBooster.CenterX - self.CenterX) * 15;
                }

                self.Speed.Y = whiteBoosterSpeed * (-1f + whiteBoosterUpSpeedMultiplier * Input.Aim.Value.Y);
                self.Speed.X = movement.X;
            }
            orig(self);
            //SaveData.Instance.Assists.Invincible = wasInvincible;
        }

        // TODO Add back in but don't desync TAS
        private IEnumerator Player_RedDashCoroutine(On.Celeste.Player.orig_RedDashCoroutine orig, Player self)
        {
            if (self.CurrentBooster is FakeBooster)
            {
                yield return null;
                FakeBooster currBooster = (FakeBooster)self.CurrentBooster;
                FieldInfo lastAim = typeof(Player).GetField("lastAim", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                FieldInfo gliderBoostDir = typeof(Player).GetField("gliderBoostDir", BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
                MethodInfo CorrectDashPrecision = typeof(Player).GetMethod("CorrectDashPrecision", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);
                MethodInfo CallDashEvents = typeof(Player).GetMethod("CallDashEvents", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);

                Vector2 movement = (Vector2)CorrectDashPrecision.Invoke(self, new object[] { lastAim.GetValue(self) }) * whiteBoosterSpeed;
                self.Speed = movement;
                gliderBoostDir.SetValue(self, self.DashDir = (Vector2)lastAim.GetValue(self));
                self.SceneAs<Level>().DirectionalShake(self.DashDir, 0.2f);
                if (self.DashDir.X != 0f)
                {
                    self.Facing = (Facings)Math.Sign(self.DashDir.X);
                }
                CallDashEvents.Invoke(self, null);
            }
            else
            {
                yield return new SwapImmediately(orig(self));
            }
        }

        bool Level_OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            if (entityData.Name.StartsWith("brokemiahelper/", StringComparison.InvariantCulture))
            {
                switch (entityData.Name.Substring("brokemiahelper/".Length))
                {
                    //case "whitebooster":
                        //level.Add(new WhiteBooster(entityData, offset));
                        //return true;
                }
            }
            return false;
        }


        // Optional, initialize anything after Celeste has initialized itself properly.
        public override void Initialize() {
            MoveBlockBarrier.Initialize();
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            context.Add<BrokemiaHelperMapDataProcessor>();
        }

        // Unload the entirety of your mod's content, remove any event listeners and undo all hooks.
        public override void Unload()
        {
            CelesteInCeleste.Unload();
            Everest.Events.Level.OnLoadEntity -= Level_OnLoadEntity;
            MoveBlockBarrierRenderer.Unload();
            MoveBlockBarrier.Unload();
            FloatierSpaceBlock.Unload();
            CelesteNetFlagSynchronizer.Unload();
            CaveWall.Unload();
            PersistentMiniTextbox.Unload();
            TronState.Unload();
            NeutralDeathPenaltyTrigger.Unload();
            ImagineAWorldWhereTextboxesDoNotHavePortraitsThisIsTheTwilightZone.Unload();
            UniversalAccelerator.Unload();
#if CARCINIZATION
            Carcinization.Unload();
#endif
            //On.Celeste.Player.RedDashCoroutine -= Player_RedDashCoroutine;
        }

    }
}