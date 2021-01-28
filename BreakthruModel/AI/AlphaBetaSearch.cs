using BreakthruBoardModel.Constants;
using BreakthruBoardModel.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BreakthruBoardModel.Utils;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace BreakthruBoardModel.AI
{
    public class AlphaBetaSearch
    {
        public int DepthCounter { get; set; }
        public Game.Game Position { get; set; }
        public int InitialDepthID { get; set; }
        public List<Move> LMS { get; set; }
        public Move[,] KillerMoves { get; set; }
        public int CurrentKM { get; set; }
        internal void SetIDInitialDepth(int depth)
        {
            InitialDepthID = depth;
        }

        internal void SetRootMovesList()
        {
            LMS = OrderMovesFirstCaptures(Position.Board.GetAllLegalMoves(Position.Turn.Player, Position.CanCapture(), Position.Turn.GetPreviouslyMovedPiece()));
        }

        public AlphaBetaSearch(Game.Game Position)
        {
            this.Position = Position;

            DepthCounter = 0;

            InitialDepthID = Const.DEPTH;

            LMS = new List<Move>();

            KillerMoves = new Move[Const.DEPTH, 2];

            CurrentKM = 0;
        }

        public Tuple<int, Move> NegaMax(int Depth, int alpha, int beta)
        {
            Position.SearchStats.NumberOfNodes++;
            //---------------------Transposition Tables - Start First Phase-------------------------------//
            int originalAlpha = alpha;
            long ZH = Position.TranspositionTable.ZobristHash(Position.Board.Grid, Position.Turn.Player, Position.CanCapture());
            TTEntry entryOfPosition = Position.TranspositionTable.GetEntry(ZH);

            if (entryOfPosition != null && entryOfPosition.SearchDepth >= Depth)
            {
                Position.SearchStats.TTGeneralCounter++;
                if (entryOfPosition.Type == Const.EXACT)
                {
                    Position.SearchStats.TTExactCounter++;
                    return new Tuple<int, Move>(entryOfPosition.Value, entryOfPosition.BestMove);
                }
                else if (entryOfPosition.Type == Const.LOWER_BOUND)
                {
                    Position.SearchStats.TTLowerBoundCounter++;
                    alpha = Math.Max(alpha, entryOfPosition.Value);
                }
                else if (entryOfPosition.Type == Const.UPPER_BOUND)
                {
                    Position.SearchStats.TTUpperBoundCounter++;
                    beta = Math.Min(beta, entryOfPosition.Value);
                }
                if (alpha >= beta)
                {
                    Position.SearchStats.PruningsCounter++;
                    return new Tuple<int, Move>(entryOfPosition.Value, entryOfPosition.BestMove);
                }
            }
            //---------------------Transposition Tables - End First Phase-------------------------------//

            //---------------------Evaluation/Quiescence Evaluation - Start-------------------------------//
            if (Depth == 0 || Position.Terminal())
            {
                //MONTE CARLO EVALUATION CUSTOM(NOT SUITABLE FOR THIS GAME AS THE BRANCHING FACTOR IS TOO HIGH AND NOT MANY GAMES COULD BE PLAYED FOR EACH LEAF NODE )
                //Console.WriteLine(Position.MCEvaluator.Evaluate() * (Position.Turn.Player == Color.Silver ? -1 : 1));
                //return new Tuple<int, Move>(Position.MCEvaluator.Evaluate() * (Position.Turn.Player == Color.Silver ? -1 : 1), null);

                int Evaluation = Const.USE_QUISCENCE_EVALUATION ? Position.Evaluator.QuiscentEvaluation(Const.QUISCENCE_EVALUATION_DEPTH, Position.CanCapture() ? alpha : -beta, Position.CanCapture() ? beta : -alpha) : Position.Evaluator.Evaluate() * (Position.SilverTurn() ? -1 : 1);

                return new Tuple<int, Move>(Evaluation, null);//null move since this is only an evaluation of the static board (actually there is a quiscence evaluation) 
            }
            //---------------------Evaluation/QUiscence Evaluation - End-------------------------------//

            //---------------------AlphaBeta Search - Start-------------------------------//
            var score = int.MinValue + 1;
            Move currentBestMove = null;
            List<Move> ListOfLegalMovesToBeUsed;

            //If root level then use root legal moves ordered from iterative deepening
            if (LMS.Count > 0 && DepthCounter == 0)
            {
                ListOfLegalMovesToBeUsed = LMS;
            }
            else //If not then use normal moves giving right of way to capture moves
            {
                ListOfLegalMovesToBeUsed = OrderMovesFirstCaptures(Position.Board.GetAllLegalMoves(Position.Turn.Player, Position.CanCapture(), Position.Turn.GetPreviouslyMovedPiece()));
            }

            ListOfLegalMovesToBeUsed = PutKillerMovesFirst(ListOfLegalMovesToBeUsed, Depth);

            Move firstMove = ListOfLegalMovesToBeUsed[0];
            ListOfLegalMovesToBeUsed.RemoveAt(0);

            score = RecursiveCall(firstMove, Depth, alpha, beta);

            //If using ordered legal moves than update the score needed for iterative deepening
            if (LMS.Count > 0 && InitialDepthID == Depth)
            {
                firstMove.IDScore = score;
            }

            //Attempt to implement NegaScout but not working properly
            if (score < beta)
            {

                foreach (var m in ListOfLegalMovesToBeUsed)
                {

                    int LBound = Math.Max(alpha, score);
                    int UBound = LBound + 1;

                    int newScore;

                    newScore = RecursiveCall(m, Depth, LBound, UBound);

                    if (newScore >= UBound && newScore < beta)
                    {
                        newScore = RecursiveCall(m, Depth, newScore, beta);
                    }

                    //If using ordered legal moves than update the score needed for iterative deepening
                    if (LMS.Count > 0 && InitialDepthID == Depth)
                    {
                        m.IDScore = newScore;
                    }

                    //Update score if necessary
                    if (newScore > score)
                    {
                        score = newScore;
                        currentBestMove = m;
                    }

                    //Check for prunings
                    if (newScore >= beta)
                    {
                        Position.SearchStats.PruningsCounter++;
                        KillerMoves[DepthCounter, CurrentKM] = m;
                        CurrentKM = (CurrentKM + 1) % 2;
                        break;
                    }
                }
            }

            
            //---------------------AlphaBeta Search - End-------------------------------//

            //---------------------Transposition Table - Start Second Phase-------------------------------//
            int type;
            //Decide type of value stored
            if (score <= originalAlpha)
            {
                type = Const.UPPER_BOUND;
            }
            else if (score >= beta)
            {
                type = Const.LOWER_BOUND;
            }
            else
            {
                type = Const.EXACT;
            }

            //Store entry
            Position.TranspositionTable.SetEntry(new TTEntry(score, type, ZH, Depth, currentBestMove), ZH);
            //---------------------Transposition Table - End Second Phase-------------------------------//

            return new Tuple<int, Move>(alpha, currentBestMove);
        }

        private int RecursiveCall(Move m, int Depth, int A, int B) 
        {
            int newScore;

            //Make move to modify state of the board on order to simulate it
            Position.SimulateMove(m);

            //Increase current depth counter used for both evaluating paths and to return to previous states together with moves history
            DepthCounter++;

            //Decrease depth only when switching players, otherwise keep same depth
            if (!Position.CanCapture())
            {
                //newScore = NegaMax(Depth, A, B).Item1;//This way 2 move is considered a ply
                newScore = NegaMax(Depth - 1, A, B).Item1;//This way 1 move are considered a ply
            }
            else
            {
                newScore = -NegaMax(Depth - 1, -B, -A).Item1;
            }

            //Decrease current depth counter used for both evaluating paths and to return to previous states together with moves history 
            DepthCounter--;

            //Undo move to return the board to previous state
            Position.UndoMove(m);

            return newScore;
        }

        private List<Move> PutKillerMovesFirst(List<Move> List, int Depth)
        {
            List<Move> KM = List.FindAll(o => KillerMoves[Depth, 0] == o || KillerMoves[Depth, 1] == o);
            foreach (Move m in KM)
            {
                List.Remove(m);
            }
            return KM.Concat(List).ToList();
        }

        private static List<Move> OrderMovesFirstCaptures(List<Move> lists)
        {
            return lists.OrderByDescending(o => o.Type()).ToList();
        }
        public void OrderMovesRootNode()
        {
            LMS = LMS.OrderByDescending(o => o.IDScore).ToList();
        }

        public void ReSetLMS()
        {
            LMS = new List<Move>();
        }

        public void ResetKillerMoves()
        {
            KillerMoves = new Move[Const.DEPTH + 1, 2];
        }
    }
}
