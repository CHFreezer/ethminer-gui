using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EthminerGUI
{
    public partial class App : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        Process process;
        bool canExit = false;
        bool mining = false;

        public const string configFilename = "ethminer-gui_config.json";
        public static Configuration Configuration { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        private void App_Load(object sender, EventArgs e)
        {
            FormClosing += App_FormClosing;

            SetWindowLong(Program.HWndConsole, -16, 0x10000000);
            SetParent(Program.HWndConsole, Handle);
            App_Resize(null, null);

            Configuration = new Configuration(configFilename);
            Logger.white("配置文件：", Path.GetFullPath(configFilename), "读取完成");
            Logger.green("准备就绪");

            if (string.IsNullOrWhiteSpace(Configuration.LocalMachineName))
            {
                textBox_localMachineName.Text = Environment.MachineName;
            }
            else
            {
                textBox_localMachineName.Text = Configuration.LocalMachineName;
            }
            comboBox_miners.Items.AddRange(Configuration.GetMinerNames());
            comboBox_miners.SelectedIndex = Configuration.SelectedIndex;
            updateComponent();
        }

        private void App_FormClosing(object sender, FormClosingEventArgs e)
        {
            Visible = false;
            if (!canExit && mining)
            {
                e.Cancel = true;
            }
        }

        private void App_Resize(object sender, EventArgs e)
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

        private void toolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            try
            {
                process?.Kill();
            }
            catch { }

            canExit = true;
            Application.Exit();
        }

        private void button_mining_Click(object sender, EventArgs e)
        {
            try
            {
                process?.Kill();
            }
            catch { }
            process?.Dispose();

            if (mining)
            {
                mining = false;

                button_mining.Text = "挖矿";
            }
            else
            {
                Console.WriteLine();
                Logger.cyan("开始挖矿", "\n");

                Task.Run(() => RunProcess());

                mining = true;

                button_mining.Text = "停止";
            }
        }

        void RunProcess()
        {
            try
            {
                var miner = Configuration.CurrentMiner;
                miner.pool = textBox_pool.Text;
                miner.pool2 = textBox_pool2.Text;
                miner.wallet = textBox_wallet.Text;
                miner.wallet2 = textBox_wallet2.Text;
                miner.passwd = textBox_password.Text;
                miner.passwd2 = textBox_password2.Text;
                miner.args = textBox_args.Text;
                Configuration.CurrentMiner = miner;
                Configuration.Save();

                process = new Process();
                Logger.white("内核：", Configuration.CurrentMiner.name.ToString());
                var args = Configuration.CurrentMiner.GetFullArgs();
                Logger.white("execute:", Configuration.CurrentMiner.exePath, args);
                process.StartInfo.FileName = Configuration.CurrentMiner.exePath;
                process.StartInfo.Arguments = args;
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
                process.Dispose();

                Console.OutputEncoding = Encoding.UTF8;

                Console.WriteLine();
                Logger.magenta("已停止", "\n");

                mining = false;

                button_mining.Invoke(new Action(() => { button_mining.Text = "挖矿"; }));
            }
        }

        void updateComponent()
        {
            textBox_pool.Text = Configuration.CurrentMiner.pool;
            textBox_pool2.Text = Configuration.CurrentMiner.pool2;
            textBox_wallet.Text = Configuration.CurrentMiner.wallet;
            textBox_wallet2.Text = Configuration.CurrentMiner.wallet2;
            textBox_password.Text = Configuration.CurrentMiner.passwd;
            textBox_password2.Text = Configuration.CurrentMiner.passwd2;
            textBox_args.Text = Configuration.CurrentMiner.args;
        }

        private void textBox_localMachineName_TextChanged(object sender, EventArgs e)
        {
            Configuration.LocalMachineName = textBox_localMachineName.Text;
            textBox_localMachineName.Text = Configuration.LocalMachineName;
        }

        private void comboBox_miners_SelectedIndexChanged(object sender, EventArgs e)
        {
            Configuration.SelectedIndex = comboBox_miners.SelectedIndex;
            updateComponent();
        }
    }
}
