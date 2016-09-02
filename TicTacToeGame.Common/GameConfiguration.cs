using TicTacToeGame.Common.Interfaces;

namespace TicTacToeGame.Common
{
    public class GameConfiguration
    {
        #region properties
        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>
        ///     Bot turn limit in milliseconds
        /// </summary>
        public int BotTurnLength { get; set; }

        public IPlayer FirstPlayer { get; set; }
        public IPlayer SecondPlayer { get; set; }
        #endregion

        #region public

        public bool IsValid()
        {
            if (Width < 3 || Height < 3)
                return false;

            if (BotTurnLength < 1)
                return false;

            if (FirstPlayer == null || SecondPlayer == null)
                return false;

            return true;
        }
        #endregion
    }
}
