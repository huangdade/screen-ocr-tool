using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace 屏幕识文
{
    public  class common
    {
        private string ImageSavePath;

        public object CopySucceedPopup { get; private set; }

        public void DirectoryStore(Bitmap bitmap, string result)
        {
            //这三行代码的作用是在本地的安装包的目录下进行创建文件
            string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string SoftwareName = "屏幕识文";
            string LocalAppData = Path.Combine(LocalApplicationData, SoftwareName);

            string ImageDir = Path.Combine(LocalAppData, "history");

            // 如Class1.cs果图片文件夹不存在，则创建之
            if (!Directory.Exists(ImageDir))
            {
                Directory.CreateDirectory(ImageDir);
            }
            // 保存图片
            ImageSavePath = Path.Combine(ImageDir, DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");
            bitmap.Save(ImageSavePath);

            var rr = new RecognizeRecord();
            rr.Image = ImageSavePath;
            rr.ResultText = result;

            RecognizeHistory.Instance.Add(rr);
        }

    }
}
