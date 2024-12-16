using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkProg
{
    public partial class App : Application
    {
        private static string configFilename = "email-settings.json";
        private static JsonElement? settings = null;
        private static Random r = new Random();

        public static string? GetConfiguration(string name)
        {
            if (settings is null)
            {
                if (!File.Exists(configFilename))
                {
                    MessageBox.Show($"Файл конфигурации '{configFilename}' не найден...",
                        "Операция не может быть выполнена", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                try
                {
                    string jsonContent = File.ReadAllText(configFilename);
                    settings = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                }
                catch
                {
                    MessageBox.Show($"Файл конфигурации '{configFilename}' повреждён и не может быть прочитан...",
                        "Операция не может быть выполнена", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            try
            {
                JsonElement? jsonElement = settings;
                foreach (string key in name.Split(':'))
                {
                    jsonElement = jsonElement?.GetProperty(key);
                }

                if (jsonElement is not null)
                {
                    return jsonElement.Value.ValueKind switch
                    {
                        JsonValueKind.String => jsonElement.Value.GetString(),
                        JsonValueKind.Number => jsonElement.Value.GetRawText(), // Для чисел возвращаем текстовое представление
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        _ => null // Игнорируем другие типы данных
                    };
                }
            }
            catch
            {
                // Если произошла ошибка доступа к элементу JSON
                MessageBox.Show($"Ключ '{name}' не найден в файле конфигурации.",
                    "Операция не может быть выполнена", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return null;
        }

        public static int GetRandomNumber(int from, int to)
        {
            return r.Next(from, to);
        }
    }
}
