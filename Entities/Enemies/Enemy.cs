using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiPlayerPrairie;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;

namespace MultiplayerPrairieKing.Entities
{
	public class Enemy
	{
		public GameMultiplayerPrairieKing gameInstance;

		public long id;

		public const int MonsterAnimationDelay = 500;

		public int health;

		public MONSTER_TYPE type;

		public int speed;

		public float movementAnimationTimer;

		public Rectangle position;

		public int movementDirection;

		public bool movedLastTurn;

		public bool oppositeMotionGuy;

		public bool invisible;

		public bool special;

		public bool uninterested;

		public bool flyer;

		public Color tint = Color.White;

		public Color flashColor = Color.Red;

		public float flashColorTimer;

		public int ticksSinceLastMovement;

		public Vector2 acceleration;

		private Point targetPosition;


		/*public CowboyMonster(GameMultiplayerPrairieKing game, int which, int health, int speed, Point position)
		{
			this.gameInstance = game;
			this.health = health;
			type = which;
			this.speed = speed;
			this.position = new Rectangle(position.X, position.Y, TileSize, TileSize);
			uninterested = (Game1.random.NextDouble() < 0.25);
		}*/

		public Enemy(GameMultiplayerPrairieKing game, MONSTER_TYPE which, Point position)
		{
			this.gameInstance = game;


			type = which;
			this.position = new Rectangle(position.X, position.Y, TileSize, TileSize);
			switch (type)
			{
				case MONSTER_TYPE.orc:
					speed = 2;
					health = 1;
					uninterested = (Game1.random.NextDouble() < 0.25);
					if (uninterested)
					{
						targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
					}
					break;
				case MONSTER_TYPE.ogre:
					speed = 1;
					health = 3;
					break;
				case MONSTER_TYPE.mushroom:
					speed = 3;
					health = 2;
					break;
				case MONSTER_TYPE.ghost:
					speed = 2;
					health = 1;
					flyer = true;
					break;
				case MONSTER_TYPE.mummy:
					health = 6;
					speed = 1;
					uninterested = (Game1.random.NextDouble() < 0.25);
					if (uninterested)
					{
						targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
					}
					break;
				case MONSTER_TYPE.devil:
					health = 3;
					speed = 3;
					flyer = true;
					break;
				case MONSTER_TYPE.spikey:
					{
						speed = 3;
						health = 2;
						int tries = 0;
						do
						{
							targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
							tries++;
						}
						while (gameInstance.IsCollidingWithMap(targetPosition) && tries < 10);
						break;
					}
			}
			oppositeMotionGuy = (Game1.random.NextDouble() < 0.5);


			//NET spawn outlaw
			if (gameInstance.isHost)
			{
				this.id = gameInstance.modInstance.Helper.Multiplayer.GetNewID();

				PK_EnemySpawn message = new();
				message.id = this.id;
				message.which = (int)which;
				message.position = position;
				game.modInstance.Helper.Multiplayer.SendMessage(message, "PK_EnemySpawn");
			}
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (type == MONSTER_TYPE.spikey && special)
			{
				if (flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(480, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(576, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				}
			}
			else if (!invisible)
			{
				if (flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(352 + (int)type * 16, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(352 + ((int)type * 2 + ((movementAnimationTimer < 250f) ? 1 : 0)) * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				}
				if (gameInstance.monsterConfusionTimer > 0)
				{
					b.DrawString(Game1.smallFont, "?", topLeftScreenCoordinate + new Vector2((float)(position.X + TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f, position.Y - TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)position.Y / 10000f);
					b.DrawString(Game1.smallFont, "?", topLeftScreenCoordinate + new Vector2((float)(position.X + TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f + 1f, position.Y - TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)position.Y / 10000f);
					b.DrawString(Game1.smallFont, "?", topLeftScreenCoordinate + new Vector2((float)(position.X + TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f - 1f, position.Y - TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)position.Y / 10000f);
				}
			}
		}

		public virtual bool TakeDamage(int damage)
		{
			health -= damage;
			health = Math.Max(0, health);
			if (health <= 0)
			{
				return true;
			}
			Game1.playSound("cowboy_monsterhit");
			flashColor = Color.Red;
			flashColorTimer = 100f;
			return false;
		}

		public virtual POWERUP_TYPE GetLootDrop()
		{
			if (type == MONSTER_TYPE.spikey && special)
			{
				return POWERUP_TYPE.LOG;
			}
			if (Game1.random.NextDouble() < 0.05)
			{
				if (type != 0 && Game1.random.NextDouble() < 0.1)
				{
					return POWERUP_TYPE.NICKEL;
				}
				if (Game1.random.NextDouble() < 0.01)
				{
					return POWERUP_TYPE.NICKEL;
				}
				return POWERUP_TYPE.COIN;
			}
			if (Game1.random.NextDouble() < 0.05)
			{
				if (Game1.random.NextDouble() < 0.15)
				{
					return (POWERUP_TYPE)Game1.random.Next(6, 8);
				}
				if (Game1.random.NextDouble() < 0.07)
				{
					return POWERUP_TYPE.SHERRIFF;
				}

				int loot = Game1.random.Next(2, 10);
				if (loot == 5 && Game1.random.NextDouble() < 0.4)
				{
					loot = Game1.random.Next(2, 10);
				}
				return (POWERUP_TYPE)loot;
			}
			return POWERUP_TYPE.LOG;
		}

		public virtual bool Move(Vector2 playerPosition, GameTime time)
		{
			movementAnimationTimer -= time.ElapsedGameTime.Milliseconds;
			if (movementAnimationTimer <= 0f)
			{
				movementAnimationTimer = Math.Max(100, 500 - speed * 50);
			}
			if (flashColorTimer > 0f)
			{
				flashColorTimer -= time.ElapsedGameTime.Milliseconds;
				return false;
			}
			if (gameInstance.monsterConfusionTimer > 0)
			{
				return false;
			}
			if (gameInstance.shopping)
			{
				gameInstance.shoppingTimer -= time.ElapsedGameTime.Milliseconds;
				if (gameInstance.shoppingTimer <= 0)
				{
					gameInstance.shoppingTimer = 100;
				}
			}
			ticksSinceLastMovement++;
			switch (type)
			{
				case MONSTER_TYPE.orc:
				case MONSTER_TYPE.ogre:
				case MONSTER_TYPE.mummy:
				case MONSTER_TYPE.mushroom:
				case MONSTER_TYPE.spikey:
					{
						if (type == MONSTER_TYPE.spikey)
						{
							if (special || invisible)
							{
								break;
							}
							if (ticksSinceLastMovement > 20)
							{
								int tries2 = 0;
								do
								{
									targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
									tries2++;
								}
								while (gameInstance.IsCollidingWithMap(targetPosition) && tries2 < 5);
							}
						}
						else if (ticksSinceLastMovement > 20)
						{
							int tries = 0;
							do
							{
								oppositeMotionGuy = !oppositeMotionGuy;
								targetPosition = new Point(Game1.random.Next(position.X - TileSize * 2, position.X + TileSize * 2), Game1.random.Next(position.Y - TileSize * 2, position.Y + TileSize * 2));
								tries++;
							}
							while (gameInstance.IsCollidingWithMap(targetPosition) && tries < 5);
						}

						Vector2 target2 = (!targetPosition.Equals(Point.Zero)) ? new Vector2(targetPosition.X, targetPosition.Y) : playerPosition;
						if (gameInstance.playingWithAbigail && target2.Equals(playerPosition))
						{
							double distanceToPlayer = Math.Sqrt(Math.Pow((float)position.X - target2.X, 2.0) - Math.Pow((float)position.Y - target2.Y, 2.0));
							if (Math.Sqrt(Math.Pow((float)position.X - gameInstance.player2Position.X, 2.0) - Math.Pow((float)position.Y - gameInstance.player2Position.Y, 2.0)) < distanceToPlayer)
							{
								target2 = gameInstance.player2Position;
							}
						}
						if (gameInstance.gopherRunning)
						{
							target2 = new Vector2(gameInstance.gopherBox.X, gameInstance.gopherBox.Y);
						}
						if (Game1.random.NextDouble() < 0.001)
						{
							oppositeMotionGuy = !oppositeMotionGuy;
						}
						if ((type == MONSTER_TYPE.spikey && !oppositeMotionGuy) || Math.Abs(target2.X - (float)position.X) > Math.Abs(target2.Y - (float)position.Y))
						{
							if (target2.X + (float)speed < (float)position.X && (movedLastTurn || movementDirection != 3))
							{
								movementDirection = 3;
							}
							else if (target2.X > (float)(position.X + speed) && (movedLastTurn || movementDirection != 1))
							{
								movementDirection = 1;
							}
							else if (target2.Y > (float)(position.Y + speed) && (movedLastTurn || movementDirection != 2))
							{
								movementDirection = 2;
							}
							else if (target2.Y + (float)speed < (float)position.Y && (movedLastTurn || movementDirection != 0))
							{
								movementDirection = 0;
							}
						}
						else if (target2.Y > (float)(position.Y + speed) && (movedLastTurn || movementDirection != 2))
						{
							movementDirection = 2;
						}
						else if (target2.Y + (float)speed < (float)position.Y && (movedLastTurn || movementDirection != 0))
						{
							movementDirection = 0;
						}
						else if (target2.X + (float)speed < (float)position.X && (movedLastTurn || movementDirection != 3))
						{
							movementDirection = 3;
						}
						else if (target2.X > (float)(position.X + speed) && (movedLastTurn || movementDirection != 1))
						{
							movementDirection = 1;
						}
						movedLastTurn = false;
						Rectangle attemptedPosition = position;
						switch (movementDirection)
						{
							case 0:
								attemptedPosition.Y -= speed;
								break;
							case 1:
								attemptedPosition.X += speed;
								break;
							case 2:
								attemptedPosition.Y += speed;
								break;
							case 3:
								attemptedPosition.X -= speed;
								break;
						}
						if (gameInstance.zombieModeTimer > 0)
						{
							attemptedPosition.X = position.X - (attemptedPosition.X - position.X);
							attemptedPosition.Y = position.Y - (attemptedPosition.Y - position.Y);
						}

						//Ogers stomp spikeys
						if (type == MONSTER_TYPE.ogre)
						{
							for (int i = gameInstance.monsters.Count - 1; i >= 0; i--)
							{
								if (gameInstance.monsters[i].type == MONSTER_TYPE.spikey && gameInstance.monsters[i].special && gameInstance.monsters[i].position.Intersects(attemptedPosition))
								{
									//Net EnemyKilled
									PK_EnemyKilled message = new();
									message.id = gameInstance.monsters[i].id;
									gameInstance.modInstance.Helper.Multiplayer.SendMessage(message, "PK_EnemyKilled");

									gameInstance.AddGuts(gameInstance.monsters[i].position.Location, gameInstance.monsters[i].type);
									Game1.playSound("Cowboy_monsterDie");
									gameInstance.monsters.RemoveAt(i);
								}
							}
						}
						if (gameInstance.IsCollidingWithMapForMonsters(attemptedPosition) || gameInstance.IsCollidingWithMonster(attemptedPosition, this) || !(gameInstance.deathTimer <= 0f))
						{
							break;
						}
						ticksSinceLastMovement = 0;
						position = attemptedPosition;
						movedLastTurn = true;
						if (!position.Contains((int)target2.X + TileSize / 2, (int)target2.Y + TileSize / 2))
						{
							break;
						}
						targetPosition = Point.Zero;
						if ((type == MONSTER_TYPE.orc || type == MONSTER_TYPE.mummy) && uninterested)
						{
							targetPosition = new Point(Game1.random.Next(2, 14) * TileSize, Game1.random.Next(2, 14) * TileSize);
							if (Game1.random.NextDouble() < 0.5)
							{
								uninterested = false;
								targetPosition = Point.Zero;
							}
						}
						if (type == MONSTER_TYPE.spikey && !invisible)
						{
							gameInstance.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(352, 1728, 16, 16), 60f, 3, 0, new Vector2(position.X, position.Y) + topLeftScreenCoordinate, flicker: false, flipped: false, (float)position.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
							{
								endFunction = SpikeyEndBehavior
							});
							invisible = true;
						}
						break;
					}
				case MONSTER_TYPE.ghost:
				case MONSTER_TYPE.devil:
					{
						if (ticksSinceLastMovement > 20)
						{
							int tries3 = 0;
							do
							{
								oppositeMotionGuy = !oppositeMotionGuy;
								targetPosition = new Point(Game1.random.Next(position.X - TileSize * 2, position.X + TileSize * 2), Game1.random.Next(position.Y - TileSize * 2, position.Y + TileSize * 2));
								tries3++;
							}
							while (gameInstance.IsCollidingWithMap(targetPosition) && tries3 < 5);
						}
						_ = targetPosition;
						Vector2 target2 = (!targetPosition.Equals(Point.Zero)) ? new Vector2(targetPosition.X, targetPosition.Y) : playerPosition;
						Vector2 targetToFly = Utility.getVelocityTowardPoint(position.Location, target2 + new Vector2(TileSize / 2, TileSize / 2), speed);
						float accelerationMultiplyer = (targetToFly.X != 0f && targetToFly.Y != 0f) ? 1.5f : 1f;
						if (targetToFly.X > acceleration.X)
						{
							acceleration.X += 0.1f * accelerationMultiplyer;
						}
						if (targetToFly.X < acceleration.X)
						{
							acceleration.X -= 0.1f * accelerationMultiplyer;
						}
						if (targetToFly.Y > acceleration.Y)
						{
							acceleration.Y += 0.1f * accelerationMultiplyer;
						}
						if (targetToFly.Y < acceleration.Y)
						{
							acceleration.Y -= 0.1f * accelerationMultiplyer;
						}
						if (!gameInstance.IsCollidingWithMonster(new Rectangle(position.X + (int)Math.Ceiling(acceleration.X), position.Y + (int)Math.Ceiling(acceleration.Y), TileSize, TileSize), this) && gameInstance.deathTimer <= 0f)
						{
							ticksSinceLastMovement = 0;
							position.X += (int)Math.Ceiling(acceleration.X);
							position.Y += (int)Math.Ceiling(acceleration.Y);
							if (position.Contains((int)target2.X + TileSize / 2, (int)target2.Y + TileSize / 2))
							{
								targetPosition = Point.Zero;
							}
						}
						break;
					}
			}
			return false;
		}

		public void SpikeyEndBehavior(int extraInfo)
		{
			invisible = false;
			health += 5;
			special = true;
		}

		public virtual void OnDeath()
		{

		}
	}
}
