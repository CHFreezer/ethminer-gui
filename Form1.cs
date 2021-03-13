using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        const string historyPath = "history.txt";

        Process process;
        bool canExit = false;
        bool mining = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(historyPath))
            {
                textBox1.Text = File.ReadAllText(historyPath);
            }

            FormClosing += Form1_FormClosing;

            Program.SetWindowLong(Program.HWndConsole, -16, 0x10000000);
            SetParent(Program.HWndConsole, Handle);
            Form1_Resize(null, null);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            if (!canExit && mining)
            {
                e.Cancel = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (Program.HWndConsole != (IntPtr)0)
            {
                MoveWindow(Program.HWndConsole, panel1.Left, panel1.Top, panel1.Width, panel1.Height, true);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Visible = true;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                process.Kill();
            }
            catch { }

            canExit = true;
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                process.Kill();
            }
            catch { }
            try
            {
                process.Dispose();
            }
            catch { }
            process = null;

            if (mining)
            {
                mining = false;

                button1.Text = "挖矿";
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("开始挖矿");
                Console.ResetColor();

                Task.Run(() => RunProcess());

                mining = true;

                button1.Text = "停止";
            }
        }

        void RunProcess()
        {
            try
            {
                File.WriteAllText(historyPath, textBox1.Text);

                process = new Process();
                process.StartInfo.FileName = "ethminer.exe";
                process.StartInfo.Arguments = textBox1.Text;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                try
                {
                    process.Kill();
                }
                catch { }
                try
                {
                    process.Dispose();
                }
                catch { }
                process = null;

                Console.OutputEncoding = Encoding.UTF8;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("已停止");
                Console.ResetColor();

                mining = false;

                button1.Text = "挖矿";
            }
        }
    }
}
