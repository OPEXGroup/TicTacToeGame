using System;
using System.Threading;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Players
{
    public class HumanPlayer : IPlayer
    {
        public const int MoveCheckInterval = 50;

        #region IPlayer
        public string Name { get; }
        public PlayerType Type => PlayerType.Human;
        public Cell GetNextMove(FieldState fieldState)
        {
            while (true)
            {
                lock (_stateLock)
                {
                    if (_nextMove != null)
                    {
                        var tmp = _nextMove;
                        _nextMove = null;
                        return tmp;
                    }
                }
                Thread.Sleep(MoveCheckInterval);
            }
        }
        #endregion

        #region public

        public HumanPlayer(string name)
        {
            Name = name;
        }

        public void SetNextMove(Cell move)
        {
            if (move == null)
                throw new ArgumentNullException(nameof(move));

            lock (_stateLock)
            {
                if (_nextMove != null)
                    throw new InvalidOperationException("Next move is already set");

                _nextMove = new Cell(move.X, move.Y);
            }
        }

        public bool HasNextMove()
        {
            lock (_stateLock)
            {
                return _nextMove != null;
            }
        }

        #endregion

        #region private

        private Cell _nextMove;
        private readonly object _stateLock = new object();

        #endregion
    }
}
