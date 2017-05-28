using System;

namespace TicTacToeGame.Common.Utils
{
    public class Cell : IEquatable<Cell>
    {
        #region IEquatable
        public bool Equals(Cell other) => other != null && X == other.X && Y == other.Y;

        #endregion

        #region public
        public int X { get; set; }
        public int Y { get; set; }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }
        #endregion
    }
}
