using System.ComponentModel;
using System.Windows;
using ITCC.Logging.Core;
using TicTacToeGame.Common;
using TicTacToeGame.Common.Interfaces;

namespace TicTacToeGame.WPF.UI.Windows
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private GameConfiguration _configuration;

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

        private bool LoadGame() => true;

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
