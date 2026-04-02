using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;
using PolesSU_Sports.Shared.Model;
using System.Windows.Forms.DataVisualization.Charting;
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

        // ==================== АНАЛИТИКА С ГРАФИКАМИ ====================
        private void LoadAnalytics()
        {
            headerLabel.Text = "📈 Аналитика и статистика";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // === 1. ВЕРХНЯЯ ПАНЕЛЬ: ФИЛЬТРЫ ===
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblType = new Label { Text = "Тип аналитики:", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var cmbType = new ComboBox { Name = "cmbType", Location = new Point(120, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new object[] {
        "📊 Посещаемость по секциям",
        "👥 Активность студентов",
        "💰 Финансовая аналитика",
        "📅 Динамика посещаемости"
    });
            cmbType.SelectedIndex = 0;

            var lblSec = new Label { Text = "Секция:", Location = new Point(340, 15), AutoSize = true };
            var cmbSec = new ComboBox { Name = "cmbSec", Location = new Point(400, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadSectionsToComboBox(cmbSec);

            var lblFaculty = new Label { Text = "Факультет:", Location = new Point(620, 15), AutoSize = true };
            var cmbFaculty = new ComboBox { Name = "cmbFaculty", Location = new Point(690, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadFacultiesToComboBox(cmbFaculty);

            var btnShow = new Button
            {
                Text = "📊 Показать аналитику",
                Location = new Point(15, 50),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnShow.Click += (s, e) => ShowAnalyticsWithCharts(cmbType, cmbSec, cmbFaculty);

            filterPanel.Controls.AddRange(new Control[] { lblType, cmbType, lblSec, cmbSec, lblFaculty, cmbFaculty, btnShow });

            // === 2. ГРАФИКИ (верхняя часть) ===
            var chartsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 300,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 10),
                BackColor = Color.FromArgb(250, 250, 250)
            };
            chartsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // График 1
            var chart1Panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            var lblChart1 = new Label { Text = "📊 Распределение по секциям", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(30, 10), AutoSize = true };
            var chart1 = new Chart { Name = "chart1", Dock = DockStyle.Fill, Location = new Point(0, 30) };
            chart1Panel.Controls.AddRange(new Control[] { lblChart1, chart1 });

            // График 2
            var chart2Panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            var lblChart2 = new Label { Text = "📈 Динамика", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(30, 10), AutoSize = true };
            var chart2 = new Chart { Name = "chart2", Dock = DockStyle.Fill, Location = new Point(0, 30) };
            chart2Panel.Controls.AddRange(new Control[] { lblChart2, chart2 });

            chartsPanel.Controls.Add(chart1Panel, 0, 0);
            chartsPanel.Controls.Add(chart2Panel, 1, 0);

            // === 3. ТАБЛИЦА ДАННЫХ (нижняя часть) ===
            var tablePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
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
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 230, 230);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(45, 55, 75);
            dgv.EnableHeadersVisualStyles = false;

            tablePanel.Controls.Add(dgv);

            // === ДОБАВЛЯЕМ ВСЁ ===
            mainPanel.Controls.Add(tablePanel);
            mainPanel.Controls.Add(chartsPanel);
            mainPanel.Controls.Add(filterPanel);
            contentPanel.Controls.Add(mainPanel);

            // Автозагрузка
            ShowAnalyticsWithCharts(cmbType, cmbSec, cmbFaculty);
        }

        // ==================== НАСТРОЙКА ГРАФИКОВ ====================
        private void SetupBarChart(Chart chart, DataTable data, string xAxis, string[] yAxes, string title)
        {
            if (chart == null || data == null || data.Rows.Count == 0) return;

            chart.Titles.Clear();
            chart.Titles.Add(title);
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add("MainArea");
            
            // Добавляем легенду
            chart.Legends.Clear();
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9),
                IsTextAutoFit = true
            };
            chart.Legends.Add(legend);

            foreach (string yAxis in yAxes)
            {
                var series = new Series(yAxis)
                {
                    ChartType = SeriesChartType.Column,
                    Color = GetColor(yAxes.ToList().IndexOf(yAxis)),
                    Legend = "MainLegend"
                };

                foreach (DataRow row in data.Rows)
                {
                    series.Points.AddXY(row[xAxis], row[yAxis]);
                }

                chart.Series.Add(series);
            }
        }

        private void SetupPieChart(Chart chart, DataTable data, string labelColumn, string valueColumn, string title)
        {
            if (chart == null || data == null || data.Rows.Count == 0) return;

            chart.Titles.Clear();
            chart.Titles.Add(title);
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add("MainArea");
            
            // Добавляем легенду
            chart.Legends.Clear();
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Right,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9),
                IsTextAutoFit = true
            };
            chart.Legends.Add(legend);

            var series = new Series("Data") 
            { 
                ChartType = SeriesChartType.Pie,
                Legend = "MainLegend"
            };

            foreach (DataRow row in data.Rows)
            {
                series.Points.AddXY(row[labelColumn], row[valueColumn]);
            }

            chart.Series.Add(series);
        }

        private void SetupLineChart(Chart chart, DataTable data, string xAxis, string[] yAxes, string title)
        {
            if (chart == null || data == null || data.Rows.Count == 0) return;

            chart.Titles.Clear();
            chart.Titles.Add(title);
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add("MainArea");
            
            // Добавляем легенду
            chart.Legends.Clear();
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9),
                IsTextAutoFit = true
            };
            chart.Legends.Add(legend);

            foreach (string yAxis in yAxes)
            {
                var series = new Series(yAxis)
                {
                    ChartType = SeriesChartType.Line,
                    Color = GetColor(yAxes.ToList().IndexOf(yAxis)),
                    Legend = "MainLegend"
                };

                foreach (DataRow row in data.Rows)
                {
                    series.Points.AddXY(row[xAxis], row[yAxis]);
                }

                chart.Series.Add(series);
            }
        }

        private Color GetColor(int index)
        {
            var colors = new[] {
        Color.FromArgb(0, 122, 204),
        Color.FromArgb(40, 167, 69),
        Color.FromArgb(255, 193, 7),
        Color.FromArgb(220, 53, 69),
        Color.FromArgb(108, 117, 125),
        Color.FromArgb(23, 162, 184),
        Color.FromArgb(102, 16, 242),
        Color.FromArgb(217, 83, 25),
        Color.FromArgb(28, 186, 79),
        Color.FromArgb(142, 68, 173)
    };
            return colors[index % colors.Length];
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

        private void LoadFacultiesToComboBox(ComboBox cmb)
        {
            try
            {
                var dt = DBConnection.Instance.ExecuteQuery("SELECT FacultyID, FacultyName FROM Faculties ORDER BY FacultyName");

                // ✅ Создаём новую таблицу с "Все факультеты"
                var newDt = new DataTable();
                newDt.Columns.Add("FacultyID", typeof(int));
                newDt.Columns.Add("FacultyName", typeof(string));

                // Добавляем "Все факультеты"
                newDt.Rows.Add(0, "Все факультеты");

                // Добавляем остальные факультеты
                foreach (DataRow row in dt.Rows)
                {
                    newDt.Rows.Add(row["FacultyID"], row["FacultyName"]);
                }

                cmb.DataSource = newDt;
                cmb.DisplayMember = "FacultyName";
                cmb.ValueMember = "FacultyID";

                // ✅ Безопасная установка SelectedValue
                if (cmb.Items.Count > 0)
                {
                    cmb.SelectedValue = 0;  // ✅ Устанавливаем значение, а не индекс
                }
            }
            catch (Exception ex)
            {
                // ✅ Если ошибка - создаём пустой ComboBox
                cmb.DataSource = null;
                cmb.Items.Clear();
                cmb.Items.Add("Все факультеты");
                cmb.SelectedIndex = 0;
            }
        }

        private void LoadSectionsToComboBox(ComboBox cmb)
        {
            try
            {
                var dt = DBConnection.Instance.ExecuteQuery("SELECT SectionID, SectionName FROM Sections ORDER BY SectionName");

                // ✅ Создаём новую таблицу с "Все секции"
                var newDt = new DataTable();
                newDt.Columns.Add("SectionID", typeof(int));
                newDt.Columns.Add("SectionName", typeof(string));

                // Добавляем "Все секции"
                newDt.Rows.Add(0, "Все секции");

                // Добавляем остальные секции
                foreach (DataRow row in dt.Rows)
                {
                    newDt.Rows.Add(row["SectionID"], row["SectionName"]);
                }

                cmb.DataSource = newDt;
                cmb.DisplayMember = "SectionName";
                cmb.ValueMember = "SectionID";

                // ✅ Безопасная установка SelectedValue
                if (cmb.Items.Count > 0)
                {
                    cmb.SelectedValue = 0;  // ✅ Устанавливаем значение, а не индекс
                }
            }
            catch (Exception ex)
            {
                // ✅ Если ошибка - создаём пустой ComboBox
                cmb.DataSource = null;
                cmb.Items.Clear();
                cmb.Items.Add("Все секции");
                cmb.SelectedIndex = 0;
            }
        }

        private void ShowAnalyticsWithCharts(ComboBox cmbType, ComboBox cmbSec, ComboBox cmbFaculty)
        {
            try
            {
                string analyticsType = cmbType.SelectedItem?.ToString() ?? "";
                int sectionID = 0;
                int facultyID = 0;

                if (cmbSec.SelectedValue != null)
                {
                    sectionID = cmbSec.SelectedValue is DataRowView rowView ? Convert.ToInt32(rowView["SectionID"]) : Convert.ToInt32(cmbSec.SelectedValue);
                }

                if (cmbFaculty.SelectedValue != null)
                {
                    facultyID = cmbFaculty.SelectedValue is DataRowView rowView2 ? Convert.ToInt32(rowView2["FacultyID"]) : Convert.ToInt32(cmbFaculty.SelectedValue);
                }

                var dgv = contentPanel.Controls.Find("dataGridView", true).FirstOrDefault() as DataGridView;
                var chart1 = contentPanel.Controls.Find("chart1", true).FirstOrDefault() as Chart;
                var chart2 = contentPanel.Controls.Find("chart2", true).FirstOrDefault() as Chart;
                
                // Проверка на null для графиков
                if (chart1 == null || chart2 == null)
                {
                    MessageBox.Show("Ошибка: не удалось найти элементы графиков", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DataTable data = null;
                string query = "";

                switch (analyticsType)
                {
                    case "📊 Посещаемость по секциям":
                        query = @"
                    SELECT 
                        sec.SectionName AS [Секция],
                        sp.SportName AS [Вид спорта],
                        COUNT(DISTINCT s.StudentCardNumber) AS [Студентов],
                        COUNT(a.AttendanceID) AS [Посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                        CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Успеваемость %]
                    FROM Sections sec
                    JOIN Sports sp ON sec.SportID = sp.SportID
                    LEFT JOIN Schedule sc ON sec.SectionID = sc.SectionID
                    LEFT JOIN Attendance a ON sc.ScheduleID = a.ScheduleID
                    LEFT JOIN Students s ON a.StudentCardNumber = s.StudentCardNumber
                    WHERE (@SectionID = 0 OR sec.SectionID = @SectionID)
                    AND (@FacultyID = 0 OR s.FacultyID = @FacultyID)
                    GROUP BY sec.SectionName, sp.SportName
                    ORDER BY [Студентов] DESC";

                        data = DBConnection.Instance.ExecuteQuery(query, new[] {
                    new SqlParameter("@SectionID", sectionID),
                    new SqlParameter("@FacultyID", facultyID)
                });



                        // График 1: Столбчатая диаграмма по секциям
                        SetupBarChart(chart1, data, "Секция", new[] { "Студентов", "Посещений" }, "Распределение по секциям");

                        // График 2: Круговая диаграмма успеваемости
                        var pieData = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        CASE WHEN a.Status = 1 THEN 'Присутствовал' ELSE 'Отсутствовал' END AS [Статус],
                        COUNT(*) AS [Количество]
                    FROM Attendance a
                    JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                    JOIN Sections sec ON sc.SectionID = sec.SectionID
                    WHERE (@SectionID = 0 OR sec.SectionID = @SectionID)
                    GROUP BY a.Status", new[] {
                        new SqlParameter("@SectionID", sectionID)
                    });
                        SetupPieChart(chart2, pieData, "Статус", "Количество", "Распределение посещений");
                        break;

                    case "👥 Активность студентов":
                        query = @"
                    SELECT TOP 10
                        s.LastName + ' ' + s.FirstName AS [Студент],
                        s.GroupName AS [Группа],
                        COUNT(a.AttendanceID) AS [Посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Активность %]
                    FROM Students s
                    LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                    WHERE (@FacultyID = 0 OR s.FacultyID = @FacultyID)
                    GROUP BY s.LastName, s.FirstName, s.GroupName
                    ORDER BY [Посещений] DESC";

                        data = DBConnection.Instance.ExecuteQuery(query, new[] {
                    new SqlParameter("@FacultyID", facultyID)
                });

                        SetupBarChart(chart1, data, "Студент", new[] { "Посещений" }, "Топ студентов");

                        var groupData = DBConnection.Instance.ExecuteQuery(@"
                    SELECT GroupName AS [Группа], COUNT(*) AS [Студентов]
                    FROM Students
                    WHERE (@FacultyID = 0 OR FacultyID = @FacultyID)
                    GROUP BY GroupName");
                        SetupBarChart(chart2, groupData, "Группа", new[] { "Студентов" }, "Распределение по группам");
                        break;

                    case "💰 Финансовая аналитика":
                        query = @"
                    SELECT 
                        sec.SectionName AS [Секция],
                        sec.PricePerMonth AS [Цена],
                        COUNT(DISTINCT ss.StudentCardNumber) AS [Студентов],
                        sec.PricePerMonth * COUNT(DISTINCT ss.StudentCardNumber) AS [Доход],
                        sec.MaxStudents AS [Макс. мест],
                        CAST(COUNT(DISTINCT ss.StudentCardNumber) * 100.0 / NULLIF(sec.MaxStudents, 0) AS DECIMAL(5,1)) AS [Заполненность %]
                    FROM Sections sec
                    LEFT JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.IsActive = 1
                    GROUP BY sec.SectionName, sec.PricePerMonth, sec.MaxStudents
                    ORDER BY [Доход] DESC";

                        data = DBConnection.Instance.ExecuteQuery(query);

                        SetupBarChart(chart1, data, "Секция", new[] { "Доход", "Студентов" }, "Финансы по секциям");

                        var fillData = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        CASE 
                            WHEN CAST(COUNT(ss.StudentCardNumber) * 100.0 / NULLIF(sec.MaxStudents, 0) AS INT) >= 80 THEN 'Заполнена (>80%)'
                            WHEN CAST(COUNT(ss.StudentCardNumber) * 100.0 / NULLIF(sec.MaxStudents, 0) AS INT) >= 50 THEN 'Средняя (50-80%)'
                            ELSE 'Низкая (<50%)'
                        END AS [Категория],
                        COUNT(*) AS [Количество]
                    FROM Sections sec
                    LEFT JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.IsActive = 1
                    GROUP BY sec.MaxStudents");
                        SetupPieChart(chart2, fillData, "Категория", "Количество", "Заполненность секций");
                        break;

                    case "📅 Динамика посещаемости":
                        query = @"
                    SELECT 
                        FORMAT(a.VisitDate, 'yyyy-MM') AS [Месяц],
                        COUNT(a.AttendanceID) AS [Посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал]
                    FROM Attendance a
                    JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                    WHERE (@SectionID = 0 OR sc.SectionID = @SectionID)
                    GROUP BY FORMAT(a.VisitDate, 'yyyy-MM')
                    ORDER BY [Месяц]";

                        data = DBConnection.Instance.ExecuteQuery(query, new[] {
                    new SqlParameter("@SectionID", sectionID)
                });

                        SetupLineChart(chart1, data, "Месяц", new[] { "Посещений", "Присутствовал", "Отсутствовал" }, "Динамика по месяцам");

                        var trendData = data;
                        SetupLineChart(chart2, trendData, "Месяц", new[] { "Посещений" }, "Тренд");
                        break;
                }

                if (dgv != null && data != null)
                {
                    dgv.DataSource = data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== ОТЧЁТЫ (ПОЛНАЯ РЕАЛИЗАЦИЯ) ====================
        private void LoadReports()
        {
            headerLabel.Text = "📑 Генерация отчётов";
            contentPanel.Controls.Clear();

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            // === 1. ВЫБОР ТИПА ОТЧЁТА ===
            var typePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblType = new Label
            {
                Text = "📋 Выберите тип отчёта:",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var cmbReportType = new ComboBox
            {
                Name = "cmbReportType",
                Location = new Point(15, 38),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbReportType.Items.AddRange(new object[] {
        "🏛️ Для ректората",
        "🎓 Для факультета",
        "👨‍🏫 Для кафедры (нагрузка)",
        "💰 Финансовый",
        "📊 По посещаемости",
        "⚙️ Настраиваемый"
    });
            cmbReportType.SelectedIndex = 0;

            typePanel.Controls.AddRange(new Control[] { lblType, cmbReportType });

            // === 2. ДИНАМИЧЕСКИЕ ФИЛЬТРЫ ===
            var filterPanel = new Panel  // ✅ Объявляем ПЕРЕД использованием в лямбдах
            {
                Name = "filterPanel",
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 10, 0, 10)
            };

            // === 3. ТАБЛИЦА (объявляем ПЕРЕД кнопками) ===
            var tablePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };

            var dgvReport = new DataGridView  // ✅ Объявляем ПЕРЕД использованием
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
            StyleDataGridView(dgvReport);
            dgvReport.CellFormatting += DgvReport_CellFormatting;

            tablePanel.Controls.Add(dgvReport);

            // === 4. КНОПКИ (теперь можно использовать dgvReport и filterPanel) ===
            var btnPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(15)
            };

            var btnGenerate = new Button
            {
                Name = "btnGenerate",
                Text = "📄 Сформировать отчёт",
                Location = new Point(15, 8),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGenerate.Click += (s, e) =>
            {
                if (cmbReportType.SelectedItem?.ToString() == "⚙️ Настраиваемый")
                {
                    // Открываем отдельное окно для настраиваемого отчёта
                    var customForm = new CustomReportForm();
                    customForm.ShowDialog();
                }
                else
                {
                    // Обычные отчёты
                    GenerateReport(cmbReportType, filterPanel, dgvReport);
                }
            };

            var btnExport = new Button
            {
                Text = "💾 Экспорт",
                Location = new Point(225, 8),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Enabled = false,
                Name = "btnExport"
            };
            btnExport.Click += (s, e) => ExportReportFromGrid(dgvReport, cmbReportType, filterPanel);

            btnPanel.Controls.AddRange(new Control[] { btnGenerate, btnExport });

            // === ДОБАВЛЯЕМ КОНТРОЛЫ В ПРАВИЛЬНОМ ПОРЯДКЕ ===
            // Сначала таблица (Fill), потом кнопки и фильтры (Top) — они лягут СВЕРХУ
            mainPanel.Controls.Add(tablePanel);      // 1. Fill
            mainPanel.Controls.Add(btnPanel);        // 2. Top
            mainPanel.Controls.Add(filterPanel);     // 3. Top
            mainPanel.Controls.Add(typePanel);       // 4. Top
            contentPanel.Controls.Add(mainPanel);

            // === ПОДПИСЫВАЕМ СОБЫТИЯ ПОСЛЕ СОЗДАНИЯ ВСЕХ ЭЛЕМЕНТОВ ===
            cmbReportType.SelectedIndexChanged += (s, e) => UpdateReportFilters(cmbReportType, filterPanel);

            // Инициализация фильтров и авто-генерация
            UpdateReportFilters(cmbReportType, filterPanel);
            GenerateReport(cmbReportType, filterPanel, dgvReport);
        }

        // ==================== ОБНОВЛЕНИЕ ФИЛЬТРОВ ПО ТИПУ ОТЧЁТА ====================
        private void UpdateReportFilters(ComboBox cmbType, Panel filterPanel)
        {
            filterPanel.Controls.Clear();
            string reportType = cmbType.SelectedItem?.ToString() ?? "";

            var controls = new List<Control>();
            int y = 20;

            // Общие фильтры: период
            controls.Add(new Label { Text = "📅 Период:", Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            var dtpStart = new DateTimePicker { Name = "dtpStart", Location = new Point(90, y - 3), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };
            var lblTo = new Label { Text = "по", Location = new Point(230, y), AutoSize = true };
            var dtpEnd = new DateTimePicker { Name = "dtpEnd", Location = new Point(255, y - 3), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            controls.AddRange(new Control[] { dtpStart, lblTo, dtpEnd });
            y += 35;

            // Фильтры по типу
            switch (reportType)
            {
                case "🏛️ Для ректората":
                    /*controls.Add(new Label
                    {
                        Text = "ℹ️ Отчёт формируется по всем факультетам",
                        Location = new Point(15, y),
                        AutoSize = true,
                        ForeColor = Color.Gray,
                        Font = new Font("Segoe UI", 9)
                    });*/
                    break;

                case "🎓 Для факультета":
                    controls.Add(new Label { Text = "Факультет:", Location = new Point(15, y), AutoSize = true });
                    var cmbFaculty2 = new ComboBox { Name = "cmbFaculty", Location = new Point(100, y - 3), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
                    LoadFacultiesToComboBox(cmbFaculty2);
                    controls.Add(cmbFaculty2);
                    y += 35;
                    controls.Add(new Label { Text = "Группа:", Location = new Point(15, y), AutoSize = true });
                    var cmbGroup = new ComboBox { Name = "cmbGroup", Location = new Point(100, y - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
                    cmbGroup.Items.Add("Все группы");
                    var groups = DBConnection.Instance.ExecuteQuery("SELECT DISTINCT GroupName FROM Students ORDER BY GroupName");
                    foreach (DataRow row in groups.Rows) cmbGroup.Items.Add(row["GroupName"].ToString());
                    cmbGroup.SelectedIndex = 0;
                    controls.Add(cmbGroup);
                    break;

                case "👨‍🏫 Для кафедры (нагрузка)":
                    controls.Add(new Label { Text = "Кафедра/Секция:", Location = new Point(15, y), AutoSize = true });
                    var cmbSection = new ComboBox { Name = "cmbSection", Location = new Point(130, y - 3), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
                    LoadSectionsToComboBox(cmbSection);
                    controls.Add(cmbSection);
                    break;

                case "💰 Финансовый":
                    controls.Add(new Label { Text = "Вид спорта:", Location = new Point(15, y), AutoSize = true });
                    var cmbSport = new ComboBox { Name = "cmbSport", Location = new Point(100, y - 3), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

                    // Загружаем виды спорта + опцию "Все виды"
                    cmbSport.Items.Add(new { Value = 0, Text = "Все виды спорта" });
                    var sports = DBConnection.Instance.ExecuteQuery("SELECT SportID, SportName FROM Sports ORDER BY SportName");
                    foreach (DataRow row in sports.Rows)
                    {
                        cmbSport.Items.Add(new { Value = Convert.ToInt32(row["SportID"]), Text = row["SportName"].ToString() });
                    }
                    cmbSport.SelectedIndex = 0;
                    cmbSport.DisplayMember = "Text";
                    cmbSport.ValueMember = "Value";

                    controls.Add(cmbSport);
                    break;

                case "📊 По посещаемости":
                    controls.Add(new Label { Text = "Секция:", Location = new Point(15, y), AutoSize = true });
                    var cmbSecAtt = new ComboBox { Name = "cmbSection", Location = new Point(80, y - 3), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
                    LoadSectionsToComboBox(cmbSecAtt);
                    controls.Add(cmbSecAtt);
                    y += 35;
                    controls.Add(new Label { Text = "Мин. пропусков:", Location = new Point(15, y), AutoSize = true });
                    var numMinAbs = new NumericUpDown { Name = "numMinAbs", Location = new Point(130, y - 3), Width = 60, Minimum = 0, Maximum = 100, Value = 3 };
                    controls.Add(numMinAbs);
                    controls.Add(new Label { Text = "Показывать:", Location = new Point(210, y), AutoSize = true });
                    var chkOnlyAbsent = new CheckBox { Name = "chkOnlyAbsent", Text = "Только прогульщиков", Location = new Point(290, y - 3), AutoSize = true, Checked = true };
                    controls.Add(chkOnlyAbsent);
                    break;

                case "⚙️ Настраиваемый":
                    // Пусто
                    break;
            }

            filterPanel.Controls.AddRange(controls.ToArray());
        }

        // ==================== ГЕНЕРАЦИЯ ОТЧЁТА ====================
        private void GenerateReport(ComboBox cmbType, Panel filterPanel, DataGridView dgv)
        {
            try
            {
                string reportType = cmbType.SelectedItem?.ToString() ?? "";
                var dtpStart = GetControl<DateTimePicker>(filterPanel, "dtpStart");
                var dtpEnd = GetControl<DateTimePicker>(filterPanel, "dtpEnd");
                DateTime startDate = dtpStart?.Value ?? DateTime.Now.AddMonths(-1);
                DateTime endDate = dtpEnd?.Value ?? DateTime.Now;

                DataTable reportData = GetReportQuery(reportType, filterPanel, startDate, endDate);

                if (reportData == null || reportData.Rows.Count == 0)
                {
                    MessageBox.Show("⚠️ Нет данных для отображения", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dgv.DataSource = reportData;

                // Включаем кнопку экспорта
                var btnExport = contentPanel.Controls.Find("btnExport", true).FirstOrDefault() as Button;
                if (btnExport != null) btnExport.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== ПОЛУЧЕНИЕ ДАННЫХ ПО ТИПУ ОТЧЁТА ====================
        private DataTable GetReportQuery(string reportType, Panel filterPanel, DateTime startDate, DateTime endDate)
        {
            string query = "";
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@StartDate", startDate.Date),
        new SqlParameter("@EndDate", endDate.Date)
    };

            switch (reportType)
            {
                // 🏛️ ДЛЯ РЕКТОРАТА - сводная статистика по университету
                case "🏛️ Для ректората":
                    // ✅ НЕ добавляем @FacultyID — показываем все факультеты
                    // Опционально: если выбран факультет, фильтруем
                    var facultyParam = GetControl<ComboBox>(filterPanel, "cmbFaculty")?.SelectedValue;
                    if (facultyParam != null && Convert.ToInt32(facultyParam) > 0)
                        parameters.Add(new SqlParameter("@FacultyID", facultyParam));
                    else
                        parameters.Add(new SqlParameter("@FacultyID", DBNull.Value));

                    query = @"
        WITH SectionStats AS (
            SELECT 
                sec.SectionID,
                sec.PricePerMonth,
                COUNT(ss.StudentCardNumber) AS StudentCount
            FROM Sections sec
            LEFT JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.IsActive = 1
            GROUP BY sec.SectionID, sec.PricePerMonth
        )
        SELECT 
            f.FacultyName AS [Факультет],
            COUNT(DISTINCT s.StudentCardNumber) AS [Всего студентов],
            COUNT(DISTINCT sec.SectionID) AS [Активных секций],
            COUNT(DISTINCT a.AttendanceID) AS [Записей посещаемости],
            CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                 NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Ср. посещаемость %],
            ISNULL(SUM(ss.PricePerMonth * ss.StudentCount), 0) AS [Расчётный доход]
        FROM Faculties f
        LEFT JOIN Students s ON f.FacultyID = s.FacultyID
        LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
        LEFT JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
        LEFT JOIN Sections sec ON sc.SectionID = sec.SectionID
        LEFT JOIN SectionStats ss ON sec.SectionID = ss.SectionID
        WHERE (a.VisitDate IS NULL OR a.VisitDate BETWEEN @StartDate AND @EndDate)
        AND (@FacultyID IS NULL OR f.FacultyID = @FacultyID)
        GROUP BY f.FacultyName
        ORDER BY [Расчётный доход] DESC";
                    break;

                // 🎓 ДЛЯ ФАКУЛЬТЕТА - по группам
                case "🎓 Для факультета":
                    var facId = GetControl<ComboBox>(filterPanel, "cmbFaculty")?.SelectedValue;
                    var groupCombo = GetControl<ComboBox>(filterPanel, "cmbGroup");
                    string groupName = groupCombo?.SelectedValue?.ToString();

                    // ✅ Всегда добавляем FacultyID
                    if (facId != null)
                        parameters.Add(new SqlParameter("@FacultyID", facId));

                    // ✅ Добавляем GroupName только если выбрана конкретная группа (не "Все группы")
                    if (groupCombo != null && groupCombo.SelectedIndex > 0)
                    {
                        parameters.Add(new SqlParameter("@GroupName", groupName));
                    }
                    else
                    {
                        parameters.Add(new SqlParameter("@GroupName", DBNull.Value));
                    }

                    query = @"
        SELECT 
            s.GroupName AS [Группа],
            COUNT(DISTINCT s.StudentCardNumber) AS [Студентов],
            COUNT(DISTINCT a.AttendanceID) AS [Посещений],
            SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
            SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
            CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                 NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Посещаемость %]
        FROM Students s
        LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
        WHERE s.FacultyID = @FacultyID
        AND (@GroupName IS NULL OR s.GroupName = @GroupName)
        AND (a.VisitDate IS NULL OR a.VisitDate BETWEEN @StartDate AND @EndDate)
        GROUP BY s.GroupName
        ORDER BY [Посещаемость %] DESC";
                    break;

                // 👨‍🏫 ДЛЯ КАФЕДРЫ - нагрузка тренеров
                case "👨‍🏫 Для кафедры (нагрузка)":

                    parameters.Add(new SqlParameter("@SectionID", DBNull.Value));

                    query = @"
        WITH StudentCount AS (
            SELECT 
                SectionID,
                COUNT(StudentCardNumber) AS Count
            FROM StudentSections
            WHERE IsActive = 1
            GROUP BY SectionID
        ),
        AttendanceCount AS (
            SELECT 
                sc.SectionID,
                COUNT(DISTINCT a.AttendanceID) AS VisitCount
            FROM Schedule sc
            LEFT JOIN Attendance a ON sc.ScheduleID = a.ScheduleID
            WHERE a.VisitDate BETWEEN @StartDate AND @EndDate
            GROUP BY sc.SectionID
        )
        SELECT 
            t.LastName + ' ' + t.FirstName AS [Тренер],
            t.Qualification AS [Квалификация],
            t.Specialization AS [Специализация],
            COUNT(DISTINCT sec.SectionID) AS [Ведёт секций],
            ISNULL(SUM(sc.Count), 0) AS [Всего студентов],
            ISNULL(SUM(ac.VisitCount), 0) AS [Проведено занятий],
            CAST(ISNULL(SUM(ac.VisitCount), 0) AS DECIMAL(10,1)) / 
                NULLIF(COUNT(DISTINCT sec.SectionID), 0) AS [Ср. занятий/секцию],
            ISNULL(SUM(sec.PricePerMonth * sc.Count), 0) AS [Доход секций]
        FROM Trainers t
        LEFT JOIN Sections sec ON t.TrainerID = sec.TrainerID
        LEFT JOIN StudentCount sc ON sec.SectionID = sc.SectionID
        LEFT JOIN AttendanceCount ac ON sec.SectionID = ac.SectionID
        WHERE a.VisitDate BETWEEN @StartDate AND @EndDate
        GROUP BY t.LastName, t.FirstName, t.Qualification, t.Specialization
        ORDER BY [Доход секций] DESC";
                    break;

                // 💰 ФИНАНСОВЫЙ
                case "💰 Финансовый":
                    var cmbSport = GetControl<ComboBox>(filterPanel, "cmbSport");
                    object sportValue = cmbSport?.SelectedValue;
                    int sportID = sportValue != null ? Convert.ToInt32(sportValue) : 0;

                    if (sportID > 0)
                        parameters.Add(new SqlParameter("@SportID", sportID));
                    else
                        parameters.Add(new SqlParameter("@SportID", DBNull.Value));

                    query = @"
        SELECT 
            sec.SectionName AS [Секция],
            sp.SportName AS [Вид спорта],
            t.LastName + ' ' + t.FirstName AS [Тренер],
            sec.PricePerMonth AS [Цена/мес],
            COUNT(DISTINCT ss.StudentCardNumber) AS [Активных студентов],
            sec.PricePerMonth * COUNT(DISTINCT ss.StudentCardNumber) AS [Доход/мес],
            sec.PricePerMonth * COUNT(DISTINCT ss.StudentCardNumber) * 6 AS [Доход/семестр],
            sec.MaxStudents AS [Макс. мест],
            CASE WHEN sec.MaxStudents > 0 
                 THEN CAST(COUNT(DISTINCT ss.StudentCardNumber) * 100.0 / sec.MaxStudents AS DECIMAL(5,1))
                 ELSE 0 END AS [Заполненность %]
        FROM Sections sec
        JOIN Sports sp ON sec.SportID = sp.SportID
        JOIN Trainers t ON sec.TrainerID = t.TrainerID
        LEFT JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.IsActive = 1
        WHERE (@SportID IS NULL OR @SportID = 0 OR sp.SportID = @SportID)
        GROUP BY sec.SectionName, sp.SportName, t.LastName, t.FirstName, 
                 sec.PricePerMonth, sec.MaxStudents
        ORDER BY [Доход/семестр] DESC";
                    break;

                // 📊 ПО ПОСЕЩАЕМОСТИ (с подсветкой прогульщиков)
                case "📊 По посещаемости":
                    var secAttId = GetControl<ComboBox>(filterPanel, "cmbSection")?.SelectedValue;
                    var minAbs = GetControl<NumericUpDown>(filterPanel, "numMinAbs")?.Value ?? 3;
                    var onlyAbsent = GetControl<CheckBox>(filterPanel, "chkOnlyAbsent")?.Checked ?? true;

                    if (secAttId != null) parameters.Add(new SqlParameter("@SectionID", secAttId));
                    parameters.Add(new SqlParameter("@MinAbsent", minAbs));

                    query = @"
                SELECT 
                    s.StudentCardNumber AS [Билет],
                    s.LastName + ' ' + s.FirstName AS [ФИО],
                    s.GroupName AS [Группа],
                    f.FacultyName AS [Факультет],
                    COUNT(a.AttendanceID) AS [Всего занятий],
                    SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                    SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                    SUM(CASE WHEN a.Status = 0 AND (a.Notes IS NULL OR LTRIM(RTRIM(a.Notes)) = '') THEN 1 ELSE 0 END) AS [Без уваж. причины],
                    CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                         NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Успеваемость %]
                FROM Students s
                JOIN Faculties f ON s.FacultyID = f.FacultyID
                LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                LEFT JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                WHERE sc.SectionID = @SectionID
                AND (a.VisitDate IS NULL OR a.VisitDate BETWEEN @StartDate AND @EndDate)
                GROUP BY s.StudentCardNumber, s.LastName, s.FirstName, s.GroupName, f.FacultyName
                HAVING COUNT(a.AttendanceID) > 0
                AND (@MinAbsent = 0 OR SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) >= @MinAbsent)
                ORDER BY [Без уваж. причины] DESC, [Отсутствовал] DESC";
                    break;

                // ⚙️ НАСТРАИВАЕМЫЙ
                case "⚙️ Настраиваемый":
                    // ✅ Открываем отдельную форму
                    var customForm = new CustomReportForm();
                    customForm.ShowDialog();
                    return null;  // Возвращаем null, так как данные показываются в отдельном окне
            }

            return DBConnection.Instance.ExecuteQuery(query, parameters.ToArray());
        }

        // ==================== СТИЛИЗАЦИЯ ТАБЛИЦЫ БЕЗ ВЫДЕЛЕНИЯ ====================
        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.DefaultCellStyle.SelectionBackColor = Color.White;     // Белый фон при выделении
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;     // Чёрный текст
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(45, 55, 75);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            dgv.RowHeadersVisible = false;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.GridColor = Color.FromArgb(220, 220, 220);
            dgv.MultiSelect = false;  // ✅ Запретить множественное выделение
        }

        // ==================== НАСТРАИВАЕМЫЙ ОТЧЁТ ====================
        private void UpdateCustomReportPanel(Panel filterPanel)
        {
            filterPanel.Controls.Clear();
            filterPanel.Height = 220;  // ✅ Увеличиваем высоту панели

            var controls = new List<Control>();
            int y = 15;

            // === ЗАГОЛОВОК ===
            var lblTitle = new Label
            {
                Text = "⚙️ Конструктор отчёта",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(15, y),
                AutoSize = true
            };
            controls.Add(lblTitle);
            y += 35;

            // === 1. ВЫБОР ТАБЛИЦЫ ===
            var grpTable = new GroupBox
            {
                Text = "📊 Выберите таблицу",
                Location = new Point(15, y),
                Size = new Size(220, 130),  // ✅ Фиксированный размер
                Font = new Font("Segoe UI", 9)
            };

            var cmbTable = new ComboBox
            {
                Name = "cmbTable",
                Location = new Point(15, 25),
                Width = 190,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbTable.Items.AddRange(new object[] {
        "🎓 Students", "📋 Attendance", "⚽ Sections",
        "👨‍🏫 Trainers", "🏛️ Faculties", "🏆 Achievements"
    });
            cmbTable.SelectedIndex = 0;
            cmbTable.SelectedIndexChanged += (s, e) => UpdateAvailableFields(filterPanel);

            var chkDistinct = new CheckBox
            {
                Name = "chkDistinct",
                Text = "DISTINCT",
                Location = new Point(15, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 8)
            };

            grpTable.Controls.AddRange(new Control[] { cmbTable, chkDistinct });
            controls.Add(grpTable);

            // === 2. ВЫБОР ПОЛЕЙ ===
            var grpFields = new GroupBox
            {
                Text = "📋 Поля для отображения",
                Location = new Point(245, y),  // ✅ Справа от таблицы
                Size = new Size(450, 130),
                Font = new Font("Segoe UI", 9)
            };

            var fldPanel = new FlowLayoutPanel
            {
                Name = "fldPanel",
                Location = new Point(15, 25),
                Size = new Size(420, 90),  // ✅ Больше места
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true
            };

            grpFields.Controls.Add(fldPanel);
            controls.Add(grpFields);

            // === 3. ФИЛЬТРЫ ===
            y += 145;
            var grpFilters = new GroupBox
            {
                Text = "🔍 WHERE и ORDER BY",
                Location = new Point(15, y),
                Size = new Size(680, 70),
                Font = new Font("Segoe UI", 9)
            };

            var lblWhere = new Label { Text = "WHERE:", Location = new Point(15, 25), AutoSize = true };
            var txtWhere = new TextBox
            {
                Name = "txtWhere",
                Location = new Point(65, 22),
                Width = 300,
                PlaceholderText = "Course = 3 AND FacultyID = 1",
                Font = new Font("Segoe UI", 8)
            };

            var lblOrder = new Label { Text = "ORDER BY:", Location = new Point(380, 25), AutoSize = true };
            var txtOrderBy = new TextBox
            {
                Name = "txtOrderBy",
                Location = new Point(455, 22),
                Width = 150,
                PlaceholderText = "LastName",
                Font = new Font("Segoe UI", 8)
            };

            var cmbOrderDir = new ComboBox
            {
                Name = "cmbOrderDir",
                Location = new Point(615, 22),
                Width = 55,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbOrderDir.Items.AddRange(new object[] { "ASC", "DESC" });
            cmbOrderDir.SelectedIndex = 0;

            grpFilters.Controls.AddRange(new Control[] { lblWhere, txtWhere, lblOrder, txtOrderBy, cmbOrderDir });
            controls.Add(grpFilters);

            // === 4. ДОПОЛНИТЕЛЬНО ===
            y += 220;
            var grpExtra = new GroupBox
            {
                Text = "📌 Дополнительно",
                Location = new Point(15, y),
                Size = new Size(680, 55),
                Font = new Font("Segoe UI", 9)
            };

            var lblLimit = new Label { Text = "MAX записей:", Location = new Point(15, 25), AutoSize = true };
            var numLimit = new NumericUpDown
            {
                Name = "numLimit",
                Location = new Point(95, 22),
                Width = 70,
                Minimum = 1,
                Maximum = 10000,
                Value = 100
            };

            var chkShowSQL = new CheckBox
            {
                Name = "chkShowSQL",
                Text = "Показать SQL перед выполнением",
                Location = new Point(180, 24),
                AutoSize = true,
                Font = new Font("Segoe UI", 8)
            };

            grpExtra.Controls.AddRange(new Control[] { lblLimit, numLimit, chkShowSQL });
            controls.Add(grpExtra);

            filterPanel.Controls.AddRange(controls.ToArray());

            // Инициализация полей
            UpdateAvailableFields(filterPanel);
        }

        private void UpdateAvailableFields(Panel filterPanel)
        {
            var cmbTable = GetControl<ComboBox>(filterPanel, "cmbTable");
            var fldPanel = GetControl<FlowLayoutPanel>(filterPanel, "fldPanel");
            if (cmbTable == null || fldPanel == null) return;
            fldPanel.Controls.Clear();

            var tableFields = new Dictionary<string, string[]>
            {
                ["🎓 Students"] = new[] { "StudentCardNumber", "LastName", "FirstName", "GroupName", "Course", "FacultyID" },
                ["📋 Attendance"] = new[] { "AttendanceID", "StudentCardNumber", "VisitDate", "Status", "Notes" },
                ["⚽ Sections"] = new[] { "SectionID", "SectionName", "PricePerMonth", "MaxStudents", "TrainerID" },
                ["👨‍🏫 Trainers"] = new[] { "TrainerID", "LastName", "FirstName", "Qualification", "Specialization" },
                ["🏛️ Faculties"] = new[] { "FacultyID", "FacultyName", "DeanName" },
                ["🏆 Achievements"] = new[] { "AchievementID", "CompetitionName", "Place", "AwardType", "CompetitionDate" }
            };

            string selectedTable = cmbTable.SelectedItem?.ToString() ?? "";
            if (tableFields.TryGetValue(selectedTable, out string[] fields))
            {
                foreach (var field in fields)
                {
                    var chk = new CheckBox { Text = field, AutoSize = true, Margin = new Padding(3), Tag = field };
                    chk.Checked = field.Contains("ID") || field.Contains("Name");
                    fldPanel.Controls.Add(chk);
                }
            }
        }

        private DataTable GetCustomReportData(Panel filterPanel, DateTime startDate, DateTime endDate)
        {
            var cmbTable = GetControl<ComboBox>(filterPanel, "cmbTable");
            var chkDistinct = GetControl<CheckBox>(filterPanel, "chkDistinct");
            var fldPanel = GetControl<FlowLayoutPanel>(filterPanel, "fldPanel");
            var txtWhere = GetControl<TextBox>(filterPanel, "txtWhere");
            var txtOrderBy = GetControl<TextBox>(filterPanel, "txtOrderBy");
            var cmbOrderDir = GetControl<ComboBox>(filterPanel, "cmbOrderDir");
            var numLimit = GetControl<NumericUpDown>(filterPanel, "numLimit");
            var chkShowSQL = GetControl<CheckBox>(filterPanel, "chkShowSQL");

            if (cmbTable == null || fldPanel == null) return null;

            var tableMap = new Dictionary<string, string>
            {
                ["🎓 Students"] = "Students",
                ["📋 Attendance"] = "Attendance",
                ["⚽ Sections"] = "Sections",
                ["👨‍🏫 Trainers"] = "Trainers",
                ["🏛️ Faculties"] = "Faculties",
                ["🏆 Achievements"] = "Achievements"
            };

            string selectedTable = cmbTable.SelectedItem?.ToString() ?? "";
            if (!tableMap.TryGetValue(selectedTable, out string tableName)) return null;

            var selectedFields = new List<string>();
            foreach (CheckBox chk in fldPanel.Controls)
            {
                if (chk.Checked && chk.Tag != null) selectedFields.Add(chk.Tag.ToString());
            }
            if (selectedFields.Count == 0) { MessageBox.Show("Выберите хотя бы одно поле", "Внимание"); return null; }

            string distinct = (chkDistinct?.Checked ?? false) ? "DISTINCT " : "";
            string fields = string.Join(", ", selectedFields);
            string whereClause = txtWhere?.Text ?? "";
            string orderBy = txtOrderBy?.Text ?? "";
            string orderDir = cmbOrderDir?.SelectedItem?.ToString() ?? "ASC";
            int limit = (int)(numLimit?.Value ?? 100);

            string query = $"SELECT {distinct}TOP {limit} {fields} FROM {tableName}";
            if (!string.IsNullOrWhiteSpace(whereClause)) query += $" WHERE {whereClause}";
            if (!string.IsNullOrWhiteSpace(orderBy)) query += $" ORDER BY {orderBy} {orderDir}";

            if (chkShowSQL?.Checked ?? false)
            {
                if (MessageBox.Show($"SQL:\n\n{query}\n\nПродолжить?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return null;
            }

            try { return DBConnection.Instance.ExecuteQuery(query); }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}\n\n{query}", "Ошибка"); return null; }
        }


        // ==================== ПОДСВЕТКА ПРОБЛЕМНЫХ СТУДЕНТОВ ====================
        private void DgvReport_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0) return;

            var row = dgv.Rows[e.RowIndex];

            // Подсветка для отчёта по посещаемости
            if (dgv.Columns[e.ColumnIndex].Name == "Без уваж. причины" ||
                dgv.Columns[e.ColumnIndex].HeaderText == "Без уваж. причины")
            {
                if (e.Value != null && Convert.ToInt32(e.Value) > 0)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);  // Светло-красный
                    row.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
            }

            // Подсветка низкой успеваемости
            if (dgv.Columns[e.ColumnIndex].HeaderText == "Успеваемость %" ||
                dgv.Columns[e.ColumnIndex].HeaderText == "Посещаемость %")
            {
                if (e.Value != null)
                {
                    decimal percent = Convert.ToDecimal(e.Value);
                    if (percent < 50)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                        row.DefaultCellStyle.ForeColor = Color.Red;
                    }
                    else if (percent < 75)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 200);
                    }
                }
            }
        }
        // ==================== ЭКСПОРТ ИЗ ТАБЛИЦЫ ====================
        private void ExportReportFromGrid(DataGridView dgv, ComboBox cmbType, Panel filterPanel)
        {
            if (dgv.DataSource is not DataTable data || data.Rows.Count == 0)
            {
                MessageBox.Show("⚠️ Нет данных для экспорта", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Получаем параметры
            string reportType = cmbType.SelectedItem?.ToString() ?? "Отчёт";

            var dtpStart = GetControl<DateTimePicker>(filterPanel, "dtpStart");
            var dtpEnd = GetControl<DateTimePicker>(filterPanel, "dtpEnd");
            DateTime startDate = dtpStart?.Value ?? DateTime.Now.AddMonths(-1);
            DateTime endDate = dtpEnd?.Value ?? DateTime.Now;

            // ✅ Вызываем оригинальный ExportReport с 4 аргументами
            ExportReport(data, reportType, startDate, endDate);
        }


        // ==================== ВСПОМОГАТЕЛЬНЫЙ МЕТОД ПОИСКА КОНТРОЛА ====================
        private T GetControl<T>(Panel panel, string name) where T : Control
        {
            var controls = panel.Controls.Find(name, true);
            return controls.FirstOrDefault() as T;
        }

        // ==================== ЭКСПОРТ (теперь с реальными данными) ====================
        private void ExportReport(DataTable data, string reportType, DateTime startDate, DateTime endDate)
        {
            try
            {
                string fileName = $"Отчёт_{reportType.Replace(" ", "_").Replace("🏛️", "").Replace("🎓", "").Replace("👨‍🏫", "").Replace("💰", "").Replace("📊", "").Replace("⚙️", "").Trim()}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
                string folder = Path.Combine(Application.StartupPath, "Отчёты");

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV (Excel)|*.csv|Excel|*.xlsx|PDF|*.pdf";
                    sfd.FileName = fileName;
                    sfd.InitialDirectory = folder;

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = sfd.FileName;
                        string ext = Path.GetExtension(filePath).ToLower();

                        if (ext == ".csv")
                        {
                            ExportToCSV(data, filePath);  // ✅ 2 аргумента
                        }
                        else if (ext == ".xlsx")
                        {
                            MessageBox.Show("📄 Экспорт в Excel требует библиотеку EPPlus.\nДля курсовой используйте CSV.",
                                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ExportToCSV(data, filePath.Replace(".xlsx", ".csv"));  // ✅ 2 аргумента
                        }
                        else if (ext == ".pdf")
                        {
                            MessageBox.Show("📄 Экспорт в PDF требует библиотеку iTextSharp.\nДля курсовой используйте CSV.",
                                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ExportToCSV(data, filePath.Replace(".pdf", ".csv"));  // ✅ 2 аргумента
                        }

                        MessageBox.Show($"✅ Отчёт сохранён:\n{filePath}", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        if (MessageBox.Show("Открыть файл?", "Отчёт готов",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка экспорта: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==================== ЭКСПОРТ В CSV (2 аргумента) ====================
        private void ExportToCSV(DataTable dt, string filePath)
        {
            var csvContent = new StringBuilder();

            // Заголовки
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                csvContent.Append($"\"{dt.Columns[i].ColumnName}\"");
                if (i < dt.Columns.Count - 1) csvContent.Append(";");
            }
            csvContent.AppendLine();

            // Данные
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string value = row[i]?.ToString()?.Replace("\"", "\"\"") ?? "";
                    csvContent.Append($"\"{value}\"");
                    if (i < dt.Columns.Count - 1) csvContent.Append(";");
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
            StyleDataGridView(dgv);

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
            StyleDataGridView(dgv);

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
            StyleDataGridView(dgv);

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