using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ITCC.UI.Utils;
using TicTacToeGame.Common;
using TicTacToeGame.Common.Enums;
using TicTacToeGame.Common.Interfaces;
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
        private const string DefaultFirstPlayerName = "Player1";
        private const string DefaultSecondPlayerName = "Player2";
        private const int DefaultWidth = 20;
        private const int DefaultHeight = 20;

        private readonly Dictionary<BotKind, string> _botKindToNameDict = new Dictionary<BotKind, string>();
        private List<string> _playerNameList;
        private string _firstPlayerString = HumanPlayerName;
        private string _secondPlayerString = HumanPlayerName;
        private string _firstPlayerCachedName = string.Empty;
        private string _secondPlayerCachedName = string.Empty;

        private IPlayer _firstPlayer;
        private IPlayer _secondPlayer;
        private int _width;
        private int _height;
        private const int BotTurnLength = 1000;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void FillBotDict()
        {
            var botEnumValues = Enum.GetValues(typeof(BotKind)).Cast<BotKind>().ToList();
            foreach (var botEnumValue in botEnumValues)
            {
                var bot = BotFactory.BuildBot(botEnumValue);
                var name = bot.Name ?? botEnumValue.ToString();
                if (name != HumanPlayerName)
                    _botKindToNameDict.Add(botEnumValue, name);
            }
        }

        private bool ConfigCorrect()
        {
            var validator = new ConditionValidator();

            validator.NonWhitespaceString(FirstPlayerNameTextBox.Text, "First player name is empty!");
            validator.NonWhitespaceString(SecondPlayerNameTextBox.Text, "Second player name is empty!");
            if (_firstPlayer.Type != PlayerType.Bot || _secondPlayer.Type != PlayerType.Bot)
                validator.AddCondition(FirstPlayerNameTextBox.Text != SecondPlayerNameTextBox.Text,
                    "Players must have different names");

            validator.NonWhitespaceString(FieldWidthTextBox.Text, "Width is incorrect");
            validator.NonWhitespaceString(FieldHeightTextBox.Text, "Height is incorrect");
            int tmp;
            validator.AddCondition(int.TryParse(FieldWidthTextBox.Text, out tmp), "Width is incorrect");
            validator.AddCondition(tmp >= Game.VictoryLength, "Width is incorrect");
            validator.AddCondition(int.TryParse(FieldHeightTextBox.Text, out tmp), "Height is incorrect");
            validator.AddCondition(tmp >= Game.VictoryLength, "Height is incorrect");

            if (!validator.ValidationPassed)
                Helpers.ShowWarning(validator.ErrorMessage);

            return validator.ValidationPassed;
        }

        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            FillBotDict();

            _playerNameList = new List<string>(_botKindToNameDict.Values) { HumanPlayerName };
            FirstPlayerComboBox.ItemsSource = _playerNameList;
            SecondPlayerComboBox.ItemsSource = _playerNameList;
            FirstPlayerComboBox.SelectedItem = HumanPlayerName;
            SecondPlayerComboBox.SelectedItem = HumanPlayerName;

            FirstPlayerNameTextBox.Text = DefaultFirstPlayerName;
            SecondPlayerNameTextBox.Text = DefaultSecondPlayerName;
            FieldWidthTextBox.Text = DefaultWidth.ToString();
            FieldHeightTextBox.Text = DefaultHeight.ToString();
        }

        private void FirstPlayerComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FirstPlayerComboBox.SelectedItem == null)
                return;

            _firstPlayerString = (string)FirstPlayerComboBox.SelectedItem;
            if (_firstPlayerString != HumanPlayerName)
            {
                var botKind = _botKindToNameDict.First(kv => kv.Value == _firstPlayerString).Key;
                var bot = BotFactory.BuildBot(botKind);
                FirstPlayerNameTextBox.IsEnabled = false;
                _firstPlayerCachedName = FirstPlayerNameTextBox.Text;
                _firstPlayer = bot;
                FirstPlayerNameTextBox.Text = bot.Name;
            }
            else
            {
                FirstPlayerNameTextBox.Text = _firstPlayerCachedName;
                _firstPlayer = new HumanPlayer(_firstPlayerCachedName);
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
                _secondPlayer = bot;
                SecondPlayerNameTextBox.Text = bot.Name;
            }
            else
            {
                SecondPlayerNameTextBox.Text = _secondPlayerCachedName;
                _secondPlayer = new HumanPlayer(_secondPlayerCachedName);
                SecondPlayerNameTextBox.IsEnabled = true;
            }
        }

        private void ApplySettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!ConfigCorrect())
                return;

            _width = Convert.ToInt32(FieldWidthTextBox.Text);
            _height = Convert.ToInt32(FieldHeightTextBox.Text);

            // Update names
            if (_firstPlayer is HumanPlayer)
                _firstPlayer = new HumanPlayer(FirstPlayerNameTextBox.Text);
            if (_secondPlayer is HumanPlayer)
                _secondPlayer = new HumanPlayer(SecondPlayerNameTextBox.Text);

            Configuration.Width = _width;
            Configuration.Height = _height;
            Configuration.FirstPlayer = _firstPlayer;
            Configuration.SecondPlayer = _secondPlayer;
            Configuration.BotTurnLength = BotTurnLength;

            Close();
        }
    }
}
