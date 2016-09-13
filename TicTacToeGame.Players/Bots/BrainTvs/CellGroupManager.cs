using System;
using System.Collections.Generic;
using System.Linq;
using TicTacToeGame.Common;
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

        private List<Cell> GetGroupByDirection(Cell cell, FlowDirection direction)
        {
            var x = cell.X;
            var y = cell.Y;
            var yIsSmall = y < Game.VictoryLength - 1;
            var xIsSmall = x < Game.VictoryLength - 1;
            var xIsBig = x > _height - Game.VictoryLength;
            var yIsBig = y > _width - Game.VictoryLength;

            switch (direction)
            {
                case FlowDirection.RightToLeft:
                    if (yIsSmall)
                        return null;
                    return new List<Cell>
                    {
                        new Cell(x, y),
                        new Cell(x, y - 1),
                        new Cell(x, y - 2),
                        new Cell(x, y - 3),
                        new Cell(x, y - 4),
                    };
                case FlowDirection.LeftToRight:
                    if (yIsBig)
                        return null;
                    return new List<Cell>
                    {
                        new Cell(x, y),
                        new Cell(x, y + 1),
                        new Cell(x, y + 2),
                        new Cell(x, y + 3),
                        new Cell(x, y + 4),
                    };
                case FlowDirection.TopToBottom:
                    if (xIsBig)
                        return null;
                    return new List<Cell>
                    {
                        new Cell(x, y),
                        new Cell(x + 1, y),
                        new Cell(x + 2, y),
                        new Cell(x + 3, y),
                        new Cell(x + 4, y),
                    };
                case FlowDirection.BottomToTop:
                    if (xIsSmall)
                        return null;
                    return new List<Cell>
                    {
                        new Cell(x, y),
                        new Cell(x - 1, y),
                        new Cell(x - 2, y),
                        new Cell(x - 3, y),
                        new Cell(x - 4, y),
                    };
                case FlowDirection.DiagonalDownRight:
                    if (xIsBig || yIsBig)
                        return null;
                    return new List<Cell>
                    {
                        new Cell(x, y),
                        new Cell(x + 1, y + 1),
                        new Cell(x + 2, y + 2),
                        new Cell(x + 3, y + 3),
                        new Cell(x + 4, y + 4),
                    };
                case FlowDirection.DiagonalDownLeft:
                    if (xIsSmall || yIsBig)
                        return null;
                    return new List<Cell>
                    {
                        new Cell(x, y),
                        new Cell(x - 1, y + 1),
                        new Cell(x - 2, y + 2),
                        new Cell(x - 3, y + 3),
                        new Cell(x - 4, y + 4),
                    };
                case FlowDirection.DiagonalUpRight:
                    if (xIsBig || yIsSmall)
                        return null;
                    return new List<Cell>
                    {
                        new Cell(x, y),
                        new Cell(x + 1, y - 1),
                        new Cell(x + 2, y - 2),
                        new Cell(x + 3, y - 3),
                        new Cell(x + 4, y - 4),
                    };
                case FlowDirection.DiagonalUpLeft:
                    if (xIsSmall || yIsSmall)
                        return null;
                    return new List<Cell>
                    {
                        new Cell(x, y),
                        new Cell(x - 1, y - 1),
                        new Cell(x - 2, y - 2),
                        new Cell(x - 3, y - 3),
                        new Cell(x - 4, y - 4),
                    };
                default:
                    return null;
            }
        }

        private FlowDirection OppositeDirection(FlowDirection direction)
        {
            switch (direction)
            {
                case FlowDirection.RightToLeft:
                    return FlowDirection.LeftToRight;
                case FlowDirection.LeftToRight:
                    return FlowDirection.RightToLeft;
                case FlowDirection.TopToBottom:
                    return FlowDirection.BottomToTop;
                case FlowDirection.BottomToTop:
                    return FlowDirection.TopToBottom;
                case FlowDirection.DiagonalDownRight:
                    return FlowDirection.DiagonalUpLeft;
                case FlowDirection.DiagonalDownLeft:
                    return FlowDirection.DiagonalUpRight;
                case FlowDirection.DiagonalUpRight:
                    return FlowDirection.DiagonalDownLeft;
                case FlowDirection.DiagonalUpLeft:
                    return FlowDirection.DiagonalDownRight;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        // We assume distance > 0
        private Cell GetCellByDistanceAndDirection(Cell cell, int distance, FlowDirection direction)
        {
            var x = cell.X;
            var y = cell.Y;
            var yIsSmall = y < distance;
            var xIsSmall = x < distance;
            var xIsBig = x > _height - distance - 1;
            var yIsBig = y > _width - distance - 1;

            int dx;
            int dy;

            switch (direction)
            {
                case FlowDirection.RightToLeft:
                    if (yIsSmall)
                        return null;
                    dx = 0;
                    dy = -distance;
                    break;
                case FlowDirection.LeftToRight:
                    if (yIsBig)
                        return null;
                    dx = 0;
                    dy = distance;
                    break;
                case FlowDirection.TopToBottom:
                    if (xIsBig)
                        return null;
                    dx = distance;
                    dy = 0;
                    break;
                case FlowDirection.BottomToTop:
                    if (xIsSmall)
                        return null;
                    dx = -distance;
                    dy = 0;
                    break;
                case FlowDirection.DiagonalDownRight:
                    if (xIsBig || yIsBig)
                        return null;
                    dx = distance;
                    dy = distance;
                    break;
                case FlowDirection.DiagonalDownLeft:
                    if (xIsSmall || yIsBig)
                        return null;
                    dx = -distance;
                    dy = distance;
                    break;
                case FlowDirection.DiagonalUpRight:
                    if (xIsBig || yIsSmall)
                        return null;
                    dx = distance;
                    dy = -distance;
                    break;
                case FlowDirection.DiagonalUpLeft:
                    if (xIsSmall || yIsSmall)
                        return null;
                    dx = -distance;
                    dy = -distance;
                    break;
                default:
                    return null;
            }

            return new Cell(x + dx, y + dy);
        }

        private Cell GetPreGroupCell(Cell cell, FlowDirection direction)
            => GetCellByDistanceAndDirection(cell, 1, OppositeDirection(direction));

        private Cell GetPostGroupCell(Cell cell, FlowDirection direction)
            => GetCellByDistanceAndDirection(cell, Game.VictoryLength, direction);

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

        private static readonly List<FlowDirection> AllDirections = Enum.GetValues(typeof(FlowDirection)).Cast<FlowDirection>().ToList();

        #endregion
    }
}
