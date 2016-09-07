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
        private static readonly BitmapImage XImage;
        private static readonly BitmapImage OImage;


        static CellControl()
        {
            XImage = new BitmapImage();
            XImage.BeginInit();
            XImage.UriSource = new Uri("Assets/X.png", UriKind.Relative);
            XImage.CacheOption = BitmapCacheOption.OnLoad;
            XImage.EndInit();

            OImage = new BitmapImage();
            OImage.BeginInit();
            OImage.UriSource = new Uri("Assets/O.png", UriKind.Relative);
            OImage.CacheOption = BitmapCacheOption.OnLoad;
            OImage.EndInit();
        }

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
            SignImage.Source = sign == CellSign.O ? OImage : XImage;
            Logger.LogEntry("DRAW", LogLevel.Trace, $"{sign} loaded into cell ({_x}, {_y})");
        }

        private void CellControll_OnClick(object sender, MouseButtonEventArgs e) => _clickCallback?.Invoke(_x, _y);
    }
}
