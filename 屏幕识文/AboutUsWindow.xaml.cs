using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace 屏幕识文
{
    /// <summary>
    /// AboutUsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutUsWindow : Window
    {
        public AboutUsWindow()
        {
            InitializeComponent();
            //VisitWebsite = new RelayCommand(() =>
            //{
            //    Process.Start(new ProcessStartInfo(OurWebsite.NavigateUri.AbsoluteUri));
            //});


            //VisitProjectPage = new RelayCommand(() =>
            //{
            //    Process.Start(new ProcessStartInfo(ProjectPage.NavigateUri.AbsoluteUri));
            //});

            DataContext = this;
        }

        //public const int current_version_code = 2;//定义版本号
        //CheckForUpdate = new RelayCommand(() =>
        // {
        // var currVersionFile = new Uri("pack://application:,,,/version.json");
        // var stream = Application.GetResourceStream(currVersionFile).Stream;
        // var str = new StreamReader(stream).ReadToEnd();
        // var obj = (JObject)JsonConvert.DeserializeObject(str);
        // var currVersion = (string)obj["current-version"];

        // var url = "http://192.168.0.107:8231/api/version";


        //    WebClient webClient = new WebClient();
        //    var tempFile = Path.GetTempFileName();
        //    webClient.DownloadFile(url, tempFile);
        //    var latestVersionObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(tempFile));
        //    var latestVersion = (string)latestVersionObj["latest_version"];

        //    if (currVersion == latestVersion)
        //    {
        //        MessageBox.Show("已经是最新版本。");
        //    }
        //    else
        //    {
        //        CheckVersionButton.Visibility = Visibility.Hidden;
        //        VersionHolder.Children.Remove(CheckVersionButton);
        //        var downloadLink = (string)latestVersionObj["update_link"];
        //        NewVersion.NavigateUri = new Uri(downloadLink);
        //        NewVersion.Inlines.Clear();
        //        NewVersion.Inlines.Add("已有新版本：" + latestVersion);
        //        NewVersionBox.Visibility = Visibility.Visible;
        //    }

        //});
        //    DownloadNewVersion = new RelayCommand(() =>
        //    {
        //    var downloadLink = NewVersion.NavigateUri.AbsoluteUri;
        //    Process.Start(new ProcessStartInfo(downloadLink));
        //});


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

        //点击检查更新
        private void CheckVersionButton_Click(object sender, RoutedEventArgs e)
         {
            //Version latestVersion = Version.LoadFromServer();

            //if (Update.current_version_code >= latestVersion.code)
            //{
            //    MessageBox.Show("已经是最新版本。");
            //}
            //else
            //{
            //    CheckVersionButton.Visibility = Visibility.Hidden;
            //    VersionHolder.Children.Remove(CheckVersionButton);
            //    NewVersion.NavigateUri = new Uri(latestVersion.downloadUrl);
            //    NewVersion.Inlines.Clear();
            //    NewVersion.Inlines.Add("已有新版本：" + latestVersion.downloadUrl);
            //    NewVersionBox.Visibility = Visibility.Visible;
            //}
        }

        //public ICommand CheckForUpdate { get; set; }
        public ICommand VisitWebsite { get; set; }
        //    public ICommand DownloadNewVersion { get; set; }
        public ICommand VisitProjectPage { get; set; }



    }
}
