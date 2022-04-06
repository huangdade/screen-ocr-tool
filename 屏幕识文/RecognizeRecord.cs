using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 屏幕识文
{
    public class RecognizeRecord
    {
        //用于储存历史记录数据的变量
        public long ID { get; set; }
        public string Image { get; set; }
        public string ResultText { get; set; }
    }
}
