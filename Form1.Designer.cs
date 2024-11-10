
namespace TTTools
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            textBox_log = new System.Windows.Forms.TextBox();
            checkBox1 = new System.Windows.Forms.CheckBox();
            mainTabPage2 = new System.Windows.Forms.TabPage();
            button2 = new System.Windows.Forms.Button();
            label_current = new System.Windows.Forms.Label();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            hWndDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            titleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            actionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            isLeadDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            isActionRunDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            isActionAutoDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            clientBindingSource = new System.Windows.Forms.BindingSource(components);
            button11 = new System.Windows.Forms.Button();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage2 = new System.Windows.Forms.TabPage();
            label14 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            label12 = new System.Windows.Forms.Label();
            button10 = new System.Windows.Forms.Button();
            button9 = new System.Windows.Forms.Button();
            comboBox1 = new System.Windows.Forms.ComboBox();
            button7 = new System.Windows.Forms.Button();
            tabPage3 = new System.Windows.Forms.TabPage();
            button16 = new System.Windows.Forms.Button();
            button15 = new System.Windows.Forms.Button();
            button12 = new System.Windows.Forms.Button();
            tabPage5 = new System.Windows.Forms.TabPage();
            button1 = new System.Windows.Forms.Button();
            button_dev_1 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            mainTabPage1 = new System.Windows.Forms.TabPage();
            button20 = new System.Windows.Forms.Button();
            label11 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            button19 = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            textBox6 = new System.Windows.Forms.TextBox();
            button18 = new System.Windows.Forms.Button();
            button8 = new System.Windows.Forms.Button();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            tabControl2 = new System.Windows.Forms.TabControl();
            mainTabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)clientBindingSource).BeginInit();
            tabControl1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage5.SuspendLayout();
            mainTabPage1.SuspendLayout();
            tabControl2.SuspendLayout();
            SuspendLayout();
            // 
            // textBox_log
            // 
            textBox_log.Location = new System.Drawing.Point(14, 426);
            textBox_log.Multiline = true;
            textBox_log.Name = "textBox_log";
            textBox_log.Size = new System.Drawing.Size(586, 126);
            textBox_log.TabIndex = 30;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox1.Location = new System.Drawing.Point(606, 430);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(75, 21);
            checkBox1.TabIndex = 31;
            checkBox1.Text = "开启日志";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // mainTabPage2
            // 
            mainTabPage2.AllowDrop = true;
            mainTabPage2.Controls.Add(button2);
            mainTabPage2.Controls.Add(label_current);
            mainTabPage2.Controls.Add(dataGridView1);
            mainTabPage2.Controls.Add(button11);
            mainTabPage2.Controls.Add(tabControl1);
            mainTabPage2.Controls.Add(button3);
            mainTabPage2.Location = new System.Drawing.Point(4, 26);
            mainTabPage2.Name = "mainTabPage2";
            mainTabPage2.Padding = new System.Windows.Forms.Padding(3);
            mainTabPage2.Size = new System.Drawing.Size(668, 382);
            mainTabPage2.TabIndex = 0;
            mainTabPage2.Text = "工具";
            mainTabPage2.UseVisualStyleBackColor = true;
            mainTabPage2.Click += mainTabPage2_Click;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(567, 55);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(95, 23);
            button2.TabIndex = 43;
            button2.Text = "取消当前任务";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            // 
            // label_current
            // 
            label_current.AutoSize = true;
            label_current.Location = new System.Drawing.Point(8, 6);
            label_current.Name = "label_current";
            label_current.Size = new System.Drawing.Size(104, 17);
            label_current.TabIndex = 42;
            label_current.Text = "当前选择的角色：";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { hWndDataGridViewTextBoxColumn, idDataGridViewTextBoxColumn, titleDataGridViewTextBoxColumn, actionDataGridViewTextBoxColumn, isLeadDataGridViewCheckBoxColumn, isActionRunDataGridViewCheckBoxColumn, isActionAutoDataGridViewCheckBoxColumn });
            dataGridView1.DataSource = clientBindingSource;
            dataGridView1.Location = new System.Drawing.Point(8, 26);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowTemplate.Height = 25;
            dataGridView1.Size = new System.Drawing.Size(550, 146);
            dataGridView1.TabIndex = 41;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            // 
            // hWndDataGridViewTextBoxColumn
            // 
            hWndDataGridViewTextBoxColumn.DataPropertyName = "HWnd";
            hWndDataGridViewTextBoxColumn.HeaderText = "HWnd";
            hWndDataGridViewTextBoxColumn.Name = "hWndDataGridViewTextBoxColumn";
            hWndDataGridViewTextBoxColumn.ReadOnly = true;
            hWndDataGridViewTextBoxColumn.Visible = false;
            // 
            // idDataGridViewTextBoxColumn
            // 
            idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            idDataGridViewTextBoxColumn.HeaderText = "Id";
            idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            idDataGridViewTextBoxColumn.ReadOnly = true;
            idDataGridViewTextBoxColumn.Visible = false;
            // 
            // titleDataGridViewTextBoxColumn
            // 
            titleDataGridViewTextBoxColumn.DataPropertyName = "Title";
            titleDataGridViewTextBoxColumn.HeaderText = "Title";
            titleDataGridViewTextBoxColumn.Name = "titleDataGridViewTextBoxColumn";
            titleDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // actionDataGridViewTextBoxColumn
            // 
            actionDataGridViewTextBoxColumn.DataPropertyName = "Action";
            actionDataGridViewTextBoxColumn.HeaderText = "Action";
            actionDataGridViewTextBoxColumn.Name = "actionDataGridViewTextBoxColumn";
            actionDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // isLeadDataGridViewCheckBoxColumn
            // 
            isLeadDataGridViewCheckBoxColumn.DataPropertyName = "IsLead";
            isLeadDataGridViewCheckBoxColumn.HeaderText = "IsLead";
            isLeadDataGridViewCheckBoxColumn.Name = "isLeadDataGridViewCheckBoxColumn";
            isLeadDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // isActionRunDataGridViewCheckBoxColumn
            // 
            isActionRunDataGridViewCheckBoxColumn.DataPropertyName = "IsActionRun";
            isActionRunDataGridViewCheckBoxColumn.HeaderText = "IsActionRun";
            isActionRunDataGridViewCheckBoxColumn.Name = "isActionRunDataGridViewCheckBoxColumn";
            isActionRunDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // isActionAutoDataGridViewCheckBoxColumn
            // 
            isActionAutoDataGridViewCheckBoxColumn.DataPropertyName = "IsActionAuto";
            isActionAutoDataGridViewCheckBoxColumn.HeaderText = "IsActionAuto";
            isActionAutoDataGridViewCheckBoxColumn.Name = "isActionAutoDataGridViewCheckBoxColumn";
            isActionAutoDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // clientBindingSource
            // 
            clientBindingSource.DataSource = typeof(client.Client);
            // 
            // button11
            // 
            button11.Location = new System.Drawing.Point(567, 26);
            button11.Name = "button11";
            button11.Size = new System.Drawing.Size(95, 23);
            button11.TabIndex = 36;
            button11.Text = "获取角色列表";
            button11.UseVisualStyleBackColor = true;
            button11.Click += button11_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Location = new System.Drawing.Point(8, 178);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(654, 194);
            tabControl1.TabIndex = 35;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(label14);
            tabPage2.Controls.Add(label13);
            tabPage2.Controls.Add(label12);
            tabPage2.Controls.Add(button10);
            tabPage2.Controls.Add(button9);
            tabPage2.Controls.Add(comboBox1);
            tabPage2.Controls.Add(button7);
            tabPage2.Location = new System.Drawing.Point(4, 26);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(646, 164);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "采集";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(15, 115);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(243, 17);
            label14.TabIndex = 33;
            label14.Text = "问2：找不到？答：被遮挡了换一个地方吧。";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(15, 89);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(266, 17);
            label13.TabIndex = 32;
            label13.Text = "问1：点击有偏差？答;关闭显示设置125%倍放大";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(15, 61);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(238, 17);
            label12.TabIndex = 31;
            label12.Text = "用法：1.获取 2.选择要采集的号 3.开始采集";
            // 
            // button10
            // 
            button10.Location = new System.Drawing.Point(152, 16);
            button10.Name = "button10";
            button10.Size = new System.Drawing.Size(105, 23);
            button10.TabIndex = 30;
            button10.Text = "开始采集";
            button10.UseVisualStyleBackColor = true;
            button10.Click += button10_Click;
            // 
            // button9
            // 
            button9.Location = new System.Drawing.Point(96, 175);
            button9.Name = "button9";
            button9.Size = new System.Drawing.Size(75, 23);
            button9.TabIndex = 29;
            button9.Text = "找矿";
            button9.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "矿石", "水晶", "皮草" });
            comboBox1.Location = new System.Drawing.Point(15, 16);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(121, 25);
            comboBox1.TabIndex = 2;
            // 
            // button7
            // 
            button7.Location = new System.Drawing.Point(15, 175);
            button7.Name = "button7";
            button7.Size = new System.Drawing.Size(75, 23);
            button7.TabIndex = 0;
            button7.Text = "寻路";
            button7.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(button16);
            tabPage3.Controls.Add(button15);
            tabPage3.Controls.Add(button12);
            tabPage3.Location = new System.Drawing.Point(4, 26);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new System.Drawing.Size(646, 164);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "挖宝";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            button16.Location = new System.Drawing.Point(181, 15);
            button16.Name = "button16";
            button16.Size = new System.Drawing.Size(75, 23);
            button16.TabIndex = 2;
            button16.Text = "button16";
            button16.UseVisualStyleBackColor = true;
            button16.Click += button16_Click;
            // 
            // button15
            // 
            button15.Location = new System.Drawing.Point(100, 15);
            button15.Name = "button15";
            button15.Size = new System.Drawing.Size(75, 23);
            button15.TabIndex = 1;
            button15.Text = "查看任务";
            button15.UseVisualStyleBackColor = true;
            button15.Click += button15_Click;
            // 
            // button12
            // 
            button12.Location = new System.Drawing.Point(19, 15);
            button12.Name = "button12";
            button12.Size = new System.Drawing.Size(75, 23);
            button12.TabIndex = 0;
            button12.Text = "接任务";
            button12.UseVisualStyleBackColor = true;
            button12.Click += button12_Click;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(button1);
            tabPage5.Controls.Add(button_dev_1);
            tabPage5.Location = new System.Drawing.Point(4, 26);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new System.Windows.Forms.Padding(3);
            tabPage5.Size = new System.Drawing.Size(646, 164);
            tabPage5.TabIndex = 3;
            tabPage5.Text = "功能单元测试";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(87, 6);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(105, 23);
            button1.TabIndex = 44;
            button1.Text = "查询子窗口信息";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button_dev_1
            // 
            button_dev_1.Location = new System.Drawing.Point(6, 6);
            button_dev_1.Name = "button_dev_1";
            button_dev_1.Size = new System.Drawing.Size(75, 23);
            button_dev_1.TabIndex = 1;
            button_dev_1.Text = "发送文本";
            button_dev_1.UseVisualStyleBackColor = true;
            button_dev_1.Click += button_dev_1_Click;
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(567, 147);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(98, 23);
            button3.TabIndex = 32;
            button3.Text = "打开脚本目录";
            button3.UseVisualStyleBackColor = true;
            button3.Visible = false;
            button3.Click += button3_Click;
            // 
            // mainTabPage1
            // 
            mainTabPage1.Controls.Add(button20);
            mainTabPage1.Controls.Add(label11);
            mainTabPage1.Controls.Add(label10);
            mainTabPage1.Controls.Add(label9);
            mainTabPage1.Controls.Add(label8);
            mainTabPage1.Controls.Add(label7);
            mainTabPage1.Controls.Add(button19);
            mainTabPage1.Controls.Add(label3);
            mainTabPage1.Controls.Add(textBox6);
            mainTabPage1.Controls.Add(button18);
            mainTabPage1.Controls.Add(button8);
            mainTabPage1.Controls.Add(flowLayoutPanel1);
            mainTabPage1.Location = new System.Drawing.Point(4, 26);
            mainTabPage1.Name = "mainTabPage1";
            mainTabPage1.Padding = new System.Windows.Forms.Padding(3);
            mainTabPage1.Size = new System.Drawing.Size(668, 382);
            mainTabPage1.TabIndex = 1;
            mainTabPage1.Text = "登陆器";
            mainTabPage1.UseVisualStyleBackColor = true;
            // 
            // button20
            // 
            button20.Location = new System.Drawing.Point(181, 6);
            button20.Name = "button20";
            button20.Size = new System.Drawing.Size(75, 23);
            button20.TabIndex = 11;
            button20.Text = "button20";
            button20.UseVisualStyleBackColor = true;
            button20.Click += button20_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(557, 39);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(56, 17);
            label11.TabIndex = 10;
            label11.Text = "备注信息";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(339, 39);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(80, 17);
            label10.TabIndex = 9;
            label10.Text = "点击登录角色";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(187, 39);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(32, 17);
            label9.TabIndex = 8;
            label9.Text = "密码";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(87, 39);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(32, 17);
            label8.TabIndex = 7;
            label8.Text = "账号";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(6, 39);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(56, 17);
            label7.TabIndex = 6;
            label7.Text = "账号列表";
            // 
            // button19
            // 
            button19.Location = new System.Drawing.Point(87, 6);
            button19.Name = "button19";
            button19.Size = new System.Drawing.Size(88, 23);
            button19.TabIndex = 5;
            button19.Text = "登录所选账号";
            button19.UseVisualStyleBackColor = true;
            button19.Click += button19_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(459, 9);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(56, 17);
            label3.TabIndex = 4;
            label3.Text = "游戏目录";
            // 
            // textBox6
            // 
            textBox6.Location = new System.Drawing.Point(521, 6);
            textBox6.Name = "textBox6";
            textBox6.Size = new System.Drawing.Size(121, 23);
            textBox6.TabIndex = 3;
            textBox6.TextChanged += textBox6_TextChanged;
            // 
            // button18
            // 
            button18.Location = new System.Drawing.Point(363, 6);
            button18.Name = "button18";
            button18.Size = new System.Drawing.Size(90, 23);
            button18.TabIndex = 2;
            button18.Text = "删除所选账号";
            button18.UseVisualStyleBackColor = true;
            button18.Click += button18_Click;
            // 
            // button8
            // 
            button8.Location = new System.Drawing.Point(6, 6);
            button8.Name = "button8";
            button8.Size = new System.Drawing.Size(75, 23);
            button8.TabIndex = 1;
            button8.Text = "新建账号";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button8_Click_1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 59);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(668, 320);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // tabControl2
            // 
            tabControl2.Controls.Add(mainTabPage1);
            tabControl2.Controls.Add(mainTabPage2);
            tabControl2.Location = new System.Drawing.Point(12, 12);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new System.Drawing.Size(676, 412);
            tabControl2.TabIndex = 29;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(695, 555);
            Controls.Add(checkBox1);
            Controls.Add(textBox_log);
            Controls.Add(tabControl2);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "TTTools v0.3 -  by 电脑玩家";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            mainTabPage2.ResumeLayout(false);
            mainTabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)clientBindingSource).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage5.ResumeLayout(false);
            mainTabPage1.ResumeLayout(false);
            mainTabPage1.PerformLayout();
            tabControl2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TextBox textBox_log;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TabPage mainTabPage2;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Button button_dev_1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TabPage mainTabPage1;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource clientBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn hWndDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn titleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn actionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isLeadDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isActionRunDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isActionAutoDataGridViewCheckBoxColumn;
        private System.Windows.Forms.Label label_current;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}

