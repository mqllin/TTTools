using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Reflection.Metadata;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Reflection;

namespace TTTools
{
    public partial class Form1 : Form
    {
        // 导入外部方法
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);



        private int RushTotal = 0;
        private System.Threading.Timer timer;
        private bool shouldClose = false;
        private bool isRunAction = false; //功能是否开启
        System.Threading.Timer timerStart;
        System.Threading.Timer timerEnd;
        private List<Method> apis = new List<Method>();
        private System.Windows.Forms.Timer timerCollect;
        private bool isCollecting = false;  // 新增字段来跟踪采集状态
        // 窗口句柄
        private IntPtr hWnd;

        private Method api;
        private ActionCollectMethod collect;
        private WindowUtilities windowApi;

        public static Form1 MainForm;
        IniFileHelper iniFileHelper;

        public int rushBTotal = 0;
        public Boolean isFighting = false;

        public void AppendGlobalLog(string message)
        {
            if (MainForm != null)
            {
                MainForm.Dbug(message);
            }
        }
        public Form1()
        {
            InitializeComponent();
            MainForm = this;  // 设置全局变量

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            iniFileHelper = new IniFileHelper("settings.ini");

            textBox_popup_x.Text = iniFileHelper.IniReadValue("popup", "x", "186");
            textBox_popup_y.Text = iniFileHelper.IniReadValue("popup", "y", "255");
            textBox1.Text = iniFileHelper.IniReadValue("ts", "y", "0");
            textBox2.Text = iniFileHelper.IniReadValue("ts", "x", "0");
            textBox3.Text = iniFileHelper.IniReadValue("ts", "time", "120");
            textBox4.Text = iniFileHelper.IniReadValue("box", "y", "38");
            textBox5.Text = iniFileHelper.IniReadValue("box", "x", "329");
            TextBox_wid.Text = iniFileHelper.IniReadValue("win", "id", "");
            textBox6.Text = iniFileHelper.IniReadValue("gamepath", "path", "");


            comboBox1.SelectedIndex = 0;
            //tabControl1.TabPages.Remove(tabPage1);
            tabControl1.TabPages.Remove(tabPage3);

            InitList();


        }


        private void InitList()
        {
            int accountCount = int.Parse(iniFileHelper.IniReadValue("General", "AccountCount", "0"));
            for (int i = 0; i < accountCount; i++)
            {
                string username = iniFileHelper.IniReadValue("Account" + i, "Username", "");
                string password = iniFileHelper.IniReadValue("Account" + i, "Password", "");
                string tips = iniFileHelper.IniReadValue("Account" + i, "Tips", "");
                CreateAccount(i, username, password, tips);
            }
        }

        private List<WindowInfo> GetCheckedWindows()
        {
            List<WindowInfo> checkedWindows = new List<WindowInfo>();

            foreach (string item in checkedListBox1.CheckedItems)
            {
                string[] parts = item.Split(new string[] { " - " }, StringSplitOptions.None);
                if (parts.Length >= 2)
                {
                    IntPtr handle = new IntPtr(int.Parse(parts[0]));  // 假设句柄是十六进制的
                    string title = parts[1];

                    checkedWindows.Add(new WindowInfo { Handle = handle, Title = title });
                }
            }

            return checkedWindows;
        }

        private async void Button_link_Click(object sender, EventArgs e)
        {
            // 获取文本框中的窗口句柄ID（假设为16进制字符串）
            string handleStr = TextBox_wid.Text;

            // 将16进制字符串转换为整数
            int handleInt;
            if (int.TryParse(handleStr, System.Globalization.NumberStyles.HexNumber, null, out handleInt))
            {
                // 得到窗口句柄
                hWnd = new IntPtr(handleInt);
                api = new Method(hWnd, this);
                api.InitWindows();
                collect = new ActionCollectMethod(hWnd, this);

            }
            else
            {
                Console.WriteLine("无效的窗口句柄ID");
            }
        }


        public void Dbug(string message)
        {
            if (this.InvokeRequired)  // 检查是否需要在UI线程上执行
            {
                // 如果需要在UI线程上执行，使用Invoke方法
                this.Invoke(new Action<string>(Dbug), message);
                return;
            }

            // 在TextBox中添加新的日志条目
            textBox_log.AppendText(message + "\r\n");

            // 自动滚动到最后一行
            textBox_log.SelectionStart = textBox_log.Text.Length;
            textBox_log.ScrollToCaret();
        }

        private void TextBox_wid_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("win", "id", TextBox_wid.Text);

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            api.GetTS();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Process.Start("explorer.exe", currentDirectory);
        }

        private void textBox_popup_x_TextChanged(object sender, EventArgs e)
        {
            // 写入配置项
            iniFileHelper.IniWriteValue("popup", "x", textBox_popup_x.Text);

        }

        private void textBox_popup_y_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("popup", "y", textBox_popup_y.Text);
        }

        private void button_popup_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (button4.Text == "开始战斗")
            {
                if (isRunAction)
                {
                    MessageBox.Show("当前正在执行其他操作");
                    return;
                }


                if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
                {
                    return;
                }
                rushBTotal = 0;
                int time = int.Parse(textBox3.Text);
                //timerStart = new System.Threading.Timer(RushB, null, 2000, 1000 * time + 20);
                timerEnd = new System.Threading.Timer(CheckTime, null, 2000, 1000 * time);
                isRunAction = true;
                button4.Text = "结束战斗";
            }
            else
            {
                // 停止并释放计时器
                timerStart?.Dispose();
                timerEnd?.Dispose();

                // 重置按钮文本和运行状态
                button4.Text = "开始战斗";
                isRunAction = false;
            }
        }
        private void CheckFightingEnd()
        {
            if (isFighting)
            {
                PictureMethod pi = new PictureMethod(this.hWnd);
                var isEnd = pi.CheckIfWindowContentChanged();
                if (isEnd)
                {
                    rushBTotal++;
                    if (isFighting)
                    {
                        var thread = new Thread(() =>
                        {
                            Clipboard.SetText("退出战斗");
                        });
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        thread.Join();
                        api = new Method(hWnd, this);
                        api.EndCombatByMsg();
                        isFighting = false;
                    }

                }
            }
        }
        private void CheckTime(object state)
        {
            api = new Method(hWnd, this);

            rushBTotal++;
            if (isFighting)
            {
                var thread = new Thread(() =>
                {
                    Clipboard.SetText("退出战斗");
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                api.EndCombatByMsg();
                isFighting = false;
            }

            Thread.Sleep(2000);
            int x = int.Parse(textBox2.Text);
            int y = int.Parse(textBox1.Text);
            isFighting = true;

            api.RushB(rushBTotal, x, y);

        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("ts", "x", textBox2.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("ts", "y", textBox1.Text);

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("ts", "time", textBox3.Text);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Clipboard.SetText("退出战斗");
            api.EndCombatByMsg();
            isFighting = false;

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("box", "x", textBox5.Text);

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("box", "y", textBox4.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            api.EndCombat2();
        }

        int localIndex = 0;
        private void button7_Click(object sender, EventArgs e)
        {
            //api.handleCollection(hWnd, "矿石");
            api.MoveCollectionLocation("矿石", localIndex++);


        }
        private void clearAction()
        {

        }


        private void button8_Click(object sender, EventArgs e)
        {
            //if (button8.Text == "开始采集")
            //{
            //    if (isRunAction)
            //    {
            //        MessageBox.Show("当前正在执行其他操作");
            //        return;
            //    }
            //    button8.Text = "结束采集";

            //    // 开始采集
            //    //timerEnd = new System.Threading.Timer(startActionCollet, null, 2000, 2000);
            //    collect.StartActionCollet();
            //    isRunAction = true;
            //}
            //else
            //{
            //    button8.Text = "开始采集";
            //    collect.clear();
            //}
        }


        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            windowApi = new WindowUtilities();
            var windows = windowApi.UpdateWindowTitles("Galaxy2DEngine");  // 以 Galaxy2DEngine 窗口为例
            checkedListBox1.Items.Clear();  // 首先清除 checkedListBox1 中的所有项

            foreach (var window in windows)
            {
                string itemText = $"{window.Handle} - {window.Title}";
                checkedListBox1.Items.Add(itemText);  // 将窗口信息添加到 checkedListBox1 中
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 假设某些窗口被关闭，然后清空所有窗口标题
            if (windowApi != null)
            {
                windowApi.ClearTitles();

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (!isCollecting)
            {
                StartCollecting();
                button10.Text = "关闭采集";
            }
            else
            {
                StopCollecting();
                button10.Text = "开始原地采集";
            }
            isCollecting = !isCollecting;  // 切换采集状态
        }
        private void StartCollecting()
        {
            var checkedWindows = GetCheckedWindows();
            apis.Clear();  // 清除旧的apis列表

            foreach (var window in checkedWindows)
            {
                apis.Add(new Method(window.Handle, this));
            }
            timerCollect = new System.Windows.Forms.Timer();  // 更改为使用 System.Windows.Forms.Timer
            timerCollect.Interval = 2000;
            timerCollect.Tick += OnTimedEvent;
            timerCollect.Start();
        }

        private void StopCollecting()
        {
            if (timerCollect != null)
            {
                timerCollect.Stop();  // 停止定时器
                timerCollect.Dispose();  // 释放定时器资源
                timerCollect = null;  // 将定时器设置为null以防止重复停止
            }
            apis.Clear();  // 清除apis列表
        }


        private void OnTimedEvent(object sender, EventArgs e)  // 更改方法签名以匹配 Tick 事件
        {
            string selectedValue = comboBox1.SelectedItem?.ToString();

            if (selectedValue != null)
            {
                var tasks = new List<Task>();  // 创建一个任务列表
                foreach (var api in apis)
                {

                    // 使用 Task.Run 异步执行每个 FindAndClickSomeThingInMap 操作
                    var task = Task.Run(() =>
                    {
                        var point = api.FindAndClickSomeThingInMap(selectedValue, true);
                        if (point != null)
                        {
                            // 注意: 如果 Dbug 方法会更新UI，需要确保它在UI线程上执行
                            Invoke(new Action(() => Dbug(point.Value.X + "," + point.Value.Y)));
                        }
                        else
                        {
                            Invoke(new Action(() =>
                            {
                                Dbug("没找到" + selectedValue);

                            }));

                        }
                    });
                    tasks.Add(task);  // 将任务添加到任务列表
                }

                // 可选: 如果你想等待所有任务完成，可以使用以下代码
                // Task.WaitAll(tasks.ToArray());
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            api.FindAndClickSomeThingInMap("矿石", true);
        }



        private void button14_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            api.pushMsg("退出战斗");
        }

        private void button12_Click_2(object sender, EventArgs e)
        {
            var wabao = new ActionTreasureMethod(hWnd, this);
            wabao.Run();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            int selectedCount = checkedListBox1.CheckedItems.Count;
            if (selectedCount > 1)
            {
                MessageBox.Show("只能选择一项");
                return;
            }
            else if (selectedCount == 0)
            {
                MessageBox.Show("请至少选择一项");
                return;
            }

            string selectedItem = checkedListBox1.CheckedItems[0].ToString();
            string[] parts = selectedItem.Split('-');

            if (parts.Length >= 2)
            {
                string handleIdStr = parts[0].Trim();
                if (int.TryParse(handleIdStr, out int handleId))
                {
                    hWnd = new IntPtr(handleId);  // 注意这里将 int 转换为 IntPtr
                    api = new Method(hWnd, this);
                    api.InitWindows();
                    collect = new ActionCollectMethod(hWnd, this);
                    TextBox_wid.Text = handleIdStr;
                }
                else
                {
                    MessageBox.Show("无法解析句柄ID");
                }
            }
            else
            {
                MessageBox.Show("选中的项格式不正确");
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            //CaptureAndOCR orc = new CaptureAndOCR(hWnd, 137, 344, 439, 31);
            //MapPoint location = orc.CaptureAndRecognize();
            //if (location != null)
            //{
            //    Dbug("接到盗宝贼任务：" + location.ToString());
            //    api.ClosePopupAuto();


            //}
            //else
            //{
            //    Dbug("阅读任务内容失败");

            //}

            PictureMethod pic = new PictureMethod(hWnd);
            Point point = new Point(401, 265);
            var lastPoint = api.ClickTabMapPoint(point);
            // 等待移动完成
            while (pic.CheckIsMoving())
            {
                // 你可能需要在这里加上一点延迟，以防止CPU使用率过高
                Thread.Sleep(500);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            api.ClickSelf();
            //MapPoint p = new MapPoint("应天府东",54,33);
            //Point p2 = p.GetPx();
            //Dbug(p2.ToString());
        }

        private void textBox_log_TextChanged(object sender, EventArgs e)
        {

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            CreateAccount();
        }

        private void CreateAccount(int? index = null, string username = "", string password = "", string tips = "")
        {

            // 创建并设置 Panel 的属性
            Panel panel = new Panel();
            panel.Width = 650; // 宽度增加以容纳所有组件
            panel.Height = 40;
            panel.Margin = new Padding(0, 10, 0, 0); // 上方间隔为10px

            // 计算垂直居中的位置
            int centerY = panel.Height / 2;

            // 创建并设置 CheckBox 的属性
            CheckBox checkBox = new CheckBox();
            checkBox.Top = centerY - checkBox.Height / 2 - 3;
            checkBox.Left = 10; // 设置 Left 属性
            checkBox.Width = 18;
            checkBox.Height = 35;


            // 创建并设置用户名输入框的属性
            TextBox txtUsername = new TextBox();
            txtUsername.Size = new Size(100, 35);
            txtUsername.Top = centerY - txtUsername.Height / 2;
            txtUsername.Left = checkBox.Right + 10; // 设置 Left 属性
            txtUsername.Text = username;
            txtUsername.TextChanged += new EventHandler(txtUsername_TextChanged);  // 绑定事件


            // 创建并设置密码输入框的属性
            TextBox txtPassword = new TextBox();
            txtPassword.Size = new Size(100, 35);
            txtPassword.Top = centerY - txtPassword.Height / 2;
            txtPassword.Left = txtUsername.Right + 10; // 设置 Left 属性
            txtPassword.Text = password;
            txtPassword.PasswordChar = '*';
            txtPassword.TextChanged += new EventHandler(txtPassword_TextChanged);  // 绑定事件

            // 创建并设置角色按钮
            for (int i = 1; i <= 5; i++)
            {
                Button btnRole = new Button();
                btnRole.Width = 50;
                btnRole.Text = "角色" + i.ToString();
                btnRole.Top = centerY - btnRole.Height / 2;
                btnRole.Left = (txtPassword.Right + 10 + i * btnRole.Width) - 50;  // 根据按钮的数量和宽度设置 Left 属性
                btnRole.Tag = i;  // 用 Tag 属性存储角色的索引
                btnRole.Click += new EventHandler(RoleButton_Click);  // 绑定点击事件

                panel.Controls.Add(btnRole);
            }

            //备注输入框
            TextBox txtTips = new TextBox();
            txtTips.Size = new Size(130, 35);
            txtTips.Top = centerY - txtTips.Height / 2;
            txtTips.Text = tips;
            txtTips.TextChanged += new EventHandler(txtTips_TextChanged);  // 绑定事件

            // 注意：这里的 270 是所有角色按钮的总宽度（250）加上两个10像素的间隔
            txtTips.Left = txtPassword.Right + 10 + 250 + 10; // 设置 Left 属性

            // 将控件添加到 Panel
            panel.Controls.Add(checkBox);
            panel.Controls.Add(txtUsername);
            panel.Controls.Add(txtPassword);
            panel.Controls.Add(txtTips);

            // 将 Panel 添加到 FlowLayoutPanel
            flowLayoutPanel1.Controls.Add(panel);


            // 如果这是一个新账号（即没有提供索引），则保存新的账号数量
            if (!index.HasValue)
            {
                int accountCount = flowLayoutPanel1.Controls.Count;
                iniFileHelper.IniWriteValue("General", "AccountCount", accountCount.ToString());
            }
        }
        private void RoleButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                Panel parentPanel = clickedButton.Parent as Panel;
                if (parentPanel != null)
                {
                    var textBoxes = parentPanel.Controls.OfType<TextBox>().ToList();
                    if (textBoxes.Count >= 2)  // 确保至少有两个 TextBox
                    {
                        TextBox txtUsername = textBoxes[0];  // 第一个 TextBox 为用户名
                        TextBox txtPassword = textBoxes[1];  // 第二个 TextBox 为密码

                        int roleIndex = (int)clickedButton.Tag;

                        // 在这里使用账号、密码和角色索引进行你需要的操作
                        string username = txtUsername.Text;
                        string password = txtPassword.Text;
                        Dbug($"开始登录角色{roleIndex},{username},{password}");
                        // 使用Task.Run来在后台线程执行耗时操作
                        Task.Run(() =>
                        {
                            var login = new LoginMethod(username, password, roleIndex - 1, this);
                            login.LoginGame();
                        });
                    }
                }
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            Panel parentPanel = txtBox.Parent as Panel;
            int panelIndex = flowLayoutPanel1.Controls.IndexOf(parentPanel);

            iniFileHelper.IniWriteValue("Account" + panelIndex, "Username", txtBox.Text);
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            Panel parentPanel = txtBox.Parent as Panel;
            int panelIndex = flowLayoutPanel1.Controls.IndexOf(parentPanel);

            iniFileHelper.IniWriteValue("Account" + panelIndex, "Password", txtBox.Text);
        }

        private void txtTips_TextChanged(object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            Panel parentPanel = txtBox.Parent as Panel;
            int panelIndex = flowLayoutPanel1.Controls.IndexOf(parentPanel);

            iniFileHelper.IniWriteValue("Account" + panelIndex, "Tips", txtBox.Text);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            List<Panel> panelsToDelete = new List<Panel>();

            // 找到所有被选中的CheckBox
            foreach (Control panel in flowLayoutPanel1.Controls)
            {
                if (panel is Panel)
                {
                    foreach (Control control in panel.Controls)
                    {
                        if (control is CheckBox checkBox && checkBox.Checked)
                        {
                            panelsToDelete.Add(panel as Panel);
                            break;
                        }
                    }
                }
            }

            // 删除相关的Panel和INI配置
            foreach (Panel panel in panelsToDelete)
            {
                int index = flowLayoutPanel1.Controls.IndexOf(panel);

                // 删除INI中的配置
                iniFileHelper.IniWriteValue("Account" + index, "Username", null);
                iniFileHelper.IniWriteValue("Account" + index, "Password", null);
                iniFileHelper.IniWriteValue("Account" + index, "Tips", null);

                // 删除Panel
                flowLayoutPanel1.Controls.Remove(panel);
                panel.Dispose();
            }

            // 更新账号数量
            int accountCount = flowLayoutPanel1.Controls.Count;
            iniFileHelper.IniWriteValue("General", "AccountCount", accountCount.ToString());
        }

        private void button20_Click(object sender, EventArgs e)
        {
            var login = new LoginMethod("mqllin1", "123321000", 0, this);
            login.LoginGame();

        }



        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("gamepath", "path", textBox6.Text);

        }

        private void button21_Click(object sender, EventArgs e)
        {
            // Assume the window handle ID is provided as a command-line argument

        }

        private async void button19_Click(object sender, EventArgs e)
        {
            List<Panel> panelsToLogin = new List<Panel>();

            // 找到所有被选中的 CheckBox
            foreach (Control panel in flowLayoutPanel1.Controls)
            {
                if (panel is Panel)
                {
                    bool isChecked = false;
                    string username = "";
                    string password = "";

                    foreach (Control control in panel.Controls)
                    {
                        if (control is CheckBox checkBox && checkBox.Checked)
                        {
                            isChecked = true;
                        }
                        if (control is TextBox txtUsername && txtUsername.Name == "txtUsername")
                        {
                            username = txtUsername.Text;
                        }
                        if (control is TextBox txtPassword && txtPassword.Name == "txtPassword")
                        {
                            password = txtPassword.PasswordChar == '*' ? txtPassword.Text : "";
                        }
                    }

                    if (isChecked)
                    {
                        // 异步对每一个账号的5个角色调用 api.handleLogin 方法
                        List<Task> tasks = new List<Task>();
                        for (int i = 1; i <= 5; i++)  // 因为有 5 个角色
                        {
                            int roleIndex = i;  // 避免闭包问题
                            tasks.Add(Task.Run(() =>
                            {
                                var login = new LoginMethod(username, password, roleIndex, this);
                                login.LoginGame();
                            }));
                        }
                        await Task.WhenAll(tasks);
                    }
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {

        }
    }
}
