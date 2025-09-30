using System.Text.Json;
using TodoLite.Models;

public class FileTodoStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly JsonSerializerOptions _opt = new() { WriteIndented = true };

    public FileTodoStore(IHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "todos.json");
        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }

    public async Task<List<TodoItem>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<TodoItem>>(json) ?? new();
        }
        finally { _lock.Release(); }
    }

    private async Task SaveAsync(List<TodoItem> items)
    {
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(items, _opt);
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally { _lock.Release(); }
    }

    public async Task<TodoItem> AddAsync(TodoItem t)
    {
        var all = await GetAllAsync();
        all.Add(t);
        await SaveAsync(all);
        return t;
    }

    public async Task<TodoItem?> UpdateAsync(Guid id, string? text, string? status)
    {
        var all = await GetAllAsync();
        var todo = all.FirstOrDefault(x => x.Id == id);
        if (todo == null) return null;

        if (!string.IsNullOrWhiteSpace(text))
            todo.Text = text;

        if (!string.IsNullOrWhiteSpace(status))
            todo.Status = status;

        await SaveAsync(all);
        return todo;
    }



    public async Task<bool> DeleteAsync(Guid id)
    {
        var all = await GetAllAsync();
        var removed = all.RemoveAll(x => x.Id == id) > 0;
        if (removed) await SaveAsync(all);
        return removed;
    }
}
