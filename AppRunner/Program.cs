using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static AppRunner.MouseOperations;

namespace AppRunner
{
    public class Program
    {

        static void Main(string[] args)
        {
            try
            {
                uint min = 2, max = 4;
                if (args != null && args.Length == 2)
                {
                    if (uint.TryParse(args[0], out uint newMin) && uint.TryParse(args[1], out uint newMax) &&
                        newMin < newMax)
                    {
                        min = newMin;
                        max = newMax;
                    }
                }

                Console.WriteLine($"Started with [{min},{max}] minutes range.");
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                var r = new Random();
                while (true)
                {
                    var now = DateTime.Now;
                    var nextDelayInSecs = r.Next((int)min * 60, (int)max * 60);
                    Console.WriteLine($"Next move: {now.AddSeconds(nextDelayInSecs).ToString("MM/dd/yyyy HH:mm:ss")}");
                    Thread.Sleep(nextDelayInSecs * 1000);
                    var currentClicks = r.Next(1, 4);
                    var w = Screen.PrimaryScreen.Bounds.Width;
                    var wS = (w / 2);
                    var wE = (w / 2) + (w / 3);
                    var h = Screen.PrimaryScreen.Bounds.Height;
                    var hS = (h / 2) - (h / 4);
                    var hE = (h / 2) + (h / 4);
                    var point = new Point(r.Next(wS, wE), r.Next(hS, hE));
                    Console.WriteLine($"Current clicks count: {currentClicks}");
                    for (var i = 0; i < currentClicks; ++i)
                    {
                        var mp = new MousePoint(point.X + i, point.Y + i);
                        SetCursorPosition(mp);
                        Click(MouseEventFlags.LeftDown, mp);
                        Click(MouseEventFlags.LeftUp, mp);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured: {e}");
                Console.WriteLine($"Retry again.");
            }
        }
    }


    public class MouseOperations
    {
        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public static bool SetCursorPosition(int x, int y)
        {
            return SetCursorPos(x, y);
        }

        public static bool SetCursorPosition(MousePoint point)
        {
            return SetCursorPos(point.X, point.Y);
        }

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void Click(MouseEventFlags value, MousePoint position)
        {
            mouse_event
                ((int)value,
                    position.X,
                    position.Y,
                    0,
                    0)
                ;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
            public MousePoint(Point point)
            {
                X = point.X;
                Y = point.Y;
            }
        }
    }
}
