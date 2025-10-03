using Microsoft.EntityFrameworkCore;
using TodoLite.Data;
using TodoLite.Models;

namespace TodoLite.Services
{
    public class TodoService
    {
        private readonly AppDbContext _context;

        public TodoService(AppDbContext context)
        {
            _context = context;
        }

        // Tüm todoları getir
        public async Task<List<TodoItem>> GetTodosAsync()
        {
            return await _context.Todos.ToListAsync();
        }

        // Id ile todo getir
        public async Task<TodoItem?> GetTodoByIdAsync(Guid id)
        {
            return await _context.Todos.FindAsync(id);
        }

        // Yeni todo ekle
        public async Task AddTodoAsync(TodoItem todo)
        {
            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
        }

        // Todo güncelle
        public async Task UpdateTodoAsync(TodoItem todo)
        {
            _context.Todos.Update(todo);
            await _context.SaveChangesAsync();
        }

        // Todo sil
        public async Task DeleteTodoAsync(Guid id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo != null)
            {
                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();
            }
        }

        // Kullanıcıya ait todoları getir
        public async Task<List<TodoItem>> GetTodosByUserAsync(string userId)
        {
            return await _context.Todos
                .Where(t => t.CreatorUserId == userId)
                .ToListAsync();
        }
    }
}
