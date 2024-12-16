using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
using System.Xml;

namespace NetworkProg
{
    public partial class CryptoWindow : Window
    {
        private readonly HttpClient _httpClient;
        public ObservableCollection<CoinData> CoinsData { get; set; }
        private ListViewItem? previousSelectedItem;

        public CryptoWindow()
        {
            InitializeComponent();

            CoinsData = new();
            DataContext = this;
            _httpClient = new() { BaseAddress = new Uri("https://api.coincap.io/") };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => LoadAssetsAsync());
        }


        private async Task LoadAssetsAsync()
        {
            var response = JsonSerializer.Deserialize<CoinCapResponse>(
                                await _httpClient.GetStringAsync("/v2/assets?limit=10")
                            );
            if (response is null)
            {
                MessageBox.Show("Deserialization error");
                return;
            }

            Dispatcher.Invoke(() => CoinsData.Clear());
            foreach (var coinData in response.data)
            {
                Dispatcher.Invoke(() => CoinsData.Add(coinData));
            }
        }

        private async void CoinData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                if (item.Content is CoinData coinData)
                {
                    if (previousSelectedItem is not null)
                    {
                        previousSelectedItem.Background = Brushes.White;
                    }
                    item.Background = Brushes.Aqua;
                    previousSelectedItem = item;

                    await ShowHistory(coinData);
                }
            }
        }

        private async Task ShowHistory(CoinData coinData)
        {
            var response = JsonSerializer.Deserialize<HisoryResponse>(
                await _httpClient.GetStringAsync($"/v2/assets/{coinData.id}/history?interval=d1")
            );

            if (response is null) { MessageBox.Show("Error deserialize 'HisoryResponse'"); return; }

            graphCanvas.Children.Clear();

            long minTime, maxTime;
            double minPrice, maxPrice;
            minTime = maxTime = response.data[0].time;
            minPrice = maxPrice = response.data[0].price;
            foreach (HistoryItem item in response.data)
            {
                if (item.time < minTime) { minTime = item.time; }
                if (item.time > maxTime) { maxTime = item.time; }
                if (item.price < minPrice) { minPrice = item.price; }
                if (item.price > maxPrice) { maxPrice = item.price; }
            }

            double yOffset = 50;
            double graphH = graphCanvas.ActualHeight - yOffset;

            double x0 = (response.data[0].time - minTime) * graphCanvas.ActualWidth / (maxTime - minTime);
            double y0 = graphH - ((response.data[0].price - minPrice) * graphH / (maxPrice - minPrice));
            double x, y;
            foreach (HistoryItem item in response.data)
            {
                x = (item.time - minTime) * graphCanvas.ActualWidth / (maxTime - minTime);
                y = graphH - ((item.price - minPrice) * graphH / (maxPrice - minPrice));
                DrawLine(x0, y0, x, y);
                x0 = x;
                y0 = y;
            }

            DrawLine(0, graphH, graphCanvas.ActualWidth, graphH, Brushes.Violet);

            DrawLabel(0, graphH + 5, Formatting.GetDateFromSeconds(minTime), Brushes.Green);
            DrawLabel(graphCanvas.ActualWidth - 60, graphH + 5, Formatting.GetDateFromSeconds(maxTime), Brushes.Green);

            DrawLabel(0, 0, Formatting.GetStringFromPrice(minPrice));
            DrawLabel(0, graphH + 25, Formatting.GetStringFromPrice(maxPrice));
        }

        private void DrawLine(double x1, double y1, double x2, double y2, Brush? brush = null)
        {
            brush ??= new SolidColorBrush(Colors.Black);
            graphCanvas.Children.Add(new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = brush,
                StrokeThickness = 2
            });
        }

        private void DrawLabel(double x, double y, string text, Brush? brush = null)
        {
            brush ??= new SolidColorBrush(Colors.Red);
            TextBlock textBlock = new() { Text = text, Foreground = brush, };
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            graphCanvas.Children.Add(textBlock);
        }
    }

    public static class Formatting
    {
        private static DateTime epoch = new(1970, 1, 1, 0, 0, 0);

        public static string GetDateFromSeconds(long seconds)
        {
            return epoch.AddSeconds(seconds / 1000).ToString("dd.MM.yyyy");
        }

        public static string GetStringFromPrice(double price)
        {
            return Math.Round(price, 2).ToString();
        }
    }
    public class HisoryResponse
    {
        public List<HistoryItem> data { get; set; } = null!;
        public long timestamp { get; set; }
    }
    public class HistoryItem
    {
        public string priceUsd { get; set; } = null!;
        public long time { get; set; }
        public double price => double.Parse(priceUsd, CultureInfo.InvariantCulture);
    }

    public class CoinCapResponse
    {
        public List<CoinData> data { get; set; } = null!;
        public long timestamp { get; set; }
    }
    public class CoinData
    {
        public string id { get; set; } = null!;
        public string rank { get; set; } = null!;
        public string symbol { get; set; } = null!;
        public string name { get; set; } = null!;
        public string supply { get; set; } = null!;
        public string maxSupply { get; set; } = null!;
        public string marketCapUsd { get; set; } = null!;
        public string volumeUsd24Hr { get; set; } = null!;
        public string priceUsd { get; set; } = null!;
        public string changePercent24Hr { get; set; } = null!;
        public string vwap24Hr { get; set; } = null!;
        public string explorer { get; set; } = null!;
    }
}
