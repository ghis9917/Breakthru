using BreakthruBoardModel.Constants;
using BreakthruBoardModel.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BreakthruBoardModel.AI
{
    public class MonteCarloEvaluation
    {

        public Game.Game Position { get; set; }
        public MonteCarloEvaluation(Game.Game G) 
        {
            Position = G;
        }
        public int Evaluate()
        {

            //Check if the position is terminal first
            if (Position.Terminal())
            {
                return Utils.Utils.PieceToColor(Position.MovesHistory.Last().Item1.FromPiece) == Color.Gold ? Const.MAX_VALUE : Const.MIN_VALUE;
            }

            int sum = 0;
            int counter = 0;

            //Play Const.MCP random games
            for (int i = 0; i < Const.MCP; i++)
            {
                sum += PlayOut(0);
                counter++;
            }

            //return average score 
            return sum / counter;
        }

        public int PlayOut(int depth)
        {
            if (Position.Terminal())
            {
                //Idea of return the evaluation of positions after playing random games -> does not work properly
                //return Evaluation(Position.Turn.Player, Position.Turn.PlyCounter == 0); 

                //Return 1 if Gold won, 0 if it lost.
                return Utils.Utils.PieceToColor(Position.MovesHistory.Last().Item1.FromPiece) == Color.Gold ? 1 : 0;
            }

            Random rd = new Random();
            List<Move> lm = Position.Board.GetAllLegalMoves(Position.Turn.Player, Position.CanCapture(), Position.Turn.GetPreviouslyMovedPiece());
            int RandIndex = rd.Next(0, lm.Count);
            Move RandMove = lm[RandIndex];

            //Play random move
            Position.SimulateMove(RandMove);

            //Continue playing the random game
            int score = PlayOut(++depth);

            //When coming back undo the move
            Position.UndoMove(RandMove);

            return score;
        }
    }
}
