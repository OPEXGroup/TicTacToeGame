using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Common
{
    public class Game
    {
        #region public

        public const int VictoryLength = 5;

        public Game CreateNewGame(GameConfiguration configuration)
        {
            if (configuration == null || !configuration.IsValid())
                return null;

            return new Game(configuration);
        }

        public void Start()
        {
            lock (_stateLock)
            {
                if (_started)
                    return;
                _started = true;
            }

            while (true)
            {
                _step++;
                var move = GetNextMove();
                if (move == null || !MoveIsValid(move))
                {
                    ReportCurrentMoveInvalid();
                    return;
                }

                ApplyMove(move);
                if (LastMoveVictorious())
                {
                    ReportCurrentPlayerWon();
                    return;
                }

                ReportGameStateChanged();
                MoveNextSign();
            }
        }

        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;
        public event EventHandler<GameEndedEventArgs> GameEnded; 
         
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

        private CellSign[,] BuildFieldCopy()
        {
            var fieldCopy = new CellSign[_height, _width];
            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width; j++)
                {
                    fieldCopy[i, j] = _field[i, j];
                }
            }
            return fieldCopy;
        }

        private FieldState BuildFieldState() => new FieldState
        {
            Field = BuildFieldCopy(),
            Height = _height,
            LastMove = GetLastMove(),
            PlayerSign = _currentSign,
            Step = _step,
            Width = _width
        };

        private GameStateChangedEventArgs BuildGameStateChangedEventArgs() => new GameStateChangedEventArgs
        {
            CurrentState = BuildFieldState(),
            CurrentPlayer = _currentPlayer
        };

        private GameEndedEventArgs BuildGameEndedEventArgs(GameState state, IPlayer winner, IPlayer loser) => new GameEndedEventArgs
        {
            Field = BuildFieldCopy(),
            Height = _height,
            History = new List<Cell>(_history),
            Loser = loser,
            State =  state,
            Width = _width,
            Winner = winner
        };

        private Cell GetNextMove()
        {
            if (_currentPlayer.Type == PlayerType.Human)
                return _currentPlayer.GetNextMove(BuildFieldState());

            var moveTask = Task.Run(() => _currentPlayer.GetNextMove(BuildFieldState()));
            Thread.Sleep(_botTurnLength);
            if (!moveTask.IsCompleted)
                return null;

            return moveTask.Result;
        }

        private Cell GetLastMove() => _history.LastOrDefault();

        public void ApplyMove(Cell move)
        {
            _field[move.X, move.Y] = _currentSign;
            _history.Add(move);
        }

        /*
         * OO 
         * XXX - This kind of situation
         *  O
         */
        private bool HorizontalBuilt(Cell move)
        {
            var xLowerBound = Math.Min(move.X - 4, 0);
            for (var i = xLowerBound; i < xLowerBound + VictoryLength; ++i)
            {
                if (_field[i, move.Y] != _currentSign)
                    return false;
            }
            return true;
        }

        /*
         * OX 
         * OXO - This kind of situation
         *  X
         */
        private bool VerticalBuilt(Cell move)
        {
            var yLowerBound = Math.Min(move.Y - 4, 0);
            for (var i = yLowerBound; i < yLowerBound + VictoryLength; ++i)
            {
                if (_field[move.X, i] != _currentSign)
                    return false;
            }
            return true;
        }

        /*
         * O X
         * OXO - This kind of situation
         * X  
         */
        private bool RightDiagonalBuilt(Cell move)
        {
            return true;
        }

        /*
         * X  
         * OXO - This kind of situation
         * O X  
         */
        private bool LeftDiagonalBuilt(Cell move)
        {
            return true;
        }

        private bool LastMoveVictorious()
        {
            var lastMove = GetLastMove();

            return VerticalBuilt(lastMove)
                   || HorizontalBuilt(lastMove)
                   || RightDiagonalBuilt(lastMove)
                   || LeftDiagonalBuilt(lastMove);
        }

        private void ReportGameStateChanged()
        {
            var changedArgs = BuildGameStateChangedEventArgs();
            OnGameStateChanged(changedArgs);
        }

        private void ReportGameEnded(GameState state, IPlayer winner, IPlayer loser)
        {
            var endArgs = BuildGameEndedEventArgs(state, winner, loser);
            OnGameEnded(endArgs);
        }

        private void ReportCurrentPlayerWon() => ReportGameEnded(_currentSign == CellSign.O ? GameState.OWon : GameState.XWon,
            _currentPlayer,
            NotCurrentPlayer);

        private void ReportCurrentMoveInvalid() => ReportGameEnded(_currentSign == CellSign.O ? GameState.OInvalidTurn : GameState.XInvalidTurn,
            NotCurrentPlayer,
            _currentPlayer);

        private bool MoveIsValid(Cell move)
        {
            var x = move.X;
            var y = move.Y;
            return x >= 0 && x < _height && y >= 0 && y < _width && _field[x, y] == CellSign.Empty;
        }

        protected virtual void OnGameStateChanged(GameStateChangedEventArgs e) => GameStateChanged?.Invoke(this, e);

        protected virtual void OnGameEnded(GameEndedEventArgs e) => GameEnded?.Invoke(this, e);

        private readonly int _width;
        private readonly int _height;
        private readonly int _botTurnLength;
        private readonly IPlayer _firstPlayer;
        private readonly IPlayer _secondPlayer;

        private CellSign _currentSign;
        private IPlayer _currentPlayer;
        private IPlayer NotCurrentPlayer => _currentPlayer == _firstPlayer ? _secondPlayer : _firstPlayer;
        private readonly CellSign[,] _field;
        private int _step;
        private readonly List<Cell> _history = new List<Cell>();

        private bool _started;
        private readonly object _stateLock = new object();

        #endregion

        
    }
}
