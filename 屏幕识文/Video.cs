using System;
using System.Runtime.InteropServices;
using System.Drawing.Imaging; 
using System.Windows.Forms;
using System.Drawing;
namespace 屏幕识文
{
    /// <summary>
    /// Class1 的摘要说明。
    /// </summary>
    public class Video
    {
        //5个传入参数
        private IntPtr myHand;
        private int myWidth;
        private short myHeight;
        private int myLeft;
        private int myTop;
        //调用avicap32.dll 
        public struct videohdr_tag
        {
            public byte[] lpData;
            public int dwBufferLength;
            public int dwBytesUsed;
            public int dwTimeCaptured;
            public int dwUser;
            public int dwFlags;
            public int[] dwReserved;

        }
        [DllImport("avicap32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int capCreateCaptureWindowA([MarshalAs(UnmanagedType.VBByRefStr)]   ref string lpszWindowName, int dwStyle, int x, int y, int nWidth, short nHeight, int hWndParent, int nID);
        [DllImport("avicap32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool capGetDriverDescriptionA(short wDriver, [MarshalAs(UnmanagedType.VBByRefStr)]   ref string lpszName, int cbName, [MarshalAs(UnmanagedType.VBByRefStr)]   ref string lpszVer, int cbVer);
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool DestroyWindow(int hndw);
        [DllImport("user32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SendMessage(int hwnd, int wMsg, int wParam, [MarshalAs(UnmanagedType.AsAny)]   object lParam);
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SetWindowPos(int hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        [DllImport("vfw32.dll")]
        public static extern string capVideoStreamCallback(int hwnd, videohdr_tag videohdr_tag);
        [DllImport("vicap32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool capSetCallbackOnFrame(int hwnd, string s);
        //自定义参数：
        private int hHwnd;
        //构造函数
        public Video(IntPtr myPtr, int left, int top, int width, short height)
        {
            myHand = myPtr;
            myLeft = left;
            myTop = top;
            myWidth = width;
            myHeight = height;
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        //打开视频：
        public void opVideo()
        {
            int intDevice = 0;
            string refDevice = intDevice.ToString();
            hHwnd = capCreateCaptureWindowA(ref refDevice, 1342177280, 0, 0, 640, 480, myHand.ToInt32(), 0);
            if (SendMessage(hHwnd, 0x40a, intDevice, 0) > 0)
            {
                SendMessage(this.hHwnd, 0x435, -1, 0);
                SendMessage(this.hHwnd, 0x434, 0x42, 0);
                SendMessage(this.hHwnd, 0x432, -1, 0);
                SetWindowPos(this.hHwnd, 1, 0, 0, myWidth, Convert.ToInt32(myHeight), 6);
            }
            else
            {
                DestroyWindow(this.hHwnd);
            }
        }
        //停止视频
        public void CloVideo()
        {
            SendMessage(this.hHwnd, 0x40b, 0, 0);
            DestroyWindow(this.hHwnd);
        }
        //捕获视频
        public Image CatchVideo()
        {
            SendMessage(this.hHwnd, 0x41e, 0, 0);
            IDataObject obj1 = Clipboard.GetDataObject();
            Image getIma = null;
            if (obj1.GetDataPresent(typeof(Bitmap)))
            {
                Image image1 = (Image)obj1.GetData(typeof(Bitmap));
                getIma = image1;
            }
            return getIma;
        }
    }
}