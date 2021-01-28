using BreakthruBoardModel.AI;
using BreakthruBoardModel.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace BreakthruBoardModel.Game
{
    public class Game
    {
        public Board Board { get; set; }
        public int[,] LegalMoves { get; set; }
        public Color Human { get; set; }
        public Color Winner { get; set; }
        public Turn Turn { get; set; }
        public int GameStatus { get; set; }
        public Point FlagshipPosition { get; set; }
        public List<Tuple<Move, int, Point>> MovesHistory { get; set; } //Move, PlyCounter, LastMovedPieceLocation
        public ImprovedABSearch AlphaBetaSearch { get; set; }
        public TranspositionTable TranspositionTable { get; set; }
        private IterativeDeepening IterativeDeepening { get; set; }
        public Evaluation Evaluator { get; set; }
        public MonteCarloEvaluation MCEvaluator { get; set; }
        public StatsAB SearchStats { get; set; }

        public Game(int[,] configuration, Color PlayerChoice)
        {
            Turn = new Turn();

            GameStatus = Const.GAME_ON;

            MovesHistory = new List<Tuple<Move, int, Point>>();

            Board = new Board(configuration);

            Human = false ? Color.Empty : PlayerChoice;

            FlagshipPosition = new Point(5, 5);

            AlphaBetaSearch = new ImprovedABSearch(this);

            IterativeDeepening = new IterativeDeepening(AlphaBetaSearch);

            TranspositionTable = new TranspositionTable(Board);

            Evaluator = new Evaluation(this);

            MCEvaluator = new MonteCarloEvaluation(this);

            SearchStats = new StatsAB();
            
            LegalMoves = new int[11, 11];
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    LegalMoves[i, j] = Const.ILLEGAL_MOVE;
                }
            }
        }

        public int MakeMove(Move m)
        {
            MovesHistory.Add(new Tuple<Move, int, Point>(m, Turn.GetPlyCounter(), Turn.GetPreviouslyMovedPiece()));
            if (m.FromPiece == Const.FLAGSHIP) //Flagship has been moved --> Update flagship position
            {
                FlagshipPosition = m.To;
            }
            else if (m.ToPiece == Const.FLAGSHIP) //Flagship has been captured --> Set flagship as out of the board
            {
                FlagshipPosition = new Point(-1, -1);
            }
            //Make move
            Board.Grid[m.To.X, m.To.Y] = Board.Grid[m.From.X, m.From.Y];
            Board.Grid[m.From.X, m.From.Y] = 0;

            Turn.PlayerMoved(m);
            CleanLegalMoves();

            if (Terminal())
            {
                Winner = Utils.Utils.PieceToColor(MovesHistory.Last().Item1.FromPiece) == Color.Gold ? Color.Gold : Color.Silver;
                GameStatus = Const.GAME_OVER;
            }
            return 0;
        }

        public void SimulateMove(Move m)
        {
            MovesHistory.Add(new Tuple<Move, int, Point>(m, Turn.GetPlyCounter(), Turn.GetPreviouslyMovedPiece()));

            Board.Grid[m.To.X, m.To.Y] = Board.Grid[m.From.X, m.From.Y];
            Board.Grid[m.From.X, m.From.Y] = 0;

            Turn.PlayerMoved(m);

            if (m.FromPiece == Const.FLAGSHIP) //Flagship has been moved --> Update flagship position
            {
                FlagshipPosition = m.To;
            }
            else if (m.ToPiece == Const.FLAGSHIP) //Flagship has been captured --> Set flagship as out of the board
            {
                FlagshipPosition = new Point(-1, -1);
            }

            CleanLegalMoves();
        }

        public void UndoMove(Move m)
        {

            Board.Grid[m.From.X, m.From.Y] = m.FromPiece;
            Board.Grid[m.To.X, m.To.Y] = m.ToPiece;

            Turn.PlayerUnMoved(MovesHistory.Last());
            
            MovesHistory.RemoveAt(MovesHistory.Count - 1);
            //Turn.SetPreviousPieceMoved(MovesHistory.Last().Item3);


            if (m.FromPiece == Const.FLAGSHIP)
            {
                FlagshipPosition = m.From;
            }
            else if (m.ToPiece == Const.FLAGSHIP)
            {
                FlagshipPosition = m.To;
            }

            CleanLegalMoves();
        }

        public Point GetLegalMoves(Point Square)
        {

            if (Square != Turn.GetPreviouslyMovedPiece() && !(Board.Grid[Square.X, Square.Y] == Const.FLAGSHIP && Turn.GetPlyCounter() > 0))
            {
                List<Move> TempLegalMoves = Board.GetLegalMoves(Square, Turn.CanCapture()).Item1;
                foreach (Move m in TempLegalMoves)
                {
                    LegalMoves[m.To.X, m.To.Y] = m.ToPiece == 0 ? Const.LEGAL_MOVE : Const.CAPTURE_MOVE;
                }
            }
            return Board.Grid[Square.X, Square.Y] != 0 ? Square : new Point(-1, -1);
        }

        public void CleanLegalMoves()
        {
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    LegalMoves[i, j] = Const.ILLEGAL_MOVE;
                }
            }
        }

        public void AI()
        {
            Console.WriteLine("__________________________AI STARTS_____________________________");
            var (bestScore, bestMove) = true ? IterativeDeepening.Start() : AlphaBetaSearch.NegaMax(2, Const.MIN_VALUE, Const.MAX_VALUE);
            if (bestMove == null)
            {
                Console.WriteLine("STALEMATE");
                GameStatus = Const.GAME_OVER;
                return;
            }
            Console.WriteLine(bestMove.ToString() + "\t-->\t" + bestScore);
            MakeMove(bestMove);
        }

        public bool CanCapture() 
        {
            return Turn.CanCapture();
        }

        public bool SilverTurn() 
        {
            return Turn.Player == Color.Silver;
        }
        public bool GoldTurn()
        {
            return Turn.Player == Color.Gold;
        }

        public bool Terminal()
        {
            return Const.FINAL_CONFIGURATIONS_FLAGSHIP.Contains(FlagshipPosition) || FlagshipPosition == new Point(-1, -1);
        }
    }
}
