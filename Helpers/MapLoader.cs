using Microsoft.Xna.Framework;
using StardewValley;

namespace MultiplayerPrairieKing.Helpers
{
    public class MapLoader
    {

		public static MAP_TILE[,] GetMap(int wave)
		{
			MAP_TILE[,] newMap = new MAP_TILE[16, 16];
			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					if ((x == 0 || x == 15 || y == 0 || y == 15) && (x <= 6 || x >= 10) && (y <= 6 || y >= 10))
					{
						newMap[x, y] = MAP_TILE.CACTUS;
					}
					else if (x == 0 || x == 15 || y == 0 || y == 15)
					{
						newMap[x, y] = (MAP_TILE)((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (x == 1 || x == 14 || y == 1 || y == 14)
					{
						newMap[x, y] = MAP_TILE.GRAVEL;
					}
					else
					{
						newMap[x, y] = (MAP_TILE)((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}
			switch (wave)
			{
				case -1:
					{
						for (int x = 0; x < 16; x++)
						{
							for (int y = 0; y < 16; y++)
							{
								if (newMap[x, y] == 0 || newMap[x, y] ==  MAP_TILE.BARRIER2 || newMap[x, y] ==  MAP_TILE.GRAVEL || newMap[x, y] ==  MAP_TILE.CACTUS)
								{
									newMap[x, y] = MAP_TILE.SAND;
								}
							}
						}
						newMap[3, 1] = MAP_TILE.CACTUS;
						newMap[8, 2] = MAP_TILE.CACTUS;
						newMap[13, 1] = MAP_TILE.CACTUS;
						newMap[5, 0] = MAP_TILE.BARRIER1;
						newMap[10, 2] = MAP_TILE.GRAVEL;
						newMap[15, 2] = MAP_TILE.BARRIER2;
						newMap[14, 12] = MAP_TILE.CACTUS;
						newMap[10, 6] = MAP_TILE.FENCE;
						newMap[11, 6] = MAP_TILE.FENCE;
						newMap[12, 6] = MAP_TILE.FENCE;
						newMap[13, 6] = MAP_TILE.FENCE;
						newMap[14, 6] = MAP_TILE.FENCE;
						newMap[14, 7] = MAP_TILE.FENCE;
						newMap[14, 8] = MAP_TILE.FENCE;
						newMap[14, 9] = MAP_TILE.FENCE;
						newMap[14, 10] = MAP_TILE.FENCE;
						newMap[14, 11] = MAP_TILE.FENCE;
						newMap[14, 12] = MAP_TILE.FENCE;
						newMap[14, 13] = MAP_TILE.FENCE;
						for (int x = 0; x < 16; x++)
						{
							newMap[x, 3] = (MAP_TILE)((x % 2 == 0) ? 9 : 8);
						}
						newMap[3, 3] = MAP_TILE.BRIDGE;
						newMap[7, 8] = MAP_TILE.GRAVEL;
						newMap[8, 8] = MAP_TILE.GRAVEL;
						newMap[4, 11] = MAP_TILE.GRAVEL;
						newMap[11, 12] = MAP_TILE.GRAVEL;
						newMap[9, 11] = MAP_TILE.GRAVEL;
						newMap[3, 9] = MAP_TILE.GRAVEL;
						newMap[2, 12] = MAP_TILE.CACTUS;
						newMap[8, 13] = MAP_TILE.CACTUS;
						newMap[12, 11] = MAP_TILE.CACTUS;
						newMap[7, 14] = 0;
						newMap[6, 14] = MAP_TILE.GRAVEL;
						newMap[8, 14] = MAP_TILE.GRAVEL;
						newMap[7, 13] = MAP_TILE.GRAVEL;
						newMap[7, 15] = MAP_TILE.GRAVEL;
						break;
					}
				case 1:
					newMap[4, 4] = MAP_TILE.FENCE;
					newMap[4, 5] = MAP_TILE.FENCE;
					newMap[5, 4] = MAP_TILE.FENCE;
					newMap[12, 4] = MAP_TILE.FENCE;
					newMap[11, 4] = MAP_TILE.FENCE;
					newMap[12, 5] = MAP_TILE.FENCE;
					newMap[4, 12] = MAP_TILE.FENCE;
					newMap[5, 12] = MAP_TILE.FENCE;
					newMap[4, 11] = MAP_TILE.FENCE;
					newMap[12, 12] = MAP_TILE.FENCE;
					newMap[11, 12] = MAP_TILE.FENCE;
					newMap[12, 11] = MAP_TILE.FENCE;
					break;
				case 2:
					newMap[8, 4] = MAP_TILE.FENCE;
					newMap[12, 8] = MAP_TILE.FENCE;
					newMap[8, 12] = MAP_TILE.FENCE;
					newMap[4, 8] = MAP_TILE.FENCE;
					newMap[1, 1] = MAP_TILE.CACTUS;
					newMap[14, 1] = MAP_TILE.CACTUS;
					newMap[14, 14] = MAP_TILE.CACTUS;
					newMap[1, 14] = MAP_TILE.CACTUS;
					newMap[2, 1] = MAP_TILE.CACTUS;
					newMap[13, 1] = MAP_TILE.CACTUS;
					newMap[13, 14] = MAP_TILE.CACTUS;
					newMap[2, 14] = MAP_TILE.CACTUS;
					newMap[1, 2] = MAP_TILE.CACTUS;
					newMap[14, 2] = MAP_TILE.CACTUS;
					newMap[14, 13] = MAP_TILE.CACTUS;
					newMap[1, 13] = MAP_TILE.CACTUS;
					break;
				case 3:
					newMap[5, 5] = MAP_TILE.FENCE;
					newMap[6, 5] = MAP_TILE.FENCE;
					newMap[7, 5] = MAP_TILE.FENCE;
					newMap[9, 5] = MAP_TILE.FENCE;
					newMap[10, 5] = MAP_TILE.FENCE;
					newMap[11, 5] = MAP_TILE.FENCE;
					newMap[5, 11] = MAP_TILE.FENCE;
					newMap[6, 11] = MAP_TILE.FENCE;
					newMap[7, 11] = MAP_TILE.FENCE;
					newMap[9, 11] = MAP_TILE.FENCE;
					newMap[10, 11] = MAP_TILE.FENCE;
					newMap[11, 11] = MAP_TILE.FENCE;
					newMap[5, 6] = MAP_TILE.FENCE;
					newMap[5, 7] = MAP_TILE.FENCE;
					newMap[5, 9] = MAP_TILE.FENCE;
					newMap[5, 10] = MAP_TILE.FENCE;
					newMap[11, 6] = MAP_TILE.FENCE;
					newMap[11, 7] = MAP_TILE.FENCE;
					newMap[11, 9] = MAP_TILE.FENCE;
					newMap[11, 10] = MAP_TILE.FENCE;
					break;
				case 4:
				case 8:
					{
						for (int x = 0; x < 16; x++)
						{
							for (int y = 0; y < 16; y++)
							{
								if (newMap[x, y] ==  MAP_TILE.CACTUS)
								{
									newMap[x, y] = (MAP_TILE)((!(Game1.random.NextDouble() < 0.5)) ? 1 : 0);
								}
							}
						}
						for (int x = 0; x < 16; x++)
						{
							newMap[x, 8] = (MAP_TILE)((Game1.random.NextDouble() < 0.5) ? 8 : 9);
						}
						newMap[8, 4] = MAP_TILE.FENCE;
						newMap[8, 12] = MAP_TILE.FENCE;
						newMap[9, 12] = MAP_TILE.FENCE;
						newMap[7, 12] = MAP_TILE.FENCE;
						newMap[5, 6] = MAP_TILE.CACTUS;
						newMap[10, 6] = MAP_TILE.CACTUS;
						break;
					}
				case 5:
					newMap[1, 1] = MAP_TILE.CACTUS;
					newMap[14, 1] = MAP_TILE.CACTUS;
					newMap[14, 14] = MAP_TILE.CACTUS;
					newMap[1, 14] = MAP_TILE.CACTUS;
					newMap[2, 1] = MAP_TILE.CACTUS;
					newMap[13, 1] = MAP_TILE.CACTUS;
					newMap[13, 14] = MAP_TILE.CACTUS;
					newMap[2, 14] = MAP_TILE.CACTUS;
					newMap[1, 2] = MAP_TILE.CACTUS;
					newMap[14, 2] = MAP_TILE.CACTUS;
					newMap[14, 13] = MAP_TILE.CACTUS;
					newMap[1, 13] = MAP_TILE.CACTUS;
					newMap[3, 1] = MAP_TILE.CACTUS;
					newMap[13, 1] = MAP_TILE.CACTUS;
					newMap[13, 13] = MAP_TILE.CACTUS;
					newMap[1, 13] = MAP_TILE.CACTUS;
					newMap[1, 3] = MAP_TILE.CACTUS;
					newMap[13, 3] = MAP_TILE.CACTUS;
					newMap[12, 13] = MAP_TILE.CACTUS;
					newMap[3, 14] = MAP_TILE.CACTUS;
					newMap[3, 3] = MAP_TILE.CACTUS;
					newMap[13, 12] = MAP_TILE.CACTUS;
					newMap[13, 12] = MAP_TILE.CACTUS;
					newMap[3, 12] = MAP_TILE.CACTUS;
					break;
				case 6:
					newMap[4, 5] = MAP_TILE.GRAVEL;
					newMap[12, 10] = MAP_TILE.CACTUS;
					newMap[10, 9] = MAP_TILE.CACTUS;
					newMap[5, 12] = MAP_TILE.GRAVEL;
					newMap[5, 9] = MAP_TILE.CACTUS;
					newMap[12, 12] = MAP_TILE.CACTUS;
					newMap[3, 4] = MAP_TILE.CACTUS;
					newMap[2, 3] = MAP_TILE.CACTUS;
					newMap[11, 3] = MAP_TILE.CACTUS;
					newMap[10, 6] = MAP_TILE.CACTUS;
					newMap[5, 9] = MAP_TILE.FENCE;
					newMap[10, 12] = MAP_TILE.FENCE;
					newMap[3, 12] = MAP_TILE.FENCE;
					newMap[10, 8] = MAP_TILE.FENCE;
					break;
				case 7:
					{
						for (int x = 0; x < 16; x++)
						{
							newMap[x, 5] = (MAP_TILE)((x % 2 == 0) ? 9 : 8);
							newMap[x, 10] = (MAP_TILE)((x % 2 == 0) ? 9 : 8);
						}
						newMap[4, 5] = MAP_TILE.BRIDGE;
						newMap[8, 5] = MAP_TILE.BRIDGE;
						newMap[12, 5] = MAP_TILE.BRIDGE;
						newMap[4, 10] = MAP_TILE.BRIDGE;
						newMap[8, 10] = MAP_TILE.BRIDGE;
						newMap[12, 10] = MAP_TILE.BRIDGE;
						break;
					}
				case 9:
					newMap[4, 4] = MAP_TILE.CACTUS;
					newMap[5, 4] = MAP_TILE.CACTUS;
					newMap[10, 4] = MAP_TILE.CACTUS;
					newMap[12, 4] = MAP_TILE.CACTUS;
					newMap[4, 5] = MAP_TILE.CACTUS;
					newMap[5, 5] = MAP_TILE.CACTUS;
					newMap[10, 5] = MAP_TILE.CACTUS;
					newMap[12, 5] = MAP_TILE.CACTUS;
					newMap[4, 10] = MAP_TILE.CACTUS;
					newMap[5, 10] = MAP_TILE.CACTUS;
					newMap[10, 10] = MAP_TILE.CACTUS;
					newMap[12, 10] = MAP_TILE.CACTUS;
					newMap[4, 12] = MAP_TILE.CACTUS;
					newMap[5, 12] = MAP_TILE.CACTUS;
					newMap[10, 12] = MAP_TILE.CACTUS;
					newMap[12, 12] = MAP_TILE.CACTUS;
					break;
				case 10:
					{
						for (int x = 0; x < 16; x++)
						{
							newMap[x, 1] = (MAP_TILE)((x % 2 == 0) ? 9 : 8);
							newMap[x, 14] = (MAP_TILE)((x % 2 == 0) ? 9 : 8);
						}
						newMap[8, 1] = MAP_TILE.BRIDGE;
						newMap[7, 1] = MAP_TILE.BRIDGE;
						newMap[9, 1] = MAP_TILE.BRIDGE;
						newMap[8, 14] = MAP_TILE.BRIDGE;
						newMap[7, 14] = MAP_TILE.BRIDGE;
						newMap[9, 14] = MAP_TILE.BRIDGE;
						newMap[6, 8] = MAP_TILE.CACTUS;
						newMap[10, 8] = MAP_TILE.CACTUS;
						newMap[8, 6] = MAP_TILE.CACTUS;
						newMap[8, 9] = MAP_TILE.CACTUS;
						break;
					}
				case 11:
					{
						for (int x = 0; x < 16; x++)
						{
							newMap[x, 0] = MAP_TILE.FENCE;
							newMap[x, 15] = MAP_TILE.FENCE;
							if (x % 2 == 0)
							{
								newMap[x, 1] = MAP_TILE.CACTUS;
								newMap[x, 14] = MAP_TILE.CACTUS;
							}
						}
						break;
					}
				case 12:
					{
						for (int x = 0; x < 16; x++)
						{
							for (int y = 0; y < 16; y++)
							{
								if (newMap[x, y] == 0 || newMap[x, y] ==  MAP_TILE.BARRIER2)
								{
									newMap[x, y] = MAP_TILE.CACTUS;
								}
							}
						}
						for (int x = 0; x < 16; x++)
						{
							newMap[x, 0] = (MAP_TILE)((x % 2 == 0) ? 9 : 8);
							newMap[x, 15] = (MAP_TILE)((x % 2 == 0) ? 9 : 8);
						}
						Rectangle r = new(1, 1, 14, 14);
						foreach (Vector2 v2 in Utility.getBorderOfThisRectangle(r))
						{
							newMap[(int)v2.X, (int)v2.Y] = MAP_TILE.BRIDGE;
						}
						r.Inflate(-1, -1);
						{
							foreach (Vector2 v in Utility.getBorderOfThisRectangle(r))
							{
								newMap[(int)v.X, (int)v.Y] = MAP_TILE.GRAVEL;
							}
							return newMap;
						}
					}
				default:
					newMap[4, 4] = MAP_TILE.CACTUS;
					newMap[12, 4] = MAP_TILE.CACTUS;
					newMap[4, 12] = MAP_TILE.CACTUS;
					newMap[12, 12] = MAP_TILE.CACTUS;
					break;
			}
			return newMap;
		}
	}
}
