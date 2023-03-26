using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiPlayerPrairie;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;

namespace MultiplayerPrairieKing.Entities
{
	public class Player : Character
	{
		public int runSpeedLevel;
		public int fireSpeedLevel;
		public int ammoLevel;
		public int bulletDamage = 1;

		int shotTimer;
		readonly int shootingDelay = 300;

		

		public Player(GameMultiplayerPrairieKing game) : base(game)
        {
			textureBase = Vector2.Zero;
			ammoLevel = 0;
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

		public override void Tick(GameTime time)
        {
			base.Tick(time);
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
						}
					}
				}

				//Reset the players bounding box
				boundingBox.X = (int)position.X + TileSize / 4;
				boundingBox.Y = (int)position.Y + TileSize / 4;
				boundingBox.Width = TileSize / 2;
				boundingBox.Height = TileSize / 2;

				//????
				motionAnimationTimer += time.ElapsedGameTime.Milliseconds;
				motionAnimationTimer %= 400;

				//Pick up powerups
				for (int i = gameInstance.powerups.Count - 1; i >= 0; i--)
				{
					Powerup powerup = gameInstance.powerups[i];
					Rectangle powerupRect = new(powerup.position.X, gameInstance.powerups[i].position.Y, TileSize, TileSize);

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
						else if (gameInstance.PickupPowerup(powerup))
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

			//NET Player Move
			gameInstance.NETmovePlayer(position);

		}

		public override void Die()
        {
			base.Die();
			SetInvincible(5000);
			gameInstance.NETmovePlayer(position);
		}
	}
}
