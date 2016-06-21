namespace NeoMapleStory
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolBtn_Game = new System.Windows.Forms.ToolStripButton();
            this.toolBtn_Settings = new System.Windows.Forms.ToolStripButton();
            this.toolBtn_Plugins = new System.Windows.Forms.ToolStripButton();
            this.panel_Game = new System.Windows.Forms.Panel();
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_LaunchServer = new System.Windows.Forms.ToolStripButton();
            this.panel_Settings = new System.Windows.Forms.Panel();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.toolStrip2.SuspendLayout();
            this.panel_Game.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.panel_Settings.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 25);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(440, 270);
            this.textBox1.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // toolStrip2
            // 
            this.toolStrip2.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip2.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolBtn_Game,
            this.toolBtn_Settings,
            this.toolBtn_Plugins});
            this.toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip2.Size = new System.Drawing.Size(944, 63);
            this.toolStrip2.TabIndex = 6;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel1.Font = new System.Drawing.Font("Microsoft YaHei UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(118, 60);
            this.toolStripLabel1.Text = "在线人数：0";
            // 
            // toolBtn_Game
            // 
            this.toolBtn_Game.AutoSize = false;
            this.toolBtn_Game.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolBtn_Game.Font = new System.Drawing.Font("Microsoft YaHei UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolBtn_Game.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolBtn_Game.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBtn_Game.Name = "toolBtn_Game";
            this.toolBtn_Game.Size = new System.Drawing.Size(105, 60);
            this.toolBtn_Game.Text = "游戏";
            this.toolBtn_Game.Click += new System.EventHandler(this.toolBtn_Game_Click);
            // 
            // toolBtn_Settings
            // 
            this.toolBtn_Settings.AutoSize = false;
            this.toolBtn_Settings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolBtn_Settings.Font = new System.Drawing.Font("Microsoft YaHei UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolBtn_Settings.Image = ((System.Drawing.Image)(resources.GetObject("toolBtn_Settings.Image")));
            this.toolBtn_Settings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBtn_Settings.Name = "toolBtn_Settings";
            this.toolBtn_Settings.Size = new System.Drawing.Size(105, 60);
            this.toolBtn_Settings.Text = "设置";
            this.toolBtn_Settings.Click += new System.EventHandler(this.toolBtn_Settings_Click);
            // 
            // toolBtn_Plugins
            // 
            this.toolBtn_Plugins.AutoSize = false;
            this.toolBtn_Plugins.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolBtn_Plugins.Font = new System.Drawing.Font("Microsoft YaHei UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolBtn_Plugins.Image = ((System.Drawing.Image)(resources.GetObject("toolBtn_Plugins.Image")));
            this.toolBtn_Plugins.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBtn_Plugins.Name = "toolBtn_Plugins";
            this.toolBtn_Plugins.Size = new System.Drawing.Size(105, 60);
            this.toolBtn_Plugins.Text = "插件";
            // 
            // panel_Game
            // 
            this.panel_Game.Controls.Add(this.textBox1);
            this.panel_Game.Controls.Add(this.toolStrip3);
            this.panel_Game.Location = new System.Drawing.Point(12, 66);
            this.panel_Game.Name = "panel_Game";
            this.panel_Game.Size = new System.Drawing.Size(440, 295);
            this.panel_Game.TabIndex = 5;
            // 
            // toolStrip3
            // 
            this.toolStrip3.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_LaunchServer});
            this.toolStrip3.Location = new System.Drawing.Point(0, 0);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip3.Size = new System.Drawing.Size(440, 25);
            this.toolStrip3.TabIndex = 3;
            this.toolStrip3.Text = "toolStrip3";
            // 
            // toolStripButton_LaunchServer
            // 
            this.toolStripButton_LaunchServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_LaunchServer.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_LaunchServer.Image")));
            this.toolStripButton_LaunchServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_LaunchServer.Name = "toolStripButton_LaunchServer";
            this.toolStripButton_LaunchServer.Size = new System.Drawing.Size(72, 22);
            this.toolStripButton_LaunchServer.Text = "开启服务端";
            this.toolStripButton_LaunchServer.Click += new System.EventHandler(this.toolStripButton_LaunchServer_Click);
            // 
            // panel_Settings
            // 
            this.panel_Settings.Controls.Add(this.propertyGrid1);
            this.panel_Settings.Location = new System.Drawing.Point(458, 66);
            this.panel_Settings.Name = "panel_Settings";
            this.panel_Settings.Size = new System.Drawing.Size(474, 295);
            this.panel_Settings.TabIndex = 7;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(474, 295);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(944, 561);
            this.Controls.Add(this.panel_Settings);
            this.Controls.Add(this.panel_Game);
            this.Controls.Add(this.toolStrip2);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NeoMapleStory";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.panel_Game.ResumeLayout(false);
            this.panel_Game.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.panel_Settings.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton toolBtn_Game;
        private System.Windows.Forms.Panel panel_Game;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripButton toolStripButton_LaunchServer;
        private System.Windows.Forms.ToolStripButton toolBtn_Settings;
        private System.Windows.Forms.ToolStripButton toolBtn_Plugins;
        private System.Windows.Forms.Panel panel_Settings;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
    }
}

