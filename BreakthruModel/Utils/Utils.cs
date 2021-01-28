using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BreakthruBoardModel.Utils
{
    public static class Utils
    {
        public static Color PieceToColor(int v)
        {
            if (v == 2 || v == 3)
            {
                return Color.Gold;
            }
            else
            {
                return Color.Silver;
            }
        }

        public static bool BelongsToPlayer(int v, Color player)
        {
            if (player == Color.Gold)
            {
                return v == 2 || v == 3;
            }
            else
            {
                return v == 1;
            }
        }

        public static string PointToString(Point p)
        {
            return (char)(65 + p.X) + (11 - p.Y).ToString();

        }
    }
}
