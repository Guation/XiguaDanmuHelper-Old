using System.Windows;
using System.Windows.Controls;

namespace Bililive_dm
{
    /// <summary>
    /// FullScreenDanmaku.xaml 的互動邏輯
    /// </summary>
    public partial class FullScreenDanmaku : UserControl
    {
        public FullScreenDanmaku()
        {
            this.InitializeComponent();
        }


        public void ChangeHeight()
        {
            this.Text.FontSize = Store.FullOverlayFontsize;
            this.Text.Measure(new Size(int.MaxValue, int.MaxValue));
        }
    }
}