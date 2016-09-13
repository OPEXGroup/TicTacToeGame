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

            var opponentOpenTriade = _opponentTriads.FirstOrDefault(g => g.Open);
            var ourOpenTriade = _ourTriads.FirstOrDefault(g => g.Open);

            if (ourOpenTriade != null)
                return ourOpenTriade.GetPossibleMove();

            if (opponentOpenTriade != null)
                return opponentOpenTriade.GetPossibleMove();

            var ourGroup = _ourTriads.FirstOrDefault() 
                ?? _ourDyads.FirstOrDefault(g => g.Open)
                ?? _ourDyads.FirstOrDefault();
            if (ourGroup != null)
                return ourGroup.GetPossibleMove();


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
            foreach (var opponentCell in _opponentCells)
            {
                foreach (var flowDirection in AllDirections)
                {
                    var group = GetGroupByDirection(opponentCell, flowDirection);
                    if (group == null)
                        continue;
                    var preCell = GetPreGroupCell(opponentCell, flowDirection);
                    var postCell = GetPreGroupCell(opponentCell, flowDirection);

                    var cellGroup = GetOpponentTetrade(preCell, group, postCell);
                    if (cellGroup != null)
                    {
                        cellGroup.FlowDirection = flowDirection;
                        _opponentTetrads.Add(cellGroup);
                        continue;
                    }

                    cellGroup = GetOpponentTriade(preCell, group, postCell);
                    if (cellGroup != null)
                    {
                        cellGroup.FlowDirection = flowDirection;
                        _opponentTetrads.Add(cellGroup);
                        continue;
                    }

                    cellGroup = GetOpponentDyade(preCell, group, postCell);
                    if (cellGroup != null)
                    {
                        cellGroup.FlowDirection = flowDirection;
                        _opponentTetrads.Add(cellGroup);
                    }
                }
            }

            foreach (var ourCell in _ourCells)
            {
                foreach (var flowDirection in AllDirections)
                {
                    var group = GetGroupByDirection(ourCell, flowDirection);
                    if (group == null)
                        continue;
                    var preCell = GetPreGroupCell(ourCell, flowDirection);
                    var postCell = GetPreGroupCell(ourCell, flowDirection);

                    var cellGroup = GetOurTetrade(preCell, group, postCell);
                    if (cellGroup != null)
                    {
                        cellGroup.FlowDirection = flowDirection;
                        _ourTetrads.Add(cellGroup);
                        continue;
                    }

                    cellGroup = GetOurTriade(preCell, group, postCell);
                    if (cellGroup != null)
                    {
                        cellGroup.FlowDirection = flowDirection;
                        _ourTetrads.Add(cellGroup);
                        continue;
                    }

                    cellGroup = GetOurDyade(preCell, group, postCell);
                    if (cellGroup != null)
                    {
                        cellGroup.FlowDirection = flowDirection;
                        _ourTetrads.Add(cellGroup);
                    }
                }
            }
        }

        public CellGroup GetOpponentTetrade(Cell preCell, List<Cell> line, Cell postCell)
        {
            var opponentsCells = line.Where(IsOpponents).ToList();
            if (opponentsCells.Count < 4)
                return null;

            if (IsOur(line[4]))
            {
                if (preCell != null && IsEmpty(preCell))
                {
                    return new CellGroup
                    {
                        Cells = opponentsCells.ToArray(),
                        Open = false,
                        FreeCells = new[] {preCell},
                        QuantityType = 4
                    };
                }
            }
            else if (IsEmpty(line[4]))
            {
                // preCell is not ours, otherwise we won already
                var isOpen = preCell != null && IsEmpty(preCell);
                var freeCells = isOpen ? new[] {line[4], preCell} : new[] {line[4]};
                return new CellGroup
                {
                    Cells = opponentsCells.ToArray(),
                    Open = isOpen,
                    FreeCells = freeCells,
                    QuantityType = 4
                };
            }
            // line[4] is opponents'
            if (line.Any(IsOur))
                return null;
            return new CellGroup
            {
                Cells = opponentsCells.ToArray(),
                Open = false,
                FreeCells = line.Where(IsEmpty).ToArray(),
                QuantityType = 4
            };
        }

        public CellGroup GetOurTetrade(Cell preCell, List<Cell> line, Cell postCell)
        {
            var ourCells = line.Where(IsOur).ToList();
            if (ourCells.Count < 4)
                return null;

            if (IsOpponents(line[4]))
            {
                if (preCell != null && IsEmpty(preCell))
                {
                    return new CellGroup
                    {
                        Cells = ourCells.ToArray(),
                        Open = false,
                        FreeCells = new[] { preCell },
                        QuantityType = 4
                    };
                }

            }
            else if (IsEmpty(line[4]))
            {
                // preCell is not ours, otherwise we won already
                var isOpen = preCell != null && IsEmpty(preCell);
                var freeCells = isOpen ? new[] { line[4], preCell } : new[] { line[4] };
                return new CellGroup
                {
                    Cells = ourCells.ToArray(),
                    Open = isOpen,
                    FreeCells = freeCells,
                    QuantityType = 4
                };
            }
            // line[4] is ours
            if (line.Any(IsOpponents))
                return null;
            return new CellGroup
            {
                Cells = ourCells.ToArray(),
                Open = false,
                FreeCells = line.Where(IsEmpty).ToArray(),
                QuantityType = 4
            };
        }

        public CellGroup GetOpponentTriade(Cell preCell, List<Cell> line, Cell postCell)
        {
            var opponentsCells = line.Where(IsOpponents).ToList();
            if (opponentsCells.Count < 3)
                return null;

            // Gap is too large
            if (IsOpponents(line[4]))
                return null;

            // 1-cell gap
            if (IsOpponents(line[1]) && IsOpponents(line[2]))
            {
                if (IsEmpty(line[4]) && IsEmpty(line[3]))
                {
                    var isOpen = preCell != null && IsEmpty(preCell);
                    var freeCells = isOpen ? new[] {preCell, line[3]} : new[] {line[3]};
                    return new CellGroup
                    {
                        Cells = new[] {line[0], line[1], line[2]},
                        FreeCells = freeCells,
                        Open = isOpen,
                        QuantityType = 3
                    };
                }
                return null;
            }

            return null;
        }

        public CellGroup GetOurTriade(Cell preCell, List<Cell> line, Cell postCell)
        {
            var ourCells = line.Where(IsOur).ToList();
            if (ourCells.Count < 3)
                return null;

            return null;
        }

        public CellGroup GetOpponentDyade(Cell preCell, List<Cell> line, Cell postCell)
        {
            var opponentsCells = line.Where(IsOpponents).ToList();
            if (opponentsCells.Count < 2)
                return null;

            return null;
        }

        public CellGroup GetOurDyade(Cell preCell, List<Cell> line, Cell postCell)
        {
            var ourCells = line.Where(IsOur).ToList();
            if (ourCells.Count < 2)
                return null;

            return null;
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
                        new Cell(x, y - 4)
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
                        new Cell(x, y + 4)
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
                        new Cell(x + 4, y)
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
                        new Cell(x - 4, y)
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
                        new Cell(x + 4, y + 4)
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
                        new Cell(x - 4, y + 4)
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
                        new Cell(x + 4, y - 4)
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
                        new Cell(x - 4, y - 4)
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

        private CellSign GetCellSign(Cell cell) => _currentField[cell.X, cell.Y];
        private bool IsOur(Cell cell) => GetCellSign(cell) == _ourSign;

        private bool IsOpponents(Cell cell)
        {
            var opponentSign = _ourSign == CellSign.O ? CellSign.X : CellSign.O;
            return GetCellSign(cell) == opponentSign;
        }

        private bool IsEmpty(Cell cell) => GetCellSign(cell) == CellSign.Empty;

        private Cell GetPreGroupCell(Cell cell, FlowDirection direction)
            => GetCellByDistanceAndDirection(cell, 1, OppositeDirection(direction));

        private Cell GetPostGroupCell(Cell cell, FlowDirection direction)
            => GetCellByDistanceAndDirection(cell, Game.VictoryLength, direction);

        private readonly int _width;
        private readonly int _height;
        private readonly CellSign _ourSign;
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
