﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Linq;
using StardewValley.Objects;
using HarmonyLib;


namespace MultiPlayerPrairie
{

    /// <summary>The mod entry point.</summary>
    public class ModMultiPlayerPrairieKing : Mod
    {


        public class GameLocationPatches
        {
            //Opens up Dialogue options when the player interacts with the arcade machine
            public static bool showPrairieKingMenu_Prefix()
            {
                //These Three options are taken from the game code, and would be available anyway
                string question = Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Menu");

                Response[] prairieKingOptions = new Response[4];
                prairieKingOptions[0] = new Response("Continue", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue"));
                prairieKingOptions[1] = new Response("NewGame", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame"));
                prairieKingOptions[3] = new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"));

                //Display additional Host/Join option depending on if theres a lobby available
                if(ModMultiPlayerPrairieKing.instance.isHostAvailable)
                    prairieKingOptions[2] = new Response("JoinMultiplayer", "Join Co-op Journey");
                else
                    prairieKingOptions[2] = new Response("HostMultiplayer", "Host Co-op Journey");

                //Create the dialogue
                Game1.currentLocation.createQuestionDialogue(question, prairieKingOptions, new GameLocation.afterQuestionBehavior(arcadeDialogueSet));

                //Always Skip original code
                return false;
            }

            // Callback for choosing from the dialogu options when interacting with the arcade machine
            static public void arcadeDialogueSet(Farmer who, string dialogue_id)
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
                        ModMultiPlayerPrairieKing.instance.isHost.Value = false;

                        //NET Join Lobby
                        PK_JoinLobby mJoinLobby = new PK_JoinLobby();
                        ModMultiPlayerPrairieKing.instance.Helper.Multiplayer.SendMessage(mJoinLobby, "PK_JoinLobby");

                        //Start Game
                        Game1.player.jotpkProgress.Value = null;
                        Game1.currentMinigame = new GameMultiplayerPrairieKing(ModMultiPlayerPrairieKing.instance, ModMultiPlayerPrairieKing.instance.isHost.Value, true);
                        break;
                    case "HostMultiplayer":
                        //When host is available
                        ModMultiPlayerPrairieKing.instance.isHost.Value = true;
                        ModMultiPlayerPrairieKing.instance.isHostAvailable = true;

                        //Open Waiting Dialogue
                        Response[] hostDialogueOptions = new Response[1];
                        hostDialogueOptions[0] = new Response("Cancel", "Cancel");

                        Game1.currentLocation.createQuestionDialogue("Waiting for other player to join...", hostDialogueOptions, new GameLocation.afterQuestionBehavior(hostDialogueSet));

                        //NET Start Hosting
                        PK_StartHosting mStartHosting = new PK_StartHosting();
                        ModMultiPlayerPrairieKing.instance.Helper.Multiplayer.SendMessage(mStartHosting, "PK_StartHosting");
                        break;
                }
            }
        }

        //The instance of the Mod
        public static ModMultiPlayerPrairieKing instance;

        // isHost is true for the player that is currently hosting the multiplayer lobby, false for all others
        public readonly PerScreen<bool> isHost = new PerScreen<bool>();

        // isHostAvailable is true when a player in the current multiplayer lobby is hosting a prairie king room
        public bool isHostAvailable = false;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this; 

            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTick;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;

            //Patch the showPrairieKingMenu method to show an additional "Host / Join co-op Journey" option.
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.GameLocation), nameof(StardewValley.GameLocation.showPrairieKingMenu)),
                prefix: new HarmonyMethod(typeof(GameLocationPatches), nameof(GameLocationPatches.showPrairieKingMenu_Prefix))
            );
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


            if (e.Button == SButton.L)
            {
                //Cast the minigame to GameMultiplayerPrairieKing 
                GameMultiplayerPrairieKing PK_game = (GameMultiplayerPrairieKing)Game1.currentMinigame;
                PK_game.NETskipLevel();
                //showArcadeDialogue();
            }

            if (e.Button == SButton.K)
            {
                if (Game1.currentMinigame != null)
                {
                    Game1.currentMinigame.unload();
                }
                Game1.currentMinigame = new GameMultiplayerPrairieKing(this, true, true);
            }


            if (e.Button == SButton.RightShoulder)
            {
                if (Game1.currentMinigame != null)
                {
                    Game1.currentMinigame.unload();
                }
                Game1.currentMinigame = new GameMultiplayerPrairieKing(this, false, true);
            }
        }

        static public void hostDialogueSet(Farmer who, string dialogue_id)
        {
            instance.Monitor.Log("Dialogue Chosen: " + dialogue_id, LogLevel.Debug);
            switch (dialogue_id)
            {
                case "Cancel":
                    instance.isHostAvailable = false;
                    //NET Stop Hosting
                    PK_StopHosting mStopHosting = new PK_StopHosting();
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
            switch(e.Type)
            {
                case "PK_StartHosting":
                    this.Monitor.Log(e.Type + " event sent by " + e.FromPlayerID + " to " + Game1.player.UniqueMultiplayerID, LogLevel.Debug);
                    PK_StartHosting mStartHosting = e.ReadAs<PK_StartHosting>();
                    isHostAvailable = true;

                    //Create Dialogue to wait for joining person
                    if (isHost.Value)
                    {

                    }
                    break;

                case "PK_StopHosting":
                    this.Monitor.Log(e.Type + " event sent by " + e.FromPlayerID + " to " + Game1.player.UniqueMultiplayerID, LogLevel.Debug);
                    PK_StopHosting mStopHosting = e.ReadAs<PK_StopHosting>();
                    isHostAvailable = false;
                    break;

                case "PK_JoinLobby":
                    this.Monitor.Log(e.Type + " event sent by " + e.FromPlayerID + " to " + Game1.player.UniqueMultiplayerID, LogLevel.Debug);
                    PK_JoinLobby mJoinLobby = e.ReadAs<PK_JoinLobby>();

                    Game1.player.jotpkProgress.Value = null;
                    Game1.currentMinigame = new GameMultiplayerPrairieKing(ModMultiPlayerPrairieKing.instance, ModMultiPlayerPrairieKing.instance.isHost.Value, true);
                    break;
            }
            //Throw away the events if player isnt playing the game
            if (Game1.currentMinigame == null)
            {
                return;
            }

            //Debug Log the Message
            if (!(e.Type == "PK_PlayerMove" || e.Type == "PK_EnemyPositions"))
            {
                this.Monitor.Log(e.Type + " event sent by " + e.FromPlayerID + " to " + Game1.player.UniqueMultiplayerID, LogLevel.Debug);
            }
            

            //Cast the minigame to GameMultiplayerPrairieKing 
            GameMultiplayerPrairieKing PK_game = (GameMultiplayerPrairieKing)Game1.currentMinigame;

            //Handle the remaining messages, about ingame events
            switch (e.Type)
            {
                case "PK_PowerupSpawn":
                    PK_PowerupSpawn mPowerupSpawn = e.ReadAs<PK_PowerupSpawn>();
                    //public CowboyPowerup(GameMultiplayerPrairieKing game, int which, Point position, int duration)
                    if (!PK_game.isHost)
                    {
                        GameMultiplayerPrairieKing.CowboyPowerup powerup = new GameMultiplayerPrairieKing.CowboyPowerup(PK_game, mPowerupSpawn.which, mPowerupSpawn.position, mPowerupSpawn.duration);
                        powerup.id = mPowerupSpawn.id;
                        PK_game.powerups.Add(powerup);
                    }
                    break;

                case "PK_PowerupPickup":
                    PK_PowerupPickup mPowerupPickup = e.ReadAs<PK_PowerupPickup>();

                    //Coins should be given both players as value
                    if (mPowerupPickup.which == GameMultiplayerPrairieKing.POWERUP_COIN)
                    {
                        PK_game.usePowerup(GameMultiplayerPrairieKing.POWERUP_COIN);
                    }
                    if (mPowerupPickup.which == GameMultiplayerPrairieKing.POWERUP_NICKEL)
                    {
                        PK_game.usePowerup(GameMultiplayerPrairieKing.POWERUP_NICKEL);
                    }
                    //Health should be given both players too
                    if (mPowerupPickup.which == GameMultiplayerPrairieKing.POWERUP_LIFE)
                    {
                        PK_game.usePowerup(GameMultiplayerPrairieKing.POWERUP_LIFE);
                    }

                    for (int i = PK_game.powerups.Count - 1; i >= 0; i--)
                    {
                        GameMultiplayerPrairieKing.CowboyPowerup m = PK_game.powerups[i];
                        if (PK_game.powerups[i].id == mPowerupPickup.id)
                        {
                            PK_game.powerups.RemoveAt(i);
                        }
                    }
                    break;

                case "PK_PlayerMove":
                    PK_PlayerMove mPlayerMove = e.ReadAs<PK_PlayerMove>();

                    PK_game.player2MovementDirections = mPlayerMove.movementDirections;
                    PK_game.player2ShootingDirections = mPlayerMove.shootingDirections;
                    PK_game.player2Position = mPlayerMove.position;
                    PK_game.player2BoundingBox.X = (int)PK_game.player2Position.X + GameMultiplayerPrairieKing.TileSize / 4;
                    PK_game.player2BoundingBox.Y = (int)PK_game.player2Position.Y + GameMultiplayerPrairieKing.TileSize / 4;
                    PK_game.player2BoundingBox.Width = GameMultiplayerPrairieKing.TileSize / 2;
                    PK_game.player2BoundingBox.Height = GameMultiplayerPrairieKing.TileSize / 2;

                    break;

                case "PK_PlayerDeath":
                    PK_PlayerDeath mPlayerDeath = e.ReadAs<PK_PlayerDeath>();
                    PK_game.playerDie();

                    break;

                case "PK_BulletSpawn":
                    PK_BulletSpawn mBulletSpawn = e.ReadAs<PK_BulletSpawn>();
                    GameMultiplayerPrairieKing.CowboyBullet bullet = new GameMultiplayerPrairieKing.CowboyBullet(PK_game, mBulletSpawn.position, mBulletSpawn.motion, mBulletSpawn.damage);

                    if (mBulletSpawn.isFriendly)
                        PK_game.bullets.Add(bullet);
                    else
                        PK_game.enemyBullets.Add(bullet);

                    break;

                case "PK_EnemySpawn":
                    PK_EnemySpawn mEnemySpawn = e.ReadAs<PK_EnemySpawn>();

                    if (mEnemySpawn.isOutlaw)
                    {
                        GameMultiplayerPrairieKing.Outlaw outlaw = new GameMultiplayerPrairieKing.Outlaw(PK_game, mEnemySpawn.position, mEnemySpawn.health);
                        outlaw.id = mEnemySpawn.id;
                        PK_game.monsters.Add(outlaw);
                    }
                    else if (mEnemySpawn.isDracula)
                    {
                        GameMultiplayerPrairieKing.Dracula dracula = new GameMultiplayerPrairieKing.Dracula(PK_game);
                        dracula.id = mEnemySpawn.id;
                        PK_game.monsters.Add(dracula);
                    }
                    else
                    {
                        GameMultiplayerPrairieKing.CowboyMonster cowbyMonster = new GameMultiplayerPrairieKing.CowboyMonster(PK_game, mEnemySpawn.which, mEnemySpawn.position);
                        cowbyMonster.id = mEnemySpawn.id;
                        PK_game.monsters.Add(cowbyMonster);
                    }
                    break;

                case "PK_EnemyPositions":
                    PK_EnemyPositions mEnemyPositions = e.ReadAs<PK_EnemyPositions>();

                    for (int i = PK_game.monsters.Count - 1; i >= 0; i--)
                    {
                        GameMultiplayerPrairieKing.CowboyMonster m = PK_game.monsters[i];
                        if (mEnemyPositions.positions.ContainsKey(m.id))
                        {
                            m.position.Location = mEnemyPositions.positions[m.id];
                        }
                        else
                        {
                            this.Monitor.Log("Entities position wasnt updated: " + m.id, LogLevel.Debug);
                            PK_game.monsters.RemoveAt(i);
                        }
                    }
                    break;

                case "PK_CompleteLevel":
                    PK_CompleteLevel mCompleteLevel = e.ReadAs<PK_CompleteLevel>();
                    PK_game.OnCompleteLevel();
                    break;

                case "PK_StartLevelTransition":
                    PK_StartLevelTransition mStartLevelTransition = e.ReadAs<PK_StartLevelTransition>();
                    PK_game.StartLevelTransition();
                    break;

                case "PK_EnemyKilled":
                    PK_EnemyKilled mEnemyKilled = e.ReadAs<PK_EnemyKilled>();

                    //Remove the killed monster
                    for (int i = PK_game.monsters.Count - 1; i >= 0; i--)
                    {
                        GameMultiplayerPrairieKing.CowboyMonster m = PK_game.monsters[i];
                        if (m.id == mEnemyKilled.id)
                        {
                            m.onDeath();
                            PK_game.monsters.RemoveAt(i);
                            PK_game.addGuts(m.position.Location, m.type);
                            Game1.playSound("Cowboy_monsterDie");
                        }
                    }
                    break;

                case "PK_BulletDespawned":
                    PK_BulletDespawned mBulletDespawned = e.ReadAs<PK_BulletDespawned>();

                    List<GameMultiplayerPrairieKing.CowboyBullet> bulletList = mBulletDespawned.isFriendly ? PK_game.bullets : PK_game.enemyBullets;
                    //Remove the killed monster
                    for (int i = bulletList.Count - 1; i >= 0; i--)
                    {
                        if (bulletList[i].id == mBulletDespawned.id)
                        {
                            bulletList.RemoveAt(i);
                        }
                    }
                    break;

                case "PK_ExitGame":
                    PK_ExitGame mExitGame = e.ReadAs<PK_ExitGame>();
                    PK_game.forceQuit();
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
        public long id = -69;
        public int which = -69;
        public int type = -69;
    }

    //Player Sync
    public class PK_PlayerMove
    {
        public long id = -69;
        public Vector2 position = Vector2.Zero;
        public Vector2 motion = Vector2.Zero;
        public List<int> shootingDirections;
        public List<int> movementDirections;
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
        public bool isFriendly = true;
    }


    //Enemy sync
    public class PK_EnemySpawn
    {
        public Point position = Point.Zero;
        public long id = -69;
        public int which = 0;
        public int health = 1;
        public bool isOutlaw = false;
        public bool isDracula = false;

    }

    public class PK_EnemyKilled
    {
        public long dick = 69; //Haha
        public long id = -69;
    }


    public class PK_EnemyPositions
    {
        public Dictionary<long, Point> positions;
    }

    //Level Sync
    public class PK_CompleteLevel
    {

    }

    public class PK_ExitGame
    {

    }


    public class PK_StartLevelTransition
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

    }
}