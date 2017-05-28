using System;
using System.Runtime.CompilerServices;
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
        private static readonly BitmapImage XWonImage;
        private static readonly BitmapImage OWonImage;


        static CellControl()
        {
            XImage = BuildImage("Assets/X.png");
            OImage = BuildImage("Assets/O.png");
            XWonImage = BuildImage("Assets/XWon.png");
            OWonImage = BuildImage("Assets/OWon.png");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitmapImage BuildImage(string filename)
        {
            var result = new BitmapImage();
            result.BeginInit();
            result.UriSource = new Uri(filename, UriKind.Relative);
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.EndInit();
            return result;
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

        public void LoadWonPicture(CellSign sign)
        {
            SignImage.Source = sign == CellSign.O ? OWonImage : XWonImage;
        }

        private void CellControll_OnClick(object sender, MouseButtonEventArgs e) => _clickCallback?.Invoke(_x, _y);
    }
}
