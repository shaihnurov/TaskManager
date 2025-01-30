using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TaskManager.Model;
using TaskManager.Services;

namespace TaskManager.ViewModel
{
    public partial class EditTaskViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TaskListViewModel _taskListViewModel;

        private readonly int _id;

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private DateTime _deadline;

        [ObservableProperty]
        private string _status;

        public ObservableCollection<string> Statuses { get; } = ["В процессе", "Завершено"];

        public EditTaskViewModel(TaskModel task, MainViewModel mainViewModel, TaskListViewModel taskListViewModel)
        {
            _mainViewModel = mainViewModel;
            _taskListViewModel = taskListViewModel;

            _id = task.Id;
            Name = task.Name;
            Description = task.Description;
            Deadline = task.Deadline;
            Status = task.Status!;
        }

        // Команда для обновления задачи
        [RelayCommand]
        private async Task UpdateTask()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(Status))
                return;

            var updatedTask = new TaskModel
            {
                Id = _id,
                Name = Name,
                Description = Description,
                Deadline = Deadline,
                Status = Status
            };

            await TaskService.UpdateTask(updatedTask);

            _taskListViewModel.UpdateTask(updatedTask);
            _mainViewModel.CurrentView = _taskListViewModel;
        }
    }
}
