using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 屏幕识文
{
    public class HotKeys
    {
        private static HotKeyModel mSnapshot = null;
        public static HotKeyModel Snapshot
        {
            get
            {
                if (mSnapshot == null)
                {
                    mSnapshot = HotKeyModel.Parse(Properties.Settings.Default.快捷键);
                }
                return mSnapshot;
            }
        }
    }
}
