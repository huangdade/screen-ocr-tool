using System;
using System.Collections.Generic;

namespace 屏幕识文
{
    /// <summary>
    /// 快捷键模型
    /// </summary>
    public class HotKeyModel
    {
        /// <summary>
        /// 设置项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设置项快捷键是否可用
        /// </summary>
        public bool IsUsable { get; set; }

        /// <summary>
        /// 是否勾选Ctrl按键
        /// </summary>
        public bool IsSelectCtrl { get; set; }

        /// <summary>
        /// 是否勾选Shift按键
        /// </summary>
        public bool IsSelectShift { get; set; }

        /// <summary>
        /// 是否勾选Alt按键
        /// </summary>
        public bool IsSelectAlt { get; set; }

        /// <summary>
        /// 选中的按键
        /// </summary>
        public EKey SelectKey { get; set; }
        
        /// <summary>
        /// 快捷键按键集合
        /// </summary>
        public static Array Keys
        {
            get
            {
                return Enum.GetValues(typeof(EKey));
            }
        }

        public static HotKeyModel Parse(string str)
        {
            var words = str.Split(',');
            var name = words[0];
            var isUsable = bool.Parse(words[1]);
            var isSelectCtrl = bool.Parse(words[2]);
            var isSelectShift = bool.Parse(words[3]);
            var isSelectAlt = bool.Parse(words[4]);
            var key = (EKey)Enum.Parse(typeof(EKey), words[5]);
            return new HotKeyModel
            {
                Name = name,
                IsUsable = isUsable,
                IsSelectCtrl = isSelectCtrl,
                IsSelectShift = isSelectShift,
                IsSelectAlt = isSelectAlt,
                SelectKey = key
            };
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5}",
                Name,
                IsUsable,
                IsSelectCtrl,
                IsSelectShift,
                IsSelectAlt,
                SelectKey);
        }

        public string GetDispString()
        {
            var keys = new List<string>();
            if (IsSelectCtrl)
            {
                keys.Add("Ctrl");
            }
            if (IsSelectShift)
            {
                keys.Add("Shift");
            }
            if (IsSelectAlt)
            {
                keys.Add("Alt");
            }
            keys.Add(SelectKey.ToString());
            return string.Join("+", keys.ToArray());
        }

        public void Save()
        {
            Properties.Settings.Default.快捷键 = ToString();
            Properties.Settings.Default.Save();
        }
    }
}
