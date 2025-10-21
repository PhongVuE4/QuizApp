using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UI.Services
{
    public class LocalStorageService
    {
        private readonly string _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuizAppData");
        public LocalStorageService()
        {
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }
        private string GetFilePath(string name) => Path.Combine(_basePath, $"{name}.json");
        public async Task SaveAsync<T>(string name, List<T> data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(GetFilePath(name), json);
        }
        public async Task<List<T>> LoadAsync<T>(string name)
        {
            string path = GetFilePath(name);
            if (!File.Exists(path)) return new List<T>();

            string json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        public bool HasLocalData(string name)
        {
            return File.Exists(GetFilePath(name));
        }
    }
}