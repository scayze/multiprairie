using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerPrairieKing
{
    public sealed class ModConfig
    {
        public bool AchievementPrairieKingEnabled { get; set; } = true;
        public bool AchievementFectorsChallengeEnabled { get; set; } = true;
        public string Difficulty { get; set; } = "Normal";
    }
}
