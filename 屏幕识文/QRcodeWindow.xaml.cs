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
using ZXing;

namespace 屏幕识文
{
    /// <summary>
    /// QRcodeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class QRcodeWindow : Window
    {
        private string ImageSavePath;
        Bitmap bitmap;
        public QRcodeWindow( string result)
        {
            InitializeComponent();
            try
            {
                bitmap = GetQRCodeByZXingNet(result, 450, 450);//将生成的二维码图片存到一个值里面
                                                               // 保存图片
                string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string SoftwareName = "屏幕识文";
                string LocalAppData = System.IO.Path.Combine(LocalApplicationData, SoftwareName);

                // 如Class1.cs果图片文件夹不存在，则创建之
                if (!Directory.Exists(LocalAppData))
                {
                    Directory.CreateDirectory(LocalAppData);
                }
                ImageSavePath = System.IO.Path.Combine(LocalAppData, "二维码.png");//将真实的图片路径传到ImageSavePath
                bitmap.Save(ImageSavePath);
                QRcodeimg.Source = Utils.Bitmap2ImageSource(bitmap);
            }
            catch
            {
                popupMsg.Text = "生成失败";
                Popup();
            }
        }
        public QRcodeWindow(RecognizeRecord rr)
        {
            InitializeComponent();

            ImageSavePath = rr.Image;
            QRcodeimg.Source = new BitmapImage(new Uri(rr.Image, UriKind.Absolute));//图片的绝对路径
            //result.Text = rr.ResultText;
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

        //复制
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StringCollection myCol = new StringCollection();
            myCol.Add(ImageSavePath);
            System.Windows.Clipboard.SetFileDropList(myCol);//绝对路径

            popupMsg.Text = "复制成功";
            Popup();
        }
        //另存为
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //saveFileDialog提供的内库进行文件的操作
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "图片文件|*.png";
            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                File.Copy(ImageSavePath, saveFileDialog.FileName,true);//文件复制一下，再放到用户保存的地方

                popupMsg.Text = "保存成功";
                Popup();
            }
        }
        //生成二维码
        private Bitmap GetQRCodeByZXingNet(String strMessage, Int32 width, Int32 height)
        {
            Bitmap result = null;
            BarcodeWriter barCodeWriter = new BarcodeWriter();
            barCodeWriter.Format = BarcodeFormat.QR_CODE;
            barCodeWriter.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            barCodeWriter.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.H);
            barCodeWriter.Options.Height = height;
            barCodeWriter.Options.Width = width;
            barCodeWriter.Options.Margin = 0;
            ZXing.Common.BitMatrix bm = barCodeWriter.Encode(strMessage);
            result = barCodeWriter.Write(bm);
            //string filename = @"C:\Users\test.png";
            //result .Save(filename, ImageFormat.Png);
            //result .Dispose();
            return result;
        }
        #region 弹出框
        private void Popup()
        {
            CopySucceedPopup.IsOpen = true;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1500;
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
