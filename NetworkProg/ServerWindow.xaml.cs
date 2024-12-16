using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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
    public partial class ServerWindow : Window
    {
        private bool isStartServer = false;
        private Socket? listenSocket; 
        private IPEndPoint? endPoint; 
        private List<ChatMessage> messages;  

        public ServerWindow()
        {
            InitializeComponent();
            messages = new List<ChatMessage>();
            CheckUIStatusState();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listenSocket?.Close();
        }


        private void SwitchServerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (listenSocket is null)
            {
                try
                {
                    IPAddress ip = IPAddress.Parse(textBoxHost.Text);

                    int port = Convert.ToInt32(textBoxPort.Text);

                    endPoint = new IPEndPoint(ip, port);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }

                listenSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

                new Thread(StartServer).Start();
            }
            else
            {
                listenSocket.Close();
            }

            isStartServer = !isStartServer;
            CheckUIStatusState();
        }

        private void StartServer()
        {
            if (listenSocket is null || endPoint is null)
            {
                MessageBox.Show("Попытка запуска без инициализации данных!");
                return;
            }
            try
            {
                listenSocket.Bind(endPoint);
                listenSocket.Listen(10);
                Dispatcher.Invoke(() => serverLog.Text += "Сервер запущен\n");

                byte[] buffer = new byte[1024];
                while (true)
                {
                    Socket socket = listenSocket.Accept();

                    MemoryStream memoryStream = new();
                    do
                    {
                        int n = socket.Receive(buffer);
                        memoryStream.Write(buffer, 0, n);
                    } while (socket.Available > 0);
                    string str = Encoding.UTF8.GetString(memoryStream.ToArray());

                    ServerResponse serverResponse = new();
                    ClientRequest? clientRequest = null;
                    try { clientRequest = JsonSerializer.Deserialize<ClientRequest>(str); } catch { }

                    bool needLog = true;
                    if (clientRequest is null)
                    {
                        str = "Error decoding JSON: " + str;
                        serverResponse.Status = "400 Bad request";
                    }
                    else
                    {
                        if (clientRequest.Command.Equals("Message"))
                        {
                            clientRequest.ChatMessage.Moment = DateTime.Now;

                            messages.Add(clientRequest.ChatMessage);

                            str = clientRequest.ChatMessage.ToString();
                            serverResponse.Status = "200 OK";
                        }
                        else if (clientRequest.Command.Equals("Check"))
                        {
                            serverResponse.Status = "200 OK";
                            serverResponse.Messages = messages;

                            needLog = false;
                        }


                    }
                    if (needLog)
                    {
                        Dispatcher.Invoke(() => serverLog.Text += $"({clientRequest!.ChatMessage.Moment}) {str}\n");
                    }

                    socket.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(serverResponse)));
                    socket.Close();
                }
            }
            catch (Exception)
            {
                listenSocket = null;
                Dispatcher.Invoke(() => serverLog.Text += "Сервер остановлен\n");
            }
        }


        private void CheckUIStatusState()
        {
            CheckStateServerButton();
            CheckStateStatusLabel();
        }

        private void CheckStateServerButton()
        {
            if (isStartServer)
            {
                btnSwitchServer.Content = "Выключить";
                btnSwitchServer.Background = Brushes.Red;
            }
            else
            {
                btnSwitchServer.Content = "Включить";
                btnSwitchServer.Background = Brushes.Green;
            }
        }

        private void CheckStateStatusLabel()
        {
            if (isStartServer)
            {
                statusLabel.Content = "Включено";
                statusLabel.Background = Brushes.Green;
            }
            else
            {
                statusLabel.Content = "Выключено";
                statusLabel.Background = Brushes.Red;
            }
        }
    }

    
}
