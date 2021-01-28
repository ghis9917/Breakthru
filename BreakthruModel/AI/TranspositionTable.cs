using BreakthruBoardModel.Constants;
using BreakthruBoardModel.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BreakthruBoardModel.AI
{
    public class TranspositionTable
    {
        public TTEntry[] TT { get; set; }
        private long[,,] RandPositions { get; set; }
        public long CurrentHash { get; set; }//Updated but not used

        private long RandSilverTurn { get; set; }
        private long RandGoldTurn { get; set; }
        private long CapturesAllowed { get; set; }
        private long CapturesNotAllowed { get; set; }
        public TranspositionTable(Board Board)
        {
            InitRandoms();

            InitTT();

            //Initialize current Hash
            CurrentHash = ZobristHash(Board.Grid, Color.Gold, true);
        }

        private void InitRandoms() 
        {
            //Initialize set of random number needed for static position (11 x 11 x 4) - Empty cells are considered as well
            RandPositions = new long[11, 11, 4];
            Random rd = new Random();
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    for (int p = 0; p <= 3; p++)
                    {
                        RandPositions[i, j, p] = LongRandom(0, long.MaxValue, rd);
                    }
                }
            }

            //2 Random values to encode the side in the position
            RandGoldTurn = LongRandom(0, long.MaxValue, rd);
            RandSilverTurn = LongRandom(0, long.MaxValue, rd);

            //2 Random values to encode the possibility to capture or move the flagship in a position
            CapturesAllowed = LongRandom(0, long.MaxValue, rd);
            CapturesNotAllowed = LongRandom(0, long.MaxValue, rd);
        }

        private void InitTT() 
        {
            //Initialize TT with 2^20 entries, each one of them to null. 2^20 since 20 bits are used for the indeces
            TT = new TTEntry[1048576];
            for (int i = 0; i < TT.Length - 1; i++)
            {
                TT[i] = null;
            }
        }

        //Function that returns a random long, C# doesn't have its own
        private long LongRandom(long min, long max, Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }

        public void SetEntry(TTEntry Entry)//Set entry using current hash
        {
            int Index = (int)(CurrentHash & (0xFFFFF));

            if (TT[Index] == null)//If the entry is null then the new one can be inserted without checking conditions
            {
                TT[Index] = Entry;
            }
            else if (TT[Index].SearchDepth <= Entry.SearchDepth)//Using Deep policy, store new ones only if they have a deeper subtree
            {
                TT[Index] = Entry;
            }
        }

        public void SetEntry(TTEntry Entry, long ZH)//Overload of previous function to make it work by calculating every time the hash
        {
            int Index = (int)(ZH & (0xFFFFF));
            if (TT[Index] == null)//If the entry is null then the new one can be inserted without checking conditions
            {
                TT[Index] = Entry;
            }
            else if (TT[Index].SearchDepth <= Entry.SearchDepth)//Using Deep policy, store new ones only if they have a deeper subtree
            {
                TT[Index] = Entry;
            }
        }

        public TTEntry GetEntry()//Set entry using current hash
        {
            int Index = (int)(CurrentHash & (0xFFFFF));
            return TT[Index] != null && TT[Index].HashKey == CurrentHash ? TT[Index] : null;
        }

        public TTEntry GetEntry(long ZH)//Overload of previous function to make it work by calculating every time the hash
        {
            int Index = (int)(ZH & (0xFFFFF));
            return TT[Index] != null && TT[Index].HashKey == ZH ? TT[Index] : null;
        }

        public long ZobristHash(int[,] p, Color Player, bool CanCapture) 
        {
            long code = 0;
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    //Hash every single position (even empty cells, which could actually be left out)
                    code ^= RandPositions[i, j, p[i, j]];
                }
            }

            //Hash side and plycounter(which tells if the ply has been fully done or only half of it has been played)
            code ^= Player == Color.Gold ? RandGoldTurn : RandSilverTurn;
            code ^= CanCapture ? CapturesAllowed : CapturesNotAllowed;

            return code;
        }

        //MOVE and UNMOVE functions to make the hashing faster, currently not used, also the hashing of the side and the ply is missing
        public void MoveMade(Move m)
        {
            CurrentHash ^= RandPositions[m.To.X, m.To.Y, m.ToPiece];
            CurrentHash ^= RandPositions[m.From.X, m.From.Y, m.FromPiece];
            CurrentHash ^= RandPositions[m.From.X, m.From.Y, Const.EMPTY_SQUARE];
            CurrentHash ^= RandPositions[m.To.X, m.To.Y, m.FromPiece];  
        }

        public void MoveUnMade(Move m)
        {
            CurrentHash ^= RandPositions[m.To.X, m.To.Y, m.FromPiece];
            CurrentHash ^= RandPositions[m.From.X, m.From.Y, Const.EMPTY_SQUARE];
            CurrentHash ^= RandPositions[m.From.X, m.From.Y, m.FromPiece];
            CurrentHash ^= RandPositions[m.To.X, m.To.Y, m.ToPiece];
        }
    }
}
