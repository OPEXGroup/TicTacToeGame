using System;
using System.Windows;
using ITCC.Logging.Core;

namespace TicTacToeGame.WPF.UI
{
    internal static class Helpers
    {
        private const string Header = @"Game";

        public static MessageBoxResult Ask(string question) => MessageBox.Show(question, Header, MessageBoxButton.YesNo, MessageBoxImage.Question,
            MessageBoxResult.No);

        public static bool AskBinary(string question) => Ask(question) == MessageBoxResult.Yes;

        public static void DoWithConfirmation(string question, Action action)
        {
            if (Ask(question) == MessageBoxResult.Yes)
                action.Invoke();
        }

        public static void ShowNotImplemented()
        {
            Logger.LogEntry("INTERFACE", LogLevel.Debug, "User got `Not implemented` message");
            MessageBox.Show("It is not implemented yet :(", Header, MessageBoxButton.OK,
                MessageBoxImage.Question);
        }

        public static void ShowInfo(string message)
        {
            Logger.LogEntry("INTERFACE", LogLevel.Debug, $"User got info `{message}`");
            MessageBox.Show(message, Header, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowWarning(string message)
        {
            Logger.LogEntry("INTERFACE", LogLevel.Debug, $"User got warning `{message}`");
            MessageBox.Show(message, Header, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
