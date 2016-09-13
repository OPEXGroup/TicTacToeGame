using System;
using System.Collections.Generic;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Players.Bots.BrainTvs
{
    internal class BrainTvsBot : BotPlayer
    {
        #region BotPlayer
        public override string Name => "bra1n_tvs bot";
        public override Cell GetNextMove(FieldState fieldState)
        {
            if (_justStarted)
            {
                _justStarted = false;
                Initialize(fieldState);
            }

            if (fieldState.Step == 1)
            {
                return GetInitialMove();
            }

            SaveCurrentState(fieldState);

            return ComputeNextMove();
        }

        public override string Author => "Vladimir Tyrin";
        #endregion

        #region private
        private void Initialize(FieldState initialState)
        {
            _width = initialState.Width;
            _height = initialState.Height;
            _ourSign = initialState.PlayerSign;
        }

        private Cell GetInitialMove() => new Cell(_height / 2, _width / 2);

        private void SaveCurrentState(FieldState currentState)
        {
            _opponentMoveList.Add(currentState.LastMove);
            _currentField = currentState.Field;
        }

        private Cell ComputeNextMove()
        {
            return null;
        }

        private readonly Random _random = new Random();

        private bool _justStarted = true;

        private int _width;
        private int _height;
        private CellSign _ourSign;
        private CellSign[,] _currentField;
        private readonly List<Cell> _opponentMoveList = new List<Cell>();

        #endregion
    }
}
