using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiPlayerPrairie;
using StardewValley;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;

namespace MultiplayerPrairieKing.Entities
{
	public class Powerup
	{
		public GameMultiplayerPrairieKing gameInstance;

		public long id;

		public POWERUP_TYPE which;

		public Point position;

		public int duration;

		public float yOffset;

		public Powerup(GameMultiplayerPrairieKing game, POWERUP_TYPE which, Point position, int duration)
		{
			this.gameInstance = game;

			this.which = which;
			this.position = position;
			this.duration = duration;

			//NET PowerupSpawn
			if (!gameInstance.isHost) return;
			//in cases where the player is *given* an item, and he doesnt pick it up, position will be Zero.
			//we do *not* want this synced
			if (position == Point.Zero) return;

			this.id = gameInstance.modInstance.Helper.Multiplayer.GetNewID();

			PK_PowerupSpawn mPowerup = new();
			mPowerup.id = this.id;
			mPowerup.which = (int)this.which;
			mPowerup.position = this.position;
			mPowerup.duration = this.duration;

			gameInstance.modInstance.Helper.Multiplayer.SendMessage(mPowerup, "PK_PowerupSpawn");
		}

		public void Draw(SpriteBatch b)
		{
			if (duration > 2000 || duration / 200 % 2 == 0)
			{
				b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, (float)position.Y + yOffset), new Rectangle(272 + (int)which * 16, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)position.Y / 10000f + 0.001f);
			}
		}
	}
}
