using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVCHOST_
{
    [Flags]
    public enum MouseEventFlags
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004,
        MiddleDown = 0x00000020,
        MiddleUp = 0x00000040,
        Move = 0x00000001,
        Absolute = 0x00008000,
        RightDown = 0x00000008,
        RightUp = 0x00000010
    }
    public class MouseOperations
    {
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern short VkKeyScan(char ch);

        [DllImport("user32.dll")]
        private static extern short keybd_event(byte bVk, byte bScan, int wFlags, int dwExtraInfo);

        public static void SetCursorPosition(int X, int Y)
        {
            SetCursorPos(X, Y);
        }

        public static void SetCursorPosition(MousePoint point)
        {
            SetCursorPos(point.X, point.Y);
        }

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void MouseEvent(MouseEventFlags value)
        {
            MousePoint position = GetCursorPosition();

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

        }

        public static void PostMessage(IntPtr name, int key)
        {
            PostMessage(name, 0x0100, key, 0);
        }

        public static short Send(char ch)
        {
            return VkKeyScan(ch);
        }

        public static void ShiftDown()
        {
            keybd_event(0x10, 0, 0x01 | 0, 0);
        }

        public static void ShiftUp()
        {
            keybd_event(0x10, 0, 0x01 | 0x02, 0);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var points = new List<MouseOperations.MousePoint>();
            points.Add(new MouseOperations.MousePoint() { X = 150, Y = 175 });
            points.Add(new MouseOperations.MousePoint() { X = 250, Y = 275 });
            points.Add(new MouseOperations.MousePoint() { X = 350, Y = 375 });
            points.Add(new MouseOperations.MousePoint() { X = 450, Y = 175 });
            points.Add(new MouseOperations.MousePoint() { X = 550, Y = 275 });
            points.Add(new MouseOperations.MousePoint() { X = 650, Y = 375 });
            points.Add(new MouseOperations.MousePoint() { X = 750, Y = 175 });

            var random = new Random();

            while(1==1)
            {
                var point = GetPoint(points, random.Next(0, 6));
                MouseOperations.SetCursorPosition(point);

                Thread.Sleep(5000);

                MouseOperations.MouseEvent(MouseEventFlags.LeftDown);
                MouseOperations.MouseEvent(MouseEventFlags.LeftUp);

                foreach(var proc in Process.GetProcessesByName("devenv"))
                {
                    var reader = new StreamReader(@"C:\WebEst\in.txt");
                    var lineKey = 0;
                    do
                    {
                        Keys key;
                        var charCollection = @"{}/<>()*&^%$#@!~+_""|:?";
                        var line = reader.ReadLine();
                        lineKey += 1;
                        var lineSpeed = Enumerable.Range(15000, 20000).OrderBy(g => Guid.NewGuid()).Take(1).First();
                        if (lineKey >= 40)
                        {
                            Thread.Sleep(lineSpeed);
                        }
                        if (line != null)
                        {
                            foreach(var ch in line)
                            {
                                var speed = Enumerable.Range(150, 300).OrderBy(g => Guid.NewGuid()).Take(1).First();
                                var str = Char.ToString(ch);
                                if(Char.IsUpper(ch) || charCollection.Contains(str))
                                {
                                    MouseOperations.ShiftDown();
                                    Thread.Sleep(1000);
                                    MouseOperations.PostMessage(proc.MainWindowHandle, MouseOperations.Send(ch));
                                    MouseOperations.ShiftUp();
                                    Thread.Sleep(1000);
                                }
                                else
                                {
                                    MouseOperations.PostMessage(proc.MainWindowHandle, MouseOperations.Send(ch));
                                    Thread.Sleep(speed);
                                }
                            }
                            key = Keys.Return;
                            MouseOperations.PostMessage(proc.MainWindowHandle, (int)key);
                        }
                    } while (!reader.EndOfStream);

                    MouseOperations.PostMessage(proc.MainWindowHandle, (int)Keys.A);
                }
            }
        }

        private static MouseOperations.MousePoint GetPoint(List<MouseOperations.MousePoint> points, int index)
        {
            return points[index];
        }
    }
}
