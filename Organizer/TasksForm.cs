using Microsoft.EntityFrameworkCore;
using Organizer.Models;
using System.Data;

namespace Organizer
{
    public partial class TasksForm : Form
    {
        private readonly TeacherOrganizerContext _dbContext;
        private readonly DateTime _selectedDate;
        private bool _hasTasks = false;

        public TasksForm(TeacherOrganizerContext dbContext, DateTime selectedDate)
        {
            _dbContext = dbContext;
            _selectedDate = selectedDate;
            InitializeComponent();
            InitializeComponents();
            LoadTasks();
        }

        private void InitializeComponents()
        {
            this.Text = $"Задачи на {_selectedDate.ToShortDateString()}";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 255, 240);
            this.Font = new Font("Segoe UI", 10);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            this.Controls.Add(mainPanel);
        }

        private void LoadTasks()
        {
            this.Controls[0].Controls.Clear();

            var startOfDayUtc = _selectedDate.Date.ToUniversalTime();
            var endOfDayUtc = startOfDayUtc.AddDays(1);

            var tasks = _dbContext.Tasks
                .Include(t => t.Category)
                .Include(t => t.Files)
                .Where(t => t.CreatedAt >= startOfDayUtc && t.CreatedAt < endOfDayUtc)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.Completed)
                .ToList();

            _hasTasks = tasks.Any();

            if (!_hasTasks)
            {
                ShowNoTasksView();
            }
            else
            {
                ShowTasksListView(tasks);
            }
        }

        private void ShowNoTasksView()
        {
            var mainPanel = this.Controls[0];

            var centerPanel = new Panel
            {
                Size = new Size(400, 150),
                Location = new Point((mainPanel.Width - 400) / 2, (mainPanel.Height - 150) / 2),
                Anchor = AnchorStyles.None
            };

            var messageLabel = new Label
            {
                Text = "Нет задач на выбранный день",
                Font = new Font("Segoe UI", 14, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = false,
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 0)
            };

            var addButton = new Button
            {
                Text = "Добавить задачу",
                Size = new Size(200, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(50, 205, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(100, 60)
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) => AddNewTask();

            centerPanel.Controls.Add(addButton);
            centerPanel.Controls.Add(messageLabel);
            mainPanel.Controls.Add(centerPanel);
        }

        private void ShowTasksListView(List<Models.Task> tasks)
        {
            var mainPanel = this.Controls[0];

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                RowStyles = {
                    new RowStyle(SizeType.Percent, 100),
                    new RowStyle(SizeType.Absolute, 60)
                }
            };

            var tasksListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                GridLines = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                Font = new Font("Segoe UI", 9)
            };

            tasksListView.Columns.Add("Статус", 100);
            tasksListView.Columns.Add("Приоритет", 80);
            tasksListView.Columns.Add("Категория", 120);
            tasksListView.Columns.Add("Название", 200);
            tasksListView.Columns.Add("Дедлайн", 100);
            tasksListView.Columns.Add("Файлы", 150);

            tasksListView.DoubleClick += (s, e) => EditSelectedTask(tasksListView);

            foreach (var task in tasks)
            {
                string statusText;
                if (task.Completed)
                {
                    statusText = "✓ Выполнена";
                }
                else if (task.DeadlineDate.HasValue && task.DeadlineDate.Value.ToDateTime(TimeOnly.MinValue) < DateTime.Today)
                {
                    statusText = "⚠ Просрочено";
                }
                else
                {
                    statusText = "⏳ В работе";
                }

                var item = new ListViewItem(statusText);
                item.SubItems.Add(GetPriorityName(task.Priority));
                item.SubItems.Add(task.Category?.Name ?? "Без категории");
                item.SubItems.Add(task.Title);
                item.SubItems.Add(task.DeadlineDate?.ToString("dd.MM.yyyy") ?? "Нет");
                item.SubItems.Add(task.Files.Any() ? $"{task.Files.Count} файлов" : "Нет файлов");
                item.Tag = task.TaskId;

                if (task.Completed)
                {
                    item.BackColor = Color.FromArgb(220, 255, 220);
                    item.Font = new Font(tasksListView.Font, FontStyle.Strikeout);
                }
                else if (task.DeadlineDate.HasValue)
                {
                    var daysUntilDeadline = (task.DeadlineDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today);
                    if (daysUntilDeadline.TotalDays <= 1 && daysUntilDeadline.TotalDays >= 0)
                    {
                        item.BackColor = Color.FromArgb(220, 53, 69);
                        item.Font = new Font(tasksListView.Font, FontStyle.Bold);
                    }
                    else if (daysUntilDeadline.TotalDays < 0)
                    {
                        item.BackColor = Color.FromArgb(150, 150, 150);
                        item.Font = new Font(tasksListView.Font, FontStyle.Italic);
                    }
                }

                tasksListView.Items.Add(item);
            }

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };

            var addButton = new Button
            {
                Text = "Добавить задачу",
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(50, 205, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) => AddNewTask();

            var closeButton = new Button
            {
                Text = "Закрыть",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(addButton);

            tableLayout.Controls.Add(tasksListView, 0, 0);
            tableLayout.Controls.Add(buttonPanel, 0, 1);

            mainPanel.Controls.Add(tableLayout);
        }

        private string GetPriorityName(int priority)
        {
            return priority switch
            {
                1 => "Низкий",
                2 => "Средний",
                3 => "Высокий",
                _ => "Не указан"
            };
        }

        private void EditSelectedTask(ListView listView)
        {
            if (listView.SelectedItems.Count == 0 || listView.SelectedItems[0].Tag == null) return;

            int taskId = (int)listView.SelectedItems[0].Tag;
            var task = _dbContext.Tasks
                .Include(t => t.Category)
                .Include(t => t.Files)
                .FirstOrDefault(t => t.TaskId == taskId);

            using (var form = new TaskEditForm(_dbContext, task))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK || result == DialogResult.Abort)
                {
                    _dbContext.SaveChanges();
                    LoadTasks();
                }
            }
        }

        private void AddNewTask()
        {
            var newTask = new Models.Task
            {
                CreatedAt = _selectedDate.Date.ToUniversalTime(),
                Title = "Новая задача",
                Priority = 2,
                Completed = false
            };

            _dbContext.Tasks.Add(newTask);

            using (var form = new TaskEditForm(_dbContext, newTask))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _dbContext.SaveChanges();
                        LoadTasks();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении задачи: {ex.InnerException?.Message ?? ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    _dbContext.Entry(newTask).State = EntityState.Detached;
                }
            }
        }
    }
}