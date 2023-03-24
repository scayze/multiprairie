using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiPlayerPrairie;
using StardewValley;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;

namespace MultiplayerPrairieKing.Entities
{
    public class PlayerSlave : Character
    {
        public PlayerSlave(GameMultiplayerPrairieKing game) : base(game)
        {
        }

        public override void Tick(GameTime time)
		{
			base.Tick(time);

			playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
			playerMotionAnimationTimer %= 400;

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
				}
				boundingBox.X = (int)position.X + TileSize / 4;
				boundingBox.Y = (int)position.Y + TileSize / 4;
				boundingBox.Width = TileSize / 2;
				boundingBox.Height = TileSize / 2;
			}
		}

		public override void Draw(SpriteBatch b)
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
					b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + position + new Vector2(0f, -TileSize / 4), new Rectangle(256, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, gameInstance.player1.position.Y / 10000f + 0.001f);
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
}
