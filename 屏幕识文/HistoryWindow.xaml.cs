using System;
using System.Collections.Generic;
using System.Data;
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
using System.IO;
using System.Drawing;
using System.Collections.ObjectModel;

namespace 屏幕识文
{
    /// <summary>
    /// HistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private ObservableCollection<RecognizeRecord> historyList;
        public HistoryWindow()
        {
            InitializeComponent();
            historyList = RecognizeHistory.Instance.Read(); // 利用单例实列属性进行调用
            History.ItemsSource = historyList;
            History.PreviewMouseWheel += History_PreviewMouseWheel;
        }

        private void History_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);

            eventArg.RoutedEvent = MouseWheelEvent;

            eventArg.Source = sender;

            History.RaiseEvent(eventArg);
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

        //查看
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            new ResultWindow(btn.Tag as RecognizeRecord).Show();
        }
        
        //删除
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var rr = btn.Tag as RecognizeRecord;
            RecognizeHistory.Instance.Delete(rr);
            historyList.Remove(rr);
        }
    }
}
