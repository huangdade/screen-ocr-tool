using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Data;
using System.Collections.Specialized;
using System.Windows.Navigation;
using Path = System.IO.Path;

namespace 屏幕识文
{
    /// <summary>
    /// ResultWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ResultWindow : Window
    {

        private string ImageSavePath;

        public ResultWindow(Bitmap bitmap, string result)
        {
            InitializeComponent();
            //把结果传到文本编辑框
            ResultBox.Text = result;
            //把图片的数据源传入编辑框
            CutedImage.Source = Utils.Bitmap2ImageSource(bitmap);
            //这三行代码的作用是在本地的安装包的目录下进行创建文件
            string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string SoftwareName = "屏幕识文";
            string LocalAppData = Path.Combine(LocalApplicationData, SoftwareName);

            string ImageDir = Path.Combine(LocalAppData, "history", "imgs");

            // 如Class1.cs果图片文件夹不存在，则创建之
            if (!Directory.Exists(ImageDir))
            {
                Directory.CreateDirectory(ImageDir);
            }
            // 保存图片
            ImageSavePath = Path.Combine(ImageDir, DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");
            bitmap.Save(ImageSavePath);

            var rr = new RecognizeRecord();//定义一个变量来接收RecognizeRecord类传过来的值
            rr.Image = ImageSavePath;//把传送过来的值赋给图片
            rr.ResultText = result;//把文本的值传递到文本框

            RecognizeHistory.Instance.Add(rr);

        }
        //辅助方法
        public ResultWindow(RecognizeRecord rr)
        {
            InitializeComponent();

            ImageSavePath = rr.Image;
            CutedImage.Source = new BitmapImage(new Uri(rr.Image, UriKind.Absolute));//图片的绝对路径
            ResultBox.Text = rr.ResultText;
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

        //公共类调用，弹出框
        private void Popup()
        {
            CopySucceedPopup.IsOpen = true;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1500;//时钟的秒速是一秒半
            timer.AutoReset = false;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(HidePopup));
        }

        private void HidePopup()
        {
            CopySucceedPopup.IsOpen = false;
        }

        //复制文字
        private void Copwords_Click(object sender, RoutedEventArgs e)
        {
            string text = ResultBox.Text.Replace("\n", "\r\n");//\n转换\r\n转换字符
            try
            {
                System.Windows.Clipboard.SetDataObject(text, true);// 复制文字的内容Text
                popupMsg.Text = "复制成功";
            }
            catch
            {
                popupMsg.Text = "复制失败";
            }
            //System.Windows.Clipboard.SetText(text);
            Popup();
        }

        // 复制图片
        private void Copypicture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringCollection myCol = new StringCollection();
                myCol.Add(ImageSavePath);
                System.Windows.Clipboard.SetFileDropList(myCol);//绝对路径
                popupMsg.Text = "复制成功";
            }
            catch
            {
                popupMsg.Text = "复制失败";
            }
            Popup();
        }

        // 下载图片
        private void Copypicturedownload_Click(object sender, RoutedEventArgs e)
        {
            //saveFileDialog提供的内库进行文件的操作
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "图片文件|*.png";
            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                File.Copy(ImageSavePath, saveFileDialog.FileName,true);//文件复制一下，再放到用户保存的地方
                popupMsg.Text = "复制成功";
                Popup();
            }
        }              
       

        //二维码
        private void QRcode_Click(object sender, RoutedEventArgs e)
        {
            QRcodeWindow qRcodeWindow = new QRcodeWindow(ResultBox.Text);
            qRcodeWindow.Show();
        }
    }
}

