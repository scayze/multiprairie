using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerPrairieKing;
using MultiplayerPrairieKing.Entities;
using MultiplayerPrairieKing.Entities.Enemies;
using MultiplayerPrairieKing.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MultiPlayerPrairie
{
    [XmlInclude(typeof(AbigailGame.JOTPKProgress))]
	[InstanceStatics]
	public class GameMultiplayerPrairieKing : IMinigame
	{
		public delegate void behaviorAfterMotionPause();

		public enum GameKeys
		{
			MoveLeft,
			MoveRight,
			MoveUp,
			MoveDown,
			ShootLeft,
			ShootRight,
			ShootUp,
			ShootDown,
			UsePowerup,
			SelectOption,
			Exit,
			MAX
		}

		public void NETskipLevel(int toLevel = -1)
        {
			//Cant skip level as non host, i think
			if (!isHost)
            {
				return;
            }

			foreach (Enemy monster in monsters)
            {
				PK_EnemyKilled enemyKilled = new();
				enemyKilled.id = monster.id;
				modInstance.Helper.Multiplayer.SendMessage(enemyKilled, "PK_EnemyKilled");
			}

			monsters.Clear();
			

			OnCompleteLevel(toLevel);
			PK_CompleteLevel completeLevel = new();
			completeLevel.toLevel = toLevel;
			modInstance.Helper.Multiplayer.SendMessage(completeLevel, "PK_CompleteLevel");
        }

        public void NETspawnBullet(bool friendly, Point position, Point motion, int damage)
		{
			Bullet bullet = new(this, friendly, true, position, motion, damage);
			bullets.Add(bullet);

			//NET Spawn Bullet
			PK_BulletSpawn mBullet = new();
			mBullet.id = bullet.id;
			mBullet.position = position;
			mBullet.motion = motion;
			mBullet.damage = damage;
			mBullet.isFriendly = friendly;
			modInstance.Helper.Multiplayer.SendMessage(mBullet, "PK_BulletSpawn");
		}

		public void NETspawnBullet(bool friendly, Point position, int direction, int damage)
		{
			Bullet bullet = new(this, friendly, true, position, direction, damage);
			bullets.Add(bullet);

			//NET Spawn Bullet
			PK_BulletSpawn mBullet = new();
			mBullet.id = bullet.id;
			mBullet.position = position;
			mBullet.motion = bullet.motion;
			mBullet.damage = damage;
			mBullet.isFriendly = friendly;
			modInstance.Helper.Multiplayer.SendMessage(mBullet, "PK_BulletSpawn");
		}

		public void NETmovePlayer(Vector2 pos)
        {
			PK_PlayerMove message = new();
			message.playerId = modInstance.playerID.Value;
			message.position = pos;
			message.id = Game1.player.UniqueMultiplayerID;
			message.shootingDirections = player.shootingDirections;
			message.movementDirections = player.movementDirections;
			modInstance.Helper.Multiplayer.SendMessage(message, "PK_PlayerMove");
        }

		public void NETspawnPowerup(POWERUP_TYPE type, Point position, int duration)
        {
			Powerup powerup = new(this,type,position,duration);
			powerups.Add(powerup);

			PK_PowerupSpawn mPowerup = new();
			mPowerup.id = powerup.id;
			mPowerup.position = position;
			mPowerup.which = (int)type;
			mPowerup.duration = duration;
			modInstance.Helper.Multiplayer.SendMessage(mPowerup, "PK_PowerupSpawn");

			modInstance.Monitor.Log("spawning " + mPowerup.which.ToString() + " with id " + mPowerup.id, LogLevel.Debug);
		}

		public ModMultiPlayerPrairieKing modInstance;

		public bool isHost;

		public const int mapWidth = 16;

		public const int mapHeight = 16;

		public const int pixelZoom = 3;

		public const double lootChance = 0.05;

		public const double coinChance = 0.05;

		public int lootDuration = 7500;

		readonly int powerupDuration = 10000;

		public const float playerSpeed = 3f;

		public const int baseTileSize = 16;

		public const int cactusDanceDelay = 800;

		public const int playerMotionDelay = 100;

		public const int playerFootStepDelay = 200;

		public const int deathDelay = 3000;

		public static Texture2D shopBubbleTexture;
		public static Texture2D startScreenTexture;
		public static Texture2D startScreenPoppetjesTexture;

		public DIFFICULTY difficulty = DIFFICULTY.NORMAL;

		public enum MONSTER_TYPE
		{
			orc = 0,
			ghost = 1,
			ogre = 2,
			mummy = 3,
			devil = 4,
			mushroom = 5,
			spikey = 6,
			dracula = -2,
			outlaw = -1
		}

		public enum DIFFICULTY
        {
			EASY = 0,
			NORMAL = 1,
			HARD = 2
        }

		public enum MAP_TYPE {
			desert = 0,
			woods = 2,
			graveyard = 1
		}

		public enum POWERUP_TYPE
        {
			LOG = -1,
			SKULL = -2,
			COIN = 0,
			NICKEL = 1,
			SPREAD = 2,
			RAPIDFIRE = 3,
			NUKE = 4,
			ZOMBIE = 5,
			SPEED = 6,
			SHOTGUN = 7,
			LIFE = 8,
			TELEPORT = 9,
			SHERRIFF = 10,
			HEART = -3
		}

		public enum ITEM_TYPE {
			NONE = -1,
			FIRESPEED1 = 0,
			FIRESPEED2 = 1,
			FIRESPEED3 = 2,
			RUNSPEED1 = 3,
			RUNSPEED2 = 4,
			LIFE = 5,
			AMMO1 = 6,
			AMMO2 = 7,
			AMMO3 = 8,
			SPREADPISTOL = 9,
			STAR = 10,
			SKULL = 11,
			LOG = 12,
			FINISHED_GAME = 13
		}

		public enum OPTION_TYPE
        {
			RETRY = 0,
			QUIT = 1
		}

		public Dictionary<long, BasePlayer> playerList = new();

		public Player player;

		public int newGamePlus;

		bool spreadPistol;

		public const int waveDuration = 80000;

		public const int betweenWaveDuration = 5000;

		public List<Enemy> monsters = new();

		public Rectangle merchantBox;

		public Rectangle noPickUpBox;

		int motionPause;

		int lives = 3;

        public int Coins { get; set; }

        int score;

		public List<Bullet> bullets = new();

		public MAP_TILE[,] map = new MAP_TILE[16, 16];

		MAP_TILE[,] nextMap = new MAP_TILE[16, 16];

		public class SpawnTask
        {
			public MONSTER_TYPE type = MONSTER_TYPE.orc;
			public int Y = 0;

			public SpawnTask()
            {

            }

			public SpawnTask(MONSTER_TYPE type, int y)
            {
				this.type = type;
				this.Y = y;
            }
        }

		List<SpawnTask>[] spawnQueue = new List<SpawnTask>[4];

		public static Vector2 topLeftScreenCoordinate;

		float cactusDanceTimer;

		public behaviorAfterMotionPause behaviorAfterPause;

		List<Vector2> monsterChances = new()
		{
			new Vector2(0.014f, 0.4f),
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero,
			Vector2.Zero
		};

		public Rectangle shoppingCarpetNoPickup;

		public Dictionary<POWERUP_TYPE, int> activePowerups = new();

		public List<Powerup> powerups = new();

		public List<TemporaryAnimatedSprite> temporarySprites = new();

		public Powerup heldItem;

		public MAP_TYPE world = MAP_TYPE.desert;

		int gameOverOption;

		int gamerestartTimer;

		int waveTimer = 80000;

		int betweenWaveTimer = 5000;

		public int whichWave;

		public int monsterConfusionTimer;

		public int zombieModeTimer;

		int shoppingTimer;

		int newMapPosition;

		public int screenFlash;

		int gopherTrainPosition;

		int endCutsceneTimer;

		int endCutscenePhase;

		public bool onStartMenu;

		bool shopping;

		public bool gopherRunning;

		bool merchantArriving;

		bool merchantShopOpen;

		bool waitingForPlayerToMoveDownAMap;

		public bool scrollingMap;

		bool hasGopherAppeared;

		public bool shootoutLevel;

		public bool gopherTrain;

		bool playerJumped;

		bool endCutscene;

		bool gameOver;

		readonly Dictionary<Rectangle, ITEM_TYPE> storeItems = new();

		bool quit;

		bool died;

		public Rectangle gopherBox;

		Point gopherMotion;

		public static ICue overworldSong;

		public static ICue outlawSong;

		public static ICue zombieSong;

		protected HashSet<GameKeys> _buttonHeldState = new();

		protected Dictionary<GameKeys, int> _buttonHeldFrames;

		public static int TileSize => 48;

        public bool LoadGame()
		{
			if (Game1.player.jotpkProgress.Value == null)
			{
				modInstance.Monitor.Log("couldnt find savefile lol");
				return false;
			}
			modInstance.Monitor.Log("FOUND SAVEFILE WEJO");
			AbigailGame.JOTPKProgress save_data = Game1.player.jotpkProgress.Value;
			player.ammoLevel = save_data.ammoLevel.Value;
			player.runSpeedLevel = save_data.runSpeedLevel.Value;
			player.fireSpeedLevel = save_data.fireSpeedLevel.Value;
			player.bulletDamage = save_data.bulletDamage.Value;
			Coins = save_data.coins.Value;
			died = save_data.died.Value;
			lives = save_data.lives.Value;
			score = save_data.score.Value;
			spreadPistol = save_data.spreadPistol.Value;
			newGamePlus = save_data.whichRound.Value;
			whichWave = save_data.whichWave.Value;
			waveTimer = save_data.waveTimer.Value;
			world = (MAP_TYPE)save_data.world.Value;
			if (save_data.heldItem.Value != -100)
			{
				heldItem = new Powerup(this, (POWERUP_TYPE)save_data.heldItem.Value, Point.Zero, 9999);
			}
			monsterChances = new List<Vector2>(save_data.monsterChances);
			ApplyLevelSpecificStates();
			if (shootoutLevel)
			{
				player.position = new Vector2(8 * TileSize, 3 * TileSize);
			}
			return true;
		}

		public void SaveGame()
		{
			if (Game1.player.jotpkProgress.Value == null)
			{
				Game1.player.jotpkProgress.Value = new AbigailGame.JOTPKProgress();
			}
			AbigailGame.JOTPKProgress save_data = Game1.player.jotpkProgress.Value;
			save_data.ammoLevel.Value = player.ammoLevel;
			save_data.runSpeedLevel.Value = player.runSpeedLevel;
			save_data.fireSpeedLevel.Value = player.fireSpeedLevel;
			save_data.bulletDamage.Value = player.bulletDamage;
			save_data.coins.Value = Coins;
			save_data.died.Value = died;
			save_data.lives.Value = lives;
			save_data.score.Value = score;
			save_data.spreadPistol.Value = spreadPistol;
			save_data.whichRound.Value = newGamePlus;
			save_data.whichWave.Value = whichWave;
			save_data.waveTimer.Value = waveTimer;
			save_data.world.Value = (int)world;
			save_data.monsterChances.Clear();
			save_data.monsterChances.AddRange(monsterChances);
			if (heldItem == null)
			{
				save_data.heldItem.Value = -100;
			}
			else
			{
				save_data.heldItem.Value = (int)heldItem.which;
			}
		}

		public GameMultiplayerPrairieKing(ModMultiPlayerPrairieKing mod, bool isHost, bool playingWithAbby = false)
		{
			this.modInstance = mod;
			this.isHost = isHost;
			Reset();

			/*
			if (LoadGame())
			{
				map = MapLoader.GetMap(whichWave);
			}
			*/
		}

		public void InstantiatePlayers()
        {
			//Let players start in a square
			List<Vector2> startPositions = new List<Vector2>()
			{
				new Vector2(352f, 352f) + new Vector2(0,0),
				new Vector2(352f, 352f) + new Vector2(64,0),
				new Vector2(352f, 352f) + new Vector2(0,64),
				new Vector2(352f, 352f) + new Vector2(64,64),
			};

			playerList.Clear();
			for(int i=0; i<modInstance.playerList.Count; i++)
			{
				long pid = modInstance.playerList[i];

				BasePlayer p;
				if (pid == modInstance.playerID.Value)
				{
					player = new Player(this);
					p = player;
				}
				else
				{
					p = new PlayerSlave(this);
				}
				p.textureBase = new Vector2(i * 64, 0);
				p.position = startPositions[i];
				playerList.Add(pid, p);
			}

			//NET Player Move
			NETmovePlayer(player.position);

			player.boundingBox.X = (int)player.position.X + TileSize / 4;
			player.boundingBox.Y = (int)player.position.Y + TileSize / 4;
			player.boundingBox.Width = TileSize / 2;
			player.boundingBox.Height = TileSize / 2;
		}

		public void ApplyNewGamePlus()
		{
			topLeftScreenCoordinate = new Vector2(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 384);
			merchantArriving = false;
			merchantShopOpen = false;
			monsterConfusionTimer = 0;
			monsters.Clear();
			newMapPosition = 16 * TileSize;
			scrollingMap = false;
			shopping = false;
			temporarySprites.Clear();
			waitingForPlayerToMoveDownAMap = false;
			waveTimer = 80000;
			whichWave = 0;
			zombieModeTimer = 0;
			shootoutLevel = false;
			betweenWaveTimer = 5000;
			gopherRunning = false;
			hasGopherAppeared = false;
			outlawSong = null;
			overworldSong = null;
			endCutscene = false;
			endCutscenePhase = 0;
			endCutsceneTimer = 0;
			gameOver = false;
			onStartMenu = false;
			powerups.Clear();
			world = 0;
			Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.MiniGame);
			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					if ((x == 0 || x == 15 || y == 0 || y == 15) && (x <= 6 || x >= 10) && (y <= 6 || y >= 10))
					{
						map[x, y] = MAP_TILE.CACTUS;
					}
					else if (x == 0 || x == 15 || y == 0 || y == 15)
					{
						map[x, y] = (MAP_TILE)((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (x == 1 || x == 14 || y == 1 || y == 14)
					{
						map[x, y] = MAP_TILE.GRAVEL;
					}
					else
					{
						map[x, y] = (MAP_TILE)((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}

			//NET Player Move
			player.position = new Vector2(384f, 384f);
			NETmovePlayer(player.position);

			player.boundingBox.X = (int)player.position.X + TileSize / 4;
			player.boundingBox.Y = (int)player.position.Y + TileSize / 4;
			player.boundingBox.Width = TileSize / 2;
			player.boundingBox.Height = TileSize / 2;

			betweenWaveTimer += 1500;

			for (int j = 0; j < 4; j++)
			{
				spawnQueue[j] = new List<SpawnTask>();
			}
			noPickUpBox = new Rectangle(0, 0, TileSize, TileSize);
			merchantBox = new Rectangle(8 * TileSize, 0, TileSize, TileSize);
			newMapPosition = 16 * TileSize;

			//Actual apply new game plus
			monsterChances[0] = new Vector2(0.014f + newGamePlus * 0.005f, 0.41f + newGamePlus * 0.05f);
			monsterChances[4] = new Vector2(0.002f, 0.1f);
		}

		public void Reset()
		{

			died = false;
			topLeftScreenCoordinate = new Vector2(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 384);
			merchantArriving = false;
			merchantShopOpen = false;
			monsterConfusionTimer = 0;
			monsters.Clear();
			newMapPosition = 16 * TileSize;
			scrollingMap = false;
			shopping = false;
			temporarySprites.Clear();
			waitingForPlayerToMoveDownAMap = false;
			waveTimer = 80000;
			whichWave = 0;
			zombieModeTimer = 0;
			shootoutLevel = false;
			betweenWaveTimer = 5000;
			gopherRunning = false;
			hasGopherAppeared = false;
			playerList.Clear();
			outlawSong = null;
			overworldSong = null;
			endCutscene = false;
			endCutscenePhase = 0;
			endCutsceneTimer = 0;
			gameOver = false;
			onStartMenu = true;
			powerups.Clear();
			world = 0;
			Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.MiniGame);
			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					if ((x == 0 || x == 15 || y == 0 || y == 15) && (x <= 6 || x >= 10) && (y <= 6 || y >= 10))
					{
						map[x, y] = MAP_TILE.CACTUS;
					}
					else if (x == 0 || x == 15 || y == 0 || y == 15)
					{
						map[x, y] = (MAP_TILE)((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (x == 1 || x == 14 || y == 1 || y == 14)
					{
						map[x, y] = MAP_TILE.GRAVEL;
					}
					else
					{
						map[x, y] = (MAP_TILE)((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}

			betweenWaveTimer += 1500;

			for (int j = 0; j < 4; j++)
			{
				spawnQueue[j] = new List<SpawnTask>();
			}
			noPickUpBox = new Rectangle(0, 0, TileSize, TileSize);
			merchantBox = new Rectangle(8 * TileSize, 0, TileSize, TileSize);
			newMapPosition = 16 * TileSize;
		}

		public static float GetMovementSpeed(float speed, int directions)
		{
			float movementSpeed = speed;
			if (directions > 1)
			{
				movementSpeed = Math.Max(1, (int)Math.Sqrt(2f * (movementSpeed * movementSpeed)) / 2);
			}
			return movementSpeed;
		}

		public bool PickupPowerup(Powerup c)
		{
			switch (c.which)
			{
				case POWERUP_TYPE.HEART:
					UsePowerup(POWERUP_TYPE.HEART);
					break;
				case POWERUP_TYPE.SKULL:
					UsePowerup(POWERUP_TYPE.SKULL);
					break;
				case POWERUP_TYPE.LOG:
					UsePowerup(POWERUP_TYPE.LOG);
					break;
				case POWERUP_TYPE.COIN:
					UsePowerup(POWERUP_TYPE.COIN);
					break;
				case POWERUP_TYPE.NICKEL:
					UsePowerup(POWERUP_TYPE.NICKEL);
					break;
				case POWERUP_TYPE.LIFE:
					UsePowerup(POWERUP_TYPE.LIFE);
					break;
				default:
					{
						if (heldItem == null)
						{
							heldItem = c;
							Game1.playSound("cowboy_powerup");
							break;
						}
						Powerup tmp = heldItem;
						heldItem = c;
						noPickUpBox.Location = c.position;
						tmp.position = c.position;
						powerups.Add(tmp);
						Game1.playSound("cowboy_powerup");
						return true;
					}
			}
			return true;
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public void UsePowerup(POWERUP_TYPE which, long pid = -1)
		{
			//If not visual only (aka sync call for player 2), send network message
			if (pid == -1)
			{
				PK_UsePowerup mUsePowerup = new();
				mUsePowerup.playerId = modInstance.playerID.Value;
				mUsePowerup.type = (int)which;
				modInstance.Helper.Multiplayer.SendMessage(mUsePowerup, "PK_UsePowerup");
			}

			if (activePowerups.ContainsKey(which))
			{
				activePowerups[which] = powerupDuration + 2000;
				return;
			}

			switch (which)
			{
				case POWERUP_TYPE.HEART:
					foreach(BasePlayer p in playerList.Values)
                    {
						p.HoldItem(ITEM_TYPE.FINISHED_GAME, 4000);
					}
					
					Game1.playSound("Cowboy_Secret");
					endCutscene = true;
					endCutsceneTimer = 4000;
					world = 0;
					if (!Game1.player.hasOrWillReceiveMail("Beat_PK"))
					{
						Game1.addMailForTomorrow("Beat_PK");
					}
					break;

				case POWERUP_TYPE.SKULL:
					StartGopherTrain(ITEM_TYPE.SKULL);
					break;

				case POWERUP_TYPE.LOG:
					StartGopherTrain(ITEM_TYPE.LOG);
					break;

				case POWERUP_TYPE.SHERRIFF:
					if (pid != -1) break;
					UsePowerup(POWERUP_TYPE.SHOTGUN);
					UsePowerup(POWERUP_TYPE.RAPIDFIRE);
					UsePowerup(POWERUP_TYPE.SPEED);
					for (int j = 0; j < activePowerups.Count; j++)
					{
						activePowerups[activePowerups.ElementAt(j).Key] *= 2;
					}
					break;

				case POWERUP_TYPE.ZOMBIE:
					if (overworldSong != null && overworldSong.IsPlaying)
					{
						overworldSong.Stop(AudioStopOptions.Immediate);
					}
					if (zombieSong != null && zombieSong.IsPlaying)
					{
						zombieSong.Stop(AudioStopOptions.Immediate);
						zombieSong = null;
					}
					zombieSong = Game1.soundBank.GetCue("Cowboy_undead");
					zombieSong.Play();
					motionPause = 1800;
					zombieModeTimer = 10000;
					break;

				case POWERUP_TYPE.TELEPORT:

					monsterConfusionTimer = 4000;

					if (pid != -1)
					{
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, playerList[pid].position + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
						break;
					}

					Point teleportSpot = Point.Zero;
					int tries = 0;
					while ((Math.Abs(teleportSpot.X - player.position.X) < 8f || Math.Abs(teleportSpot.Y - player.position.Y) < 8f || IsCollidingWithMap(teleportSpot) || IsCollidingWithMonster(new Rectangle(teleportSpot.X, teleportSpot.Y, TileSize, TileSize), null)) && tries < 10)
					{
						teleportSpot = new Point(Game1.random.Next(TileSize, 16 * TileSize - TileSize), Game1.random.Next(TileSize, 16 * TileSize - TileSize));
						tries++;
					}

					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, player.position + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X, teleportSpot.Y) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X - TileSize / 2, teleportSpot.Y) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 200
					});
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X + TileSize / 2, teleportSpot.Y) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 400
					});

					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X, teleportSpot.Y - TileSize / 2) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 600
					});
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X, teleportSpot.Y + TileSize / 2) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 800
					});

					if (tries < 10)
					{
						player.position = new Vector2(teleportSpot.X, teleportSpot.Y);

						//NET Player Move
						NETmovePlayer(player.position);

						player.SetInvincible(4000);
						Game1.playSound("cowboy_powerup");
					}
					break;

				case POWERUP_TYPE.LIFE:
					lives++;
					Game1.playSound("cowboy_powerup");
					break;
				case POWERUP_TYPE.NUKE:
					Game1.playSound("cowboy_explosion");
					//Spawn the little explosion things yk yes
					for (int i = 0; i < 30; i++)
					{
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(Game1.random.Next(1, 16), Game1.random.Next(1, 16)) * TileSize + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
						{
							delayBeforeAnimationStart = Game1.random.Next(800)
						});
					}
					

					if (!shootoutLevel)
					{
						foreach (Enemy e in monsters)
						{
							AddGuts(e.position.Location, e.type);

							if (pid != -1) continue;

							PK_EnemyKilled mEnemyKilled = new();
							mEnemyKilled.id = e.id;
							modInstance.Helper.Multiplayer.SendMessage(mEnemyKilled, "PK_EnemyKilled");
						}

						if (pid == -1) monsters.Clear();
					}
					else
					{
						if (pid != -1) break;
						foreach (Enemy c in monsters)
						{
							c.TakeDamage(30);
							NETspawnBullet(true, c.position.Center, 2, 1);
						}
					}
					break;

				case POWERUP_TYPE.SPREAD:
				case POWERUP_TYPE.RAPIDFIRE:
				case POWERUP_TYPE.SHOTGUN:
					if (pid != -1) break;
					Game1.playSound("cowboy_gunload");
					activePowerups.Add(which, powerupDuration + 2000);
					break;
				case POWERUP_TYPE.COIN:
					Coins++;
					Game1.playSound("Pickup_Coin15");
					break;
				case POWERUP_TYPE.NICKEL:
					Coins += 5;
					Game1.playSound("Pickup_Coin15");
					Game1.playSound("Pickup_Coin15");
					break;
				default:
					if (pid != -1) break;
					activePowerups.Add(which, powerupDuration);
					Game1.playSound("cowboy_powerup");
					break;
			}
			if (newGamePlus > 0 && activePowerups.ContainsKey(which))
			{
				activePowerups[which] /= 2;
			}
		}

		public void StartGopherTrain(ITEM_TYPE item = ITEM_TYPE.NONE)
        {
			foreach (BasePlayer p in playerList.Values)
			{
				p.HoldItem(item, 2000);
			}

			Game1.playSound("Cowboy_Secret");
			gopherTrain = true;
			gopherTrainPosition = -TileSize * 2;
		}

		public void AddGuts(Point position, MONSTER_TYPE whichGuts)
		{
			switch (whichGuts)
			{
				case MONSTER_TYPE.orc:
				case MONSTER_TYPE.ogre:
				case MONSTER_TYPE.mushroom:
				case MONSTER_TYPE.spikey:
				case MONSTER_TYPE.dracula:
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(512, 1696, 16, 16), 80f, 6, 0, topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 1696, 16, 16), 10000f, 1, 0, topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
					{
						delayBeforeAnimationStart = 480
					});
					break;
				case MONSTER_TYPE.mummy:
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
					break;
				case MONSTER_TYPE.ghost:
				case MONSTER_TYPE.devil:
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(544, 1728, 16, 16), 80f, 4, 0, topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
					break;
			}
		}

		public void EndOfGopherAnimationBehavior2(int extraInfo)
		{
			Game1.playSound("cowboy_gopher");
			if (Math.Abs(gopherBox.X - 8 * TileSize) > Math.Abs(gopherBox.Y - 8 * TileSize))
			{
				if (gopherBox.X > 8 * TileSize)
				{
					gopherMotion = new Point(-2, 0);
				}
				else
				{
					gopherMotion = new Point(2, 0);
				}
			}
			else if (gopherBox.Y > 8 * TileSize)
			{
				gopherMotion = new Point(0, -2);
			}
			else
			{
				gopherMotion = new Point(0, 2);
			}
			gopherRunning = true;
		}

		public void EndOfGopherAnimationBehavior(int extrainfo)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(384, 1792, 16, 16), 120f, 4, 2, topLeftScreenCoordinate + new Vector2(gopherBox.X + TileSize / 2, gopherBox.Y + TileSize / 2), flicker: false, flipped: false, (float)gopherBox.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				endFunction = EndOfGopherAnimationBehavior2
			});
			Game1.playSound("cowboy_gopher");
		}

		public void UpdateBullets(GameTime time)
		{
			for (int m = bullets.Count - 1; m >= 0; m--)
			{
				bullets[m].Update();
				if(bullets[m].queuedForDeletion)
                {
					//NET Despawn Bullet
					PK_BulletDespawned mBulletDespawned = new();
					mBulletDespawned.id = bullets[m].id;
					modInstance.Helper.Multiplayer.SendMessage(mBulletDespawned, "PK_BulletDespawned");
					bullets.RemoveAt(m);
				}
			}
		}

		public void PlayerDie()
		{
			gopherRunning = false;
			hasGopherAppeared = false;
			spawnQueue = new List<SpawnTask>[4];
			for (int i = 0; i < 4; i++)
			{
				spawnQueue[i] = new List<SpawnTask>();
			}

			//enemyBullets.Clear(); Still needed? Might even look cool, and not scary because of invincibility timer

			if (!shootoutLevel)
			{
				powerups.Clear();
				monsters.Clear();
			}
			died = true;
			activePowerups.Clear();

			if (overworldSong != null && overworldSong.IsPlaying)
			{
				overworldSong.Stop(AudioStopOptions.Immediate);
			}
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, player.position + topLeftScreenCoordinate, flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
			waveTimer = Math.Min(80000, waveTimer + 10000);
			betweenWaveTimer = 4000;
			lives--;

			foreach(BasePlayer p in playerList.Values)
            {
				p.Die();
            }

			if (lives < 0)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 550f, 5, 0, player.position + topLeftScreenCoordinate, flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0.001f,
					endFunction = AfterPlayerDeathFunction
				});

				foreach (BasePlayer p in playerList.Values)
				{
					p.deathTimer *= 3f;
				}

				Game1.player.jotpkProgress.Value = null;
			}
			else if (!shootoutLevel)
			{
				SaveGame();
			}
		}

		public void AfterPlayerDeathFunction(int extra)
		{
			if (lives < 0)
			{
				gameOver = true;
				if (overworldSong != null && !overworldSong.IsPlaying)
				{
					overworldSong.Stop(AudioStopOptions.Immediate);
				}
				if (outlawSong != null && !outlawSong.IsPlaying)
				{
					overworldSong.Stop(AudioStopOptions.Immediate);
				}
				monsters.Clear();
				powerups.Clear();
				died = false;
				Game1.playSound("Cowboy_monsterDie");
			}
		}

		public void StartNewRound()
		{
			gamerestartTimer = 2000;
			Game1.playSound("Cowboy_monsterDie");
			newGamePlus++;
			//ApplyNewGamePlus();
		}

		public void StartLevelTransition()
        {
			SaveGame();
			shopping = false;
			merchantArriving = false;
			merchantShopOpen = false;
			merchantBox.Y = -TileSize;
			scrollingMap = true;
			nextMap = MapLoader.GetMap(whichWave);
			newMapPosition = 16 * TileSize;
			temporarySprites.Clear();
			powerups.Clear();
		}

		protected void UpdateInput()
		{
			if (Game1.options.gamepadControls)
			{
				GamePadState pad_state = Game1.input.GetGamePadState();
				ButtonCollection button_collection = new(ref pad_state);
				if (pad_state.ThumbSticks.Left.X < -0.2)
				{
					_buttonHeldState.Add(GameKeys.MoveLeft);
				}
				if (pad_state.ThumbSticks.Left.X > 0.2)
				{
					_buttonHeldState.Add(GameKeys.MoveRight);
				}
				if (pad_state.ThumbSticks.Left.Y < -0.2)
				{
					_buttonHeldState.Add(GameKeys.MoveDown);
				}
				if (pad_state.ThumbSticks.Left.Y > 0.2)
				{
					_buttonHeldState.Add(GameKeys.MoveUp);
				}
				if (pad_state.ThumbSticks.Right.X < -0.2)
				{
					_buttonHeldState.Add(GameKeys.ShootLeft);
				}
				if (pad_state.ThumbSticks.Right.X > 0.2)
				{
					_buttonHeldState.Add(GameKeys.ShootRight);
				}
				if (pad_state.ThumbSticks.Right.Y < -0.2)
				{
					_buttonHeldState.Add(GameKeys.ShootDown);
				}
				if (pad_state.ThumbSticks.Right.Y > 0.2)
				{
					_buttonHeldState.Add(GameKeys.ShootUp);
				}
				ButtonCollection.ButtonEnumerator enumerator = button_collection.GetEnumerator();
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
						case Buttons.A:
							if (gameOver)
							{
								_buttonHeldState.Add(GameKeys.SelectOption);
							}
							else if (true)//Program.sdk.IsEnterButtonAssignmentFlipped) TODO: Figure this out
							{
								_buttonHeldState.Add(GameKeys.ShootRight);
							}
							else
							{
								_buttonHeldState.Add(GameKeys.ShootDown);
							}
							break;
						case Buttons.Y:
							_buttonHeldState.Add(GameKeys.ShootUp);
							break;
						case Buttons.X:
							_buttonHeldState.Add(GameKeys.ShootLeft);
							break;
						case Buttons.B:
							if (gameOver)
							{
								_buttonHeldState.Add(GameKeys.Exit);
							}
							else if (true) //Program.sdk.IsEnterButtonAssignmentFlipped) TODO: Figure this out
							{
								_buttonHeldState.Add(GameKeys.ShootDown);
							}
							else
							{
								_buttonHeldState.Add(GameKeys.ShootRight);
							}
							break;
						case Buttons.DPadUp:
							_buttonHeldState.Add(GameKeys.MoveUp);
							break;
						case Buttons.DPadDown:
							_buttonHeldState.Add(GameKeys.MoveDown);
							break;
						case Buttons.DPadLeft:
							_buttonHeldState.Add(GameKeys.MoveLeft);
							break;
						case Buttons.DPadRight:
							_buttonHeldState.Add(GameKeys.MoveRight);
							break;
						case Buttons.Start:
						case Buttons.LeftShoulder:
						case Buttons.RightShoulder:
						case Buttons.RightTrigger:
						case Buttons.LeftTrigger:
							_buttonHeldState.Add(GameKeys.UsePowerup);
							break;
						case Buttons.Back:
							_buttonHeldState.Add(GameKeys.Exit);
							break;
					}
				}
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.W))
			{
				_buttonHeldState.Add(GameKeys.MoveUp);
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.S))
			{
				_buttonHeldState.Add(GameKeys.MoveDown);
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.A))
			{
				_buttonHeldState.Add(GameKeys.MoveLeft);
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.D))
			{
				_buttonHeldState.Add(GameKeys.MoveRight);
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Up))
			{
				if (gameOver)
				{
					_buttonHeldState.Add(GameKeys.MoveUp);
				}
				else
				{
					_buttonHeldState.Add(GameKeys.ShootUp);
				}
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Down))
			{
				if (gameOver)
				{
					_buttonHeldState.Add(GameKeys.MoveDown);
				}
				else
				{
					_buttonHeldState.Add(GameKeys.ShootDown);
				}
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Left))
			{
				_buttonHeldState.Add(GameKeys.ShootLeft);
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Right))
			{
				_buttonHeldState.Add(GameKeys.ShootRight);
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.X) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Enter) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Space))
			{
				if (gameOver)
				{
					_buttonHeldState.Add(GameKeys.SelectOption);
				}
				else
				{
					_buttonHeldState.Add(GameKeys.UsePowerup);
				}
			}
			if (Game1.input.GetKeyboardState().IsKeyDown(Keys.Escape))
			{
				_buttonHeldState.Add(GameKeys.Exit);
			}
		}
		public bool tick(GameTime time)
		{
			Game1.gameTimeInterval = 0;
			if (_buttonHeldFrames == null)
			{
				_buttonHeldFrames = new Dictionary<GameKeys, int>();
				for (int k = 0; k < 11; k++)
				{
					_buttonHeldFrames[(GameKeys)k] = 0;
				}
			}
			_buttonHeldState.Clear();
			UpdateInput();

			for (int l = 0; l < 11; l++)
			{
				if (_buttonHeldState.Contains((GameKeys)l))
				{
					_buttonHeldFrames[(GameKeys)l]++;
				}
				else
				{
					_buttonHeldFrames[(GameKeys)l] = 0;
				}
			}

			ProcessInputs();

			if (quit)
			{
				Game1.stopMusicTrack(Game1.MusicContext.MiniGame);
				return true;
			}
			if (gameOver)
			{
				//startTimer = 0;
				return false;
			}
			if (onStartMenu)
			{
				return false;
			}
			if (gamerestartTimer > 0)
			{
				gamerestartTimer -= time.ElapsedGameTime.Milliseconds;
				if (gamerestartTimer <= 0)
				{
					if (newGamePlus == 0 || !endCutscene)
					{
						unload();
						Game1.currentMinigame = new GameMultiplayerPrairieKing(modInstance, isHost);
					}
					else
					{
						ApplyNewGamePlus();
					}
				}
			}
			if (endCutscene)
			{
				endCutsceneTimer -= time.ElapsedGameTime.Milliseconds;
				if (endCutsceneTimer < 0)
				{
					endCutscenePhase++;
					if (endCutscenePhase > 5)
					{
						endCutscenePhase = 5;
					}
					switch (endCutscenePhase)
					{
						case 1:
							if(modInstance.Config.AchievementPrairieKingEnabled)
                            {
								Game1.getSteamAchievement("Achievement_PrairieKing");
							}
								
							if (!died && modInstance.Config.AchievementFectorsChallengeEnabled)
							{
								Game1.getSteamAchievement("Achievement_FectorsChallenge");
							}
							//Game1.multiplayer.globalChatInfoMessage("PrairieKing", Game1.player.Name);
							// TODO: Replacement with SMAPI?

							endCutsceneTimer = 15500;
							Game1.playSound("Cowboy_singing");
							map = MapLoader.GetMap(-1);
							break;
						case 2:
							player.position = new Vector2(0f, 8 * TileSize);

							//NET Player Move
							NETmovePlayer(player.position);

							endCutsceneTimer = 12000;
							break;
						case 3:
							endCutsceneTimer = 5000;
							break;
						case 4:
							endCutsceneTimer = 1000;
							break;
						case 5:
							if (Game1.input.GetKeyboardState().GetPressedKeys().Length == 0)
							{
								Game1.input.GetGamePadState();
								if (Game1.input.GetGamePadState().Buttons.X != ButtonState.Pressed && Game1.input.GetGamePadState().Buttons.Start != ButtonState.Pressed && Game1.input.GetGamePadState().Buttons.A != ButtonState.Pressed)
								{
									break;
								}
							}
							if (gamerestartTimer <= 0)
							{
								if(isHost)
                                {
									StartNewRound();
									PK_StartNewGamePlus message = new();
									modInstance.Helper.Multiplayer.SendMessage(message, "PK_StartNewGamePlus");
								}
								
							}
							break;
					}
				}
				if (endCutscenePhase == 2 && player.position.X < (float)(9 * TileSize))
				{
					player.position.X += 1f;
					player.motionAnimationTimer += time.ElapsedGameTime.Milliseconds;
					player.motionAnimationTimer %= 400;
				}
				return false;
			}
			if (motionPause > 0)
			{
				motionPause -= time.ElapsedGameTime.Milliseconds;
				if (motionPause <= 0 && behaviorAfterPause != null)
				{
					behaviorAfterPause();
					behaviorAfterPause = null;
				}
			}
			else if (monsterConfusionTimer > 0)
			{
				monsterConfusionTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (zombieModeTimer > 0)
			{
				zombieModeTimer -= time.ElapsedGameTime.Milliseconds;
			}

			//TODO tick really here?
			foreach(BasePlayer p in playerList.Values)
            {
				p.Tick(time);
            }

			//Dont stop if the player is still holding up the item?
			if (player.IsHoldingItem()) return false;

			
			//Screen flash timer
			if (screenFlash > 0)
			{
				screenFlash -= time.ElapsedGameTime.Milliseconds;
			}

			//Weird gopher train shit
			if (gopherTrain)
			{
				gopherTrainPosition += 3;
				if (gopherTrainPosition % 30 == 0)
				{
					Game1.playSound("Cowboy_Footstep");
				}
				if (playerJumped)
				{
					player.position.Y += 3f;
				}
				if (Math.Abs(player.position.Y - (float)(gopherTrainPosition - TileSize)) <= 16f)
				{
					playerJumped = true;
					player.position.Y = gopherTrainPosition - TileSize;
				}
				if (gopherTrainPosition > 16 * TileSize + TileSize)
				{
					gopherTrain = false;
					playerJumped = false;
					whichWave++;
					map = MapLoader.GetMap(whichWave);
					player.position = new Vector2(8 * TileSize, 8 * TileSize);

					//NET Player Move
					NETmovePlayer(player.position);

					world = ((world != MAP_TYPE.desert) ? MAP_TYPE.graveyard : MAP_TYPE.woods);
					waveTimer = 80000;
					betweenWaveTimer = 5000;
					waitingForPlayerToMoveDownAMap = false;
					shootoutLevel = false;
					SaveGame();
				}
			}

			// Shopping lady moving to her place
			if ((shopping || merchantArriving || waitingForPlayerToMoveDownAMap) && !player.IsHoldingItem())
			{
				int oldTimer = shoppingTimer;
				shoppingTimer += time.ElapsedGameTime.Milliseconds;
				shoppingTimer %= 500;
				if (!merchantShopOpen && merchantArriving && shopping && ((oldTimer < 250 && shoppingTimer >= 250) || oldTimer > shoppingTimer))
				{
					Game1.playSound("Cowboy_Footstep");
				}
			}

			//Move palyers along when moving down to the next map
			if (scrollingMap)
			{
				newMapPosition -= TileSize / 8;

				foreach (BasePlayer p in playerList.Values)
				{
					p.position.Y -= (float)TileSize / 8;
					p.position.Y += 3f;
					p.boundingBox.X = (int)player.position.X + TileSize / 4;
					p.boundingBox.Y = (int)player.position.Y + TileSize / 4;
					p.boundingBox.Width = TileSize / 2;
					p.boundingBox.Height = TileSize / 2;
					p.movementDirections = new List<int> { 2 };
					p.motionAnimationTimer += time.ElapsedGameTime.Milliseconds;
					p.motionAnimationTimer %= 400;
				}

				//Swap to the next map once the map is loaded
				if (newMapPosition <= 0)
				{
					scrollingMap = false;
					map = nextMap;
					newMapPosition = 16 * TileSize;
					shopping = false;
					betweenWaveTimer = 5000;
					waitingForPlayerToMoveDownAMap = false;
					player.movementDirections.Clear();
					ApplyLevelSpecificStates();
				}
			}
			if (gopherRunning)
			{
				gopherBox.X += gopherMotion.X;
				gopherBox.Y += gopherMotion.Y;
				for (int m = monsters.Count - 1; m >= 0; m--)
				{
					if (gopherBox.Intersects(monsters[m].position))
					{
						//Net EnemyKilled
						PK_EnemyKilled message = new();
						message.id = monsters[m].id;
						modInstance.Helper.Multiplayer.SendMessage(message, "PK_EnemyKilled");

						AddGuts(monsters[m].position.Location, monsters[m].type);
						monsters.RemoveAt(m);
						Game1.playSound("Cowboy_monsterDie");
					}
				}
				if (gopherBox.X < 0 || gopherBox.Y < 0 || gopherBox.X > 16 * TileSize || gopherBox.Y > 16 * TileSize)
				{
					gopherRunning = false;
				}
			}
			for (int n = temporarySprites.Count - 1; n >= 0; n--)
			{
				if (temporarySprites[n].update(time))
				{
					temporarySprites.RemoveAt(n);
				}
			}
			if (motionPause <= 0)
			{
				for (int i = powerups.Count - 1; i >= 0; i--)
				{
					if (Utility.distance(player.boundingBox.Center.X, powerups[i].position.X + TileSize / 2, player.boundingBox.Center.Y, powerups[i].position.Y + TileSize / 2) <= (float)(TileSize + 3) && (powerups[i].position.X < TileSize || powerups[i].position.X >= 16 * TileSize - TileSize || powerups[i].position.Y < TileSize || powerups[i].position.Y >= 16 * TileSize - TileSize))
					{
						if (powerups[i].position.X + TileSize / 2 < player.boundingBox.Center.X)
						{
							powerups[i].position.X++;
						}
						if (powerups[i].position.X + TileSize / 2 > player.boundingBox.Center.X)
						{
							powerups[i].position.X--;
						}
						if (powerups[i].position.Y + TileSize / 2 < player.boundingBox.Center.Y)
						{
							powerups[i].position.Y++;
						}
						if (powerups[i].position.Y + TileSize / 2 > player.boundingBox.Center.Y)
						{
							powerups[i].position.Y--;
						}
					}
					powerups[i].duration -= time.ElapsedGameTime.Milliseconds;
					if (powerups[i].duration <= 0)
					{
						powerups.RemoveAt(i);
					}
				}
				for (int i = activePowerups.Count - 1; i >= 0; i--)
				{
					activePowerups[activePowerups.ElementAt(i).Key] -= time.ElapsedGameTime.Milliseconds;
					if (activePowerups[activePowerups.ElementAt(i).Key] <= 0)
					{
						activePowerups.Remove(activePowerups.ElementAt(i).Key);
					}
				}

				//As Host, move down to the level if the level is finished and both players are at the bottom of the screen
				if (waitingForPlayerToMoveDownAMap && isHost)
				{
					float bottomBorder = 16 * TileSize - TileSize / 2;
					bool readyToRumble = true;
					foreach(BasePlayer p in playerList.Values)
                    {
						if (p.boundingBox.Bottom < bottomBorder)
						{
							readyToRumble = false;
						}
					}

					if(readyToRumble)
                    {
						PK_StartLevelTransition message = new();
						modInstance.Helper.Multiplayer.SendMessage(message, "PK_StartLevelTransition"); 
						StartLevelTransition();
					}
				}
				if (shopping)
				{
					if (merchantBox.Y < 8 * TileSize - TileSize * 3 && merchantArriving)
					{
						merchantBox.Y += 2;
						if (merchantBox.Y >= 8 * TileSize - TileSize * 3)
						{
							merchantShopOpen = true;
							merchantArriving = false;
							Game1.playSound("cowboy_monsterhit");
							map[7, 15] = MAP_TILE.SAND;
							map[8, 15] = MAP_TILE.SAND;
							map[9, 15] = MAP_TILE.SAND;
							map[7, 14] = MAP_TILE.SAND;
							map[8, 14] = MAP_TILE.SAND;
							map[9, 14] = MAP_TILE.SAND;
							shoppingCarpetNoPickup = new Rectangle(merchantBox.X - TileSize, merchantBox.Y + TileSize, TileSize * 3, TileSize * 2);
						}
					}
					else if (merchantShopOpen)
					{
						for (int i = storeItems.Count - 1; i >= 0; i--)
						{
							if (!player.boundingBox.Intersects(shoppingCarpetNoPickup) && player.boundingBox.Intersects(storeItems.ElementAt(i).Key) && Coins >= GetPriceForItem(storeItems.ElementAt(i).Value))
							{
								Game1.playSound("Cowboy_Secret");
								motionPause = 2500;
								ITEM_TYPE boughtItem = storeItems.ElementAt(i).Value;
								Coins -= GetPriceForItem(boughtItem);

								player.HoldItem(boughtItem, 2500);

								merchantArriving = false;
								merchantShopOpen = false;

								PK_BuyItem message = new();
								message.playerId = modInstance.playerID.Value;
								message.type = (int)boughtItem;
								modInstance.Helper.Multiplayer.SendMessage(message, "PK_BuyItem");

								switch (boughtItem)
								{
									case ITEM_TYPE.AMMO1:
									case ITEM_TYPE.AMMO2:
									case ITEM_TYPE.AMMO3:
										player.ammoLevel++;
										player.bulletDamage++;
										break;
									case ITEM_TYPE.FIRESPEED1:
									case ITEM_TYPE.FIRESPEED2:
									case ITEM_TYPE.FIRESPEED3:
										player.fireSpeedLevel++;
										break;
									case ITEM_TYPE.RUNSPEED1:
									case ITEM_TYPE.RUNSPEED2:
										player.runSpeedLevel++;
										break;
									case ITEM_TYPE.LIFE:
										lives++;
										break;
									case ITEM_TYPE.SPREADPISTOL:
										spreadPistol = true;
										break;
									case ITEM_TYPE.STAR:
										heldItem = new Powerup(this, POWERUP_TYPE.SHERRIFF, Point.Zero, 9999);
										break;
								}
							}
						}
					}
				}
				cactusDanceTimer += time.ElapsedGameTime.Milliseconds;
				cactusDanceTimer %= 1600f;

				UpdateBullets(time);

				//Update powerups
				foreach (Powerup powerup in powerups)
				{
					powerup.Tick(time);
				}

				if (waveTimer > 0 && betweenWaveTimer <= 0 && zombieModeTimer <= 0 && !shootoutLevel && (overworldSong == null || !overworldSong.IsPlaying) && Game1.soundBank != null)
				{
					overworldSong = Game1.soundBank.GetCue("Cowboy_OVERWORLD");
					overworldSong.Play();
					Game1.musicPlayerVolume = Game1.options.musicVolumeLevel;
					Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
				}
				if (player.deathTimer > 0f)
				{
					player.deathTimer -= time.ElapsedGameTime.Milliseconds;
				}
				if (betweenWaveTimer > 0 && monsters.Count == 0 && IsSpawnQueueEmpty() && !shopping && !waitingForPlayerToMoveDownAMap)
				{
					betweenWaveTimer -= time.ElapsedGameTime.Milliseconds;
				}
				else if (player.deathTimer <= 0f && !waitingForPlayerToMoveDownAMap && !shopping && !shootoutLevel)
				{
					if (waveTimer > 0)
					{
						int oldWaveTimer = waveTimer;
						waveTimer -= time.ElapsedGameTime.Milliseconds;

						int u = 0;
						foreach (Vector2 v in monsterChances)
						{
							if (Game1.random.NextDouble() < (v.X * ((monsters.Count != 0) ? 1 : 2)))
							{
								int numMonsters = 1;
								while (Game1.random.NextDouble() < v.Y && numMonsters < 15)
								{
									numMonsters++;
								}
								spawnQueue[(whichWave == 11) ? (Game1.random.Next(1, 3) * 2 - 1) : Game1.random.Next(4)].Add(new SpawnTask((MONSTER_TYPE)u, numMonsters));
							}
							u++;
						}
						if (!hasGopherAppeared && monsters.Count > 6 && Game1.random.NextDouble() < 0.0004 && waveTimer > 7000 && waveTimer < 50000)
						{
							hasGopherAppeared = true;
							gopherBox = new Rectangle(Game1.random.Next(16 * TileSize), Game1.random.Next(16 * TileSize), TileSize, TileSize);
							int tries2 = 0;
							while ((IsCollidingWithMap(gopherBox) || IsCollidingWithMonster(gopherBox, null) || Math.Abs(gopherBox.X - player.position.X) < (TileSize * 6) || Math.Abs(gopherBox.Y - player.position.Y) < (float)(TileSize * 6) || Math.Abs(gopherBox.X - 8 * TileSize) < TileSize * 4 || Math.Abs(gopherBox.Y - 8 * TileSize) < TileSize * 4) && tries2 < 10)
							{
								gopherBox.X = Game1.random.Next(16 * TileSize);
								gopherBox.Y = Game1.random.Next(16 * TileSize);
								tries2++;
							}
							if (tries2 < 10)
							{
								temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(256, 1664, 16, 32), 80f, 5, 0, topLeftScreenCoordinate + new Vector2(gopherBox.X + TileSize / 2, gopherBox.Y - TileSize + TileSize / 2), flicker: false, flipped: false, (float)gopherBox.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
								{
									endFunction = EndOfGopherAnimationBehavior
								});
							}
						}
					}
					for (int p = 0; p < 4; p++)
					{
						if (spawnQueue[p].Count <= 0)
						{
							continue;
						}
						if (spawnQueue[p][0].type == MONSTER_TYPE.ghost || spawnQueue[p][0].type == MONSTER_TYPE.devil)
						{
							List<Vector2> border = Utility.getBorderOfThisRectangle(new Rectangle(0, 0, 16, 16));
							Vector2 tile = border.ElementAt(Game1.random.Next(border.Count));
							int tries = 0;
							while (IsCollidingWithMonster(new Rectangle((int)tile.X * TileSize, (int)tile.Y * TileSize, TileSize, TileSize), null) && tries < 10)
							{
								tile = border.ElementAt(Game1.random.Next(border.Count));
								tries++;
							}
							if (tries < 10)
							{
								if(isHost)
									monsters.Add(new Enemy(this,spawnQueue[p][0].type, new Point((int)tile.X * TileSize, (int)tile.Y * TileSize)));

								spawnQueue[p][0] = new SpawnTask(spawnQueue[p][0].type, spawnQueue[p][0].Y - 1);
								if (spawnQueue[p][0].Y <= 0)
								{
									spawnQueue[p].RemoveAt(0);
								}
							}
							continue;
						}
						switch (p)
						{
							case 0:
								{
									for (int x = 7; x < 10; x++)
									{
										if (Game1.random.NextDouble() < 0.5 && !IsCollidingWithMonster(new Rectangle(x * 16 * 3, 0, 48, 48), null))
										{
											if (isHost)
												monsters.Add(new Enemy(this, spawnQueue[p].First().type, new Point(x * TileSize, 0)));

											spawnQueue[p][0] = new SpawnTask(spawnQueue[p][0].type, spawnQueue[p][0].Y - 1);
											if (spawnQueue[p][0].Y <= 0)
											{
												spawnQueue[p].RemoveAt(0);
											}
											break;
										}
									}
									break;
								}
							case 1:
								{
									for (int y = 7; y < 10; y++)
									{
										if (Game1.random.NextDouble() < 0.5 && !IsCollidingWithMonster(new Rectangle(720, y * TileSize, 48, 48), null))
										{
											if (isHost)
												monsters.Add(new Enemy(this,spawnQueue[p].First().type, new Point(15 * TileSize, y * TileSize)));

											spawnQueue[p][0] = new SpawnTask(spawnQueue[p][0].type, spawnQueue[p][0].Y - 1);
											if (spawnQueue[p][0].Y <= 0)
											{
												spawnQueue[p].RemoveAt(0);
											}
											break;
										}
									}
									break;
								}
							case 2:
								{
									for (int x2 = 7; x2 < 10; x2++)
									{
										if (Game1.random.NextDouble() < 0.5 && !IsCollidingWithMonster(new Rectangle(x2 * 16 * 3, 15 * TileSize, 48, 48), null))
										{
											if (isHost)
												monsters.Add(new Enemy(this,spawnQueue[p].First().type, new Point(x2 * TileSize, 15 * TileSize)));

											spawnQueue[p][0] = new SpawnTask(spawnQueue[p][0].type, spawnQueue[p][0].Y - 1);
											if (spawnQueue[p][0].Y <= 0)
											{
												spawnQueue[p].RemoveAt(0);
											}
											break;
										}
									}
									break;
								}
							case 3:
								{
									for (int y2 = 7; y2 < 10; y2++)
									{
										if (Game1.random.NextDouble() < 0.5 && !IsCollidingWithMonster(new Rectangle(0, y2 * TileSize, 48, 48), null))
										{
											if (isHost)
												monsters.Add(new Enemy(this,spawnQueue[p].First().type, new Point(0, y2 * TileSize)));

											spawnQueue[p][0] = new SpawnTask(spawnQueue[p][0].type, spawnQueue[p][0].Y - 1);
											if (spawnQueue[p][0].Y <= 0)
											{
												spawnQueue[p].RemoveAt(0);
											}
											break;
										}
									}
									break;
								}
						}
					}
					if (waveTimer <= 0 && monsters.Count > 0 && IsSpawnQueueEmpty())
					{
						bool onlySpikeys = true;
						foreach (Enemy monster in monsters)
						{
							if (monster.type != MONSTER_TYPE.spikey)
							{
								onlySpikeys = false;
								break;
							}
						}
						if (onlySpikeys)
						{
							foreach (Enemy monster2 in monsters)
							{
								monster2.health = 1;
							}
						}
					}

					//If finished the level
					if (waveTimer <= 0 && monsters.Count == 0 && IsSpawnQueueEmpty())
					{

						//NET Complete Level
						if(isHost)
                        {
							PK_CompleteLevel message = new();
							modInstance.Helper.Multiplayer.SendMessage(message, "PK_CompleteLevel");
							OnCompleteLevel();
						}
						
					}
				}

				for (int i = monsters.Count - 1; i >= 0; i--)
				{
					// Target the closest player
					float minimumDist = float.MaxValue;
					Vector2 targetPosition = Vector2.Zero;

					foreach(BasePlayer p in playerList.Values)
                    {
						float dist = (p.position - monsters[i].position.Location.ToVector2()).LengthSquared();
						if (dist < minimumDist)
						{
							minimumDist = dist;
							targetPosition = p.position;
						}
					}

					monsters[i].Move(targetPosition, time);

					if (i < monsters.Count && monsters[i].position.Intersects(player.boundingBox) && !player.IsInvincible())
					{
						if (zombieModeTimer <= 0)
						{

							PlayerDie();

							//NET player death
							PK_PlayerDeath message = new();
							message.id = Game1.player.UniqueMultiplayerID;
							modInstance.Helper.Multiplayer.SendMessage(message, "PK_PlayerDeath");

							break;
						}
						if (monsters[i].type != MONSTER_TYPE.dracula)
						{
							//NET EnemyKilled
							PK_EnemyKilled message = new();
							message.id = monsters[i].id;
							modInstance.Helper.Multiplayer.SendMessage(message, "PK_EnemyKilled");

							AddGuts(monsters[i].position.Location, monsters[i].type);
							monsters.RemoveAt(i);
							Game1.playSound("Cowboy_monsterDie");
						}
					}
				}
			}


			//NET EnemyPositions
			if(isHost)
            {
				PK_EnemyPositions message = new();
				message.positions = new Dictionary<long, Point>();
				foreach (Enemy m in monsters)
				{
					message.positions.Add(m.id, m.position.Location);
				}
				modInstance.Helper.Multiplayer.SendMessage(message, "PK_EnemyPositions");
			}

			return false;
		}

		public void OnCompleteLevel(int level = -1)
        {
			hasGopherAppeared = false;
			waveTimer = 80000;
			betweenWaveTimer = 3333;
			if(level == -1)
            {
				whichWave++;
			}
			else
            {
				whichWave = level;
            }
			

			switch (whichWave)
			{
				case 1:
				case 2:
				case 3:
					monsterChances[0] = new Vector2(monsterChances[0].X + 0.001f, monsterChances[0].Y + 0.02f);
					if (whichWave > 1)
					{
						monsterChances[2] = new Vector2(monsterChances[2].X + 0.001f, monsterChances[2].Y + 0.01f);
					}
					monsterChances[6] = new Vector2(monsterChances[6].X + 0.001f, monsterChances[6].Y + 0.01f);
					if (newGamePlus > 0)
					{
						monsterChances[4] = new Vector2(0.002f, 0.1f);
					}
					break;
				case 4:
				case 5:
				case 6:
				case 7:
					if (monsterChances[5].Equals(Vector2.Zero))
					{
						monsterChances[5] = new Vector2(0.01f, 0.15f);
						if (newGamePlus > 0)
						{
							monsterChances[5] = new Vector2(0.01f + newGamePlus * 0.004f, 0.15f + newGamePlus * 0.04f);
						}
					}
					monsterChances[0] = Vector2.Zero;
					monsterChances[6] = Vector2.Zero;
					monsterChances[2] = new Vector2(monsterChances[2].X + 0.002f, monsterChances[2].Y + 0.02f);
					monsterChances[5] = new Vector2(monsterChances[5].X + 0.001f, monsterChances[5].Y + 0.02f);
					monsterChances[1] = new Vector2(monsterChances[1].X + 0.0018f, monsterChances[1].Y + 0.08f);
					if (newGamePlus > 0)
					{
						monsterChances[4] = new Vector2(0.001f, 0.1f);
					}
					break;
				case 8:
				case 9:
				case 10:
				case 11:
					monsterChances[5] = Vector2.Zero;
					monsterChances[1] = Vector2.Zero;
					monsterChances[2] = Vector2.Zero;
					if (monsterChances[3].Equals(Vector2.Zero))
					{
						monsterChances[3] = new Vector2(0.012f, 0.4f);
						if (newGamePlus > 0)
						{
							monsterChances[3] = new Vector2(0.012f + (float)newGamePlus * 0.005f, 0.4f + (float)newGamePlus * 0.075f);
						}
					}
					if (monsterChances[4].Equals(Vector2.Zero))
					{
						monsterChances[4] = new Vector2(0.003f, 0.1f);
					}
					monsterChances[3] = new Vector2(monsterChances[3].X + 0.002f, monsterChances[3].Y + 0.05f);
					monsterChances[4] = new Vector2(monsterChances[4].X + 0.0015f, monsterChances[4].Y + 0.04f);
					if (whichWave == 11)
					{
						monsterChances[4] = new Vector2(monsterChances[4].X + 0.01f, monsterChances[4].Y + 0.04f);
						monsterChances[3] = new Vector2(monsterChances[3].X - 0.01f, monsterChances[3].Y + 0.04f);
					}
					break;
			}
			//Slightly ramp up chanced for each round completed
			if (newGamePlus > 0)
			{
				for (int j = 0; j < monsterChances.Count; j++)
				{
					monsterChances[j] *= 1.1f;
				}
			}
			//Every second level shall be a shopping level (? probably overwritten by boss level then?)
			if (whichWave > 0 && whichWave % 2 == 0)
			{
				StartShoppingLevel();
			}
			else if (whichWave > 0)
			{
				waitingForPlayerToMoveDownAMap = true;
				map[7, 15] = MAP_TILE.SAND;
				map[8, 15] = MAP_TILE.SAND;
				map[9, 15] = MAP_TILE.SAND;
			}
		}

		protected void ProcessInputs()
		{
			//Start the game after startmenu
			if (_buttonHeldFrames[GameKeys.UsePowerup] > 0)
			{
				if (_buttonHeldFrames[GameKeys.UsePowerup] == 1 && onStartMenu && isHost)
				{
					onStartMenu = false;
					InstantiatePlayers();
					Game1.playSound("Pickup_Coin15");
					PK_StartNewGame mNewGame = new();
					modInstance.Helper.Multiplayer.SendMessage(mNewGame, "PK_StartNewGame");
				}
			}

			if (_buttonHeldFrames[GameKeys.MoveUp] > 0)
			{
				if (_buttonHeldFrames[GameKeys.MoveUp] == 1 && gameOver)
				{
					gameOverOption = Math.Max(0, gameOverOption - 1);
					Game1.playSound("Cowboy_gunshot");
				}
			}

			if (_buttonHeldFrames[GameKeys.MoveDown] > 0)
			{
				if (_buttonHeldFrames[GameKeys.MoveDown] == 1 && gameOver)
				{
					gameOverOption = Math.Min(1, gameOverOption + 1);
					Game1.playSound("Cowboy_gunshot");
				}
			}
			if (_buttonHeldFrames[GameKeys.Exit] == 1)
			{
				quit = true;
			}

			if (onStartMenu) return;

			player.ProcessInputs(_buttonHeldFrames);

			if (_buttonHeldFrames[GameKeys.SelectOption] == 1 && gameOver)
			{
				if (gameOverOption == 1)
				{
					quit = true;
				}
				else
				{
					gamerestartTimer = 1500;
					gameOver = false;
					gameOverOption = 0;
					Game1.playSound("Pickup_Coin15");
				}
			}
			if (_buttonHeldFrames[GameKeys.UsePowerup] == 1 && !gameOver && heldItem != null && player.deathTimer <= 0f && zombieModeTimer <= 0)
			{
				UsePowerup(heldItem.which);
				heldItem = null;
			}

		}

		public virtual void ApplyLevelSpecificStates()
		{
			if (whichWave == 12)
			{
				shootoutLevel = true;

				if(isHost)
					monsters.Add(new Dracula(this));

			}
			else if (whichWave > 0 && whichWave % 4 == 0)
			{
				shootoutLevel = true;

				if(isHost)
					monsters.Add(new Outlaw(this, new Point(8 * TileSize, 13 * TileSize)));

				if (Game1.soundBank != null)
				{
					outlawSong = Game1.soundBank.GetCue("cowboy_outlawsong");
					outlawSong.Play();
				}
			}
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void SpawnBullets(int[] directions, Vector2 spawn)
		{
			Point bulletSpawn = new((int)spawn.X + 24, (int)spawn.Y + 24 - 6);
			int speed = (int)GetMovementSpeed(8f, 2);
			if (directions.Length == 1)
			{
				int playerShootingDirection = directions[0];
				switch (playerShootingDirection)
				{
					case 0:
						bulletSpawn.Y -= 22;
						break;
					case 1:
						bulletSpawn.X += 16;
						bulletSpawn.Y -= 6;
						break;
					case 2:
						bulletSpawn.Y += 10;
						break;
					case 3:
						bulletSpawn.X -= 16;
						bulletSpawn.Y -= 6;
						break;
				}

				NETspawnBullet(true, bulletSpawn, playerShootingDirection, player.bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					switch (playerShootingDirection)
					{
						case 0:
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, -8), player.bulletDamage);
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, -8), player.bulletDamage);
							break;
						case 1:
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, -2), player.bulletDamage);
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, 2), player.bulletDamage);
							break;
						case 2:
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, 8), player.bulletDamage);
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, 8), player.bulletDamage);
							break;
						case 3:
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, -2), player.bulletDamage);
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, 2), player.bulletDamage);
							break;
					}
				}
			}
			else if (directions.Contains(0) && directions.Contains(1))
			{
				bulletSpawn.X += TileSize / 2;
				bulletSpawn.Y -= TileSize / 2;
				NETspawnBullet(true, bulletSpawn, new Point(speed, -speed), player.bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					int modifier8 = -2;
					NETspawnBullet(true, bulletSpawn, new Point(speed + modifier8, -speed + modifier8), player.bulletDamage);
					modifier8 = 2;
					NETspawnBullet(true, bulletSpawn, new Point(speed + modifier8, -speed + modifier8), player.bulletDamage);
				}
			}
			else if (directions.Contains(0) && directions.Contains(3))
			{
				bulletSpawn.X -= TileSize / 2;
				bulletSpawn.Y -= TileSize / 2;
				NETspawnBullet(true, bulletSpawn, new Point(-speed, -speed), player.bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					int modifier6 = -2;
					NETspawnBullet(true, bulletSpawn, new Point(-speed - modifier6, -speed + modifier6), player.bulletDamage);
					modifier6 = 2;
					NETspawnBullet(true, bulletSpawn, new Point(-speed - modifier6, -speed + modifier6), player.bulletDamage);
				}
			}
			else if (directions.Contains(2) && directions.Contains(1))
			{
				bulletSpawn.X += TileSize / 2;
				bulletSpawn.Y += TileSize / 4;
				NETspawnBullet(true, bulletSpawn, new Point(speed, speed), player.bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					int modifier4 = -2;
					NETspawnBullet(true, bulletSpawn, new Point(speed - modifier4, speed + modifier4), player.bulletDamage);
					modifier4 = 2;
					NETspawnBullet(true, bulletSpawn, new Point(speed - modifier4, speed + modifier4), player.bulletDamage);
				}
			}
			else if (directions.Contains(2) && directions.Contains(3))
			{
				bulletSpawn.X -= TileSize / 2;
				bulletSpawn.Y += TileSize / 4;

				NETspawnBullet(true, bulletSpawn, new Point(-speed, speed), player.bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					int modifier2 = -2;
					NETspawnBullet(true, bulletSpawn, new Point(-speed + modifier2, speed + modifier2), player.bulletDamage);
					modifier2 = 2;
					NETspawnBullet(true, bulletSpawn, new Point(-speed + modifier2, speed + modifier2), player.bulletDamage);
				}
			}
		}

		public bool IsSpawnQueueEmpty()
		{
			for (int i = 0; i < 4; i++)
			{
				if (spawnQueue[i].Count > 0)
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsMapTilePassable(MAP_TILE tileType)
		{
			if ((uint)tileType <= 1u || (uint)(tileType - 5) <= 4u)
			{
				return false;
			}
			return true;
		}

		public static bool IsMapTilePassableForMonsters(MAP_TILE tileType)
		{
			if (tileType == MAP_TILE.CACTUS || (uint)(tileType - 7) <= 2u)
			{
				return false;
			}
			return true;
		}

		public bool IsCollidingWithMonster(Rectangle r, Enemy subject)
		{
			foreach (Enemy c in monsters)
			{
				if ((subject == null || !subject.Equals(c)) && Math.Abs(c.position.X - r.X) < 48 && Math.Abs(c.position.Y - r.Y) < 48 && r.Intersects(new Rectangle(c.position.X, c.position.Y, 48, 48)))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsCollidingWithMapForMonsters(Rectangle positionToCheck)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !IsMapTilePassableForMonsters(map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsCollidingWithMap(Rectangle positionToCheck)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !IsMapTilePassable(map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsCollidingWithMap(Point position)
		{
			Rectangle positionToCheck = new(position.X, position.Y, 48, 48);
			for (int i = 0; i < 4; i++)
			{
				Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !IsMapTilePassable(map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsCollidingWithMap(Vector2 position)
		{
			Rectangle positionToCheck = new((int)position.X, (int)position.Y, 48, 48);
			for (int i = 0; i < 4; i++)
			{
				Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
				if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !IsMapTilePassable(map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
				{
					return true;
				}
			}
			return false;
		}
		public void StartShoppingLevel()
		{
			merchantBox.Y = -TileSize;
			shopping = true;
			merchantArriving = true;
			merchantShopOpen = false;
			if (overworldSong != null)
			{
				overworldSong.Stop(AudioStopOptions.Immediate);
			}
			monsters.Clear();
			waitingForPlayerToMoveDownAMap = true;
			storeItems.Clear();

			//Fill store with the next upgrade items
			ITEM_TYPE runSpeedItem;
			if (player.runSpeedLevel == 0) runSpeedItem = ITEM_TYPE.RUNSPEED1;
			else if (player.runSpeedLevel == 1) runSpeedItem = ITEM_TYPE.RUNSPEED2;
			else runSpeedItem = ITEM_TYPE.LIFE;

			ITEM_TYPE fireSpeedItem;
			if (player.fireSpeedLevel == 0) fireSpeedItem = ITEM_TYPE.FIRESPEED1;
			else if (player.fireSpeedLevel == 1) fireSpeedItem = ITEM_TYPE.FIRESPEED2;
			else if (player.fireSpeedLevel == 2) fireSpeedItem = ITEM_TYPE.FIRESPEED3;
			else if (player.ammoLevel >= 3 && !spreadPistol) fireSpeedItem = ITEM_TYPE.SPREADPISTOL;
			else fireSpeedItem = ITEM_TYPE.STAR;

			ITEM_TYPE ammoItem;
			if (player.ammoLevel == 0) ammoItem = ITEM_TYPE.AMMO1;
			else if (player.ammoLevel == 1) ammoItem = ITEM_TYPE.AMMO2;
			else if (player.ammoLevel == 2) ammoItem = ITEM_TYPE.AMMO3;
			else ammoItem = ITEM_TYPE.STAR;

			storeItems.Add(new Rectangle(7 * TileSize + 12, 8 * TileSize - TileSize * 2, TileSize, TileSize), runSpeedItem);
			storeItems.Add(new Rectangle(8 * TileSize + 24, 8 * TileSize - TileSize * 2, TileSize, TileSize), fireSpeedItem);
			storeItems.Add(new Rectangle(9 * TileSize + 36, 8 * TileSize - TileSize * 2, TileSize, TileSize), ammoItem);

		}

		public void receiveKeyPress(Keys k)
		{

		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public static int GetPriceForItem(ITEM_TYPE whichItem)
		{
            return whichItem switch
            {
                ITEM_TYPE.AMMO1 => 15,
                ITEM_TYPE.AMMO2 => 30,
                ITEM_TYPE.AMMO3 => 45,
                ITEM_TYPE.FIRESPEED1 => 10,
                ITEM_TYPE.FIRESPEED2 => 20,
                ITEM_TYPE.FIRESPEED3 => 30,
                ITEM_TYPE.LIFE => 10,
                ITEM_TYPE.RUNSPEED1 => 8,
                ITEM_TYPE.RUNSPEED2 => 20,
                ITEM_TYPE.SPREADPISTOL => 99,
                ITEM_TYPE.STAR => 10,
                _ => 5,
            };
        }

		public void draw(SpriteBatch b)
		{
			//Start drawing
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

			//Draw start menu
			if (onStartMenu)
			{
				b.Draw(
					Game1.staminaRect,
					new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize),
					Game1.staminaRect.Bounds,
					Color.Black,
					0f,
					Vector2.Zero,
					SpriteEffects.None,
					0.97f
				);
				b.Draw(
					startScreenTexture,
					new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height / 2),
					new Rectangle(0, 0, 256, 256),
					Color.White,
					0f,
					new Vector2(128,128),
					3f,
					SpriteEffects.None,
					1f
				);

				List<Vector2> poppetjePositions = new()
				{
					new Vector2(65, 134),
					new Vector2(82, 134),
					new Vector2(159, 134),
					new Vector2(176, 134)
				};

				for(int i=0; i<4; i++)
                {
					bool playerJoined = i < modInstance.playerList.Count;
					Rectangle sourceRect;

					if(playerJoined) sourceRect = new Rectangle(16 * i, 0, 16, 16);
					else sourceRect = new Rectangle(16 * 4, 0, 16, 16);

					b.Draw(
						startScreenPoppetjesTexture,
						topLeftScreenCoordinate + poppetjePositions[i] * 3f,
						sourceRect,
						playerJoined ? Color.White : new Color(255,255,255,20),
						0f,
						Vector2.Zero,
						3f,
						SpriteEffects.None,
						1f
					);
				}
			}

			//Draw game over screen
			else if ((gameOver || gamerestartTimer > 0) && !endCutscene)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), topLeftScreenCoordinate + new Vector2(6f, 7f) * TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), topLeftScreenCoordinate + new Vector2(6f, 7f) * TileSize + new Vector2(-1f, 0f), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), topLeftScreenCoordinate + new Vector2(6f, 7f) * TileSize + new Vector2(1f, 0f), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				string option = Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11917");
				if ((OPTION_TYPE)gameOverOption == OPTION_TYPE.RETRY)
				{
					option = "> " + option;
				}
				string option2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11919");
				if ((OPTION_TYPE)gameOverOption == OPTION_TYPE.QUIT)
				{
					option2 = "> " + option2;
				}
				if (gamerestartTimer <= 0 || gamerestartTimer / 500 % 2 == 0)
				{
					b.DrawString(Game1.smallFont, option, topLeftScreenCoordinate + new Vector2(6f, 9f) * TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				}
				b.DrawString(Game1.smallFont, option2, topLeftScreenCoordinate + new Vector2(6f, 9f) * TileSize + new Vector2(0f, TileSize * 2 / 3), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}

			//Draw the final cutscene
			else if (endCutscene)
			{
				switch (endCutscenePhase)
				{
					case 0:
						b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player.position + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White * ((endCutsceneTimer < 2000) ? (1f * ((float)endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, player.position.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player.position + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + (int)player.GetHeldItem() * 16, 1776, 16, 16), Color.White * ((endCutsceneTimer < 2000) ? (1f * ((float)endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, player.position.Y / 10000f + 0.002f);
						break;
					case 4:
					case 5:
						b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.97f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(6 * TileSize, 3 * TileSize), new Rectangle(224, 1744, 64, 48), Color.White * ((endCutsceneTimer > 0) ? (1f - ((float)endCutsceneTimer - 2000f) / 2000f) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
						if (endCutscenePhase == 5 && gamerestartTimer <= 0)
						{
							b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_PK_NewGame+"), topLeftScreenCoordinate + new Vector2(3f, 10f) * TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
						}
						break;
					case 1:
					case 2:
					case 3:
						{
							for (int x = 0; x < 16; x++)
							{
								for (int y = 0; y < 16; y++)
								{
									b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(x, y) * 16f * 3f + new Vector2(0f, newMapPosition - 16 * TileSize), new Rectangle(464 + 16 * (int)map[x, y] + ((map[x, y] == MAP_TILE.CACTUS && cactusDanceTimer > 800f) ? 16 : 0), 1680 - (int)world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
								}
							}
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(6 * TileSize, 3 * TileSize), new Rectangle(288, 1697, 64, 80), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.01f);
							if (endCutscenePhase == 3)
							{
								b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(9 * TileSize, 7 * TileSize), new Rectangle(544, 1792, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.05f);
								if (endCutsceneTimer < 3000)
								{
									b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black * (1f - (float)endCutsceneTimer / 3000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
								}
								break;
							}
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(10 * TileSize, 8 * TileSize), new Rectangle(272 - endCutsceneTimer / 300 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.02f);
							if (endCutscenePhase == 2)
							{
								b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player.position + new Vector2(4f, 13f) * 3f, new Rectangle(484, 1760 + (int)(player.motionAnimationTimer / 100f) * 3, 8, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, player.position.Y / 10000f + 0.001f + 0.001f);
								b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player.position, new Rectangle(384, 1760, 16, 13), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, player.position.Y / 10000f + 0.002f + 0.001f);
								b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player.position + new Vector2(0f, -TileSize * 2 / 3 - TileSize / 4), new Rectangle(320 + (int)player.GetHeldItem() * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, player.position.Y / 10000f + 0.005f);
							}
							b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black * ((endCutscenePhase == 1 && endCutsceneTimer > 12500) ? ((float)((endCutsceneTimer - 12500) / 3000)) : 0f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
							break;
						}
				}
			}
			else
			{
				//Lightning animation for zombie powerup
				if (zombieModeTimer > 8200)
				{
					//Draw the flashing characters
					foreach (BasePlayer p in playerList.Values)
                    {
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + p.position, new Rectangle(384 + ((zombieModeTimer / 200 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					}

					//Draw the lighning above the players
					foreach(BasePlayer p in playerList.Values)
                    {
						for (int y = (int)(p.position.Y - TileSize); y > -TileSize; y -= TileSize)
						{
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(p.position.X, y), new Rectangle(368 + ((y / TileSize % 3 == 0) ? 16 : 0), 1744, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
						}
					}

					b.End();
					return;
				}

				//Draw the Map
				for (int x = 0; x < 16; x++)
				{
					for (int y = 0; y < 16; y++)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(x, y) * 16f * 3f + new Vector2(0f, newMapPosition - 16 * TileSize), new Rectangle(464 + 16 * (int)map[x, y] + ((map[x, y] == MAP_TILE.CACTUS && cactusDanceTimer > 800f) ? 16 : 0), 1680 - (int)world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
					}
				}
				//Also draw the next map if transitioning between levels
				if (scrollingMap)
				{
					for (int x = 0; x < 16; x++)
					{
						for (int y = 0; y < 16; y++)
						{
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(x, y) * 16f * 3f + new Vector2(0f, newMapPosition), new Rectangle(464 + 16 * (int)nextMap[x, y] + ((nextMap[x, y] == MAP_TILE.CACTUS && cactusDanceTimer > 800f) ? 16 : 0), 1680 - (int)world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
						}
					}
					// Not quite sure?
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, -1, 16 * TileSize, (int)topLeftScreenCoordinate.Y), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y + 16 * TileSize, 16 * TileSize, (int)topLeftScreenCoordinate.Y + 2), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}

				//Draw all temporary sprites
				foreach (TemporaryAnimatedSprite temporarySprite in temporarySprites)
				{
					temporarySprite.draw(b, localPosition: true);
				}

				//Draw all powerups
				foreach (Powerup powerup in powerups)
				{
					powerup.Draw(b);
				}

				//Draw all bullets
				foreach (Bullet bullet in bullets)
				{
					bullet.Draw(b);
				}

				//Draw shop
				if (shopping)
				{
					if (merchantArriving && !merchantShopOpen)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X, merchantBox.Location.Y), new Rectangle(464 + ((shoppingTimer / 100 % 2 == 0) ? 16 : 0), 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
					}
					else
					{
						int whichFrame = (player.boundingBox.X - merchantBox.X > TileSize) ? 2 : ((merchantBox.X - player.boundingBox.X > TileSize) ? 1 : 0);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X, merchantBox.Location.Y), new Rectangle(496 + whichFrame * 16, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X - TileSize, merchantBox.Location.Y + TileSize), new Rectangle(529, 1744, 63, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
						foreach (KeyValuePair<Rectangle, ITEM_TYPE> v in storeItems)
						{
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(v.Key.Location.X, v.Key.Location.Y), new Rectangle(320 + (int)v.Value * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f);

							//Draw three times with slight offset to obtain a thicker font i suppose
							b.DrawString(Game1.smallFont, string.Concat(GetPriceForItem(v.Value)), topLeftScreenCoordinate + new Vector2((v.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(string.Concat(GetPriceForItem(v.Value))).X / 2f, v.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, v.Key.Location.Y / 10000f + 0.002f);
							b.DrawString(Game1.smallFont, string.Concat(GetPriceForItem(v.Value)), topLeftScreenCoordinate + new Vector2((v.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(string.Concat(GetPriceForItem(v.Value))).X / 2f - 1f, v.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, v.Key.Location.Y / 10000f + 0.002f);
							b.DrawString(Game1.smallFont, string.Concat(GetPriceForItem(v.Value)), topLeftScreenCoordinate + new Vector2((v.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(string.Concat(GetPriceForItem(v.Value))).X / 2f + 1f, v.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, v.Key.Location.Y / 10000f + 0.002f);
						}

						//Draw Speech bubble
						b.Draw(
							shopBubbleTexture,
							topLeftScreenCoordinate + new Vector2(merchantBox.Location.X - TileSize / 2, merchantBox.Location.Y - TileSize * 2),
							new Rectangle(0, 0, 32, 32),
							Color.White,
							0f,
							Vector2.Zero,
							3f,
							SpriteEffects.None,
							merchantBox.Location.Y / 10000f + 0.001f
						);
					}
				}

				//Draw the arrow pointing down to the next map if the level is finished
				if (waitingForPlayerToMoveDownAMap && (merchantShopOpen || !shopping) && shoppingTimer < 250)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(8.5f, 15f) * TileSize + new Vector2(-12f, 0f), new Rectangle(355, 1750, 8, 8), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.001f);
				}

				//Draw Players
				foreach(BasePlayer p in playerList.Values)
                {
					p.Draw(b);
                }

				// Draw all monsters
				foreach (Enemy monster in monsters)
				{
					monster.Draw(b);
				}

				//Draw the animation when transitioning to the next level
				if (gopherRunning)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(gopherBox.X, gopherBox.Y), new Rectangle(320 + waveTimer / 100 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)gopherBox.Y / 10000f + 0.001f);
				}
				if (gopherTrain && gopherTrainPosition > -TileSize)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(player.position.X - (float)(TileSize / 2), gopherTrainPosition), new Rectangle(384 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(player.position.X + (float)(TileSize / 2), gopherTrainPosition), new Rectangle(384 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(player.position.X, gopherTrainPosition - TileSize * 3), new Rectangle(320 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(player.position.X - (float)(TileSize / 2), gopherTrainPosition - TileSize), new Rectangle(400, 1728, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.97f);
					if (player.IsHoldingItem())
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player.position + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player.position + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + (int)player.GetHeldItem() * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player.position + new Vector2(0f, -TileSize / 4), new Rectangle(464, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
					}
				}
				else // Draw the user interface
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize + 27, 0f), new Rectangle(294, 1782, 22, 22), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.25f);
					if (heldItem != null)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize + 18, -9f), new Rectangle(272 + (int)heldItem.which * 16, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize * 2, -TileSize - 18), new Rectangle(400, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					b.DrawString(Game1.smallFont, "x" + Math.Max(lives, 0), topLeftScreenCoordinate - new Vector2(TileSize, -TileSize - TileSize / 4 - 18), Color.White);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize * 2, -TileSize * 2 - 18), new Rectangle(272, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					b.DrawString(Game1.smallFont, "x" + Coins, topLeftScreenCoordinate - new Vector2(TileSize, -TileSize * 2 - TileSize / 4 - 18), Color.White);
					for (int j = 0; j < whichWave + newGamePlus * 12; j++)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(TileSize * 16 + 3, j * 3 * 6), new Rectangle(512, 1760, 5, 5), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					b.Draw(Game1.mouseCursors, new Vector2((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y - TileSize / 2 - 12), new Rectangle(595, 1748, 9, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					if (!shootoutLevel)
					{
						b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X + 30, (int)topLeftScreenCoordinate.Y - TileSize / 2 + 3, (int)((float)(16 * TileSize - 30) * ((float)waveTimer / 80000f)), TileSize / 4), (waveTimer < 8000) ? new Color(188, 51, 74) : new Color(147, 177, 38));
					}
					if (betweenWaveTimer > 0 && whichWave == 0 && !scrollingMap)
					{
						Vector2 pos = new(Game1.viewport.Width / 2 - 120, Game1.viewport.Height - 144 - 3);
						if (!Game1.options.gamepadControls)
						{
							b.Draw(Game1.mouseCursors, pos, new Rectangle(352, 1648, 80, 48), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
						}
						else
						{
							b.Draw(Game1.controllerMaps, pos, Utility.controllerMapSourceRect(new Rectangle(681, 157, 160, 96)), Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.99f);
						}
					}

					//Draw Powerup Icons
					if (player.bulletDamage > 1)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize), new Rectangle(416 + (player.ammoLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (player.fireSpeedLevel > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize * 2), new Rectangle(320 + (player.fireSpeedLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (player.runSpeedLevel > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize * 3), new Rectangle(368 + (player.runSpeedLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (spreadPistol)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize * 4), new Rectangle(464, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
				}

				//Draw Screen Flash
				if (screenFlash > 0)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, new Color(255, 214, 168), 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
			}
			b.End();
		}

		public void changeScreenSize()
		{
			topLeftScreenCoordinate = new Vector2(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 384);
		}

		public void unload()
		{
			if (overworldSong != null && overworldSong.IsPlaying)
			{
				overworldSong.Stop(AudioStopOptions.Immediate);
			}
			if (outlawSong != null && outlawSong.IsPlaying)
			{
				outlawSong.Stop(AudioStopOptions.Immediate);
			}
			lives = 3;
			Game1.stopMusicTrack(Game1.MusicContext.MiniGame);

			isHost = false;
			modInstance.isHost.Value = false;
			modInstance.isHostAvailable = false;

			PK_ExitGame message = new();
			modInstance.Helper.Multiplayer.SendMessage(message, "PK_ExitGame");
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return "PrairieKing";
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool forceQuit()
		{
			unload();
			return true;
		}
	}
}
