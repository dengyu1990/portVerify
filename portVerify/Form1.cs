using System;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Text;
using System.Threading;

namespace portVerify
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            btnDetect.Enabled = false;
            string ip = txtIP.Text.Trim();
            int port;
            bool isPort = false;
            isPort = Int32.TryParse(txtPort.Text.Trim(), out port);
            string regformat = @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(ip) && ip.Length >= 7 && ip.Length <= 15 && regex.IsMatch(ip))
            {
                if (isPort && port >= 0 && port <= 65535)
                {
                    //验证通过
                    try
                    {
                        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socket.Connect(IPAddress.Parse(ip), port);
                        txtResult.AppendText(string.Format("检测到该通道（{0}:{1}）可用，返回数据：", ip, port));
                        Thread thread = new Thread(new ParameterizedThreadStart(ReceivedData));
                        thread.IsBackground = true;
                        thread.Start(socket);

                    }
                    catch(Exception ex)
                    {
                        txtResult.AppendText("检测到该通道不可用，报错信息：\n" + ex.ToString() + "\n");
                        btnDetect.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("您输入的端口不规范，请重新输入\n", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnDetect.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("您输入的IP地址不规范，请重新输入\n", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnDetect.Enabled = true;
            }
        }

        public void ReceivedData(object socket)
        {
            var proxSocket = socket as Socket;
            byte[] data = new byte[1024 * 1024];
            //while (true)
            {
                int len = proxSocket.Receive(data, 0, data.Length, SocketFlags.None);
                string recStr = Encoding.Default.GetString(data, 0, len);
                lblIP.Invoke(new Action<string>(s =>
                {
                    txtResult.AppendText(s + "\n");
                    btnDetect.Enabled = true;
                }), recStr);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtResult.AppendText("验证工具初始化完成...\n");
            try
            {
                string HostName = Dns.GetHostName();
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                        txtResult.AppendText(string.Format("本机IP地址：{0}\n", IpEntry.AddressList[i].ToString()));
                }
            }
            catch (Exception ex)
            {
                txtResult.AppendText(ex.ToString());
            }
        }
    }
}
