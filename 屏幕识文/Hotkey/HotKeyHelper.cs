using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace 屏幕识文
{
    /// <summary>
    /// 热键注册帮助
    /// </summary>
    public class HotKeyHelper
    {
        /// <summary>
        /// 记录快捷键注册项的唯一标识符
        /// </summary>
        private static Dictionary<string, int> m_HotKeySettingsDic = new Dictionary<string, int>();

        public static int GetHotKeySid(HotKeyModel hotKey)
        {
            return m_HotKeySettingsDic[hotKey.Name];
        }
        
        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hotKeyModel">热键待注册项</param>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool RegisterHotKey(HotKeyModel hotKeyModel, IntPtr hWnd)
        {
            var fsModifierKey = new ModifierKeys();

            if (!m_HotKeySettingsDic.ContainsKey(hotKeyModel.Name))
            {
                // 全局原子不会在应用程序终止时自动删除。每次调用GlobalAddAtom函数，必须相应的调用GlobalDeleteAtom函数删除原子。
                if (HotKeyManager.GlobalFindAtom(hotKeyModel.Name) != 0)
                {
                    HotKeyManager.GlobalDeleteAtom(HotKeyManager.GlobalFindAtom(hotKeyModel.Name));
                }
                // 获取唯一标识符
                m_HotKeySettingsDic[hotKeyModel.Name] = HotKeyManager.GlobalAddAtom(hotKeyModel.Name);
            }
            else
            {
                // 注销旧的热键
                // 注意，只有我们自己注册的全局快捷键我们才会注销它，而不能轻易地更改其他程序的快捷键。
                // 所以，只有我们自己注册过的名称所对应的快捷键我们才会取消它。
                HotKeyManager.UnregisterHotKey(hWnd, m_HotKeySettingsDic[hotKeyModel.Name]);
            }
            if (!hotKeyModel.IsUsable)
            {
                return true;
            }

            // 注册热键
            if (hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control;
            }
            else if (!hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Shift;
            }
            else if (!hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Alt;
            }
            else if (hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Shift;
            }
            else if (hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Alt;
            }
            else if (!hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Shift | ModifierKeys.Alt;
            }
            else if (hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt;
            }

            // 如果某个快捷键已经注册过了，该函数会返回否。我们需要根据这个决定是否调整快捷键。
            return HotKeyManager.RegisterHotKey(hWnd, m_HotKeySettingsDic[hotKeyModel.Name], fsModifierKey, (int)hotKeyModel.SelectKey);
        }

    }
}
