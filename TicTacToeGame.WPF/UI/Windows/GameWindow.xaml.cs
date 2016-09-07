using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ITCC.Logging.Core;
using TicTacToeGame.Common;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;
using TicTacToeGame.Common.Utils;
using TicTacToeGame.Players;
using TicTacToeGame.WPF.UI.Controls;

namespace TicTacToeGame.WPF.UI.Windows
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private GameConfiguration _configuration;
        private Game _game;

        private CellControl[,] _cellControls;

        private bool _waitingForTurn;
        private readonly object _stateLock = new object();
        private IPlayer _currentPlayer;
        private CellSign _currentSign;
        private CellSign[,] _fieldState;

        public GameWindow()
        {
            InitializeComponent();

            App.CenterWindowOnScreen(this);
        }

        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("GAME WINDOW", level, message);

        private static string PlayerDescription(IPlayer player) => $"{player.Name} ({player.Type})";

        private bool LoadConfiguration()
        {
            _configuration = new GameConfiguration();
            App.LoadDialog<SettingsWindow>(this, window => window.Configuration = _configuration);
            return _configuration.IsValid();
        }

        private void LoadGame()
        {
            _game = Game.CreateNewGame(_configuration);
            if (_game == null)
            {
                LogMessage(LogLevel.Warning, "Failed to create game");
                return;
            }

            _currentPlayer = _configuration.FirstPlayer;
            _currentSign = CellSign.X;
            FirstPlayerNameLabel.Content = PlayerDescription(_configuration.FirstPlayer);
            SecondPlayerNameLabel.Content = PlayerDescription(_configuration.SecondPlayer);

            _game.GameStateChanged += ProcessGameStateChanged;
            _game.GameEnded += ProcessGameEnded;
            ReloadGrid();
            LoadCellControls();

            _game.Start();
            if (!IsHumansTurn())
                return;
            lock (_stateLock)
            {
                _waitingForTurn = true;
            }
        }

        private void ReloadGrid()
        {
            FieldGrid.Children.Clear();
            FieldGrid.RowDefinitions.Clear();
            FieldGrid.ColumnDefinitions.Clear();

            for (var i = 0; i < _configuration.Height; ++i)
            {
                FieldGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (var i = 0; i < _configuration.Width; ++i)
            {
                FieldGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        private void LoadCellControls()
        {
            var height = _configuration.Height;
            var width = _configuration.Width;

            _fieldState = new CellSign[height, width];
            _cellControls = new CellControl[height, width];
            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    _cellControls[i, j] = new CellControl();
                    _cellControls[i, j].SetParams(i, j, ProcessCellClick);
                    FieldGrid.Children.Add(_cellControls[i, j]);
                    Grid.SetRow(_cellControls[i, j], i);
                    Grid.SetColumn(_cellControls[i, j], j);
                }
            }
        }

        private void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == _configuration.FirstPlayer
                ? _configuration.SecondPlayer
                : _configuration.FirstPlayer;
            _currentSign = _currentSign == CellSign.O ? CellSign.X : CellSign.O;
        }

        private bool IsHumansTurn() => _currentPlayer.Type == PlayerType.Human;

        private void ProcessCellClick(int row, int column)
        {
            LogMessage(LogLevel.Trace, $"Cell ({row}, {column}) clicked");

            if (_fieldState[row, column] != CellSign.Empty)
            {
                return;
            }

            lock (_stateLock)
            {
                if (!_waitingForTurn)
                    return;
                _waitingForTurn = false;
            }

            var human = _currentPlayer as HumanPlayer;
            if (human == null)
            {
                LogMessage(LogLevel.Warning, "Wrong game state: processing click on bot turn");
                return;
            }

            human.SetNextMove(new Cell(row, column));
        }

        private void ProcessGameEnded(object sender, GameEndedEventArgs gameEndedEventArgs)
        {
            var winnerMessage = gameEndedEventArgs.Winner == null ? "Draw" : $"{gameEndedEventArgs.Winner.Name} won";
            LogMessage(LogLevel.Debug, $"Game ended, {winnerMessage}");
            var lastMove = gameEndedEventArgs.History.Last();
            App.RunOnUiThread(() => _cellControls[lastMove.X, lastMove.Y].LoadPicture(_currentSign));

            if (gameEndedEventArgs.WinningSet != null)
            {
                App.RunOnUiThread(() =>
                {
                    foreach (var cell in gameEndedEventArgs.WinningSet)
                    {
                        _cellControls[cell.X, cell.Y].LoadWonPicture(_currentSign);
                    }
                });
            }

            App.RunOnUiThread(() => WinnerLabel.Content = winnerMessage);
        }

        private void ProcessGameStateChanged(object sender, GameStateChangedEventArgs gameStateChangedEventArgs)
        {
            LogMessage(LogLevel.Debug, $"Game state changed, step {gameStateChangedEventArgs.CurrentState.Step}");

            for (var i = 0; i < gameStateChangedEventArgs.CurrentState.Height; ++i)
            {
                for (var j = 0; j < gameStateChangedEventArgs.CurrentState.Width; ++j)
                {
                    _fieldState[i, j] = gameStateChangedEventArgs.CurrentState.Field[i, j];
                }
            }
            var lastMove = gameStateChangedEventArgs.CurrentState.LastMove;
            var currentSign = gameStateChangedEventArgs.CurrentState.PlayerSign;
            App.RunOnUiThread(() => _cellControls[lastMove.X, lastMove.Y].LoadPicture(currentSign));

            SwitchPlayer();
            if (IsHumansTurn())
            {
                lock (_stateLock)
                {
                    _waitingForTurn = true;
                }
            }
            Thread.Sleep(200);
            _game.ReportStepProcessed();
            LogMessage(LogLevel.Debug, $"Step {gameStateChangedEventArgs.CurrentState.Step} processed");
        }

        private void GameWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var configLoaded = LoadConfiguration();
            if (!configLoaded)
            {
                Helpers.ShowWarning("Configuration is not loaded");
                return;
            }

            LogMessage(LogLevel.Info, $"Loading game: {PlayerDescription(_configuration.FirstPlayer)} vs {PlayerDescription(_configuration.SecondPlayer)}");
            LoadGame();
        }

        private void GameWindow_OnClosing(object sender, CancelEventArgs e) => ((App)Application.Current).CloseLogWindow();

    }
}
