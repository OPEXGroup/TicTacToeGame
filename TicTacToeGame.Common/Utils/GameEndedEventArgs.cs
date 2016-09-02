using System.Collections.Generic;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;

namespace TicTacToeGame.Common.Utils
{
    public class GameEndedEventArgs
    {
        public GameState State { get; set; }
        public IPlayer Winner { get; set; }
        public IPlayer Loser { get; set; }
        public CellSign[,] Field { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Cell> History { get; set; }
    }
}
