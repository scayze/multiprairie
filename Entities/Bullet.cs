using Microsoft.Xna.Framework;
using MultiPlayerPrairie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerPrairieKing.Entities
{
	public class Bullet
	{
		public long id;

		public GameMultiplayerPrairieKing gameInstance;

		public Point position;

		public Point motion;

		public int damage;


		public Bullet(GameMultiplayerPrairieKing game, Point position, Point motion, int damage)
		{
			this.gameInstance = game;
			this.id = gameInstance.modInstance.Helper.Multiplayer.GetNewID();

			this.position = position;
			this.motion = motion;
			this.damage = damage;
		}

		public Bullet(GameMultiplayerPrairieKing game, Point position, int direction, int damage)
		{
			this.gameInstance = game;
			this.id = gameInstance.modInstance.Helper.Multiplayer.GetNewID();

			this.position = position;
			switch (direction)
			{
				case 0:
					motion = new Point(0, -8);
					break;
				case 1:
					motion = new Point(8, 0);
					break;
				case 2:
					motion = new Point(0, 8);
					break;
				case 3:
					motion = new Point(-8, 0);
					break;
			}
			this.damage = damage;
		}
	}
}
