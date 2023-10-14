using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Text;

namespace TTTools
{
    class TcpClientWrapper
    {
        private TcpClient client;
        private NetworkStream stream;

        public TcpClientWrapper(string serverIp, int port)
        {
            // 创建 TcpClient 并连接到服务器
            client = new TcpClient(serverIp, port);
            // 获取用于读写的 NetworkStream
            stream = client.GetStream();
        }

        public void SendData(string data)
        {
            // 将字符串转换为字节
            byte[] bytesToSend = StringToByteArray(data);

            // 发送数据
            stream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        public string ReceiveData()
        {
            // 创建接收缓冲区
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            // 将字节转换为字符串
            string receivedData = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", " ");
            return receivedData;
        }

        public void Close()
        {
            // 关闭 NetworkStream 和 TcpClient
            stream.Close();
            client.Close();
        }

        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            byte[] result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }
    }
}
