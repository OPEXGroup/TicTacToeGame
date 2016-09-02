using TicTacToeGame.Common.Enums;

namespace TicTacToeGame.Common.Utils
{
    public class FieldState
    {
        #region public
        /// <summary>
        ///     Current field state
        /// </summary>
        public CellSign[,] Field { get; set; }
        /// <summary>
        ///     Field width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        ///     Field height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        ///     Reuse enum
        /// </summary>
        public CellSign PlayerSign { get; set; }
        /// <summary>
        ///     Last opponent move
        /// </summary>
        public Cell LastMove { get; set; }
        /// <summary>
        ///     Game step (1 is the very first)
        /// </summary>
        public int Step { get; set; }
        #endregion
    }
}
