using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Bililive_dm
{
    /// <summary>
    ///     DanmakuTextControl.xaml 的互動邏輯
    /// </summary>
    public partial class DanmakuTextControl : UserControl
    {
        public DanmakuTextControl()
        {
            InitializeComponent();
            Loaded += DanmakuTextControl_Loaded;


            var sb = (Storyboard)Resources["Storyboard1"];
            Storyboard.SetTarget(sb.Children[2], this);

            (sb.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[1].KeyTime =
                KeyTime.FromTimeSpan(new TimeSpan(Convert.ToInt64(Store.MainOverlayEffect1 * TimeSpan.TicksPerSecond)));

            (sb.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[1].KeyTime =
                KeyTime.FromTimeSpan(new TimeSpan(Convert.ToInt64(Store.MainOverlayEffect1 * TimeSpan.TicksPerSecond)));

            (sb.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[2].KeyTime =
                KeyTime.FromTimeSpan(
                    new TimeSpan(
                        Convert.ToInt64((Store.MainOverlayEffect2 + Store.MainOverlayEffect1) *
                                        TimeSpan.TicksPerSecond)));

            (sb.Children[2] as DoubleAnimationUsingKeyFrames).KeyFrames[0].KeyTime =
                KeyTime.FromTimeSpan(
                    new TimeSpan(
                        Convert.ToInt64(
                            (Store.MainOverlayEffect3 + Store.MainOverlayEffect2 + Store.MainOverlayEffect1) *
                            TimeSpan.TicksPerSecond)));
            (sb.Children[2] as DoubleAnimationUsingKeyFrames).KeyFrames[1].KeyTime =
                KeyTime.FromTimeSpan(
                    new TimeSpan(
                        Convert.ToInt64((Store.MainOverlayEffect4 + Store.MainOverlayEffect3 +
                                         Store.MainOverlayEffect2 +
                                         Store.MainOverlayEffect1) * TimeSpan.TicksPerSecond)));
        }

        public void ChangeHeight()
        {
            TextBox.FontSize = Store.MainOverlayFontsize;
            TextBox.Measure(new Size(Store.MainOverlayWidth, int.MaxValue));
            var sb = (Storyboard)Resources["Storyboard1"];
            var kf1 = sb.Children[0] as DoubleAnimationUsingKeyFrames;
            kf1.KeyFrames[1].Value = TextBox.DesiredSize.Height + 4;
        }

        private void DanmakuTextControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= DanmakuTextControl_Loaded;
        }
    }
}