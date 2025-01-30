using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using TaskManager.Services;

namespace TaskManager.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? _currentView;

        private readonly AddTaskViewModel? _addTaskViewModel;
        private readonly TaskListViewModel? _taskListViewModel;
        private PrintTaskViewModel? _printTaskViewModel;

        public RelayCommand TaskListViewCommand { get; set; }
        public RelayCommand AddTaskViewCommand { get; set; }
        public RelayCommand PrintTaskViewCommand { get; set; }
        public AsyncRelayCommand SyncTasksCommand { get; set; }

        public MainViewModel()
        {
            _taskListViewModel = new TaskListViewModel(this);
            _addTaskViewModel = new AddTaskViewModel(this, _taskListViewModel);

            CurrentView = _taskListViewModel;

            TaskListViewCommand = new RelayCommand(() => { CurrentView = _taskListViewModel; });
            AddTaskViewCommand = new RelayCommand(() => { CurrentView = _addTaskViewModel; });
            PrintTaskViewCommand = new RelayCommand(() =>
            {
                // При переходе на страницу печати передаем коллекцию задач
                if (_taskListViewModel is TaskListViewModel taskListViewModel)
                {
                    _printTaskViewModel = new PrintTaskViewModel(taskListViewModel.Tasks);
                    CurrentView = _printTaskViewModel;
                }
            });
            SyncTasksCommand = new AsyncRelayCommand(SyncTasks);
        }

        private async Task SyncTasks()
        {
            var apiTasks = await RestApiService.GetTasksFromApiAsync();
            await TaskService.AddTasksWithoutDuplicates(apiTasks);

            await _taskListViewModel!.LoadTasks();
            CurrentView = _taskListViewModel;
        }
    }
}