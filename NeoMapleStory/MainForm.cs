using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MySql.Data.MySqlClient;
using NeoMapleStory.Core;
using NeoMapleStory.Core.Encryption;
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

        private void button2_Click(object sender, EventArgs e)
        {
            //Username = "cby40899570",
            //        Password = Core.Encryption.Sha256.Get("cby159753", grid),
            //        Salt = grid,
            //        SecretKey = "159753",
            //        IsGm = true,
            //        MaplePoint = 1000000,
            //        NexonPoint = 1000000,

            var g1 = Guid.NewGuid();
            var g2 = Guid.NewGuid();

            var cmd = new MySqlCommand(
                @"INSERT Account(Username,Password,PasswordSalt,SecretKey,SecretKeySalt,IsGm,MaplePoint,NexonPoint,BirthDate,RegisterDate,Email) 
                             VALUES(@Username,@Password,@PasswordSalt,@SecretKey,@SecretKeySalt,@IsGm,@MaplePoint,@NexonPoint,@BirthDate,@RegisterDate,@Email)");
            cmd.Parameters.Add(new MySqlParameter("@Username", "cby40899570"));
            cmd.Parameters.Add(new MySqlParameter("@Password", Sha256.Get("cby159753", g1.ToString())));
            cmd.Parameters.Add(new MySqlParameter("@PasswordSalt", g1.ToString().ToUpper()));
            cmd.Parameters.Add(new MySqlParameter("@SecretKey", Sha256.Get("159753", g2.ToString())));
            cmd.Parameters.Add(new MySqlParameter("@SecretKeySalt", g2.ToString().ToUpper()));
            cmd.Parameters.Add(new MySqlParameter("@IsGm", true));
            cmd.Parameters.Add(new MySqlParameter("@MaplePoint", 1000000));
            cmd.Parameters.Add(new MySqlParameter("@BirthDate", DateTime.Now));
            cmd.Parameters.Add(new MySqlParameter("@NexonPoint", 1000000));
            cmd.Parameters.Add(new MySqlParameter("@RegisterDate", DateTime.Now));
            cmd.Parameters.Add(new MySqlParameter("@Email", "cby40899570@qq.com"));

            using (var con = DbConnectionManager.Instance.GetConnection())
            {
                cmd.Connection = con;

                con.Open();

                MessageBox.Show(cmd.ExecuteNonQuery() > 0 ? "成功" : "失败");
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            Console.SetOut(new TextBoxWriter(textBox1));
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


        private void toolStripButton_StartServer_Click(object sender, EventArgs e)
        {

            if (!m_isRunning)
            {
                Thread startThread = new Thread(() => { MasterServer.Instance.Start(); });
                startThread.Start();
                if (startThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    m_isRunning = true;
                    toolStripButton_StartServer.Text = "关闭服务端";
                }
            }
            else
            {
                Thread stopThread = new Thread(() => { MasterServer.Instance.Stop(); });
                stopThread.Start();
                m_isRunning = false;
                toolStripButton_StartServer.Text = "启动服务端";
                toolStripButton_StartServer.Enabled = false;
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel_ServerTime.Text = $"服务器时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
            if (m_isRunning)
            {
                m_runningTime = m_runningTime.AddSeconds(1);
                toolStripStatusLabel_ServerRunningTime.Text = $"服务端已运行时间：{m_runningTime.ToString("HH:mm:ss")}";
            }
        }
    }
}