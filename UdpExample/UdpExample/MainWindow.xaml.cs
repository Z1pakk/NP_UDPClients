using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UdpExample
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket;
        EndPoint epLocal, epRemote;
        public MainWindow()
        {
            InitializeComponent();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            string ip = string.Empty;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var item in host.AddressList)
            {
                if(item.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = item.ToString();
                }
            }
            myIp.Text = friendIp.Text = ip;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Parse(myIp.Text), int.Parse(myPort.Text));
                epRemote = new IPEndPoint(IPAddress.Parse(friendIp.Text), int.Parse(friendPort.Text));

                socket.Bind(epLocal);

                socket.Connect(epRemote);

                byte[] buffer = new byte[1024];
                socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallback), buffer);

                btnStart.IsEnabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void MessageCallback(IAsyncResult aResult)
        {
            int size = socket.EndReceiveFrom(aResult, ref epRemote);
            if(size > 0)
            {
                byte[] receivedData = (byte[])aResult.AsyncState;

                string receivedMessage = Encoding.UTF8.GetString(receivedData);

                this.Dispatcher.Invoke(()=> lbMessages.Items.Add("Friend: " + receivedMessage));
            }

            byte[] buffer = new byte[1024];
            socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallback), buffer);
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {

            socket.Send(Encoding.UTF8.GetBytes(tbMessage.Text));
            lbMessages.Items.Add("You: " + tbMessage.Text);
        }
    }
}
