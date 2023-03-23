using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiPlayerPrairie;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;

namespace MultiplayerPrairieKing.Entities
{
	public class Player
	{
		GameMultiplayerPrairieKing gameInstance;

		public int runSpeedLevel;
		public int fireSpeedLevel;
		public int ammoLevel;
		public Vector2 position;
		public Rectangle boundingBox;
		int shotTimer;
		public int bulletDamage;
		public bool isSelf;

		public int shootingDelay = 300;

		ITEM_TYPE itemToHold = ITEM_TYPE.NONE;

		public float deathTimer;
		int playerInvincibleTimer;
		int holdItemTimer;
		public int playerMotionAnimationTimer;
		float playerFootstepSoundTimer = 200f;

		public List<int> movementDirections = new();
		public List<int> shootingDirections = new();

		public Player(GameMultiplayerPrairieKing game, bool isSelf)
        {
			this.gameInstance = game;
			this.isSelf = isSelf;
        }

		public void ProcessInputs(Dictionary<GameKeys, int> _buttonHeldFrames)
        {
			if (_buttonHeldFrames[GameKeys.MoveUp] > 0)
			{
				AddMovementDirection(0);
			}
			else if (movementDirections.Contains(0))
			{
				movementDirections.Remove(0);
			}

			if (_buttonHeldFrames[GameKeys.MoveDown] > 0)
			{
				AddMovementDirection(2);
			}
			else if (movementDirections.Contains(2))
			{
				movementDirections.Remove(2);
			}

			if (_buttonHeldFrames[GameKeys.MoveLeft] > 0)
			{
				AddMovementDirection(3);
			}
			else if (movementDirections.Contains(3))
			{
				movementDirections.Remove(3);
			}

			if (_buttonHeldFrames[GameKeys.MoveRight] > 0)
			{
				AddMovementDirection(1);
			}
			else if (movementDirections.Contains(1))
			{
				movementDirections.Remove(1);
			}

			if (_buttonHeldFrames[GameKeys.ShootUp] > 0)
			{
				AddShootingDirection(0);
			}
			else if (shootingDirections.Contains(0))
			{
				shootingDirections.Remove(0);
			}

			if (_buttonHeldFrames[GameKeys.ShootDown] > 0)
			{
				AddShootingDirection(2);
			}
			else if (shootingDirections.Contains(2))
			{
				shootingDirections.Remove(2);
			}

			if (_buttonHeldFrames[GameKeys.ShootLeft] > 0)
			{
				AddShootingDirection(3);
			}
			else if (shootingDirections.Contains(3))
			{
				shootingDirections.Remove(3);
			}

			if (_buttonHeldFrames[GameKeys.ShootRight] > 0)
			{
				AddShootingDirection(1);
			}
			else if (shootingDirections.Contains(1))
			{
				shootingDirections.Remove(1);
			}
		}

		private void AddMovementDirection(int direction)
		{
			if (!gameInstance.gopherTrain && !movementDirections.Contains(direction))
			{
				if (movementDirections.Count == 1)
				{
					_ = (movementDirections.ElementAt(0) + 2) % 4;
				}
				movementDirections.Add(direction);
			}
		}

		private void AddShootingDirection(int direction)
		{
			if (!shootingDirections.Contains(direction))
			{
				shootingDirections.Add(direction);
			}
		}

		public void Tick(GameTime time)
        {
			//Timers
			if (holdItemTimer > 0)
			{
				holdItemTimer -= time.ElapsedGameTime.Milliseconds;
			}

			if (playerInvincibleTimer > 0)
			{
				playerInvincibleTimer -= time.ElapsedGameTime.Milliseconds;
			}

			//Shotting
			if (shotTimer > 0)
			{
				shotTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (deathTimer <= 0f && shootingDirections.Count > 0 && shotTimer <= 0)
			{
				if (gameInstance.activePowerups.ContainsKey(POWERUP_TYPE.SPREAD))
				{
					gameInstance.SpawnBullets(new int[1], position);
					gameInstance.SpawnBullets(new int[1] {1}, position);
					gameInstance.SpawnBullets(new int[1] {2}, position);
					gameInstance.SpawnBullets(new int[1] {3}, position);
					gameInstance.SpawnBullets(new int[2] {0,1}, position);
					gameInstance.SpawnBullets(new int[2] {1,2}, position);
					gameInstance.SpawnBullets(new int[2] {2,3}, position);
					gameInstance.SpawnBullets(new int[2] {3,0}, position);
				}
				else if (shootingDirections.Count == 1 || shootingDirections.Last() == (shootingDirections.ElementAt(shootingDirections.Count - 2) + 2) % 4)
				{
					gameInstance.SpawnBullets(new int[1]
					{
							(shootingDirections.Count == 2 && shootingDirections.Last() == (shootingDirections.ElementAt(shootingDirections.Count - 2) + 2) % 4) ? shootingDirections.ElementAt(1) : shootingDirections.ElementAt(0)
					}, position);
				}
				else
				{
					gameInstance.SpawnBullets(shootingDirections.ToArray(), position);
				}
				Game1.playSound("Cowboy_gunshot");
				shotTimer = shootingDelay;
				if (gameInstance.activePowerups.ContainsKey(POWERUP_TYPE.RAPIDFIRE))
				{
					shotTimer /= 4;
				}
				for (int i3 = 0; i3 < fireSpeedLevel; i3++)
				{
					shotTimer = shotTimer * 3 / 4;
				}
				if (gameInstance.activePowerups.ContainsKey(POWERUP_TYPE.SHOTGUN))
				{
					shotTimer = shotTimer * 3 / 2;
				}
				shotTimer = Math.Max(shotTimer, 20);
			}

			//Movement
			if (deathTimer <= 0f && movementDirections.Count > 0 && !gameInstance.scrollingMap)
			{
				int effectiveDirections = movementDirections.Count;
				if (effectiveDirections >= 2 && movementDirections.Last() == (movementDirections.ElementAt(movementDirections.Count - 2) + 2) % 4)
				{
					effectiveDirections = 1;
				}
				float speed = GetMovementSpeed(3f, effectiveDirections);

				//Run faster if COFFEE
				if (gameInstance.activePowerups.Keys.Contains(POWERUP_TYPE.SPEED))
				{
					speed *= 1.5f;
				}
				//Run faster if the zombie mode is actuve
				if (gameInstance.zombieModeTimer > 0)
				{
					speed *= 1.5f;
				}
				//Run faster for each shoe level
				for (int i = 0; i < runSpeedLevel; i++)
				{
					speed *= 1.25f;
				}

				for (int i = Math.Max(0, movementDirections.Count - 2); i < movementDirections.Count; i++)
				{
					if (i != 0 || movementDirections.Count < 2 || movementDirections.Last() != (movementDirections.ElementAt(movementDirections.Count - 2) + 2) % 4)
					{
						Vector2 newPlayerPosition = position;
						switch (movementDirections.ElementAt(i))
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
						if (!gameInstance.IsCollidingWithMap(newPlayerBox) && (!gameInstance.merchantBox.Intersects(newPlayerBox) || gameInstance.merchantBox.Intersects(boundingBox)))
						{
							position = newPlayerPosition;

							//NET Player Move
							gameInstance.NETmovePlayer(position);
						}
					}
				}

				//Reset the players bounding box
				boundingBox.X = (int)position.X + TileSize / 4;
				boundingBox.Y = (int)position.Y + TileSize / 4;
				boundingBox.Width = TileSize / 2;
				boundingBox.Height = TileSize / 2;

				//????
				playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
				playerMotionAnimationTimer %= 400;
				playerFootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;

				//Play footstep sound every once in a while
				if (playerFootstepSoundTimer <= 0f)
				{
					Game1.playSound("Cowboy_Footstep");
					playerFootstepSoundTimer = 200f;
				}

				//Pick up powerups
				for (int i = gameInstance.powerups.Count - 1; i >= 0; i--)
				{
					Powerup powerup = gameInstance.powerups[i];
					Rectangle powerupRect = new Rectangle(powerup.position.X, gameInstance.powerups[i].position.Y, TileSize, TileSize);

					if (boundingBox.Intersects(powerupRect) && !boundingBox.Intersects(gameInstance.noPickUpBox))
					{
						//NET Pickup Powerup
						PK_PowerupPickup message = new();
						message.id = powerup.id;
						message.which = (int)powerup.which;
						gameInstance.modInstance.Helper.Multiplayer.SendMessage(message, "PK_PowerupPickup");

						if (gameInstance.heldItem != null)
						{
							gameInstance.UsePowerup(powerup.which);
							gameInstance.powerups.RemoveAt(i);
						}
						else if (gameInstance.GetPowerUp(powerup))
						{
							gameInstance.powerups.RemoveAt(i);
						}
					}
				}

				//What
				if (!boundingBox.Intersects(gameInstance.noPickUpBox))
				{
					gameInstance.noPickUpBox.Location = new Point(0, 0);
				}

				//What again
				if (!gameInstance.shoppingCarpetNoPickup.Intersects(boundingBox))
				{
					gameInstance.shoppingCarpetNoPickup.X = -1000;
				}
			}

		}

		public void Draw(SpriteBatch b)
        {
			if (deathTimer <= 0f && (playerInvincibleTimer <= 0 || playerInvincibleTimer / 100 % 2 == 0))
			{
				if (holdItemTimer > 0)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + (int)itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.002f);
				}
				else if (gameInstance.zombieModeTimer > 0)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle(352 + ((gameInstance.zombieModeTimer / 50 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f);
				}
				else if (movementDirections.Count == 0 && shootingDirections.Count == 0)
				{
					if(isSelf) b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle(496, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f);
					else       b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle(256, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, gameInstance.player1.position.Y / 10000f + 0.001f);
				}
				else
				{
					if(isSelf)
                    {
						int facingDirection = (shootingDirections.Count == 0) ? movementDirections.ElementAt(0) : shootingDirections.Last();
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle(483, 1760 + playerMotionAnimationTimer / 100 * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(3f, -TileSize / 4), new Rectangle(464 + facingDirection * 16, 1744, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.002f + 0.001f);

					}
					else
                    {
						int facingDirection = (shootingDirections.Count == 0) ? movementDirections[0] : shootingDirections[0];
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle(243, 1728 + playerMotionAnimationTimer / 100 * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f + 0.001f);
						b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle(224 + facingDirection * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.002f + 0.001f);
					}
				}
			}
		}

		public void HoldItem(ITEM_TYPE item, int duration = 4000)
        {
			if (item != ITEM_TYPE.NONE)
			{
				itemToHold = item;
			}
			holdItemTimer = duration;
		}

		public bool IsHoldingItem()
        {
			return holdItemTimer > 0;
        }

		public ITEM_TYPE GetHeldItem()
        {
			return itemToHold;
        }

		public void SetInvincible(int duration)
        {
			playerInvincibleTimer = duration;
        }

		public bool IsInvincible()
        {
			return playerInvincibleTimer > 0;
        }

		public void Die()
        {
			deathTimer = 3000f;
			SetInvincible(5000);

			//Reset the player position (Different depending on boss or non boss level)
			if (gameInstance.shootoutLevel)
			{
				position = new Vector2(8 * TileSize, 3 * TileSize);
				Game1.playSound("Cowboy_monsterDie");
			}
			else
			{
				position = new Vector2(8 * TileSize - TileSize, 8 * TileSize);

				boundingBox.X = (int)position.X + TileSize / 4;
				boundingBox.Y = (int)position.Y + TileSize / 4;
				boundingBox.Width = TileSize / 2;
				boundingBox.Height = TileSize / 2;

				Game1.playSound("cowboy_dead");
			}

			if(isSelf) gameInstance.NETmovePlayer(position);
		}

		public void TickSlave(GameTime time)
		{
			playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
			playerMotionAnimationTimer %= 400;

			if (playerInvincibleTimer > 0)
			{
				playerInvincibleTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (deathTimer > 0)
			{
				deathTimer -= time.ElapsedGameTime.Milliseconds;
			}

			if (movementDirections.Count > 0)
			{
				float speed = GetMovementSpeed(3f, movementDirections.Count);
				for (int j = 0; j < movementDirections.Count; j++)
				{
					Vector2 newPlayerPosition = position;
					switch (movementDirections[j])
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
				}
				boundingBox.X = (int)position.X + TileSize / 4;
				boundingBox.Y = (int)position.Y + TileSize / 4;
				boundingBox.Width = TileSize / 2;
				boundingBox.Height = TileSize / 2;

				playerFootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
				if (playerFootstepSoundTimer <= 0)
				{
					Game1.playSound("Cowboy_Footstep");
					playerFootstepSoundTimer = 200;
				}
			}
		}

		//On die
		/*
		 if (playingWithAbigail && i < monsters.Count && monsters[i].position.Intersects(player2.boundingBox) && player2invincibletimer <= 0)
		{
			//TODO player 2 death
			Game1.playSound("Cowboy_monsterDie");
			player2deathtimer = 3000;
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, player2.position + topLeftScreenCoordinate + new Vector2(TileSize / 2, TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
			player2invincibletimer = 4000;
			player2.position = new Vector2(8f, 8f) * TileSize;
			player2.boundingBox.X = (int)player2.position.X + TileSize / 4;
			player2.boundingBox.Y = (int)player2.position.Y + TileSize / 4;
			player2.boundingBox.Width = TileSize / 2;
			player2.boundingBox.Height = TileSize / 2;
			/*
			if (player1.boundingBox.Intersects(player2.boundingBox))
			{
				player2.position.X = player1.boundingBox.Right + 2;
			}

			player2.boundingBox.X = (int) player2.position.X + TileSize / 4;
			player2.boundingBox.Y = (int) player2.position.Y + TileSize / 4;
			player2.boundingBox.Width = TileSize / 2;
			player2.boundingBox.Height = TileSize / 2;
		}
		*/

	}
}
