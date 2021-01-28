using System;
using System.Collections.Generic;
using System.Text;

namespace BreakthruBoardModel.AI
{
    public class StatsAB
    {

        /*
         TODO: 
            Implement total average evaluation score and average chosen evaluation score
         */
        public int TTGeneralCounter { get; set; }
        public int TTExactCounter { get; set; }
        public int TTLowerBoundCounter { get; set; }
        public int TTUpperBoundCounter { get; set; }
        public int PruningsCounter { get; set; }
        public int NumberOfNodes { get; set; }

        public StatsAB() 
        {
            TTGeneralCounter = 0;
            TTExactCounter = 0;
            TTLowerBoundCounter = 0;
            TTUpperBoundCounter = 0;
            PruningsCounter = 0;
            NumberOfNodes = 0;
        }

        public void ResetCounters()
        {
            TTGeneralCounter = 0;
            TTExactCounter = 0;
            TTLowerBoundCounter = 0;
            TTUpperBoundCounter = 0;
            PruningsCounter = 0;
            NumberOfNodes = 0;
        }

        public string PrintStats() 
        {
            return "" +
                "Transposition Table usages: " + TTGeneralCounter + "\n" +
                    "\tExact:" + TTExactCounter + "\n" +
                    "\tLowerBound: " + TTLowerBoundCounter + "\n" +
                    "\tUpperBound: " + TTUpperBoundCounter + "\n" +
                "Prunings: "+PruningsCounter+"\n" +
                "Nodes visited: "+NumberOfNodes;
        }
    }
}
