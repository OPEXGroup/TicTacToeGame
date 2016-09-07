using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITCC.Logging.Core;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.Common
{
    public sealed class Game
    {
        #region public

        public const int VictoryLength = 5;
        public const int StepWaitInterval = 50;

        public static Game CreateNewGame(GameConfiguration configuration)
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
            LogMessage(LogLevel.Info, $"Game {_height}x{_width} created ({_firstPlayer.Name} vs {_secondPlayer.Name})");
        }

        private void GameLoop()
        {
            LogMessage(LogLevel.Info, "Game loop started");
            while (true)
            {
                _step++;
                var move = GetNextMove();

                if (move == null)
                {
                    if (_currentPlayer.Type == PlayerType.Bot)
                        LogMessage(LogLevel.Info, $"Move by {_currentPlayer.Name} took longer than {_botTurnLength}ms, {NotCurrentPlayer.Name} won");
                    else
                        LogMessage(LogLevel.Info, $"{_currentPlayer.Name} tried to submit null move");
                    ReportCurrentMoveInvalid();
                    return;
                }

                if (!MoveIsValid(move))
                {
                    LogMessage(LogLevel.Info, $"Move ({move.X}, {move.Y}) by {_currentPlayer.Name} is not valid, {NotCurrentPlayer.Name} won");
                    ReportCurrentMoveInvalid();
                    return;
                }

                ApplyMove(move);
                var winningSet = GetWinningSet();
                if (winningSet != null)
                {
                    LogMessage(LogLevel.Info, $"{_currentPlayer.Name} won with ({move.X}, {move.Y}) move");
                    ReportCurrentPlayerWon(winningSet);
                    return;
                }

                if (FieldIsFull())
                {
                    LogMessage(LogLevel.Info, "Field is full, draw");
                    ReportDraw();
                    return;
                }

                LogMessage(LogLevel.Debug, $"Step {_step} computed, waiting for observers");
                ReportGameStateChanged();
                WaitStepProcessed();
                LogMessage(LogLevel.Debug, $"Step {_step} processed");
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

        private GameEndedEventArgs BuildGameEndedEventArgs(GameState state, IPlayer winner, IPlayer loser, List<Cell> winningSet) => new GameEndedEventArgs
        {
            Field = BuildFieldCopy(),
            Height = _height,
            History = new List<Cell>(_history),
            Loser = loser,
            State =  state,
            Width = _width,
            Winner = winner,
            WinningSet = winningSet
        };

        private Cell GetNextMove()
        {
            if (_currentPlayer.Type == PlayerType.Human)
                return _currentPlayer.GetNextMove(BuildFieldState());

            var moveTask = Task.Run(() => _currentPlayer.GetNextMove(BuildFieldState()));
            var waitTask = Task.Delay(_botTurnLength);

            var completedTask = Task.WhenAny(moveTask, waitTask).Result;

            if (completedTask == moveTask)
                return moveTask.Result;
            return null;
        }

        private Cell GetLastMove() => _history.LastOrDefault();

        public void ApplyMove(Cell move)
        {
            _field[move.X, move.Y] = _currentSign;
            _history.Add(move);
            LogMessage(LogLevel.Debug, $"Move ({move.X}, {move.Y} applied)");
        }

        /*
         * OO 
         * XXX - This kind of situation
         *  O
         */
        private List<Cell> HorizontalBuilt(Cell move)
        {
            var yLowerBound = Math.Max(move.Y - VictoryLength + 1, 0); // included
            var yUpperBould = Math.Min(move.Y + 1, _width - VictoryLength + 1); // excluded
            var result = new List<Cell>();
            for (var yStart = yLowerBound; yStart < yUpperBould; ++yStart)
            {
                var foundVictory = true;
                result.Clear();
                for (var y = yStart; y < yStart + VictoryLength; ++y)
                {
                    result.Add(new Cell(move.X, y));
                    if (_field[move.X, y] == _currentSign)
                        continue;
                    foundVictory = false;
                    break;
                }

                if (foundVictory)
                    return result;
            }
            return null;
        }

        /*
         * OX 
         * OXO - This kind of situation
         *  X
         */
        private List<Cell> VerticalBuilt(Cell move)
        {
            var xLowerBound = Math.Max(move.X - VictoryLength + 1, 0); // included
            var xUpperBould = Math.Min(move.X + 1, _height - VictoryLength + 1); // excluded
            var result = new List<Cell>();
            for (var xStart = xLowerBound; xStart < xUpperBould; ++xStart)
            {
                var foundVictory = true;
                result.Clear();
                for (var x = xStart; x < xStart + VictoryLength; ++x)
                {
                    result.Add(new Cell(x, move.Y));
                    if (_field[x, move.Y] == _currentSign)
                        continue;
                    foundVictory = false;
                    break;
                }

                if (foundVictory)
                    return result;
            }
            return null;
        }

        /*
         * O X
         * OXO - This kind of situation
         * X  
         */
        private List<Cell> RightDiagonalBuilt(Cell move)
        {
            //var deviationLowerBound = Math.Min(-Math.Min(move.X, VictoryLength - 1), Math.Min(-1, _width - move.Y - VictoryLength + 1)); // included
            //var deviationUpperBound = Math.Min(Math.Min(_height - move.X - VictoryLength + 1, 1), Math.Min(move.Y - 1, 1)); // excluded
            //Logger.LogEntry("DIAG", LogLevel.Info, $"{deviationLowerBound} {deviationUpperBound}");

            //var result = new List<Cell>();
            //for (var startDeviation = deviationLowerBound; startDeviation < deviationUpperBound; startDeviation++)
            //{
            //    var foundVictory = true;
            //    result.Clear();
            //    for (var deviation = startDeviation; deviation < startDeviation + VictoryLength; ++deviation)
            //    {
            //        result.Add(new Cell(move.X + deviation, move.Y - deviation));
            //        if (_field[move.X + deviation, move.Y - deviation] == _currentSign)
            //            continue;
            //        foundVictory = false;
            //        break;
            //    }

            //    if (foundVictory)
            //        return result;
            //}
            return null;
        }

        /*
         * X  
         * OXO - This kind of situation
         * O X  
         */
        private List<Cell> LeftDiagonalBuilt(Cell move)
        {
            var deviationLowerBound = -Math.Min(Math.Min(move.X, move.Y), VictoryLength - 1); // included
            var deviationUpperBound = Math.Min(Math.Min(_height - move.X - VictoryLength + 1, _width - move.Y - VictoryLength + 1), 1); // excluded

            var result = new List<Cell>();
            for (var startDeviation = deviationLowerBound; startDeviation < deviationUpperBound; startDeviation++)
            {
                var foundVictory = true;
                result.Clear();
                for (var deviation = startDeviation; deviation < startDeviation + VictoryLength; ++deviation)
                {
                    result.Add(new Cell(move.X + deviation, move.Y + deviation));
                    if (_field[move.X + deviation, move.Y + deviation] == _currentSign)
                        continue;
                    foundVictory = false;
                    break;
                }

                if (foundVictory)
                    return result;
            }
            return null;
        }

        private List<Cell> GetWinningSet()
        {
            var lastMove = GetLastMove();

            return VerticalBuilt(lastMove) ?? HorizontalBuilt(lastMove) ?? RightDiagonalBuilt(lastMove) ?? LeftDiagonalBuilt(lastMove);
        }

        private void ReportGameStateChanged()
        {
            var changedArgs = BuildGameStateChangedEventArgs();
            OnGameStateChanged(changedArgs);
        }

        private bool FieldIsFull() => _step == _width*_height;

        private void ReportGameEnded(GameState state, IPlayer winner, IPlayer loser, List<Cell> winningSet)
        {
            var endArgs = BuildGameEndedEventArgs(state, winner, loser, winningSet);
            OnGameEnded(endArgs);
        }

        private void ReportCurrentPlayerWon(List<Cell> winningSet) => ReportGameEnded(_currentSign == CellSign.O ? GameState.OWon : GameState.XWon,
            _currentPlayer,
            NotCurrentPlayer,
            winningSet);

        private void ReportMoveTimeout() => ReportGameEnded(_currentSign == CellSign.O ? GameState.OInvalidTurn : GameState.XInvalidTurn,
            NotCurrentPlayer,
            _currentPlayer,
            null);

        private void ReportCurrentMoveInvalid() => ReportGameEnded(_currentSign == CellSign.O ? GameState.OInvalidTurn : GameState.XInvalidTurn,
            NotCurrentPlayer,
            _currentPlayer,
            null);

        private void ReportDraw() => ReportGameEnded(GameState.Draw, null, null, null);

        private bool MoveIsValid(Cell move)
        {
            var x = move.X;
            var y = move.Y;
            return x >= 0 && x < _height && y >= 0 && y < _width && _field[x, y] == CellSign.Empty;
        }

        private void OnGameStateChanged(GameStateChangedEventArgs e) => GameStateChanged?.Invoke(this, e);

        private void OnGameEnded(GameEndedEventArgs e) => GameEnded?.Invoke(this, e);

        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("TICTACTOE", level, message);

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
