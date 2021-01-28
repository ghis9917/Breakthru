using BreakthruBoardModel.Constants;
using System;
using System.Drawing;

namespace BreakthruBoardModel.Game

{
    public class Move
    {
        public Point From { get; set; }
        public int FromPiece { get; set; }
        public Point To { get; set; }
        public int ToPiece { get; set; }
        public int IDScore{ get; set; }

        public Move(Point From, Point To, int FromPiece, int ToPiece)
        {

            this.From = From;
            this.To = To;
            this.FromPiece = FromPiece;
            this.ToPiece = ToPiece;
            this.IDScore = 0;
        }

        public char Type()
        {
            if (ToPiece == 0)
            {
                return '-';
            }
            else
            {
                return 'x';
            }
        }

        public String LeftPart()
        {
            return GetPieceEncoding(FromPiece, From);
        }

        public String RightPart()
        {
            return GetPieceEncoding(ToPiece, To);
        }

        private String GetPieceEncoding(int p, Point c) 
        {
            switch (p)
            {
                case Const.FLAGSHIP:
                    return "F" + Utils.Utils.PointToString(c);
                case Const.SILVER_PAWN:
                    return "S" + Utils.Utils.PointToString(c);
                case Const.GOLD_PAWN:
                    return "G" + Utils.Utils.PointToString(c);
                default:
                    return "" + Utils.Utils.PointToString(c);
            }
        }

        override public String ToString()
        {
            return LeftPart() + Type() + RightPart();
        }
    }
}
