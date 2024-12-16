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
    public partial class ClientWindow : Window
    {
        private Random r = new();
        private IPEndPoint? endPoint;
        private DateTime lastSyncMoment; 
        private bool isTryConnectServer;  
        private CancellationTokenSource? cts;

        public ClientWindow()
        {
            InitializeComponent();
            lastSyncMoment = default;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxLogin.Text = "User" + r.Next(100);
            textBoxMessage.Text = "Some text";
            isTryConnectServer = true;
            cts = new();
            Sync(cts.Token);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            cts?.Cancel();
        }


        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string[] address = textBoxHost.Text.Split(':');
            try
            {
                endPoint = new IPEndPoint(IPAddress.Parse(address[0]), Convert.ToInt32(address[1]));
                new Thread(SendMessage).Start(
                    new ClientRequest
                    {
                        Command = "Message",
                        ChatMessage = new ChatMessage()
                        {
                            Login = textBoxLogin.Text,
                            Text = textBoxMessage.Text,
                        }
                    }
                );
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async void Sync(CancellationToken token)
        {
            if (isTryConnectServer)
            {
                string[] address = textBoxHost.Text.Split(':');
                try
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(address[0]), Convert.ToInt32(address[1]));
                    new Thread(SendMessage).Start(
                        new ClientRequest
                        {
                            Command = "Check",
                            ChatMessage = new ChatMessage() { Moment = lastSyncMoment }
                        }
                    );
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            await Task.Delay(1000);

            if (token!.IsCancellationRequested) return;
            else Sync(token);
        }

        private void SendMessage(object? arg)
        {
            var clientRequest = arg as ClientRequest;
            if (endPoint is null || clientRequest is null) return;

            Socket clientSocket = new(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            try
            {
                clientSocket.Connect(endPoint);
                clientSocket.Send(
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(clientRequest))
                );

                MemoryStream memoryStream = new();
                byte[] buffer = new byte[1024];
                do
                {
                    int n = clientSocket.Receive(buffer);
                    memoryStream.Write(buffer, 0, n);
                } while (clientSocket.Available > 0);
                string str = Encoding.UTF8.GetString(memoryStream.ToArray());

                ServerResponse? response = null;
                //try { response = JsonSerializer.Deserialize<ServerResponse>(str); } catch { }
                try
                {
                    response = JsonSerializer.Deserialize<ServerResponse>(str);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка десериализации: " + ex.Message);
                }
                if (response is null)
                {
                    str = "JSON Error in " + str;
                    CheckSendStatus(false);
                }
                else
                {
                    str = "";
                    if (response.Messages is not null)
                    {
                        foreach (var message in response.Messages)
                        {
                            str += message.ToString() + " (" + message.GetSendTime() + ")" + "\n";
                            if (message.Moment > lastSyncMoment)
                            {
                                lastSyncMoment = message.Moment;
                            }
                        }
                    }
                    else
                    {
                        if (response.Status != "200 OK") CheckSendStatus(false);
                        else CheckSendStatus();
                    }
                }

                Dispatcher.Invoke(() => clientLog.Text = str);

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                if (isTryConnectServer)
                {
                    isTryConnectServer = false;
                    clientSocket.Dispose();
                    MessageBox.Show(ex.Message);
                    isTryConnectServer = true;
                }
            }
        }


        private async void CheckSendStatus(bool status = true)
        {
            Dispatcher.Invoke(() => statusLabel.Visibility = Visibility.Visible);
            if (status)
            {
                Dispatcher.Invoke(() =>
                {
                    statusLabel.Background = Brushes.Green;
                    statusLabel.Content = "Отправлено";
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    statusLabel.Background = Brushes.Pink;
                    statusLabel.Content = "Ошибка";
                });
            }
            await Task.Delay(3000);
            Dispatcher.Invoke(() => statusLabel.Visibility = Visibility.Hidden);
        }
    }
}
