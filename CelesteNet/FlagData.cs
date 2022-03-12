using Celeste.Mod.CelesteNet;
using Celeste.Mod.CelesteNet.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokemiaHelper.CelesteNet {
    class FlagData : DataType<FlagData> {
        static FlagData() {
            DataID = "brokemiaHelperFlagData";
        }

        public DataPlayerInfo Player;

        public string flag;

        public bool value;

        public DateTime timestamp;

        public override MetaType[] GenerateMeta(DataContext ctx)
        => new MetaType[] {
            new MetaPlayerUpdate(Player)
        };

        public override void FixupMeta(DataContext ctx) {
            Player = Get<MetaPlayerUpdate>(ctx);
        }

        protected override void Read(CelesteNetBinaryReader reader) {
            flag = reader.ReadNetString();
            value = reader.ReadBoolean();
            timestamp = reader.ReadDateTime();
        }

        protected override void Write(CelesteNetBinaryWriter writer) {
            writer.WriteNetString(flag);
            writer.Write(value);
            writer.Write(timestamp);
        }
    }
}
