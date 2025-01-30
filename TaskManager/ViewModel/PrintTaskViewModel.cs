using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using TaskManager.Model;

namespace TaskManager.ViewModel
{
    public partial class PrintTaskViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<TaskModel> _filteredTasks;

        public PrintTaskViewModel(ObservableCollection<TaskModel> filteredTasks)
        {
            FilteredTasks = filteredTasks;
        }

        [RelayCommand]
        public async Task PrintAsync()
        {
            PrintDialog printDialog = new();

            if (printDialog.ShowDialog() == true)
            {
                // Создаем документ
                FixedDocument document = await Application.Current.Dispatcher.InvokeAsync(() => CreatePrintDocument(printDialog));

                // Отправляем документ на печать
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        printDialog.PrintDocument(document.DocumentPaginator, "Список задач");
                    });
                });
            }
        }
        private FixedDocument CreatePrintDocument(PrintDialog printDialog)
        {
            // Создаем документ для печати
            FixedDocument document = new();
            FixedPage page = new()
            {
                Width = printDialog.PrintableAreaWidth,
                Height = printDialog.PrintableAreaHeight
            };

            // Создаем таблицу для печати
            Table table = new() { CellSpacing = 5 };
            table.Columns.Add(new TableColumn()); // Название задачи
            table.Columns.Add(new TableColumn()); // Описание
            table.Columns.Add(new TableColumn()); // Дата создания
            table.Columns.Add(new TableColumn()); // Дедлайн
            table.Columns.Add(new TableColumn()); // Статус

            // Добавляем заголовок таблицы
            TableRowGroup headerGroup = new();
            TableRow headerRow = new();
            headerRow.Cells.Add(CreateCell("Название", true));
            headerRow.Cells.Add(CreateCell("Описание", true));
            headerRow.Cells.Add(CreateCell("Дата создания", true));
            headerRow.Cells.Add(CreateCell("Дедлайн", true));
            headerRow.Cells.Add(CreateCell("Статус", true));
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            // Добавляем данные задач
            TableRowGroup dataGroup = new();
            foreach (var task in FilteredTasks)
            {
                TableRow row = new();
                row.Cells.Add(CreateCell(task.Name));
                row.Cells.Add(CreateCell(task.Description));
                row.Cells.Add(CreateCell(task.CreateDate.ToString("dd.MM.yyyy")));
                row.Cells.Add(CreateCell(task.Deadline.ToString("dd.MM.yyyy")));
                row.Cells.Add(CreateCell(task.Status));
                dataGroup.Rows.Add(row);
            }
            table.RowGroups.Add(dataGroup);

            // Добавляем таблицу в документ
            FlowDocument flowDocument = new();
            flowDocument.Blocks.Add(table);

            // Вставляем в страницу
            RichTextBox richTextBox = new() { Document = flowDocument, Width = printDialog.PrintableAreaWidth };
            page.Children.Add(richTextBox);

            // Создаем страницу
            PageContent pageContent = new();
            ((IAddChild)pageContent).AddChild(page);
            document.Pages.Add(pageContent);

            return document;
        }
        private static TableCell CreateCell(string? text, bool isHeader = false)
        {
            return new TableCell(new Paragraph(new Run(text ?? "")))
            {
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                BorderThickness = new Thickness(1),
                BorderBrush = System.Windows.Media.Brushes.Black,
                Padding = new Thickness(5)
            };
        }
    }
}