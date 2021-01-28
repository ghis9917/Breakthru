using BreakthruBoardModel.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace BreakthruBoardModel.AI
{
    public class TTEntry
    {
        public int Value { get; set; }//Flag related value
        public int Type { get; set; }//Flag
        public long HashKey { get; set; }//Position hash
        public int SearchDepth { get; set; }//Depth of the position
        public Move BestMove { get; set; }//BestMove in position

        public TTEntry(int v, int t, long hk, int sd, Move m) 
        {
            Value = v;
            Type = t;
            HashKey = hk;
            SearchDepth = sd;
            BestMove = m;
        }
    }
}
