using System;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Players.Bots
{
    /// <summary>
    ///     Next move is a random empty cell
    /// </summary>
    public class TrivialBot : BotPlayer
    {
        public override string Name => "Trivial bot";
        public override Cell GetNextMove(FieldState fieldState)
        {
            while (true)
            {
                var random = new Random();
                var x = random.Next(fieldState.Height);
                var y = random.Next(fieldState.Width);
                if (fieldState.Field[x, y] == CellSign.Empty)
                    return new Cell(x, y);
            }
        }
    }
}
