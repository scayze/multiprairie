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

		public Powerup(GameMultiplayerPrairieKing gameInstance, POWERUP_TYPE which, Point position, int duration)
		{
			this.gameInstance = gameInstance;

			this.which = which;
			this.position = position;
			this.duration = duration;

			this.id = gameInstance.modInstance.Helper.Multiplayer.GetNewID();
		}

		public void Draw(SpriteBatch b)
		{
			if (duration > 2000 || duration / 200 % 2 == 0)
			{
				b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y + yOffset), new Rectangle(272 + (int)which * 16, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, position.Y / 10000f + 0.001f);
			}
		}
	}
}
