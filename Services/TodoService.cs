using System.Text.Json;
using TodoLite.Models;

namespace TodoLite.Services
{
    public class TodoService
    {
        private readonly string _todoFile;

        public TodoService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "data");
            Directory.CreateDirectory(dataDir);
            _todoFile = Path.Combine(dataDir, "todos.json");
        }

        public async Task<List<TodoItem>> LoadTodos()
        {
            if (!File.Exists(_todoFile)) return new List<TodoItem>();
            return JsonSerializer.Deserialize<List<TodoItem>>(await File.ReadAllTextAsync(_todoFile)) ?? new();
        }

        public async Task SaveTodos(List<TodoItem> list)
        {
            await File.WriteAllTextAsync(_todoFile,
                JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
