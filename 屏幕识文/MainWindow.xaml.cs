using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WIA;
using Forms = System.Windows.Forms;

namespace 屏幕识文
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            CreateNotifyIcon();

            DataContext = this;
            Closed += MainWindow_Closed;
            Closing += MainWindow_Closing;
            
        }


#region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#endregion

#region 用户设置 - 快捷键
        public bool UseHotKey
        {
            get => HotKeys.Snapshot.IsUsable;
            set
            {
                HotKeys.Snapshot.IsUsable = value;
                HotKeys.Snapshot.Save();
                RaisePropertyChanged(nameof(UseHotKey));
            }
        }
        public bool UseCtrl
        {
            get => HotKeys.Snapshot.IsSelectCtrl;
            set
            {
                HotKeys.Snapshot.IsSelectCtrl = value;
                HotKeys.Snapshot.Save();
            }
        }
        public bool UseShift
        {
            get => HotKeys.Snapshot.IsSelectShift;
            set
            {
                HotKeys.Snapshot.IsSelectShift = value;
                HotKeys.Snapshot.Save();
            }
        }
        public bool UseAlt
        {
            get => HotKeys.Snapshot.IsSelectAlt;
            set
            {
                HotKeys.Snapshot.IsSelectAlt = value;
                HotKeys.Snapshot.Save();
            }
        }
        public EKey UsedKey
        {
            get => HotKeys.Snapshot.SelectKey;
            set
            {
                HotKeys.Snapshot.SelectKey = value;
                HotKeys.Snapshot.Save();
            }
        }
        public IEnumerable UsableKeys
        {
            get => Enum.GetValues(typeof(EKey));
        }
#endregion

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            HideNotifyIcon();
        }



#region 快捷键注册与处理
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // 获取窗体句柄
            // 必须在此处获取窗口的句柄。如果在窗口处创建的时候去获取，我们会得到 null。我已经测试过了。那个时候窗口可能尚未准备好，无法获取句柄。
            // 这一点至关重要。否则，快捷键注册成功，但是我们的窗口永远无法捕获。
            var m_Hwnd = new WindowInteropHelper(this).Handle;
            RegisterHotKey(m_Hwnd); // 注册快捷键
            HwndSource hWndSource = HwndSource.FromHwnd(m_Hwnd);
            // 添加处理程序
            if (hWndSource != null)
            {
                hWndSource.AddHook(WndProc);
            }
        }

        /*
         * 对于不启用快捷键的情况，这里的代码不需要修改。原因是：
         * 1. HotKeyHelper.RegisterHotKey 方法在注册快捷键时，如果快捷键不可以，直接返回 true，表示注册成功，但实际上根本没有注册；
         * 2. 用户即使如果按下快捷键，由于我们根本没有注册，所以根本不会收到消息，也就不必进行处理了。
         */

        // 快捷键处理
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case HotKeyManager.WM_HOTKEY:
                    int sid = wParam.ToInt32();
                    if (sid == HotKeyHelper.GetHotKeySid(HotKeys.Snapshot))
                    {
                        TriggerSnapshot();
                    }
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        private void RegisterHotKey(IntPtr m_Hwnd)
        {
            bool result = HotKeyHelper.RegisterHotKey(HotKeys.Snapshot, m_Hwnd);
            if (!result)
            {
                MessageBox.Show("快捷键注册失败！使用时请手动点击右下角图标操作。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
#endregion

#region 右下角小图标
        private Forms.NotifyIcon NotifyIcon = null;
        // 程序启动时创建图标
        private void CreateNotifyIcon()
        {
            Forms.ToolStripItem setting = new Forms.ToolStripMenuItem();
            setting.Name = "设置";
            setting.Text = "设置";
            // setting.Font = new Font(setting.Font, System.Drawing.FontStyle.Bold);
            var settingImgFile = new Uri("pack://application:,,,/Resources/Images/设置.png");
            setting.Image = System.Drawing.Image.FromStream(Application.GetResourceStream(settingImgFile).Stream);
            setting.Click += Setting_Click;

            Forms.ToolStripItem exit = new Forms.ToolStripMenuItem();
            exit.Name = "退出";
            exit.Text = "退出";
            var exitImgFile = new Uri("pack://application:,,,/Resources/Images/退出.png");
            exit.Image = System.Drawing.Image.FromStream(Application.GetResourceStream(exitImgFile).Stream);
            exit.Click += Exit_Click;

            Forms.ToolStripItem hotkey = new Forms.ToolStripMenuItem();
            hotkey.Name = "快捷键";
            hotkey.Text = "快捷键";
            // hotkey.Font = new Font(hotkey.Font, System.Drawing.FontStyle.Bold);
            var hotkeyImgFile = new Uri("pack://application:,,,/Resources/Images/快捷键.png");
            hotkey.Image = System.Drawing.Image.FromStream(Application.GetResourceStream(hotkeyImgFile).Stream);
            hotkey.Click += Hotkey_Click;

            Forms.ToolStripItem Camera = new Forms.ToolStripMenuItem();
            //var hotkeyImgFile1 = new Uri("pack://application:,,,/Resources/Images/快捷键.png");
            var CameraImgFile = new Uri("pack://application:,,,/Resources/Images/摄像头.png");
            Camera.Image = System.Drawing.Image.FromStream(Application.GetResourceStream(CameraImgFile).Stream);
            Camera.Name = "打开摄像头";
            Camera.Text = "打开摄像头";
            Camera.Click += Camera_Click;

            Forms.ToolStripItem Scanning = new Forms.ToolStripMenuItem();
            Scanning.Name = "打开扫描仪";
            Scanning.Text = "打开扫描仪";
            var ScanningImgFile = new Uri("pack://application:,,,/Resources/Images/扫描仪.png");
            Scanning.Image = System.Drawing.Image.FromStream(Application.GetResourceStream(ScanningImgFile).Stream);
            Scanning.Click += Scanning_Click;

            Forms.ToolStripItem History = new Forms.ToolStripMenuItem();
            History.Name = "历史记录";
            History.Text = "历史记录";
            var HistoryImgFile = new Uri("pack://application:,,,/Resources/Images/历史.png");
            History.Image = System.Drawing.Image.FromStream(Application.GetResourceStream(HistoryImgFile).Stream);
            History.Click += History_Click;

            Forms.ToolStripItem aboutus = new Forms.ToolStripMenuItem();
            aboutus.Name = "关于";
            aboutus.Text = "关于";
            var aboutusImgFile = new Uri("pack://application:,,,/Resources/Images/关于.png");
            aboutus.Image = System.Drawing.Image.FromStream(Application.GetResourceStream(aboutusImgFile).Stream);
            aboutus.Click += Aboutus_Click;

            /*
            Forms.ToolStripItem open = new Forms.ToolStripMenuItem();
            open.Name = "从图片、视频或摄像头……";
            open.Text = "从图片、视频或摄像头……";
            open.Click += Open_Click;
            */


            var NotifyIconMenu = new Forms.ContextMenuStrip();
            NotifyIconMenu.DefaultDropDownDirection = Forms.ToolStripDropDownDirection.AboveRight;
            // NotifyIconMenu.Items.Add(open);
            NotifyIconMenu.Items.Add(hotkey);
            NotifyIconMenu.Items.Add(setting);
            NotifyIconMenu.Items.Add(aboutus);
            NotifyIconMenu.Items.Add(Camera);
            NotifyIconMenu.Items.Add(Scanning);
            NotifyIconMenu.Items.Add(History);
            NotifyIconMenu.Items.Add(exit);

            var iconFile = new Uri("pack://application:,,,/Resources/Images/ocr_16.ico");
            NotifyIcon = new Forms.NotifyIcon
            {
                Icon = new Icon(Application.GetResourceStream(iconFile).Stream),
                Text = "屏幕识文",
                ContextMenuStrip = NotifyIconMenu
            };
            NotifyIcon.MouseClick += NotifyIcon_MouseClick;
            // NotifyIcon.Click += NotifyIcon_Click; // += NotifyIcon_MouseDoubleClick;
            NotifyIcon.Visible = true;
        }

        // 单击触发识别操作
        private void NotifyIcon_MouseClick(object sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                TriggerSnapshot();
            }
        }

        private void Aboutus_Click(object sender, EventArgs e)
        {
            var aboutus = new AboutUsWindow();
            aboutus.ShowDialog();
        }

        private void Hotkey_Click(object sender, EventArgs e)
        {
            var disp = HotKeys.Snapshot.IsUsable ? "快捷键为：" + HotKeys.Snapshot.GetDispString() : "未启用快捷键。可以在“设置”页面进行设置。";
            MessageBox.Show(disp, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 打开摄像头
        private void Camera_Click(object sender, EventArgs e)
        {
            /*
            Video v = new Video(IntPtr.Zero, 0, 0, 200, 200);
            v.opVideo();
            return;
            */
            CameraWindow cameraWindow = new CameraWindow();
            if (cameraWindow.HasCameras)
            {
                cameraWindow.Show();
            }
            else
            {
                MessageBox.Show("电脑没有安装任何可用摄像头");
            }
        }

        //一键扫描
        private void Scanning_Click(object sender, EventArgs e)
        {

            try
            {
                ImageFile imgs = Scan();//先定义一个变量进行接收扫描传来的值
                ScanningWindow scanningWindow = new ScanningWindow(imgs);
                scanningWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        //一键扫描的方法
        public static ImageFile Scan()
        {
            ImageFile image;

            try
            {
                WIA.CommonDialog dialog = new WIA.CommonDialog();

                image = dialog.ShowAcquireImage(
                        WiaDeviceType.ScannerDeviceType,
                        WiaImageIntent.ColorIntent,
                        WiaImageBias.MaximizeQuality);

                return image;
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == -2145320939)
                {
                    throw new Exception("找不到扫描仪");
                }
                else
                {
                    throw new Exception("COM错误");
                }
            }
        }

        // 浏览历史记录
        private void History_Click(object sender, EventArgs e)
        {
            HistoryWindow historyWindow = new HistoryWindow();
            historyWindow.Show();
        }

        // 程序退出时隐藏图标
        private void HideNotifyIcon()
        {
            if (NotifyIcon != null)
            {
                NotifyIcon.Visible = false;
            }
        }

#region 小图标菜单
        private void Setting_Click(object sender, EventArgs e)
        {
            // 本窗口就是设置窗口。也就是说把主窗口充作设置窗口，发挥一些作用。
            stPUseHotKey.Visibility = Visibility.Visible;
            Show();
        }

        private bool ExitApplication = false;
        private void Exit_Click(object sender, EventArgs e)
        {
            // 更干净的退出方式，避免进程残留。
            Application.Current.Shutdown();
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!ExitApplication)
            {
                e.Cancel = true;
                Hide();
                // 判断点击右上角×以后，设置页面显示出来
                if (stPOk.Visibility == Visibility.Visible)
                {
                    Button_Click(null, null);
                }
            }
        }
#endregion
#endregion

#region 启动截屏识文窗口
        private FullScreenWindow SnapshotWindow = null;
        private void TriggerSnapshot()
        {
            // 保证只有一个截屏窗口被启动
            if (SnapshotWindow != null)
            {
                return;
            }
            SnapshotWindow = new FullScreenWindow();
            SnapshotWindow.Show();
            SnapshotWindow.Activate();
            SnapshotWindow.Closed += SnapshotWindow_Closed;
        }

        private void SnapshotWindow_Closed(object sender, EventArgs e)
        {
            SnapshotWindow = null;
        }

#endregion
        //关闭按钮
        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //点击确定按钮
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            stPOk.Visibility = Visibility.Hidden;
            stPUseHotKey.Visibility = Visibility.Visible;
            Close();
        }

        //窗口移动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}
