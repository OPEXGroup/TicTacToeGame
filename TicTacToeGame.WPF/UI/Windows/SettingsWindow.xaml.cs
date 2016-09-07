using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TicTacToeGame.Common;
using TicTacToeGame.Players;
using TicTacToeGame.Players.Enums;

namespace TicTacToeGame.WPF.UI.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        ///     This will be filled on settings apply
        /// </summary>
        public GameConfiguration Configuration;

        private const string HumanPlayerName = "HUMAN";

        private readonly Dictionary<BotKind, string> _botKindToNameDict = new Dictionary<BotKind, string>(); 
        private List<string> _playerNameList;
        private string _firstPlayerString = HumanPlayerName;
        private string _secondPlayerString = HumanPlayerName;
        private string _firstPlayerCachedName = string.Empty;
        private string _secondPlayerCachedName = string.Empty;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            FillBotDict();

            _playerNameList = new List<string>(_botKindToNameDict.Values) {HumanPlayerName};
            FirstPlayerComboBox.ItemsSource = _playerNameList;
            SecondPlayerComboBox.ItemsSource = _playerNameList;
            FirstPlayerComboBox.SelectedItem = HumanPlayerName;
            SecondPlayerComboBox.SelectedItem = HumanPlayerName;
        }

        private void FillBotDict()
        {
            var botEnumValues = Enum.GetValues(typeof (BotKind)).Cast<BotKind>().ToList();
            foreach (var botEnumValue in botEnumValues)
            {
                var bot = BotFactory.BuildBot(botEnumValue);
                var name = bot.Name ?? botEnumValue.ToString();
                if (name != HumanPlayerName)
                    _botKindToNameDict.Add(botEnumValue, name);
            }
        }

        private void FirstPlayerComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FirstPlayerComboBox.SelectedItem == null)
                return;

            _firstPlayerString = (string) FirstPlayerComboBox.SelectedItem;
            if (_firstPlayerString != HumanPlayerName)
            {
                var botKind = _botKindToNameDict.First(kv => kv.Value == _firstPlayerString).Key;
                var bot = BotFactory.BuildBot(botKind);
                FirstPlayerNameTextBox.IsEnabled = false;
                _firstPlayerCachedName = FirstPlayerNameTextBox.Text;
                FirstPlayerNameTextBox.Text = bot.Name;
            }
            else
            {
                FirstPlayerNameTextBox.Text = _firstPlayerCachedName;
                FirstPlayerNameTextBox.IsEnabled = true;
            }
        }

        private void SecondPlayerComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SecondPlayerComboBox.SelectedItem == null)
                return;

            _secondPlayerString = (string)SecondPlayerComboBox.SelectedItem;
            if (_secondPlayerString != HumanPlayerName)
            {
                var botKind = _botKindToNameDict.First(kv => kv.Value == _secondPlayerString).Key;
                var bot = BotFactory.BuildBot(botKind);
                SecondPlayerNameTextBox.IsEnabled = false;
                _secondPlayerCachedName = SecondPlayerNameTextBox.Text;
                SecondPlayerNameTextBox.Text = bot.Name;
            }
            else
            {
                SecondPlayerNameTextBox.Text = _secondPlayerCachedName;
                SecondPlayerNameTextBox.IsEnabled = true;
            }
        }
    }
}
