using System;
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

            _cellGroupManager.Fill(fieldState.Field, fieldState.LastMove);

            return _cellGroupManager.GetNextMove();
        }

        public override string Author => "Vladimir Tyrin";
        #endregion

        #region private
        private void Initialize(FieldState initialState)
        {
            _width = initialState.Width;
            _height = initialState.Height;
            _cellGroupManager = new CellGroupManager(initialState);
        }

        private Cell GetInitialMove() => new Cell(_height / 2, _width / 2);

        private readonly Random _random = new Random();

        private bool _justStarted = true;

        private CellGroupManager _cellGroupManager;
        private int _width;
        private int _height;

        #endregion
    }
}
