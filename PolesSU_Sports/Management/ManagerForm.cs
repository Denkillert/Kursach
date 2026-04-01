using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;
using PolesSU_Sports.Shared.Model;
using PolesSU_Sports.Management.SettingsForms;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PolesSU_Sports.Management
{
    public partial class ManagerForm : Form
    {
        private Panel sidebarPanel;
        private Panel headerPanel;
        private Panel contentPanel;
        private Label headerLabel;
        private Button currentActiveButton;

        public ManagerForm()
        {
            InitializeComponent();
            SetupModernUI();
            LoadDashboard();
        }

        private void SetupModernUI()
        {
            this.Text = "PolesSU Sports: Панель Менеджера";
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
            CreateMenuButton("📊 Дашборд", LoadDashboard);
            CreateMenuButton("📈 Аналитика", LoadAnalytics);
            CreateMenuButton("📑 Отчёты", LoadReports);
            CreateMenuButton("🎓 По факультетам", LoadFacultyReports);
            CreateMenuButton("👥 По группам", LoadGroupReports);
            CreateMenuButton("⚽ По секциям", LoadSectionReports);
            CreateMenuButton("🎓 Заявки студентов", LoadStudentRequests);
            CreateMenuButton("⚙️ Настройки", LoadSettings);

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
                    this.Close();
                    new LoginForm().Show();
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

        // ==================== ДАШБОРД ====================
        private void LoadDashboard()
        {
            headerLabel.Text = "📊 Обзор показателей";
            contentPanel.Controls.Clear();

            var statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10),
                Height = 180,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            AddStatCard(statsPanel, "🎓 Всего студентов", Count("SELECT COUNT(*) FROM Students"), Color.FromArgb(0, 122, 204));
            AddStatCard(statsPanel, "👨‍🏫 Тренеров", Count("SELECT COUNT(*) FROM Trainers"), Color.FromArgb(40, 167, 69));
            AddStatCard(statsPanel, "⚽ Секций", Count("SELECT COUNT(*) FROM Sections"), Color.FromArgb(255, 193, 7));
            AddStatCard(statsPanel, "📋 Посещений (мес)", Count("SELECT COUNT(*) FROM Attendance WHERE MONTH(VisitDate) = MONTH(GETDATE())"), Color.FromArgb(220, 53, 69));

            contentPanel.Controls.Add(statsPanel);
        }

        private void AddStatCard(FlowLayoutPanel p, string title, string val, Color c)
        {
            var card = new Panel { Size = new Size(220, 140), Margin = new Padding(15), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var t = new Label { Text = title, Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
            var v = new Label { Text = val, Location = new Point(15, 50), Font = new Font("Segoe UI", 32, FontStyle.Bold), ForeColor = c, AutoSize = true };
            card.Controls.AddRange(new Control[] { t, v });
            p.Controls.Add(card);
        }

        private string Count(string q) { try { return DBConnection.Instance.ExecuteScalar(q)?.ToString() ?? "0"; } catch { return "0"; } }

        // ==================== АНАЛИТИКА ====================
        private void LoadAnalytics()
        {
            headerLabel.Text = "📈 Общая аналитика";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Фильтры
            var filterPanel = CreateFilterPanel();
            mainPanel.Controls.Add(filterPanel);

            // Таблица аналитики
            var dgv = new DataGridView
            {
                Location = new Point(10, 130),
                Size = new Size(mainPanel.Width - 30, mainPanel.Height - 150),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                BackgroundColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            var btnShow = new Button
            {
                Text = "📊 Показать аналитику",
                Location = new Point(10, 90),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnShow.Click += (s, e) => ShowAnalyticsData(dgv, filterPanel);

            mainPanel.Controls.Add(btnShow);
            mainPanel.Controls.Add(dgv);
            contentPanel.Controls.Add(mainPanel);
        }

        private Panel CreateFilterPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(10) };

            var lblType = new Label { Text = "Тип отчёта:", Location = new Point(10, 15), AutoSize = true };
            var cmbType = new ComboBox { Name = "cmbType", Location = new Point(90, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new object[] { "Посещаемость", "Финансы", "Успеваемость", "Активность" });
            cmbType.SelectedIndex = 0;

            var lblSec = new Label { Text = "Секция:", Location = new Point(310, 15), AutoSize = true };
            var cmbSec = new ComboBox { Name = "cmbSec", Location = new Point(370, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadSectionsToComboBox(cmbSec);

            var lblFaculty = new Label { Text = "Факультет:", Location = new Point(590, 15), AutoSize = true };
            var cmbFaculty = new ComboBox { Name = "cmbFaculty", Location = new Point(670, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadFacultiesToComboBox(cmbFaculty);

            pnl.Controls.AddRange(new Control[] { lblType, cmbType, lblSec, cmbSec, lblFaculty, cmbFaculty });
            return pnl;
        }

        private void LoadSectionsToComboBox(ComboBox cmb)
        {
            var dt = DBConnection.Instance.ExecuteQuery("SELECT SectionID, SectionName FROM Sections ORDER BY SectionName");
            cmb.DataSource = dt;
            cmb.DisplayMember = "SectionName";
            cmb.ValueMember = "SectionID";
        }

        private void LoadFacultiesToComboBox(ComboBox cmb)
        {
            var dt = DBConnection.Instance.ExecuteQuery("SELECT FacultyID, FacultyName FROM Faculties ORDER BY FacultyName");
            cmb.DataSource = dt;
            cmb.DisplayMember = "FacultyName";
            cmb.ValueMember = "FacultyID";
        }

        private void ShowAnalyticsData(DataGridView dgv, Panel filterPanel)
        {
            var cmbType = filterPanel.Controls.Find("cmbType", false).FirstOrDefault() as ComboBox;
            var cmbSec = filterPanel.Controls.Find("cmbSec", false).FirstOrDefault() as ComboBox;
            var cmbFaculty = filterPanel.Controls.Find("cmbFaculty", false).FirstOrDefault() as ComboBox;

            string query = "";
            if (cmbType.SelectedItem.ToString() == "Посещаемость")
            {
                query = @"
                    SELECT 
                        s.LastName + ' ' + s.FirstName AS [Студент],
                        f.FacultyName AS [Факультет],
                        s.GroupName AS [Группа],
                        sec.SectionName AS [Секция],
                        COUNT(a.AttendanceID) AS [Всего посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                        CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / COUNT(*) AS DECIMAL(5,1)) AS [Процент %]
                    FROM Students s
                    JOIN Faculties f ON s.FacultyID = f.FacultyID
                    LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                    LEFT JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                    LEFT JOIN Sections sec ON sc.SectionID = sec.SectionID
                    WHERE (@SectionID IS NULL OR sec.SectionID = @SectionID)
                    AND (@FacultyID IS NULL OR f.FacultyID = @FacultyID)
                    GROUP BY s.LastName, s.FirstName, f.FacultyName, s.GroupName, sec.SectionName
                    ORDER BY [Процент %] DESC";
            }

            dgv.DataSource = DBConnection.Instance.ExecuteQuery(query, new[] {
                new SqlParameter("@SectionID", cmbSec.SelectedValue ?? (object)DBNull.Value),
                new SqlParameter("@FacultyID", cmbFaculty.SelectedValue ?? (object)DBNull.Value)
            });
        }

        // ==================== ОТЧЁТЫ ====================
        private void LoadReports()
        {
            headerLabel.Text = "📑 Генерация отчётов";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // === НАСТРОЙКИ ОТЧЁТА (вместо GroupBox используем Panel с рамкой) ===
            var settingsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitle = new Label
            {
                Text = "📋 Параметры отчёта",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            settingsPanel.Controls.Add(lblTitle);

            var lblReportType = new Label { Text = "Тип отчёта:", Location = new Point(15, 45), AutoSize = true };
            var cmbReportType = new ComboBox { Name = "cmbReportType", Location = new Point(100, 42), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReportType.Items.AddRange(new object[] { "Для ректората", "Для кафедры", "Финансовый отчёт", "Отчёт по посещаемости", "Сводный отчёт" });
            cmbReportType.SelectedIndex = 0;

            var lblPeriod = new Label { Text = "Период:", Location = new Point(15, 85), AutoSize = true };
            var dtpStart = new DateTimePicker { Name = "dtpStart", Location = new Point(80, 82), Width = 150, Format = DateTimePickerFormat.Short };
            var lblTo = new Label { Text = "по", Location = new Point(240, 85), AutoSize = true };
            var dtpEnd = new DateTimePicker { Name = "dtpEnd", Location = new Point(270, 82), Width = 150, Format = DateTimePickerFormat.Short };

            var lblFormat = new Label { Text = "Формат:", Location = new Point(15, 125), AutoSize = true };
            var cmbFormat = new ComboBox { Name = "cmbFormat", Location = new Point(80, 122), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFormat.Items.AddRange(new object[] { "Excel (.xlsx)", "PDF", "Word (.docx)", "CSV" });
            cmbFormat.SelectedIndex = 0;

            var chkIncludeCharts = new CheckBox { Name = "chkCharts", Text = "Включить графики", Location = new Point(250, 123), AutoSize = true, Checked = true };

            var btnGenerate = new Button
            {
                Text = "📄 Сформировать отчёт",
                Location = new Point(15, 160),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGenerate.Click += (s, e) => GenerateReport(settingsPanel);

            settingsPanel.Controls.AddRange(new Control[] { lblReportType, cmbReportType, lblPeriod, dtpStart, lblTo, dtpEnd, lblFormat, cmbFormat, chkIncludeCharts, btnGenerate });

            // === ПРЕДПРОСМОТР ===
            var previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 10, 0, 0)
            };

            var lblPreview = new Label
            {
                Text = "👁️ Предпросмотр данных",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var dgvPreview = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(previewPanel.Width - 30, previewPanel.Height - 60),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                BackgroundColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            previewPanel.Controls.Add(lblPreview);
            previewPanel.Controls.Add(dgvPreview);

            mainPanel.Controls.Add(settingsPanel);
            mainPanel.Controls.Add(previewPanel);
            contentPanel.Controls.Add(mainPanel);
        }

        private void GenerateReport(Panel settingsPanel)
        {
            try
            {
                var cmbReportType = settingsPanel.Controls.Find("cmbReportType", false).FirstOrDefault() as ComboBox;
                var dtpStart = settingsPanel.Controls.Find("dtpStart", false).FirstOrDefault() as DateTimePicker;
                var dtpEnd = settingsPanel.Controls.Find("dtpEnd", false).FirstOrDefault() as DateTimePicker;
                var cmbFormat = settingsPanel.Controls.Find("cmbFormat", false).FirstOrDefault() as ComboBox;

                // Получаем данные в зависимости от типа отчёта
                DataTable reportData = GetReportData(cmbReportType.SelectedItem.ToString(), dtpStart.Value, dtpEnd.Value);

                if (reportData.Rows.Count == 0)
                {
                    MessageBox.Show("⚠️ Нет данных для отображения", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Показываем предпросмотр
                var dgvPreview = contentPanel.Controls.Find("dataGridView", false)
                    .OfType<DataGridView>().FirstOrDefault();

                if (dgvPreview != null)
                {
                    dgvPreview.DataSource = reportData;
                }

                // Спрашиваем сохранить ли
                if (MessageBox.Show(
                    $"✅ Отчёт сформирован!\n\n" +
                    $"Тип: {cmbReportType.SelectedItem}\n" +
                    $"Период: {dtpStart.Value.ToShortDateString()} - {dtpEnd.Value.ToShortDateString()}\n" +
                    $"Записей: {reportData.Rows.Count}\n\n" +
                    $"Сохранить файл?",
                    "Отчёт готов",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    SaveReport(reportData, cmbReportType.SelectedItem.ToString(),
                              dtpStart.Value, dtpEnd.Value, cmbFormat.SelectedItem.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка генерации отчёта: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GetReportData(string reportType, DateTime startDate, DateTime endDate)
        {
            string query = "";
            SqlParameter[] parameters = {
        new SqlParameter("@StartDate", startDate.Date),
        new SqlParameter("@EndDate", endDate.Date)
    };

            switch (reportType)
            {
                case "Для ректората":
                    query = @"
                SELECT 
                    f.FacultyName AS [Факультет],
                    COUNT(DISTINCT s.StudentCardNumber) AS [Всего студентов],
                    COUNT(DISTINCT sec.SectionID) AS [Количество секций],
                    COUNT(DISTINCT a.AttendanceID) AS [Всего посещений],
                    CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                         NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Посещаемость %]
                FROM Faculties f
                LEFT JOIN Students s ON f.FacultyID = s.FacultyID
                LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                LEFT JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                LEFT JOIN Sections sec ON sc.SectionID = sec.SectionID
                WHERE a.VisitDate BETWEEN @StartDate AND @EndDate
                GROUP BY f.FacultyName
                ORDER BY [Всего студентов] DESC";
                    break;

                case "Для кафедры":
                    query = @"
                SELECT 
                    sec.SectionName AS [Секция],
                    sp.SportName AS [Вид спорта],
                    t.LastName + ' ' + t.FirstName AS [Тренер],
                    COUNT(DISTINCT ss.StudentCardNumber) AS [Записано студентов],
                    sec.PricePerMonth AS [Цена в месяц],
                    sec.PricePerMonth * COUNT(DISTINCT ss.StudentCardNumber) AS [Доход]
                FROM Sections sec
                JOIN Sports sp ON sec.SportID = sp.SportID
                JOIN Trainers t ON sec.TrainerID = t.TrainerID
                LEFT JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.IsActive = 1
                GROUP BY sec.SectionName, sp.SportName, t.LastName, t.FirstName, 
                         sec.PricePerMonth
                ORDER BY [Доход] DESC";
                    break;

                case "Финансовый отчёт":
                    query = @"
                SELECT 
                    sec.SectionName AS [Секция],
                    COUNT(DISTINCT ss.StudentCardNumber) AS [Студентов],
                    sec.PricePerMonth AS [Цена],
                    sec.PricePerMonth * COUNT(DISTINCT ss.StudentCardNumber) AS [Месячный доход],
                    sec.PricePerMonth * COUNT(DISTINCT ss.StudentCardNumber) * 6 AS [Доход за семестр]
                FROM Sections sec
                LEFT JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.IsActive = 1
                GROUP BY sec.SectionName, sec.PricePerMonth
                ORDER BY [Доход за семестр] DESC";
                    break;

                case "Отчёт по посещаемости":
                    query = @"
                SELECT 
                    s.StudentCardNumber AS [Билет],
                    s.LastName + ' ' + s.FirstName AS [ФИО],
                    f.FacultyName AS [Факультет],
                    s.GroupName AS [Группа],
                    sec.SectionName AS [Секция],
                    COUNT(a.AttendanceID) AS [Всего занятий],
                    SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                    SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                    CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                         COUNT(*) AS DECIMAL(5,1)) AS [Процент %]
                FROM Students s
                JOIN Faculties f ON s.FacultyID = f.FacultyID
                LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                LEFT JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                LEFT JOIN Sections sec ON sc.SectionID = sec.SectionID
                WHERE a.VisitDate BETWEEN @StartDate AND @EndDate
                GROUP BY s.StudentCardNumber, s.LastName, s.FirstName, 
                         f.FacultyName, s.GroupName, sec.SectionName
                ORDER BY [Процент %] DESC";
                    break;

                default:
                    query = @"
                SELECT 
                    s.StudentCardNumber AS [Билет],
                    s.LastName + ' ' + s.FirstName AS [ФИО],
                    f.FacultyName AS [Факультет],
                    s.GroupName AS [Группа],
                    COUNT(a.AttendanceID) AS [Посещений]
                FROM Students s
                JOIN Faculties f ON s.FacultyID = f.FacultyID
                LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                WHERE a.VisitDate BETWEEN @StartDate AND @EndDate
                GROUP BY s.StudentCardNumber, s.LastName, s.FirstName, 
                         f.FacultyName, s.GroupName";
                    break;
            }

            return DBConnection.Instance.ExecuteQuery(query, parameters);
        }

        private void SaveReport(DataTable data, string reportType, DateTime startDate,
                               DateTime endDate, string format)
        {
            try
            {
                // Создаём имя файла
                string fileName = $"Отчёт_{reportType.Replace(" ", "_")}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                string folder = Path.Combine(Application.StartupPath, "Отчёты");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fullPath = "";

                if (format.Contains("CSV") || format.Contains("Excel"))
                {
                    // Генерация CSV (открывается в Excel)
                    fullPath = Path.Combine(folder, fileName + ".csv");
                    ExportToCSV(data, fullPath);
                }
                else if (format.Contains("PDF"))
                {
                    MessageBox.Show("📄 PDF экспорт будет добавлен в следующем обновлении\n(Требуется библиотека iTextSharp или similar)",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (format.Contains("Word"))
                {
                    MessageBox.Show("📄 Word экспорт будет добавлен в следующем обновлении",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                MessageBox.Show($"✅ Отчёт сохранён:\n{fullPath}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Предлагаем открыть файл
                if (MessageBox.Show("Открыть файл?", "Отчёт готов",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(fullPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка сохранения: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(DataTable dt, string filePath)
        {
            StringBuilder csvContent = new StringBuilder();

            // Заголовки столбцов
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                csvContent.Append(dt.Columns[i].ColumnName);
                if (i < dt.Columns.Count - 1)
                    csvContent.Append(";");
            }
            csvContent.AppendLine();

            // Данные
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string value = row[i].ToString().Replace(";", ","); // Заменяем точки с запятой
                    csvContent.Append(value);
                    if (i < dt.Columns.Count - 1)
                        csvContent.Append(";");
                }
                csvContent.AppendLine();
            }

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.GetEncoding("windows-1251"));
        }

        // ==================== ПО ФАКУЛЬТЕТАМ ====================
        private void LoadFacultyReports()
        {
            headerLabel.Text = "🎓 Отчёты по факультетам";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            // === ФИЛЬТРЫ (вверху) ===
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblFaculty = new Label { Text = "Факультет:", Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var cmbFaculty = new ComboBox { Name = "cmbFaculty", Location = new Point(90, 22), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadFacultiesToComboBox(cmbFaculty);

            var btnShow = new Button
            {
                Name = "btnShow",
                Text = "📊 Показать",
                Location = new Point(410, 20),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnShow.Click += (s, e) => ShowFacultyReport(cmbFaculty);

            filterPanel.Controls.AddRange(new Control[] { lblFaculty, cmbFaculty, btnShow });

            // === ТАБЛИЦА (ниже фильтров) ===
            var tablePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 15, 0, 0)
            };

            var dgv = new DataGridView
            {
                Name = "dataGridView",
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
                RowHeadersVisible = false
            };

            tablePanel.Controls.Add(dgv);

            // ✅ ВАЖНО: сначала таблица, потом фильтры (чтобы фильтры были сверху)
            mainPanel.Controls.Add(tablePanel);
            mainPanel.Controls.Add(filterPanel);
            contentPanel.Controls.Add(mainPanel);

            // Автозагрузка при открытии
            if (cmbFaculty.SelectedValue != null)
            {
                ShowFacultyReport(cmbFaculty);
            }
        }

        private void ShowFacultyReport(ComboBox cmbFaculty)
        {
            if (cmbFaculty.SelectedValue == null)
            {
                MessageBox.Show("Выберите факультет", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int facultyID = cmbFaculty.SelectedValue is DataRowView rowView ? Convert.ToInt32(rowView["FacultyID"]) : Convert.ToInt32(cmbFaculty.SelectedValue);

                // ✅ Ищем DataGridView по имени
                var dgv = contentPanel.Controls.Find("dataGridView", true).FirstOrDefault() as DataGridView;
                if (dgv == null) return;

                dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
            SELECT 
                s.GroupName AS [Группа],
                COUNT(DISTINCT s.StudentCardNumber) AS [Студентов],
                COUNT(DISTINCT a.AttendanceID) AS [Посещений],
                SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                     NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Посещаемость %]
            FROM Students s
            LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
            WHERE s.FacultyID = @FacultyID
            GROUP BY s.GroupName
            ORDER BY [Посещаемость %] DESC",
                    new[] { new SqlParameter("@FacultyID", facultyID) });

                if (dgv.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных по выбранному факультету", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== ПО ГРУППАМ ====================
        private void LoadGroupReports()
        {
            headerLabel.Text = "👥 Отчёты по группам";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            // === ФИЛЬТРЫ ===
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblGroup = new Label { Text = "Группа:", Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var cmbGroup = new ComboBox { Name = "cmbGroup", Location = new Point(80, 22), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };

            var groups = DBConnection.Instance.ExecuteQuery("SELECT DISTINCT GroupName FROM Students ORDER BY GroupName");
            cmbGroup.DataSource = groups;
            cmbGroup.DisplayMember = "GroupName";
            cmbGroup.ValueMember = "GroupName";

            var btnShow = new Button
            {
                Name = "btnShow",
                Text = "📊 Показать",
                Location = new Point(400, 20),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnShow.Click += (s, e) => ShowGroupReport(cmbGroup);

            filterPanel.Controls.AddRange(new Control[] { lblGroup, cmbGroup, btnShow });

            // === ТАБЛИЦА ===
            var tablePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 15, 0, 0)
            };

            var dgv = new DataGridView
            {
                Name = "dataGridView",
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
                RowHeadersVisible = false
            };

            tablePanel.Controls.Add(dgv);

            mainPanel.Controls.Add(tablePanel);
            mainPanel.Controls.Add(filterPanel);
            contentPanel.Controls.Add(mainPanel);

            // Автозагрузка
            if (cmbGroup.SelectedValue != null)
            {
                ShowGroupReport(cmbGroup);
            }
        }

        private void ShowGroupReport(ComboBox cmbGroup)
        {
            string groupName = cmbGroup.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Выберите группу", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var dgv = contentPanel.Controls.Find("dataGridView", true).FirstOrDefault() as DataGridView;
                if (dgv == null) return;

                dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
            SELECT 
                s.StudentCardNumber AS [Билет],
                s.LastName + ' ' + s.FirstName AS [ФИО],
                f.FacultyName AS [Факультет],
                COUNT(a.AttendanceID) AS [Посещений],
                SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                     NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Успеваемость %]
            FROM Students s
            JOIN Faculties f ON s.FacultyID = f.FacultyID
            LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
            WHERE s.GroupName = @GroupName
            GROUP BY s.StudentCardNumber, s.LastName, s.FirstName, f.FacultyName
            ORDER BY [Успеваемость %] DESC",
                    new[] { new SqlParameter("@GroupName", groupName) });

                if (dgv.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных по выбранной группе", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== ПО СЕКЦИЯМ ====================
        private void LoadSectionReports()
        {
            headerLabel.Text = "⚽ Отчёты по секциям";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            // === ФИЛЬТРЫ (вверху) ===
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblSection = new Label { Text = "Секция:", Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var cmbSection = new ComboBox { Name = "cmbSection", Location = new Point(80, 17), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadSectionsToComboBox(cmbSection);

            var lblPeriod = new Label { Text = "Период:", Location = new Point(350, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var dtpStart = new DateTimePicker { Name = "dtpStart", Location = new Point(410, 17), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };
            var lblTo = new Label { Text = "по", Location = new Point(550, 20), AutoSize = true };
            var dtpEnd = new DateTimePicker { Name = "dtpEnd", Location = new Point(575, 17), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now };

            var btnShow = new Button
            {
                Name = "btnShow",
                Text = "📊 Показать",
                Location = new Point(720, 15),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnShow.Click += (s, e) => ShowSectionReport(cmbSection, dtpStart, dtpEnd);

            filterPanel.Controls.AddRange(new Control[] { lblSection, cmbSection, lblPeriod, dtpStart, lblTo, dtpEnd, btnShow });

            // === ТАБЛИЦА ДАННЫХ (ниже фильтров) ===
            var tablePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 15, 0, 0)
            };

            var dgv = new DataGridView
            {
                Name = "dataGridView",
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
                RowHeadersVisible = false
            };

            tablePanel.Controls.Add(dgv);

            // Добавляем в правильном порядке: сначала таблица, потом фильтры (чтобы фильтры были сверху)
            mainPanel.Controls.Add(tablePanel);
            mainPanel.Controls.Add(filterPanel);
            contentPanel.Controls.Add(mainPanel);

            // Автоматически загружаем данные
            if (cmbSection.SelectedValue != null)
            {
                ShowSectionReport(cmbSection, dtpStart, dtpEnd);
            }
        }

        private void ShowSectionReport(ComboBox cmbSection, DateTimePicker dtpStart, DateTimePicker dtpEnd)
        {
            if (cmbSection.SelectedValue == null)
            {
                MessageBox.Show("Выберите секцию", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int sectionID = cmbSection.SelectedValue is DataRowView rowView ? Convert.ToInt32(rowView["SectionID"]) : Convert.ToInt32(cmbSection.SelectedValue);

                var dgv = contentPanel.Controls.Find("dataGridView", true).FirstOrDefault() as DataGridView;
                if (dgv == null) return;

                dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
            SELECT 
                s.StudentCardNumber AS [Билет],
                s.LastName + ' ' + s.FirstName AS [ФИО],
                f.FacultyName AS [Факультет],
                s.GroupName AS [Группа],
                COUNT(a.AttendanceID) AS [Всего занятий],
                SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                     NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Посещаемость %]
            FROM Students s
            JOIN Faculties f ON s.FacultyID = f.FacultyID
            LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
            LEFT JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
            WHERE sc.SectionID = @SectionID
            AND (a.VisitDate IS NULL OR a.VisitDate BETWEEN @StartDate AND @EndDate)
            GROUP BY s.StudentCardNumber, s.LastName, s.FirstName, f.FacultyName, s.GroupName
            ORDER BY [Посещаемость %] DESC",
                    new[] {
                new SqlParameter("@SectionID", sectionID),
                new SqlParameter("@StartDate", dtpStart.Value.Date),
                new SqlParameter("@EndDate", dtpEnd.Value.Date)
                    });

                if (dgv.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных за выбранный период", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== ЗАЯВКИ ====================
        private void LoadStudentRequests()
        {
            headerLabel.Text = "🎓 Заявки студентов на запись в секции";
            contentPanel.Controls.Clear();

            var infoPanel = new Panel
            {
                Location = new Point(50, 50),
                Size = new Size(500, 250),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitle = new Label
            {
                Text = "📝 Заявки студентов",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(45, 55, 75)
            };

            var lblInfo = new Label
            {
                Text = "Функционал в разработке.\n\n" +
                       "В будущем здесь будет:\n" +
                       "• Просмотр заявок от студентов\n" +
                       "• Одобрение/отклонение заявок\n" +
                       "• Автоматическая запись в секции\n" +
                       "• Уведомления о новых заявках",
                Location = new Point(20, 70),
                Size = new Size(460, 160),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray
            };

            infoPanel.Controls.AddRange(new Control[] { lblTitle, lblInfo });
            contentPanel.Controls.Add(infoPanel);
        }

        // ==================== НАСТРОЙКИ ====================
        private void LoadSettings()
        {
            headerLabel.Text = "⚙️ Настройки системы";
            contentPanel.Controls.Clear();

            var settingsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20)
            };

            AddSettingCard(settingsPanel, "👥 Управление пользователями", "Добавление, редактирование и удаление пользователей",
                Color.FromArgb(0, 122, 204), () => new PolesSU_Sports.Management.SettingsForms.UserManagementForm().ShowDialog());

            AddSettingCard(settingsPanel, "🔐 Права доступа", "Настройка ролей и разрешений",
                Color.FromArgb(40, 167, 69), () => new PolesSU_Sports.Management.SettingsForms.AccessRightsForm().ShowDialog());

            AddSettingCard(settingsPanel, "📊 Настройка отчётов", "Шаблоны и параметры генерации отчётов",
                Color.FromArgb(255, 193, 7), () => new PolesSU_Sports.Management.SettingsForms.ReportSettingsForm().ShowDialog());

            AddSettingCard(settingsPanel, "💾 Резервное копирование", "Создание и восстановление резервных копий БД",
                Color.FromArgb(220, 53, 69), () => new PolesSU_Sports.Management.SettingsForms.BackupForm().ShowDialog());

            contentPanel.Controls.Add(settingsPanel);
        }

        private void AddSettingCard(FlowLayoutPanel parent, string title, string description, Color color, Action click)
        {
            var card = new Panel { Size = new Size(600, 100), Margin = new Padding(10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var t = new Label { Text = title, Location = new Point(20, 15), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = color, AutoSize = true };
            var d = new Label { Text = description, Location = new Point(20, 45), Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true };
            var btn = new Button { Text = "Открыть", Location = new Point(500, 35), Size = new Size(80, 30), FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White };
            btn.Click += (s, e) => click();
            card.Controls.AddRange(new Control[] { t, d, btn });
            parent.Controls.Add(card);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(1280, 720);
            this.Name = "ManagerForm";
            this.ResumeLayout(false);
        }
    }
}