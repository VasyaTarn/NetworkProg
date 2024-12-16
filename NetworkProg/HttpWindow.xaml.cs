using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
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
    public partial class HttpWindow : Window
    {
        private List<NbuRate>? rates;
        private static string[] popularRates = { "EUR", "USD", "XAU" };

        public HttpWindow()
        {
            InitializeComponent();
        }

        private async void ShowContentFromResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                textBlock1.Text = await response.Content.ReadAsStringAsync() + "\r\n";
            }
            else
            {
                MessageBox.Show($"Error: {(int)response.StatusCode} {response.ReasonPhrase}\r\n");
            }
        }


        private async void Get1Button_Click(object sender, RoutedEventArgs e)
        {
            using HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync("https://itstep.org/uk");

            textBlock1.Text = "";
            textBlock1.Text += (int)response.StatusCode + " " + response.ReasonPhrase + "\r\n";
            foreach (var header in response.Headers)
            {
                textBlock1.Text += $"{header.Key,-20}: " + String.Join(',', header.Value) + "\r\n";
            }

            string body = await response.Content.ReadAsStringAsync();
            textBlock1.Text += $"\r\n{body}";
        }

        private async void RatesXML_Click(object sender, RoutedEventArgs e)
        {
            using HttpClient httpClient = new();
            var response = await httpClient.GetAsync("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange");
            ShowContentFromResponse(response);
        }

        private async void RatesJSON_Click(object sender, RoutedEventArgs e)
        {
            using HttpClient httpClient = new();
            var response = await httpClient.GetAsync("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json");
            ShowContentFromResponse(response);
        }


        private async void RatesButton_Click(object sender, RoutedEventArgs e)
        {
            if (rates is null)
            {
                await LoadRatesAsync();
            }
            if (rates is null)
            {
                return;
            }
            textBlock1.Text = "";
            foreach (var rate in rates)
            {
                textBlock1.Text += $"{rate.cc} {rate.txt} {rate.rate} \r\n";
            }
        }

        private async void PopularButton_Click(object sender, RoutedEventArgs e)
        {
            if (rates is null)
            {
                await LoadRatesAsync();
            }
            if (rates is null)
            {
                return;
            }
            textBlock1.Text = "";
            foreach (var rate in rates)
            {
                if (popularRates.Contains(rate.cc))
                {
                    textBlock1.Text += $"{rate.cc} {rate.txt} {rate.rate}\r\n";
                }
            }
        }

        private async Task LoadRatesAsync()
        {
            using HttpClient httpClient = new();
            string body = await httpClient.GetStringAsync(@"https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json");

            // ORM (в данном контексте) - преобразование string body в объекты
            rates = JsonSerializer.Deserialize<List<NbuRate>>(body);
            if (rates is null)
            {
                MessageBox.Show("Error deserializing");
                return;
            }
        }
    }

    class NbuRate
    {
        public int r030 { get; set; }
        public string txt { get; set; } = null!;
        public double rate { get; set; }
        public string cc { get; set; } = null!;
        public string exchangedate { get; set; } = null!;
    }

    public static class EllipsisExtensions
    {
        public static string Ellipsis(this string str, int maxLength)
        {
            return str.Length > maxLength ? str[..(maxLength - 3)] + "..." : str;
        }
    }
}
