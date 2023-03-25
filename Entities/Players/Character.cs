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

		public virtual void Draw(SpriteBatch b)
        {

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
