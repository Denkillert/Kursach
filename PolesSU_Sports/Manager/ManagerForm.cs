using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;
using PolesSU_Sports.Shared.Model;

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
            var cmbReportType = settingsPanel.Controls.Find("cmbReportType", false).FirstOrDefault() as ComboBox;
            var dtpStart = settingsPanel.Controls.Find("dtpStart", false).FirstOrDefault() as DateTimePicker;
            var dtpEnd = settingsPanel.Controls.Find("dtpEnd", false).FirstOrDefault() as DateTimePicker;
            var cmbFormat = settingsPanel.Controls.Find("cmbFormat", false).FirstOrDefault() as ComboBox;
            var chkCharts = settingsPanel.Controls.Find("chkCharts", false).FirstOrDefault() as CheckBox;

            MessageBox.Show(
                $"✅ Отчёт сформирован!\n\n" +
                $"Тип: {cmbReportType.SelectedItem}\n" +
                $"Период: {dtpStart.Value.ToShortDateString()} - {dtpEnd.Value.ToShortDateString()}\n" +
                $"Формат: {cmbFormat.SelectedItem}\n" +
                $"Графики: {(chkCharts.Checked ? "включены" : "выключены")}\n\n" +
                $"Файл сохранён в папку \"Отчёты\"",
                "Отчёт готов",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ==================== ПО ФАКУЛЬТЕТАМ ====================
        private void LoadFacultyReports()
        {
            headerLabel.Text = "🎓 Отчёты по факультетам";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblFaculty = new Label { Text = "Факультет:", Location = new Point(15, 30), AutoSize = true };
            var cmbFaculty = new ComboBox { Name = "cmbFaculty", Location = new Point(90, 27), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadFacultiesToComboBox(cmbFaculty);

            var btnShow = new Button
            {
                Text = "Показать",
                Location = new Point(410, 27),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnShow.Click += (s, e) => ShowFacultyReport(cmbFaculty);

            filterPanel.Controls.AddRange(new Control[] { lblFaculty, cmbFaculty, btnShow });

            var dgvPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            var dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, BackgroundColor = Color.White };

            dgvPanel.Controls.Add(dgv);
            mainPanel.Controls.Add(filterPanel);
            mainPanel.Controls.Add(dgvPanel);
            contentPanel.Controls.Add(mainPanel);
        }

        private void ShowFacultyReport(ComboBox cmbFaculty)
        {
            if (cmbFaculty.SelectedValue == null) return;

            int facultyID = cmbFaculty.SelectedValue is DataRowView rowView ? Convert.ToInt32(rowView["FacultyID"]) : Convert.ToInt32(cmbFaculty.SelectedValue);

            var dgv = contentPanel.Controls.Find("dataGridView", false).FirstOrDefault() as DataGridView;
            if (dgv == null)
            {
                foreach (Control c in contentPanel.Controls)
                {
                    if (c is Panel p)
                    {
                        foreach (Control c2 in p.Controls)
                        {
                            if (c2 is DataGridView dgv2) { dgv = dgv2; break; }
                        }
                    }
                }
            }

            if (dgv != null)
            {
                dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        s.GroupName AS [Группа],
                        COUNT(DISTINCT s.StudentCardNumber) AS [Студентов],
                        COUNT(DISTINCT a.AttendanceID) AS [Посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / COUNT(*) AS DECIMAL(5,1)) AS [Успеваемость %]
                    FROM Students s
                    LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                    WHERE s.FacultyID = @FacultyID
                    GROUP BY s.GroupName
                    ORDER BY [Успеваемость %] DESC",
                    new[] { new SqlParameter("@FacultyID", facultyID) });
            }
        }

        // ==================== ПО ГРУППАМ ====================
        private void LoadGroupReports()
        {
            headerLabel.Text = "👥 Отчёты по группам";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var cmbGroup = new ComboBox { Name = "cmbGroup", Location = new Point(15, 30), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            var groups = DBConnection.Instance.ExecuteQuery("SELECT DISTINCT GroupName FROM Students ORDER BY GroupName");
            cmbGroup.DataSource = groups;
            cmbGroup.DisplayMember = "GroupName";

            var btnShow = new Button
            {
                Text = "Показать отчёт",
                Location = new Point(330, 30),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnShow.Click += (s, e) => ShowGroupReport(cmbGroup);

            filterPanel.Controls.AddRange(new Control[] { cmbGroup, btnShow });

            var dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, BackgroundColor = Color.White };
            mainPanel.Controls.Add(filterPanel);
            mainPanel.Controls.Add(dgv);
            contentPanel.Controls.Add(mainPanel);
        }

        private void ShowGroupReport(ComboBox cmbGroup)
        {
            string groupName = cmbGroup.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(groupName)) return;

            var dgv = contentPanel.Controls.OfType<DataGridView>().FirstOrDefault();
            if (dgv != null)
            {
                dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        s.StudentCardNumber AS [Билет],
                        s.LastName + ' ' + s.FirstName AS [ФИО],
                        f.FacultyName AS [Факультет],
                        COUNT(a.AttendanceID) AS [Посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / COUNT(*) AS DECIMAL(5,1)) AS [%]
                    FROM Students s
                    JOIN Faculties f ON s.FacultyID = f.FacultyID
                    LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                    WHERE s.GroupName = @GroupName
                    GROUP BY s.StudentCardNumber, s.LastName, s.FirstName, f.FacultyName
                    ORDER BY [%] DESC",
                    new[] { new SqlParameter("@GroupName", groupName) });
            }
        }

        // ==================== ПО СЕКЦИЯМ ====================
        private void LoadSectionReports()
        {
            headerLabel.Text = "⚽ Отчёты по секциям";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblSection = new Label { Text = "Секция:", Location = new Point(15, 30), AutoSize = true };
            var cmbSection = new ComboBox { Name = "cmbSection", Location = new Point(70, 27), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadSectionsToComboBox(cmbSection);

            var lblPeriod = new Label { Text = "Период:", Location = new Point(340, 30), AutoSize = true };
            var dtpStart = new DateTimePicker { Name = "dtpStart", Location = new Point(400, 27), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };
            var dtpEnd = new DateTimePicker { Name = "dtpEnd", Location = new Point(540, 27), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now };

            var btnShow = new Button
            {
                Text = "Показать",
                Location = new Point(690, 27),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnShow.Click += (s, e) => ShowSectionReport(cmbSection, dtpStart, dtpEnd);

            filterPanel.Controls.AddRange(new Control[] { lblSection, cmbSection, lblPeriod, dtpStart, dtpEnd, btnShow });

            var dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, BackgroundColor = Color.White };
            mainPanel.Controls.Add(filterPanel);
            mainPanel.Controls.Add(dgv);
            contentPanel.Controls.Add(mainPanel);
        }

        private void ShowSectionReport(ComboBox cmbSection, DateTimePicker dtpStart, DateTimePicker dtpEnd)
        {
            if (cmbSection.SelectedValue == null) return;

            int sectionID = cmbSection.SelectedValue is DataRowView rowView ? Convert.ToInt32(rowView["SectionID"]) : Convert.ToInt32(cmbSection.SelectedValue);

            var dgv = contentPanel.Controls.OfType<DataGridView>().FirstOrDefault();
            if (dgv != null)
            {
                dgv.DataSource = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        s.StudentCardNumber AS [Билет],
                        s.LastName + ' ' + s.FirstName AS [ФИО],
                        s.GroupName AS [Группа],
                        COUNT(a.AttendanceID) AS [Посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                        CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / COUNT(*) AS DECIMAL(5,1)) AS [Успеваемость %]
                    FROM Students s
                    JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                    JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                    WHERE sc.SectionID = @SectionID
                    AND a.VisitDate BETWEEN @StartDate AND @EndDate
                    GROUP BY s.StudentCardNumber, s.LastName, s.FirstName, s.GroupName
                    ORDER BY [Успеваемость %] DESC",
                    new[] {
                        new SqlParameter("@SectionID", sectionID),
                        new SqlParameter("@StartDate", dtpStart.Value.Date),
                        new SqlParameter("@EndDate", dtpEnd.Value.Date)
                    });
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

            AddSettingCard(settingsPanel, "👥 Управление пользователями", "Добавление, редактирование и удаление пользователей", Color.FromArgb(0, 122, 204));
            AddSettingCard(settingsPanel, "🔐 Права доступа", "Настройка ролей и разрешений", Color.FromArgb(40, 167, 69));
            AddSettingCard(settingsPanel, "📊 Настройка отчётов", "Шаблоны и параметры генерации отчётов", Color.FromArgb(255, 193, 7));
            AddSettingCard(settingsPanel, "💾 Резервное копирование", "Создание и восстановление резервных копий БД", Color.FromArgb(220, 53, 69));

            contentPanel.Controls.Add(settingsPanel);
        }

        private void AddSettingCard(FlowLayoutPanel parent, string title, string description, Color color)
        {
            var card = new Panel { Size = new Size(600, 100), Margin = new Padding(10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var t = new Label { Text = title, Location = new Point(20, 15), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = color, AutoSize = true };
            var d = new Label { Text = description, Location = new Point(20, 45), Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true };
            var btn = new Button { Text = "Открыть", Location = new Point(500, 35), Size = new Size(80, 30), FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White };
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