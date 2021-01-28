using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BreakthruBoardModel.Constants
{
    public static class Const
    {
        //GAME STATUS
        public const int GAME_OVER = -1;
        public const int GAME_ON = 0;
        public const int DRAW = 1;

        //BOARD VARS
        public const int SIZE = 11;
        public const int ILLEGAL_MOVE = -1;
        public const int EMPTY_SQUARE = 0;
        public const int SILVER_PAWN = 1;
        public const int GOLD_PAWN = 2;
        public const int FLAGSHIP = 3;
        public const int LEGAL_MOVE = 4;
        public const int MOTION_MOVE = '-';
        public const int CAPTURE_MOVE = 'x';
        public static readonly Point[] FINAL_CONFIGURATIONS_FLAGSHIP = {
            new Point(0, 0),    new Point(0, 1),    new Point(0, 2),    new Point(0, 3),    new Point(0, 4),    new Point(0, 5),    new Point(0, 6),    new Point(0, 7),    new Point(0, 8),    new Point(0, 9),    new Point(0, 10),
            new Point(1, 0),    new Point(2, 0),    new Point(3, 0),    new Point(4, 0),    new Point(5, 0),    new Point(6, 0),    new Point(7, 0),    new Point(8, 0),    new Point(9, 0),
            new Point(1, 10),   new Point(2, 10),   new Point(3, 10),   new Point(4, 10),   new Point(5, 10),   new Point(6, 10),   new Point(7, 10),   new Point(8, 10),   new Point(9, 10),
            new Point(10, 0),   new Point(10, 1),   new Point(10, 2),   new Point(10, 3),   new Point(10, 4),   new Point(10, 5),   new Point(10, 6),   new Point(10, 7),   new Point(10, 8),   new Point(10, 9),   new Point(10, 10)
        };
        public static int[,] InitialConfiguration = {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 1, 0, 0, 2, 2, 2, 0, 0, 1, 0},
            { 0, 1, 0, 2, 0, 0, 0, 2, 0, 1, 0},
            { 0, 1, 0, 2, 0, 3, 0, 2, 0, 1, 0},
            { 0, 1, 0, 2, 0, 0, 0, 2, 0, 1, 0},
            { 0, 1, 0, 0, 2, 2, 2, 0, 0, 1, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        };

        //GUI
        public const int NOT_CLICKED = 1;
        public const int CLICKED = 2;

        //TRANSPOSITION TABLE
        public const int EXACT = 1;
        public const int LOWER_BOUND = 2;
        public const int UPPER_BOUND = 3;

        //MONTE CARLO
        public const int MCP = 1;

        //Quiscence search vars
        public const bool USE_QUISCENCE_EVALUATION = true;
        public const int QUISCENCE_EVALUATION_DEPTH = 1;

        //ITERATIVE DEEPENING
        public const int DEPTH = 6;
        public const int MAX_VALUE = 1000000;
        public const int MIN_VALUE = -1000000;
        public const int TIME_THREADS = 5;

        //WINDOW
        public const int ALPHA_WINDOW = -100;
        public const int BETA_WINDOW = 100;
    }
}
