using TicTacToeGame.Common.Interfaces;

namespace TicTacToeGame.Common.Utils
{
    public class GameStateChangedEventArgs
    {
        public FieldState CurrentState { get; set; }
        public IPlayer CurrentPlayer { get; set; }
    }
}
