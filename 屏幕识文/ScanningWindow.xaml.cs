using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WIA;

namespace 屏幕识文
{
    /// <summary>
    /// ScanningWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScanningWindow : Window
    {
        //private string 保存路径;

        public ScanningWindow(ImageFile imageFile)
        {
            InitializeComponent();
            //定义一个string变量（本地）
            string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string SoftwareName = "屏幕识文";
            string LocalAppData =  Path.Combine(LocalApplicationData, SoftwareName);

            // 如果图片文件夹不存在，则创建之
            if (!Directory.Exists(LocalAppData))
            {
                Directory.CreateDirectory(LocalAppData);
            }
            
            var 保存路径 = Path.Combine(LocalAppData, "扫描.png");
            //判断文件是否存在，如果存在就删除          
            if (File.Exists(保存路径))
            {                
                File.Delete(保存路径);                
            }
            // 把扫描到的图片保存为一个文件
            imageFile.SaveFile(保存路径);
            ScanningImg.Source = new BitmapImage(new Uri(保存路径, UriKind.Absolute));
        }

        //最小化
        private void Minimized(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        //最大化
        private void Maximized(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                this.maximize.Source = new BitmapImage(new Uri("pack://application:,,,/img/maximize.png"));
            }
            else
            {
                WindowState = WindowState.Maximized;
                this.maximize.Source = new BitmapImage(new Uri("pack://application:,,,/img/normal.png"));
            }
        }
        //关闭
        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //窗口移动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        //截取识别
        private void Button_Click(object sender, RoutedEventArgs e )
        {
            开始识别();
        }

        public void 开始识别()
        {
            FullScreenWindow SnapshotWindow = new FullScreenWindow();
            SnapshotWindow.Show();
            SnapshotWindow.Activate();
        }
        //图片识别
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
