using System.Linq;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Players.Bots.BrainTvs
{
    internal class CellGroup
    {
        public int QuantityType { get; set; }
        public Cell[] Cells { get; set; }
        public Cell[] FreeCells { get; set; }
        public bool Open { get; set; }
        public FlowDirection FlowDirection { get; set; }

        public Cell GetPossibleMove() => FreeCells.First();
        public string CellsDisplay() => string.Join(" ", Cells.Select(c => $"({c.X}, {c.Y})"));
    }
}
