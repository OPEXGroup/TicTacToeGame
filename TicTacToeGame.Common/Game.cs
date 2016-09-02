using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;

namespace TicTacToeGame.Common
{
    public class Game
    {
        #region public

        public Game CreateNewGame(GameConfiguration configuration)
        {
            if (configuration == null || !configuration.IsValid())
                return null;

            return new Game(configuration);
        }

        public void Start()
        {
            
        }
        #endregion

        #region private
        private Game(GameConfiguration configuration)
        {
            _width = configuration.Width;
            _height = configuration.Height;
            _botTurnLength = configuration.BotTurnLength;
            _firstPlayer = configuration.FirstPlayer;
            _secondPlayer = configuration.SecondPlayer;

            _currentSign = CellSign.X;
            _currentPlayer = _firstPlayer;
            _field = new CellSign[_height, _width];
        }

        private void MoveNextSign()
        {
            _currentSign = _currentSign == CellSign.O ? CellSign.X : CellSign.O;
            _currentPlayer = _currentPlayer == _firstPlayer ? _secondPlayer : _firstPlayer;
        }

        private readonly int _width;
        private readonly int _height;
        private readonly int _botTurnLength;
        private readonly IPlayer _firstPlayer;
        private readonly IPlayer _secondPlayer;

        private CellSign _currentSign;
        private IPlayer _currentPlayer;
        private CellSign[,] _field;
        private int _step;

        #endregion
    }
}
