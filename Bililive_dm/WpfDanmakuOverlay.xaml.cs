using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Bililive_dm
{
    /// <summary>
    ///     WpfDanmakuOverlay.xaml 的互動邏輯
    /// </summary>
    public partial class WpfDanmakuOverlay : Window, IDanmakuWindow
    {
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080; // 不在Alt-Tab中显示 && Win10下，在所有虚拟桌面显示


        public WpfDanmakuOverlay()
        {
            InitializeComponent();

            Deactivated += Overlay_Deactivated;
            Background = Brushes.Transparent;
            SourceInitialized += delegate
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
            };
            ShowInTaskbar = false;
            Topmost = true;
            Top = SystemParameters.WorkArea.Top;
            Left = SystemParameters.WorkArea.Left;
            Width = SystemParameters.WorkArea.Width;
            Height = 550;
        }

        void IDisposable.Dispose()
        {
            // do nothing
        }

        void IDanmakuWindow.Show()
        {
            Show();
        }

        void IDanmakuWindow.Close()
        {
            Close();
        }

        void IDanmakuWindow.ForceTopmost()
        {
            Topmost = false;
            Topmost = true;
        }

        void IDanmakuWindow.AddDanmaku(DanmakuType type, string comment, uint color)
        {
            if (CheckAccess())
                lock (LayoutRoot.Children)
                {
                    var v = new FullScreenDanmaku();
                    v.Text.Text = comment;
                    v.ChangeHeight();
                    var wd = v.Text.DesiredSize.Width;

                    var dd = new Dictionary<double, bool>();
                    dd.Add(0, true);
                    foreach (var child in LayoutRoot.Children)
                        if (child is FullScreenDanmaku)
                        {
                            var c = child as FullScreenDanmaku;
                            if (!dd.ContainsKey(Convert.ToInt32(c.Margin.Top)))
                                dd.Add(Convert.ToInt32(c.Margin.Top), true);
                            if (c.Margin.Left > SystemParameters.PrimaryScreenWidth - wd - 50)
                                dd[Convert.ToInt32(c.Margin.Top)] = false;
                        }

                    double top;
                    if (dd.All(p => p.Value == false))
                        top = dd.Max(p => p.Key) + v.Text.DesiredSize.Height;
                    else
                        top = dd.Where(p => p.Value).Min(p => p.Key);
                    // v.Height = v.Text.DesiredSize.Height;
                    // v.Width = v.Text.DesiredSize.Width;
                    var s = new Storyboard();
                    var duration =
                        new Duration(
                            TimeSpan.FromTicks(Convert.ToInt64((SystemParameters.PrimaryScreenWidth + wd) /
                                                               Store.FullOverlayEffect1 * TimeSpan.TicksPerSecond)));
                    var f =
                        new ThicknessAnimation(new Thickness(SystemParameters.PrimaryScreenWidth, top, 0, 0),
                            new Thickness(-wd, top, 0, 0), duration);
                    s.Children.Add(f);
                    s.Duration = duration;
                    Storyboard.SetTarget(f, v);
                    Storyboard.SetTargetProperty(f, new PropertyPath("(FrameworkElement.Margin)"));
                    LayoutRoot.Children.Add(v);
                    s.Completed += s_Completed;
                    s.Begin();
                }
            else
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                    () => (this as IDanmakuWindow).AddDanmaku(type, comment, color))
                );
        }

        void IDanmakuWindow.OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // ignore
        }

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        private void s_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Children[0].Timeline) as FullScreenDanmaku;
            if (c != null) LayoutRoot.Children.Remove(c);
        }

        private void Overlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is WpfDanmakuOverlay) (sender as WpfDanmakuOverlay).Topmost = true;
        }
    }
}