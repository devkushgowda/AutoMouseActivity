using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static AppRunner.MouseOperations;

namespace AppRunner
{
    public class Program
    {

        static void Main(string[] args)
        {
            uint min = 2, max = 4;
            if (args != null && args.Length == 2)
            {
                if (uint.TryParse(args[0], out uint newMin) && uint.TryParse(args[1], out uint newMax) && newMin < newMax)
                {
                    min = newMin;
                    max = newMax;
                }
            }

            Console.WriteLine($"Started with [{min},{max}] minutes range.");
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            var clicksList = new List<List<MouseEventFlags>>{
                new List<MouseEventFlags> {MouseEventFlags.RightDown,MouseEventFlags.RightUp ,MouseEventFlags.LeftDown, MouseEventFlags.LeftUp},
                new List<MouseEventFlags> {MouseEventFlags.RightDown,MouseEventFlags.RightUp,MouseEventFlags.LeftDown, MouseEventFlags.LeftUp,MouseEventFlags.LeftDown, MouseEventFlags.LeftUp },
                new List<MouseEventFlags> {MouseEventFlags.LeftDown, MouseEventFlags.LeftUp},
            };
            var r = new Random();
            while (true)
            {
                var now = DateTime.Now;
                var nextDelay = r.Next((int)min * 60, (int)max * 60);
                Console.WriteLine($"Next move: {now.AddSeconds(nextDelay).ToString("MM/dd/yyyy HH:mm:ss")}");
                Thread.Sleep(nextDelay * 1000);
                var currentClicks = clicksList[r.Next(0, clicksList.Count)];
                var w = Screen.PrimaryScreen.Bounds.Width;
                var wS = (w / 2) - (w / 4);
                var wE = (w / 2) + (w / 4);
                var h = Screen.PrimaryScreen.Bounds.Height;
                var hS = (h / 2) - (h / 4);
                var hE = (h / 2) + (h / 4);
                var point = new Point(r.Next(wS, wE), r.Next(hS, hE));
                currentClicks.ForEach(c =>
                {
                    Console.WriteLine(Enum.GetName(c.GetType(), c));
                    Click(c, new MousePoint(point));
                });
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
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
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
