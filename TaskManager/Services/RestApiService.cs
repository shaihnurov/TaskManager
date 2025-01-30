using System.Net.Http;
using System.Text.Json;
using TaskManager.Model;

namespace TaskManager.Services
{
    public class RestApiService
    {
        private static readonly HttpClient _httpClient = new();

        // Кэшируем JsonSerializerOptions
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<List<TaskModel>> GetTasksFromApiAsync()
        {
            try
            {
                string url = "https://jsonplaceholder.typicode.com/todos";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                List<RestApiTaskModel>? apiTasks = JsonSerializer.Deserialize<List<RestApiTaskModel>>(json, _jsonOptions);

                if (apiTasks is null)
                    return [];

                // Преобразуем в TaskModel
                List<TaskModel> tasks = [];
                foreach (var apiTask in apiTasks)
                {
                    tasks.Add(new TaskModel
                    {
                        Name = apiTask.Title,
                        Description = "Задача из API", // Заглушка для описания
                        Status = apiTask.Completed ? "Завершено" : "В процессе",
                        CreateDate = DateTime.Now,
                        Deadline = DateTime.Now.AddDays(7) // Заглушка для дедлайна
                    });
                }

                return tasks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
                return [];
            }
        }
    }
}