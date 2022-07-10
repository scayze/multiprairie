using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace MultiPlayerPrairie
{
    class MachineMultiplayerPrairieKing
    {
        public MachineMultiplayerPrairieKing()
        {

        }

        public static void start(IModHelper helper)
        {
            //Game1.currentMinigame = new GameMultiplayerPrairieKing(true);
        }
        public static StardewValley.Object GetNew(StardewValley.Object alt)
        {
            if (Game1.bigCraftablesInformation.Values.Any(v => v.Contains("2048 Arcade Machine")))
            {
                var obj = new StardewValley.Object(Vector2.Zero, Game1.bigCraftablesInformation.FirstOrDefault(b => b.Value.Contains("2048 Arcade Machine")).Key, false);
                return obj;
            }

            return alt;
        }

    }
}