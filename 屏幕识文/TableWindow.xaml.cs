using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
namespace 屏幕识文
{
    /// <summary>
    /// TableWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TableWindow : Window
    {
        private string ImageSavePath;
        private string str;
        public TableWindow(Bitmap bitmap, string path)
        {
            InitializeComponent();
            str = path;
            
            //把图片的数据源传入编辑框
            CutedTableImage.Source = Utils.Bitmap2ImageSource(bitmap);

            //这三行代码的作用是在本地的安装包的目录下进行创建文件
            string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string SoftwareName = "屏幕识文";
            string LocalAppData = Path.Combine(LocalApplicationData, SoftwareName);

            //在本地创建一个history文件夹
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
            
            RecognizeHistory.Instance.Add(rr);
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

        // 下载图片
        private void Copypicturedownload_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "图片文件|*.png";
            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                File.Copy(ImageSavePath, saveFileDialog.FileName, true);//文件复制一下，再放到用户保存的地方
                Popup();
            }
        }

        //打开表格
        private void Opentable_Click(object sender, RoutedEventArgs e)
        {
            
                System.Diagnostics.Process.Start(str);//打开exal表格
                Popup();
        }

        //另存为表格
        private void Tabledownload_Click(object sender, RoutedEventArgs e)
        {                       
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog(); //打开另存为的对话框
            saveFileDialog.Filter = "表格文件|*.xls";
            var result1 = saveFileDialog.ShowDialog();
            if (result1 == true)
            {
                
                File.Copy(str, saveFileDialog.FileName, true);//文件复制一下，再放到用户保存的地方
                Popup();
            }            
        }

        #region //公共类调用，弹出框
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


        #endregion


    }
}
