using BrokemiaHelper.Deco;
using Celeste.Mod;
using System.Collections.Generic;

namespace BrokemiaHelper {
    public class BrokemiaHelperSession : EverestModuleSession {
        public bool NeutralDeathPenalty { get; set; } = false;

        public Dictionary<string, List<Noodle.NoodleParams>> Noodles { get; set; } = new();
    }
}
