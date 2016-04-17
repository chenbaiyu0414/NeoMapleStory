using NeoMapleStory.Core;
using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace NeoMapleStory
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private bool _mIsRunning;
        

        private void button1_Click(object sender, EventArgs e)
        {
            if (!_mIsRunning)
            {
                if (Server.MasterServer.Instance.Start())
                {
                    _mIsRunning = true;
                    button1.Text = "停止";
                }
            }
            else
            {
                Server.MasterServer.Instance.Stop();
                _mIsRunning = false;
                button1.Text = "启动";
                button1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Console.SetOut(new TextBoxWriter(textBox1));
        }

        private void button3_Click(object sender, EventArgs e)
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
            cmd.Parameters.Add(new MySqlParameter("@Password",Core.Encryption.Sha256.Get("cby159753",g1.ToString())));
            cmd.Parameters.Add(new MySqlParameter("@PasswordSalt", g1.ToString().ToUpper()));
            cmd.Parameters.Add(new MySqlParameter("@SecretKey", Core.Encryption.Sha256.Get("159753", g2.ToString())));
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
    }
}
