using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
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
        private bool m_mIsRunning;
        private readonly PerformanceCounter m_memoryCounter = new PerformanceCounter("Process", "Working Set - Private", Process.GetCurrentProcess().ProcessName);
        private readonly PerformanceCounter m_cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
        private readonly Queue m_cpuQueue = new Queue(60);
        private readonly Queue m_memoryQueue = new Queue(60);

        public MainForm()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (!m_mIsRunning)
            {
                if (MasterServer.Instance.Start())
                {
                    m_mIsRunning = true;
                    button1.Text = "停止";
                }
            }
            else
            {
                MasterServer.Instance.Stop();
                m_mIsRunning = false;
                button1.Text = "启动";
                button1.Enabled = false;
            }
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

                if (cmd.ExecuteNonQuery() > 0)
                    MessageBox.Show("成功");
                else
                    MessageBox.Show("失败");
            }
        }

        TimeSpan prevCpuTime = TimeSpan.Zero;
        private void MainForm_Load(object sender, EventArgs e)
        {
            Console.SetOut(new TextBoxWriter(textBox1));

            chart_CPU.DataSource = m_cpuQueue;
            chart_Memory.DataSource = m_cpuQueue;

            TimerManager.Instance.Start();

            TimerManager.Instance.RepeatTask(() => chart_CPU.BeginInvoke(new Action(() =>
              {
                  //当前时间
                  var curTime = Process.GetCurrentProcess().TotalProcessorTime;
                  //间隔时间内的CPU运行时间除以逻辑CPU数量
                  var value = (curTime - prevCpuTime).TotalMilliseconds / 1000 / Environment.ProcessorCount * 100;
                  prevCpuTime = curTime;

                  if (m_cpuQueue.Count == 60)
                      m_cpuQueue.Dequeue();
                  if (m_cpuQueue.Count < 60)
                      m_cpuQueue.Enqueue(value);

                  chart_CPU.Series[0].Points.DataBindY(m_cpuQueue);
                  chart_CPU.DataBind();

              })), 1000);

            TimerManager.Instance.RepeatTask(() => chart_Memory.BeginInvoke(new Action(() =>
            {
                //double cpu = Math.Round(m_cpuCounter.NextValue(), 2, MidpointRounding.AwayFromZero);
                double memory = Math.Round(Process.GetCurrentProcess().WorkingSet64/1024D/1024 , 2, MidpointRounding.AwayFromZero);

                if (m_memoryQueue.Count == 60)
                    m_memoryQueue.Dequeue();
                if (m_memoryQueue.Count < 60)
                    m_memoryQueue.Enqueue(memory);



                chart_Memory.Series[0].Points.DataBindY(m_memoryQueue);
                chart_Memory.DataBind();

            })), 1000);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_mIsRunning)
            {
                MessageBox.Show("服务端运行中，请停止后再关闭!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
            else
            {
                var result = MessageBox.Show("确定要关闭吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
              
                if (result && TimerManager.Instance.IsStarted)
                    TimerManager.Instance.Stop();

                e.Cancel = !result;
            }
        }

    }
}