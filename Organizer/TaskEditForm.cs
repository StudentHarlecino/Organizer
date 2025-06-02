using Organizer.Models;
using System.Diagnostics;

namespace Organizer
{
    // Форма для создания/редактирования задачи
    public partial class TaskEditForm : Form
    {
        private readonly TeacherOrganizerContext _dbContext;
        private readonly Models.Task _task;
        private List<Models.File> _selectedFiles = new List<Models.File>();

        // Инициализация формы с контекстом БД и задачей
        public TaskEditForm(TeacherOrganizerContext dbContext, Models.Task task)
        {
            _dbContext = dbContext;
            _task = task;
            InitializeComponent();
            InitializeComponents();
            LoadData();
        }

        // Инициализация компонентов формы
        private void InitializeComponents()
        {
            // Настройка основных параметров формы
            this.Text = _task.TaskId == 0 ? "Создание задачи" : "Редактирование задачи";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создание главной панели
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Создание таблицы для компоновки элементов
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 8,
                ColumnCount = 2,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // Настройка строк таблицы
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            // Создание элементов управления
            var lblCategory = new Label
            {
                Text = "Категория:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 0, 0, 2)
            };

            var cmbCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "cmbCategory",
                Margin = new Padding(3, 5, 0, 5)
            };

            // Панель для категории с кнопками
            var categoryPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Name = "categoryPanel",
                ColumnCount = 3,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.Absolute, 35F),
                    new ColumnStyle(SizeType.Absolute, 35F)
                },
                Margin = new Padding(0, 5, 0, 0)
            };

            // Кнопки для работы с категориями
            var btnAddCategory = new Button
            {
                Text = "+",
                Size = new Size(30, 30),
                BackColor = Color.FromArgb(50, 205, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.None,
                Padding = new Padding(5, 0, 5, 0),
                Tag = cmbCategory
            };
            btnAddCategory.Click += BtnAddCategory_Click;

            var btnDeleteCategory = new Button
            {
                Text = "-",
                Size = new Size(30, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.None,
                Padding = new Padding(5, 0, 5, 0),
                Tag = cmbCategory
            };
            btnDeleteCategory.Click += BtnDeleteCategory_Click;

            // Добавление элементов на панель категории
            categoryPanel.Controls.Add(cmbCategory, 0, 0);
            categoryPanel.Controls.Add(btnAddCategory, 1, 0);
            categoryPanel.Controls.Add(btnDeleteCategory, 2, 0);

            cmbCategory.Dock = DockStyle.Fill;
            cmbCategory.Width = categoryPanel.Width - 60;

            // Создание остальных элементов формы
            var lblTitle = new Label { Text = "Название:", TextAlign = ContentAlignment.MiddleLeft };
            var txtTitle = new TextBox { Dock = DockStyle.Fill, Name = "txtTitle" };

            var lblDescription = new Label { Text = "Описание:", TextAlign = ContentAlignment.MiddleLeft };
            var txtDescription = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, Name = "txtDescription" };

            var lblPriority = new Label { Text = "Приоритет:", TextAlign = ContentAlignment.MiddleLeft };
            var cmbPriority = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Name = "cmbPriority" };

            var lblDeadline = new Label { Text = "Дедлайн:", TextAlign = ContentAlignment.MiddleLeft };
            var dtpDeadline = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = " ",
                ShowCheckBox = true,
                Name = "dtpDeadline"
            };

            var lblCompleted = new Label { Text = "Статус:", TextAlign = ContentAlignment.MiddleLeft };
            var chkCompleted = new CheckBox
            {
                Text = "Выполнена",
                AutoSize = true,
                CheckAlign = ContentAlignment.MiddleLeft,
                Name = "chkCompleted",
                Margin = new Padding(0, 10, 0, 0)
            };

            var lblFiles = new Label { Text = "Файлы:", TextAlign = ContentAlignment.MiddleLeft };
            var pnlFiles = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 120
            };

            var btnAddFile = new Button
            {
                Text = "Добавить файл",
                Size = new Size(120, 30),
                Margin = new Padding(0, 5, 5, 5)
            };
            btnAddFile.Click += BtnAddFile_Click;

            pnlFiles.Controls.Add(btnAddFile);

            // Кнопки управления формой
            var btnDelete = new Button
            {
                Text = "Удалить",
                Visible = _task.TaskId != 0,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 10, 5, 0)
            };

            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(50, 205, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 10, 5, 0)
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 10, 10, 0)
            };

            // Добавление элементов в таблицу
            var statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };
            statusPanel.Controls.Add(chkCompleted);

            tableLayout.Controls.Add(lblCompleted, 0, 5);
            tableLayout.Controls.Add(statusPanel, 1, 5);
            tableLayout.Controls.Add(lblCategory, 0, 0);
            tableLayout.Controls.Add(categoryPanel, 1, 0);
            tableLayout.Controls.Add(lblTitle, 0, 1);
            tableLayout.Controls.Add(txtTitle, 1, 1);
            tableLayout.Controls.Add(lblDescription, 0, 2);
            tableLayout.Controls.Add(txtDescription, 1, 2);
            tableLayout.Controls.Add(lblPriority, 0, 3);
            tableLayout.Controls.Add(cmbPriority, 1, 3);
            tableLayout.Controls.Add(lblDeadline, 0, 4);
            tableLayout.Controls.Add(dtpDeadline, 1, 4);
            tableLayout.Controls.Add(lblCompleted, 0, 5);
            tableLayout.Controls.Add(statusPanel, 1, 5);
            tableLayout.Controls.Add(lblFiles, 0, 6);
            tableLayout.Controls.Add(pnlFiles, 1, 6);

            // Панель для кнопок внизу формы
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = _task.TaskId == 0 ? 2 : 3,
                RowCount = 1
            };

            if (_task.TaskId == 0)
            {
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                buttonPanel.Controls.Add(btnSave, 0, 0);
                buttonPanel.Controls.Add(btnCancel, 1, 0);
            }
            else
            {
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
                buttonPanel.Controls.Add(btnCancel, 2, 0);
                buttonPanel.Controls.Add(btnSave, 0, 0);
                buttonPanel.Controls.Add(btnDelete, 1, 0);
            }

            tableLayout.Controls.Add(buttonPanel, 0, 7);
            tableLayout.SetColumnSpan(buttonPanel, 2);

            mainPanel.Controls.Add(tableLayout);
            this.Controls.Add(mainPanel);

            // Подписка на события
            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnCancel.Click += (s, e) => this.Close();
            dtpDeadline.ValueChanged += (s, e) =>
            {
                dtpDeadline.CustomFormat = dtpDeadline.Checked ? "dd.MM.yyyy" : " ";
            };
        }

        // Обработчик добавления новой категории
        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            var cmbCategory = (sender as Button)?.Tag as ComboBox;
            if (cmbCategory == null) return;

            using (var form = new Form())
            {
                form.Text = "Добавить новую категорию";
                form.Size = new Size(400, 140);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.Font = new Font("Segoe UI", 10);

                var mainPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10)
                };

                var tableLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 3,
                    ColumnStyles = {
                        new ColumnStyle(SizeType.Absolute, 150),
                        new ColumnStyle(SizeType.Percent, 100)
                    },
                    RowStyles = {
                        new RowStyle(SizeType.Absolute, 40),
                        new RowStyle(SizeType.Absolute, 5),
                        new RowStyle(SizeType.Absolute, 35)
                    }
                };

                var lblName = new Label
                {
                    Text = "Название категории:",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 0, 10, 0)
                };

                var txtName = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 5, 0, 0),
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };

                var buttonPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    Padding = new Padding(0, 8, 0, 0)
                };

                var btnOk = new Button
                {
                    Text = "Добавить",
                    DialogResult = DialogResult.OK,
                    Size = new Size(100, 30),
                    BackColor = Color.FromArgb(50, 205, 50),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Anchor = AnchorStyles.None
                };

                var btnCancel = new Button
                {
                    Text = "Отмена",
                    DialogResult = DialogResult.Cancel,
                    Size = new Size(100, 30),
                    BackColor = Color.FromArgb(108, 117, 125),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Anchor = AnchorStyles.None
                };

                buttonPanel.Controls.Add(btnOk);
                buttonPanel.Controls.Add(btnCancel);
                btnOk.Left = (buttonPanel.Width - btnOk.Width - btnCancel.Width - 10) / 2;
                btnCancel.Left = btnOk.Right + 10;
                btnOk.Top = btnCancel.Top = (buttonPanel.Height - btnOk.Height) / 2;

                tableLayout.Controls.Add(lblName, 0, 0);
                tableLayout.Controls.Add(txtName, 1, 0);
                tableLayout.Controls.Add(buttonPanel, 0, 2);
                tableLayout.SetColumnSpan(buttonPanel, 2);

                mainPanel.Controls.Add(tableLayout);
                form.Controls.Add(mainPanel);

                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtName.Text))
                {
                    try
                    {
                        var newCategory = new TaskCategory { Name = txtName.Text.Trim() };
                        _dbContext.TaskCategories.Add(newCategory);
                        _dbContext.SaveChanges();

                        var categories = _dbContext.TaskCategories.ToList();
                        cmbCategory.DataSource = categories;
                        cmbCategory.DisplayMember = "Name";
                        cmbCategory.ValueMember = "CategoryId";
                        cmbCategory.SelectedValue = newCategory.CategoryId;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Обработчик удаления категории
        private void BtnDeleteCategory_Click(object sender, EventArgs e)
        {
            var cmbCategory = (sender as Button)?.Tag as ComboBox;
            if (cmbCategory == null || cmbCategory.SelectedItem == null) return;

            var selectedCategory = cmbCategory.SelectedItem as TaskCategory;
            if (selectedCategory == null) return;

            var isUsed = _dbContext.Tasks.Any(t => t.CategoryId == selectedCategory.CategoryId);

            if (isUsed)
            {
                MessageBox.Show("Невозможно удалить категорию, так как она используется в задачах.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Вы действительно хотите удалить категорию '{selectedCategory.Name}'?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _dbContext.TaskCategories.Remove(selectedCategory);
                    _dbContext.SaveChanges();

                    var categories = _dbContext.TaskCategories.ToList();
                    cmbCategory.DataSource = categories;
                    cmbCategory.DisplayMember = "Name";
                    cmbCategory.ValueMember = "CategoryId";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении категории: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Загрузка данных в форму
        private void LoadData()
        {
            var categoryPanel = this.Controls.Find("categoryPanel", true).FirstOrDefault() as Panel;
            if (categoryPanel == null) return;

            var cmbCategory = categoryPanel.Controls.OfType<ComboBox>().FirstOrDefault(c => c.Name == "cmbCategory");
            var cmbPriority = this.Controls.Find("cmbPriority", true).FirstOrDefault() as ComboBox;
            var txtTitle = this.Controls.Find("txtTitle", true).FirstOrDefault() as TextBox;
            var txtDescription = this.Controls.Find("txtDescription", true).FirstOrDefault() as TextBox;
            var dtpDeadline = this.Controls.Find("dtpDeadline", true).FirstOrDefault() as DateTimePicker;
            var chkCompleted = this.Controls.Find("chkCompleted", true).FirstOrDefault() as CheckBox;

            if (cmbCategory == null || cmbPriority == null || txtTitle == null ||
                txtDescription == null || dtpDeadline == null || chkCompleted == null)
            {
                MessageBox.Show("Ошибка инициализации формы: не найдены необходимые элементы управления");
                return;
            }

            // Загрузка категорий
            var categories = _dbContext.TaskCategories.ToList();
            cmbCategory.DataSource = categories;
            cmbCategory.DisplayMember = "Name";
            cmbCategory.ValueMember = "CategoryId";

            // Загрузка приоритетов
            cmbPriority.Items.AddRange(new object[] { "Низкий", "Средний", "Высокий" });

            // Установка значений из задачи
            txtTitle.Text = _task.Title;
            txtDescription.Text = _task.Description;
            cmbCategory.SelectedValue = _task.CategoryId ?? 0;
            cmbPriority.SelectedIndex = _task.Priority - 1;
            chkCompleted.Checked = _task.Completed;

            // Настройка дедлайна
            if (_task.DeadlineDate.HasValue)
            {
                dtpDeadline.Value = _task.DeadlineDate.Value.ToDateTime(TimeOnly.MinValue);
                dtpDeadline.Checked = true;
                dtpDeadline.CustomFormat = "dd.MM.yyyy";
            }
            else
            {
                dtpDeadline.Checked = false;
                dtpDeadline.CustomFormat = " ";
            }

            // Загрузка файлов, если задача уже существует
            if (_task.TaskId != 0)
            {
                _dbContext.Entry(_task).Collection(t => t.Files).Load();
                _selectedFiles = _task.Files.ToList();
                UpdateFilesList();
            }
        }

        // Обработчик добавления файлов
        private void BtnAddFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Выберите файлы для задачи";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var fileName in openFileDialog.FileNames)
                    {
                        var fileInfo = new System.IO.FileInfo(fileName);
                        var file = new Models.File
                        {
                            OriginalName = fileInfo.Name,
                            FilePath = fileName,
                            FileSize = fileInfo.Length,
                            MimeType = MimeTypes.GetMimeType(fileName),
                            UploadedAt = DateTime.UtcNow
                        };

                        _selectedFiles.Add(file);
                    }

                    UpdateFilesList();
                }
            }
        }

        // Обновление списка файлов на форме
        private void UpdateFilesList()
        {
            var mainPanel = this.Controls[0] as Panel;
            if (mainPanel == null) return;

            var tableLayout = mainPanel.Controls[0] as TableLayoutPanel;
            if (tableLayout == null) return;

            var pnlFiles = tableLayout.GetControlFromPosition(1, 6) as FlowLayoutPanel;
            if (pnlFiles == null) return;

            pnlFiles.Controls.Clear();

            var btnAddFile = new Button
            {
                Text = "Добавить файл",
                Size = new Size(120, 30),
                Margin = new Padding(0, 5, 5, 5)
            };
            btnAddFile.Click += BtnAddFile_Click;
            pnlFiles.Controls.Add(btnAddFile);

            // Добавление файлов в список
            foreach (var file in _selectedFiles)
            {
                var filePanel = new Panel
                {
                    Size = new Size(pnlFiles.Width - 30, 30),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(0, 5, 0, 0)
                };

                var lblFileName = new Label
                {
                    Text = file.OriginalName,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(5, 0, 0, 0),
                    Cursor = Cursors.Hand,
                    Tag = file
                };
                lblFileName.Click += (s, e) => OpenFile((Models.File)lblFileName.Tag);

                var btnOpenFolder = new Button
                {
                    Text = "📂",
                    Size = new Size(25, 25),
                    Dock = DockStyle.Right,
                    FlatStyle = FlatStyle.Flat,
                    Tag = file
                };
                btnOpenFolder.Click += (s, e) => OpenFileFolder((Models.File)btnOpenFolder.Tag);

                var btnRemove = new Button
                {
                    Text = "×",
                    Size = new Size(25, 25),
                    Dock = DockStyle.Right,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.Red,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Tag = file
                };
                btnRemove.Click += (s, e) =>
                {
                    _selectedFiles.Remove((Models.File)btnRemove.Tag);
                    UpdateFilesList();
                };

                filePanel.Controls.Add(btnRemove);
                filePanel.Controls.Add(btnOpenFolder);
                filePanel.Controls.Add(lblFileName);
                pnlFiles.Controls.Add(filePanel);
            }
        }

        // Открытие файла
        private void OpenFile(Models.File file)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = file.FilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Открытие папки с файлом
        private void OpenFileFolder(Models.File file)
        {
            try
            {
                string folderPath = Path.GetDirectoryName(file.FilePath);
                Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть папку: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик сохранения задачи
        private void BtnSave_Click(object sender, EventArgs e)
        {
            var txtTitle = this.Controls.Find("txtTitle", true).FirstOrDefault() as TextBox;
            var txtDescription = this.Controls.Find("txtDescription", true).FirstOrDefault() as TextBox;
            var cmbCategory = this.Controls.Find("cmbCategory", true).FirstOrDefault() as ComboBox;
            var cmbPriority = this.Controls.Find("cmbPriority", true).FirstOrDefault() as ComboBox;
            var dtpDeadline = this.Controls.Find("dtpDeadline", true).FirstOrDefault() as DateTimePicker;
            var chkCompleted = this.Controls.Find("chkCompleted", true).FirstOrDefault() as CheckBox;

            // Валидация названия задачи
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Название задачи не может быть пустым", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Обновление данных задачи
            _task.Title = txtTitle.Text;
            _task.Description = txtDescription.Text;
            _task.Priority = cmbPriority.SelectedIndex + 1;
            _task.CategoryId = (int?)cmbCategory.SelectedValue;
            _task.DeadlineDate = dtpDeadline.Checked ? DateOnly.FromDateTime(dtpDeadline.Value) : null;
            _task.Completed = chkCompleted.Checked;

            bool newCompletedStatus = chkCompleted.Checked;
            _task.Completed = newCompletedStatus;

            // Добавление новых файлов
            foreach (var file in _selectedFiles.Where(f => f.FileId == 0))
            {
                _dbContext.Files.Add(file);
            }

            // Обновление списка файлов задачи
            _task.Files.Clear();
            foreach (var file in _selectedFiles)
            {
                _task.Files.Add(file);
            }

            // Сохранение изменений
            try
            {
                _dbContext.SaveChanges();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении задачи: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик удаления задачи
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить эту задачу?", "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    if (_task.TaskId != 0)
                    {
                        _dbContext.Tasks.Remove(_task);
                        _dbContext.SaveChanges();
                        this.DialogResult = DialogResult.Abort;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Класс для определения MIME-типов файлов
    public static class MimeTypes
    {
        private static readonly Dictionary<string, string> MimeTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {".txt", "text/plain"},
            {".pdf", "application/pdf"},
            {".doc", "application/msword"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".png", "image/png"},
            {".gif", "image/gif"},
            {".zip", "application/zip"},
            {".rar", "application/x-rar-compressed"}
        };

        // Получение MIME-типа по расширению файла
        public static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return MimeTypeMappings.TryGetValue(extension, out var mimeType) ? mimeType : "application/octet-stream";
        }
    }
}