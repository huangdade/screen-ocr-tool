using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFMediaKit.DirectShow.Controls;


namespace 屏幕识文
{
    /// <summary>
    /// CameraWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CameraWindow : Window
    {
        //private string fullPath;

        public bool HasCameras => MultimediaUtil.VideoInputNames.Length > 0;

        public CameraWindow()
        {
            InitializeComponent();

            cmb_camera.ItemsSource = MultimediaUtil.VideoInputNames;
            if (HasCameras)
            {
                cmb_camera.SelectedIndex = 0; // 第0个摄像头为默认摄像头
            }
        }

        private void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vce.VideoCaptureSource = (string)cmb_camera.SelectedItem;            
        }


        //private void btnCapture_Click(object sender, RoutedEventArgs e)
        //{
        //    // 建立目标渲染图像器，高度为前台控件实显高度，此处不能使用.width或.height属性，否则出现错误。
        //    // 为了避免图像抓取出现黑边现象，需要对图象进行重新测量及缩放
        //    RenderTargetBitmap bitmap = new RenderTargetBitmap((int)vce.ActualWidth, (int)vce.ActualHeight, 96, 96, PixelFormats.Default);
        //    //VideoCaptureElement的Stretch="Fill"
        //    vce.Measure(vce.RenderSize);
        //    vce.Arrange(new Rect(vce.RenderSize));
        //    // 指定图像渲染目标
        //    bitmap.Render(vce);
        //    // 建立图像解码器。类型为jpeg
        //    BitmapEncoder encoder = new JpegBitmapEncoder();
        //    // 将当前渲染器中渲染位图作为一个位图帧加入解码器，进行解码，取得数据流。
        //    encoder.Frames.Add(BitmapFrame.Create(bitmap));
        //    // 建立内存流，将得到解码图像流写入内存流。
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        encoder.Save(stream);
        //        byte[] pics = stream.ToArray(); // 将流以文件形式存储于计算机中。
        //        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        //        fullPath = System.IO.Path.Combine(@"E:\Photo", fileName + "_cap.jpg");
        //        //保存图片
        //        File.WriteAllBytes(fullPath, pics);
        //    }   
        //    // 预览效果暂停。
        //    vce.Pause();
        //}

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

        //点击识别
        private void btnanew_Click(object sender, RoutedEventArgs e)
        {
            开始识别();
        }
        public void 开始识别()
        {
            FullScreenWindow SnapshotWindow = new FullScreenWindow();
            SnapshotWindow.Show();
            SnapshotWindow.Activate();
        }
    }
}
