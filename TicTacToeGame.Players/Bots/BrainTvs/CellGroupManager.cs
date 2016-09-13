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

            return null;
        }

        private void FillCellGroups()
        {
            
        }

        private int _width;
        private int _height;
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
