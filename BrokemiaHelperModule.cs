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

// WhiteBooster, CassetteZipMover, and CelesteInCeleste commented out for a mostly-functioning release
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

        private bool cassetteBlocksAdded;

        private static PropertyInfo ShouldCreateCassetteManager = typeof(Level).GetProperty("ShouldCreateCassetteManager", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

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
        public override Type SettingsType => null;

        // If you don't need to store any save data, => null
        public override Type SaveDataType => null;

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
            Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
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
            On.Celeste.CassetteBlockManager.StopBlocks += CassetteBlockManager_StopBlocks;
            On.Celeste.CassetteBlockManager.SilentUpdateBlocks += CassetteBlockManager_SilentUpdateBlocks;
            On.Celeste.CassetteBlockManager.SetActiveIndex += CassetteBlockManager_SetActiveIndex;
            On.Celeste.CassetteBlockManager.SetWillActivate += CassetteBlockManager_SetWillActivate;

            On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
            On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
            On.Celeste.Solid.MoveHExact += Solid_MoveHExact;
            On.Celeste.Solid.MoveVExact += Solid_MoveVExact;

            MoveBlockBarrierRenderer.Load();
            MoveBlockBarrier.Load();
            FloatierSpaceBlock.Load();
            CelesteNetFlagSynchronizer.Load();
            CaveWall.Load();

            DecalRegistry.AddPropertyHandler("BrokemiaHelper_cassetteAnimated", (decal, attrs) => {
                int[] beatFrames = new[] { 0, 0, 0, 0 };
                string[] attrNames = new[] { "redFrame", "blueFrame", "yellowFrame", "greenFrame" };
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
            using (new DetourContext("BrokemiaHelper")
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

        void CassetteBlockManager_SetActiveIndex(On.Celeste.CassetteBlockManager.orig_SetActiveIndex orig, CassetteBlockManager self, int index) {
            foreach (CassetteEntity entity in self.Scene.Entities.OfType<CassetteEntity>()) {
                entity.Activated = (entity.Index == index);
            }
            orig(self, index);
        }

        void CassetteBlockManager_StopBlocks(On.Celeste.CassetteBlockManager.orig_StopBlocks orig, CassetteBlockManager self) {
            foreach (CassetteEntity entity in self.Scene.Entities.OfType<CassetteEntity>()) {
                entity.Finish();
            }
            orig(self);
        }

        void CassetteBlockManager_SilentUpdateBlocks(On.Celeste.CassetteBlockManager.orig_SilentUpdateBlocks orig, CassetteBlockManager self) {
            DynData<CassetteBlockManager> selfData = new DynData<CassetteBlockManager>(self);
            foreach (CassetteEntity entity in self.Scene.Entities.OfType<CassetteEntity>()) {
                if (entity.ID.Level == self.SceneAs<Level>().Session.Level) {
                    entity.SetActivatedSilently(entity.Index == selfData.Get<int>("currentIndex"));
                }
            }
            orig(self);
        }

        void CassetteBlockManager_SetWillActivate(On.Celeste.CassetteBlockManager.orig_SetWillActivate orig, CassetteBlockManager self, int index) {
            foreach (CassetteEntity entity in self.Scene.Entities.OfType<CassetteEntity>()) {
                if (entity.Index == index || entity.Activated) {
                    entity.WillToggle();
                }
            }
            orig(self, index);
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
            DynData<Player> selfData = new DynData<Player>(self);
            float? shadowTimer = selfData.Get<float?>("brokemiaHelperShadowTimer");
            bool wasInvincible = SaveData.Instance.Assists.Invincible;
            if (shadowTimer.HasValue && shadowTimer.Value > 0) {
                selfData.Set<float?>("brokemiaHelperShadowTimer", shadowTimer - Engine.DeltaTime);
                SaveData.Instance.Assists.Invincible = true;
            }
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
            SaveData.Instance.Assists.Invincible = wasInvincible;
        }

        // TODO Add back in but don't desync TAS
        private IEnumerator Player_RedDashCoroutine(On.Celeste.Player.orig_RedDashCoroutine orig, Celeste.Player self)
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
                self.SceneAs<Celeste.Level>().DirectionalShake(self.DashDir, 0.2f);
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

        void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (cassetteBlocksAdded && level.Tracker.GetEntity<CassetteBlockManager>() == null && (bool)ShouldCreateCassetteManager.GetValue(level))
            {
                level.Add(new CassetteBlockManager());
                level.Tracker.GetEntity<CassetteBlockManager>()?.OnLevelStart();
            }
            cassetteBlocksAdded = false;
        }

        bool Level_OnLoadEntity(Celeste.Level level, Celeste.LevelData levelData, Vector2 offset, Celeste.EntityData entityData)
        {
            if (entityData.Name.StartsWith("brokemiahelper/", StringComparison.InvariantCulture))
            {
                CassetteEntity cassetteEntity = null;
                switch (entityData.Name.Substring("brokemiahelper/".Length))
                {
                    //case "whitebooster":
                        //level.Add(new WhiteBooster(entityData, offset));
                        //return true;
                    case "cassetteIntroCar":
                        cassetteEntity = new CassetteIntroCar(entityData, offset);
                        break;
                    case "cassetteSpinner":
                        cassetteEntity = new CassetteSpinner(entityData, offset);
                        break;
                    case "cassetteDreamBlock":
                        cassetteEntity = new CassetteDreamBlock(entityData, offset);
                        break;
                    case "cassetteCassette":
                        cassetteEntity = new CassetteCassette(entityData, offset);
                        break;
                    //case "cassetteZipMover":
                        //cassetteEntity = new CassetteZipMover(entityData, offset);
                        //break;
                }

                if(cassetteEntity != null)
                {
                    level.Add((Entity)cassetteEntity);
                    level.HasCassetteBlocks = true;
                    if (level.CassetteBlockTempo == 1f)
                    {
                        level.CassetteBlockTempo = cassetteEntity.Tempo;
                    }
                    level.CassetteBlockBeats = Math.Max(cassetteEntity.Index + 1, level.CassetteBlockBeats);
                    cassetteBlocksAdded = true;
                    return true;
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
            //On.Celeste.Player.RedDashCoroutine -= Player_RedDashCoroutine;
        }

    }
}