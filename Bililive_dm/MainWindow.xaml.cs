using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Serialization;
using XiguaDanmakuHelper;
using Baidu;
using System.Diagnostics;

namespace Bililive_dm
{
    /// <summary>
    ///     MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = -20;
        private const int _maxCapacity = 100;
        private int abc = 0;

        private readonly Queue<MessageModel> _danmakuQueue = new Queue<MessageModel>();

        private readonly ObservableCollection<string> _messageQueue = new ObservableCollection<string>();

        private readonly Thread ProcDanmakuThread;

        private readonly ObservableCollection<SessionItem> SessionItems = new ObservableCollection<SessionItem>();

        private readonly DispatcherTimer timer;
        private Api b;
        private IDanmakuWindow fulloverlay;
        private Thread getDanmakuThread;
        public MainOverlay overlay;
        private readonly Thread releaseThread;

        private StoreModel settings;
        
        private bool ChatOpt;
        private bool GiftOpt;
        private bool LikeOpt;
        private bool Danmu1;


        public MainWindow()
        {
            
            InitializeComponent();
            
            //初始化日志

            try
            {
                LiverName.Text = Properties.Settings.Default.name;
            }
            catch
            {
                LiverName.Text = "sy挂神";
            }

            ChatOpt = true;
            GiftOpt = true;
            LikeOpt = true;
            Danmu1 = true;
            b = new Api();
            overlay_enabled = true;
            OpenOverlay();
            overlay.Show();

            Closed += MainWindow_Closed;

            Api.OnMessage += b_ReceivedDanmaku;
            Api.OnLeave += OnLiveStop;
//            b.OnMessage += ProcDanmaku;
            Api.LogMessage += b_LogMessage;
            Api.OnRoomCounting += b_ReceivedRoomCount;


            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, FuckMicrosoft,
                Dispatcher);
            timer.Start();

            Log.DataContext = _messageQueue;

            releaseThread = new Thread(() =>
            {
                while (true)
                {
                    Utils.ReleaseMemory(true);
                    Thread.Sleep(30 * 1000);
                }
            });
            releaseThread.IsBackground = true;
            getDanmakuThread = new Thread(() =>
            {
                while (true)
                    if (b.isLive)
                    {
                        b.GetDanmaku();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(100000);
                    }
            });
            getDanmakuThread.IsBackground = true;
            //            releaseThread.Start();
            ProcDanmakuThread = new Thread(() =>
            {
                while (true)
                {
                    lock (_danmakuQueue)
                    {
                        var count = 0;
                        if (_danmakuQueue.Any()) count = (int) Math.Ceiling(_danmakuQueue.Count / 30.0);

                        for (var i = 0; i < count; i++)
                            if (_danmakuQueue.Any())
                            {
                                var danmaku = _danmakuQueue.Dequeue();
                                ProcDanmaku(danmaku);
                            }
                    }

                    Thread.Sleep(25);
                }
            })
            {
                IsBackground = true
            };
            ProcDanmakuThread.Start();

            for (var i = 0; i < 100; i++) _messageQueue.Add("");
            logging("可以点击日志复制到剪贴板");

            Loaded += MainWindow_Loaded;
        }

        private void b_LogMessage(string e)
        {
            logging(e);
        }

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var sc = Log.Template.FindName("LogScroll", Log) as ScrollViewer;
            sc?.ScrollToEnd();
            showChat.IsChecked = ChatOpt;
            showPresent.IsChecked = GiftOpt;
            try
            {
                var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                            IsolatedStorageScope.Domain |
                                                            IsolatedStorageScope.Assembly, null, null);
                var settingsreader =
                    new XmlSerializer(typeof(StoreModel));
                var reader = new StreamReader(new IsolatedStorageFileStream(
                    "settings.xml", FileMode.Open, isoStore));
                settings = (StoreModel) settingsreader.Deserialize(reader);
                reader.Close();
            }
            catch (Exception)
            {
                settings = new StoreModel();
            }

            settings.SaveConfig();
            settings.toStatic();
            OptionDialog.LayoutRoot.DataContext = settings;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
        }

        ~MainWindow()
        {
            if (fulloverlay != null)
            {
                fulloverlay.Dispose();
                fulloverlay = null;
            }
        }

        private void FuckMicrosoft(object sender, EventArgs eventArgs)
        {
            if (fulloverlay != null) fulloverlay.ForceTopmost();
            if (overlay != null)
            {
                overlay.Topmost = false;
                overlay.Topmost = true;
            }
        }

        private void OpenOverlay()
        {
            overlay = new MainOverlay();
            overlay.Deactivated += overlay_Deactivated;
            overlay.SourceInitialized += delegate
            {
                var hwnd = new WindowInteropHelper(overlay).Handle;
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            };
            overlay.Background = Brushes.Transparent;
            overlay.ShowInTaskbar = false;
            overlay.Topmost = true;
            overlay.Top = SystemParameters.WorkArea.Top + Store.MainOverlayXoffset;
            overlay.Left = SystemParameters.WorkArea.Right - Store.MainOverlayWidth + Store.MainOverlayYoffset;
            overlay.Height = SystemParameters.WorkArea.Height;
            overlay.Width = Store.MainOverlayWidth;
        }

        private void overlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is MainOverlay) (sender as MainOverlay).Topmost = true;
        }

        private async void connbtn_Click(object sender, RoutedEventArgs e)
        {
            Name = LiverName.Text.Trim();
            b = new Api(Name);

            ConnBtn.IsEnabled = false;
            DisconnBtn.IsEnabled = false;
            var connectresult = false;
            logging("正在连接");

            connectresult = await b.ConnectAsync();

            if (connectresult)
            {
                logging("連接成功");
                AddDMText("提示", "連接成功", true);
                getDanmakuThread.Start();
            }
            else
            {
                logging("連接失敗");
                AddDMText("提示", "連接失敗", true);
                ConnBtn.IsEnabled = true;
            }

            LiverName.Text = b.user.ToString();
            DisconnBtn.IsEnabled = true;
        }

        public void b_ReceivedRoomCount(long popularity)
        {
//            logging("當前房間人數:" + e.UserCount);
//            AddDMText("當前房間人數", e.UserCount+"", true);
            //AddDMText(e.Danmaku.CommentUser, e.Danmaku.CommentText);
            if (CheckAccess())
            {
                OnlinePopularity.Text = popularity.ToString();
                //AddDMText("当前房间人气", popularity.ToString() + "", true);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { OnlinePopularity.Text = popularity.ToString(); }));
            }
        }

        public void b_ReceivedDanmaku(MessageModel e)
        {
            lock (_danmakuQueue)
            {
                _danmakuQueue.Enqueue(e);
            }
        }

        private void ProcDanmaku(MessageModel danmakuModel)
        {
            switch (danmakuModel.MsgType)
            {
                case MessageEnum.Chat:
                    if (ChatOpt)
                    {
                        logging(danmakuModel.ChatModel.ToString());
                        Hecheng(danmakuModel.ChatModel.content);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            AddDMText(danmakuModel.ChatModel.user,
                                danmakuModel.ChatModel.content);
                        }));
                    }
                    break;
                case MessageEnum.Gifting:
                    break;
                case MessageEnum.Gift:
                {
                    if (GiftOpt)
                    {
                        logging("收到礼物 : " + danmakuModel.GiftModel.user + " 赠送的 " + danmakuModel.GiftModel.count +
                                " 个 " + danmakuModel.GiftModel.GetName());
                        Hecheng("感谢" + danmakuModel.GiftModel.user + " 赠送的 " + danmakuModel.GiftModel.count +
                                " 个 " + danmakuModel.GiftModel.GetName());
                            Dispatcher.BeginInvoke(new Action(() =>
                        {
                            AddDMText("收到礼物",
                                danmakuModel.GiftModel.ToString(), true);
                        }));
                    }
                    break;
                }
                case MessageEnum.Join:
                {
                    if (GiftOpt)
                    {
                        logging("粉丝团新成员 : 欢迎 " + danmakuModel.UserModel + " 加入了粉丝团");
                        Hecheng("欢迎 " + danmakuModel.UserModel + " 加入了粉丝团");
                            Dispatcher.BeginInvoke(new Action(() =>
                        {
                            AddDMText("粉丝团新成员",
                                "欢迎" + danmakuModel.UserModel + "加入了粉丝团", true);
                        }));
                    }
                    break;
                }
                case MessageEnum.Like:
                {
                    if (LikeOpt)
                    {
                        logging($"用户 {danmakuModel.UserModel} 点了喜欢");
                        AddDMText("点亮",
                            "用户" + danmakuModel.UserModel + "点了喜欢", true);
                    }
                    break;
                }
            }
        }

        public void logging(string text)
        {
            if (Log.Dispatcher.CheckAccess())
                lock (_messageQueue)
                {
                    if (_messageQueue.Count >= _maxCapacity) _messageQueue.RemoveAt(0);

                    _messageQueue.Add("[" + DateTime.Now.ToString("T") + "]" + text);
                }
            else
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => logging(text)));
        }

        public void AddDMText(string notify, string text, bool warn = false)
        {
            if (!overlay_enabled) return;
            if (Dispatcher.CheckAccess())
            {
                var c = new DanmakuTextControl();

                c.UserName.Text = notify;
                if (warn) c.UserName.Foreground = Brushes.Red;
                c.Text.Text = text;
                c.ChangeHeight();
                var sb = (Storyboard) c.Resources["Storyboard1"];
                //Storyboard.SetTarget(sb,c);
                sb.Completed += sb_Completed;
                overlay.LayoutRoot.Children.Add(c);
            }
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDMText(notify, text, warn)));
            }
        }

        public void AddDMText(User user, string text)
        {
            if (!overlay_enabled) return;
            if (Dispatcher.CheckAccess())
            {
                var c = new DanmakuTextControl();

                c.UserName.Text = user.ToString();
                c.Text.Text = text;
                c.ChangeHeight();
                var sb = (Storyboard) c.Resources["Storyboard1"];
                //Storyboard.SetTarget(sb,c);
                sb.Completed += sb_Completed;
                overlay.LayoutRoot.Children.Add(c);
            }
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDMText(user, text)));
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Children[2].Timeline) as DanmakuTextControl;
            if (c != null) overlay.LayoutRoot.Children.Remove(c);
        }

        public void Test_OnClick(object sender, RoutedEventArgs e)
        {
            AddDMText("提示", "這是一個測試😀😭", true);
        }

        private void OnLiveStop()
        {
            logging("提示：主播已下播");
            Disconnbtn_OnClick(this, new RoutedEventArgs());
        }

        private void Disconnbtn_OnClick(object sender, RoutedEventArgs e)
        {
            ConnBtn.IsEnabled = true;
            getDanmakuThread.Abort();
            getDanmakuThread = new Thread(() =>
            {
                while (true)
                    if (b.isLive)
                    {
                        b.GetDanmaku();
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Thread.Sleep(100000);
                    }
            }) {IsBackground = true};
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is TextBlock textBlock)
                {
                    Clipboard.SetText(textBlock.Text);
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() => { MessageBox.Show("本行记录已复制到剪贴板"); }));
                }
            }
            catch (Exception)
            {
            }
        }

        // 合成
        public void Hecheng(string wenzi)
        {
            // 设置APPID/AK/SK
            var APP_ID = "";
            var API_KEY = "";
            var SECRET_KEY = "";

            var client = new Baidu.Aip.Speech.Tts(API_KEY, SECRET_KEY);
            client.Timeout = 60000;  // 修改超时时间
            // 可选参数
            var option = new Dictionary<string, object>()
    {
        {"spd", 5}, // 语速
        {"vol", 1}, // 音量
        {"per", 4}  // 发音人，4：情感度丫丫童声
    };
            if (Danmu1)
            {
                var result = client.Synthesis(wenzi, option);

                if (result.ErrorCode == 0)  // 或 result.Success
                {
                    File.WriteAllBytes("tmp/" + abc + ".mp3", result.Data);
                    Landu("tmp/" + abc + ".mp3");
                    abc++;

                }
            }
        }

        private void Landu(string mp3FilePath) {
            // 需要的头文件


            // 这里是要调用的可执行文件的文件夹目录
            string targetPath = string.Format(System.Environment.CurrentDirectory);

            // Process:提供对本地和远程进程的访问并使你能够启动和停止本地系统进程
            Process process = new Process();

            // 初始化可执行文件的一些基础信息
            process.StartInfo.WorkingDirectory = targetPath; // 初始化可执行文件的文件夹信息
            process.StartInfo.FileName = "cmdmp3win.exe"; // 初始化可执行文件名

            // 当我们需要给可执行文件传入参数时候可以设置这个参数
            // "para1 para2 para3" 参数为字符串形式，每一个参数用空格隔开
            process.StartInfo.Arguments = mp3FilePath;
            process.StartInfo.UseShellExecute = true;        // 使用操作系统shell启动进程

            // 启动可执行文件
            process.Start();
        }

        private string Runcmd(string str) {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(str + "&exit");

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();

            //StreamReader reader = p.StandardOutput;
            //string line=reader.ReadLine();
            //while (!reader.EndOfStream)
            //{
            //    str += line + "  ";
            //    line = reader.ReadLine();
            //}

            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            return output;
        }

        #region Runtime settings

        private readonly bool overlay_enabled = true;

        #endregion

        private void ShowChat_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ChatOpt = false;
        }

        private void showPresent_OnUnchecked(object sender, RoutedEventArgs e)
        {
            GiftOpt = false;
        }

        private void showPresent_OnChecked(object sender, RoutedEventArgs e)
        {
            GiftOpt = true;
        }

        private void showChat_OnChecked(object sender, RoutedEventArgs e)
        {
            ChatOpt = true;
        }

        private void showBrand_OnChecked(object sender, RoutedEventArgs e)
        {
            User.showBrand = true;
        }

        private void showBrand_OnUnchecked(object sender, RoutedEventArgs e)
        {
            User.showBrand = false;
        }

        private void ShowLike_OnChecked(object sender, RoutedEventArgs e)
        {
            LikeOpt = true;
        }

        private void ShowLike_OnUnchecked(object sender, RoutedEventArgs e)
        {
            LikeOpt = false;
        }
        private void Danmu_OnChecked(object sender, RoutedEventArgs e)
        {
            Danmu1 = true;
        }
        private void Danmu_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Danmu1 = false;
        }
    }
}
