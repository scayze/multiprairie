using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiPlayerPrairie;
using StardewValley;
using static MultiPlayerPrairie.GameMultiplayerPrairieKing;

namespace MultiplayerPrairieKing.Entities
{
	public class Bullet
	{
		public long id;

		public GameMultiplayerPrairieKing gameInstance;

		public Point position;

		public Point motion;

		public int damage;

		public bool queuedForDeletion = false;

		public bool isFriendly;

		public bool isPersonal;


		public Bullet(GameMultiplayerPrairieKing gameInstance, bool isFriendly, bool isPersonal, Point position, Point motion, int damage)
		{
			this.gameInstance = gameInstance;
			this.id = gameInstance.modInstance.Helper.Multiplayer.GetNewID();

			this.position = position;
			this.motion = motion;
			this.damage = damage;
			this.isFriendly = isFriendly;
			this.isPersonal = isPersonal;
		}

		public Bullet(GameMultiplayerPrairieKing game, bool isFriendly, bool isPersonal, Point position, int direction, int damage)
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
			this.isFriendly = isFriendly;
			this.isPersonal = isPersonal;
		}

		public void Draw(SpriteBatch b)
        {
			if(isFriendly)
            {
				b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(518, 1760 + (damage - 1) * 4, 4, 4), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
			}
			else
            {
				b.Draw(Game1.mouseCursors, topLeftScreenCoordinate + new Vector2(position.X, position.Y), new Rectangle(523, 1760, 5, 5), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
			}
		}

		public void EnemyUpdate()
        {
			if (!gameInstance.player1.IsInvincible() && gameInstance.player1.deathTimer <= 0f && gameInstance.player1.boundingBox.Intersects(new Rectangle(position.X, position.Y, 15, 15)))
			{
				gameInstance.PlayerDie();

				//NET Player death
				PK_PlayerDeath message = new();
				message.id = Game1.player.UniqueMultiplayerID;
				gameInstance.modInstance.Helper.Multiplayer.SendMessage(message, "PK_PlayerDeath");
			}
        }

		public void FriendlyUpdate()
        {
			if (!isPersonal) return;

			for (int k = gameInstance.monsters.Count - 1; k >= 0; k--)
			{
				Enemy monster = gameInstance.monsters[k];
				if (monster.position.Intersects(new Rectangle(position.X, position.Y, 12, 12)))
				{
					int monsterhealth = monster.health;
					int monsterAfterDamageHealth;
					if (monster.TakeDamage(damage))
					{
						monsterAfterDamageHealth = monster.health;
						gameInstance.AddGuts(monster.position.Location, monster.type);
						POWERUP_TYPE loot = monster.GetLootDrop();
						if (gameInstance.whichRound == 1 && Game1.random.NextDouble() < 0.5)
						{
							loot = POWERUP_TYPE.LOG;
						}
						if (gameInstance.whichRound > 0 && (loot == POWERUP_TYPE.ZOMBIE || loot == POWERUP_TYPE.LIFE) && Game1.random.NextDouble() < 0.4)
						{
							loot = POWERUP_TYPE.LOG;
						}
						if (loot != POWERUP_TYPE.LOG && gameInstance.whichWave != 12)
						{
							gameInstance.NETspawnPowerup(loot, monster.position.Location, gameInstance.lootDuration);
						}

						//NET EnemyKilled
						PK_EnemyKilled mEnemyKilled = new();
						mEnemyKilled.id = monster.id;
						gameInstance.modInstance.Helper.Multiplayer.SendMessage(mEnemyKilled, "PK_EnemyKilled");
						Game1.playSound("Cowboy_monsterDie");

						gameInstance.monsters.RemoveAt(k);
					}
					else
					{
						monsterAfterDamageHealth = monster.health;
					}

					damage -= monsterhealth - monsterAfterDamageHealth;
					if (damage <= 0)
					{
						//NET Despawn Bullet
						PK_BulletDespawned mBulletDespawned = new();
						mBulletDespawned.id = id;
						mBulletDespawned.monsterId = monster.id;
						mBulletDespawned.damage = monsterhealth - monsterAfterDamageHealth;
						gameInstance.modInstance.Helper.Multiplayer.SendMessage(mBulletDespawned, "PK_BulletDespawned");

						queuedForDeletion = true;
					}
				}
			}
		}

		public void Update()
        {
			position.X += motion.X;
			position.Y += motion.Y;

			//Despawn bullets out of bounds
			if (position.X <= 0 || position.Y <= 0 || position.X >= 768 || position.Y >= 768)
			{
				queuedForDeletion = true;
				return;
			}

			//enemy bullet: else if (map[(enemyBullets[l].position.X + 6) / 16 / 3, (enemyBullets[l].position.Y + 6) / 16 / 3] == MAP_TILE.FENCE) ???
			//Despawn if colliding with fence
			if (gameInstance.map[position.X / 16 / 3, position.Y / 16 / 3] == MAP_TILE.FENCE)
			{
				queuedForDeletion = true;
				return;
			}

			if (isFriendly) FriendlyUpdate();
			else EnemyUpdate();
		}
	}
}
