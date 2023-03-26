using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiPlayerPrairie;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;

namespace MultiplayerPrairieKing.Entities
{
    public class Character
    {
		protected GameMultiplayerPrairieKing gameInstance;

		public Vector2 position;
		public Rectangle boundingBox;

		public List<int> movementDirections = new();
		public List<int> shootingDirections = new();

		public int motionAnimationTimer;
		public float deathTimer;

		protected float footstepSoundTimer = 200f;
		protected int invincibleTimer;

		protected int holdItemTimer;
		protected ITEM_TYPE itemToHold = ITEM_TYPE.NONE;

		public static Texture2D texture;
		protected Vector2 textureBase;

		public Character(GameMultiplayerPrairieKing game)
		{
			this.gameInstance = game;
		}

		protected void AddMovementDirection(int direction)
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

		protected void AddShootingDirection(int direction)
		{
			if (!shootingDirections.Contains(direction))
			{
				shootingDirections.Add(direction);
			}
		}

		public bool IsHoldingItem()
		{
			return holdItemTimer > 0;
		}

		public ITEM_TYPE GetHeldItem()
		{
			return itemToHold;
		}

		public void HoldItem(ITEM_TYPE item, int duration = 4000)
		{
			if (item != ITEM_TYPE.NONE)
			{
				itemToHold = item;
			}
			holdItemTimer = duration;
		}

		public void SetInvincible(int duration)
		{
			invincibleTimer = duration;
		}

		public bool IsInvincible()
		{
			return invincibleTimer > 0;
		}

		public virtual void Tick(GameTime time)
        {
			if (invincibleTimer > 0)
			{
				invincibleTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (deathTimer > 0)
			{
				deathTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (holdItemTimer > 0)
			{
				holdItemTimer -= time.ElapsedGameTime.Milliseconds;
			}

			if (movementDirections.Count > 0)
			{
				footstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
				if (footstepSoundTimer <= 0)
				{
					Game1.playSound("Cowboy_Footstep");
					footstepSoundTimer = 200;
				}
			}
		}

		public void Draw(SpriteBatch b)
		{
			if (deathTimer <= 0f && (invincibleTimer <= 0 || invincibleTimer / 100 % 2 == 0))
			{
				if (holdItemTimer > 0)
				{
					b.Draw(texture, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle((int)textureBase.X + 48, (int)textureBase.Y + 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f);
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize * 2 / 3) + new Vector2(0f, -TileSize / 4), new Rectangle(320 + (int)itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.002f);
				}
				else if (gameInstance.zombieModeTimer > 0)
				{
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle(352 + ((gameInstance.zombieModeTimer / 50 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f);
				}
				else if (movementDirections.Count == 0 && shootingDirections.Count == 0)
				{
					b.Draw(texture, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle((int)textureBase.X + 32, (int)textureBase.Y + 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f);
				}
				else
				{
					int facingDirection = (shootingDirections.Count == 0) ? movementDirections.ElementAt(0) : shootingDirections.Last();
					b.Draw(texture, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle((int)textureBase.X + 19, (int)textureBase.Y + 16 + motionAnimationTimer / 100 * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f + 0.001f);
					b.Draw(texture, topLeftScreenCoordinate + position + new Vector2(3f, -TileSize / 4), new Rectangle((int)textureBase.X + facingDirection * 16, (int)textureBase.Y, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.002f + 0.001f);
				}
			}
		}

		public virtual void Die()
        {
			deathTimer = 3000f;

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
		}
    }
}
