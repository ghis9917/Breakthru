using BreakthruBoardModel.Constants;
using BreakthruBoardModel.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BreakthruBoardModel.AI
{
    public class Evaluation
    {
        //Features weights
        private const int MOBILITY_WEIGHT = 1;
        private const int SQUARE_CONTROL_WEIGHT = 1;
        private const int MATERIAL_WEIGHT = 8;

        public Game.Game Position { get; set; }
        private int GoldInitialScore { get; set; }
        private int SilverInitialScore { get; set; }

        public Evaluation(Game.Game G) 
        {
            Position = G;
            GoldInitialScore = GoldScore();
            SilverInitialScore = SilverScore();
        }
        public int QuiscentEvaluation(int Depth, int alpha, int beta) 
        {
            List<Move> LegalMoves = Position.Board.GetAllLegalMoves(Position.Turn.Player, Position.CanCapture(), Position.Turn.GetPreviouslyMovedPiece());

            //Filters the legal moves: keep only capture moves and flagship moves
            LegalMoves = LegalMoves.FindAll(o => (o.Type() == Const.CAPTURE_MOVE || (o.FromPiece == Const.FLAGSHIP && Const.FINAL_CONFIGURATIONS_FLAGSHIP.Contains(o.To))));

            //Check if the list of legal moves filtered is emoty or it's time to stop or the position is terminal
            if (LegalMoves.Count == 0 || Depth == 0 || Position.Terminal())
            {
                //Get static evaluation score
                int Evaluation = Position.Turn.Player == Color.Gold ? Evaluate() : -Evaluate();

                if (Evaluation < 0)
                {
                    Evaluation += Position.AlphaBetaSearch.DepthCounter;//Prefer longest path to loss
                }
                else
                {
                    Evaluation -= Position.AlphaBetaSearch.DepthCounter;//Prefer shortest path to victory
                }
                return Evaluation;
            }

            int score = Const.MIN_VALUE;

            //This for loop basically repeats the Negamax for, of course since only "action moves" are considered than it's not necessary to check wether a move is going to pass the turn or not
            foreach (Move m in LegalMoves)
            {
                Position.SimulateMove(m);
                Position.AlphaBetaSearch.DepthCounter++;
                int newScore;
                newScore = -QuiscentEvaluation(Depth-1, -beta, -alpha);
                if (newScore > score)
                {
                    score = newScore;
                }
                alpha = Math.Max(alpha, score);
                Position.AlphaBetaSearch.DepthCounter--;
                Position.UndoMove(m);
                if (alpha >= beta)
                {
                    break;
                }
            }
            return score;
        }
        public int Evaluate()
        {
            //Check if position is a win for someone
            if (Position.Terminal())
            {
                return Utils.Utils.PieceToColor(Position.MovesHistory.Last().Item1.FromPiece) == Color.Gold ? Const.MAX_VALUE/2 : Const.MIN_VALUE/2;
            }

            //Return difference of normalized values between 0 and 100, producing a range [-100, 100]. 
            //This range is obtained by calculating th einitial score of bth player at the beginning
            return Normalize(GoldScore(), Color.Gold) - Normalize(SilverScore(), Color.Silver);
        }

        private int Normalize(int v, Color player)
        {
            return (v * 100) / (player == Color.Gold ? GoldInitialScore : SilverInitialScore);
        }

        private int SilverScore()
        {
            int Material = 0;
            int Mobility = 0;
            int SquareControl = 0;

            for (int i = 0; i < Const.SIZE; i++)
            {
                for (int j = 0; j < Const.SIZE; j++)
                {
                    if (Utils.Utils.BelongsToPlayer(Position.Board.Grid[i, j], Color.Silver))
                    {
                        Material += 21 - ManhattanDistance(new Point(i, j), Position.FlagshipPosition); //Each ship has a score [1, 20] depending how close it is to the flagship

                        Tuple<List<Move>, int, int> Legals_Captures = Position.Turn.GetPreviouslyMovedPiece() == new Point(i, j) ? new Tuple<List<Move>, int, int>(null, 0, 0) : Position.Board.GetLegalMoves(new Point(i, j), Position.CanCapture());

                        Mobility += Legals_Captures.Item2;

                        SquareControl += Legals_Captures.Item3;

                        if (Position.Turn.Player == Color.Silver && Position.CanCapture() && Legals_Captures.Item1 != null)
                        {
                            foreach (Move m in Legals_Captures.Item1)
                            {
                                if (m.ToPiece == Const.FLAGSHIP)
                                {
                                    return Const.MAX_VALUE/2; //A silver ship has a capture legal move towards the flagship, return half a win
                                }
                            }
                        }
                    }
                }
            }

            return MATERIAL_WEIGHT * Material * (100 / 20) + MOBILITY_WEIGHT * Mobility * (100 / 20) + SQUARE_CONTROL_WEIGHT * SquareControl;
        }

        private int GoldScore()
        {
            int Material = 0;
            int Mobility = 0;
            int SquareControl = 0;

            for (int i = 0; i < Const.SIZE; i++)
            {
                for (int j = 0; j < Const.SIZE; j++)
                {
                    if (Utils.Utils.BelongsToPlayer(Position.Board.Grid[i, j], Color.Gold))
                    {
                        Material += 21 - ManhattanDistance(new Point(i, j), Position.FlagshipPosition); //Each ship has a score [1, 20] depending how close it is to the flagship

                        Tuple<List<Move>, int, int> Legals_Captures = Position.Turn.GetPreviouslyMovedPiece() == new Point(i, j) ? new Tuple<List<Move>, int, int>(null, 0, 0) : Position.Board.GetLegalMoves(new Point(i, j), Position.CanCapture());

                        Mobility += Legals_Captures.Item2 * (Position.Board.Grid[i, j] == Const.FLAGSHIP ? 10 : 1);

                        SquareControl += Legals_Captures.Item3;

                        if (Position.Board.Grid[i, j] == Const.FLAGSHIP && Position.Turn.Player == Color.Gold && Position.CanCapture() && Legals_Captures.Item1 != null)
                        {
                            foreach (Move m in Legals_Captures.Item1)
                            {
                                if (Const.FINAL_CONFIGURATIONS_FLAGSHIP.Contains(m.To))
                                {
                                    return Const.MAX_VALUE/2; //Flagship has the turn and a direct way to the side of the board, return half win since it's not a win yet
                                }
                            }
                        }
                    }
                }
            }

            return MATERIAL_WEIGHT * Material * (100/13) + MOBILITY_WEIGHT * Mobility * (100/13) + SQUARE_CONTROL_WEIGHT * SquareControl;
        }

        public static int ManhattanDistance(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }
    }
}
