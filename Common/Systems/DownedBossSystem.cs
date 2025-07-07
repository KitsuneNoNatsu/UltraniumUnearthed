using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria;

namespace UltraniumUnearthed.Common.Systems
{
    public class DownedBossSystem : ModSystem
    {
        public static bool downedZephyrSquid = false;

        public override void ClearWorld()
        {
            downedZephyrSquid = false;
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (downedZephyrSquid)
            {
                tag["downedZephyrSquid"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedZephyrSquid = tag.ContainsKey("downedZephyrSquid");
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[1] = downedZephyrSquid;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedZephyrSquid = flags[1];
        }
    }
}
