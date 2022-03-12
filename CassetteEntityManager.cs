using Celeste;
using Celeste.Mod.Meta;
using FMOD.Studio;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrokemiaHelper
{
    [Tracked(false)]
    public class CassetteEntityManager : Entity
    {
        protected FieldInfo currentIndexInfo = typeof(CassetteBlockManager).GetField("currentIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        // Just in case there's no other manager
        private int currIndex;

        protected int currentIndex
        {
            get {
                return manager == null ? currIndex : (int)currentIndexInfo.GetValue(manager);
            }
            set
            {
                if (manager == null)
                {
                    currIndex = value;
                }
                else
                {
                    currentIndexInfo.SetValue(manager, value);
                }
            }
        }

        protected FieldInfo beatTimerInfo = typeof(CassetteBlockManager).GetField("beatTimer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        // Just in case there's no other manager
        private float bTimer;
        // The beat timer last update
        private float oldBeatTimer = 0;

        protected float beatTimer
        {
            get
            {
                return manager == null ? bTimer : (float)beatTimerInfo.GetValue(manager);
            }
            set
            {
                if (manager == null)
                {
                    bTimer = value;
                }
                else
                {
                    beatTimerInfo.SetValue(manager, value);
                }
            }
        }

        protected FieldInfo beatIndexInfo = typeof(CassetteBlockManager).GetField("beatIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        // Just in case there's no other manager
        private int bIndex;

        protected int beatIndex
        {
            get
            {
                return manager == null ? bIndex : (int)beatIndexInfo.GetValue(manager);
            }
            set
            {
                if (manager == null)
                {
                    bIndex = value;
                }
                else
                {
                    beatIndexInfo.SetValue(manager, value);
                }
            }
        }

        private float tempoMult;

        private int leadBeats;

        private int maxBeat;

        private bool isLevelMusic;

        private int beatIndexOffset;

        private EventInstance sfx;

        private EventInstance snapshot;

        private int beatsPerTick;

        private int ticksPerSwap;

        private int beatIndexMax;

        protected CassetteBlockManager manager;

        public CassetteEntityManager()
        {
            base.Tag = Tags.Global;
            Add(new TransitionListener
            {
                OnOutBegin = delegate
                {
                    if (!SceneAs<Level>().HasCassetteBlocks)
                    {
                        RemoveSelf();
                    }
                    else
                    {
                        maxBeat = SceneAs<Level>().CassetteBlockBeats;
                        tempoMult = SceneAs<Level>().CassetteBlockTempo;
                    }
                }
            });
        }

        public override void Awake(Scene scene)
        {
            orig_Awake(scene);
            manager = scene.Tracker.GetEntity<CassetteBlockManager>();
            beatsPerTick = 4;
            ticksPerSwap = 2;
            beatIndexMax = 256;
            MapMetaCassetteModifier mapMetaCassetteModifier = AreaData.Get((base.Scene as Level).Session).GetMeta()?.CassetteModifier;
            if (mapMetaCassetteModifier != null)
            {
                tempoMult = mapMetaCassetteModifier.TempoMult;
                leadBeats = mapMetaCassetteModifier.LeadBeats;
                beatsPerTick = mapMetaCassetteModifier.BeatsPerTick;
                ticksPerSwap = mapMetaCassetteModifier.TicksPerSwap;
                maxBeat = mapMetaCassetteModifier.Blocks;
                beatIndexMax = mapMetaCassetteModifier.BeatsMax;
            }

        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (!isLevelMusic)
            {
                Audio.Stop(snapshot);
                Audio.Stop(sfx);
            }
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            if (!isLevelMusic)
            {
                Audio.Stop(snapshot);
                Audio.Stop(sfx);
            }
        }

        public override void Update()
        {
            base.Update();
            if (manager == null)
            {
                manager = Scene.Tracker.GetEntity<CassetteBlockManager>();
            }
            if (isLevelMusic)
            {
                sfx = Audio.CurrentMusicEventInstance;
            }
            if (sfx == null && !isLevelMusic)
            {
                string cassetteSong = AreaData.Areas[SceneAs<Level>().Session.Area.ID].CassetteSong;
                sfx = Audio.CreateInstance(cassetteSong);
                Audio.Play("event:/game/general/cassette_block_switch_2");
            }
            else
            {
                AdvanceMusic(Engine.DeltaTime * tempoMult);
            }
        }

        public void AdvanceMusic(float time)
        {
            if (manager == null)
                beatTimer += time;
            if (beatTimer < 355f / (678f * (float)Math.PI) && beatTimer >= oldBeatTimer)
            {
                oldBeatTimer = beatTimer;
                return;
            }
            if (manager == null)
            {
                beatTimer -= 355f / (678f * (float)Math.PI);
                beatIndex++;
                beatIndex %= beatIndexMax;
                if (beatIndex % (beatsPerTick * ticksPerSwap) == 0)
                {
                    currentIndex++;
                    currentIndex %= maxBeat;
                    SetActiveIndex(currentIndex);
                    if (!isLevelMusic)
                    {
                        Audio.Play("event:/game/general/cassette_block_switch_2");
                    }
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                }
                else
                {
                    if ((beatIndex + 1) % (beatsPerTick * ticksPerSwap) == 0)
                    {
                        SetWillActivate((currentIndex + 1) % maxBeat);
                    }
                    if (beatIndex % beatsPerTick == 0 && !isLevelMusic)
                    {
                        Audio.Play("event:/game/general/cassette_block_switch_1");
                    }
                }
            }
            else
            {
                if (beatIndex % (beatsPerTick * ticksPerSwap) == 0)
                {
                    SetActiveIndex(currentIndex);
                }
                else
                {
                    if ((beatIndex + 1) % (beatsPerTick * ticksPerSwap) == 0)
                    {
                        SetWillActivate((currentIndex + 1) % maxBeat);
                    }
                }
            }
            if (leadBeats > 0)
            {
                leadBeats--;
                if (leadBeats == 0)
                {
                    beatIndex = 0;
                    if (!isLevelMusic)
                    {
                        sfx.start();
                    }
                }
            }
            if (leadBeats <= 0 && manager == null)
            {
                sfx.setParameterValue("sixteenth_note", GetSixteenthNote());
            }
            oldBeatTimer = beatTimer;
        }

        public int GetSixteenthNote()
        {
            return (beatIndex + beatIndexOffset) % 256 + 1;
        }

        public List<Entity> CassetteEntitiesInScene()
        {
            List<Entity> cassetteEntities = new List<Entity>();
            foreach (Entity e in Scene.Entities)
            {
                if(e is CassetteEntity)
                {
                    cassetteEntities.Add(e);
                }
            }

            return cassetteEntities;
        }

        public void StopBlocks()
        {
            foreach (CassetteEntity entity in CassetteEntitiesInScene())
            {
                entity.Finish();
            }
            if (!isLevelMusic)
            {
                Audio.Stop(sfx);
            }
        }

        public void Finish()
        {
            if (!isLevelMusic)
            {
                Audio.Stop(snapshot);
            }
            RemoveSelf();
        }

        public void OnLevelStart()
        {
            manager = Scene.Tracker.GetEntity<CassetteBlockManager>();
            if(manager != null)
            {
                beatIndexOffset = 0;
                leadBeats = 16;
            }
            maxBeat = SceneAs<Level>().CassetteBlockBeats;
            tempoMult = SceneAs<Level>().CassetteBlockTempo;
            if (manager == null)
            {
                if (SceneAs<Level>().Session.Area.GetLevelSet() == "Celeste")
                {
                    if (beatIndex % 8 >= 5)
                    {
                        currentIndex = maxBeat - 2;
                    }
                    else
                    {
                        currentIndex = maxBeat - 1;
                    }
                }
                else
                {
                    currentIndex = maxBeat - 1 - beatIndex / beatsPerTick % maxBeat;
                }
            }
            SilentUpdateBlocks();
        }

        private void SilentUpdateBlocks()
        {
            foreach (CassetteEntity entity in CassetteEntitiesInScene())
            {
                if (entity.ID.Level == SceneAs<Level>().Session.Level)
                {
                    entity.SetActivatedSilently(entity.Index == currentIndex);
                }
            }
        }

        public void SetActiveIndex(int index)
        {
            foreach (CassetteEntity entity in CassetteEntitiesInScene())
            {
                entity.Activated = (entity.Index == index);
            }
        }

        public void SetWillActivate(int index)
        {
            foreach (CassetteEntity entity in CassetteEntitiesInScene())
            {
                if (entity.Index == index || entity.Activated)
                {
                    entity.WillToggle();
                }
            }
        }

        public void orig_Awake(Scene scene)
        {
            base.Awake(scene);
            isLevelMusic = (AreaData.Areas[SceneAs<Level>().Session.Area.ID].CassetteSong == null) || manager != null;
            if (isLevelMusic)
            {
                leadBeats = 0;
                beatIndexOffset = 5;
            }
            else
            {
                beatIndexOffset = 0;
                leadBeats = 16;
                snapshot = Audio.CreateSnapshot("snapshot:/music_mains_mute");
            }
            maxBeat = SceneAs<Level>().CassetteBlockBeats;
            tempoMult = SceneAs<Level>().CassetteBlockTempo;
        }
    }
}
