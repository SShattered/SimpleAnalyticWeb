using SimpleAnalyticApp.Models;

namespace SimpleAnalyticApp.Services
{
    public interface ITaskService
    {
        Task<List<TaskModel>> GetTasks();
        Task<TaskModel> GetTask(int id);
        Task AddTask(TaskModel task);
        Task UpdateTask(TaskModel task);
        Task DeleteTask(int id);
    }
}
