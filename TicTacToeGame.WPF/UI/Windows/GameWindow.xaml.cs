using System.Windows;
using TicTacToeGame.Common;

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
        }

        private void GameWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var configLoaded = LoadConfiguration();
            if (!configLoaded)
            {
                Helpers.ShowWarning("Configuration is not loaded");
                return;
            }

            LoadGame();
        }

        private bool LoadConfiguration()
        {
            _configuration = new GameConfiguration();
            App.LoadDialog<SettingsWindow>(this, window => window.Configuration = _configuration);
            return _configuration.IsValid();
        }

        private bool LoadGame()
        {
            return true;
        }
    }
}
