using BreakthruBoardModel.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

namespace BreakthruBoardModel.Game
{
    public class Board
    {
        public int[,] Grid { get; set; }
        public Board(int[,] configuration)
        {
            Grid = configuration;
        }

        public Tuple<List<Move>, int, int> GetLegalMoves(Point Square, bool CanCapture)
        {
            List<Move> LegalMoves = new List<Move>();
            int NLegalMoves = 0;
            int NCaptureMoves = 0;

            //Compute legal motion moves
            if (Grid[Square.X, Square.Y] != Const.EMPTY_SQUARE)
            {

                int x = Square.X;
                int y = Square.Y;

                //check horizontally-left all the cells
                for (int i = x - 1; i >= 0; i--)
                {
                    if (Grid[i, y] == Const.EMPTY_SQUARE)
                    {
                        NLegalMoves++;
                        LegalMoves.Add(new Move(new Point(Square.X, Square.Y), new Point(i, y), Grid[Square.X, Square.Y], Grid[i, y]));
                    }
                    else
                    {
                        break;
                    }
                }

                //check horizontally-right all the cells
                for (int i = x + 1; i <= Const.SIZE - 1; i++)
                {
                    if (Grid[i, y] == Const.EMPTY_SQUARE)
                    {
                        NLegalMoves++;
                        LegalMoves.Add(new Move(new Point(Square.X, Square.Y), new Point(i, y), Grid[Square.X, Square.Y], Grid[i, y]));
                    }
                    else
                    {
                        break;
                    }
                }

                //check vertically-up all the cells
                for (int i = y - 1; i >= 0; i--)
                {
                    if (Grid[x, i] == Const.EMPTY_SQUARE)
                    {
                        NLegalMoves++;
                        LegalMoves.Add(new Move(new Point(Square.X, Square.Y), new Point(x, i), Grid[Square.X, Square.Y], Grid[x, i]));
                    }
                    else
                    {
                        break;
                    }
                }

                //check vertically-down all the cells
                for (int i = y + 1; i <= Const.SIZE - 1; i++)
                {
                    if (Grid[x, i] == Const.EMPTY_SQUARE)
                    {
                        NLegalMoves++;
                        LegalMoves.Add(new Move(new Point(Square.X, Square.Y), new Point(x, i), Grid[Square.X, Square.Y], Grid[x, i]));
                    }
                    else
                    {
                        break;
                    }
                }

                if (CanCapture)
                {
                    Tuple<List<Move>, int> Captures = CaptureMoves(Square);

                    NCaptureMoves += Captures.Item2;
                    LegalMoves = LegalMoves.Concat(Captures.Item1).ToList();
                }
            }

            return new Tuple<List<Move>, int, int>(LegalMoves, NLegalMoves, NCaptureMoves);
        }

        private Tuple<List<Move>, int> CaptureMoves(Point Square) 
        {
            List<Move> CaptureMoves = new List<Move>();
            int NCaptureMoves = 0;
            int x = Square.X;
            int y = Square.Y;

            Point[] Directions = { 
                new Point(1, 1), 
                new Point(1, -1), 
                new Point(-1, -1), 
                new Point(-1, 1) 
            };

            foreach (Point offset in Directions)
            {
                if ((x + offset.X < Const.SIZE && x + offset.X >= 0) && (y + offset.Y < Const.SIZE && y + offset.Y >= 0) && Grid[x + offset.X, y + offset.Y] != Const.EMPTY_SQUARE)
                {
                    if (Utils.Utils.PieceToColor(Grid[x + offset.X, y + offset.Y]) != Utils.Utils.PieceToColor(Grid[x, y]))
                    {
                        NCaptureMoves++;
                        CaptureMoves.Add(new Move(new Point(x, y), new Point(x + offset.X, y + offset.Y), Grid[x, y], Grid[x + offset.X, y + offset.Y]));
                    }
                }
            }

            return new Tuple<List<Move>, int>(CaptureMoves, NCaptureMoves);
        }

        public List<Move> GetAllLegalMoves(Color Player, bool CanCapture, Point LastMovedPieceLocation)
        {
            List<Move> AllLegalMoves = new List<Move>();

            for (int i = 0; i < Const.SIZE; i++)
            {
                for (int j = 0; j < Const.SIZE; j++)
                {
                    if (Grid[i, j] != Const.EMPTY_SQUARE && Utils.Utils.BelongsToPlayer(Grid[i, j], Player) && LastMovedPieceLocation != new Point(i, j))
                    {
                        if (!CanCapture && Grid[i, j] == Const.FLAGSHIP) 
                        {
                            continue;
                        }
                        AllLegalMoves = AllLegalMoves.Concat(GetLegalMoves(new Point(i, j), CanCapture).Item1).ToList();
                    }
                }
            }

            return AllLegalMoves;
        }
    }
}
