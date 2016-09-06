﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Common
{
    public sealed class Game
    {
        #region public

        public const int VictoryLength = 5;
        public const int StepWaitInterval = 5;

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

            _gameThread = new Thread(GameLoop);
            _gameThread.Start();
        }

        public void ReportStepProcessed()
        {
            lock (_stepLock)
            {
                _stepProcessed = true;
            }
        }

        public void WaitGameCompleted() => _gameThread.Join();

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

        private void GameLoop()
        {
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

                if (FieldIsFull())
                {
                    ReportDraw();
                    return;
                }

                ReportGameStateChanged();
                WaitStepProcessed();
                MoveNextSign();
            }
        }

        private void WaitStepProcessed()
        {
            while (true)
            {
                lock (_stepLock)
                {
                    if (_stepProcessed)
                        return;
                }
                Thread.Sleep(StepWaitInterval);
            }
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
            var yLowerBound = Math.Max(move.Y - VictoryLength + 1, 0); // included
            var yUpperBould = Math.Min(move.Y, _width- VictoryLength); // eycluded
            for (var yStart = yLowerBound; yStart < yUpperBould - VictoryLength; ++yStart)
            {
                var foundVictory = true;
                for (var y = yStart; y < yStart + VictoryLength; ++y)
                {
                    if (_field[move.X, y] == _currentSign)
                        continue;
                    foundVictory = false;
                    break;
                }

                if (foundVictory)
                    return true;
            }
            return false;
        }

        /*
         * OX 
         * OXO - This kind of situation
         *  X
         */
        private bool VerticalBuilt(Cell move)
        {
            var xLowerBound = Math.Max(move.X - VictoryLength + 1, 0); // included
            var xUpperBould = Math.Min(move.X, _height - VictoryLength); // excluded
            for (var xStart = xLowerBound; xStart < xUpperBould - VictoryLength; ++xStart)
            {
                var foundVictory = true;
                for (var x = xStart; x < xStart + VictoryLength; ++x)
                {
                    if (_field[x, move.Y] == _currentSign)
                        continue;
                    foundVictory = false;
                    break;
                }

                if (foundVictory)
                    return true;
            }
            return false;
        }

        /*
         * O X
         * OXO - This kind of situation
         * X  
         */
        private bool RightDiagonalBuilt(Cell move)
        {
            var deviationLowerBound = -Math.Min(Math.Min(move.X, VictoryLength - 1), -Math.Min(1, _width - move.Y - VictoryLength + 1)); // included
            var deviationUpperBound = Math.Min(Math.Min(_height - move.X - VictoryLength + 1, 1), -Math.Min(move.Y - 1, VictoryLength - 2)); // excluded
            for (var startDeviation = deviationLowerBound; startDeviation < deviationUpperBound; startDeviation++)
            {
                var foundVictory = true;
                for (var deviation = startDeviation; deviation < startDeviation + VictoryLength; ++deviation)
                {
                    if (_field[move.X + deviation, move.Y - deviation] == _currentSign)
                        continue;
                    foundVictory = false;
                    break;
                }

                if (foundVictory)
                    return true;
            }
            return false;
        }

        /*
         * X  
         * OXO - This kind of situation
         * O X  
         */
        private bool LeftDiagonalBuilt(Cell move)
        {
            var deviationLowerBound = -Math.Min(Math.Min(move.X, move.Y), VictoryLength - 1); // included
            var deviationUpperBound = Math.Min(Math.Min(_height - move.X - VictoryLength + 1, _width - move.Y - VictoryLength + 1), 1); // excluded
            for (var startDeviation = deviationLowerBound; startDeviation < deviationUpperBound; startDeviation++)
            {
                var foundVictory = true;
                for (var deviation = startDeviation; deviation < startDeviation + VictoryLength; ++deviation)
                {
                    if (_field[move.X + deviation, move.Y + deviation] == _currentSign)
                        continue;
                    foundVictory = false;
                    break;
                }

                if (foundVictory)
                    return true;
            }
            return false;
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

        private bool FieldIsFull() => _step == _width*_height;

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

        private void ReportDraw() => ReportGameEnded(GameState.Draw, null, null);

        private bool MoveIsValid(Cell move)
        {
            var x = move.X;
            var y = move.Y;
            return x >= 0 && x < _height && y >= 0 && y < _width && _field[x, y] == CellSign.Empty;
        }

        private void OnGameStateChanged(GameStateChangedEventArgs e) => GameStateChanged?.Invoke(this, e);

        private void OnGameEnded(GameEndedEventArgs e) => GameEnded?.Invoke(this, e);

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

        private Thread _gameThread;
        private bool _started;
        private bool _stepProcessed;
        private readonly object _stateLock = new object();
        private readonly object _stepLock = new object();

        #endregion

        
    }
}
