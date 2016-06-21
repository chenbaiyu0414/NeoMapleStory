using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using NeoMapleStory.Core;
using NeoMapleStory.Server;

namespace NeoMapleStory
{
    public partial class MainForm : Form
    {
        private bool m_isRunning;
        private DateTime m_runningTime;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Console.SetOut(new TextBoxWriter(textBox1));

            panel_Game.Dock = panel_Settings.Dock = DockStyle.Fill;
            panel_Game.BringToFront();

            propertyGrid1.SelectedObject = new ServerProperties();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_isRunning)
            {
                MessageBox.Show("服务端运行中，请停止后再关闭!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
            else
            {
                var result = MessageBox.Show("确定要关闭吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
                e.Cancel = !result;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //toolStripStatusLabel_ServerTime.Text = $"服务器时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
            //if (m_isRunning)
            //{
            //    m_runningTime = m_runningTime.AddSeconds(1);
            //    toolStripStatusLabel_ServerRunningTime.Text = $"服务端已运行时间：{m_runningTime.ToString("HH:mm:ss")}";
            //}
        }

        private void toolStripButton_LaunchServer_Click(object sender, EventArgs e)
        {
            if (!m_isRunning)
            {
                Thread startThread = new Thread(() => { MasterServer.Instance.Start(); });
                startThread.Start();
                if (startThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    m_isRunning = true;
                    toolStripButton_LaunchServer.Text = "关闭服务端";
                }
            }
            else
            {
                Thread stopThread = new Thread(() => { MasterServer.Instance.Stop(); });
                stopThread.Start();
                m_isRunning = false;
                toolStripButton_LaunchServer.Text = "启动服务端";
                toolStripButton_LaunchServer.Enabled = false;
            }
        }

        private void toolBtn_Game_Click(object sender, EventArgs e) => panel_Game.BringToFront();

        private void toolBtn_Settings_Click(object sender, EventArgs e) => panel_Settings.BringToFront();

    }

    public class ServerProperties
    {
        public enum ServerLocale :byte
        {
            China,
        }

        [Category("基本设置"), DescriptionAttribute("冒险岛版本")]
        public int Version { get; set; } = 79;

        [Category("基本设置"), DescriptionAttribute("冒险岛地区版本")]
        public ServerLocale Locale { get; set; } = ServerLocale.China;

        [Category("基本设置"), DescriptionAttribute("冒险岛服务器名字")]
        public string ServerName { get; set; } = "NeoMapleStory";

        [Category("基本设置"), DescriptionAttribute("冒险岛顶部滚动的广告")]
        public string ServerAdvertisment { get; set; } = "NeoMapleStory v79 测试中可能频繁掉线";

        [Category("倍率"), DescriptionAttribute("掉落倍率")]
        public double DropRate { get; set; } = 1.0;

        [Category("倍率"), DescriptionAttribute("经验倍率")]
        public double ExpRate { get; set; } = 1.0;

        [Category("倍率"), DescriptionAttribute("金币倍率")]
        public double MesoRate { get; set; } = 1.0;
    }
}