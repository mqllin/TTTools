using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTTools.client;
using TTTools.gameTools;
using TTTools.windowsTools;

namespace TTTools
{
    public partial class Form1 : Form
    {
        private List<Client> clients = new List<Client>();

        /**
         * 配置文件保存代码
         * iniFileHelper.IniWriteValue("ts", "time", textBox3.Text);
         * 配置读取方法
         * textBox_popup_x.Text = iniFileHelper.IniReadValue("popup", "x", "186");
         **/


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
        private WindowApi windowApi;

        public static Form1 MainForm;
        IniFileHelper iniFileHelper;

        public int rushBTotal = 0;
        public Boolean isFighting = false;


        public Form1()
        {
            InitializeComponent();
            MainForm = this;  // 设置全局变量

        }

        private void Form1_Load(object sender, EventArgs e)
        {
#if DEBUG
            button20.Visible = true;
#else
                    button20.Visible = false;
                    button3.Visible = false;
                     button47.Visible = false;
                     button40.Visible = false;
            textBox3.Enabled = false;
#endif

            LogService.Log("说明，现在能用的只有登录功能");
            LogService.Log("说明，点击偏移的笔记本请修改显示比例为100%,关闭125%缩放");

            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            iniFileHelper = new IniFileHelper("settings.ini");

            textBox6.Text = iniFileHelper.IniReadValue("gamepath", "path", "");


            LogService.IsDebugEnabled = true;
            //tabControl2.TabPages.Remove(mainTabPage1);

            comboBox1.SelectedIndex = 0;
            //tabControl1.TabPages.Remove(tabPage1);
            //tabControl1.TabPages.Remove(tabPage3);

            InitList();
            QueryGameWindows();

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

        //private List<WindowInfo> GetCheckedWindows()
        //{
        //    List<WindowInfo> checkedWindows = new List<WindowInfo>();

        //    foreach (string item in checkedListBox1.CheckedItems)
        //    {
        //        string[] parts = item.Split(new string[] { " - " }, StringSplitOptions.None);
        //        if (parts.Length >= 2)
        //        {
        //            IntPtr handle = new IntPtr(int.Parse(parts[0]));  // 假设句柄是十六进制的
        //            string title = parts[1];

        //            checkedWindows.Add(new WindowInfo { Handle = handle, Title = title });
        //        }
        //    }

        //    return checkedWindows;
        //}



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





        private void button3_Click(object sender, EventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Process.Start("explorer.exe", currentDirectory);
        }





        int localIndex = 0;
        private void button7_Click(object sender, EventArgs e)
        {
            //api.handleCollection(hWnd, "矿石");
            api.MoveCollectionLocation("矿石", localIndex++);


        }

        private void button11_Click(object sender, EventArgs e)
        {

            QueryGameWindows();
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
            var currentClient = ClientManager.CurrentSelectedClient;
            currentClient.Action = "采集" + comboBox1.Text;
            // 刷新 DataGridView 以更新显示

            dataGridView1.Refresh();

        }

        int isTableLoad = 0;

        private void InitializeDataGridView()
        {
            // 清空现有的列配置
            dataGridView1.Columns.Clear();

            // 添加列配置
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HWnd",
                HeaderText = "窗口句柄",
                DataPropertyName = "HWnd", // 数据绑定的属性名
                ReadOnly = true,
                Width = 150
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "客户端ID",
                DataPropertyName = "Id", // 数据绑定的属性名
                ReadOnly = true,
                Width = 100
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Title",
                HeaderText = "窗口标题",
                DataPropertyName = "Title", // 数据绑定的属性名
                ReadOnly = true,
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Action",
                HeaderText = "当前动作",
                DataPropertyName = "Action", // 数据绑定的属性名
                ReadOnly = false,
                Width = 150
            });

            dataGridView1.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsLead",
                HeaderText = "是否队长",
                DataPropertyName = "IsLead", // 数据绑定的属性名
                ReadOnly = false,
                Width = 120
            });

            // 设置表格样式（可选）
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            isTableLoad++;
        }
        public void QueryGameWindows()
        {
            // 暂时移除事件绑定
            dataGridView1.SelectionChanged -= dataGridView1_SelectionChanged;

            try
            {
                dataGridView1.DataSource = null; // 清空已有数据
                dataGridView1.Rows.Clear(); // 清空 DataGridView 的行，确保每次刷新时数据是最新的

                // 初始化 DataGridView 列
                InitializeDataGridView();


                // 获取客户端列表
                windowApi = new WindowApi();
                var windows = windowApi.UpdateWindowTitles("Galaxy2DEngine");  // 以 Galaxy2DEngine 窗口为例


                ClientManager.InitializeActionMap();
                ClientManager.ClearAllTasks();
                ClientManager.Clients.Clear();

                int index = 0;
                foreach (var window in windows)
                {
                    string itemText = $"{window.Handle} - {window.Title}";
                    LogService.Debug(itemText);

                    var client = new Client(hWnd: window.Handle, id: index.ToString(), title: window.Title, isLead: false);
                    client.StartMonitoring();
                    ClientManager.Clients.Add(client);
                    dataGridView1.Rows.Add(client.HWnd, client.Id, client.Title, client.Action, client.IsLead);
                }

                if (ClientManager.Clients.Count > 0)
                {
                    ClientManager.CurrentSelectedClient = ClientManager.Clients[0];
                }
                dataGridView1.DataSource = ClientManager.Clients;
            }
            finally
            {
                // 恢复事件绑定
                dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            }
        }

        private void StartCollecting()
        {
            //var checkedWindows = GetCheckedWindows();
            //apis.Clear();  // 清除旧的apis列表

            //foreach (var window in checkedWindows)
            //{
            //    apis.Add(new Method(window.Handle, this));
            //}
            //timerCollect = new System.Windows.Forms.Timer();  // 更改为使用 System.Windows.Forms.Timer
            //timerCollect.Interval = 2000;
            //timerCollect.Tick += OnTimedEvent;
            //timerCollect.Start();
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




        private void button12_Click_2(object sender, EventArgs e)
        {
            var wabao = new ActionTreasureMethod(hWnd);
            wabao.Run();
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

        private async void button20_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    //var login = new LoginMethod("584363361", "123321000", 0, this);
                    var login = new LoginMethod("513727961", "7213939", 0, this);
                    login.LoginGame();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button40.Enabled = true; // 恢复按钮状态
            }



        }



        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            iniFileHelper.IniWriteValue("gamepath", "path", textBox6.Text);

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


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                LogService.IsDebugEnabled = true;
            }
            else
            {
                LogService.IsDebugEnabled = false;
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            int index = dataGridView1.CurrentRow.Index;
            if (index >= 0)
            {
                var value = dataGridView1.Rows[index].Cells[2].Value.ToString();
                label_current.Text = $"当前选择的角色：{value}";
                ClientManager.CurrentSelectedClient = ClientManager.Clients[index];
            }
            else
            {
                LogService.Debug($"程序错误没有获取到角色数据,dataGridView1.CurrentRow.Index={dataGridView1.CurrentRow.Index}");
            }

        }

        private async void button_dev_1_Click(object sender, EventArgs e)
        {
            var hWnd = ClientManager.CurrentSelectedClient.HWnd;
            var api = new Method(hWnd);
            await api.GameSendMsg("Hello World");

        }


        private void button1_Click(object sender, EventArgs e)
        {
            WindowApi.LogAllWindowHandles(ClientManager.CurrentSelectedClient.HWnd);

        }

        private void mainTabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            ClientManager.CurrentSelectedClient.Action = "";
            dataGridView1.Refresh();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            ToolsFunction.OpenBackPack();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ToolsFunction.CloseBackPack();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            ToolsFunction.UseBagItem("回程符");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ToolsFunction.IsUseQuMoXiang();

        }

        private void button14_Click(object sender, EventArgs e)
        {
            ToolsFunction.MoveToMapByFly("星秀村");
        }

        private void tabPage5_Click(object sender, EventArgs e)
        {

        }

        private void button17_Click(object sender, EventArgs e)
        {
            ToolsFunction.MoveToMapByFly("应天府");
        }

        private void button21_Click(object sender, EventArgs e)
        {
            ToolsFunction.MoveToMapByFly("汴京城");
        }

        private void button22_Click(object sender, EventArgs e)
        {
            ToolsFunction.MoveToMapByFly("清河县");
        }

        private void button23_Click(object sender, EventArgs e)
        {
            ToolsFunction.MoveToMapByFly("阳谷县");

        }
        private async void button40_Click(object sender, EventArgs e)
        {
            button40.Enabled = false; // 禁用按钮，防止重复点击
            try
            {
                await Task.Run(() =>
                {
                    var name = textBox3.Text;
                    ToolsFunction.OpenMapAndMoveToPointAuto(name);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button40.Enabled = true; // 恢复按钮状态
            }
        }

        private void button30_Click(object sender, EventArgs e)
        {
            string outputFile = Path.Combine("D:\\Project\\TTTools", "data", "digits.dat");
            string[] files = Directory.GetFiles("D:\\Project\\TTTools\\data\\xy", "*.png");
            LogService.Debug(files.Length.ToString());
            // 确保顺序为 0-9 和点号（point.png 对应 .）
            files = files.OrderBy(f =>
            {
                string fileName = Path.GetFileNameWithoutExtension(f);
                if (fileName == "point") return 10; // 确保点号排在数字后
                return int.TryParse(fileName, out int result) ? result : int.MaxValue;
            }).ToArray();

            using (BinaryWriter writer = new BinaryWriter(File.Open(outputFile, FileMode.Create)))
            {
                writer.Write(files.Length); // 写入图片数量

                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    char character = fileName == "point" ? '.' : fileName[0]; // 如果是 point.png，字符为 '.'

                    Bitmap bmp = new Bitmap(file);

                    writer.Write(character); // 写入字符
                    writer.Write(bmp.Width); // 写入宽度
                    writer.Write(bmp.Height); // 写入高度

                    // 锁定位图以读取像素数据
                    var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    byte[] pixelData = new byte[bmp.Width * bmp.Height * 4];
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixelData, 0, pixelData.Length);

                    bmp.UnlockBits(data);

                    writer.Write(pixelData); // 写入像素数据
                }
            }

            Console.WriteLine($"Binary file generated successfully: {outputFile}");

        }

        private void button39_Click(object sender, EventArgs e)
        {
            ToolsFunction.BuySomeThing("驱魔香", 20);

        }

        private void mainTabPage1_Click(object sender, EventArgs e)
        {

        }

        private void button41_Click(object sender, EventArgs e)
        {
            ToolsFunction.up();

        }

        private void button42_Click(object sender, EventArgs e)
        {
            ToolsFunction.lfet();

        }

        private void button45_Click(object sender, EventArgs e)
        {
            ToolsFunction.down();

        }

        private void button44_Click(object sender, EventArgs e)
        {
            ToolsFunction.center();

        }

        private void button43_Click(object sender, EventArgs e)
        {
            ToolsFunction.right();

        }

        private void button46_Click(object sender, EventArgs e)
        {
            int x = int.Parse(textBox1.Text);
            int y = int.Parse(textBox2.Text);
            string mapName = textBox3.Text;

            ToolsFunction.MapGotoClick(mapName, x, y);
        }

        private void button47_Click(object sender, EventArgs e)
        {
            ToolsFunction.ExportCoordinateDataToText(textBox3.Text);

        }

        private void button48_Click(object sender, EventArgs e)
        {
            ToolsFunction.GetCurrentMapName();
        }

        private void clientBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void button49_Click(object sender, EventArgs e)
        {
            ToolsFunction.WabaoTest();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            ToolsFunction.MoveToMapAsync("星秀村东");

        }

        private void button50_Click(object sender, EventArgs e)
        {
            ToolsFunction.MoveToMapAsync("芒砀山麓");
        }

        private void button31_Click(object sender, EventArgs e)
        {
            ToolsFunction.OpenTask();
        }

        private void button32_Click(object sender, EventArgs e)
        {

        }

        private async void button51_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    ToolsFunction.WabaoTaskAsync();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button51.Enabled = true; // 恢复按钮状态
            }
        }

        private void button52_Click(object sender, EventArgs e)
        {
            ToolsFunction.OpenTask();
        }
    }
}
