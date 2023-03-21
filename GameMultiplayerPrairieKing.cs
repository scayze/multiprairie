using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerPrairieKing;
using MultiplayerPrairieKing.Entities;
using MultiplayerPrairieKing.Entities.Enemies;
using MultiplayerPrairieKing.Helpers;
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
			Bullet bullet = new(this, position, motion, damage);

			if (friendly)
				bullets.Add(bullet);
			else
				enemyBullets.Add(bullet);


			//NET Spawn Bullet
			PK_BulletSpawn mBullet = new();
			mBullet.id = bullet.id;
			mBullet.position = bullet.position;
			mBullet.motion = bullet.motion;
			mBullet.damage = bullet.damage;
			mBullet.isFriendly = friendly;
			modInstance.Helper.Multiplayer.SendMessage(mBullet, "PK_BulletSpawn");
		}

		public void NETspawnBullet(bool friendly, Point position, int direction, int damage)
		{
			Bullet bullet = new(this, position, direction, damage);

			if (friendly)
				bullets.Add(bullet);
			else
				enemyBullets.Add(bullet);


			//NET Spawn Bullet
			PK_BulletSpawn mBullet = new();
			mBullet.id = bullet.id;
			mBullet.position = bullet.position;
			mBullet.motion = bullet.motion;
			mBullet.damage = bullet.damage;
			mBullet.isFriendly = friendly;
			modInstance.Helper.Multiplayer.SendMessage(mBullet, "PK_BulletSpawn");
		}

		public void NETmovePlayer(Vector2 pos)
        {
			PK_PlayerMove message = new();
			message.position = pos;
			message.id = Game1.player.UniqueMultiplayerID;
			message.shootingDirections = playerShootingDirections;
			message.movementDirections = playerMovementDirections;
			modInstance.Helper.Multiplayer.SendMessage(message, "PK_PlayerMove");
        }

		public ModMultiPlayerPrairieKing modInstance;

		public bool isHost;

		public const int mapWidth = 16;

		public const int mapHeight = 16;

		public const int pixelZoom = 3;

		public const int bulletSpeed = 8;

		public const double lootChance = 0.05;

		public const double coinChance = 0.05;

		public int lootDuration = 7500;

		public int powerupDuration = 10000;

		public const float playerSpeed = 3f;

		public const int baseTileSize = 16;

		public const int cactusDanceDelay = 800;

		public const int playerMotionDelay = 100;

		public const int playerFootStepDelay = 200;

		public const int deathDelay = 3000;

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

		//Unused. for potential future refactor
		public class Player
		{
			public int runSpeedLevel;
			public int fireSpeedLevel;
			public int ammoLevel;
			public Vector2 position;
			public Rectangle boundingBox;
			public int shotTimer;
			public int bulletDamage;

			public List<int> movementDirections = new();
			public List<int> shootingDirections = new();

		}


		public int runSpeedLevel;

		public int fireSpeedLevel;

		public int ammoLevel;

		public int whichRound;

		public bool spreadPistol;

		public const int waveDuration = 80000;

		public const int betweenWaveDuration = 5000;

		public List<Enemy> monsters = new();

		protected HashSet<Vector2> _borderTiles = new();

		public Vector2 playerPosition;

		public Vector2 player2Position;

        public Rectangle playerBoundingBox;

		public Rectangle merchantBox;

		public Rectangle player2BoundingBox;

		public Rectangle noPickUpBox;

		public List<int> playerMovementDirections = new();

		public List<int> playerShootingDirections = new();
		
		public List<int> player2MovementDirections = new();

		public List<int> player2ShootingDirections = new();

		public int shootingDelay = 300;

		public int shotTimer;

		public int motionPause;

		public int bulletDamage;

		public int speedBonus;

		public int fireRateBonus;

		public int lives = 3;

		public int coins;

		public int score;

		public int player2deathtimer;

		public int player2invincibletimer;

		public List<Bullet> bullets = new();

		public List<Bullet> enemyBullets = new();

		public MAP_TILE[,] map = new MAP_TILE[16, 16];

		public MAP_TILE[,] nextMap = new MAP_TILE[16, 16];

		public class SpawnTask
        {
			public SpawnTask()
            {

            }

			public SpawnTask(MONSTER_TYPE type, int y)
            {
				this.type = type;
				this.Y = y;
            }
			public MONSTER_TYPE type = MONSTER_TYPE.orc;
			public int Y = 0;
        }

		public List<SpawnTask>[] spawnQueue = new List<SpawnTask>[4];

		public static Vector2 topLeftScreenCoordinate;

		public float cactusDanceTimer;

		public float playerMotionAnimationTimer;

		public float playerFootstepSoundTimer = 200f;

		public behaviorAfterMotionPause behaviorAfterPause;

		public List<Vector2> monsterChances = new()
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

		public int world = 0;

		public int gameOverOption;

		public int gamerestartTimer;

		public int player2TargetUpdateTimer;

		public int player2AnimationTimer;

		public int fadethenQuitTimer;

		public int waveTimer = 80000;

		public int betweenWaveTimer = 5000;

		public int whichWave;

		public int monsterConfusionTimer;

		public int zombieModeTimer;

		public int shoppingTimer;

		public int holdItemTimer;

		public ITEM_TYPE itemToHold;

		public int newMapPosition;

		public int playerInvincibleTimer;

		public int screenFlash;

		public int gopherTrainPosition;

		public int endCutsceneTimer;

		public int endCutscenePhase;

		public int startTimer;

		public float deathTimer;

		public bool onStartMenu;

		public bool shopping;

		public bool gopherRunning;

		public bool store;

		public bool merchantLeaving;

		public bool merchantArriving;

		public bool merchantShopOpen;

		public bool waitingForPlayerToMoveDownAMap;

		public bool scrollingMap;

		public bool hasGopherAppeared;

		public bool shootoutLevel;

		public bool gopherTrain;

		public bool playerJumped;

		public bool endCutscene;

		public bool gameOver;

		public bool playingWithAbigail;

		public Dictionary<Rectangle, ITEM_TYPE> storeItems = new();

		public bool quit;

		public bool died;

		public Rectangle gopherBox;

		public Point gopherMotion;

		public static ICue overworldSong;

		public static ICue outlawSong;

		public static ICue zombieSong;

		protected HashSet<GameKeys> _buttonHeldState = new();

		protected Dictionary<GameKeys, int> _buttonHeldFrames;

		private int player2FootstepSoundTimer;

		public static int TileSize => 48;

		public bool LoadGame()
		{
			if (playingWithAbigail)
			{
				return false;
			}
			if (Game1.player.jotpkProgress.Value == null)
			{
				return false;
			}
			AbigailGame.JOTPKProgress save_data = Game1.player.jotpkProgress.Value;
			ammoLevel = save_data.ammoLevel.Value;
			bulletDamage = save_data.bulletDamage.Value;
			coins = save_data.coins.Value;
			died = save_data.died.Value;
			fireSpeedLevel = save_data.fireSpeedLevel.Value;
			lives = save_data.lives.Value;
			score = save_data.score.Value;
			runSpeedLevel = save_data.runSpeedLevel.Value;
			spreadPistol = save_data.spreadPistol.Value;
			whichRound = save_data.whichRound.Value;
			whichWave = save_data.whichWave.Value;
			waveTimer = save_data.waveTimer.Value;
			world = save_data.world.Value;
			if (save_data.heldItem.Value != -100)
			{
				heldItem = new Powerup(this, (POWERUP_TYPE)save_data.heldItem.Value, Point.Zero, 9999);
			}
			monsterChances = new List<Vector2>(save_data.monsterChances);
			ApplyLevelSpecificStates();
			if (shootoutLevel)
			{
				playerPosition = new Vector2(8 * TileSize, 3 * TileSize);
			}
			return true;
		}

		public void SaveGame()
		{
			if (!playingWithAbigail)
			{
				if (Game1.player.jotpkProgress.Value == null)
				{
					Game1.player.jotpkProgress.Value = new AbigailGame.JOTPKProgress();
				}
				AbigailGame.JOTPKProgress save_data = Game1.player.jotpkProgress.Value;
				save_data.ammoLevel.Value = ammoLevel;
				save_data.bulletDamage.Value = bulletDamage;
				save_data.coins.Value = coins;
				save_data.died.Value = died;
				save_data.fireSpeedLevel.Value = fireSpeedLevel;
				save_data.lives.Value = lives;
				save_data.score.Value = score;
				save_data.runSpeedLevel.Value = runSpeedLevel;
				save_data.spreadPistol.Value = spreadPistol;
				save_data.whichRound.Value = whichRound;
				save_data.whichWave.Value = whichWave;
				save_data.waveTimer.Value = waveTimer;
				save_data.world.Value = world;
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
		}

		public GameMultiplayerPrairieKing(ModMultiPlayerPrairieKing mod, bool isHost, bool playingWithAbby = false)
		{
			this.modInstance = mod;
			this.isHost = isHost;
			Reset(playingWithAbby);
			if (!playingWithAbigail && LoadGame())
			{
				map = MapLoader.GetMap(whichWave);
			}
		}

		public GameMultiplayerPrairieKing(ModMultiPlayerPrairieKing mod, bool isHost, bool playingWithAbby, int coins, int ammoLevel, int bulletDamage, int fireSpeedLevel, int runSpeedLevel, int lives, bool spreadPistol, int whichRound)
		{
			this.modInstance = mod;
			this.isHost = isHost;
			Reset(playingWithAbby);
			this.coins = coins;
			this.ammoLevel = ammoLevel;
			this.bulletDamage = bulletDamage;
			this.fireSpeedLevel = fireSpeedLevel;
			this.runSpeedLevel = runSpeedLevel;
			this.lives = lives;
			this.spreadPistol = spreadPistol;
			this.whichRound = whichRound;
			ApplyNewGamePlus();
			SaveGame();
			onStartMenu = false;
		}

		public void ApplyNewGamePlus()
		{
			monsterChances[0] = new Vector2(0.014f + (float)whichRound * 0.005f, 0.41f + (float)whichRound * 0.05f);
			monsterChances[4] = new Vector2(0.002f, 0.1f);
		}

		public void Reset(bool playingWithAbby)
		{
			Rectangle r = new(0, 0, 16, 16);
			_borderTiles = new HashSet<Vector2>(Utility.getBorderOfThisRectangle(r));
			died = false;
			topLeftScreenCoordinate = new Vector2(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 384);
			enemyBullets.Clear();
			holdItemTimer = 0;
			itemToHold = ITEM_TYPE.NONE;
			merchantArriving = false;
			merchantLeaving = false;
			merchantShopOpen = false;
			monsterConfusionTimer = 0;
			monsters.Clear();
			newMapPosition = 16 * TileSize;
			scrollingMap = false;
			shopping = false;
			store = false;
			temporarySprites.Clear();
			waitingForPlayerToMoveDownAMap = false;
			waveTimer = 80000;
			whichWave = 0;
			zombieModeTimer = 0;
			bulletDamage = 1;
			deathTimer = 0f;
			shootoutLevel = false;
			betweenWaveTimer = 5000;
			gopherRunning = false;
			hasGopherAppeared = false;
			playerMovementDirections.Clear();
			outlawSong = null;
			overworldSong = null;
			endCutscene = false;
			endCutscenePhase = 0;
			endCutsceneTimer = 0;
			gameOver = false;
			deathTimer = 0f;
			playerInvincibleTimer = 0;
			playingWithAbigail = playingWithAbby;
			onStartMenu = true;
			startTimer = 0;
			powerups.Clear();
			world = 0;
			Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.MiniGame);
			for (int k = 0; k < 16; k++)
			{
				for (int i = 0; i < 16; i++)
				{
					if ((k == 0 || k == 15 || i == 0 || i == 15) && (k <= 6 || k >= 10) && (i <= 6 || i >= 10))
					{
						map[k, i] = MAP_TILE.CACTUS;
					}
					else if (k == 0 || k == 15 || i == 0 || i == 15)
					{
						map[k, i] = (MAP_TILE)((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (k == 1 || k == 14 || i == 1 || i == 14)
					{
						map[k, i] = MAP_TILE.GRAVEL;
					}
					else
					{
						map[k, i] = (MAP_TILE)((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}
			playerPosition = new Vector2(384f, 384f);

			//NET Player Move
			NETmovePlayer(playerPosition);

			playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
			playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
			playerBoundingBox.Width = TileSize / 2;
			playerBoundingBox.Height = TileSize / 2;
			if (playingWithAbigail)
			{
				onStartMenu = false; //TODO show anyway?
				player2Position = new Vector2(432f, 384f);
				player2BoundingBox = new Rectangle(9 * TileSize, 8 * TileSize, TileSize, TileSize);
				betweenWaveTimer += 1500;
			}
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

		public bool GetPowerUp(Powerup c)
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
					coins++;
					Game1.playSound("Pickup_Coin15");
					break;
				case POWERUP_TYPE.NICKEL:
					coins += 5;
					Game1.playSound("Pickup_Coin15");
					break;
				case POWERUP_TYPE.LIFE:
					lives++;
					Game1.playSound("cowboy_powerup");
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

		public void UsePowerup(POWERUP_TYPE which)
		{
			if (activePowerups.ContainsKey(which))
			{
				activePowerups[which] = powerupDuration + 2000;
				return;
			}

			switch (which)
			{
				case POWERUP_TYPE.HEART:
					itemToHold = ITEM_TYPE.FINISHED_GAME;
					holdItemTimer = 4000;
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
					//NET Start Gopher Train
					PK_StartGopherTrain mStartTrainSkull = new();
					modInstance.Helper.Multiplayer.SendMessage(mStartTrainSkull, "PK_StartGopherTrain");

					StartGopherTrain(ITEM_TYPE.SKULL);
					break;
				case POWERUP_TYPE.LOG:
					//NET Start Gopher Train
					PK_StartGopherTrain mStartTrainLog = new();
					modInstance.Helper.Multiplayer.SendMessage(mStartTrainLog, "PK_StartGopherTrain");

					StartGopherTrain(ITEM_TYPE.LOG);
					break;
				case POWERUP_TYPE.SHERRIFF:
					{
						UsePowerup(POWERUP_TYPE.SHOTGUN);
						UsePowerup(POWERUP_TYPE.RAPIDFIRE);
						UsePowerup(POWERUP_TYPE.SPEED);
						for (int j = 0; j < activePowerups.Count; j++)
						{
							activePowerups[activePowerups.ElementAt(j).Key] *= 2;
						}
						break;
					}
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
					{
						Point teleportSpot = Point.Zero;
						int tries = 0;
						while ((Math.Abs((float)teleportSpot.X - playerPosition.X) < 8f || Math.Abs((float)teleportSpot.Y - playerPosition.Y) < 8f || IsCollidingWithMap(teleportSpot) || IsCollidingWithMonster(new Rectangle(teleportSpot.X, teleportSpot.Y, TileSize, TileSize), null)) && tries < 10)
						{
							teleportSpot = new Point(Game1.random.Next(TileSize, 16 * TileSize - TileSize), Game1.random.Next(TileSize, 16 * TileSize - TileSize));
							tries++;
						}
						if (tries < 10)
						{
							temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, playerPosition + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
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
							playerPosition = new Vector2(teleportSpot.X, teleportSpot.Y);

							//NET Player Move
							NETmovePlayer(playerPosition);

							monsterConfusionTimer = 4000;
							playerInvincibleTimer = 4000;
							Game1.playSound("cowboy_powerup");
						}
						break;
					}
				case POWERUP_TYPE.LIFE:
					lives++;
					Game1.playSound("cowboy_powerup");
					break;
				case POWERUP_TYPE.NUKE:
					{
						Game1.playSound("cowboy_explosion");
						if (!shootoutLevel)
						{
							foreach (Enemy c2 in monsters)
							{
								AddGuts(c2.position.Location, c2.type);
							}
							monsters.Clear();
						}
						else
						{
							foreach (Enemy c in monsters)
							{
								c.TakeDamage(30);
								//bullets.Add(new CowboyBullet(this, c.position.Center, 2, 1));
								NETspawnBullet(true, c.position.Center, 2, 1);
							}
						}
						for (int i = 0; i < 30; i++)
						{
							temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(Game1.random.Next(1, 16), Game1.random.Next(1, 16)) * TileSize + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
							{
								delayBeforeAnimationStart = Game1.random.Next(800)
							});
						}
						break;
					}
				case POWERUP_TYPE.SPREAD:
				case POWERUP_TYPE.RAPIDFIRE:
				case POWERUP_TYPE.SHOTGUN:
					shotTimer = 0;
					Game1.playSound("cowboy_gunload");
					activePowerups.Add(which, powerupDuration + 2000);
					break;
				case POWERUP_TYPE.COIN:
					coins++;
					Game1.playSound("Pickup_Coin15");
					break;
				case POWERUP_TYPE.NICKEL:
					coins += 5;
					Game1.playSound("Pickup_Coin15");
					Game1.playSound("Pickup_Coin15");
					break;
				default:
					{
						activePowerups.Add(which, powerupDuration);
						Game1.playSound("cowboy_powerup");
						break;
					}
			}
			if (whichRound > 0 && activePowerups.ContainsKey(which))
			{
				activePowerups[which] /= 2;
			}
		}

		public void StartGopherTrain(ITEM_TYPE item = ITEM_TYPE.NONE)
        {
			if(item != ITEM_TYPE.NONE)
            {
				itemToHold = item;
				
			}
			holdItemTimer = 2000;
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
				bullets[m].position.X += bullets[m].motion.X;
				bullets[m].position.Y += bullets[m].motion.Y;
				if (bullets[m].position.X <= 0 || bullets[m].position.Y <= 0 || bullets[m].position.X >= 768 || bullets[m].position.Y >= 768)
				{
					//NET Despawn Bullet
					PK_BulletDespawned mBulletDespawned = new();
					mBulletDespawned.id = bullets[m].id;
					mBulletDespawned.isFriendly = true;
					modInstance.Helper.Multiplayer.SendMessage(mBulletDespawned, "PK_BulletDespawned");

					bullets.RemoveAt(m);
				}
				else if (map[bullets[m].position.X / 16 / 3, bullets[m].position.Y / 16 / 3] == MAP_TILE.FENCE)
				{
					//NET Despawn Bullet
					PK_BulletDespawned mBulletDespawned = new();
					mBulletDespawned.id = bullets[m].id;
					mBulletDespawned.isFriendly = true;
					modInstance.Helper.Multiplayer.SendMessage(mBulletDespawned, "PK_BulletDespawned");

					bullets.RemoveAt(m);
				}
				else
				{
					for (int k = monsters.Count - 1; k >= 0; k--)
					{
						if (monsters[k].position.Intersects(new Rectangle(bullets[m].position.X, bullets[m].position.Y, 12, 12)))
						{
							int monsterhealth = monsters[k].health;
							int monsterAfterDamageHealth;
							if (monsters[k].TakeDamage(bullets[m].damage))
							{
								monsterAfterDamageHealth = monsters[k].health;
								AddGuts(monsters[k].position.Location, monsters[k].type);
								POWERUP_TYPE loot = monsters[k].GetLootDrop();
								if (whichRound == 1 && Game1.random.NextDouble() < 0.5)
								{
									loot = POWERUP_TYPE.LOG;
								}
								if (whichRound > 0 && (loot == POWERUP_TYPE.ZOMBIE || loot == POWERUP_TYPE.LIFE) && Game1.random.NextDouble() < 0.4)
								{
									loot = POWERUP_TYPE.LOG;
								}
								if (loot != POWERUP_TYPE.LOG && whichWave != 12)
								{
									if(isHost)
                                    {
										powerups.Add(new Powerup(this, loot, monsters[k].position.Location, lootDuration));
									}
									
								}
								
								//NET EnemyKilled
								PK_EnemyKilled mEnemyKilled = new();
								mEnemyKilled.id = monsters[k].id;
								modInstance.Helper.Multiplayer.SendMessage(mEnemyKilled, "PK_EnemyKilled");
								Game1.playSound("Cowboy_monsterDie");

								monsters.RemoveAt(k);
							}
							else
							{
								monsterAfterDamageHealth = monsters[k].health;
							}
							bullets[m].damage -= monsterhealth - monsterAfterDamageHealth;
							if (bullets[m].damage <= 0)
							{
								//NET Despawn Bullet
								PK_BulletDespawned mBulletDespawned = new();
								mBulletDespawned.id = bullets[m].id;
								mBulletDespawned.isFriendly = true;
								modInstance.Helper.Multiplayer.SendMessage(mBulletDespawned, "PK_BulletDespawned");

								bullets.RemoveAt(m);
							}
							break;
						}
					}
				}
			}
			//Bro this is litterally the worst loop ive ever seen in my entire life whattheFUCK
			int l = enemyBullets.Count - 1;
			while (true)
			{
				if (l >= 0)
				{
					enemyBullets[l].position.X += enemyBullets[l].motion.X;
					enemyBullets[l].position.Y += enemyBullets[l].motion.Y;
					if (enemyBullets[l].position.X <= 0 || enemyBullets[l].position.Y <= 0 || enemyBullets[l].position.X >= 762 || enemyBullets[l].position.Y >= 762)
					{
						//NET Despawn Bullet
						PK_BulletDespawned mBulletDespawned = new();
						mBulletDespawned.id = enemyBullets[l].id;
						mBulletDespawned.isFriendly = false;
						modInstance.Helper.Multiplayer.SendMessage(mBulletDespawned, "PK_BulletDespawned");

						enemyBullets.RemoveAt(l);
					}
					else if (map[(enemyBullets[l].position.X + 6) / 16 / 3, (enemyBullets[l].position.Y + 6) / 16 / 3] == MAP_TILE.FENCE)
					{
						//NET Despawn Bullet
						PK_BulletDespawned mBulletDespawned = new();
						mBulletDespawned.id = enemyBullets[l].id;
						mBulletDespawned.isFriendly = false;
						modInstance.Helper.Multiplayer.SendMessage(mBulletDespawned, "PK_BulletDespawned");

						enemyBullets.RemoveAt(l);
					}
					else if (playerInvincibleTimer <= 0 && deathTimer <= 0f && playerBoundingBox.Intersects(new Rectangle(enemyBullets[l].position.X, enemyBullets[l].position.Y, 15, 15)))
					{
						break;
					}
					l--;
					continue;
				}
				return;
			}
			PlayerDie();

			//NET Player death
			PK_PlayerDeath message = new();
			message.id = Game1.player.UniqueMultiplayerID;
			modInstance.Helper.Multiplayer.SendMessage(message, "PK_PlayerDeath");
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
			enemyBullets.Clear();
			if (!shootoutLevel)
			{
				powerups.Clear();
				monsters.Clear();
			}
			died = true;
			activePowerups.Clear();
			deathTimer = 3000f;
			if (overworldSong != null && overworldSong.IsPlaying)
			{
				overworldSong.Stop(AudioStopOptions.Immediate);
			}
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, playerPosition + topLeftScreenCoordinate, flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
			waveTimer = Math.Min(80000, waveTimer + 10000);
			betweenWaveTimer = 4000;
			lives--;
			playerInvincibleTimer = 5000;
			if (shootoutLevel)
			{
				playerPosition = new Vector2(8 * TileSize, 3 * TileSize);

				//NET Player Move
				NETmovePlayer(playerPosition);

				Game1.playSound("Cowboy_monsterDie");
			}
			else
			{
				playerPosition = new Vector2(8 * TileSize - TileSize, 8 * TileSize);

				//NET Player Move
				NETmovePlayer(playerPosition);

				playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
				playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
				playerBoundingBox.Width = TileSize / 2;
				playerBoundingBox.Height = TileSize / 2;

				Game1.playSound("cowboy_dead");
			}
			if (lives < 0)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 550f, 5, 0, playerPosition + topLeftScreenCoordinate, flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0.001f,
					endFunction = AfterPlayerDeathFunction
				});
				deathTimer *= 3f;
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
			whichRound++;
		}

		public void StartLevelTransition()
        {
			SaveGame();
			shopping = false;
			merchantArriving = false;
			merchantLeaving = false;
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
			if (_buttonHeldFrames == null)
			{
				_buttonHeldFrames = new Dictionary<GameKeys, int>();
				for (int k = 0; k < 11; k++)
				{
					_buttonHeldFrames[(GameKeys)k] = 0;
				}
			}
			_buttonHeldState.Clear();
			if (startTimer <= 0)
			{
				UpdateInput();
			}
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
				startTimer = 0;
				return false;
			}
			if (onStartMenu)
			{
				if (startTimer > 0)
				{
					startTimer -= time.ElapsedGameTime.Milliseconds;
					if (startTimer <= 0)
					{
						shotTimer = 100;
						onStartMenu = false;
					}
				}
				else
				{
					Game1.playSound("Pickup_Coin15");
					startTimer = 1500;
				}
				return false;
			}
			if (gamerestartTimer > 0)
			{
				gamerestartTimer -= time.ElapsedGameTime.Milliseconds;
				if (gamerestartTimer <= 0)
				{
					unload();
					if (whichRound == 0 || !endCutscene)
					{
						Game1.currentMinigame = new GameMultiplayerPrairieKing(modInstance, isHost, playingWithAbigail);
					}
					else
					{
						Game1.currentMinigame = new GameMultiplayerPrairieKing(modInstance, isHost, playingWithAbigail, coins, ammoLevel, bulletDamage, fireSpeedLevel, runSpeedLevel, lives, spreadPistol, whichRound);
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
							Game1.getSteamAchievement("Achievement_PrairieKing");
							if (!died)
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
							playerPosition = new Vector2(0f, 8 * TileSize);

							//NET Player Move
							NETmovePlayer(playerPosition);

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
								StartNewRound();
							}
							break;
					}
				}
				if (endCutscenePhase == 2 && playerPosition.X < (float)(9 * TileSize))
				{
					playerPosition.X += 1f;
					playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
					playerMotionAnimationTimer %= 400f;
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
			if (holdItemTimer > 0)
			{
				holdItemTimer -= time.ElapsedGameTime.Milliseconds;
				return false;
			}
			if (screenFlash > 0)
			{
				screenFlash -= time.ElapsedGameTime.Milliseconds;
			}
			if (gopherTrain)
			{
				gopherTrainPosition += 3;
				if (gopherTrainPosition % 30 == 0)
				{
					Game1.playSound("Cowboy_Footstep");
				}
				if (playerJumped)
				{
					playerPosition.Y += 3f;
				}
				if (Math.Abs(playerPosition.Y - (float)(gopherTrainPosition - TileSize)) <= 16f)
				{
					playerJumped = true;
					playerPosition.Y = gopherTrainPosition - TileSize;
				}
				if (gopherTrainPosition > 16 * TileSize + TileSize)
				{
					gopherTrain = false;
					playerJumped = false;
					whichWave++;
					map = MapLoader.GetMap(whichWave);
					playerPosition = new Vector2(8 * TileSize, 8 * TileSize);

					//NET Player Move
					NETmovePlayer(playerPosition);

					world = ((world != 0) ? 1 : 2);
					waveTimer = 80000;
					betweenWaveTimer = 5000;
					waitingForPlayerToMoveDownAMap = false;
					shootoutLevel = false;
					SaveGame();
				}
			}
			if ((shopping || merchantArriving || merchantLeaving || waitingForPlayerToMoveDownAMap) && holdItemTimer <= 0)
			{
				int oldTimer = shoppingTimer;
				shoppingTimer += time.ElapsedGameTime.Milliseconds;
				shoppingTimer %= 500;
				if (!merchantShopOpen && shopping && ((oldTimer < 250 && shoppingTimer >= 250) || oldTimer > shoppingTimer))
				{
					Game1.playSound("Cowboy_Footstep");
				}
			}
			if (playerInvincibleTimer > 0)
			{
				playerInvincibleTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (scrollingMap)
			{
				newMapPosition -= TileSize / 8;

				playerPosition.Y -= TileSize / 8;
				playerPosition.Y += 3f;
				playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
				playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
				playerBoundingBox.Width = TileSize / 2;
				playerBoundingBox.Height = TileSize / 2;
				playerMovementDirections = new List<int>{2};
				playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
				playerMotionAnimationTimer %= 400f;


				player2Position.Y -= TileSize / 8;
				player2Position.Y += 3f;
				player2BoundingBox.X = (int)playerPosition.X + TileSize / 4;
				player2BoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
				player2BoundingBox.Width = TileSize / 2;
				player2BoundingBox.Height = TileSize / 2;
				player2MovementDirections = new List<int> { 2 };


				if (newMapPosition <= 0)
				{
					scrollingMap = false;
					map = nextMap;
					newMapPosition = 16 * TileSize;
					shopping = false;
					betweenWaveTimer = 5000;
					waitingForPlayerToMoveDownAMap = false;
					playerMovementDirections.Clear();
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
				for (int i2 = powerups.Count - 1; i2 >= 0; i2--)
				{
					if (Utility.distance(playerBoundingBox.Center.X, powerups[i2].position.X + TileSize / 2, playerBoundingBox.Center.Y, powerups[i2].position.Y + TileSize / 2) <= (float)(TileSize + 3) && (powerups[i2].position.X < TileSize || powerups[i2].position.X >= 16 * TileSize - TileSize || powerups[i2].position.Y < TileSize || powerups[i2].position.Y >= 16 * TileSize - TileSize))
					{
						if (powerups[i2].position.X + TileSize / 2 < playerBoundingBox.Center.X)
						{
							powerups[i2].position.X++;
						}
						if (powerups[i2].position.X + TileSize / 2 > playerBoundingBox.Center.X)
						{
							powerups[i2].position.X--;
						}
						if (powerups[i2].position.Y + TileSize / 2 < playerBoundingBox.Center.Y)
						{
							powerups[i2].position.Y++;
						}
						if (powerups[i2].position.Y + TileSize / 2 > playerBoundingBox.Center.Y)
						{
							powerups[i2].position.Y--;
						}
					}
					powerups[i2].duration -= time.ElapsedGameTime.Milliseconds;
					if (powerups[i2].duration <= 0)
					{
						powerups.RemoveAt(i2);
					}
				}
				for (int i4 = activePowerups.Count - 1; i4 >= 0; i4--)
				{
					activePowerups[activePowerups.ElementAt(i4).Key] -= time.ElapsedGameTime.Milliseconds;
					if (activePowerups[activePowerups.ElementAt(i4).Key] <= 0)
					{
						activePowerups.Remove(activePowerups.ElementAt(i4).Key);
					}
				}
				if (deathTimer <= 0f && playerMovementDirections.Count > 0 && !scrollingMap)
				{
					int effectiveDirections = playerMovementDirections.Count;
					if (effectiveDirections >= 2 && playerMovementDirections.Last() == (playerMovementDirections.ElementAt(playerMovementDirections.Count - 2) + 2) % 4)
					{
						effectiveDirections = 1;
					}
					float speed = GetMovementSpeed(3f, effectiveDirections);
					if (activePowerups.Keys.Contains(POWERUP_TYPE.SPEED))
					{
						speed *= 1.5f;
					}
					if (zombieModeTimer > 0)
					{
						speed *= 1.5f;
					}
					for (int i5 = 0; i5 < runSpeedLevel; i5++)
					{
						speed *= 1.25f;
					}
					for (int i6 = Math.Max(0, playerMovementDirections.Count - 2); i6 < playerMovementDirections.Count; i6++)
					{
						if (i6 != 0 || playerMovementDirections.Count < 2 || playerMovementDirections.Last() != (playerMovementDirections.ElementAt(playerMovementDirections.Count - 2) + 2) % 4)
						{
							Vector2 newPlayerPosition = playerPosition;
							switch (playerMovementDirections.ElementAt(i6))
							{
								case 0:
									newPlayerPosition.Y -= speed;
									break;
								case 3:
									newPlayerPosition.X -= speed;
									break;
								case 2:
									newPlayerPosition.Y += speed;
									break;
								case 1:
									newPlayerPosition.X += speed;
									break;
							}
							Rectangle newPlayerBox = new((int)newPlayerPosition.X + TileSize / 4, (int)newPlayerPosition.Y + TileSize / 4, TileSize / 2, TileSize / 2);
							//Stop players from colliding
							if (!IsCollidingWithMap(newPlayerBox) && (!merchantBox.Intersects(newPlayerBox) || merchantBox.Intersects(playerBoundingBox)))// && (!playingWithAbigail || !newPlayerBox.Intersects(player2BoundingBox)))
							{
								playerPosition = newPlayerPosition;

								//NET Player Move
								NETmovePlayer(playerPosition);
							}
						}
					}
					playerBoundingBox.X = (int)playerPosition.X + TileSize / 4;
					playerBoundingBox.Y = (int)playerPosition.Y + TileSize / 4;
					playerBoundingBox.Width = TileSize / 2;
					playerBoundingBox.Height = TileSize / 2;
					playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
					playerMotionAnimationTimer %= 400f;
					playerFootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
					if (playerFootstepSoundTimer <= 0f)
					{
						Game1.playSound("Cowboy_Footstep");
						playerFootstepSoundTimer = 200f;
					}
					for (int i7 = powerups.Count - 1; i7 >= 0; i7--)
					{
						if (playerBoundingBox.Intersects(new Rectangle(powerups[i7].position.X, powerups[i7].position.Y, TileSize, TileSize)) && !playerBoundingBox.Intersects(noPickUpBox))
						{
							//NET Pickup Powerup
							PK_PowerupPickup message = new();
							message.id = powerups[i7].id;
							message.which = (int)powerups[i7].which;
							modInstance.Helper.Multiplayer.SendMessage(message, "PK_PowerupPickup");

							if (heldItem != null)
							{
								UsePowerup(powerups[i7].which);
								powerups.RemoveAt(i7);
							}
							else if (GetPowerUp(powerups[i7]))
							{
								powerups.RemoveAt(i7);
							}
						}
					}
					if (!playerBoundingBox.Intersects(noPickUpBox))
					{
						noPickUpBox.Location = new Point(0, 0);
					}

					if (waitingForPlayerToMoveDownAMap && isHost)
					{
						float bottomBorder = 16 * TileSize - TileSize / 2;
						if(playerBoundingBox.Bottom >= bottomBorder && player2BoundingBox.Bottom >= bottomBorder)
                        {
							PK_StartLevelTransition message = new();
							modInstance.Helper.Multiplayer.SendMessage(message, "PK_StartLevelTransition");

							StartLevelTransition();
						}
					}
					if (!shoppingCarpetNoPickup.Intersects(playerBoundingBox))
					{
						shoppingCarpetNoPickup.X = -1000;
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
							Game1.playSound("cowboy_monsterhit");
							map[8, 15] = MAP_TILE.SAND;
							map[7, 15] = MAP_TILE.SAND;
							map[7, 15] = MAP_TILE.SAND;
							map[8, 14] = MAP_TILE.SAND;
							map[7, 14] = MAP_TILE.SAND;
							map[7, 14] = MAP_TILE.SAND;
							shoppingCarpetNoPickup = new Rectangle(merchantBox.X - TileSize, merchantBox.Y + TileSize, TileSize * 3, TileSize * 2);
						}
					}
					else if (merchantLeaving)
					{
						merchantBox.Y -= 2;
						if (merchantBox.Y <= -TileSize)
						{
							shopping = false;
							merchantLeaving = false;
							merchantArriving = true;
						}
					}
					else if (merchantShopOpen)
					{
						for (int i8 = storeItems.Count - 1; i8 >= 0; i8--)
						{
							if (!playerBoundingBox.Intersects(shoppingCarpetNoPickup) && playerBoundingBox.Intersects(storeItems.ElementAt(i8).Key) && coins >= GetPriceForItem(storeItems.ElementAt(i8).Value))
							{
								Game1.playSound("Cowboy_Secret");
								holdItemTimer = 2500;
								motionPause = 2500;
								itemToHold = storeItems.ElementAt(i8).Value;
								storeItems.Remove(storeItems.ElementAt(i8).Key);
								merchantLeaving = true;
								merchantArriving = false;
								merchantShopOpen = false;
								coins -= GetPriceForItem(itemToHold);
								switch (itemToHold)
								{
									case ITEM_TYPE.AMMO1:
									case ITEM_TYPE.AMMO2:
									case ITEM_TYPE.AMMO3:
										ammoLevel++;
										bulletDamage++;
										break;
									case ITEM_TYPE.FIRESPEED1:
									case ITEM_TYPE.FIRESPEED2:
									case ITEM_TYPE.FIRESPEED3:
										fireSpeedLevel++;
										break;
									case ITEM_TYPE.RUNSPEED1:
									case ITEM_TYPE.RUNSPEED2:
										runSpeedLevel++;
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
				if (shotTimer > 0)
				{
					shotTimer -= time.ElapsedGameTime.Milliseconds;
				}
				if (deathTimer <= 0f && playerShootingDirections.Count > 0 && shotTimer <= 0)
				{
					if (activePowerups.ContainsKey(POWERUP_TYPE.SPREAD))
					{
						SpawnBullets(new int[1], playerPosition);
						SpawnBullets(new int[1]
						{
							1
						}, playerPosition);
						SpawnBullets(new int[1]
						{
							2
						}, playerPosition);
						SpawnBullets(new int[1]
						{
							3
						}, playerPosition);
						SpawnBullets(new int[2]
						{
							0,
							1
						}, playerPosition);
						SpawnBullets(new int[2]
						{
							1,
							2
						}, playerPosition);
						SpawnBullets(new int[2]
						{
							2,
							3
						}, playerPosition);
						SpawnBullets(new int[2]
						{
							3,
							0
						}, playerPosition);
					}
					else if (playerShootingDirections.Count == 1 || playerShootingDirections.Last() == (playerShootingDirections.ElementAt(playerShootingDirections.Count - 2) + 2) % 4)
					{
						SpawnBullets(new int[1]
						{
							(playerShootingDirections.Count == 2 && playerShootingDirections.Last() == (playerShootingDirections.ElementAt(playerShootingDirections.Count - 2) + 2) % 4) ? playerShootingDirections.ElementAt(1) : playerShootingDirections.ElementAt(0)
						}, playerPosition);
					}
					else
					{
						SpawnBullets(playerShootingDirections.ToArray(), playerPosition);
					}
					Game1.playSound("Cowboy_gunshot");
					shotTimer = shootingDelay;
					if (activePowerups.ContainsKey(POWERUP_TYPE.RAPIDFIRE))
					{
						shotTimer /= 4;
					}
					for (int i3 = 0; i3 < fireSpeedLevel; i3++)
					{
						shotTimer = shotTimer * 3 / 4;
					}
					if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN))
					{
						shotTimer = shotTimer * 3 / 2;
					}
					shotTimer = Math.Max(shotTimer, 20);
				}
				UpdateBullets(time);
				foreach (Powerup powerup in powerups)
				{
					Vector2 tile_position = new((powerup.position.X + TileSize / 2) / TileSize, (powerup.position.Y + TileSize / 2) / TileSize);
					Vector2 corner_7 = new(powerup.position.X / TileSize, powerup.position.Y / TileSize);
					Vector2 corner_6 = new((powerup.position.X + TileSize) / TileSize, powerup.position.Y / TileSize);
					Vector2 corner_5 = new(powerup.position.X / TileSize, powerup.position.Y / TileSize);
					Vector2 corner_4 = new(powerup.position.X / TileSize, (powerup.position.Y + 64) / TileSize);
					if (_borderTiles.Contains(tile_position) || _borderTiles.Contains(corner_7) || _borderTiles.Contains(corner_6) || _borderTiles.Contains(corner_5) || _borderTiles.Contains(corner_4))
					{
						Point push_direction = default;
						if (Math.Abs(tile_position.X - 8f) > Math.Abs(tile_position.Y - 8f))
						{
							push_direction.X = Math.Sign(tile_position.X - 8f);
						}
						else
						{
							push_direction.Y = Math.Sign(tile_position.Y - 8f);
						}
						powerup.position.X -= push_direction.X;
						powerup.position.Y -= push_direction.Y;
					}
				}
				if (waveTimer > 0 && betweenWaveTimer <= 0 && zombieModeTimer <= 0 && !shootoutLevel && (overworldSong == null || !overworldSong.IsPlaying) && Game1.soundBank != null)
				{
					overworldSong = Game1.soundBank.GetCue("Cowboy_OVERWORLD");
					overworldSong.Play();
					Game1.musicPlayerVolume = Game1.options.musicVolumeLevel;
					Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
				}
				if (deathTimer > 0f)
				{
					deathTimer -= time.ElapsedGameTime.Milliseconds;
				}
				if (betweenWaveTimer > 0 && monsters.Count == 0 && IsSpawnQueueEmpty() && !shopping && !waitingForPlayerToMoveDownAMap)
				{
					betweenWaveTimer -= time.ElapsedGameTime.Milliseconds;
				}
				else if (deathTimer <= 0f && !waitingForPlayerToMoveDownAMap && !shopping && !shootoutLevel)
				{
					if (waveTimer > 0)
					{
						int oldWaveTimer = waveTimer;
						waveTimer -= time.ElapsedGameTime.Milliseconds;

						int u = 0;
						foreach (Vector2 v in monsterChances)
						{
							if (Game1.random.NextDouble() < (double)(v.X * (float)((monsters.Count != 0) ? 1 : 2)))
							{
								int numMonsters = 1;
								while (Game1.random.NextDouble() < (double)v.Y && numMonsters < 15)
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
							while ((IsCollidingWithMap(gopherBox) || IsCollidingWithMonster(gopherBox, null) || Math.Abs((float)gopherBox.X - playerPosition.X) < (float)(TileSize * 6) || Math.Abs((float)gopherBox.Y - playerPosition.Y) < (float)(TileSize * 6) || Math.Abs(gopherBox.X - 8 * TileSize) < TileSize * 4 || Math.Abs(gopherBox.Y - 8 * TileSize) < TileSize * 4) && tries2 < 10)
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

								if (whichRound > 0)
								{
									monsters.Last().health += whichRound * 2;
								}
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
												monsters.Add(new Enemy(this,spawnQueue[p].First().type, new Point(x * TileSize, 0)));

											if (whichRound > 0)
											{
												monsters.Last().health += whichRound * 2;
											}
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

											if (whichRound > 0)
											{
												monsters.Last().health += whichRound * 2;
											}
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

											if (whichRound > 0)
											{
												monsters.Last().health += whichRound * 2;
											}
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

											if (whichRound > 0)
											{
												monsters.Last().health += whichRound * 2;
											}
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
				if (playingWithAbigail)
				{
					UpdateAbigail(time);
				}
				for (int i = monsters.Count - 1; i >= 0; i--)
				{
					//TODO: Target closest player, for now: Target host
					if(isHost)
						monsters[i].Move(playerPosition, time);
					else
						monsters[i].Move(player2Position, time);


					if (i < monsters.Count && monsters[i].position.Intersects(playerBoundingBox) && playerInvincibleTimer <= 0)
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
					if (playingWithAbigail && i < monsters.Count && monsters[i].position.Intersects(player2BoundingBox) && player2invincibletimer <= 0)
					{
						//TODO player 2 death
						Game1.playSound("Cowboy_monsterDie");
						player2deathtimer = 3000;
						temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, player2Position + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
						player2invincibletimer = 4000;
						player2Position = new Vector2(8f, 8f) * TileSize;
						player2BoundingBox.X = (int)player2Position.X + TileSize / 4;
						player2BoundingBox.Y = (int)player2Position.Y + TileSize / 4;
						player2BoundingBox.Width = TileSize / 2;
						player2BoundingBox.Height = TileSize / 2;
						/*
						if (playerBoundingBox.Intersects(player2BoundingBox))
						{
							player2Position.X = playerBoundingBox.Right + 2;
						}
						*/
						player2BoundingBox.X = (int)player2Position.X + TileSize / 4;
						player2BoundingBox.Y = (int)player2Position.Y + TileSize / 4;
						player2BoundingBox.Width = TileSize / 2;
						player2BoundingBox.Height = TileSize / 2;
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
					if (whichRound > 0)
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
						if (whichRound > 0)
						{
							monsterChances[5] = new Vector2(0.01f + (float)whichRound * 0.004f, 0.15f + (float)whichRound * 0.04f);
						}
					}
					monsterChances[0] = Vector2.Zero;
					monsterChances[6] = Vector2.Zero;
					monsterChances[2] = new Vector2(monsterChances[2].X + 0.002f, monsterChances[2].Y + 0.02f);
					monsterChances[5] = new Vector2(monsterChances[5].X + 0.001f, monsterChances[5].Y + 0.02f);
					monsterChances[1] = new Vector2(monsterChances[1].X + 0.0018f, monsterChances[1].Y + 0.08f);
					if (whichRound > 0)
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
						if (whichRound > 0)
						{
							monsterChances[3] = new Vector2(0.012f + (float)whichRound * 0.005f, 0.4f + (float)whichRound * 0.075f);
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
			if (whichRound > 0)
			{
				for (int j = 0; j < monsterChances.Count; j++)
				{
					_ = monsterChances[j];
					monsterChances[j] *= 1.1f;
				}
			}
			if (whichWave > 0 && whichWave % 2 == 0)
			{
				StartShoppingLevel();
			}
			else if (whichWave > 0)
			{
				waitingForPlayerToMoveDownAMap = true;
				map[8, 15] = MAP_TILE.SAND;
				map[7, 15] = MAP_TILE.SAND;
				map[9, 15] = MAP_TILE.SAND;
			}
		}

		protected void ProcessInputs()
		{
			if (_buttonHeldFrames[GameKeys.MoveUp] > 0)
			{
				if (_buttonHeldFrames[GameKeys.MoveUp] == 1 && gameOver)
				{
					gameOverOption = Math.Max(0, gameOverOption - 1);
					Game1.playSound("Cowboy_gunshot");
				}
				AddPlayerMovementDirection(0);
			}
			else if (playerMovementDirections.Contains(0))
			{
				playerMovementDirections.Remove(0);
			}
			if (_buttonHeldFrames[GameKeys.MoveDown] > 0)
			{
				if (_buttonHeldFrames[GameKeys.MoveDown] == 1 && gameOver)
				{
					gameOverOption = Math.Min(1, gameOverOption + 1);
					Game1.playSound("Cowboy_gunshot");
				}
				AddPlayerMovementDirection(2);
			}
			else if (playerMovementDirections.Contains(2))
			{
				playerMovementDirections.Remove(2);
			}
			if (_buttonHeldFrames[GameKeys.MoveLeft] > 0)
			{
				AddPlayerMovementDirection(3);
			}
			else if (playerMovementDirections.Contains(3))
			{
				playerMovementDirections.Remove(3);
			}
			if (_buttonHeldFrames[GameKeys.MoveRight] > 0)
			{
				AddPlayerMovementDirection(1);
			}
			else if (playerMovementDirections.Contains(1))
			{
				playerMovementDirections.Remove(1);
			}
			if (_buttonHeldFrames[GameKeys.ShootUp] > 0)
			{
				AddPlayerShootingDirection(0);
			}
			else if (playerShootingDirections.Contains(0))
			{
				playerShootingDirections.Remove(0);
			}
			if (_buttonHeldFrames[GameKeys.ShootDown] > 0)
			{
				AddPlayerShootingDirection(2);
			}
			else if (playerShootingDirections.Contains(2))
			{
				playerShootingDirections.Remove(2);
			}
			if (_buttonHeldFrames[GameKeys.ShootLeft] > 0)
			{
				AddPlayerShootingDirection(3);
			}
			else if (playerShootingDirections.Contains(3))
			{
				playerShootingDirections.Remove(3);
			}
			if (_buttonHeldFrames[GameKeys.ShootRight] > 0)
			{
				AddPlayerShootingDirection(1);
			}
			else if (playerShootingDirections.Contains(1))
			{
				playerShootingDirections.Remove(1);
			}
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
			if (_buttonHeldFrames[GameKeys.UsePowerup] == 1 && !gameOver && heldItem != null && deathTimer <= 0f && zombieModeTimer <= 0)
			{
				UsePowerup(heldItem.which);
				heldItem = null;
			}
			if (_buttonHeldFrames[GameKeys.Exit] == 1)
			{
				quit = true;
			}
		}

		public virtual void ApplyLevelSpecificStates()
		{
			if (whichWave == 12)
			{
				shootoutLevel = true;

				if(isHost)
					monsters.Add(new Dracula(this));

				if (whichRound > 0)
				{
					monsters.Last().health *= 2;
				}
			}
			else if (whichWave > 0 && whichWave % 4 == 0)
			{
				shootoutLevel = true;

				if(isHost)
					monsters.Add(new Outlaw(this, new Point(8 * TileSize, 13 * TileSize), (world == 0) ? 50 : 100));

				if (Game1.soundBank != null)
				{
					outlawSong = Game1.soundBank.GetCue("cowboy_outlawsong");
					outlawSong.Play();
				}
			}
		}

		public void UpdateAbigail(GameTime time)
		{
			player2TargetUpdateTimer -= time.ElapsedGameTime.Milliseconds;
			if (player2deathtimer > 0)
			{
				player2deathtimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (player2invincibletimer > 0)
			{
				player2invincibletimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (player2deathtimer > 0)
			{
				return;
			}


			
			if (player2MovementDirections.Count > 0)
			{
				float speed = GetMovementSpeed(3f, player2MovementDirections.Count);
				for (int j = 0; j < player2MovementDirections.Count; j++)
				{
					Vector2 newPlayerPosition = player2Position;
					switch (player2MovementDirections[j])
					{
						case 0:
							newPlayerPosition.Y -= speed;
							break;
						case 3:
							newPlayerPosition.X -= speed;
							break;
						case 2:
							newPlayerPosition.Y += speed;
							break;
						case 1:
							newPlayerPosition.X += speed;
							break;
					}
					Rectangle newPlayerBox = new((int)newPlayerPosition.X + TileSize / 4, (int)newPlayerPosition.Y + TileSize / 4, TileSize / 2, TileSize / 2);
					if (!IsCollidingWithMap(newPlayerBox) && (!merchantBox.Intersects(newPlayerBox) || merchantBox.Intersects(player2BoundingBox)) && !newPlayerBox.Intersects(playerBoundingBox))
					{
						//DISABLE abigail movement
						//player2Position = newPlayerPosition;
					}
				}
				player2BoundingBox.X = (int)player2Position.X + TileSize / 4;
				player2BoundingBox.Y = (int)player2Position.Y + TileSize / 4;
				player2BoundingBox.Width = TileSize / 2;
				player2BoundingBox.Height = TileSize / 2;
				player2AnimationTimer += time.ElapsedGameTime.Milliseconds;
				player2AnimationTimer %= 400;
				player2FootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
				if (player2FootstepSoundTimer <= 0)
				{
					Game1.playSound("Cowboy_Footstep");
					player2FootstepSoundTimer = 200;
				}
				for (int i = powerups.Count - 1; i >= 0; i--)
				{
					if (player2BoundingBox.Intersects(new Rectangle(powerups[i].position.X, powerups[i].position.Y, TileSize, TileSize)) && !player2BoundingBox.Intersects(noPickUpBox))
					{
						powerups.RemoveAt(i);
					}
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
				//bullets.Add(new CowboyBullet(this, bulletSpawn, playerShootingDirection, bulletDamage));
				NETspawnBullet(true, bulletSpawn, playerShootingDirection, bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					switch (playerShootingDirection)
					{
						case 0:
							//bullets.Add(new CowboyBullet(this, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, -8), bulletDamage));
							//bullets.Add(new CowboyBullet(this, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, -8), bulletDamage));
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, -8), bulletDamage);
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, -8), bulletDamage);
							break;
						case 1:
							//bullets.Add(new CowboyBullet(this, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, -2), bulletDamage));
							//bullets.Add(new CowboyBullet(this, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, 2), bulletDamage));
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, -2), bulletDamage);
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, 2), bulletDamage);
							break;
						case 2:
							//bullets.Add(new CowboyBullet(this, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, 8), bulletDamage));
							//bullets.Add(new CowboyBullet(this, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, 8), bulletDamage));
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, 8), bulletDamage);
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, 8), bulletDamage);
							break;
						case 3:
							//bullets.Add(new CowboyBullet(this, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, -2), bulletDamage));
							//bullets.Add(new CowboyBullet(this, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, 2), bulletDamage));
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, -2), bulletDamage);
							NETspawnBullet(true, new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, 2), bulletDamage);
							break;
					}
				}
			}
			else if (directions.Contains(0) && directions.Contains(1))
			{
				bulletSpawn.X += TileSize / 2;
				bulletSpawn.Y -= TileSize / 2;
				//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(speed, -speed), bulletDamage));
				NETspawnBullet(true, bulletSpawn, new Point(speed, -speed), bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					int modifier8 = -2;
					//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(speed + modifier8, -speed + modifier8), bulletDamage));
					NETspawnBullet(true, bulletSpawn, new Point(speed + modifier8, -speed + modifier8), bulletDamage);
					modifier8 = 2;
					//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(speed + modifier8, -speed + modifier8), bulletDamage));
					NETspawnBullet(true, bulletSpawn, new Point(speed + modifier8, -speed + modifier8), bulletDamage);
				}
			}
			else if (directions.Contains(0) && directions.Contains(3))
			{
				bulletSpawn.X -= TileSize / 2;
				bulletSpawn.Y -= TileSize / 2;
				//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(-speed, -speed), bulletDamage));
				NETspawnBullet(true, bulletSpawn, new Point(-speed, -speed), bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					int modifier6 = -2;
					//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(-speed - modifier6, -speed + modifier6), bulletDamage));
					NETspawnBullet(true, bulletSpawn, new Point(-speed - modifier6, -speed + modifier6), bulletDamage);
					modifier6 = 2;
					//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(-speed - modifier6, -speed + modifier6), bulletDamage));
					NETspawnBullet(true, bulletSpawn, new Point(-speed - modifier6, -speed + modifier6), bulletDamage);
				}
			}
			else if (directions.Contains(2) && directions.Contains(1))
			{
				bulletSpawn.X += TileSize / 2;
				bulletSpawn.Y += TileSize / 4;
				//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(speed, speed), bulletDamage));
				NETspawnBullet(true, bulletSpawn, new Point(speed, speed), bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					int modifier4 = -2;
					//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(speed - modifier4, speed + modifier4), bulletDamage));
					NETspawnBullet(true, bulletSpawn, new Point(speed - modifier4, speed + modifier4), bulletDamage);
					modifier4 = 2;
					//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(speed - modifier4, speed + modifier4), bulletDamage));
					NETspawnBullet(true, bulletSpawn, new Point(speed - modifier4, speed + modifier4), bulletDamage);
				}
			}
			else if (directions.Contains(2) && directions.Contains(3))
			{
				bulletSpawn.X -= TileSize / 2;
				bulletSpawn.Y += TileSize / 4;
				//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(-speed, speed), bulletDamage));
				NETspawnBullet(true, bulletSpawn, new Point(-speed, speed), bulletDamage);
				if (activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN) || spreadPistol)
				{
					int modifier2 = -2;
					//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(-speed + modifier2, speed + modifier2), bulletDamage));
					NETspawnBullet(true, bulletSpawn, new Point(-speed + modifier2, speed + modifier2), bulletDamage);
					modifier2 = 2;
					//bullets.Add(new CowboyBullet(this, bulletSpawn, new Point(-speed + modifier2, speed + modifier2), bulletDamage));
					NETspawnBullet(true, bulletSpawn, new Point(-speed + modifier2, speed + modifier2), bulletDamage);
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


		private void AddPlayerMovementDirection(int direction)
		{
			if (!gopherTrain && !playerMovementDirections.Contains(direction))
			{
				if (playerMovementDirections.Count == 1)
				{
					_ = (playerMovementDirections.ElementAt(0) + 2) % 4;
				}
				playerMovementDirections.Add(direction);
			}
		}

		private void AddPlayerShootingDirection(int direction)
		{
			if (!playerShootingDirections.Contains(direction))
			{
				playerShootingDirections.Add(direction);
			}
		}

		public void StartShoppingLevel()
		{
			merchantBox.Y = -TileSize;
			shopping = true;
			merchantArriving = true;
			merchantLeaving = false;
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
			if (runSpeedLevel == 0) runSpeedItem = ITEM_TYPE.RUNSPEED1;
			else if (runSpeedLevel == 1) runSpeedItem = ITEM_TYPE.RUNSPEED2;
			else runSpeedItem = ITEM_TYPE.LIFE;

			ITEM_TYPE fireSpeedItem;
			if (fireSpeedLevel == 0) fireSpeedItem = ITEM_TYPE.FIRESPEED1;
			else if (fireSpeedLevel == 1) fireSpeedItem = ITEM_TYPE.FIRESPEED2;
			else if (fireSpeedLevel == 2) fireSpeedItem = ITEM_TYPE.FIRESPEED3;
			else if (ammoLevel >= 3 && !spreadPistol) fireSpeedItem = ITEM_TYPE.SPREADPISTOL;
			else fireSpeedItem = ITEM_TYPE.STAR;

			ITEM_TYPE ammoItem;
			if (ammoLevel == 0) ammoItem = ITEM_TYPE.AMMO1;
			else if (ammoLevel == 1) ammoItem = ITEM_TYPE.AMMO2;
			else if (ammoLevel == 2) ammoItem = ITEM_TYPE.AMMO3;
			else ammoItem = ITEM_TYPE.STAR;

			storeItems.Add(new Rectangle(7 * TileSize + 12, 8 * TileSize - TileSize * 2, TileSize, TileSize), runSpeedItem);
			storeItems.Add(new Rectangle(8 * TileSize + 24, 8 * TileSize - TileSize * 2, TileSize, TileSize), fireSpeedItem);
			storeItems.Add(new Rectangle(9 * TileSize + 36, 8 * TileSize - TileSize * 2, TileSize, TileSize), ammoItem);

		}

		public void receiveKeyPress(Keys k)
		{
			if (onStartMenu)
			{
				startTimer = 1;
			}
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
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.97f);
				b.Draw(Game1.mouseCursors, new Vector2(Game1.viewport.Width / 2 - 3 * TileSize, topLeftScreenCoordinate.Y + (float)(5 * TileSize)), new Rectangle(128, 1744, 96, 56), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
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
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White * ((endCutsceneTimer < 2000) ? (1f * ((float)endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + (int)itemToHold * 16, 1776, 16, 16), Color.White * ((endCutsceneTimer < 2000) ? (1f * ((float)endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.002f);
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
									b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(x, y) * 16f * 3f + new Vector2(0f, newMapPosition - 16 * TileSize), new Rectangle(464 + 16 * (int)map[x, y] + ((map[x, y] == MAP_TILE.CACTUS && cactusDanceTimer > 800f) ? 16 : 0), 1680 - world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
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
								b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(4f, 13f) * 3f, new Rectangle(484, 1760 + (int)(playerMotionAnimationTimer / 100f) * 3, 8, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f + 0.001f);
								b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition, new Rectangle(384, 1760, 16, 13), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.002f + 0.001f);
								b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize * 2 / 3 - TileSize / 4), new Rectangle(320 + (int)itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.005f);
							}
							b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black * ((endCutscenePhase == 1 && endCutsceneTimer > 12500) ? ((float)((endCutsceneTimer - 12500) / 3000)) : 0f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
							break;
						}
				}
			}
			else
			{
				if (zombieModeTimer > 8200)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition, new Rectangle(384 + ((zombieModeTimer / 200 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					for (int y = (int)(playerPosition.Y - (float)TileSize); y > -TileSize; y -= TileSize)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X, y), new Rectangle(368 + ((y / TileSize % 3 == 0) ? 16 : 0), 1744, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
					}
					b.End();
					return;
				}
				for (int x = 0; x < 16; x++)
				{
					for (int y = 0; y < 16; y++)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(x, y) * 16f * 3f + new Vector2(0f, newMapPosition - 16 * TileSize), new Rectangle(464 + 16 * (int)map[x, y] + ((map[x, y] == MAP_TILE.CACTUS && cactusDanceTimer > 800f) ? 16 : 0), 1680 - world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
					}
				}
				if (scrollingMap)
				{
					for (int x = 0; x < 16; x++)
					{
						for (int y = 0; y < 16; y++)
						{
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(x, y) * 16f * 3f + new Vector2(0f, newMapPosition), new Rectangle(464 + 16 * (int)nextMap[x, y] + ((nextMap[x, y] == MAP_TILE.CACTUS && cactusDanceTimer > 800f) ? 16 : 0), 1680 - world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
						}
					}
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, -1, 16 * TileSize, (int)topLeftScreenCoordinate.Y), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y + 16 * TileSize, 16 * TileSize, (int)topLeftScreenCoordinate.Y + 2), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
				if (deathTimer <= 0f && (playerInvincibleTimer <= 0 || playerInvincibleTimer / 100 % 2 == 0))
				{
					if (holdItemTimer > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + (int)itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.002f);
					}
					else if (zombieModeTimer > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(352 + ((zombieModeTimer / 50 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
					}
					else if (playerMovementDirections.Count == 0 && playerShootingDirections.Count == 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(496, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
					}
					else
					{
						int facingDirection = (playerShootingDirections.Count == 0) ? playerMovementDirections.ElementAt(0) : playerShootingDirections.Last();
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle(483, 1760 + (int)(playerMotionAnimationTimer / 100f) * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(3f, -TileSize / 4), new Rectangle(464 + facingDirection * 16, 1744, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.002f + 0.001f);
					}
				}

				//Draw player2 (abigail)
				if (playingWithAbigail && player2deathtimer <= 0 && (player2invincibletimer <= 0 || player2invincibletimer / 100 % 2 == 0))
				{
					if (player2MovementDirections.Count == 0 && player2ShootingDirections.Count == 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player2Position + new Vector2(0f, -TileSize / 4), new Rectangle(256, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, playerPosition.Y / 10000f + 0.001f);
					}
					else
					{
						int facingDirection2 = (player2ShootingDirections.Count == 0) ? player2MovementDirections[0] : player2ShootingDirections[0];
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player2Position + new Vector2(0f, -TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle(243, 1728 + player2AnimationTimer / 100 * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, player2Position.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + player2Position + new Vector2(0f, -TileSize / 4), new Rectangle(224 + facingDirection2 * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, player2Position.Y / 10000f + 0.002f + 0.001f);
					}

					//player2ShootingDirections.Clear();
					//player2MovementDirections.Clear();
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
				foreach (Bullet p2 in bullets)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(p2.position.X, p2.position.Y), new Rectangle(518, 1760 + (bulletDamage - 1) * 4, 4, 4), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
				}
				foreach (Bullet p in enemyBullets)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(p.position.X, p.position.Y), new Rectangle(523, 1760, 5, 5), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
				}

				//Draw shop
				if (shopping)
				{
					if ((merchantArriving || merchantLeaving) && !merchantShopOpen)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X, merchantBox.Location.Y), new Rectangle(464 + ((shoppingTimer / 100 % 2 == 0) ? 16 : 0), 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
					}
					else
					{
						int whichFrame = (playerBoundingBox.X - merchantBox.X > TileSize) ? 2 : ((merchantBox.X - playerBoundingBox.X > TileSize) ? 1 : 0);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X, merchantBox.Location.Y), new Rectangle(496 + whichFrame * 16, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(merchantBox.Location.X - TileSize, merchantBox.Location.Y + TileSize), new Rectangle(529, 1744, 63, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)merchantBox.Y / 10000f + 0.001f);
						foreach (KeyValuePair<Rectangle, ITEM_TYPE> v in storeItems)
						{
							b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(v.Key.Location.X, v.Key.Location.Y), new Rectangle(320 + (int)v.Value * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f);
							b.DrawString(Game1.smallFont, string.Concat(GetPriceForItem(v.Value)), topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(string.Concat(GetPriceForItem(v.Value))).X / 2f, v.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
							b.DrawString(Game1.smallFont, string.Concat(GetPriceForItem(v.Value)), topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(string.Concat(GetPriceForItem(v.Value))).X / 2f - 1f, v.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
							b.DrawString(Game1.smallFont, string.Concat(GetPriceForItem(v.Value)), topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + TileSize / 2) - Game1.smallFont.MeasureString(string.Concat(GetPriceForItem(v.Value))).X / 2f + 1f, v.Key.Location.Y + TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
						}
					}
				}
				if (waitingForPlayerToMoveDownAMap && (merchantShopOpen || merchantLeaving || !shopping) && shoppingTimer < 250)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(8.5f, 15f) * TileSize + new Vector2(-12f, 0f), new Rectangle(355, 1750, 8, 8), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.001f);
				}

				// Draw all monsters
				foreach (Enemy monster in monsters)
				{
					monster.Draw(b);
				}

				if (gopherRunning)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(gopherBox.X, gopherBox.Y), new Rectangle(320 + waveTimer / 100 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)gopherBox.Y / 10000f + 0.001f);
				}
				if (gopherTrain && gopherTrainPosition > -TileSize)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y, 16 * TileSize, 16 * TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X - (float)(TileSize / 2), gopherTrainPosition), new Rectangle(384 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X + (float)(TileSize / 2), gopherTrainPosition), new Rectangle(384 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X, gopherTrainPosition - TileSize * 3), new Rectangle(320 + gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(playerPosition.X - (float)(TileSize / 2), gopherTrainPosition - TileSize), new Rectangle(400, 1728, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.97f);
					if (holdItemTimer > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + (int)itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + playerPosition + new Vector2(0f, -TileSize / 4), new Rectangle(464, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
					}
				}
				else
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize + 27, 0f), new Rectangle(294, 1782, 22, 22), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.25f);
					if (heldItem != null)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize + 18, -9f), new Rectangle(272 + (int)heldItem.which * 16, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize * 2, -TileSize - 18), new Rectangle(400, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					b.DrawString(Game1.smallFont, "x" + Math.Max(lives, 0), topLeftScreenCoordinate - new Vector2(TileSize, -TileSize - TileSize / 4 - 18), Color.White);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate - new Vector2(TileSize * 2, -TileSize * 2 - 18), new Rectangle(272, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					b.DrawString(Game1.smallFont, "x" + coins, topLeftScreenCoordinate - new Vector2(TileSize, -TileSize * 2 - TileSize / 4 - 18), Color.White);
					for (int j = 0; j < whichWave + whichRound * 12; j++)
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
					if (bulletDamage > 1)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize), new Rectangle(416 + (ammoLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (fireSpeedLevel > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize * 2), new Rectangle(320 + (fireSpeedLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
					}
					if (runSpeedLevel > 0)
					{
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(-TileSize - 3, 16 * TileSize - TileSize * 3), new Rectangle(368 + (runSpeedLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
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

			//Draw Fadeout before quit
			if (fadethenQuitTimer > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Game1.staminaRect.Bounds, Color.Black * (1f - (float)fadethenQuitTimer / 2000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
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
