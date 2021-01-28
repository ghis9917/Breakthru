using BreakthruBoardModel.Constants;
using BreakthruBoardModel.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BreakthruBoardModel.AI
{
    class IterativeDeepening
    {
        public ImprovedABSearch ABSearch { get; set; }
        public IterativeDeepening(ImprovedABSearch ABS)
        {
            ABSearch = ABS;
        }
        public Tuple<int, Move> Start() {
            
            Tuple<int, Move> BestMove = null;

            ABSearch.SetRootMovesList();

            for (int Depth = 1; Depth <= Const.DEPTH; Depth++)
            {
                //Setup time variables
                long start_time = ((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds);
                long wait_time = Const.TIME_THREADS  * 1000;
                long end_time = start_time + wait_time;

                ABSearch.SetIDInitialDepth(Depth);
                ABSearch.OrderMovesRootNode();//MOVE ORDERING for root node

                //Setup thread and start it
                Thread t = new Thread(() => {
                    BestMove = ABSearch.NegaMax(Depth, Const.ALPHA_WINDOW, Const.BETA_WINDOW);
                });
                t.Start();
                
                //Wait until thread dies or the time is up
                while (t.IsAlive && ((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds) <= end_time){}

                //Print Stats and reset counters
                Console.WriteLine(ABSearch.Position.SearchStats.PrintStats());
                ABSearch.Position.SearchStats.ResetCounters();

                //Handle thread in case it's still alive
                if (t.IsAlive)
                {
                    t.Abort();

                    while (ABSearch.DepthCounter > 0)//Go backward with the simulation of the game that has been interrupted
                    {
                        ABSearch.Position.UndoMove(ABSearch.Position.MovesHistory.Last().Item1);
                        ABSearch.DepthCounter--;
                    }

                    ABSearch.ReSetLMS();
                    ABSearch.ResetKillerMoves();

                    return BestMove;

                }
            }

            ABSearch.ReSetLMS();
            ABSearch.ResetKillerMoves();

            return BestMove;
        } 
    }
}
