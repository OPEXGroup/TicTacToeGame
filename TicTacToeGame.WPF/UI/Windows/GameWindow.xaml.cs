using System;
using System.ComponentModel;
using System.Windows;
using ITCC.Logging.Core;
using TicTacToeGame.Common;
using TicTacToeGame.Common.Interfaces;
using TicTacToeGame.Common.Utils;

namespace TicTacToeGame.WPF.UI.Windows
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private GameConfiguration _configuration;
        private Game _game;

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

            FirstPlayerNameLabel.Content = PlayerDescription(_configuration.FirstPlayer);
            SecondPlayerNameLabel.Content = PlayerDescription(_configuration.SecondPlayer);

            _game.GameStateChanged += ProcessGameStateChanged;
            _game.GameEnded += ProcessGameEnded;
            _game.Start();
        }

        private void ProcessGameEnded(object sender, GameEndedEventArgs gameEndedEventArgs)
        {
            var winnerMessage = gameEndedEventArgs.Winner == null ? "draw" : $"{gameEndedEventArgs.Winner.Name} won";
            LogMessage(LogLevel.Debug, $"Game ended, {winnerMessage}");
        }

        private void ProcessGameStateChanged(object sender, GameStateChangedEventArgs gameStateChangedEventArgs)
        {
            LogMessage(LogLevel.Debug, $"Game state changed, step {gameStateChangedEventArgs.CurrentState.Step}");

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
