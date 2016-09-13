using System;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Players.Bots.Trivial
{
    /// <summary>
    ///     Next move is a random empty cell
    /// </summary>
    public class TrivialBot : BotPlayer
    {
        #region IPlayer
        public override string Name => "Trivial bot";
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
        #endregion

        #region private
        private readonly Random _random = new Random();
        #endregion
    }
}
