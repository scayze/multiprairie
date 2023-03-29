using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiplayerPrairieKing.Utility;

namespace MultiplayerPrairieKing.Components
{
    public class SpawnTask
    {
        public MONSTER_TYPE type = MONSTER_TYPE.orc;
        public int Y = 0;

        public SpawnTask()
        {

        }

        public SpawnTask(MONSTER_TYPE type, int y)
        {
            this.type = type;
            Y = y;
        }
    }
}
