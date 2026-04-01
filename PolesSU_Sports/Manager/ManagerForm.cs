using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;
using PolesSU_Sports.Shared.Model;

namespace PolesSU_Sports.Manager
{
    public partial class ManagerForm : Form
    {
        private Panel contentPanel;
        private Panel menuPanel;
        private Panel headerPanel;
        private DataGridView currentDgv;
        private string currentTableType;
        private TextBox txtSearch;
        private DateTimePicker dtpDate;
        private Panel filterPanel;

        public ManagerForm()
        {
            InitializeComponent();
            this.Text = "ПолесГУ Спорт — Панель руководителя";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            CreateHeader();
            CreateMenuAndContent();
            ShowDashboard();
        }

        // ============================================================ ШАПКА
        private void CreateHeader()
        {
            headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(this.Width, 60),
                BackColor = Color.FromArgb(0, 122, 204),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblWelcome = new Label
            {
                Text = $"Добро пожаловать, {User.CurrentUser?.FullName ?? "Пользователь"}",
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };

            Label lblRole = new Label
            {
                Text = $"Роль: {User.CurrentUser?.Role}",
                Font = new Font("Microsoft Sans Serif", 10),
                ForeColor = Color.White,
                Location = new Point(20, 35),
                AutoSize = true
            };

            Button btnLogout = new Button
            {
                Text = "🚪 Выйти",
                Location = new Point(this.Width - 120, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLogout.Click += BtnLogout_Click;

            headerPanel.Controls.AddRange(new Control[] { lblWelcome, lblRole, btnLogout });
            this.Controls.Add(headerPanel);
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Выйти из системы?", "Выход",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                User.Logout();
                this.Close();
            }
        }

        // ============================================================ МЕНЮ 
        private void CreateMenuAndContent()
        {
            menuPanel = new Panel
            {
                Location = new Point(0, 63),
                Size = new Size(this.Width, 50),
                BackColor = Color.FromArgb(240, 240, 240),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Panel separator = new Panel
            {
                Location = new Point(0, 113),
                Size = new Size(this.Width, 3),
                BackColor = Color.FromArgb(0, 122, 204),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            contentPanel = new Panel
            {
                Location = new Point(0, 116),
                Size = new Size(this.Width, this.Height - 116),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15),
                AutoScroll = true
            };

            CreateMenuButton("📊 Дашборд", 5, ShowDashboard);
            CreateMenuButton("📈 Аналитика", 140, ShowAnalytics);
            CreateMenuButton("📋 Посещаемость", 275, () => ShowGrid("Посещаемость"));
            CreateMenuButton("💰 Финансы", 410, ShowFinance);
            CreateMenuButton("📑 Отчёты", 545, ShowReports);
            CreateMenuButton("📝 Заявки", 680, ShowRequests);
            CreateMenuButton("⚙️ Доступ", 815, ShowAccess);

            this.Controls.Add(menuPanel);
            this.Controls.Add(separator);
            this.Controls.Add(contentPanel);
        }

        private void CreateMenuButton(string text, int x, Action click)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, 7),
                Size = new Size(125, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Microsoft Sans Serif", 9),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.Click += (s, e) => click();
            menuPanel.Controls.Add(btn);
        }

        // ============================================================ ДАШБОРД 
        private void ShowDashboard()
        {
            contentPanel.Controls.Clear();

            // Карточки статистики
            FlowLayoutPanel statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10),
                Height = 180,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            AddCard(statsPanel, "🎓 Всего студентов", Count("SELECT COUNT(*) FROM Students"), Color.FromArgb(0, 122, 204));
            AddCard(statsPanel, "👨‍ Тренеров", Count("SELECT COUNT(*) FROM Trainers"), Color.FromArgb(40, 167, 69));
            AddCard(statsPanel, "⚽ Секций", Count("SELECT COUNT(*) FROM Sections"), Color.FromArgb(255, 193, 7));
            AddCard(statsPanel, "📋 Посещений (мес)", Count("SELECT COUNT(*) FROM Attendance WHERE MONTH(VisitDate) = MONTH(GETDATE())"), Color.FromArgb(220, 53, 69));

            // Кнопки управления
            FlowLayoutPanel buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(30),
                BackColor = Color.White
            };

            // Запись в секции
            Button btnEnrollment = new Button
            {
                Text = "📋 Запись в секции",
                Size = new Size(200, 70),
                Margin = new Padding(15),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold)
            };
            btnEnrollment.Click += (s, e) => new PolesSU_Sports.Admin.Students.StudentSectionForm().ShowDialog();
            buttonsPanel.Controls.Add(btnEnrollment);

            // Аналитика посещаемости
            Button btnAttendance = new Button
            {
                Text = "📈 Посещаемость",
                Size = new Size(200, 70),
                Margin = new Padding(15),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold)
            };
            btnAttendance.Click += (s, e) => ShowAnalytics();
            buttonsPanel.Controls.Add(btnAttendance);

            // Финансы
            Button btnFinance = new Button
            {
                Text = "💰 Финансы",
                Size = new Size(200, 70),
                Margin = new Padding(15),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold)
            };
            btnFinance.Click += (s, e) => ShowFinance();
            buttonsPanel.Controls.Add(btnFinance);

            // Заявки (заглушка)
            Button btnRequests = new Button
            {
                Text = "📝 Заявки студентов",
                Size = new Size(200, 70),
                Margin = new Padding(15),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold)
            };
            btnRequests.Click += (s, e) => ShowRequests();
            buttonsPanel.Controls.Add(btnRequests);

            contentPanel.Controls.Add(statsPanel);
            contentPanel.Controls.Add(buttonsPanel);
        }

        private void AddCard(FlowLayoutPanel p, string title, string val, Color c)
        {
            Panel card = new Panel
            {
                Size = new Size(200, 130),
                Margin = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Label t = new Label { Text = title, Location = new Point(10, 10), AutoSize = true };
            Label v = new Label { Text = val, Location = new Point(10, 45), Font = new Font("Microsoft Sans Serif", 26, FontStyle.Bold), ForeColor = c, AutoSize = true };
            card.Controls.AddRange(new Control[] { t, v });
            p.Controls.Add(card);
        }

        private string Count(string q)
        {
            try { return DBConnection.Instance.ExecuteScalar(q)?.ToString() ?? "0"; }
            catch { return "0"; }
        }

        // ============================================================ АНАЛИТИКА
        private void ShowAnalytics()
        {
            contentPanel.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "📈 Аналитика посещаемости",
                Font = new Font("Microsoft Sans Serif", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Фильтры
            Panel filterPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(400, 50)
            };

            Label lblSection = new Label { Text = "Секция:", Location = new Point(0, 15), AutoSize = true };
            ComboBox cmbSection = new ComboBox
            {
                Location = new Point(60, 12),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Загрузка секций
            DataTable sections = DBConnection.Instance.ExecuteQuery("SELECT SectionID, SectionName FROM Sections ORDER BY SectionName");
            cmbSection.DataSource = sections;
            cmbSection.DisplayMember = "SectionName";
            cmbSection.ValueMember = "SectionID";

            Button btnShow = new Button
            {
                Text = "Показать",
                Location = new Point(270, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            filterPanel.Controls.AddRange(new Control[] { lblSection, cmbSection, btnShow });

            // Таблица аналитики
            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 120),
                Size = new Size(this.Width - 60, this.Height - 180),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                BackgroundColor = Color.White
            };

            btnShow.Click += (s, e) =>
            {
                if (cmbSection.SelectedValue != null)
                {
                    int sectionID = cmbSection.SelectedValue is DataRowView rowView
                        ? Convert.ToInt32(rowView["SectionID"])
                        : Convert.ToInt32(cmbSection.SelectedValue);

                    dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
                        SELECT 
                            s.LastName + ' ' + s.FirstName AS [Студент],
                            s.GroupName AS [Группа],
                            COUNT(a.AttendanceID) AS [Всего посещений],
                            SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                            SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                            CAST(SUM(CASE WHEN a.Status = 1 THEN 1.0 ELSE 0.0 END) / COUNT(*) * 100 AS DECIMAL(5,1)) AS [Процент %]
                        FROM Students s
                        JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                        JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                        WHERE sc.SectionID = @SectionID
                        GROUP BY s.LastName, s.FirstName, s.GroupName
                        ORDER BY [Процент %] DESC",
                        new[] { new SqlParameter("@SectionID", sectionID) });
                }
            };

            contentPanel.Controls.AddRange(new Control[] { lblTitle, filterPanel, dgv });
        }

        // ============================================================ ПОСЕЩАЕМОСТЬ
        private void ShowGrid(string type)
        {
            currentTableType = type;
            contentPanel.Controls.Clear();

            // Поиск
            Panel searchPanel = new Panel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(10), BackColor = Color.White };
            Label lblSearch = new Label { Text = "🔍 Поиск:", Location = new Point(10, 12), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(70, 10), Size = new Size(250, 23), PlaceholderText = "Введите для поиска..." };
            txtSearch.TextChanged += (s, e) => ApplySearch();
            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch });

            // Дата (для посещаемости)
            if (type == "Посещаемость")
            {
                filterPanel = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10), BackColor = Color.FromArgb(250, 250, 250), Visible = true };
                Label lblDate = new Label { Text = "📅 Дата:", Location = new Point(10, 15), AutoSize = true };
                dtpDate = new DateTimePicker { Location = new Point(60, 12), Size = new Size(150, 23), Format = DateTimePickerFormat.Short, Value = DateTime.Now };
                Button btnLoad = new Button { Text = "Загрузить", Location = new Point(220, 10), Size = new Size(100, 28), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnLoad.Click += (s, e) => LoadGridData();
                filterPanel.Controls.AddRange(new Control[] { lblDate, dtpDate, btnLoad });
                contentPanel.Controls.Add(filterPanel);
            }

            // DataGridView
            currentDgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White
            };

            contentPanel.Controls.Add(currentDgv);
            contentPanel.Controls.Add(searchPanel);

            LoadGridData();
        }

        private void ApplySearch()
        {
            if (currentDgv == null || string.IsNullOrEmpty(txtSearch.Text)) { LoadGridData(); return; }
            string searchText = txtSearch.Text.ToLower();
            DataTable dt = currentDgv.DataSource as DataTable;
            if (dt == null) return;
            DataTable filtered = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    if (row[col].ToString().ToLower().Contains(searchText)) { filtered.ImportRow(row); break; }
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
                if (currentTableType == "Посещаемость")
                {
                    DateTime visitDate = dtpDate?.Value ?? DateTime.Now;
                    query = $@"
                        SELECT a.AttendanceID AS [ID], s.StudentCardNumber AS [Билет],
                            s.LastName + ' ' + s.FirstName AS [Студент],
                            sec.SectionName AS [Секция], a.VisitDate AS [Дата],
                            CASE WHEN a.Status = 1 THEN 'Присутствовал' ELSE 'Отсутствовал' END AS [Статус],
                            ISNULL(a.Notes, '') AS [Примечание]
                        FROM Attendance a
                        JOIN Students s ON a.StudentCardNumber = s.StudentCardNumber
                        JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                        JOIN Sections sec ON sc.SectionID = sec.SectionID
                        WHERE CAST(a.VisitDate AS DATE) = '{visitDate:yyyy-MM-dd}'
                        ORDER BY s.LastName";
                }
                currentDgv.DataSource = DBConnection.Instance.ExecuteQuery(query);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ============================================================ ФИНАНСЫ
        private void ShowFinance()
        {
            contentPanel.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "💰 Финансовая аналитика",
                Font = new Font("Microsoft Sans Serif", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(this.Width - 60, this.Height - 130),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                BackgroundColor = Color.White
            };

            dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
                SELECT 
                    sec.SectionName AS [Секция],
                    sp.SportName AS [Вид спорта],
                    t.LastName + ' ' + t.FirstName AS [Тренер],
                    sec.PricePerMonth AS [Цена в месяц],
                    sec.MaxStudents AS [Макс. студентов],
                    (SELECT COUNT(*) FROM StudentSections ss WHERE ss.SectionID = sec.SectionID AND ss.IsActive = 1) AS [Записано],
                    sec.PricePerMonth * (SELECT COUNT(*) FROM StudentSections ss WHERE ss.SectionID = sec.SectionID AND ss.IsActive = 1) AS [Доход в месяц]
                FROM Sections sec
                JOIN Sports sp ON sec.SportID = sp.SportID
                JOIN Trainers t ON sec.TrainerID = t.TrainerID
                ORDER BY [Доход в месяц] DESC");

            contentPanel.Controls.AddRange(new Control[] { lblTitle, dgv });
        }

        // ============================================================ ОТЧЁТЫ
        private void ShowReports()
        {
            contentPanel.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "📑 Отчёты",
                Font = new Font("Microsoft Sans Serif", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            FlowLayoutPanel pnl = new FlowLayoutPanel
            {
                Location = new Point(20, 70),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10)
            };

            AddReportButton(pnl, "📄 Для ректората", "Rector");
            AddReportButton(pnl, "📄 Для кафедры", "Department");
            AddReportButton(pnl, "🖨️ Печать", "Print");

            contentPanel.Controls.AddRange(new Control[] { lblTitle, pnl });
        }

        private void AddReportButton(FlowLayoutPanel pnl, string text, string type)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(200, 60),
                Margin = new Padding(10),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            btn.Click += (s, e) =>
            {
                if (type == "Rector") MessageBox.Show("✅ Отчёт для ректората формируется...\n(Здесь будет экспорт в Excel/PDF)", "Информация");
                else if (type == "Department") MessageBox.Show("✅ Отчёт для кафедры формируется...\n(Здесь будет экспорт в Excel/PDF)", "Информация");
                else if (type == "Print") MessageBox.Show("🖨️ Печать отчёта...\n(Здесь будет предпросмотр и печать)", "Информация");
            };
            pnl.Controls.Add(btn);
        }

        // ============================================================ ЗАЯВКИ (ЗАГЛУШКА)
        private void ShowRequests()
        {
            contentPanel.Controls.Clear();

            Panel infoPanel = new Panel
            {
                Location = new Point(50, 50),
                Size = new Size(400, 200),
                BackColor = Color.FromArgb(255, 243, 205),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "📝 Заявки студентов на запись",
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblInfo = new Label
            {
                Text = "Функционал в разработке.\n\nСтуденты смогут подавать заявки на запись в секции через личный кабинет.\n\nРуководитель сможет:\n• Просматривать заявки\n• Одобрять/отклонять\n• Автоматически записывать",
                Location = new Point(20, 60),
                Size = new Size(360, 120),
                Font = new Font("Microsoft Sans Serif", 9)
            };

            infoPanel.Controls.AddRange(new Control[] { lblTitle, lblInfo });
            contentPanel.Controls.Add(infoPanel);
        }

        // ============================================================ ДОСТУП
        private void ShowAccess()
        {
            contentPanel.Controls.Clear();

            Panel infoPanel = new Panel
            {
                Location = new Point(50, 50),
                Size = new Size(400, 150),
                BackColor = Color.FromArgb(255, 243, 205),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "⚙️ Управление доступом",
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblInfo = new Label
            {
                Text = "Функционал в разработке.\n\nЗдесь будет управление правами доступа для пользователей системы.",
                Location = new Point(20, 60),
                Size = new Size(360, 80),
                Font = new Font("Microsoft Sans Serif", 9)
            };

            infoPanel.Controls.AddRange(new Control[] { lblTitle, lblInfo });
            contentPanel.Controls.Add(infoPanel);
        }

        // ============================================================ РЕСАЙЗ 
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (headerPanel != null) { headerPanel.Size = new Size(this.Width, 60); }
            if (menuPanel != null) { menuPanel.Size = new Size(this.Width, 50); }
            if (contentPanel != null) { contentPanel.Size = new Size(this.Width, this.Height - 116); }
        }
    }
}