﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MultiplayerPrairieKing.Utility
{
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
        public int errorCode = -1;
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

    public class PK_RestartGame
    {

    }

    //Basic Lobby messages
    public class PK_StartHosting
    {
        public int arcadeMachine = -1; 
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
        public SaveState saveState;
        public int difficulty = (int)DIFFICULTY.NORMAL;
    }
}
