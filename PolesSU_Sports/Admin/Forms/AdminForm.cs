using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using PolesSU_Sports.Shared.DB;
using PolesSU_Sports.Shared.Model;

namespace PolesSU_Sports.Admin
{
    public partial class AdminForm : Form
    {
        private Panel sidebarPanel;
        private Panel headerPanel;
        private Panel contentPanel;
        private Label headerLabel;
        private Button currentActiveButton;
        private DataGridView currentDgv;
        private string currentTableType;
        private TextBox txtSearch;
        private DateTimePicker dtpDate;
        private Panel filterPanel;

        public AdminForm()
        {
            InitializeComponent();
            SetupModernUI();
            ShowDashboard();
        }

        private void SetupModernUI()
        {
            this.Text = "PolesSU Sports: Панель Администратора";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 240, 245);

            // 1. SIDEBAR (слева)
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(45, 55, 75),
                Padding = new Padding(0, 0, 0, 20)
            };

            // Логотип
            var logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(35, 45, 65)
            };
            var logoLabel = new Label
            {
                Text = "PolesSU\nSports",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(280, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            logoPanel.Controls.Add(logoLabel);
            sidebarPanel.Controls.Add(logoPanel);

            // Кнопки меню
            CreateMenuButton("📊 Дашборд", ShowDashboard);
            CreateMenuButton("🎓 Студенты", () => ShowGrid("Студенты"));
            CreateMenuButton("🏆 Тренеры", () => ShowGrid("Тренеры"));
            CreateMenuButton("⚽ Секции", () => ShowGrid("Секции"));
            CreateMenuButton("📋 Посещаемость", () => ShowGrid("Посещаемость"));
            CreateMenuButton("🔧 Управление", ShowManagement);

            // Кнопка выхода
            var separator = new Panel { Dock = DockStyle.Bottom, Height = 20, BackColor = Color.FromArgb(45, 55, 75) };
            sidebarPanel.Controls.Add(separator);

            var exitBtn = new Button
            {
                Text = "🚪 Выйти",
                Dock = DockStyle.Bottom,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            exitBtn.FlatAppearance.BorderSize = 0;
            exitBtn.Click += (s, e) => {
                if (MessageBox.Show("Выйти из системы?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    User.Logout();
                    this.DialogResult = DialogResult.Retry;
                    this.Close();
                }
            };
            sidebarPanel.Controls.Add(exitBtn);

            // 2. HEADER (сверху)
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(30, 15, 30, 15)
            };

            headerLabel = new Label
            {
                Text = "Дашборд",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 55, 75),
                AutoSize = true,
                Location = new Point(30, 20)
            };

            var userInfo = new Label
            {
                Text = $"{User.CurrentUser?.FullName ?? "Пользователь"}\n{User.CurrentUser?.Role}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(headerPanel.Width - 200, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight
            };

            headerPanel.Controls.Add(headerLabel);
            headerPanel.Controls.Add(userInfo);

            // 3. CONTENT (основная область)
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                BackColor = Color.FromArgb(240, 240, 245),
                AutoScroll = true
            };

            // Порядок добавления важен!
            this.Controls.Add(contentPanel);  // Fill
            this.Controls.Add(headerPanel);   // Top
            this.Controls.Add(sidebarPanel);  // Left
        }

        private void CreateMenuButton(string text, Action clickAction)
        {
            var btn = new Button
            {
                Text = "  " + text,
                Dock = DockStyle.Top,
                Height = 55,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 55, 75),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => { if (btn != currentActiveButton) btn.BackColor = Color.FromArgb(55, 65, 85); };
            btn.MouseLeave += (s, e) => { if (btn != currentActiveButton) btn.BackColor = Color.FromArgb(45, 55, 75); };
            btn.Click += (s, e) => { SetActiveButton(btn); clickAction(); };
            sidebarPanel.Controls.Add(btn);
        }

        private void SetActiveButton(Button btn)
        {
            if (currentActiveButton != null)
            {
                currentActiveButton.BackColor = Color.FromArgb(45, 55, 75);
                currentActiveButton.ForeColor = Color.FromArgb(200, 200, 200);
                currentActiveButton.Font = new Font("Segoe UI", 11);
            }
            currentActiveButton = btn;
            btn.BackColor = Color.FromArgb(60, 70, 90);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        // ============================================================ ДАШБОРД
        private void ShowDashboard()
        {
            contentPanel.Controls.Clear();

            // ✅ КАРТОЧКИ СТАТИСТИКИ
            FlowLayoutPanel statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20),
                Height = 190,
                AutoScroll = false,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            AddStatCard(statsPanel, "🏛️ Факультеты", Count("SELECT COUNT(*) FROM Faculties"), Color.FromArgb(0, 122, 204));
            AddStatCard(statsPanel, "👨‍🏫 Тренеры", Count("SELECT COUNT(*) FROM Trainers"), Color.FromArgb(40, 167, 69));
            AddStatCard(statsPanel, "🎓 Студенты", Count("SELECT COUNT(*) FROM Students"), Color.FromArgb(255, 193, 7));
            AddStatCard(statsPanel, "⚽ Секции", Count("SELECT COUNT(*) FROM Sections"), Color.FromArgb(220, 53, 69));
            AddStatCard(statsPanel, "📋 Посещаемость", Count("SELECT COUNT(*) FROM Attendance"), Color.FromArgb(108, 117, 125));

            contentPanel.Controls.Add(statsPanel);
        }

        private void AddStatCard(FlowLayoutPanel p, string title, string val, Color c)
        {
            var card = new Panel
            {
                Size = new Size(200, 140),
                Margin = new Padding(15),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var t = new Label { Text = title, Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
            var v = new Label { Text = val, Location = new Point(15, 50), Font = new Font("Segoe UI", 32, FontStyle.Bold), ForeColor = c, AutoSize = true };
            card.Controls.AddRange(new Control[] { t, v });
            p.Controls.Add(card);
        }

        private string Count(string q)
        {
            try { return DBConnection.Instance.ExecuteScalar(q)?.ToString() ?? "0"; }
            catch { return "0"; }
        }

        // ============================================================ ТАБЛИЦЫ
        private void ShowGrid(string tableType)
        {
            currentTableType = tableType;
            headerLabel.Text = GetHeaderTitle(tableType);
            contentPanel.Controls.Clear();

            // ПАНЕЛЬ ПОИСКА
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblSearch = new Label
            {
                Text = "🔍 Поиск:",
                Location = new Point(15, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtSearch = new TextBox
            {
                Location = new Point(80, 15),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 9),
                PlaceholderText = "Введите для поиска...",
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => ApplySearch();

            var btnClear = new Button
            {
                Text = "❌ Очистить",
                Location = new Point(390, 14),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };
            btnClear.Click += (s, e) => { txtSearch.Clear(); ApplySearch(); };

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnClear });

            // ПАНЕЛЬ ФИЛЬТРОВ (для посещаемости)
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = (tableType == "Посещаемость")
            };

            if (tableType == "Посещаемость")
            {
                var lblDate = new Label
                {
                    Text = "📅 Дата посещения:",
                    Location = new Point(15, 18),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                dtpDate = new DateTimePicker
                {
                    Location = new Point(160, 15),
                    Size = new Size(150, 25),
                    Format = DateTimePickerFormat.Short,
                    Font = new Font("Segoe UI", 9)
                };
                dtpDate.Value = DateTime.Now;

                var btnLoad = new Button
                {
                    Text = "📋 Загрузить",
                    Location = new Point(325, 14),
                    Size = new Size(110, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9)
                };
                btnLoad.Click += (s, e) => LoadGridData();

                var btnToday = new Button
                {
                    Text = "📅 Сегодня",
                    Location = new Point(445, 14),
                    Size = new Size(100, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9)
                };
                btnToday.Click += (s, e) => { dtpDate.Value = DateTime.Now; LoadGridData(); };

                filterPanel.Controls.AddRange(new Control[] { lblDate, dtpDate, btnLoad, btnToday });
            }

            // ПАНЕЛЬ КНОПОК CRUD
            var btnPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnRefresh = CreateCrudButton("🔄 Обновить", 15, LoadGridData);

            if (tableType == "Посещаемость")
            {
                var btnAdd = CreateCrudButton("➕ Отметить", 135, OnAddAttendance, Color.FromArgb(40, 167, 69));
                var btnEdit = CreateCrudButton("✏️ Изменить", 255, OnEditAttendance);
                var btnDelete = CreateCrudButton("🗑️ Удалить", 375, OnDeleteAttendance, Color.FromArgb(220, 53, 69));
                btnPanel.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnEdit, btnDelete });
            }
            else
            {
                var btnAdd = CreateCrudButton("➕ Добавить", 135, OnAdd, Color.FromArgb(40, 167, 69));
                var btnEdit = CreateCrudButton("✏️ Изменить", 255, OnEdit);
                var btnDelete = CreateCrudButton("🗑️ Удалить", 375, OnDelete, Color.FromArgb(220, 53, 69));
                btnPanel.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnEdit, btnDelete });
            }

            // ТАБЛИЦА
            currentDgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            currentDgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 230, 230);
            currentDgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(45, 55, 75);

            contentPanel.Controls.Add(currentDgv);
            contentPanel.Controls.Add(btnPanel);
            if (filterPanel != null) contentPanel.Controls.Add(filterPanel);
            contentPanel.Controls.Add(searchPanel);

            LoadGridData();
        }

        private string GetHeaderTitle(string tableType)
        {
            return tableType switch
            {
                "Студенты" => "🎓 Управление студентами",
                "Тренеры" => "🏆 Управление тренерами",
                "Секции" => "⚽ Управление секциями",
                "Посещаемость" => "📋 Отметка посещаемости",
                _ => "Панель управления"
            };
        }

        private Button CreateCrudButton(string text, int x, Action click, Color? bgColor = null)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 15),
                Size = new Size(110, 32),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };

            if (bgColor.HasValue)
            {
                btn.BackColor = bgColor.Value;
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.BackColor = Color.White;
                btn.ForeColor = Color.FromArgb(45, 55, 75);
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            }

            btn.Click += (s, e) => click();
            return btn;
        }

        private void ApplySearch()
        {
            if (currentDgv == null || string.IsNullOrEmpty(txtSearch.Text))
            {
                LoadGridData();
                return;
            }

            string searchText = txtSearch.Text.ToLower();
            DataTable dt = currentDgv.DataSource as DataTable;
            if (dt == null) return;

            DataTable filtered = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    if (row[col].ToString().ToLower().Contains(searchText))
                    {
                        filtered.ImportRow(row);
                        break;
                    }
                }
            }

            currentDgv.DataSource = filtered;
        }

        private void LoadGridData()
        {
            if (currentDgv == null || string.IsNullOrEmpty(currentTableType)) return;

            try
            {
                string query = "";
                switch (currentTableType)
                {
                    case "Студенты":
                        query = @"
                            SELECT s.StudentCardNumber AS [Номер билета],
                            s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.MiddleName, '') AS [ФИО],
                            f.FacultyName AS [Факультет], s.GroupName AS [Группа],
                            s.Course AS [Курс], s.Phone AS [Телефон]
                            FROM Students s
                            JOIN Faculties f ON s.FacultyID = f.FacultyID
                            ORDER BY s.StudentCardNumber";
                        break;
                    case "Тренеры":
                        query = @"
                            SELECT TrainerID AS [ID], DocumentNumber AS [Документ],
                            LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS [ФИО],
                            Qualification AS [Квалификация], Specialization AS [Специализация],
                            Phone AS [Телефон], HireDate AS [Дата приёма]
                            FROM Trainers ORDER BY LastName";
                        break;
                    case "Секции":
                        query = @"
                            SELECT sec.SectionID AS [ID], sec.SectionName AS [Название],
                            sp.SportName AS [Вид спорта],
                            t.LastName + ' ' + t.FirstName AS [Тренер],
                            sec.MaxStudents AS [Макс. студентов], sec.PricePerMonth AS [Цена]
                            FROM Sections sec
                            JOIN Sports sp ON sec.SportID = sp.SportID
                            JOIN Trainers t ON sec.TrainerID = t.TrainerID
                            ORDER BY sec.SectionName";
                        break;
                    case "Посещаемость":
                        DateTime visitDate = dtpDate?.Value ?? DateTime.Now;
                        query = $@"
                            SELECT
                            a.AttendanceID AS [ID],
                            s.StudentCardNumber AS [Билет],
                            s.LastName + ' ' + s.FirstName AS [Студент],
                            sec.SectionName AS [Секция],
                            a.VisitDate AS [Дата],
                            CASE WHEN a.Status = 1 THEN 'Присутствовал' ELSE 'Отсутствовал' END AS [Статус],
                            ISNULL(a.Notes, '') AS [Примечание]
                            FROM Attendance a
                            JOIN Students s ON a.StudentCardNumber = s.StudentCardNumber
                            JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                            JOIN Sections sec ON sc.SectionID = sec.SectionID
                            WHERE CAST(a.VisitDate AS DATE) = '{visitDate:yyyy-MM-dd}'
                            ORDER BY s.LastName";
                        break;
                }

                currentDgv.DataSource = DBConnection.Instance.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ==================== НОВАЯ ВКЛАДКА: УПРАВЛЕНИЕ ====================
        private void ShowManagement()
        {
            headerLabel.Text = "🔧 Управление системой";
            contentPanel.Controls.Clear();

            // ✅ Контейнер с сеткой (2 колонки)
            var gridPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(30),
                AutoScroll = true,
                BackColor = Color.FromArgb(240, 240, 245)
            };
            gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            gridPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            gridPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            gridPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // ✅ Современные карточки (по принципам Material Design 3)
            AddModernCard(gridPanel, "🏛️", "Факультеты",
                "Управление списком факультетов",
                Color.FromArgb(0, 122, 204),
                () => new PolesSU_Sports.Admin.Forms.Dictionary.FacultyForm().ShowDialog());

            AddModernCard(gridPanel, "⚽", "Виды спорта",
                "Справочник видов спорта",
                Color.FromArgb(0, 122, 204),
                () => new PolesSU_Sports.Admin.Forms.Dictionary.SportForm().ShowDialog());

            AddModernCard(gridPanel, "📅", "Расписание",
                "Настройка расписания занятий",
                Color.FromArgb(0, 122, 204),
                () => new PolesSU_Sports.Admin.Forms.Dictionary.ScheduleForm().ShowDialog());

            AddModernCard(gridPanel, "🔐", "Аккаунты",
                "Управление пользователями и ролями",
                Color.FromArgb(40, 167, 69),
                () => new PolesSU_Sports.Admin.Forms.Dictionary.AccountForm().ShowDialog());

            AddModernCard(gridPanel, "📋", "Запись в секции",
                "Зачисление студентов в секции",
                Color.FromArgb(255, 193, 7),
                () => new PolesSU_Sports.Admin.Students.StudentSectionForm().ShowDialog());

            AddModernCard(gridPanel, "🏆", "Достижения",
                "Учёт спортивных достижений",
                Color.FromArgb(220, 53, 69),
                ShowAchievements);

            contentPanel.Controls.Add(gridPanel);
        }

        // ✅ СОВРЕМЕННАЯ КАРТОЧКА (по принципам Material Design 3) [[12]][[4]]
        private void AddModernCard(TableLayoutPanel grid, string icon, string title, string desc, Color accent, Action click)
        {
            var card = new Panel
            {
                Size = new Size(280, 160),
                Margin = new Padding(15),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                Tag = accent
            };

            // ✅ Тень (визуальная глубина)
            card.Paint += (s, e) =>
            {
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddRectangle(new Rectangle(0, 0, card.Width - 1, card.Height - 1));
                    using (var shadow = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        shadow.CenterColor = Color.FromArgb(30, 0, 0, 0);
                        shadow.SurroundColors = new[] { Color.Transparent };
                        e.Graphics.FillRectangle(shadow, new Rectangle(0, 0, card.Width, card.Height));
                    }
                }
                // ✅ Акцентная линия снизу
                using (var pen = new Pen(accent, 3))
                {
                    e.Graphics.DrawLine(pen, 0, card.Height - 1, card.Width, card.Height - 1);
                }
            };

            // ✅ Иконка в круге (Material Design)
            var iconCircle = new Panel
            {
                Size = new Size(56, 56),
                Location = new Point(20, 20),
                BackColor = Color.Transparent,  // 15% прозрачности
                BorderStyle = BorderStyle.None
            };
            iconCircle.Paint += (s, e) =>
            {
                using (var brush = new SolidBrush(Color.FromArgb(15, accent.R, accent.G, accent.B)))
                using (var pen = new Pen(accent, 2))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 56, 56);
                    e.Graphics.DrawEllipse(pen, 0, 0, 55, 55);
                }
            };

            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 22, FontStyle.Regular),
                AutoSize = false,
                Size = new Size(56, 56),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = accent,
                Dock = DockStyle.Fill
            };

            // ✅ Заголовок + описание
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),  // ✅ Bold - корректное значение
                ForeColor = Color.FromArgb(45, 55, 75),
                Location = new Point(90, 22),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var descLabel = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(90, 48),
                Size = new Size(170, 40),
                AutoSize = false,
                BackColor = Color.Transparent
            };

            // ✅ Индикатор перехода (стрелка)
           /* var arrow = new Label
            {
                Text = "→",
                Font = new Font("Segoe UI", 18, FontStyle.Regular),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(245, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };*/

            // ✅ Hover эффекты (плавные)
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = Color.FromArgb(252, 252, 252);
                iconCircle.BackColor = Color.FromArgb(25, accent.R, accent.G, accent.B);
                //arrow.ForeColor = accent;
                //arrow.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = Color.White;
                iconCircle.BackColor = Color.FromArgb(15, accent.R, accent.G, accent.B);
               // arrow.ForeColor = Color.FromArgb(180, 180, 180);
                //arrow.Font = new Font("Segoe UI", 18, FontStyle.Regular);
            };

            // ✅ Клик по всей карточке
            card.Click += (s, e) => click();
            iconLabel.Click += (s, e) => click();
            titleLabel.Click += (s, e) => click();
            descLabel.Click += (s, e) => click();

            iconCircle.Controls.Add(iconLabel);
            card.Controls.AddRange(new Control[] { iconCircle, titleLabel, descLabel, /*arrow*/ });
            grid.Controls.Add(card);
        }

        // ============================================================ CRUD
        private void OnAdd()
        {
            try
            {
                switch (currentTableType)
                {
                    case "Студенты":
                        using (var form = new PolesSU_Sports.Admin.Students.StudentForm())
                        {
                            if (form.ShowDialog() == DialogResult.OK) LoadGridData();
                        }
                        break;
                    case "Тренеры":
                        using (var form = new PolesSU_Sports.Admin.Trainers.TrainerForm())
                        {
                            if (form.ShowDialog() == DialogResult.OK) LoadGridData();
                        }
                        break;
                    case "Секции":
                        using (var form = new PolesSU_Sports.Admin.Sections.SectionForm())
                        {
                            if (form.ShowDialog() == DialogResult.OK) LoadGridData();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnEdit()
        {
            if (currentDgv?.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                switch (currentTableType)
                {
                    case "Студенты":
                        string card = currentDgv.SelectedRows[0].Cells["Номер билета"].Value.ToString();
                        using (var form = new PolesSU_Sports.Admin.Students.StudentForm(card))
                        {
                            if (form.ShowDialog() == DialogResult.OK) LoadGridData();
                        }
                        break;
                    case "Тренеры":
                        int tid = Convert.ToInt32(currentDgv.SelectedRows[0].Cells["ID"].Value);
                        using (var form = new PolesSU_Sports.Admin.Trainers.TrainerForm(tid))
                        {
                            if (form.ShowDialog() == DialogResult.OK) LoadGridData();
                        }
                        break;
                    case "Секции":
                        int sid = Convert.ToInt32(currentDgv.SelectedRows[0].Cells["ID"].Value);
                        using (var form = new PolesSU_Sports.Admin.Sections.SectionForm(sid))
                        {
                            if (form.ShowDialog() == DialogResult.OK) LoadGridData();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDelete()
        {
            if (currentDgv?.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                bool deleted = false;
                switch (currentTableType)
                {
                    case "Студенты":
                        string card = currentDgv.SelectedRows[0].Cells["Номер билета"].Value.ToString();
                        if (!DBConnection.Instance.CanDeleteStudent(card))
                        {
                            MessageBox.Show("❌ Есть связанные записи", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        deleted = DBConnection.Instance.DeleteStudent(card);
                        break;
                    case "Тренеры":
                        int tid = Convert.ToInt32(currentDgv.SelectedRows[0].Cells["ID"].Value);
                        deleted = DBConnection.Instance.DeleteTrainer(tid);
                        break;
                    case "Секции":
                        int sid = Convert.ToInt32(currentDgv.SelectedRows[0].Cells["ID"].Value);
                        deleted = DBConnection.Instance.DeleteSection(sid);
                        break;
                }

                if (deleted)
                {
                    MessageBox.Show("✅ Удалено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGridData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnAddAttendance()
        {
            using (var form = new PolesSU_Sports.Admin.Attendance.AttendanceMarkForm(dtpDate.Value))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadGridData();
                }
            }
        }

        private void OnEditAttendance()
        {
            if (currentDgv?.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int attendanceID = Convert.ToInt32(currentDgv.SelectedRows[0].Cells["ID"].Value);
                string studentCard = currentDgv.SelectedRows[0].Cells["Билет"].Value.ToString();
                DateTime visitDate = dtpDate.Value;

                using (var form = new PolesSU_Sports.Admin.Attendance.AttendanceEditForm(attendanceID, studentCard, visitDate))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadGridData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDeleteAttendance()
        {
            if (currentDgv?.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить запись о посещении?\nЭто действие нельзя отменить!", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                int attendanceID = Convert.ToInt32(currentDgv.SelectedRows[0].Cells["ID"].Value);
                int result = DBConnection.Instance.ExecuteCommand(
                    "DELETE FROM Attendance WHERE AttendanceID = @AttendanceID",
                    new[] { new Microsoft.Data.SqlClient.SqlParameter("@AttendanceID", attendanceID) });

                if (result > 0)
                {
                    MessageBox.Show("✅ Запись о посещении удалена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGridData();
                }
                else
                {
                    MessageBox.Show("❌ Не удалось удалить", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAchievements()
        {
            headerLabel.Text = "🏆 Достижения студентов";
            contentPanel.Controls.Clear();

            var btnPanel = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var btnAdd = new Button
            {
                Text = "➕ Добавить достижение",
                Location = new Point(15, 15),
                Size = new Size(200, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };
            btnAdd.Click += (s, e) =>
            {
                using (var form = new PolesSU_Sports.Admin.Forms.Dictionary.Achievements.AchievementForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                        LoadAchievements();
                }
            };
            btnPanel.Controls.Add(btnAdd);

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                BackgroundColor = Color.White,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                },
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            contentPanel.Controls.Add(dgv);
            contentPanel.Controls.Add(btnPanel);
            LoadAchievements(dgv);
        }

        private void LoadAchievements(DataGridView dgv = null)
        {
            if (dgv == null)
            {
                foreach (Control c in contentPanel.Controls)
                {
                    if (c is DataGridView) { dgv = c as DataGridView; break; }
                }
            }

            if (dgv != null)
            {
                dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
                    SELECT a.AchievementID AS [ID],
                    s.StudentCardNumber AS [Билет],
                    s.LastName + ' ' + s.FirstName AS [Студент],
                    sec.SectionName AS [Секция],
                    a.CompetitionName AS [Соревнование],
                    a.CompetitionDate AS [Дата],
                    a.Place AS [Место],
                    a.AwardType AS [Награда],
                    a.AwardDescription AS [Описание]
                    FROM Achievements a
                    JOIN Students s ON a.StudentCardNumber = s.StudentCardNumber
                    LEFT JOIN Sections sec ON a.SectionID = sec.SectionID
                    ORDER BY a.CompetitionDate DESC");
            }
        }
    }
}