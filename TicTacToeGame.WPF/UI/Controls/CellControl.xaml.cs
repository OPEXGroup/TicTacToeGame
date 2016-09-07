using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ITCC.Logging.Core;
using TicTacToeGame.Common.Enums;

namespace TicTacToeGame.WPF.UI.Controls
{
    /// <summary>
    /// Interaction logic for CellControl.xaml
    /// </summary>
    public partial class CellControl : UserControl
    {
        private int _x;
        private int _y;
        private Action<int, int> _clickCallback;

        public CellControl()
        {
            InitializeComponent();
        }

        public void SetParams(int x, int y, Action<int, int> clickCallback)
        {
            _x = x;
            _y = y;
            _clickCallback = clickCallback;
        }

        public void LoadPicture(CellSign sign)
        {
            var path = sign == CellSign.O ? "Assets\\O.png" : "Assets\\X.png";
            SignImage.BeginInit();
            SignImage.Source = new BitmapImage(new Uri(path, UriKind.Relative));
            SignImage.EndInit();
            Logger.LogEntry("DRAW", LogLevel.Trace, $"{path} loaded into cell ({_x}, {_y})");
        }

        private void CellControll_OnClick(object sender, MouseButtonEventArgs e) => _clickCallback?.Invoke(_x, _y);
    }
}
