using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MultiPlayerPrairie;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;

namespace MultiplayerPrairieKing.Entities.Enemies
{
	public class Outlaw : Enemy
	{
		public const int talkingPhase = -1;

		public const int hidingPhase = 0;

		public const int dartOutAndShootPhase = 1;

		public const int runAndGunPhase = 2;

		public const int runGunAndPantPhase = 3;

		public const int shootAtPlayerPhase = 4;

		public int phase;

		public int phaseCountdown;

		public int shootTimer;

		public int phaseInternalTimer;

		public int phaseInternalCounter;

		public bool dartLeft;

		public int fullHealth;

		public Point homePosition;

		public Outlaw(GameMultiplayerPrairieKing game, Point position, int health)
			: base(game, MONSTER_TYPE.outlaw, position)
		{
			homePosition = position;
			base.health = health;
			fullHealth = health;
			phaseCountdown = 4000;
			phase = -1;


		}

		public override void Draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle((int)topLeftScreenCoordinate.X, (int)topLeftScreenCoordinate.Y + 16 * TileSize + 3, (int)((float)(16 * TileSize) * ((float)health / (float)fullHealth)), TileSize / 3), new Color(188, 51, 74));
			if (flashColorTimer > 0f)
			{
				b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(496, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				return;
			}
			int num = phase;
			if ((uint)(num - -1) <= 1u)
			{
				b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(560 + ((phaseCountdown / 250 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
				if (phase == -1 && phaseCountdown > 1000)
				{
					b.Draw(
						Game1.mouseCursors,
						topLeftScreenCoordinate + new Vector2(position.X - TileSize / 2, position.Y - TileSize * 2),
						new Rectangle(576 + ((gameInstance.whichWave > 5) ? 32 : 0), 1792, 32, 32),
						Color.White,
						0f,
						Vector2.Zero,
						3f,
						SpriteEffects.None,
						(float)position.Y / 10000f + 0.001f
					);
				}
			}
			else if (phase == 3 && phaseInternalCounter == 2)
			{
				b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(560 + ((phaseCountdown / 250 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(592 + ((phaseCountdown / 80 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
			}
		}

		public override bool Move(Vector2 playerPosition, GameTime time)
		{
			if (flashColorTimer > 0f)
			{
				flashColorTimer -= time.ElapsedGameTime.Milliseconds;
			}
			phaseCountdown -= time.ElapsedGameTime.Milliseconds;
			if (position.X > 17 * TileSize || position.X < -TileSize)
			{
				position.X = 16 * TileSize / 2;
			}
			switch (phase)
			{
				case -1:
				case 0:
					if (phaseCountdown >= 0)
					{
						break;
					}
					phase = Game1.random.Next(1, 5);
					dartLeft = (playerPosition.X < (float)position.X);
					if (playerPosition.X > (float)(7 * TileSize) && playerPosition.X < (float)(9 * TileSize))
					{
						if (Game1.random.NextDouble() < 0.66 || phase == 2)
						{
							phase = 4;
						}
					}
					else if (phase == 4)
					{
						phase = 3;
					}
					phaseInternalCounter = 0;
					phaseInternalTimer = 0;
					break;
				case 4:
					{
						int motion4 = dartLeft ? (-3) : 3;
						if (phaseInternalCounter == 0 && (!(playerPosition.X > (float)(7 * TileSize)) || !(playerPosition.X < (float)(9 * TileSize))))
						{
							phaseInternalCounter = 1;
							phaseInternalTimer = Game1.random.Next(500, 1500);
							break;
						}
						if (Math.Abs(position.Location.X - homePosition.X + TileSize / 2) < TileSize * 7 + 12 && phaseInternalCounter == 0)
						{
							position.X += motion4;
							break;
						}
						if (phaseInternalCounter == 2)
						{
							motion4 = (dartLeft ? (-4) : 4);
							position.X -= motion4;
							if (Math.Abs(position.X - homePosition.X) < 4)
							{
								position.X = homePosition.X;
								phase = 0;
								phaseCountdown = Game1.random.Next(1000, 2000);
							}
							break;
						}
						if (phaseInternalCounter == 0)
						{
							phaseInternalCounter++;
							phaseInternalTimer = Game1.random.Next(1000, 2000);
						}
						phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
						shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (shootTimer < 0)
						{
							Vector2 trajectory = Utility.getVelocityTowardPoint(new Point(position.X + TileSize / 2, position.Y), playerPosition + new Vector2(TileSize / 2, TileSize / 2), 8f);
							//gameInstance.enemyBullets.Add(new CowboyBullet(gameInstance, new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point((int)trajectory.X, (int)trajectory.Y), 1));
							gameInstance.NETspawnBullet(false, new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point((int)trajectory.X, (int)trajectory.Y), 1);
							shootTimer = 120;
							Game1.playSound("Cowboy_gunshot");
						}
						if (phaseInternalTimer <= 0)
						{
							phaseInternalCounter++;
						}
						break;
					}
				case 1:
					{
						int motion4 = dartLeft ? (-3) : 3;
						if (Math.Abs(position.Location.X - homePosition.X + TileSize / 2) < TileSize * 2 + 12 && phaseInternalCounter == 0)
						{
							position.X += motion4;
							if (position.X > 256)
							{
								phaseInternalCounter = 2;
							}
							break;
						}
						if (phaseInternalCounter == 2)
						{
							position.X -= motion4;
							if (Math.Abs(position.X - homePosition.X) < 4)
							{
								position.X = homePosition.X;
								phase = 0;
								phaseCountdown = Game1.random.Next(1000, 2000);
							}
							break;
						}
						if (phaseInternalCounter == 0)
						{
							phaseInternalCounter++;
							phaseInternalTimer = Game1.random.Next(1000, 2000);
						}
						phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
						shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (shootTimer < 0)
						{
							//gameInstance.enemyBullets.Add(new CowboyBullet(gameInstance, new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-2, 3), -8), 1));
							gameInstance.NETspawnBullet(false, new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-2, 3), -8), 1);
							shootTimer = 150;
							Game1.playSound("Cowboy_gunshot");
						}
						if (phaseInternalTimer <= 0)
						{
							phaseInternalCounter++;
						}
						break;
					}
				case 2:
					if (phaseInternalCounter == 2)
					{
						if (position.X < homePosition.X)
						{
							position.X += 4;
						}
						else
						{
							position.X -= 4;
						}
						if (Math.Abs(position.X - homePosition.X) < 5)
						{
							position.X = homePosition.X;
							phase = 0;
							phaseCountdown = Game1.random.Next(1000, 2000);
						}
						return false;
					}
					if (phaseInternalCounter == 0)
					{
						phaseInternalCounter++;
						phaseInternalTimer = Game1.random.Next(4000, 7000);
					}
					phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					if ((float)position.X > playerPosition.X && (float)position.X - playerPosition.X > 3f)
					{
						position.X -= 2;
					}
					else if ((float)position.X < playerPosition.X && playerPosition.X - (float)position.X > 3f)
					{
						position.X += 2;
					}
					shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (shootTimer < 0)
					{
						//gameInstance.enemyBullets.Add(new CowboyBullet(gameInstance, new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1));
						gameInstance.NETspawnBullet(false, new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1);
						shootTimer = 250;
						if (fullHealth > 50)
						{
							shootTimer -= 50;
						}
						if (Game1.random.NextDouble() < 0.2)
						{
							shootTimer = 150;
						}
						Game1.playSound("Cowboy_gunshot");
					}
					if (phaseInternalTimer <= 0)
					{
						phaseInternalCounter++;
					}
					break;
				case 3:
					{
						if (phaseInternalCounter == 0)
						{
							phaseInternalCounter++;
							phaseInternalTimer = Game1.random.Next(3000, 6500);
							break;
						}
						if (phaseInternalCounter == 2)
						{
							phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
							if (phaseInternalTimer <= 0)
							{
								phaseInternalCounter++;
							}
							break;
						}
						if (phaseInternalCounter == 3)
						{
							if (position.X < homePosition.X)
							{
								position.X += 4;
							}
							else
							{
								position.X -= 4;
							}
							if (Math.Abs(position.X - homePosition.X) < 5)
							{
								position.X = homePosition.X;
								phase = 0;
								phaseCountdown = Game1.random.Next(1000, 2000);
							}
							break;
						}
						int motion4 = dartLeft ? (-3) : 3;
						position.X += motion4;
						if (position.X < TileSize || position.X > 15 * TileSize)
						{
							dartLeft = !dartLeft;
						}
						shootTimer -= time.ElapsedGameTime.Milliseconds;
						if (shootTimer < 0)
						{
							//gameInstance.enemyBullets.Add(new CowboyBullet(gameInstance, new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1));
							gameInstance.NETspawnBullet(false, new Point(position.X + TileSize / 2, position.Y - TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1);
							shootTimer = 250;
							if (fullHealth > 50)
							{
								shootTimer -= 50;
							}
							if (Game1.random.NextDouble() < 0.2)
							{
								shootTimer = 150;
							}
							Game1.playSound("Cowboy_gunshot");
						}
						phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
						if (phaseInternalTimer <= 0)
						{
							if (phase == 2)
							{
								phaseInternalCounter = 3;
								break;
							}
							phaseInternalTimer = 3000;
							phaseInternalCounter++;
						}
						break;
					}
			}
			if (position.X <= 16 * TileSize)
			{
				_ = position.X;
				_ = 0;
			}
			return false;
		}

		public override POWERUP_TYPE GetLootDrop()
		{
			return POWERUP_TYPE.LIFE;
		}

		public override void OnDeath()
		{
			if (gameInstance.isHost)
			{
                gameInstance.powerups.Add(new Powerup(gameInstance, (gameInstance.world == 0) ? POWERUP_TYPE.LOG : POWERUP_TYPE.SKULL, new Point(8 * TileSize, 10 * TileSize), 9999999));
			}

			if (outlawSong != null && outlawSong.IsPlaying)
			{
				outlawSong.Stop(AudioStopOptions.Immediate);
			}
			gameInstance.map[8, 8] = MAP_TILE.BRIDGE;
			gameInstance.screenFlash = 200;
			for (int i = 0; i < 15; i++)
			{
				gameInstance.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(position.X + Game1.random.Next(-TileSize, TileSize), position.Y + Game1.random.Next(-TileSize, TileSize)) + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = i * 75
				});
			}
		}

		public override bool TakeDamage(int damage)
		{
			if (Math.Abs(position.X - homePosition.X) < 5)
			{
				return false;
			}
			health -= damage;
			if (health < 0)
			{
				OnDeath();
				return true;
			}
			flashColorTimer = 150f;
			Game1.playSound("cowboy_monsterhit");
			return false;
		}
	}
}
