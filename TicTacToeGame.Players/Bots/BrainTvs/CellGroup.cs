using System.Linq;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Players.Bots.BrainTvs
{
    internal class CellGroup
    {
        public int QuantityType { get; set; }
        public Cell[] OpponentCells { get; set; }
        public Cell[] OurCells { get; set; }
        public Cell[] FreeCells { get; set; }
        public bool OurOpen { get; set; }
        public bool OpponentOpen { get; set; }
        public FlowDirection FlowDirection { get; set; }

        public Cell GetPossibleMove() => FreeCells.First();
    }
}
