using Organizer.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Organizer
{
    // Главная форма приложения - календарь задач с возможностью просмотра, добавления и управления задачами
    public partial class CalendarForm : Form
    {
        // Сервис для отправки email-уведомлений
        private EmailService _emailService;

        // Поля для хранения состояния формы
        private DateTime _lastNotificationCheckDate = DateTime.MinValue;
        private DateTime currentDate;
        private TableLayoutPanel calendarTable;
        private Label monthLabel;
        private Label userNameLabel;
        private TeacherOrganizerContext dbContext;
        private Panel selectedDayPanel = null;
        private Button prevMonthButton;
        private Button nextMonthButton;
        private EventHandler userNameLabelClickHandler;
        private EventHandler avatarClickHandler;


        // Конструктор формы календаря
        public CalendarForm()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeCalendarComponents();
            currentDate = DateTime.Today;
            LoadUserName();
            UpdateCalendar();
            this.Resize += CalendarForm_Resize;

            // Инициализация сервиса отправки email
            _emailService = new EmailService(
                "smtp.mail.ru",
                465,
                "organizerapt@mail.ru",
                "2zGQ6ymJ2oQDwPVI8V4W",
                true
            );

            CheckDeadlineNotifications();
        }

        // --- Обработчики событий ---

        private void CalendarForm_Resize(object sender, EventArgs e)
        {
            UpdateCalendar();
        }

        // --- Методы инициализации ---

        // Инициализация подключения к базе данных
        private void InitializeDatabase()
        {
            dbContext = new TeacherOrganizerContext();
        }

        // Инициализация компонентов календаря
        private void InitializeCalendarComponents()
        {
            // Настройка основной формы
            this.Text = "Календарь задач";
            this.Size = new Size(900, 650);
            this.MinimumSize = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.FromArgb(240, 255, 240);

            // Создание главной панели с табличным layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.FromArgb(240, 255, 240)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Панель навигации с кнопками переключения месяцев
            var navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(34, 139, 34),
                Padding = new Padding(0, 10, 0, 0)
            };

            var navContainer = new Panel
            {
                Size = new Size(400, 50),
                Location = new Point(10, 10),
                BackColor = Color.Transparent
            };

            // Кнопка перехода к предыдущему месяцу
            prevMonthButton = new Button
            {
                Text = "◄",
                Size = new Size(50, 40),
                Location = new Point(0, 0),
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(144, 238, 144),
                ForeColor = Color.DarkGreen,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            prevMonthButton.Click += (s, e) => { currentDate = currentDate.AddMonths(-1); UpdateCalendar(); };

            // Метка с названием текущего месяца и года
            monthLabel = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(300, 40),
                Location = new Point(50, 0),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            // Кнопка перехода к следующему месяцу
            nextMonthButton = new Button
            {
                Text = "►",
                Size = new Size(50, 40),
                Location = new Point(350, 0),
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(144, 238, 144),
                ForeColor = Color.DarkGreen,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            nextMonthButton.Click += (s, e) => { currentDate = currentDate.AddMonths(1); UpdateCalendar(); };

            navContainer.Controls.Add(prevMonthButton);
            navContainer.Controls.Add(monthLabel);
            navContainer.Controls.Add(nextMonthButton);
            navPanel.Controls.Add(navContainer);

            // Метка с именем пользователя
            userNameLabel = new Label
            {
                Text = "Петров Алексей",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            navPanel.Controls.Add(userNameLabel);
            userNameLabel.Location = new Point(navPanel.Width - userNameLabel.Width - 20, 15);

            // Панель с днями недели
            var weekdaysPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                BackColor = Color.FromArgb(50, 205, 50)
            };
            for (int i = 0; i < 7; i++)
            {
                weekdaysPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));
                var lblDay = new Label
                {
                    Text = GetWeekdayShortName(i),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                weekdaysPanel.Controls.Add(lblDay, i, 0);
            }

            // Таблица календаря (6 строк х 7 столбцов)
            calendarTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 6,
                BackColor = Color.FromArgb(240, 255, 240),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Margin = new Padding(5)
            };
            for (int i = 0; i < 7; i++)
            {
                calendarTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));
            }
            for (int i = 0; i < 6; i++)
            {
                calendarTable.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66f));
            }

            // Кнопка очистки задач
            var clearTasksButton = new Button
            {
                Text = "Очистить задачи",
                Size = new Size(120, 40),
                Location = new Point(navContainer.Right + 20, 10),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            clearTasksButton.Click += ClearTasksButton_Click;
            navPanel.Controls.Add(clearTasksButton);

            userNameLabel.Location = new Point(navPanel.Width - userNameLabel.Width - 20, 15);

            // Сборка всех компонентов на главной панели
            mainPanel.Controls.Add(navPanel, 0, 0);
            mainPanel.Controls.Add(weekdaysPanel, 0, 1);
            mainPanel.Controls.Add(calendarTable, 0, 2);

            this.Controls.Add(mainPanel);
        }

        // --- Методы работы с пользователем ---

        // Загрузка и отображение имени пользователя и аватара
        private void LoadUserName()
        {
            var user = dbContext.UserProfiles.FirstOrDefault();

            // Очистка предыдущего аватара
            foreach (Control c in userNameLabel.Parent.Controls.OfType<PictureBox>().ToList())
            {
                if (avatarClickHandler != null)
                {
                    c.Click -= avatarClickHandler;
                }
                userNameLabel.Parent.Controls.Remove(c);
            }

            if (userNameLabelClickHandler != null)
            {
                userNameLabel.Click -= userNameLabelClickHandler;
            }

            if (user != null)
            {
                // Формирование инициалов пользователя
                string initials = "";
                if (!string.IsNullOrEmpty(user.FirstName) && user.FirstName.Length > 0)
                {
                    initials += user.FirstName[0] + ".";
                }
                if (!string.IsNullOrEmpty(user.MiddleName) && user.MiddleName.Length > 0)
                {
                    initials += user.MiddleName[0] + ".";
                }

                userNameLabel.Text = $"{user.LastName} {initials}";

                // Создание аватара пользователя
                var avatarBox = new PictureBox
                {
                    Image = GetUserAvatar(),
                    Size = new Size(40, 40),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };

                avatarBox.Location = new Point(
                    userNameLabel.Parent.Width - avatarBox.Width - 20,
                    (userNameLabel.Parent.Height - avatarBox.Height) / 2);

                userNameLabel.Location = new Point(
                    avatarBox.Left - userNameLabel.Width - 10,
                    (userNameLabel.Parent.Height - userNameLabel.Height) / 2);

                // Обработчики кликов по аватару и имени пользователя
                avatarClickHandler = (s, e) => ShowUserProfileForm(user);
                userNameLabelClickHandler = (s, e) => ShowUserProfileForm(user);

                avatarBox.Click += avatarClickHandler;
                userNameLabel.Click += userNameLabelClickHandler;

                userNameLabel.Parent.Controls.Add(avatarBox);
            }
            else
            {
                userNameLabel.Text = "Регистрация";
                userNameLabel.Location = new Point(
                    userNameLabel.Parent.Width - userNameLabel.Width - 20,
                    (userNameLabel.Parent.Height - userNameLabel.Height) / 2);

                userNameLabelClickHandler = (s, e) => ShowUserProfileForm(null);
                userNameLabel.Click += userNameLabelClickHandler;
            }
        }

        // Получение аватара пользователя (из файла или создание стандартного)
        private Image GetUserAvatar()
        {
            var user = dbContext.UserProfiles.FirstOrDefault();
            if (user == null) return CreateDefaultAvatar(null);

            if (!string.IsNullOrEmpty(user.AvatarPath) && System.IO.File.Exists(user.AvatarPath))
            {
                try
                {
                    return Image.FromFile(user.AvatarPath);
                }
                catch
                {
                    return CreateDefaultAvatar(user);
                }
            }

            return CreateDefaultAvatar(user);
        }

        // Создание стандартного аватара с инициалами пользователя
        private Image CreateDefaultAvatar(UserProfile user)
        {
            var bmp = new Bitmap(40, 40);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(Brushes.LightGray, 0, 0, 39, 39);

                string initials = "";
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.LastName) && user.LastName.Length > 0)
                    {
                        initials += user.LastName[0];
                    }
                    if (!string.IsNullOrEmpty(user.FirstName) && user.FirstName.Length > 0)
                    {
                        initials += user.FirstName[0];
                    }
                }

                if (string.IsNullOrEmpty(initials))
                {
                    initials = "?";
                }
                else
                {
                    initials = initials.ToUpper();
                }

                using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
                {
                    var size = g.MeasureString(initials, font);
                    g.DrawString(initials, font, Brushes.DarkGreen,
                        (bmp.Width - size.Width) / 2,
                        (bmp.Height - size.Height) / 2);
                }
            }
            return bmp;
        }

        // Отображение формы профиля пользователя
        private void ShowUserProfileForm(UserProfile user)
        {
            using (var form = new UserProfileForm(dbContext, user))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadUserName();
                }
            }
        }

        // --- Методы работы с календарем ---

        // Обновление отображения календаря
        private void UpdateCalendar()
        {
            monthLabel.Text = currentDate.ToString("MMMM yyyy").ToUpper();

            DateTime? selectedDate = selectedDayPanel?.Tag as DateTime?;

            calendarTable.SuspendLayout();
            calendarTable.Controls.Clear();

            DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

            int dayOfWeek = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

            // Добавление пустых ячеек перед первым днем месяца
            for (int i = 0; i < dayOfWeek; i++)
            {
                var emptyPanel = new Panel
                {
                    BackColor = Color.Transparent,
                    Dock = DockStyle.Fill
                };
                calendarTable.Controls.Add(emptyPanel);
            }

            // Добавление дней месяца
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(currentDate.Year, currentDate.Month, day);
                var dayPanel = CreateDayPanel(day, date);

                if (selectedDate.HasValue && date.Date == selectedDate.Value.Date)
                {
                    dayPanel.BackColor = Color.FromArgb(152, 251, 152);
                    selectedDayPanel = dayPanel;
                }

                calendarTable.Controls.Add(dayPanel);
            }

            // Добавление пустых ячеек после последнего дня месяца
            int totalCells = 42;
            int filledCells = dayOfWeek + daysInMonth;
            for (int i = filledCells; i < totalCells; i++)
            {
                var emptyPanel = new Panel
                {
                    BackColor = Color.Transparent,
                    Dock = DockStyle.Fill
                };
                calendarTable.Controls.Add(emptyPanel);
            }

            calendarTable.ResumeLayout();
        }

        // Создание панели для отображения дня в календаре
        private Panel CreateDayPanel(int day, DateTime date)
        {
            var dayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Tag = date,
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Выделение текущего дня
            if (date.Date == DateTime.Today.Date)
            {
                dayPanel.BackColor = Color.FromArgb(173, 216, 230);
                dayPanel.BorderStyle = BorderStyle.Fixed3D;
            }

            var dayLabel = new Label
            {
                Text = day.ToString(),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 2, 5, 0),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black
            };

            // Подсчет задач на день
            var startOfDayUtc = date.Date.ToUniversalTime();
            var endOfDayUtc = startOfDayUtc.AddDays(1);
            int tasksCount = dbContext.Tasks.Count(t => t.CreatedAt.HasValue &&
                t.CreatedAt.Value.ToUniversalTime() >= startOfDayUtc &&
                t.CreatedAt.Value.ToUniversalTime() < endOfDayUtc);

            // Отображение количества задач
            if (tasksCount > 0)
            {
                var tasksLabel = new Label
                {
                    Text = $"Задач: {tasksCount}",
                    Dock = DockStyle.Bottom,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(0, 80, 0),
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    BackColor = Color.FromArgb(220, 255, 220)
                };
                dayPanel.Controls.Add(tasksLabel);
            }

            dayPanel.Controls.Add(dayLabel);
            dayPanel.Click += (s, e) => SelectDay(dayPanel, date);

            return dayPanel;
        }

        // Выбор дня в календаре
        private void SelectDay(Panel dayPanel, DateTime date)
        {
            // Сброс выделения предыдущего дня
            if (selectedDayPanel != null)
            {
                if (((DateTime)selectedDayPanel.Tag).Date == DateTime.Today.Date)
                {
                    selectedDayPanel.BackColor = Color.FromArgb(173, 216, 230);
                    selectedDayPanel.BorderStyle = BorderStyle.Fixed3D;
                }
                else
                {
                    selectedDayPanel.BackColor = Color.White;
                    selectedDayPanel.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            // Выделение выбранного дня
            dayPanel.BackColor = Color.FromArgb(200, 255, 200);
            dayPanel.BorderStyle = BorderStyle.Fixed3D;
            selectedDayPanel = dayPanel;

            ShowTasksForDate(date);
        }

        // Отображение задач для выбранной даты
        private void ShowTasksForDate(DateTime date)
        {
            using (var form = new TasksForm(dbContext, date))
            {
                var result = form.ShowDialog();

                if (result == DialogResult.OK || result == DialogResult.Cancel)
                {
                    UpdateCalendar();
                }
            }
        }

        // --- Методы работы с уведомлениями ---

        // Проверка задач с приближающимся дедлайном и отправка уведомлений
        private void CheckDeadlineNotifications()
        {
            // Проверка не чаще чем раз в 6 часов
            if ((DateTime.Now - _lastNotificationCheckDate).TotalHours < 6)
                return;

            _lastNotificationCheckDate = DateTime.Now;

            var user = dbContext.UserProfiles.FirstOrDefault();
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || !user.ReceiveMailNotifications)
                return;

            var nowUtc = DateTime.UtcNow;
            var nowWithoutMsUtc = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, nowUtc.Hour, nowUtc.Minute, 0, DateTimeKind.Utc);
            var todayUtc = nowWithoutMsUtc.Date;
            var deadlineThresholdUtc = nowUtc.AddHours(24);

            // Получение задач с приближающимся дедлайном
            var tasks = dbContext.Tasks
                .Where(t => !t.Completed && t.DeadlineDate.HasValue)
                .AsEnumerable()
                .Where(t =>
                {
                    var deadlineDate = t.DeadlineDate.Value;
                    var deadlineDateTime = deadlineDate.ToDateTime(TimeOnly.MinValue);
                    var deadlineUtc = DateTime.SpecifyKind(deadlineDateTime, DateTimeKind.Utc);

                    if (deadlineUtc.Date == todayUtc)
                    {
                        return t.LastNotificationSent == null ||
                               t.LastNotificationSent.Value.Date < todayUtc;
                    }
                    else if (deadlineUtc <= deadlineThresholdUtc && deadlineUtc >= nowWithoutMsUtc)
                    {
                        return t.LastNotificationSent == null ||
                               (nowWithoutMsUtc - t.LastNotificationSent.Value).TotalHours >= 6;
                    }
                    return false;
                })
                .ToList();

            // Отправка уведомления, если есть задачи
            if (tasks.Any())
            {
                string subject = "⏰ Срочные задачи: приближается дедлайн!";
                string body = $@"Уважаемый пользователь, в вашем органайзере есть {tasks.Count} задач(-а), у которых скоро истекает срок выполнения (сегодня или менее 24 часов):" + Environment.NewLine + Environment.NewLine;

                foreach (var task in tasks)
                {
                    var deadlineDate = task.DeadlineDate.Value;
                    var deadlineDateTime = deadlineDate.ToDateTime(TimeOnly.MinValue);
                    var deadlineUtc = DateTime.SpecifyKind(deadlineDateTime, DateTimeKind.Utc);
                    var timeLeft = deadlineUtc - nowWithoutMsUtc;
                    var hoursLeft = (int)timeLeft.TotalHours;
                    var minutesLeft = timeLeft.Minutes;

                    string urgencyLevel = task.Priority switch
                    {
                        3 => "❗❗❗ ВЫСОКИЙ приоритет",
                        2 => "❗❗ Средний приоритет",
                        1 => "❗ Низкий приоритет",
                        _ => "Приоритет не указан"
                    };

                    string timeLeftText = deadlineUtc.Date == todayUtc ?
                        "Срок выполнения: СЕГОДНЯ" :
                        $"- Осталось времени: {hoursLeft} ч. {minutesLeft} мин.";

                    body += $@"• {task.Title}" + Environment.NewLine +
                           $"\t- {urgencyLevel}" + Environment.NewLine +
                           $"\t- Срок выполнения: {deadlineDate:dd.MM.yyyy}" + Environment.NewLine +
                           $"\t{timeLeftText}" +
                           Environment.NewLine + Environment.NewLine;
                }

                body += @"Рекомендуем завершить эти задачи как можно скорее!" + Environment.NewLine + Environment.NewLine +
                        "С уважением," + Environment.NewLine +
                        "Ваш Органайзер задач";

                try
                {
                    _emailService.SendEmail(user.Email, subject, body);

                    // Обновление времени последнего уведомления
                    foreach (var task in tasks)
                    {
                        task.LastNotificationSent = nowWithoutMsUtc;
                    }
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке уведомления: {ex.Message}");
                }
            }
        }

        // --- Вспомогательные методы ---

        // Получение сокращенного названия дня недели
        private string GetWeekdayShortName(int index)
        {
            string[] days = { "Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" };
            return days[index];
        }

        // --- Методы работы с задачами ---

        // Обработчик клика по кнопке очистки задач
        private void ClearTasksButton_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Очистить задачи за сегодня", null,
                (s, args) => ClearTasksForPeriod(Period.Today));

            DateTime weekStart = DateTime.Today.AddDays(-6);
            DateTime weekEnd = DateTime.Today;
            menu.Items.Add($"Очистить задачи за неделю ({weekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy})", null,
                (s, args) => ClearTasksForPeriod(Period.Week));

            DateTime monthStart = DateTime.Today.AddDays(-29);
            DateTime monthEnd = DateTime.Today;
            menu.Items.Add($"Очистить задачи за месяц ({monthStart:dd.MM.yyyy} - {monthEnd:dd.MM.yyyy})", null,
                (s, args) => ClearTasksForPeriod(Period.Month));

            DateTime yearStart = DateTime.Today.AddDays(-364);
            DateTime yearEnd = DateTime.Today;
            menu.Items.Add($"Очистить задачи за год ({yearStart:dd.MM.yyyy} - {yearEnd:dd.MM.yyyy})", null,
                (s, args) => ClearTasksForPeriod(Period.Year));

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Выбрать свой период...", null, (s, args) => ShowCustomPeriodDialog());

            var button = (Button)sender;
            menu.Show(button, new Point(0, button.Height));
        }

        // Перечисление для выбора периода очистки задач
        private enum Period { Today, Week, Month, Year }

        // Очистка задач за указанный период
        private void ClearTasksForPeriod(Period period)
        {
            DateTime startDate;
            DateTime endDate = DateTime.Today.AddDays(1);

            switch (period)
            {
                case Period.Today:
                    startDate = DateTime.Today;
                    break;
                case Period.Week:
                    startDate = DateTime.Today.AddDays(-6);
                    break;
                case Period.Month:
                    startDate = DateTime.Today.AddDays(-29);
                    break;
                case Period.Year:
                    startDate = DateTime.Today.AddDays(-364);
                    break;
                default:
                    startDate = DateTime.Today;
                    break;
            }

            var startDateUtc = startDate.ToUniversalTime();
            var endDateUtc = endDate.ToUniversalTime();

            var tasksToDelete = dbContext.Tasks
                .Where(t => t.CreatedAt.HasValue &&
                           t.CreatedAt.Value.ToUniversalTime() >= startDateUtc &&
                           t.CreatedAt.Value.ToUniversalTime() < endDateUtc)
                .ToList();

            if (tasksToDelete.Count == 0)
            {
                MessageBox.Show($"Нет задач за выбранный период ({GetPeriodDescription(period, startDate, endDate.AddDays(-1))})",
                               "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить {tasksToDelete.Count} задач за {GetPeriodDescription(period, startDate, endDate.AddDays(-1))}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    dbContext.Tasks.RemoveRange(tasksToDelete);
                    dbContext.SaveChanges();
                    MessageBox.Show($"Удалено {tasksToDelete.Count} задач", "Успешно",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateCalendar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении задач: {ex.Message}", "Ошибка",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Получение описания периода для отображения в сообщениях
        private string GetPeriodDescription(Period period, DateTime startDate, DateTime endDate)
        {
            switch (period)
            {
                case Period.Today:
                    return "сегодня";
                case Period.Week:
                    return $"неделю (с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy})";
                case Period.Month:
                    return $"месяц (с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy})";
                case Period.Year:
                    return $"год (с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy})";
                default:
                    return "период";
            }
        }

        // Очистка задач за пользовательский период
        private void ClearCustomPeriod(DateTime startDate, DateTime endDate, Form parentForm = null)
        {
            var startDateUtc = startDate.Date.ToUniversalTime();
            var endDateUtc = endDate.Date.AddDays(1).ToUniversalTime();

            var tasksToDelete = dbContext.Tasks
                .Where(t => t.CreatedAt.HasValue &&
                           t.CreatedAt.Value.ToUniversalTime() >= startDateUtc &&
                           t.CreatedAt.Value.ToUniversalTime() < endDateUtc)
                .ToList();

            if (tasksToDelete.Count == 0)
            {
                MessageBox.Show($"Нет задач за выбранный период (с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy})",
                               "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(parentForm,
                $"Вы действительно хотите удалить {tasksToDelete.Count} задач за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    dbContext.Tasks.RemoveRange(tasksToDelete);
                    dbContext.SaveChanges();
                    MessageBox.Show($"Удалено {tasksToDelete.Count} задач", "Успешно",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateCalendar();

                    if (parentForm != null)
                    {
                        parentForm.DialogResult = DialogResult.OK;
                        parentForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении задач: {ex.Message}", "Ошибка",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Отображение диалога выбора пользовательского периода для очистки задач
        private void ShowCustomPeriodDialog()
        {
            using (var form = new Form())
            {
                form.Text = "Выберите период для очистки задач";
                form.Size = new Size(350, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var lblStart = new Label { Text = "Начальная дата:", Left = 20, Top = 20, Width = 100 };
                var dateStart = new DateTimePicker { Left = 130, Top = 20, Width = 150, Format = DateTimePickerFormat.Short };

                var lblEnd = new Label { Text = "Конечная дата:", Left = 20, Top = 60, Width = 100 };
                var dateEnd = new DateTimePicker { Left = 130, Top = 60, Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

                var btnOk = new Button { Text = "Очистить", Left = 130, Top = 100, Width = 80 };
                var btnCancel = new Button { Text = "Отмена", Left = 220, Top = 100, Width = 80 };

                btnOk.Click += (s, e) =>
                {
                    if (dateStart.Value > dateEnd.Value)
                    {
                        MessageBox.Show("Начальная дата не может быть позже конечной!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        ClearCustomPeriod(dateStart.Value, dateEnd.Value, form);
                    }
                };

                btnCancel.Click += (s, e) => form.Close();

                form.Controls.AddRange(new Control[] { lblStart, dateStart, lblEnd, dateEnd, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                form.ShowDialog();
            }
        }
    }
}