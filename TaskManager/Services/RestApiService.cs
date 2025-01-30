using Serilog;
using System.Net.Http;
using System.Text.Json;
using TaskManager.Model;

namespace TaskManager.Services
{
    /// <summary>
    /// Сервис для взаимодействия с REST API JSONPlaceholder.
    /// Позволяет загружать задачи из удалённого источника.
    /// </summary>
    public class RestApiService
    {
        // Экземпляр HttpClient
        private static readonly HttpClient _httpClient = new();

        // Опции сериализации JSON
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Получает список задач с удалённого API.
        /// </summary>
        /// <returns>Список задач или пустой список в случае ошибки.</returns>
        public static async Task<List<TaskModel>> GetTasksFromApiAsync()
        {
            try
            {
                string url = "https://jsonplaceholder.typicode.com/todos";

                // Выполняем HTTP запрос
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Бросает исключение, если код состояния не 2xx

                // Читаем JSON ответ
                string json = await response.Content.ReadAsStringAsync();

                // Десериализуем JSON в список RestApiTaskModel
                List<RestApiTaskModel>? apiTasks = JsonSerializer.Deserialize<List<RestApiTaskModel>>(json, _jsonOptions);

                // Если десериализация не удалась, возвращаем пустой список
                if (apiTasks is null)
                    return [];

                // Преобразуем RestApiTaskModel в TaskModel
                List<TaskModel> tasks = [];
                foreach (var apiTask in apiTasks)
                {
                    tasks.Add(new TaskModel
                    {
                        Name = apiTask.Title,
                        Description = "Задача из API", // В API нет описания, добавляем заглушку
                        Status = apiTask.Completed ? "Завершено" : "В процессе",
                        CreateDate = DateTime.Now, // Используем текущее время как дату создания
                        Deadline = DateTime.Now.AddDays(7) // Условный дедлайн через 7 дней
                    });
                }

                return tasks;
            }
            catch (HttpRequestException ex) // Ошибки HTTP запроса
            {
                Log.Error($"Ошибка HTTP-запроса: {ex.Message}");
            }
            catch (TaskCanceledException ex) // Таймаут или отмена запроса
            {
                Log.Error($"Запрос был отменён или истекло время ожидания: {ex.Message}");
            }
            catch (JsonException ex) // Ошибка при разборе JSON неверный формат
            {
                Log.Error($"Ошибка десериализации JSON: {ex.Message}");
            }
            catch (NotSupportedException ex) // если JSON-сериализация невозможна
            {
                Log.Error($"Ошибка обработки данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Произошла неожиданная ошибка: {ex.Message}");
            }

            return [];
        }
    }
}