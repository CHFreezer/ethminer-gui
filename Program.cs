using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EthminerGUI
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static IntPtr HWndConsole { get; set; }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if !DEBUG
            if (args.Length > 0 && args[0] == "start")
            {
            #region TCP监听hWnd指针
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
            #endregion
            }
            else
            {
                var hWnd = GetConsoleWindow();
                ShowWindow(hWnd, 0);
            }

            if (HWndConsole == IntPtr.Zero)
            {
            #region 注册字体
                var fontPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "Windows", "Fonts");
                var cascadiaPath = Path.Combine(fontPath, "Cascadia.ttf");
                var CascadiaMonoPath = Path.Combine(fontPath, "CascadiaMono.ttf");
                if (!File.Exists(cascadiaPath))
                {
                    Directory.CreateDirectory(fontPath);
                    File.Copy(Path.Combine("wt", "Cascadia.ttf"), cascadiaPath);
                }
                if (!File.Exists(CascadiaMonoPath))
                {
                    Directory.CreateDirectory(fontPath);
                    File.Copy(Path.Combine("wt", "CascadiaMono.ttf"), CascadiaMonoPath);
                }
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
                if (key.GetValue("Cascadia Code Regular (TrueType)") == null)
                {
                    key.SetValue("Cascadia Code Regular (TrueType)", cascadiaPath);
                }
                if (key.GetValue("Cascadia Mono Regular (TrueType)") == null)
                {
                    key.SetValue("Cascadia Mono Regular (TrueType)", CascadiaMonoPath);
                }
            #endregion

            #region 用wt启动自己（套娃
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
            #endregion

            #region TCP发送hWnd指针
                var ip = IPAddress.Loopback;
                var endPoint = new IPEndPoint(ip, 8888);

                Socket sender = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                sender.Connect(endPoint);
                sender.Send(Encoding.ASCII.GetBytes("ethminer-gui"));
                sender.Send(BitConverter.GetBytes((int)process.MainWindowHandle));
                sender.Close();
            #endregion
            }
            else
            {
#else
            HWndConsole = GetConsoleWindow();
#endif
            // Winform主入口
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new App());
#if !DEBUG
            }
#endif
        }
    }
}
