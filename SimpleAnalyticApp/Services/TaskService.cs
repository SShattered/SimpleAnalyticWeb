using Microsoft.EntityFrameworkCore;
using SimpleAnalyticApp.Data;
using SimpleAnalyticApp.Models;

namespace SimpleAnalyticApp.Services
{
    public class TaskService : ITaskService
    {
        public readonly ApplicationDbContext _context;
        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTask(TaskModel task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public Task DeleteTask(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<TaskModel> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                throw new Exception("Task not found");

            return task;
        }

        public async Task<List<TaskModel>> GetTasks()
        {
            var list = await _context.Tasks.ToListAsync();
            return list.OrderBy(x => x.TaskName).ToList();
        }

        public async Task UpdateTask(TaskModel task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }
    }
}
