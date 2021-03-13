using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        [DllImport("kernel32")]
        static extern bool AttachConsole(int dwProcessId);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static IntPtr HWndConsole;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "start")
            {
                var ip = IPAddress.Loopback;
                var endPoint = new IPEndPoint(ip, 8888);
                var listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(endPoint);
                listener.Listen(1);

                while (true)
                {
                    var handler = listener.Accept();
                    var buffer = new byte[1024];
                    var data = new byte[16];
                    var pos = 0;

                    while (true)
                    {
                        int bytesRec = handler.Receive(buffer);
                        Buffer.BlockCopy(buffer, 0, data, pos, bytesRec);
                        pos += bytesRec;
                        if (pos == 16) break;
                    }
                    handler.Close();

                    if (Encoding.ASCII.GetString(data, 0, 12) == "ethminer-gui")
                    {
                        HWndConsole = (IntPtr)BitConverter.ToInt32(data, 12);
                        break;
                    }
                }

                listener.Close();
            }
            else
            {
                var hWnd = GetConsoleWindow();
                ShowWindow(hWnd, 0);
            }

            if (HWndConsole == IntPtr.Zero)
            {
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
                if (key.GetValue("Cascadia Code Regular (TrueType)") == null)
                {
                    key.SetValue("Cascadia Code Regular (TrueType)", Path.GetFullPath(Path.Combine("wt", "Cascadia.ttf")));
                }
                if (key.GetValue("Cascadia Mono Regular (TrueType)") == null)
                {
                    key.SetValue("Cascadia Mono Regular (TrueType)", Path.GetFullPath(Path.Combine("wt", "CascadiaMono.ttf")));
                }

                var process = new Process();
                process.StartInfo.FileName = @"wt\WindowsTerminal.exe";
                process.StartInfo.Arguments = "-f -d . ethminer-gui.exe start";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                process.Start();

                int readCount = 0;
                string title = null;
                while (true)
                {
                    Thread.Sleep(10);

                    process.Refresh();

                    if (process.MainWindowHandle != HWndConsole && readCount < 2)
                    {
                        readCount++;
                        HWndConsole = process.MainWindowHandle;
                        title = process.MainWindowTitle;
                    }

                    if (readCount == 2)
                    {
                        if (process.MainWindowTitle != title)
                        {
                            break;
                        }
                    }
                }

                ShowWindow(process.MainWindowHandle, 1);

                var ip = IPAddress.Loopback;
                var endPoint = new IPEndPoint(ip, 8888);

                Socket sender = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                sender.Connect(endPoint);
                sender.Send(Encoding.ASCII.GetBytes("ethminer-gui"));
                sender.Send(BitConverter.GetBytes((int)process.MainWindowHandle));
                sender.Close();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("准备就绪");
                Console.ResetColor();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}
