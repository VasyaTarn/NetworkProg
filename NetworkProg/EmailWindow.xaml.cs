using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Mime;

namespace NetworkProg
{
    public partial class EmailWindow : Window
    {
        public EmailWindow()
        {
            InitializeComponent();
            StartSettingsTextBox();
        }

        private SmtpClient? GetSmtpClient()
        {
            string? host = App.GetConfiguration("smtp:host");
            if (host is null) { MessageBox.Show("Error getting host..."); return null; }

            string? portString = App.GetConfiguration("smtp:port");
            if (portString is null) { MessageBox.Show("Error getting port..."); return null; }
            int port;
            try { port = Convert.ToInt32(portString); }
            catch { MessageBox.Show("Error parsing port..."); return null; }

            string? email = App.GetConfiguration("smtp:email");
            if (email is null) { MessageBox.Show("Error getting email..."); return null; }

            string? password = App.GetConfiguration("smtp:password");
            if (password is null) { MessageBox.Show("Error getting password..."); return null; }

            string? sslString = App.GetConfiguration("smtp:ssl");
            if (sslString is null) { MessageBox.Show("Error getting ssl..."); return null; }
            bool ssl;
            try { ssl = Convert.ToBoolean(sslString); }
            catch { MessageBox.Show("Error parsing ssl..."); return null; }

            if (!textBoxTo.Text.Contains("@")) { MessageBox.Show("Введите правильный email"); return null; }

            return new(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
        }


        private void BtnSendButton1_Click(object sender, RoutedEventArgs e)
        {
            SmtpClient? smtpClient = GetSmtpClient();
            if (smtpClient is null) { return; }
            smtpClient.Send(
                App.GetConfiguration("smtp:email")!,
                textBoxTo.Text,
                textBoxSubject.Text,
                textBoxContent.Text
            );
            MessageBox.Show("Sent!");
        }


        private void BtnSendButton2_Click(object sender, RoutedEventArgs e)
        {
            SmtpClient? smtpClient = GetSmtpClient();
            if (smtpClient is null) { return; }

            MailMessage mailMessage = new(
                App.GetConfiguration("smtp:email")!,
                textBoxTo.Text,
                textBoxSubject.Text,
                textBoxHtml.Text)
            {
                IsBodyHtml = true
            };

            ContentType pngType = new("image/png");
            mailMessage.Attachments.Add(new Attachment("mob.png", pngType));

            ContentType mp3Type = new("audio/mpeg");
            mailMessage.Attachments.Add(new Attachment("coin.mp3", mp3Type));

            smtpClient.Send(mailMessage);
            MessageBox.Show("Sent!");
        }


        private void StartSettingsTextBox()
        {
            textBoxHtmlHW.Text = $"<h2>Confirm code: <b style='color:tomato'>{App.GetRandomNumber(100000, 999999)}</b>";
        }

        private void BtnSendButtonHW_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SmtpClient? smtpClient = GetSmtpClient();
                if (smtpClient is null) { return; }

                MailMessage mailMessage = new(
                    App.GetConfiguration("smtp:email")!,
                    textBoxTo.Text,
                    textBoxSubject.Text,
                    textBoxHtmlHW.Text)
                {
                    IsBodyHtml = true
                };

                ContentType txtType = new("text/plain");
                mailMessage.Attachments.Add(new Attachment("privacy.txt", txtType));

                ContentType docType = new("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                mailMessage.Attachments.Add(new Attachment("privacy.docx", docType));

                smtpClient.Send(mailMessage);
                MessageBox.Show("Sent!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
