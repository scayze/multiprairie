using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerPrairieKing.Utility
{

    class PlayerSaveState
    {
        public int bulletDamage = 0;

        public int fireSpeedLevel = 0;

        public int ammoLevel = 0;

        public int spreadPistol = 0;

        public int runSpeedLevel = 0;

        public int heldItem = -100;
    }

    internal class SaveState
    {
        List<PlayerSaveState> playerSaveStates = new();

        public int lives = 0;

        public int coins = 0;

        public int score = 0;

        public bool died = false;

        public int whichRound = 0;

        public int whichWave = 0;

        public int world = 0;

        public int waveTimer = 0;

        public List<Vector2> monsterChances = new();


    }
}
