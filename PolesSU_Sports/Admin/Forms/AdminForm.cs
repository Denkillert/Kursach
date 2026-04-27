using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;

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

        // ✅ ЦВЕТА ПОЛЕСГУ
        private readonly Color GreenMain = Color.FromArgb(5, 66, 38);
        private readonly Color GreenLight = Color.FromArgb(6, 87, 50);
        private readonly Color BlueAccent = Color.FromArgb(0, 147, 197);
        private readonly Color GrayBg = Color.FromArgb(245, 247, 250);

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
            this.BackColor = Color.White;
            this.MinimumSize = new Size(1200, 700);

            // 1. SIDEBAR (слева) - тёмно-зелёный
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = GreenMain,
                Padding = new Padding(0)
            };

            // Логотип
            var logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                BackColor = GreenMain,
                Padding = new Padding(0)
            };

            var logoPictureBox = new PictureBox
            {
                Image = Properties.Resources.PolesSU_Emblem,
                Size = new Size(160, 160),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Location = new Point(40, 102)  // ✅ Эмблема НИЖЕ
            };

            var logoText = new Label
            {
                Text = "PolesSU Sports",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(220, 120),
                Location = new Point(20, 10),  // ✅ Текст ВВЕРХУ
                TextAlign = ContentAlignment.TopRight
                
            };

            logoPanel.Controls.AddRange(new Control[] { logoText, logoPictureBox });
            sidebarPanel.Controls.Add(logoPanel);

            // Разделитель
            var separator1 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(10, 100, 60)
            };
            sidebarPanel.Controls.Add(separator1);

            // Кнопки меню
            CreateMenuButton("📊 Главная", ShowDashboard, true);
            CreateMenuButton("🎓 Студенты", () => ShowGrid("Студенты"), false);
            CreateMenuButton("🏆 Тренеры", () => ShowGrid("Тренеры"), false);
            CreateMenuButton("⚽ Секции", () => ShowGrid("Секции"), false);
            CreateMenuButton("📋 Посещаемость", () => ShowGrid("Посещаемость"), false);
            CreateMenuButton("🔧 Управление", ShowManagement, false);

            // Spacer
            var spacer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            sidebarPanel.Controls.Add(spacer);

            // Кнопка выхода (внизу)
            var exitPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(180, 50, 50),
                Padding = new Padding(10)
            };
            var exitBtn = new Button
            {
                Text = "🚪 Выйти из системы",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            exitBtn.FlatAppearance.BorderSize = 0;
            exitBtn.Click += (s, e) =>
            {
                if (MessageBox.Show("Выйти из системы?", "Выход",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    User.Logout();
                    this.DialogResult = DialogResult.Retry;
                    this.Close();
                }
            };
            exitPanel.Controls.Add(exitBtn);
            sidebarPanel.Controls.Add(exitPanel);

            // 2. HEADER (сверху) - белый
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(30, 15, 30, 15)
            };

            headerLabel = new Label
            {
                Text = "Главная",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = GreenMain,
                AutoSize = true,
                Location = new Point(30, 18)
            };

            // Информация о пользователе (справа)
            var userPanel = new Panel
            {
                Location = new Point(headerPanel.Width - 250, 15),
                Size = new Size(240, 40),
                BackColor = GrayBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            var userIcon = new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI", 16),
                Location = new Point(10, 5),
                Size = new Size(35, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var userInfo = new Label
            {
                Text = $"{User.CurrentUser?.Login ?? "Admin"}\nAdministrator",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(50, 6),
                Size = new Size(180, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            userPanel.Controls.AddRange(new Control[] { userIcon, userInfo });

            headerPanel.Controls.Add(headerLabel);
            headerPanel.Controls.Add(userPanel);

            // 3. CONTENT (основная область) - светло-серый фон
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                BackColor = GrayBg,
                AutoScroll = true
            };

            // Порядок добавления
            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(sidebarPanel);
        }

        private void CreateMenuButton(string text, Action clickAction, bool isActive = false)
        {
            var btn = new Button
            {
                Text = "  " + text,
                Dock = DockStyle.Top,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = isActive ? GreenLight : GreenMain,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, isActive ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(25, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) =>
            {
                if (btn != currentActiveButton)
                    btn.BackColor = GreenLight;
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != currentActiveButton)
                    btn.BackColor = GreenMain;
            };
            btn.Click += (s, e) =>
            {
                SetActiveButton(btn);
                clickAction();
            };
            sidebarPanel.Controls.Add(btn);

            if (isActive)
                currentActiveButton = btn;
        }

        private void SetActiveButton(Button btn)
        {
            if (currentActiveButton != null)
            {
                currentActiveButton.BackColor = GreenMain;
                currentActiveButton.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            }
            currentActiveButton = btn;
            btn.BackColor = GreenLight;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        // ============================================================ ДАШБОРД
        private void ShowDashboard()
        {
            contentPanel.Controls.Clear();
            headerLabel.Text = "Главная";

            // Заголовок с датой
            var dateLabel = new Label
            {
                Text = DateTime.Now.ToString("dd MMMM yyyy"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(30, 5),
                AutoSize = true
            };
            contentPanel.Controls.Add(dateLabel);

            // ✅ КАРТОЧКИ СТАТИСТИКИ
            FlowLayoutPanel statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20, 20, 20, 30),
                Height = 180,
                AutoScroll = false,
                BackColor = Color.Transparent
            };

            AddModernStatCard(statsPanel, "🏛️ Факультеты",
                Count("SELECT COUNT(*) FROM Faculties"),
                Color.FromArgb(0, 102, 204), "факультет(ов)");

            AddModernStatCard(statsPanel, "👨‍ Тренеры",
                Count("SELECT COUNT(*) FROM Trainers"),
                Color.FromArgb(40, 167, 69), "тренер(ов)");

            AddModernStatCard(statsPanel, "🎓 Студенты",
                Count("SELECT COUNT(*) FROM Students"),
                Color.FromArgb(255, 193, 7), "студент(ов)");

            AddModernStatCard(statsPanel, "⚽ Секции",
                Count("SELECT COUNT(*) FROM Sections"),
                Color.FromArgb(220, 53, 69), "секцй(и)");

            AddModernStatCard(statsPanel, "📋 Посещаемость",
                Count("SELECT COUNT(*) FROM Attendance"),
                Color.FromArgb(108, 117, 125), "записей");

            contentPanel.Controls.Add(statsPanel);

           /* // Быстрые действия
            var quickActionsLabel = new Label
            {
                Text = "Быстрые действия",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = GreenMain,
                Location = new Point(30, 210),
                AutoSize = true
            };
            contentPanel.Controls.Add(quickActionsLabel);*/

            var actionsPanel = new FlowLayoutPanel
            {
                Location = new Point(30, 245),
                Size = new Size(contentPanel.Width - 60, 100),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true
            };

           /* AddQuickActionButton(actionsPanel, "➕ Добавить студента",
                () => new PolesSU_Sports.Admin.Students.StudentForm().ShowDialog(), BlueAccent);
            AddQuickActionButton(actionsPanel, "📝 Отметить посещаемость",
                () => new PolesSU_Sports.Admin.Attendance.AttendanceMarkForm(DateTime.Now).ShowDialog(),
                Color.FromArgb(40, 167, 69));
            AddQuickActionButton(actionsPanel, "📅 Расписание",
                () => new PolesSU_Sports.Admin.Forms.Dictionary.ScheduleForm().ShowDialog(),
                Color.FromArgb(255, 193, 7));

            contentPanel.Controls.Add(actionsPanel);*/
        }

        private void AddModernStatCard(FlowLayoutPanel p, string title, string val, Color c, string suffix)
        {
            var card = new Panel
            {
                Size = new Size(220, 140),
                Margin = new Padding(15),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            // Тень
            card.Paint += (s, e) =>
            {
                using (var path = new GraphicsPath())
                {
                    path.AddRectangle(new Rectangle(0, 0, card.Width - 1, card.Height - 1));
                    using (var shadow = new PathGradientBrush(path))
                    {
                        shadow.CenterColor = Color.FromArgb(20, 0, 0, 0);
                        shadow.SurroundColors = new[] { Color.Transparent };
                        e.Graphics.FillRectangle(shadow, new Rectangle(0, 0, card.Width, card.Height));
                    }
                }
                // Акцентная линия слева
                using (var pen = new Pen(c, 4))
                {
                    e.Graphics.DrawLine(pen, 0, 10, 0, card.Height - 10);
                }
            };

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 100, 100),
                BackColor = Color.Transparent
            };

            var valueLabel = new Label
            {
                Text = val,
                Location = new Point(20, 45),
                Font = new Font("Segoe UI", 42, FontStyle.Bold),
                ForeColor = c,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var suffixLabel = new Label
            {
                Text = suffix,
                Location = new Point(20, 95),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[] { titleLabel, valueLabel, suffixLabel });
            p.Controls.Add(card);
        }

        /*private void AddQuickActionButton(FlowLayoutPanel p, string text, Action click, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(220, 50),
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Dark(color);
            btn.MouseLeave += (s, e) => btn.BackColor = color;
            btn.Click += (s, e) => click();
            p.Controls.Add(btn);
        }*/

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
                Height = 70,
                Padding = new Padding(10),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15)
            };

            var lblSearch = new Label
            {
                Text = "🔍 Поиск:",
                Location = new Point(15, 22),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = GreenMain
            };

            txtSearch = new TextBox
            {
                Location = new Point(100, 20),
                Size = new Size(350, 30),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Введите для поиска...",
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => ApplySearch();

            var btnClear = new Button
            {
                Text = "❌ Очистить",
                Location = new Point(450, 19),
                Size = new Size(110, 32),
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
                Height = 70,
                Padding = new Padding(10),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15),
                Visible = (tableType == "Посещаемость")
            };

            if (tableType == "Посещаемость")
            {
                var lblDate = new Label
                {
                    Text = "📅 Дата:",
                    Location = new Point(15, 22),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = GreenMain
                };

                dtpDate = new DateTimePicker
                {
                    Location = new Point(90, 20),
                    Size = new Size(180, 30),
                    Format = DateTimePickerFormat.Short,
                    Font = new Font("Segoe UI", 10)
                };
                dtpDate.Value = DateTime.Now;

                var btnLoad = new Button
                {
                    Text = "📋 Загрузить",
                    Location = new Point(285, 19),
                    Size = new Size(120, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = BlueAccent,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9)
                };
                btnLoad.Click += (s, e) => LoadGridData();

                var btnToday = new Button
                {
                    Text = "📅 Сегодня",
                    Location = new Point(415, 19),
                    Size = new Size(110, 32),
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
                Margin = new Padding(0, 0, 0, 15)
            };

            var btnRefresh = CreateCrudButton("🔄 Обновить", 15, LoadGridData);

            if (tableType == "Посещаемость")
            {
                var btnAdd = CreateCrudButton("➕ Отметить", 145, OnAddAttendance, Color.FromArgb(40, 167, 69));
                var btnEdit = CreateCrudButton("✏️ Изменить", 275, OnEditAttendance);
                var btnDelete = CreateCrudButton("🗑️ Удалить", 405, OnDeleteAttendance, Color.FromArgb(220, 53, 69));
                btnPanel.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnEdit, btnDelete });
            }
            else
            {
                var btnAdd = CreateCrudButton("➕ Добавить", 145, OnAdd, Color.FromArgb(40, 167, 69));
                var btnEdit = CreateCrudButton("✏️ Изменить", 275, OnEdit);
                var btnDelete = CreateCrudButton("🗑️ Удалить", 405, OnDelete, Color.FromArgb(220, 53, 69));
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
                    BackColor = GreenMain,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(10)
                },
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            // ✅ RowTemplate.Height задаём ОТДЕЛЬНО:
            currentDgv.RowTemplate.Height = 40;
            currentDgv.DefaultCellStyle.SelectionBackColor = BlueAccent;
            currentDgv.DefaultCellStyle.SelectionForeColor = Color.White;
            currentDgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            currentDgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            // Добавляем таблицу в панель с белым фоном
            var tableContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            tableContainer.Controls.Add(currentDgv);

            contentPanel.Controls.Add(tableContainer);
            contentPanel.Controls.Add(btnPanel);
            if (filterPanel != null) contentPanel.Controls.Add(filterPanel);
            contentPanel.Controls.Add(searchPanel);

            LoadGridData();
        }

        private string GetHeaderTitle(string tableType)
        {
            return tableType switch
            {
                "Студенты" => "🎓 Студенты",
                "Тренеры" => "🏆 Тренеры",
                "Секции" => "⚽ Секции",
                "Посещаемость" => "📋 Посещаемость",
                _ => "Панель управления"
            };
        }

        private Button CreateCrudButton(string text, int x, Action click, Color? bgColor = null)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 14),
                Size = new Size(120, 35),
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
                btn.ForeColor = GreenMain;
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
                            CASE WHEN a.Status = 1 THEN '✅ Присутствовал' ELSE '❌ Отсутствовал' END AS [Статус],
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
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================ УПРАВЛЕНИЕ
        private void ShowManagement()
        {
            headerLabel.Text = "🔧 Управление системой";
            contentPanel.Controls.Clear();

            var gridPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(30),
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            gridPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            gridPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            gridPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            AddModernCard(gridPanel, "🏛️", "Факультеты",
                "Управление списком факультетов",
                Color.FromArgb(0, 102, 204),
                () => new PolesSU_Sports.Admin.Forms.Dictionary.FacultyForm().ShowDialog());

            AddModernCard(gridPanel, "⚽", "Виды спорта",
                "Справочник видов спорта",
                Color.FromArgb(0, 102, 204),
                () => new PolesSU_Sports.Admin.Forms.Dictionary.SportForm().ShowDialog());

            AddModernCard(gridPanel, "📅", "Расписание",
                "Настройка расписания занятий",
                BlueAccent,
                () => new PolesSU_Sports.Admin.Forms.Dictionary.ScheduleForm().ShowDialog());

            AddModernCard(gridPanel, "🔐", "Аккаунты",
                "Управление пользователями",
                Color.FromArgb(40, 167, 69),
                () => new PolesSU_Sports.Admin.Forms.Dictionary.AccountForm().ShowDialog());

            AddModernCard(gridPanel, "📋", "Запись в секции",
                "Зачисление студентов",
                Color.FromArgb(255, 193, 7),
                () => new PolesSU_Sports.Admin.Students.StudentSectionForm().ShowDialog());

            AddModernCard(gridPanel, "🏆", "Достижения",
                "Учёт спортивных достижений",
                Color.FromArgb(220, 53, 69),
                ShowAchievements);

            contentPanel.Controls.Add(gridPanel);
        }

        private void AddModernCard(TableLayoutPanel grid, string icon, string title, string desc, Color accent, Action click)
        {
            var card = new Panel
            {
                Size = new Size(300, 150),
                Margin = new Padding(20),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand
            };

            // Тень и акцентная линия
            card.Paint += (s, e) =>
            {
                using (var path = new GraphicsPath())
                {
                    path.AddRectangle(new Rectangle(0, 0, card.Width - 1, card.Height - 1));
                    using (var shadow = new PathGradientBrush(path))
                    {
                        shadow.CenterColor = Color.FromArgb(25, 0, 0, 0);
                        shadow.SurroundColors = new[] { Color.Transparent };
                        e.Graphics.FillRectangle(shadow, new Rectangle(0, 0, card.Width, card.Height));
                    }
                }
                using (var pen = new Pen(accent, 4))
                {
                    e.Graphics.DrawLine(pen, 0, 0, card.Width, 0);
                }
            };

            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 32),
                Location = new Point(25, 20),
                Size = new Size(60, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = GreenMain,
                Location = new Point(100, 25),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var descLabel = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(100, 50),
                Size = new Size(180, 40),
                AutoSize = false,
                BackColor = Color.Transparent
            };

            var arrowLabel = new Label
            {
                Text = "→",
                Font = new Font("Segoe UI", 24),
                ForeColor = accent,
                Location = new Point(260, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(250, 250, 250);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;

            card.Click += (s, e) => click();
            iconLabel.Click += (s, e) => click();
            titleLabel.Click += (s, e) => click();
            descLabel.Click += (s, e) => click();
            arrowLabel.Click += (s, e) => click();

            card.Controls.AddRange(new Control[] { iconLabel, titleLabel, descLabel, arrowLabel });
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
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnEdit()
        {
            if (currentDgv?.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        string tid = currentDgv.SelectedRows[0].Cells["ID"].Value.ToString();
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
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDelete()
        {
            if (currentDgv?.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить запись?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                bool deleted = false;
                switch (currentTableType)
                {
                    case "Студенты":
                        string card = currentDgv.SelectedRows[0].Cells["Номер билета"].Value.ToString();
                        if (!DBConnection.Instance.CanDeleteStudent(card))
                        {
                            MessageBox.Show("❌ Есть связанные записи", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        deleted = DBConnection.Instance.DeleteStudent(card);
                        break;
                    case "Тренеры":
                        string tid = currentDgv.SelectedRows[0].Cells["ID"].Value.ToString();
                        deleted = DBConnection.Instance.DeleteTrainer(tid);
                        break;
                    case "Секции":
                        int sid = Convert.ToInt32(currentDgv.SelectedRows[0].Cells["ID"].Value);
                        deleted = DBConnection.Instance.DeleteSection(sid);
                        break;
                }

                if (deleted)
                {
                    MessageBox.Show("✅ Удалено", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGridData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Выберите запись для редактирования", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnDeleteAttendance()
        {
            if (currentDgv?.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить запись о посещении?\nЭто действие нельзя отменить!",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                int attendanceID = Convert.ToInt32(currentDgv.SelectedRows[0].Cells["ID"].Value);
                int result = DBConnection.Instance.ExecuteCommand(
                    "DELETE FROM Attendance WHERE AttendanceID = @AttendanceID",
                    new[] { new Microsoft.Data.SqlClient.SqlParameter("@AttendanceID", attendanceID) });

                if (result > 0)
                {
                    MessageBox.Show("✅ Запись о посещении удалена", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGridData();
                }
                else
                {
                    MessageBox.Show("❌ Не удалось удалить", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAchievements()
        {
            headerLabel.Text = "🏆 Достижения студентов";
            contentPanel.Controls.Clear();

            var btnPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15)
            };

            var btnAdd = new Button
            {
                Text = "➕ Добавить достижение",
                Location = new Point(15, 14),
                Size = new Size(220, 35),
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
                    BackColor = GreenMain,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                },
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None
            };
            // ✅ RowTemplate.Height задаём ОТДЕЛЬНО:
            dgv.RowTemplate.Height = 40;

            var container = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            container.Controls.Add(dgv);

            contentPanel.Controls.Add(container);
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