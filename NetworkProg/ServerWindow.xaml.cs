using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkProg
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private Socket? listenSocket;
        private IPEndPoint? endPoint;

        public ServerWindow()
        {
            InitializeComponent();
        }

        private void SwitchServer_Click(object sender, RoutedEventArgs e)
        {
            if(listenSocket == null)
            {
                try
                {
                    IPAddress ip = IPAddress.Parse(HostTextBox.Text);
                    int port = Convert.ToInt32(PortTextBox.Text);
                    endPoint = new IPEndPoint(ip, port);
                }
                catch(Exception ex) 
                {
                    MessageBox.Show("Неправильні параметри конфігурації: ", ex.Message);
                    return;
                }

                listenSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                new Thread(StartServer).Start();    
            }
            else
            {
                listenSocket.Close();
            }

            
        }

        private void StartServer()
        {
            if(listenSocket == null || endPoint == null) 
            {
                MessageBox.Show("Спроба запуску без ініціалізації даних");
                return;
            }

            try
            {
                listenSocket.Bind(endPoint);
                listenSocket.Listen(10);
                Dispatcher.Invoke(() =>
                {
                    ServerLog.Text += "Сервер запущен\n";
                    StatusLabel.Background = new SolidColorBrush(Colors.LightGreen);
                    StatusLabel.Content = "ON";
                    SwitchServer.Content = "Вимкнути";
                });
                //Dispatcher.Invoke(() => StatusLabel.Background = new SolidColorBrush(Colors.LightGreen));
                //Dispatcher.Invoke(() => StatusLabel.Content = "ON");

                byte[] buffer = new byte[1024];
                while(true) 
                {
                    Socket socket = listenSocket.Accept();
                    StringBuilder stringBuilder = new StringBuilder();

                    do
                    {
                        int n = socket.Receive(buffer);
                        stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, n));
                    } while (socket.Available > 0);

                    string str = stringBuilder.ToString();
                    Dispatcher.Invoke(() => ServerLog.Text += $"{DateTime.Now} {str}\n");
                }
            }
            catch (Exception ex) 
            {
                listenSocket = null;
                Dispatcher.Invoke(() =>
                {
                    ServerLog.Text += "Сервер зупинено\n";
                    StatusLabel.Background = new SolidColorBrush(Colors.Red);
                    StatusLabel.Content = "OFF";
                    SwitchServer.Content = "Увімкнути";
                });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listenSocket?.Close();
        }
    }

    
}
