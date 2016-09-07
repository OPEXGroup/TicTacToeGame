using System;
using System.Windows;
using ITCC.Logging.Core;
using ITCC.UI.Loggers;
using ITCC.UI.Windows;

namespace TicTacToeGame.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region public
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            DispatcherUnhandledException += (sender, args) =>
            {          
                LogException(LogLevel.Critical, args.Exception);
                Logger.FlushAll();
#if DEBUG
                args.Handled = true;
#else
                Current.Shutdown();
#endif
            };

#if DEBUG
            Logger.Level = LogLevel.Trace;
            Logger.AddBannedScope("GENERATOR");
            var logger = new ObservableLogger(1000, RunOnUiThread);
            Logger.RegisterReceiver(logger);
            _logWindow = new LogWindow(logger);
            _logWindow.Show();
#else
            Logger.Level = LogLevel.None;
#endif

            LogMessage(LogLevel.Info, "Application ready");
        }

        public static void RunOnUiThread(Action action) => Current.Dispatcher.BeginInvoke(action);

        public static void LoadDialog<TWindow>(Window current, Action<TWindow> prepareAction = null, bool centerWindowOnScreen = false)
            where TWindow : Window, new()
        {
            var dialogWindow = new TWindow();
            if (centerWindowOnScreen)
            {
                CenterWindowOnScreen(dialogWindow);
            }
            else
            {
                dialogWindow.Top = current.Top + (current.Height - dialogWindow.Height) / 2;
                dialogWindow.Left = current.Left + (current.Width - dialogWindow.Width) / 2;
            }

            dialogWindow.Owner = current;
            prepareAction?.Invoke(dialogWindow);
            dialogWindow.Closing += (sender, args) =>
            {
                LogMessage(LogLevel.Debug, $"Closed dialog {typeof(TWindow).Name}");
            };
            LogMessage(LogLevel.Debug, $"Opened dialog {typeof(TWindow).Name}");
            dialogWindow.ShowDialog();
        }

        public static void CenterWindowOnScreen(Window window)
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = window.Width;
            var windowHeight = window.Height;
            window.Left = screenWidth / 2 - windowWidth / 2;
            window.Top = screenHeight / 2 - windowHeight / 2;
        }

        public void CloseLogWindow()
        {
#if DEBUG
            try
            {
                _logWindow.Close();
            }
            catch (Exception)
            {
                // Ignore
            }
#endif
        }
        #endregion

        #region private
        private static void LogMessage(LogLevel level, string message) => Logger.LogEntry("APP", level, message);
        private static void LogException(LogLevel level, Exception exception) => Logger.LogException("APP", level, exception);

        private LogWindow _logWindow;

        #endregion
    }
}
