using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Players
{
    public abstract class BotPlayer : IPlayer
    {
        #region IPlayer

        public abstract string Name { get; } 
        public PlayerType Type => PlayerType.Bot;
        public abstract Cell GetNextMove(FieldState fieldState);

        #endregion

        // These will be shown in bot competitions
        public virtual string Description => null;
        public virtual string Author => null;
    }
}
