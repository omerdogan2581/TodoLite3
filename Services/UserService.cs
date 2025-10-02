using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TodoLite.Models;

namespace TodoLite.Services
{
    public class UserService
    {
        private readonly string _userFile;

        public UserService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "data");
            Directory.CreateDirectory(dataDir);
            _userFile = Path.Combine(dataDir, "users.json");
        }

        public async Task<List<User>> LoadUsers()
        {
            if (!File.Exists(_userFile)) return new List<User>();
            return JsonSerializer.Deserialize<List<User>>(await File.ReadAllTextAsync(_userFile)) ?? new();
        }

        public async Task SaveUsers(List<User> list)
        {
            await File.WriteAllTextAsync(_userFile,
                JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }

        public string HashPassword(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
