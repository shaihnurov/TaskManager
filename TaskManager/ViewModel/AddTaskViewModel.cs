using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using TaskManager.Model;
using TaskManager.Services;

namespace TaskManager.ViewModel
{
    public partial class AddTaskViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TaskListViewModel _taskListViewModel;

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private DateTime _deadline = DateTime.Now.AddDays(1);

        [ObservableProperty]
        private string _status = "В процессе";

        public ObservableCollection<string> Statuses { get; } = ["В процессе", "Завершено"];

        public AddTaskViewModel(MainViewModel mainViewModel, TaskListViewModel taskListViewModel)
        {
            _mainViewModel = mainViewModel;
            _taskListViewModel = taskListViewModel;
        }

        [RelayCommand]
        private async Task AddTask()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(Status))
                return;

            var newTask = new TaskModel
            {
                Name = Name,
                Description = Description,
                Deadline = Deadline,
                CreateDate = DateTime.Now,
                Status = Status
            };

            await TaskService.AddTask(newTask);
            _taskListViewModel.Tasks.Add(newTask);
            _taskListViewModel.FilteredTasks.Add(newTask);
            _mainViewModel.CurrentView = _taskListViewModel;

            // Очистка полей после добавления
            Name = string.Empty;
            Description = string.Empty;
            Deadline = DateTime.Now.AddDays(1);
            Status = "В процессе";
        }
    }
}