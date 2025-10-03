using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TodoLite.Data;
using TodoLite.Models;

namespace TodoLite.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // Tüm kullanıcıları getir
        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // Id ile kullanıcı getir
        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        // Username ile kullanıcı getir
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        // Yeni kullanıcı ekle
        public async Task AddUserAsync(User user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash); // şifreyi hashle
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Kullanıcı güncelle
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // Kullanıcı sil
        public async Task DeleteUserAsync(string id)
        {
            var user = await GetUserByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        // Şifreyi hashle
        public string HashPassword(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // Kullanıcı doğrulama (login için)
        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var hash = HashPassword(password);
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash);
        }
    }
}
