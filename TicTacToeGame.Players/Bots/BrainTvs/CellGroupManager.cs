using System.Collections.Generic;
using System.Linq;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Players.Bots.BrainTvs
{
    internal class CellGroupManager
    {
        #region public

        public CellGroupManager(FieldState state)
        {
            _width = state.Width;
            _height = state.Height;
            _ourSign = state.PlayerSign;
        }

        public void Fill(CellSign[,] cells, Cell opponentMove)
        {
            _currentField = cells;
            _opponentCells.Add(opponentMove);

            CleanUp();
            FillCellGroups();
        }

        public Cell GetNextMove()
        {
            var cell = InnerGetNextMove();
            _ourCells.Add(cell);
            return cell;
        }
        #endregion

        #region private

        private void CleanUp()
        {
            _ourDyads.Clear();
            _ourTriads.Clear();
            _ourTetrads.Clear();

            _opponentDyads.Clear();
            _opponentTriads.Clear();
            _opponentTetrads.Clear();
        }

        private Cell InnerGetNextMove()
        {
            if (_ourTetrads.Any())
                return _ourTetrads.First().GetPossibleMove();
            if (_opponentTetrads.Any())
                return _opponentTetrads.First().GetPossibleMove();

            return GetRandomMove();
        }

        private Cell GetRandomMove()
        {
            foreach (var ourCell in _ourCells)
            {
                var emptyCell = GetEmptyNeightbor(ourCell);
                if (emptyCell != null)
                    return emptyCell;
            }

            foreach (var ourCell in _opponentCells)
            {
                var emptyCell = GetEmptyNeightbor(ourCell);
                if (emptyCell != null)
                    return emptyCell;
            }

            // Unreacheable
            return new Cell(0, 0);
        }

        private Cell GetEmptyNeightbor(Cell cell)
        {
            for (var dx = -1; dx < 2; ++dx)
            {
                for (var dy = -1; dy < 2; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    var x = cell.X + dx;
                    var y = cell.Y + dy;

                    if (x < 0 || y < 0 || x >= _height || y >= _width)
                        continue;

                    if (_currentField[x, y] == CellSign.Empty)
                        return new Cell(x,y);
                }
            }

            return null;
        }

        private void FillCellGroups()
        {
            
        }

        private readonly int _width;
        private readonly int _height;
        private CellSign _ourSign;
        private CellSign[,] _currentField;

        private readonly List<Cell> _ourCells = new List<Cell>();
        private readonly List<Cell> _opponentCells = new List<Cell>();

        private readonly List<CellGroup> _ourDyads = new List<CellGroup>();
        private readonly List<CellGroup> _ourTriads = new List<CellGroup>();
        private readonly List<CellGroup> _ourTetrads = new List<CellGroup>();

        private readonly List<CellGroup> _opponentDyads = new List<CellGroup>();
        private readonly List<CellGroup> _opponentTriads = new List<CellGroup>();
        private readonly List<CellGroup> _opponentTetrads = new List<CellGroup>();

        #endregion
    }
}
