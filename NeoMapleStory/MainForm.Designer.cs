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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.performanceTimer = new System.Windows.Forms.Timer(this.components);
            this.chart_CPU = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chart_Memory = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chart_CPU)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_Memory)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(14, 17);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 33);
            this.button1.TabIndex = 0;
            this.button1.Text = "启动";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(108, 17);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(87, 33);
            this.button2.TabIndex = 1;
            this.button2.Text = "注册账号";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(14, 58);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(784, 587);
            this.textBox1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1051, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 34);
            this.label1.TabIndex = 3;
            this.label1.Text = "内存：0 MB\r\nCPU：0 %";
            // 
            // chart_CPU
            // 
            this.chart_CPU.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.LabelAutoFitStyle = System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.None;
            chartArea1.AxisX.LabelStyle.Enabled = false;
            chartArea1.AxisX.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            chartArea1.AxisX.MajorGrid.Interval = 0D;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(234)))), ((int)(((byte)(244)))));
            chartArea1.AxisX.MajorTickMark.Enabled = false;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.Transparent;
            chartArea1.AxisX.MinorGrid.Interval = 10D;
            chartArea1.AxisX.MinorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(234)))), ((int)(((byte)(244)))));
            chartArea1.AxisX2.IsReversed = true;
            chartArea1.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisY.LabelAutoFitStyle = System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.None;
            chartArea1.AxisY.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.Transparent;
            chartArea1.AxisY.MinorGrid.Interval = 20D;
            chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(234)))), ((int)(((byte)(244)))));
            chartArea1.BackColor = System.Drawing.Color.White;
            chartArea1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            chartArea1.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.Name = "ChartArea1";
            this.chart_CPU.ChartAreas.Add(chartArea1);
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.DockedToChartArea = "ChartArea1";
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.IsDockedInsideChartArea = false;
            legend1.Name = "Legend1";
            this.chart_CPU.Legends.Add(legend1);
            this.chart_CPU.Location = new System.Drawing.Point(804, 58);
            this.chart_CPU.Name = "chart_CPU";
            series1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
            series1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            series1.LabelBorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
            series1.Legend = "Legend1";
            series1.LegendText = "CPU Usage(%)";
            series1.Name = "Series1";
            this.chart_CPU.Series.Add(series1);
            this.chart_CPU.Size = new System.Drawing.Size(387, 195);
            this.chart_CPU.TabIndex = 4;
            this.chart_CPU.Text = "chart1";
            // 
            // chart_Memory
            // 
            this.chart_Memory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.AxisX.IsMarginVisible = false;
            chartArea2.AxisX.LabelAutoFitStyle = System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.None;
            chartArea2.AxisX.LabelStyle.Enabled = false;
            chartArea2.AxisX.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            chartArea2.AxisX.MajorGrid.Interval = 0D;
            chartArea2.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            chartArea2.AxisX.MajorTickMark.Enabled = false;
            chartArea2.AxisX.MajorTickMark.Interval = 10D;
            chartArea2.AxisX.MajorTickMark.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(125)))), ((int)(((byte)(187)))));
            chartArea2.AxisX.MinorGrid.Interval = 10D;
            chartArea2.AxisX.MinorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            chartArea2.AxisX2.IsReversed = true;
            chartArea2.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea2.AxisY.LabelAutoFitStyle = System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.None;
            chartArea2.AxisY.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            chartArea2.AxisY.MajorGrid.Enabled = false;
            chartArea2.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            chartArea2.AxisY.MajorTickMark.LineColor = System.Drawing.Color.Transparent;
            chartArea2.AxisY.MinorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(237)))), ((int)(((byte)(206)))));
            chartArea2.BackColor = System.Drawing.Color.White;
            chartArea2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            chartArea2.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea2.Name = "ChartArea1";
            this.chart_Memory.ChartAreas.Add(chartArea2);
            legend2.Alignment = System.Drawing.StringAlignment.Center;
            legend2.DockedToChartArea = "ChartArea1";
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend2.IsDockedInsideChartArea = false;
            legend2.IsEquallySpacedItems = true;
            legend2.Name = "Legend1";
            this.chart_Memory.Legends.Add(legend2);
            this.chart_Memory.Location = new System.Drawing.Point(804, 269);
            this.chart_Memory.Name = "chart_Memory";
            series2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
            series2.Color = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(77)))), ((int)(((byte)(166)))), ((int)(((byte)(12)))));
            series2.LabelBorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
            series2.Legend = "Legend1";
            series2.LegendText = "Memory Usage(MB)";
            series2.Name = "Series1";
            this.chart_Memory.Series.Add(series2);
            this.chart_Memory.Size = new System.Drawing.Size(387, 195);
            this.chart_Memory.TabIndex = 5;
            this.chart_Memory.Text = "chart1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1203, 658);
            this.Controls.Add(this.chart_Memory);
            this.Controls.Add(this.chart_CPU);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NeoMapleStory";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart_CPU)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart_Memory)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer performanceTimer;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_CPU;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_Memory;
    }
}

