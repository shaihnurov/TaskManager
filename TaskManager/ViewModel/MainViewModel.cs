using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Net.Http;
using System.Windows;
using TaskManager.Services;

namespace TaskManager.ViewModel
{
    /// <summary>
    /// Главное ViewModel, управляющее навигацией между страницами и синхронизацией задач
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? _currentView;

        private readonly AddTaskViewModel? _addTaskViewModel;
        private readonly TaskListViewModel? _taskListViewModel;
        private PrintTaskViewModel? _printTaskViewModel;
        private TasksStatisticsViewModel? _tasksStatisticsViewModel;

        public RelayCommand TaskListViewCommand { get; set; }
        public RelayCommand AddTaskViewCommand { get; set; }
        public RelayCommand PrintTaskViewCommand { get; set; }
        public RelayCommand TasksStatisticsViewCommand { get; set; }
        public AsyncRelayCommand SyncTasksCommand { get; set; }

        public MainViewModel()
        {
            try
            {
                _taskListViewModel = new TaskListViewModel(this);
                _addTaskViewModel = new AddTaskViewModel(this, _taskListViewModel);

                CurrentView = _taskListViewModel;

                TaskListViewCommand = new RelayCommand(() => { CurrentView = _taskListViewModel; });
                AddTaskViewCommand = new RelayCommand(() => { CurrentView = _addTaskViewModel; });

                PrintTaskViewCommand = new RelayCommand(() =>
                {
                    try
                    {
                        // При переходе на страницу печати передаем коллекцию задач
                        if (_taskListViewModel is TaskListViewModel taskListViewModel)
                        {
                            _printTaskViewModel = new PrintTaskViewModel(taskListViewModel.Tasks);
                            CurrentView = _printTaskViewModel;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при переключении на печать задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Log.Error($"Ошибка PrintViewModel: {ex.Message}");
                    }
                });
                TasksStatisticsViewCommand = new RelayCommand(() =>
                {
                    try
                    {
                        if (_taskListViewModel is TaskListViewModel taskListViewModel)
                        {
                            _tasksStatisticsViewModel = new TasksStatisticsViewModel(taskListViewModel.Tasks);
                            CurrentView = _tasksStatisticsViewModel;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при переключении на статистику задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Log.Error($"Ошибка TasksStatisticsView: {ex.Message}");
                    }
                });


                SyncTasksCommand = new AsyncRelayCommand(SyncTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error($"Ошибка в конструкторе MainViewModel: {ex.Message}");
            }
        }

        // Команда для синхронизации задач с API.
        private async Task SyncTasks()
        {
            try
            {
                // Получаем задачи с API
                var apiTasks = await RestApiService.GetTasksFromApiAsync();

                // Добавляем их в базу
                await TaskService.AddTasksWithoutDuplicates(apiTasks);

                // Перезагружаем задачи в списке
                await _taskListViewModel!.LoadTasks();

                // После обновления переключаемся на список задач
                CurrentView = _taskListViewModel;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка сети при загрузке задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error($"HttpRequestException: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("Превышено время ожидания запроса к API.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                Log.Error($"TaskCanceledException: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка синхронизации задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error($"Exception: {ex.Message}");
            }
        }
    }
}