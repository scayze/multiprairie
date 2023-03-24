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

		public int playerMotionAnimationTimer;
		public float deathTimer;

		protected float playerFootstepSoundTimer = 200f;
		protected int playerInvincibleTimer;

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
			playerInvincibleTimer = duration;
		}

		public bool IsInvincible()
		{
			return playerInvincibleTimer > 0;
		}

		public virtual void Tick(GameTime time)
        {

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
