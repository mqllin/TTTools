using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TTTools
{
    public class InjectorUI
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public static void ShowUI(IntPtr targetHandle)
        {
            Rect targetRect = new Rect();
            GetWindowRect(targetHandle, ref targetRect);

            Form form = new Form
            {
                Text = "Injection Successful",
                Size = new System.Drawing.Size(300, 200),
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(targetRect.Right, targetRect.Top)
            };

            Button button = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point((form.Width - 80) / 2, (form.Height - 30) / 2),
                Size = new System.Drawing.Size(80, 30)
            };

            button.Click += (sender, e) =>
            {
                // Send TCP data
                string response = SendTCPData("121.41.112.13", 9874);
                MessageBox.Show($"Server Response: {response}");
                form.Close();
            };

            form.Controls.Add(button);
            form.Show();
            SetForegroundWindow(targetHandle);
        }

        private static string SendTCPData(string server, int port)
        {
            byte[] data = new byte[]
            {
                0x05, 0x00, 0x80, 0xCB, 0x92, 0xCD, 0x0D, 0xB0, 0x01
            };

            string response = string.Empty;

            using (TcpClient client = new TcpClient(server, port))
            {
                using (NetworkStream stream = client.GetStream())
                {
                    stream.Write(data, 0, data.Length);

                    byte[] buffer = new byte[256];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }
            }

            return response;
        }
    }
}
