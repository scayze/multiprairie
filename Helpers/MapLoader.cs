using Microsoft.Xna.Framework;
using StardewValley;

namespace MultiplayerPrairieKing.Helpers
{
    public class MapLoader
    {
		public static int[,] GetMap(int wave)
		{
			int[,] newMap = new int[16, 16];
			for (int i8 = 0; i8 < 16; i8++)
			{
				for (int i = 0; i < 16; i++)
				{
					if ((i8 == 0 || i8 == 15 || i == 0 || i == 15) && (i8 <= 6 || i8 >= 10) && (i <= 6 || i >= 10))
					{
						newMap[i8, i] = 5;
					}
					else if (i8 == 0 || i8 == 15 || i == 0 || i == 15)
					{
						newMap[i8, i] = ((Game1.random.NextDouble() < 0.15) ? 1 : 0);
					}
					else if (i8 == 1 || i8 == 14 || i == 1 || i == 14)
					{
						newMap[i8, i] = 2;
					}
					else
					{
						newMap[i8, i] = ((Game1.random.NextDouble() < 0.1) ? 4 : 3);
					}
				}
			}
			switch (wave)
			{
				case -1:
					{
						for (int l = 0; l < 16; l++)
						{
							for (int j = 0; j < 16; j++)
							{
								if (newMap[l, j] == 0 || newMap[l, j] == 1 || newMap[l, j] == 2 || newMap[l, j] == 5)
								{
									newMap[l, j] = 3;
								}
							}
						}
						newMap[3, 1] = 5;
						newMap[8, 2] = 5;
						newMap[13, 1] = 5;
						newMap[5, 0] = 0;
						newMap[10, 2] = 2;
						newMap[15, 2] = 1;
						newMap[14, 12] = 5;
						newMap[10, 6] = 7;
						newMap[11, 6] = 7;
						newMap[12, 6] = 7;
						newMap[13, 6] = 7;
						newMap[14, 6] = 7;
						newMap[14, 7] = 7;
						newMap[14, 8] = 7;
						newMap[14, 9] = 7;
						newMap[14, 10] = 7;
						newMap[14, 11] = 7;
						newMap[14, 12] = 7;
						newMap[14, 13] = 7;
						for (int k = 0; k < 16; k++)
						{
							newMap[k, 3] = ((k % 2 == 0) ? 9 : 8);
						}
						newMap[3, 3] = 10;
						newMap[7, 8] = 2;
						newMap[8, 8] = 2;
						newMap[4, 11] = 2;
						newMap[11, 12] = 2;
						newMap[9, 11] = 2;
						newMap[3, 9] = 2;
						newMap[2, 12] = 5;
						newMap[8, 13] = 5;
						newMap[12, 11] = 5;
						newMap[7, 14] = 0;
						newMap[6, 14] = 2;
						newMap[8, 14] = 2;
						newMap[7, 13] = 2;
						newMap[7, 15] = 2;
						break;
					}
				case 1:
					newMap[4, 4] = 7;
					newMap[4, 5] = 7;
					newMap[5, 4] = 7;
					newMap[12, 4] = 7;
					newMap[11, 4] = 7;
					newMap[12, 5] = 7;
					newMap[4, 12] = 7;
					newMap[5, 12] = 7;
					newMap[4, 11] = 7;
					newMap[12, 12] = 7;
					newMap[11, 12] = 7;
					newMap[12, 11] = 7;
					break;
				case 2:
					newMap[8, 4] = 7;
					newMap[12, 8] = 7;
					newMap[8, 12] = 7;
					newMap[4, 8] = 7;
					newMap[1, 1] = 5;
					newMap[14, 1] = 5;
					newMap[14, 14] = 5;
					newMap[1, 14] = 5;
					newMap[2, 1] = 5;
					newMap[13, 1] = 5;
					newMap[13, 14] = 5;
					newMap[2, 14] = 5;
					newMap[1, 2] = 5;
					newMap[14, 2] = 5;
					newMap[14, 13] = 5;
					newMap[1, 13] = 5;
					break;
				case 3:
					newMap[5, 5] = 7;
					newMap[6, 5] = 7;
					newMap[7, 5] = 7;
					newMap[9, 5] = 7;
					newMap[10, 5] = 7;
					newMap[11, 5] = 7;
					newMap[5, 11] = 7;
					newMap[6, 11] = 7;
					newMap[7, 11] = 7;
					newMap[9, 11] = 7;
					newMap[10, 11] = 7;
					newMap[11, 11] = 7;
					newMap[5, 6] = 7;
					newMap[5, 7] = 7;
					newMap[5, 9] = 7;
					newMap[5, 10] = 7;
					newMap[11, 6] = 7;
					newMap[11, 7] = 7;
					newMap[11, 9] = 7;
					newMap[11, 10] = 7;
					break;
				case 4:
				case 8:
					{
						for (int i2 = 0; i2 < 16; i2++)
						{
							for (int m = 0; m < 16; m++)
							{
								if (newMap[i2, m] == 5)
								{
									newMap[i2, m] = ((!(Game1.random.NextDouble() < 0.5)) ? 1 : 0);
								}
							}
						}
						for (int n = 0; n < 16; n++)
						{
							newMap[n, 8] = ((Game1.random.NextDouble() < 0.5) ? 8 : 9);
						}
						newMap[8, 4] = 7;
						newMap[8, 12] = 7;
						newMap[9, 12] = 7;
						newMap[7, 12] = 7;
						newMap[5, 6] = 5;
						newMap[10, 6] = 5;
						break;
					}
				case 5:
					newMap[1, 1] = 5;
					newMap[14, 1] = 5;
					newMap[14, 14] = 5;
					newMap[1, 14] = 5;
					newMap[2, 1] = 5;
					newMap[13, 1] = 5;
					newMap[13, 14] = 5;
					newMap[2, 14] = 5;
					newMap[1, 2] = 5;
					newMap[14, 2] = 5;
					newMap[14, 13] = 5;
					newMap[1, 13] = 5;
					newMap[3, 1] = 5;
					newMap[13, 1] = 5;
					newMap[13, 13] = 5;
					newMap[1, 13] = 5;
					newMap[1, 3] = 5;
					newMap[13, 3] = 5;
					newMap[12, 13] = 5;
					newMap[3, 14] = 5;
					newMap[3, 3] = 5;
					newMap[13, 12] = 5;
					newMap[13, 12] = 5;
					newMap[3, 12] = 5;
					break;
				case 6:
					newMap[4, 5] = 2;
					newMap[12, 10] = 5;
					newMap[10, 9] = 5;
					newMap[5, 12] = 2;
					newMap[5, 9] = 5;
					newMap[12, 12] = 5;
					newMap[3, 4] = 5;
					newMap[2, 3] = 5;
					newMap[11, 3] = 5;
					newMap[10, 6] = 5;
					newMap[5, 9] = 7;
					newMap[10, 12] = 7;
					newMap[3, 12] = 7;
					newMap[10, 8] = 7;
					break;
				case 7:
					{
						for (int i3 = 0; i3 < 16; i3++)
						{
							newMap[i3, 5] = ((i3 % 2 == 0) ? 9 : 8);
							newMap[i3, 10] = ((i3 % 2 == 0) ? 9 : 8);
						}
						newMap[4, 5] = 10;
						newMap[8, 5] = 10;
						newMap[12, 5] = 10;
						newMap[4, 10] = 10;
						newMap[8, 10] = 10;
						newMap[12, 10] = 10;
						break;
					}
				case 9:
					newMap[4, 4] = 5;
					newMap[5, 4] = 5;
					newMap[10, 4] = 5;
					newMap[12, 4] = 5;
					newMap[4, 5] = 5;
					newMap[5, 5] = 5;
					newMap[10, 5] = 5;
					newMap[12, 5] = 5;
					newMap[4, 10] = 5;
					newMap[5, 10] = 5;
					newMap[10, 10] = 5;
					newMap[12, 10] = 5;
					newMap[4, 12] = 5;
					newMap[5, 12] = 5;
					newMap[10, 12] = 5;
					newMap[12, 12] = 5;
					break;
				case 10:
					{
						for (int i4 = 0; i4 < 16; i4++)
						{
							newMap[i4, 1] = ((i4 % 2 == 0) ? 9 : 8);
							newMap[i4, 14] = ((i4 % 2 == 0) ? 9 : 8);
						}
						newMap[8, 1] = 10;
						newMap[7, 1] = 10;
						newMap[9, 1] = 10;
						newMap[8, 14] = 10;
						newMap[7, 14] = 10;
						newMap[9, 14] = 10;
						newMap[6, 8] = 5;
						newMap[10, 8] = 5;
						newMap[8, 6] = 5;
						newMap[8, 9] = 5;
						break;
					}
				case 11:
					{
						for (int i5 = 0; i5 < 16; i5++)
						{
							newMap[i5, 0] = 7;
							newMap[i5, 15] = 7;
							if (i5 % 2 == 0)
							{
								newMap[i5, 1] = 5;
								newMap[i5, 14] = 5;
							}
						}
						break;
					}
				case 12:
					{
						for (int i7 = 0; i7 < 16; i7++)
						{
							for (int j2 = 0; j2 < 16; j2++)
							{
								if (newMap[i7, j2] == 0 || newMap[i7, j2] == 1)
								{
									newMap[i7, j2] = 5;
								}
							}
						}
						for (int i6 = 0; i6 < 16; i6++)
						{
							newMap[i6, 0] = ((i6 % 2 == 0) ? 9 : 8);
							newMap[i6, 15] = ((i6 % 2 == 0) ? 9 : 8);
						}
						Rectangle r = new(1, 1, 14, 14);
						foreach (Vector2 v2 in Utility.getBorderOfThisRectangle(r))
						{
							newMap[(int)v2.X, (int)v2.Y] = 10;
						}
						r.Inflate(-1, -1);
						{
							foreach (Vector2 v in Utility.getBorderOfThisRectangle(r))
							{
								newMap[(int)v.X, (int)v.Y] = 2;
							}
							return newMap;
						}
					}
				default:
					newMap[4, 4] = 5;
					newMap[12, 4] = 5;
					newMap[4, 12] = 5;
					newMap[12, 12] = 5;
					break;
			}
			return newMap;
		}
	}
}
