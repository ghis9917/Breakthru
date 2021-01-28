using BreakthruBoardModel.Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreakthruBoardModel.Game
{
    //This class keeps track of whose turn it is and checks plies rules:
    // -2 ply per player per turn are allowed if a normal piece is moved
    // -1 ply per player per turn is allowed if a capture is made
    // -1 ply is allowed if the flagship is moved or used to capture an enemy piece

    public class Turn
    {
        public bool FirstMoveOfTheGame { get; set; }
        public Color Player { get; set; }
        private int PlyCounter { get; set; }
        private int TurnCounter { get; set; }

        private Point PreviousMovedPieceLocation { get; set; }

        public Turn()
        {
            Player = Color.Gold;
            PlyCounter = 0;
            TurnCounter = 0;
            PreviousMovedPieceLocation = new Point(-1, -1);
            FirstMoveOfTheGame = true;
        }

        public void PlayerMoved(Move m)
        {

            if (m.Type() == '-') //Player moved a piece
            {
                if (PlyCounter == 0 && m.FromPiece != Const.FLAGSHIP) //Player moved a normal piece --> They get another ply
                {
                    PlyCounter++;
                    PreviousMovedPieceLocation = m.To;
                }
                else //Player moved the flagship --> The turn goes to the opponent
                {
                    PlyCounter = 0;
                    TurnCounter++;
                    Player = Player == Color.Gold ? Color.Silver : Color.Gold;
                    PreviousMovedPieceLocation = new Point(-1, -1); //Previously moved piece set to (-1, -1) since it doesn't count for the next player what you moved
                }
            }
            else if (m.Type() == 'x') //Player captured a piece --> The turn goes to the opponent
            {
                PlyCounter = 0;
                TurnCounter++;
                Player = Player == Color.Gold ? Color.Silver : Color.Gold;
                PreviousMovedPieceLocation = new Point(-1, -1); //Previously moved piece set to (-1, -1) since it doesn't count for the next player what you moved
            }
            FirstMoveOfTheGame = false;
        }

        //Undo previous move and re-establish the previous state
        public void PlayerUnMoved(Tuple<Move, int, Point> h)
        {
            Player = Utils.Utils.PieceToColor(h.Item1.FromPiece);
            PlyCounter = h.Item2;
            PreviousMovedPieceLocation = h.Item3;
        }

        public void SetPreviousPieceMoved(Point location) { PreviousMovedPieceLocation = location; }

        public void SetPlyCounter(int n) { PlyCounter = n; }

        public int GetPlyCounter() { return PlyCounter; }

        public Point GetPreviouslyMovedPiece() { return PreviousMovedPieceLocation; }

        public bool CanCapture() { return PlyCounter == 0; }//Player can capture only if it's the first "half-ply" of it's turn
    }
}
