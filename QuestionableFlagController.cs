using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace BrokemiaHelper {
    [CustomEntity("BrokemiaHelper/questionableFlagController")]
    public class QuestionableFlagController : Entity {
        private static readonly string FLAG_PREFIX = "BrokemiaHelperFlag_";

        public enum QuestionableFlags {
            XNAFNA,
            VersionCeleste,
            VersionEverest,
            VersionEverestCeleste,
            VersionEverestFull,
            VersionEverestBuild,
            VersionEverestSuffix,
            VersionEverestTag,
            VersionEverestCommit,
            VersionBrokemiaHelper,
            SystemMemoryMB,
            PlayMode,
            Platform,
            UserName
        }

        private QuestionableFlags which;

        private bool onlyOnce;

        private EntityID id;

        public QuestionableFlagController(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
            this.id = id;
            which = data.Enum("which", QuestionableFlags.XNAFNA);
            onlyOnce = data.Bool("onlyOnce", false);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Level level = SceneAs<Level>();
            level.Session.SetFlag(GetFlag());
            if (onlyOnce) {
                level.Session.DoNotLoad.Add(id);
            }
            RemoveSelf();
        }

        private string GetFlag() {
            switch(which) {
                case QuestionableFlags.XNAFNA:
                    if(typeof(Game).Assembly.FullName.Contains("FNA")) {
                        return FLAG_PREFIX + "XNAFNA_FNA";
                    }
                    return FLAG_PREFIX + "XNAFNA_XNA";
                case QuestionableFlags.VersionCeleste:
                    return FLAG_PREFIX + "VersionCeleste_" + Celeste.Celeste.Instance.Version;
                case QuestionableFlags.VersionEverest:
                    return FLAG_PREFIX + "VersionEverest_" + Everest.Version;
                case QuestionableFlags.VersionEverestCeleste:
                    return FLAG_PREFIX + "VersionEverestCeleste_" + Everest.VersionCelesteString;
                case QuestionableFlags.VersionEverestFull:
                    return FLAG_PREFIX + "VersionEverestFull_" + Everest.VersionString;
                case QuestionableFlags.VersionEverestBuild:
                    return FLAG_PREFIX + "VersionEverestBuild_" + Everest.BuildString;
                case QuestionableFlags.VersionEverestSuffix:
                    return FLAG_PREFIX + "VersionEverestSuffix_" + Everest.VersionSuffix;
                case QuestionableFlags.VersionEverestTag:
                    return FLAG_PREFIX + "VersionEverestTag_" + Everest.VersionTag;
                case QuestionableFlags.VersionEverestCommit:
                    return FLAG_PREFIX + "VersionEverestCommit_" + Everest.VersionCommit;
                case QuestionableFlags.VersionBrokemiaHelper:
                    return FLAG_PREFIX + "VersionBrokemiaHelper_" + BrokemiaHelperModule.Instance.Metadata.Version;
                case QuestionableFlags.SystemMemoryMB:
                    return FLAG_PREFIX + "SystemMemoryMB_" + Everest.SystemMemoryMB;
                case QuestionableFlags.PlayMode:
                    return FLAG_PREFIX + "PlayMode_" + Celeste.Celeste.PlayMode;
                case QuestionableFlags.Platform:
                    return FLAG_PREFIX + "Platform_" + PlatformHelper.Current;
                case QuestionableFlags.UserName:
                    return FLAG_PREFIX + "UserName_" + Environment.UserName;
            }
            throw new Exception("Unsupported flag in QuestionableFlagController");
        }

    }
}
