using System;
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
            while (true)
            {
                var x = _random.Next(fieldState.Height);
                var y = _random.Next(fieldState.Width);
                if (fieldState.Field[x, y] == CellSign.Empty)
                    return new Cell(x, y);
            }
        }

        public override string Author => "Vladimir Tyrin";
        #endregion

        #region private
        private readonly Random _random = new Random();
        #endregion
    }
}
