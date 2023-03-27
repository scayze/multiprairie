using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using HarmonyLib;
using MultiplayerPrairieKing.Entities;
using MultiplayerPrairieKing.Entities.Enemies;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;
using Microsoft.Xna.Framework.Graphics;
using BasePlayer = MultiplayerPrairieKing.Entities.BasePlayer;
using MultiplayerPrairieKing;

namespace MultiPlayerPrairie
{

    /// <summary>The mod entry point.</summary>
    public class ModMultiPlayerPrairieKing : Mod
    {
        public ModConfig Config;

        public class GameLocationPatches
        {
            //Opens up Dialogue options when the player interacts with the arcade machine
            public static bool ShowPrairieKingMenu_Prefix()
            {
                instance.playerID.Value = Game1.player.UniqueMultiplayerID;
                //These Three options are taken from the game code, and would be available anyway
                string question = Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Menu");

                Response[] prairieKingOptions = new Response[4];
                prairieKingOptions[0] = new Response("Continue", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue"));
                prairieKingOptions[1] = new Response("NewGame", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame"));
                prairieKingOptions[3] = new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"));

                //Display additional Host/Join option depending on if theres a lobby available
                if(instance.isHostAvailable)
                    prairieKingOptions[2] = new Response("JoinMultiplayer", "Join Co-op Journey");
                else
                    prairieKingOptions[2] = new Response("HostMultiplayer", "Host Co-op Journey");

                //Create the dialogue
                Game1.currentLocation.createQuestionDialogue(question, prairieKingOptions, new GameLocation.afterQuestionBehavior(ArcadeDialogueSet));

                //Always Skip original code
                return false;
            }

            // Callback for choosing from the dialogue options when interacting with the arcade machine
            static public void ArcadeDialogueSet(Farmer who, string dialogue_id)
            {
                switch(dialogue_id)
                {
                    case "NewGame":
                        Game1.player.jotpkProgress.Value = null;
                        Game1.currentMinigame = new StardewValley.Minigames.AbigailGame();
                        break;
                    case "Continue":
                        Game1.currentMinigame = new StardewValley.Minigames.AbigailGame();
                        break;
                    case "JoinMultiplayer":
                        instance.isHost.Value = false;

                        //NET Join Lobby
                        PK_JoinLobby mJoinLobby = new();
                        mJoinLobby.playerId = instance.playerID.Value;
                        instance.Helper.Multiplayer.SendMessage(mJoinLobby, "PK_JoinLobby");

                        //Start Game
                        Game1.player.jotpkProgress.Value = null;
                        Game1.currentMinigame = new GameMultiplayerPrairieKing(instance, instance.isHost.Value, true);
                        break;
                    case "HostMultiplayer":
                        //When host is available
                        instance.isHost.Value = true;
                        instance.isHostAvailable = true;

                        instance.playerList.Clear();
                        instance.playerList.Add(instance.playerID.Value);

                        //NET Start Hosting
                        PK_StartHosting mStartHosting = new();
                        instance.Helper.Multiplayer.SendMessage(mStartHosting, "PK_StartHosting");

                        Game1.player.jotpkProgress.Value = null;
                        Game1.currentMinigame = new GameMultiplayerPrairieKing(ModMultiPlayerPrairieKing.instance, ModMultiPlayerPrairieKing.instance.isHost.Value, true);

                        break;
                }
            }
        }

        //The instance of the Mod
        public static ModMultiPlayerPrairieKing instance;

        // isHost is true for the player that is currently hosting the multiplayer lobby, false for all others
        public readonly PerScreen<bool> isHost = new();

        // isHostAvailable is true when a player in the current multiplayer lobby is hosting a prairie king room
        public bool isHostAvailable = false;

        public readonly PerScreen<long> playerID = new();

        public List<long> playerList = new();

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;

            //Generate random long because apparently multiplayer helper aint ready yet?

            //playerID = Helper.Multiplayer.GetNewID();

            Config = Helper.ReadConfig<ModConfig>();

            //Load custom texture for players
            BasePlayer.texture = helper.ModContent.Load<Texture2D>("assets/poppetjes.png");
            GameMultiplayerPrairieKing.shopBubbleTexture = helper.ModContent.Load<Texture2D>("assets/shopBubble.png");
            GameMultiplayerPrairieKing.startScreenTexture = helper.ModContent.Load<Texture2D>("assets/jotpk_start_screen.png");
            GameMultiplayerPrairieKing.startScreenPoppetjesTexture = helper.ModContent.Load<Texture2D>("assets/poppetjes_lobby.png");

            //Register to events
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTick;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;

            //Patch the showPrairieKingMenu method to show an additional "Host / Join co-op Journey" option.
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.showPrairieKingMenu)),
                prefix: new HarmonyMethod(typeof(GameLocationPatches), nameof(GameLocationPatches.ShowPrairieKingMenu_Prefix))
            );

            //Console commands
            helper.ConsoleCommands.Add("pk_SetStage", "Sets the new stage of prairie king.", this.SkipToStage);
            helper.ConsoleCommands.Add("pk_SetCoins", "Sets the amount of coins the player has in prairie king.", this.SetCoins);
            helper.ConsoleCommands.Add("pk_UsePowerup", "Use a defined powerup by id", this.UsePowerup);
            helper.ConsoleCommands.Add("pk_GoCrazy", "We ballin.", this.GoCrazy);
        }

        private void UsePowerup(string command, string[] args)
        {
            GameMultiplayerPrairieKing PK_game = (GameMultiplayerPrairieKing)Game1.currentMinigame;
            PK_game.UsePowerup((POWERUP_TYPE)int.Parse(args[0]));
        }


        private void GoCrazy(string command, string[] args)
        {
            GameMultiplayerPrairieKing PK_game = (GameMultiplayerPrairieKing)Game1.currentMinigame;
            PK_game.player.SetInvincible(int.MaxValue);
            PK_game.UsePowerup(POWERUP_TYPE.SHERRIFF);
            PK_game.activePowerups[0] = int.MaxValue;
        }

        private void SetCoins(string command, string[] args)
        {
            GameMultiplayerPrairieKing PK_game = (GameMultiplayerPrairieKing)Game1.currentMinigame;
            PK_game.Coins = int.Parse(args[0]);
        }

        private void SkipToStage(string command, string[] args)
        {
            GameMultiplayerPrairieKing PK_game = (GameMultiplayerPrairieKing)Game1.currentMinigame;
            PK_game.NETskipLevel(int.Parse(args[0]));
        }


        private void OnGameLaunched(object sencer, GameLaunchedEventArgs e)
        {
            isHostAvailable = false;
            isHost.Value = false;
        }

        private void OnSaveLoaded(object sencer, SaveLoadedEventArgs e)
        {
            isHostAvailable = false;
            isHost.Value = false;
        }


        private void OnButtonPressed(object sencer, ButtonPressedEventArgs e)
        {

        }

        static public void HostDialogueSet(Farmer who, string dialogue_id)
        {
            instance.Monitor.Log("Dialogue Chosen: " + dialogue_id, LogLevel.Info);
            switch (dialogue_id)
            {
                case "Cancel":
                    instance.isHostAvailable = false;
                    //NET Stop Hosting
                    PK_StopHosting mStopHosting = new();
                    instance.Helper.Multiplayer.SendMessage(mStopHosting, "PK_StopHosting");
                    instance.isHost.Value = false;
                    break;
            }
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTick(object sender, EventArgs e)
        {

        }

        public void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            //Handle Lobby messages
            switch (e.Type)
            {
                case "PK_StartHosting":
                    {
                        Monitor.Log(e.Type + " event sent by " + e.FromPlayerID + " to " + Game1.player.UniqueMultiplayerID, LogLevel.Debug);
                        isHostAvailable = true;
                        break;
                    }

                case "PK_StopHosting":
                    {
                        Monitor.Log(e.Type + " event sent by " + e.FromPlayerID + " to " + Game1.player.UniqueMultiplayerID, LogLevel.Debug);
                        isHostAvailable = false;

                        if (Game1.currentMinigame is GameMultiplayerPrairieKing PK_Game)
                        {
                            PK_Game.forceQuit();
                            Game1.currentMinigame = null;
                        }
                        break;
                    }

                case "PK_JoinLobby":
                    {
                        //Join Lobby is only relevant for host
                        if (!instance.isHost.Value) break;

                        //Add player to the lobby
                        PK_JoinLobby mJoinLobby = e.ReadAs<PK_JoinLobby>();
                        playerList.Add(mJoinLobby.playerId);

                        //Send the new lobby information to the rest of the gang
                        PK_LobbyInfo mLobbyInfoMessage = new();
                        mLobbyInfoMessage.playerList = playerList;

                        DIFFICULTY difficulty;
                        if (Config.Difficulty == "Easy") difficulty = DIFFICULTY.EASY;
                        else if (Config.Difficulty == "Normal") difficulty = DIFFICULTY.NORMAL;
                        else if (Config.Difficulty == "Hard") difficulty = DIFFICULTY.HARD;
                        else difficulty = DIFFICULTY.NORMAL;

                        mLobbyInfoMessage.difficulty = (int)difficulty;
                        Helper.Multiplayer.SendMessage(mLobbyInfoMessage, "PK_LobbyInfo");

                        if (Game1.currentMinigame is GameMultiplayerPrairieKing PK_Game)
                        {
                            PK_Game.difficulty = (DIFFICULTY)mLobbyInfoMessage.difficulty;
                        }

                        break;
                    }

                case "PK_LobbyInfo":
                    //Update playerList information
                    PK_LobbyInfo mLobbyInfo = e.ReadAs<PK_LobbyInfo>();
                    playerList = mLobbyInfo.playerList;

                    if (Game1.currentMinigame is GameMultiplayerPrairieKing)
                    {
                        GameMultiplayerPrairieKing currentGame = (GameMultiplayerPrairieKing)Game1.currentMinigame;
                        currentGame.difficulty = (DIFFICULTY)mLobbyInfo.difficulty;
                    }
                    break;
            }
            //Throw away the events if player isnt playing the game
            if (Game1.currentMinigame == null)
            {
                return;
            }

            //Cast the minigame to GameMultiplayerPrairieKing 
            GameMultiplayerPrairieKing PK_game = (GameMultiplayerPrairieKing)Game1.currentMinigame;

            //Handle the remaining messages, about ingame events
            switch (e.Type)
            {
                case "PK_PowerupSpawn":
                    PK_PowerupSpawn mPowerupSpawn = e.ReadAs<PK_PowerupSpawn>();
                    POWERUP_TYPE powerupType = (POWERUP_TYPE)mPowerupSpawn.which;

                    Powerup powerupSpawn = new(PK_game, powerupType, mPowerupSpawn.position, mPowerupSpawn.duration);
                    powerupSpawn.id = mPowerupSpawn.id;
                    PK_game.powerups.Add(powerupSpawn);

                    Monitor.Log(e.Type + " event, spawning " + powerupType.ToString() + " with id " +powerupSpawn.id, LogLevel.Debug);
                    break;

                case "PK_PowerupPickup":
                    PK_PowerupPickup mPowerupPickup = e.ReadAs<PK_PowerupPickup>();

                    for (int i = PK_game.powerups.Count - 1; i >= 0; i--)
                    {
                        Powerup powerup = PK_game.powerups[i];
                        if (powerup.id == mPowerupPickup.id)
                        {
                            PK_game.powerups.RemoveAt(i);
                        }
                    }
                    break;

                case "PK_UsePowerup":
                    PK_UsePowerup mPowerupUse = e.ReadAs<PK_UsePowerup>();
                    PK_game.UsePowerup((POWERUP_TYPE)mPowerupUse.type, mPowerupUse.playerId);
                    break;

                case "PK_BuyItem":
                    PK_BuyItem mBuyItem = e.ReadAs<PK_BuyItem>();
                    PK_game.playerList[mBuyItem.playerId].HoldItem((ITEM_TYPE)mBuyItem.type, 2500);
                    break;

                case "PK_PlayerMove":
                    PK_PlayerMove mPlayerMove = e.ReadAs<PK_PlayerMove>();

                    if (!PK_game.playerList.ContainsKey(mPlayerMove.playerId)) break;

                    BasePlayer basePlayer = PK_game.playerList[mPlayerMove.playerId];

                    basePlayer.movementDirections = mPlayerMove.movementDirections;
                    basePlayer.shootingDirections = mPlayerMove.shootingDirections;
                    basePlayer.position = mPlayerMove.position;
                    basePlayer.boundingBox.X = (int)basePlayer.position.X + TileSize / 4;
                    basePlayer.boundingBox.Y = (int)basePlayer.position.Y + TileSize / 4;
                    basePlayer.boundingBox.Width = TileSize / 2;
                    basePlayer.boundingBox.Height = TileSize / 2;
                    break;

                case "PK_PlayerDeath":
                    PK_game.PlayerDie();

                    break;

                case "PK_BulletSpawn":
                    PK_BulletSpawn mBulletSpawn = e.ReadAs<PK_BulletSpawn>();
                    Bullet bullet = new(PK_game, mBulletSpawn.isFriendly, false, mBulletSpawn.position, mBulletSpawn.motion, mBulletSpawn.damage);
                    bullet.id = mBulletSpawn.id;
                    PK_game.bullets.Add(bullet);
                    break;

                case "PK_BulletDespawned":
                    PK_BulletDespawned mBulletDespawned = e.ReadAs<PK_BulletDespawned>();

                    //Remove the despawned bullet
                    for (int i = PK_game.bullets.Count - 1; i >= 0; i--)
                    {
                        if (PK_game.bullets[i].id == mBulletDespawned.id)
                        {
                            PK_game.bullets.RemoveAt(i);
                        }
                    }

                    if (mBulletDespawned.monsterId == -69) break;

                    //Damage the enemy the bullet despawned on
                    for (int i = PK_game.monsters.Count - 1; i >= 0; i--)
                    {
                        if (PK_game.monsters[i].id == mBulletDespawned.monsterId)
                        {
                            PK_game.monsters[i].TakeDamage(mBulletDespawned.damage);
                        }
                    }

                    break;

                case "PK_EnemySpawn":
                    PK_EnemySpawn mEnemySpawn = e.ReadAs<PK_EnemySpawn>();
                    MONSTER_TYPE monsterType = (MONSTER_TYPE)mEnemySpawn.which;

                    if (mEnemySpawn.which == (int)MONSTER_TYPE.outlaw)
                    {
                        Outlaw outlaw = new(PK_game, mEnemySpawn.position);
                        outlaw.id = mEnemySpawn.id;
                        PK_game.monsters.Add(outlaw);
                    }
                    else if (mEnemySpawn.which == (int)MONSTER_TYPE.dracula)
                    {
                        Dracula dracula = new(PK_game);
                        dracula.id = mEnemySpawn.id;
                        PK_game.monsters.Add(dracula);
                    }
                    else
                    {
                        Enemy cowbyMonster = new(PK_game, monsterType, mEnemySpawn.position);
                        cowbyMonster.id = mEnemySpawn.id;
                        PK_game.monsters.Add(cowbyMonster);
                    }
                    break;

                case "PK_EnemyPositions":
                    PK_EnemyPositions mEnemyPositions = e.ReadAs<PK_EnemyPositions>();

                    for (int i = PK_game.monsters.Count - 1; i >= 0; i--)
                    {
                        Enemy m = PK_game.monsters[i];
                        if (mEnemyPositions.positions.ContainsKey(m.id))
                        {
                            m.position.Location = mEnemyPositions.positions[m.id];
                        }
                        else
                        {
                            Monitor.Log("Entities position wasnt updated: " + m.id, LogLevel.Debug);
                            PK_game.monsters.RemoveAt(i);
                        }
                    }
                    break;

                case "PK_SpikeyTransform":
                    PK_SpikeyTransform mSpikeyTransform = e.ReadAs<PK_SpikeyTransform>();

                    //manuallay transform the spikey. Kinda hacky but donT CARE
                    for (int i = PK_game.monsters.Count - 1; i >= 0; i--)
                    {
                        if (PK_game.monsters[i].id == mSpikeyTransform.id)
                        {
                            PK_game.monsters[i].SpikeyStartTransform();
                        }
                    }
                    break;

                case "PK_SpikeyNewTarget":
                    PK_SpikeyNewTarget mSpikeyNewTarget = e.ReadAs<PK_SpikeyNewTarget>();

                    //manuallay set spikey target. Kinda hacky but donT CARE
                    for (int i = PK_game.monsters.Count - 1; i >= 0; i--)
                    {
                        if (PK_game.monsters[i].id == mSpikeyNewTarget.id)
                        {
                            PK_game.monsters[i].targetPosition = mSpikeyNewTarget.target;
                        }
                    }
                    break;

                case "PK_CompleteLevel":
                    PK_CompleteLevel mCompletLevel = e.ReadAs<PK_CompleteLevel>();
                    PK_game.OnCompleteLevel(mCompletLevel.toLevel);
                    break;

                case "PK_StartLevelTransition":
                    PK_game.StartLevelTransition();
                    break;

                case "PK_StartNewGamePlus":
                    PK_game.StartNewRound();
                    break;

                case "PK_StartNewGame":
                    PK_game.onStartMenu = false;
                    PK_game.InstantiatePlayers();
                    Game1.playSound("Pickup_Coin15");
                    break;

                case "PK_EnemyKilled":
                    PK_EnemyKilled mEnemyKilled = e.ReadAs<PK_EnemyKilled>();

                    //Remove the killed monster
                    for (int i = PK_game.monsters.Count - 1; i >= 0; i--)
                    {
                        Enemy m = PK_game.monsters[i];
                        if (m.id == mEnemyKilled.id)
                        {
                            m.OnDeath();
                            PK_game.monsters.RemoveAt(i);
                            PK_game.AddGuts(m.position.Location, m.type);
                            Game1.playSound("Cowboy_monsterDie");

                            Monitor.Log("Monser killed by event: " + m.id, LogLevel.Debug);
                        }
                    }
                    break;

                case "PK_ExitGame":
                    PK_game.forceQuit();
                    Game1.currentMinigame = null;
                    isHostAvailable = false;
                    isHost.Value = false;
                    break;
            }

        }
    }


    //PowerupSync
    public class PK_PowerupSpawn
    {
        public long id = -69;
        public int which = -69;
        public Point position;
        public int duration;
    }
    public class PK_PowerupPickup
    {
        public long id = -1;
        public int which = -69;
        public int type = -69;
    }

    public class PK_UsePowerup
    {
        public int type = -69;
        public long playerId = -1;
    }

    public class PK_BuyItem
    {
        public int type = -69;
        public long playerId = -1;
        //TODO: player ID for multiple players?
    }

    //Player Sync
    public class PK_PlayerMove
    {
        public long id = -69;
        public Vector2 position = Vector2.Zero;
        public Vector2 motion = Vector2.Zero;
        public List<int> shootingDirections;
        public List<int> movementDirections;
        public long playerId = -1;
    }

    public class PK_PlayerDeath
    {
        public long id = -69;
    }


    //Bullet Sync
    public class PK_BulletSpawn
    {
        public long id = -69;
        public bool isFriendly = true;
        public Point position = Point.Zero;
        public Point motion = Point.Zero;
        public int damage = 1;
    }

    public class PK_BulletDespawned
    {
        public long id = -69;

        public long monsterId = -69;
        public int damage;
    }


    //Enemy sync
    public class PK_EnemySpawn
    {
        public Point position = Point.Zero;
        public long id = -69;
        public int which = 0;
    }

    public class PK_EnemyKilled
    {
        public long dick = 69; //Haha
        public long id = -69;
    }

    public class PK_SpikeyTransform
    {
        public long id = -69;
    }

    public class PK_SpikeyNewTarget
    {
        public long id = -69;
        public Point target;
    }

    public class PK_EnemyPositions
    {
        public Dictionary<long, Point> positions;
    }

    //Level Sync
    public class PK_CompleteLevel
    {
        public int toLevel = -1;
    }

    public class PK_ExitGame
    {

    }

    public class PK_StartLevelTransition
    {

    }

    public class PK_StartNewGamePlus
    {

    }

    public class PK_StartNewGame
    {

    }

    //Basic Lobby messages
    public class PK_StartHosting
    {

    }

    public class PK_StopHosting
    {

    }

    public class PK_JoinLobby
    {
        public long playerId = -1;
    }

    public class PK_LobbyInfo
    {
        public List<long> playerList;
        public int difficulty = (int)DIFFICULTY.NORMAL;
    }
}