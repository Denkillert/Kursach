using Microsoft.Data.SqlClient;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;
using PolesSU_Sports.Management;
using PolesSU_Sports.Management.SettingsForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace PolesSU_Sports.Management
{
    public partial class ManagerForm : Form
    {
        private Panel sidebarPanel;
        private Panel headerPanel;
        private Panel contentPanel;
        private Label headerLabel;
        private Button currentActiveButton;

        // ✅ ЦВЕТА ПОЛЕСГУ
        private readonly Color GreenMain = Color.FromArgb(5, 66, 38);
        private readonly Color GreenLight = Color.FromArgb(6, 87, 50);
        private readonly Color BlueAccent = Color.FromArgb(0, 147, 197);
        private readonly Color GrayBg = Color.FromArgb(245, 247, 250);

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
                Height = 180,
                BackColor = GreenMain,
                Padding = new Padding(20, 25, 20, 20)
            };
            var logoPictureBox = new PictureBox
            {
                Image = Properties.Resources.PolesSU_Emblem,
                Size = new Size(160, 160),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Location = new Point(40, 80)  // ✅ Эмблема НИЖЕ
            };
            var logoLabel = new Label
            {
                Text = "PolesSU\nSports",
                Font = new System.Drawing.Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(220, 80),
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(20, 10),
            };
            logoPanel.Controls.AddRange(logoLabel, logoPictureBox);
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
            CreateMenuButton("📊 Главная", LoadDashboard, true);
            CreateMenuButton("📈 Аналитика", LoadAnalytics, false);
            CreateMenuButton("📑 Отчёты", LoadReports, false);
            CreateMenuButton("🎓 По факультетам", LoadFacultyReports, false);
            CreateMenuButton("👥 По группам", LoadGroupReports, false);
            CreateMenuButton("⚽ По секциям", LoadSectionReports, false);
            CreateMenuButton("🎓 Заявки студентов", LoadStudentRequests, false);
            CreateMenuButton("⚙️ Настройки", LoadSettings, false);

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
                Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold),
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
                Text = "Дашборд",
                Font = new System.Drawing.Font("Segoe UI", 22, FontStyle.Bold),
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
                Font = new System.Drawing.Font("Segoe UI", 16),
                Location = new Point(10, 5),
                Size = new Size(35, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var userInfo = new Label
            {
                Text = $"{User.CurrentUser?.Login ?? "Manager"}\nManager",
                Font = new System.Drawing.Font("Segoe UI", 9),
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
                Font = new System.Drawing.Font("Segoe UI", 11, isActive ? FontStyle.Bold : FontStyle.Regular),
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
                currentActiveButton.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Regular);
            }
            currentActiveButton = btn;
            btn.BackColor = GreenLight;
            btn.Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold);
        }

        // ==================== ДАШБОРД ====================
        private void LoadDashboard()
        {
            headerLabel.Text = "📊 Обзор показателей";
            contentPanel.Controls.Clear();

            // Заголовок с датой
            var dateLabel = new Label
            {
                Text = DateTime.Now.ToString("dd MMMM yyyy"),
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(0, 5),
                AutoSize = true
            };
            contentPanel.Controls.Add(dateLabel);

            var statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 40, 0, 30),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                AutoScroll = false,
                BackColor = Color.Transparent
            };

            // 🎓 Студенты
            AddModernStatCard(statsPanel, "🎓 Всего студентов",
                Count("SELECT COUNT(*) FROM Students"),
                Color.FromArgb(0, 102, 204),
                "SELECT TOP 10 FacultyName AS [Факультет], COUNT(*) AS [Студентов] FROM Students s JOIN Faculties f ON s.FacultyID = f.FacultyID GROUP BY FacultyName ORDER BY [Студентов] DESC",
                new[] { "Факультет", "Студентов" });

            // 👨‍ Тренеры
            AddModernStatCard(statsPanel, "👨‍🏫 Тренеров",
                Count("SELECT COUNT(*) FROM Trainers"),
                Color.FromArgb(40, 167, 69),
                "SELECT TOP 10 LastName + ' ' + FirstName AS [Тренер], ISNULL(Qualification, 'Не указана') AS [Квалификация] FROM Trainers ORDER BY LastName",
                new[] { "Тренер", "Квалификация" });

            // ⚽ Секции
            AddModernStatCard(statsPanel, "⚽ Секций",
                Count("SELECT COUNT(*) FROM Sections"),
                Color.FromArgb(255, 193, 7),
                "SELECT TOP 10 SectionName AS [Секция], ISNULL(SportName, 'Не указан') AS [Вид спорта], ISNULL(CAST(PricePerMonth AS VARCHAR), '0') AS [Цена] FROM Sections sec LEFT JOIN Sports sp ON sec.SportID = sp.SportID ORDER BY SectionName",
                new[] { "Секция", "Вид спорта", "Цена" });

            // 📋 Посещения
            AddModernStatCard(statsPanel, "📋 Посещений (мес)",
                Count("SELECT COUNT(*) FROM Attendance WHERE MONTH(VisitDate) = MONTH(GETDATE())"),
                Color.FromArgb(220, 53, 69),
                "SELECT TOP 10 FORMAT(VisitDate, 'dd.MM.yyyy') AS [Дата], COUNT(*) AS [Посещения] FROM Attendance WHERE MONTH(VisitDate) = MONTH(GETDATE()) GROUP BY VisitDate ORDER BY VisitDate DESC",
                new[] { "Дата", "Посещения" });

            contentPanel.Controls.Add(statsPanel);
        }

        private void AddModernStatCard(FlowLayoutPanel p, string title, string val, Color c, string detailQuery, string[] detailColumns)
        {
            var card = new Panel
            {
                Size = new Size(220, 140),
                Margin = new Padding(15),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand,
                Tag = new CardData { Title = title, Query = detailQuery, Columns = detailColumns, Color = c },
                Padding = new Padding(0)
            };

            // Тень
            card.Paint += (s, e) =>
            {
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddRectangle(new System.Drawing.Rectangle(0, 0, card.Width - 1, card.Height - 1));
                    using (var shadow = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        shadow.CenterColor = Color.FromArgb(20, 0, 0, 0);
                        shadow.SurroundColors = new[] { Color.Transparent };
                        e.Graphics.FillRectangle(shadow, new System.Drawing.Rectangle(0, 0, card.Width, card.Height));
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
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 100, 100),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            var valueLabel = new Label
            {
                Text = val,
                Location = new Point(20, 45),
                Font = new System.Drawing.Font("Segoe UI", 42, FontStyle.Bold),
                ForeColor = c,
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            var suffixLabel = new Label
            {
                Text = GetSuffix(title),
                Location = new Point(20, 95),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent
            };

            // Индикатор раскрытия (стрелка)
            var arrowLabel = new Label
            {
                Text = "▼",
                Location = new Point(185, 15),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(150, 150, 150),
                Tag = "arrow",
                Cursor = Cursors.Hand
            };

            // Контейнер для деталей (изначально скрыт)
            var detailPanel = new Panel
            {
                Location = new Point(0, 140),
                Size = new Size(220, 0),
                BackColor = Color.FromArgb(250, 250, 250),
                Tag = "details",
                Visible = false,
                Padding = new Padding(0)
            };

            card.Controls.AddRange(new Control[] { titleLabel, valueLabel, suffixLabel, arrowLabel, detailPanel });
            p.Controls.Add(card);

            // ✅ КЛИК ПО КАРТОЧКЕ - раскрываем/сворачиваем
            card.Click += (s, e) => ToggleCard(card, detailPanel, arrowLabel, detailQuery, detailColumns);
            titleLabel.Click += (s, e) => ToggleCard(card, detailPanel, arrowLabel, detailQuery, detailColumns);
            valueLabel.Click += (s, e) => ToggleCard(card, detailPanel, arrowLabel, detailQuery, detailColumns);
            arrowLabel.Click += (s, e) => ToggleCard(card, detailPanel, arrowLabel, detailQuery, detailColumns);

            // Hover эффект
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(250, 250, 250);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;
        }

        // ✅ МЕТОД ПЕРЕКЛЮЧЕНИЯ КАРТОЧКИ
        private void ToggleCard(Panel card, Panel detailPanel, Label arrowLabel, string query, string[] columns)
        {
            bool isExpanded = detailPanel.Height > 0;

            if (isExpanded)
            {
                // СВОРАЧИВАЕМ
                AnimateCollapse(card, detailPanel, arrowLabel);
            }
            else
            {
                // РАСКРЫВАЕМ - загружаем данные
                var data = DBConnection.Instance.ExecuteQuery(query);
                if (data == null || data.Rows.Count == 0) return;

                // Создаём список
                detailPanel.Controls.Clear();
                var list = CreateModernList(data, columns);
                detailPanel.Controls.Add(list);
                detailPanel.Visible = true;

                // Анимация раскрытия
                AnimateExpand(card, detailPanel, arrowLabel, data.Rows.Count);
            }
        }

        // ✅ СОЗДАНИЕ СОВРЕМЕННОГО СПИСКА
        private FlowLayoutPanel CreateModernList(DataTable data, string[] columns)
        {
            var list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(0),
                BackColor = Color.Transparent,
                MinimumSize = new Size(220, 0)
            };

            int itemIndex = 0;
            foreach (DataRow row in data.Rows)
            {
                var item = new Panel
                {
                    Size = new Size(190, 40),
                    Margin = new Padding(0),
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };

                var label = new Label
                {
                    Text = string.Join(" — ", columns.Select(col => row[col]?.ToString())),
                    AutoSize = false,
                    Size = new Size(190, 40),
                    Font = new System.Drawing.Font("Segoe UI", 9),
                    BackColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0),
                    Dock = DockStyle.Fill
                };

                // Hover эффект
                item.MouseEnter += (s, e) =>
                {
                    item.BackColor = Color.FromArgb(245, 245, 245);
                    label.BackColor = Color.FromArgb(245, 245, 245);
                };
                item.MouseLeave += (s, e) =>
                {
                    item.BackColor = Color.White;
                    label.BackColor = Color.White;
                };

                // Разделительная линия
                var separator = new Panel
                {
                    Size = new Size(190, 1),
                    BackColor = Color.FromArgb(230, 230, 230),
                    Dock = DockStyle.Bottom
                };

                item.Controls.Add(label);
                item.Controls.Add(separator);
                list.Controls.Add(item);

                itemIndex++;
                if (itemIndex >= 10) break; // Максимум 10 элементов
            }

            return list;
        }

        // ✅ АНИМАЦИЯ РАСКРЫТИЯ
        private void AnimateExpand(Panel card, Panel detailPanel, Label arrowLabel, int rowCount)
        {
            int targetHeight = Math.Min(rowCount * 40 + 10, 400); // Максимум 400px

            var timer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60 FPS
            int currentHeight = 0;
            double step = 0;

            timer.Tick += (s, e) =>
            {
                step += 0.15;
                // Easing функция (ease-out-cubic)
                double progress = 1 - Math.Pow(1 - Math.Min(step, 1), 3);

                currentHeight = (int)(targetHeight * progress);
                detailPanel.Height = currentHeight;
                card.Height = 140 + currentHeight;

                arrowLabel.Text = "▲";

                if (step >= 1)
                {
                    detailPanel.Height = targetHeight;
                    card.Height = 140 + targetHeight;
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        // ✅ АНИМАЦИЯ СВORAЧИВАНИЯ
        private void AnimateCollapse(Panel card, Panel detailPanel, Label arrowLabel)
        {
            var timer = new System.Windows.Forms.Timer { Interval = 16 };
            int startHeight = detailPanel.Height;
            double step = 0;

            timer.Tick += (s, e) =>
            {
                step += 0.15;
                // Easing функция (ease-in-cubic)
                double progress = Math.Pow(Math.Min(step, 1), 3);

                int newHeight = startHeight - (int)(startHeight * progress);
                detailPanel.Height = newHeight;
                card.Height = 140 + newHeight;

                if (step >= 1)
                {
                    detailPanel.Height = 0;
                    detailPanel.Visible = false;
                    card.Height = 140;
                    arrowLabel.Text = "▼";
                    timer.Stop();
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        private string GetSuffix(string title)
        {
            return title switch
            {
                "🎓 Всего студентов" => "студент(ов)",
                "👨‍ Тренеров" => "тренер(ов)",
                "⚽ Секций" => "секций",
                "📋 Посещений (мес)" => "посещений",
                _ => ""
            };
        }

        private class CardData
        {
            public string Title { get; set; }
            public string Query { get; set; }
            public string[] Columns { get; set; }
            public Color Color { get; set; }
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

            var lblType = new Label { Text = "Тип аналитики:", Location = new Point(15, 15), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold) };
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
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10)
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
            var lblChart1 = new Label { Text = "📊", Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(30, 10), AutoSize = true };
            var chart1 = new Chart { Name = "chart1", Dock = DockStyle.Fill, Location = new Point(0, 30) };
            chart1Panel.Controls.AddRange(new Control[] { lblChart1, chart1 });

            // График 2
            var chart2Panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            var lblChart2 = new Label { Text = "📈", Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(30, 10), AutoSize = true };
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
                    BackColor = Color.FromArgb(0, 86, 179),
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold)
                },
                RowHeadersVisible = false
            };
            StyleDataGridView(dgv);

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
            chart.Titles.Add(new Title(title, Docking.Top,
                new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold),
                Color.FromArgb(0, 61, 130)));

            chart.Series.Clear();
            chart.ChartAreas.Clear();

            var chartArea = new ChartArea("MainArea");
            chartArea.Position.X = 5;
            chartArea.Position.Y = 15;
            chartArea.Position.Width = 90;
            chartArea.Position.Height = 70;

            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.IsLabelAutoFit = true;

            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            chartArea.AxisY.Minimum = 0;

            chart.ChartAreas.Add(chartArea);

            chart.Legends.Clear();
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new System.Drawing.Font("Segoe UI", 9),
                IsTextAutoFit = true
            };
            chart.Legends.Add(legend);

            var colors = new[] {
        Color.FromArgb(0, 86, 179),
        Color.FromArgb(40, 167, 69),
        Color.FromArgb(255, 193, 7),
        Color.FromArgb(220, 53, 69)
    };

            for (int i = 0; i < yAxes.Length; i++)
            {
                var series = new Series(yAxes[i])
                {
                    ChartType = SeriesChartType.Column,
                    Color = colors[i % colors.Length],
                    Legend = "MainLegend",
                    IsValueShownAsLabel = false,
                    Font = new System.Drawing.Font("Segoe UI", 7)
                };

                foreach (DataRow row in data.Rows)
                {
                    series.Points.AddXY(row[xAxis], row[yAxes[i]]);
                }

                chart.Series.Add(series);
            }
        }

        private void SetupPieChartFin(Chart chart, DataTable data, string labelColumn, string valueColumn, string title, bool showLegend = false)
        {
            if (chart == null || data == null || data.Rows.Count == 0) return;

            chart.Titles.Clear();
            chart.Titles.Add(new Title(title, Docking.Top,
                new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold),
                Color.FromArgb(0, 61, 130)));

            chart.Series.Clear();
            chart.ChartAreas.Clear();

            var chartArea = new ChartArea("MainArea");
            chartArea.Position.X = 10;
            chartArea.Position.Y = 15;
            chartArea.Position.Width = 80;
            chartArea.Position.Height = 80;
            chart.ChartAreas.Add(chartArea);

            chart.Legends.Clear();

            var series = new Series("Data")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                Label = "#PERCENT{P1}",
                Font = new System.Drawing.Font("Segoe UI", 8)
            };

            // ✅ ЦВЕТА: топ-5 разные, остальные - серый
            var colors = new[] {
        Color.FromArgb(0, 86, 179),      // Синий
        Color.FromArgb(40, 167, 69),      // Зелёный
        Color.FromArgb(255, 193, 7),      // Жёлтый
        Color.FromArgb(220, 53, 69),      // Красный
        Color.FromArgb(108, 117, 125),    // Серый (5-й)
        Color.FromArgb(200, 200, 200)     // Светло-серый (Остальные)
    };

            int colorIndex = 0;
            foreach (DataRow row in data.Rows)
            {
                var pointIndex = series.Points.AddXY(row[labelColumn], row[valueColumn]);

                // ✅ Если "Остальные" - последний цвет, иначе по порядку
                if (row[labelColumn].ToString() == "Остальные")
                    series.Points[pointIndex].Color = colors[5];
                else
                    series.Points[pointIndex].Color = colors[colorIndex % 5];

                series.Points[pointIndex].AxisLabel = row[labelColumn].ToString();
                colorIndex++;
            }

            chart.Series.Add(series);
        }
        private void SetupPieChart(Chart chart, DataTable data, string labelColumn, string valueColumn, string title)
        {
            if (chart == null || data == null || data.Rows.Count == 0) return;

            chart.Titles.Clear();
            chart.Titles.Add(new Title(title, Docking.Top,
                new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold),
                Color.FromArgb(0, 61, 130)));

            chart.Series.Clear();
            chart.ChartAreas.Clear();

            var chartArea = new ChartArea("MainArea");
            chartArea.Position.X = 5;
            chartArea.Position.Y = 15;
            chartArea.Position.Width = 55;  
            chartArea.Position.Height = 80;
            chart.ChartAreas.Add(chartArea);

            chart.Legends.Clear();
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Right,
                Alignment = StringAlignment.Far,
                Font = new System.Drawing.Font("Segoe UI", 8),
                IsTextAutoFit = true,
                LegendStyle = LegendStyle.Table
            };
            chart.Legends.Add(legend);

            var series = new Series("Data")
            {
                ChartType = SeriesChartType.Pie,
                Legend = "MainLegend",
                IsValueShownAsLabel = true,
                Font = new System.Drawing.Font("Segoe UI", 8)
            };

            var colors = new[] {
        Color.FromArgb(0, 86, 179),
        Color.FromArgb(40, 167, 69),
        Color.FromArgb(255, 193, 7),
        Color.FromArgb(220, 53, 69),
        Color.FromArgb(108, 117, 125),
        Color.FromArgb(23, 162, 184),
        Color.FromArgb(255, 140, 0),
        Color.FromArgb(142, 68, 173),
        Color.FromArgb(28, 186, 79),
        Color.FromArgb(217, 83, 25)
    };

            int colorIndex = 0;
            foreach (DataRow row in data.Rows)
            {
                double value = Convert.ToDouble(row[valueColumn]);
                string label = row[labelColumn].ToString();

                int pointIndex = series.Points.AddXY(label, value);
                DataPoint point = series.Points[pointIndex];

                point.Color = colors[colorIndex % colors.Length];

                point.LegendText = label;

                // #PERCENT — автоматический расчет процента от общей суммы
                point.Label = "#PERCENT{P0}";

                colorIndex++;
            }

            chart.Series.Add(series);
        }

        private void SetupLineChart(Chart chart, DataTable data, string xAxis, string[] yAxes, string title)
        {
            if (chart == null || data == null || data.Rows.Count == 0) return;

            chart.Titles.Clear();
            chart.Titles.Add(new Title(title, Docking.Top,
                new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold),
                Color.FromArgb(0, 61, 130)));

            chart.Series.Clear();
            chart.ChartAreas.Clear();

            var chartArea = new ChartArea("MainArea");
            chartArea.Position.X = 5;
            chartArea.Position.Y = 15;
            chartArea.Position.Width = 90;
            chartArea.Position.Height = 70;

            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.IsLabelAutoFit = true;

            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8);
            chartArea.AxisY.Minimum = 0;

            chart.ChartAreas.Add(chartArea);

            chart.Legends.Clear();
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new System.Drawing.Font("Segoe UI", 9),
                IsTextAutoFit = true
            };
            chart.Legends.Add(legend);

            var colors = new[] {
        Color.FromArgb(0, 86, 179),
        Color.FromArgb(40, 167, 69),
        Color.FromArgb(255, 193, 7),
        Color.FromArgb(220, 53, 69)
    };

            foreach (string yAxis in yAxes)
            {
                var series = new Series(yAxis)
                {
                    ChartType = SeriesChartType.Line,
                    Color = colors[yAxes.ToList().IndexOf(yAxis) % colors.Length],
                    Legend = "MainLegend",
                    IsValueShownAsLabel = false,
                    Font = new System.Drawing.Font("Segoe UI", 7),
                    BorderWidth = 2
                };

                foreach (DataRow row in data.Rows)
                {
                    series.Points.AddXY(row[xAxis], row[yAxis]);
                }

                chart.Series.Add(series);
            }
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
                    cmb.SelectedValue = 0;  
                }
            }
            catch (Exception ex)
            {
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

                var newDt = new DataTable();
                newDt.Columns.Add("SectionID", typeof(int));
                newDt.Columns.Add("SectionName", typeof(string));

                newDt.Rows.Add(0, "Все секции");

                foreach (DataRow row in dt.Rows)
                {
                    newDt.Rows.Add(row["SectionID"], row["SectionName"]);
                }

                cmb.DataSource = newDt;
                cmb.DisplayMember = "SectionName";
                cmb.ValueMember = "SectionID";

                if (cmb.Items.Count > 0)
                {
                    cmb.SelectedValue = 0;  
                }
            }
            catch (Exception ex)
            {
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

                if (cmbSec.SelectedValue != null && cmbSec.SelectedValue != DBNull.Value)
                {
                    sectionID = Convert.ToInt32(cmbSec.SelectedValue);
                }

                if (cmbFaculty.SelectedValue != null && cmbFaculty.SelectedValue != DBNull.Value)
                {
                    facultyID = Convert.ToInt32(cmbFaculty.SelectedValue);
                }

                var dgv = contentPanel.Controls.Find("dataGridView", true).FirstOrDefault() as DataGridView;
                var chart1 = contentPanel.Controls.Find("chart1", true).FirstOrDefault() as Chart;
                var chart2 = contentPanel.Controls.Find("chart2", true).FirstOrDefault() as Chart;

                DataTable data = null;

                switch (analyticsType)
                {
                    case "📊 Посещаемость по секциям":
                        var secParams = new SqlParameter[]
                        {
                    new SqlParameter("@SectionID", sectionID == 0 ? (object)DBNull.Value : sectionID),
                    new SqlParameter("@FacultyID", facultyID == 0 ? (object)DBNull.Value : facultyID)
                        };

                        data = DBConnection.Instance.ExecuteQuery(@"
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
                    WHERE (@SectionID IS NULL OR sec.SectionID = @SectionID)
                    AND (@FacultyID IS NULL OR s.FacultyID = @FacultyID)
                    GROUP BY sec.SectionName, sp.SportName
                    ORDER BY [Студентов] DESC", secParams);

                        // График 1: Столбчатая диаграмма
                        SetupBarChart(chart1, data, "Секция", new[] { "Студентов", "Посещений" }, "Распределение по секциям");

                        // График 2: Круговая диаграмма
                        var pieData = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        CASE WHEN a.Status = 1 THEN 'Присутствовал' ELSE 'Отсутствовал' END AS [Статус],
                        COUNT(*) AS [Количество]
                    FROM Attendance a
                    JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                    JOIN Sections sec ON sc.SectionID = sec.SectionID
                    WHERE (@SectionID IS NULL OR sec.SectionID = @SectionID)
                    GROUP BY a.Status",
                            new SqlParameter[] { new SqlParameter("@SectionID", sectionID == 0 ? (object)DBNull.Value : sectionID) });

                        SetupPieChart(chart2, pieData, "Статус", "Количество", "Распределение посещений");
                        break;

                    case "👥 Активность студентов":
                        var studParams = new SqlParameter[]
                        {
                    new SqlParameter("@FacultyID", facultyID == 0 ? (object)DBNull.Value : facultyID)
                        };

                        data = DBConnection.Instance.ExecuteQuery(@"
                    SELECT TOP 10
                        s.StudentCardNumber AS [Билет],
                        s.LastName + ' ' + s.FirstName AS [Студент],
                        s.GroupName AS [Группа],
                        f.FacultyName AS [Факультет],
                        COUNT(a.AttendanceID) AS [Посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                        CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Активность %]
                    FROM Students s
                    LEFT JOIN Faculties f ON s.FacultyID = f.FacultyID
                    LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                    WHERE (@FacultyID IS NULL OR s.FacultyID = @FacultyID)
                    GROUP BY s.StudentCardNumber, s.LastName, s.FirstName, s.GroupName, f.FacultyName
                    ORDER BY [Посещений] DESC", studParams);

                        SetupBarChart(chart1, data, "Студент", new[] { "Посещений" }, "Топ студентов");

                        var groupData = DBConnection.Instance.ExecuteQuery(@"
                    SELECT GroupName AS [Группа], COUNT(*) AS [Студентов]
                    FROM Students
                    WHERE (@FacultyID IS NULL OR FacultyID = @FacultyID)
                    GROUP BY GroupName
                    ORDER BY [Студентов] DESC",
                            new SqlParameter[] { new SqlParameter("@FacultyID", facultyID == 0 ? (object)DBNull.Value : facultyID) });

                        SetupBarChart(chart2, groupData, "Группа", new[] { "Студентов" }, "Распределение по группам");
                        break;

                    case "💰 Финансовая аналитика":
                        data = DBConnection.Instance.ExecuteQuery(@"
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
        ORDER BY [Заполненность %] DESC");  

                        SetupBarChart(chart1, data, "Секция", new[] { "Доход", "Студентов" }, "Финансы по секциям");

                        var fillData = DBConnection.Instance.ExecuteQuery(@"
        WITH RankedSections AS (
            SELECT 
                sec.SectionName,
                CAST(COUNT(DISTINCT ss.StudentCardNumber) * 100.0 / NULLIF(sec.MaxStudents, 0) AS DECIMAL(5,1)) AS [Заполненность %],
                ROW_NUMBER() OVER (ORDER BY CAST(COUNT(DISTINCT ss.StudentCardNumber) * 100.0 / NULLIF(sec.MaxStudents, 0) AS DECIMAL(5,1)) DESC) AS RowNum
            FROM Sections sec
            LEFT JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.IsActive = 1
            WHERE sec.MaxStudents > 0
            GROUP BY sec.SectionName, sec.MaxStudents
        )
        SELECT 
            SectionName,
            [Заполненность %]
        FROM RankedSections
        WHERE RowNum <= 5
        
        UNION ALL
        
        SELECT 
            'Остальные' AS SectionName,
            SUM([Заполненность %]) AS [Заполненность %]
        FROM RankedSections
        WHERE RowNum > 5");

                        SetupPieChartFin(chart2, fillData, "SectionName", "Заполненность %", "Заполненность секций (Топ-5 + Остальные)", false);

                        if (dgv != null && data != null)
                        {
                            dgv.DataSource = null;
                            dgv.Columns.Clear();
                            dgv.DataSource = data;

                            var colorColumn = new DataGridViewTextBoxColumn
                            {
                                Name = "ColorMarker",
                                HeaderText = "",
                                Width = 40
                            };
                            dgv.Columns.Insert(0, colorColumn);

                            dgv.Columns["ColorMarker"].DisplayIndex = 0;
                            dgv.Columns["Секция"].DisplayIndex = 1;
                            dgv.Columns["Цена"].DisplayIndex = 2;
                            dgv.Columns["Студентов"].DisplayIndex = 3;
                            dgv.Columns["Доход"].DisplayIndex = 4;
                            dgv.Columns["Макс. мест"].DisplayIndex = 5;
                            dgv.Columns["Заполненность %"].DisplayIndex = 6;

                            var top5Colors = new[] {
                                Color.FromArgb(0, 86, 179),
                                Color.FromArgb(40, 167, 69),
                                Color.FromArgb(255, 193, 7),
                                Color.FromArgb(220, 53, 69),
                                Color.FromArgb(108, 117, 125)     
        };
                            var othersColor = Color.FromArgb(200, 200, 200);  

                            for (int i = 0; i < dgv.Rows.Count; i++)
                            {
                                dgv.Rows[i].Cells["ColorMarker"].Value = "■";

                                // ✅ Топ-5 получают свои цвета, остальные - один серый
                                if (i < 5)
                                {
                                    dgv.Rows[i].Cells["ColorMarker"].Style.BackColor = top5Colors[i];
                                    dgv.Rows[i].Cells["ColorMarker"].Style.ForeColor = top5Colors[i];
                                }
                                else
                                {
                                    dgv.Rows[i].Cells["ColorMarker"].Style.BackColor = othersColor;
                                    dgv.Rows[i].Cells["ColorMarker"].Style.ForeColor = othersColor;
                                }

                                dgv.Rows[i].Cells["ColorMarker"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                                dgv.Rows[i].Cells["ColorMarker"].Style.Font = new System.Drawing.Font("Segoe UI", 14, FontStyle.Bold);
                                dgv.Rows[i].Cells["ColorMarker"].ReadOnly = true;
                            }

                            StyleDataGridView(dgv);
                            foreach (DataGridViewColumn column in dgv.Columns)
                            {
                                column.SortMode = DataGridViewColumnSortMode.NotSortable; 
                            }
                        }
                        break;

                    case "📅 Динамика посещаемости":
                        data = DBConnection.Instance.ExecuteQuery(@"
                    SELECT 
                        FORMAT(a.VisitDate, 'yyyy-MM') AS [Месяц],
                        COUNT(a.AttendanceID) AS [Посещений],
                        SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                        SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал]
                    FROM Attendance a
                    JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                    JOIN Sections sec ON sc.SectionID = sec.SectionID
                    WHERE (@SectionID IS NULL OR sec.SectionID = @SectionID)
                    GROUP BY FORMAT(a.VisitDate, 'yyyy-MM')
                    ORDER BY [Месяц]",
                            new SqlParameter[] { new SqlParameter("@SectionID", sectionID == 0 ? (object)DBNull.Value : sectionID) });

                        SetupLineChart(chart1, data, "Месяц", new[] { "Посещений", "Присутствовал", "Отсутствовал" }, "Динамика по месяцам");
                        SetupLineChart(chart2, data, "Месяц", new[] { "Посещений" }, "Тренд посещаемости");
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

        // ======================================== ОТЧЁТЫ
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
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold)
            };

            var cmbReportType = new ComboBox
            {
                Name = "cmbReportType",
                Location = new Point(15, 38),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new System.Drawing.Font("Segoe UI", 10)
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
                    BackColor = Color.FromArgb(0, 86, 179),
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold)
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
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold)
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
                Font = new System.Drawing.Font("Segoe UI", 10),
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

        // ======================================== ОБНОВЛЕНИЕ ФИЛЬТРОВ ПО ТИПУ ОТЧЁТА
        private void UpdateReportFilters(ComboBox cmbType, Panel filterPanel)
        {
            filterPanel.Controls.Clear();
            string reportType = cmbType.SelectedItem?.ToString() ?? "";

            var controls = new List<Control>();
            int y = 20;

            // Общие фильтры: период
            controls.Add(new Label { Text = "📅 Период:", Location = new Point(15, y), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold) });
            var dtpStart = new DateTimePicker { Name = "dtpStart", Location = new Point(90, y - 3), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };
            var lblTo = new Label { Text = "по", Location = new Point(230, y), AutoSize = true };
            var dtpEnd = new DateTimePicker { Name = "dtpEnd", Location = new Point(255, y - 3), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            controls.AddRange(new Control[] { dtpStart, lblTo, dtpEnd });
            y += 35;

            // Фильтры по типу
            switch (reportType)
            {
                case "🏛️ Для ректората":
                    controls.Add(new Label
                    {
                        Text = "ℹ️ Отчёт формируется по всем факультетам",
                        Location = new Point(15, y),
                        AutoSize = true,
                        ForeColor = Color.Gray,
                        Font = new System.Drawing.Font("Segoe UI", 9)
                    });
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

        // ======================================== ГЕНЕРАЦИЯ ОТЧЁТА
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

        // ======================================== ПОЛУЧЕНИЕ ДАННЫХ ПО ТИПУ ОТЧЁТА
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
            CAST(ISNULL(SUM(ac.VisitCount), 0.0) / NULLIF(COUNT(DISTINCT sec.SectionID), 0) AS DECIMAL(10,1)) AS [Ср. занятий/секцию],
            ISNULL(SUM(sec.PricePerMonth * sc.Count), 0) AS [Доход секций, BYN]
        FROM Trainers t
        LEFT JOIN Sections sec ON t.DocumentNumber = sec.TrainerID
        LEFT JOIN StudentCount sc ON sec.SectionID = sc.SectionID
        LEFT JOIN AttendanceCount ac ON sec.SectionID = ac.SectionID
        LEFT JOIN Attendance a ON sec.SectionID = (
            SELECT SectionID FROM Schedule WHERE ScheduleID = a.ScheduleID
        )
        WHERE a.VisitDate BETWEEN @StartDate AND @EndDate
        GROUP BY t.LastName, t.FirstName, t.Qualification, t.Specialization
        ORDER BY [Доход секций, BYN] DESC";
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
            sec.PricePerMonth AS [Цена/мес, BYN],
            COUNT(DISTINCT ss.StudentCardNumber) AS [Активных студентов],
            sec.PricePerMonth * COUNT(DISTINCT ss.StudentCardNumber) AS [Доход/мес, BYN],
            sec.PricePerMonth * COUNT(DISTINCT ss.StudentCardNumber) * 6 AS [Доход/семестр, BYN],
            sec.MaxStudents AS [Макс. мест],
            CASE WHEN sec.MaxStudents > 0 
                 THEN CAST(COUNT(DISTINCT ss.StudentCardNumber) * 100.0 / sec.MaxStudents AS DECIMAL(5,1))
                 ELSE 0 END AS [Заполненность %]
        FROM Sections sec
        JOIN Sports sp ON sec.SportID = sp.SportID
        JOIN Trainers t ON sec.TrainerID = t.DocumentNumber
        LEFT JOIN StudentSections ss ON sec.SectionID = ss.SectionID AND ss.IsActive = 1
        WHERE (@SportID IS NULL OR @SportID = 0 OR sp.SportID = @SportID)
        GROUP BY sec.SectionName, sp.SportName, t.LastName, t.FirstName, 
                 sec.PricePerMonth, sec.MaxStudents
        ORDER BY [Доход/семестр, BYN] DESC";
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
                    return null;  
            }

            return DBConnection.Instance.ExecuteQuery(query, parameters.ToArray());
        }

        // ======================================== СТИЛИЗАЦИЯ ТАБЛИЦЫ БЕЗ ВЫДЕЛЕНИЯ
        private void StyleDataGridView(DataGridView dgv)
        {
            // 1. Убираем стандартные цвета выделения (делаем их как у обычных строк)
            dgv.DefaultCellStyle.SelectionBackColor = Color.White;
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 61, 130);

            // 2. Для чередующихся строк тоже настраиваем цвет выделения, 
            // чтобы при клике на серую строку она не становилась белой
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(250, 250, 250);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 61, 130);

            // 3. Базовые настройки стиля
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(0, 61, 130);
            dgv.RowHeadersVisible = false;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.GridColor = Color.FromArgb(220, 220, 220);
            dgv.MultiSelect = false;
            dgv.EnableHeadersVisualStyles = false;

            // 4. ГЛАВНОЕ: чтобы не было синей рамки фокуса вокруг ячейки
            dgv.RowTemplate.Height = 30; // опционально, для красоты
            dgv.StandardTab = true;

            // Подписываемся на событие сброса выделения
            dgv.SelectionChanged += (s, e) => dgv.ClearSelection();
        }
        
        // ======================================== ПОДСВЕТКА ПРОБЛЕМНЫХ СТУДЕНТОВ
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
                    row.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold);
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
        // ======================================== ЭКСПОРТ ИЗ ТАБЛИЦЫ
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

            // Вызываем оригинальный ExportReport с 4 аргументами
            ExportReport(data, reportType, startDate, endDate);
        }
        

        // Вспомогательный метод (нужен для работы с диапазонами Excel)
        private string GetExcelColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modifier;
            while (dividend > 0)
            {
                modifier = (dividend - 1) % 26;
                columnName = (char)(65 + modifier) + columnName;
                dividend = (int)((dividend - modifier) / 26);
            }
            return columnName;
        }

        // ======================================== ВСПОМОГАТЕЛЬНЫЙ МЕТОД ПОИСКА КОНТРОЛА
        private T GetControl<T>(Panel panel, string name) where T : Control
        {
            var controls = panel.Controls.Find(name, true);
            return controls.FirstOrDefault() as T;
        }

        // ======================================== ЭКСПОРТ 
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
                            ExportToCSV(data, filePath); 
                        }
                        else if (ext == ".xlsx")
                        {
                            ExportToExcelInterop(data, filePath, reportType);
                        }
                        else if (ext == ".pdf")
                        {
                            ExportToPDF(data, filePath, reportType);
                        }

                        MessageBox.Show($"✅ Отчёт сохранён:\n{filePath}", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка экспорта: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetFriendlyReportName(string reportType)
        {
            // Очищаем от эмодзи для сравнения
            string type = reportType.Replace("🏛️", "").Replace("🎓", "").Replace("👨‍🏫", "").Replace("💰", "").Replace("📊", "").Replace("⚙️", "").Trim();

            return type switch
            {
                "Для ректората" => "Отчет для ректората",
                "Для факультета" => "Отчет по факультету",
                "Для кафедры (нагрузка)" => "Отчет по нагрузке кафедры",
                "Финансовый" => "Отчет по финансам",
                "По посещаемости" => "Отчет по посещаемости студентов",
                "Настраиваемый" => "Пользовательский отчет",
                _ => $"Отчет: {type}" // На случай, если появится новый тип
            };
        }

        // ======================================== ЭКСПОРТ В CSV
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

        //======================================================= ЭКСПОРТ В .XLXS
        private void ExportToExcelInterop(DataTable dt, string filePath, string reportType)
        {
            Excel.Application excelApp = new Excel.Application();
            if (excelApp == null)
            {
                MessageBox.Show("Excel не установлен!");
                return;
            }

            Excel.Workbook workbook = excelApp.Workbooks.Add();
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.ActiveSheet;
            worksheet.Name = "Отчёт";

            try
            {
                // === 1. СОЗДАНИЕ ШАПКИ ===
                string displayTitle = GetFriendlyReportName(reportType);

                // Заголовок (Строка 1)
                Excel.Range titleRange = worksheet.get_Range("A1", GetExcelColumnName(dt.Columns.Count) + "1");
                titleRange.Merge(); // Объединяем ячейки по ширине таблицы
                titleRange.Value = displayTitle.ToUpper();
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Информация об организации и дате (Строки 2-3)
                worksheet.Cells[2, 1] = "Организация: Полесский Государственный Университет";
                worksheet.Cells[3, 1] = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";

                Excel.Range infoRange = worksheet.get_Range("A2", "A3");
                infoRange.Font.Size = 11;
                infoRange.Font.Italic = true;

                // Смещение данных: заголовки таблицы теперь начнутся с 5-й строки
                int startRow = 5;

                // === 2. ЗАГОЛОВКИ ТАБЛИЦЫ ===
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    worksheet.Cells[startRow, i + 1] = dt.Columns[i].ColumnName;
                }

                // === 3. ПОДГОТОВКА И ВСТАВКА ДАННЫХ ===
                object[,] arr = new object[dt.Rows.Count, dt.Columns.Count];
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        arr[r, c] = dt.Rows[r][c];
                    }
                }

                Excel.Range startCell = (Excel.Range)worksheet.Cells[startRow + 1, 1];
                Excel.Range endCell = (Excel.Range)worksheet.Cells[startRow + dt.Rows.Count, dt.Columns.Count];
                Excel.Range writeRange = worksheet.get_Range(startCell, endCell);
                writeRange.Value = arr;

                // === 4. ОФОРМЛЕНИЕ ТАБЛИЦЫ ===
                // Стили для шапки таблицы
                Excel.Range tableHeaderRange = worksheet.get_Range(
                    GetExcelColumnName(1) + startRow,
                    GetExcelColumnName(dt.Columns.Count) + startRow
                );
                tableHeaderRange.Font.Bold = true;
                tableHeaderRange.Interior.Color = ColorTranslator.ToOle(Color.LightGray);
                tableHeaderRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                // Сетка для данных
                writeRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                worksheet.Columns.AutoFit();

                // 5. Сохранение
                excelApp.DisplayAlerts = false; // Чтобы не спрашивал подтверждение при перезаписи
                workbook.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка внутри Excel: " + ex.Message);
            }
            finally
            {
                workbook.Close(false);
                excelApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            }
        }

        //======================================================= ЭКСПОРТ В PDF
        private void ExportToPDF(DataTable dt, string filePath, string reportType)
        {
            // 1. Создаем документ. Rotate() делает его альбомным.
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 20f, 20f, 30f, 50f);

            try
            {
                iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();

                // --- ШРИФТЫ (Объявляем их строго ВНУТРИ метода, чтобы не было ошибки CS0103) ---
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "Arial.ttf");
                iTextSharp.text.pdf.BaseFont bf = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);

                iTextSharp.text.Font titleFont = new iTextSharp.text.Font(bf, 16, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font boldFont = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.BOLD); // Вот он!
                iTextSharp.text.Font microFont = new iTextSharp.text.Font(bf, 7, iTextSharp.text.Font.NORMAL);

                // === 1. ШАПКА ===
                string displayTitle = GetFriendlyReportName(reportType);
                var title = new iTextSharp.text.Paragraph(displayTitle.ToUpper(), titleFont);
                title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                document.Add(title);

                var info = new iTextSharp.text.Paragraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                                                         $"Организация: Полесский Государственный Университет\n ", font);
                document.Add(info);

                // === 2. ТАБЛИЦА С ДАННЫМИ ===
                // Явно приводим количество колонок к int, чтобы избежать CS1503
                int columnCount = dt.Columns.Count;
                iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(columnCount);
                table.WidthPercentage = 100f;
                table.SpacingBefore = 15f;

                // Заголовки таблицы
                foreach (DataColumn column in dt.Columns)
                {
                    // Используем именно iTextSharp.text.Phrase
                    var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(column.ColumnName, boldFont));
                    cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
                    cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    cell.PaddingBottom = 5f;
                    table.AddCell(cell);
                }

                // Данные таблицы
                foreach (DataRow row in dt.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        table.AddCell(new iTextSharp.text.Phrase(item.ToString(), font));
                    }
                }

                document.Add(table);

                // === 3. ФУТЕР (ПОДПИСЬ) ===
                document.Add(new iTextSharp.text.Paragraph("\n\n"));
                iTextSharp.text.pdf.PdfPTable footerTable = new iTextSharp.text.pdf.PdfPTable(3);
                footerTable.WidthPercentage = 100f;
                footerTable.SetWidths(new float[] { 20f, 30f, 50f });

                // Ряд 1: Линии
                footerTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Менеджер:", font)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                footerTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(" ", font)) { Border = iTextSharp.text.Rectangle.BOTTOM_BORDER });
                footerTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(" / ", font)) { Border = iTextSharp.text.Rectangle.BOTTOM_BORDER });

                // Ряд 2: Подстрочники
                footerTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("", microFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                footerTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("(Подпись)", microFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER });
                footerTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("(Фамилия И.О.)", microFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER });

                document.Add(footerTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании PDF: " + ex.Message);
            }
            finally
            {
                document.Close();
            }
        }


        // ======================================== ПО ФАКУЛЬТЕТАМ
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

            var lblFaculty = new Label { Text = "Факультет:", Location = new Point(15, 25), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold) };
            var cmbFaculty = new ComboBox { Name = "cmbFaculty", Location = new Point(90, 22), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadFacultiesToComboBox(cmbFaculty);

            var btnShow = new Button
            {
                Name = "btnShow",
                Text = "📊 Показать",
                Location = new Point(410, 20),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10)
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
                    BackColor = Color.FromArgb(0, 86, 179),
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold)
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
            try
            {
                int facultyID = 0;

                // ✅ Проверяем, выбран ли конкретный факультет или "Все факультеты"
                if (cmbFaculty.SelectedValue != null && cmbFaculty.SelectedValue != DBNull.Value)
                {
                    facultyID = cmbFaculty.SelectedValue is DataRowView rowView
                        ? Convert.ToInt32(rowView["FacultyID"])
                        : Convert.ToInt32(cmbFaculty.SelectedValue);
                }

                var dgv = contentPanel.Controls.Find("dataGridView", true).FirstOrDefault() as DataGridView;
                if (dgv == null) return;

                // ✅ ИЗМЕНЁННЫЙ ЗАПРОС: работает и с конкретным факультетом, и со всеми
                string query = "";
                SqlParameter[] parameters = null;

                if (facultyID == 0)
                {
                    // ✅ ВСЕ ФАКУЛЬТЕТЫ
                    query = @"
                SELECT 
                    f.FacultyName AS [Факультет],
                    s.GroupName AS [Группа],
                    COUNT(DISTINCT s.StudentCardNumber) AS [Студентов],
                    COUNT(DISTINCT a.AttendanceID) AS [Посещений],
                    SUM(CASE WHEN a.Status = 1 THEN 1 ELSE 0 END) AS [Присутствовал],
                    SUM(CASE WHEN a.Status = 0 THEN 1 ELSE 0 END) AS [Отсутствовал],
                    CAST(SUM(CASE WHEN a.Status = 1 THEN 100.0 ELSE 0.0 END) / 
                         NULLIF(COUNT(*), 0) AS DECIMAL(5,1)) AS [Посещаемость %]
                FROM Faculties f
                LEFT JOIN Students s ON f.FacultyID = s.FacultyID
                LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber
                GROUP BY f.FacultyName, s.GroupName
                ORDER BY f.FacultyName, [Посещаемость %] DESC";

                    parameters = new SqlParameter[] { };
                }
                else
                {
                    // ✅ КОНКРЕТНЫЙ ФАКУЛЬТЕТ
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
                GROUP BY s.GroupName
                ORDER BY [Посещаемость %] DESC";

                    parameters = new SqlParameter[] {
                new SqlParameter("@FacultyID", facultyID)
            };
                }

                dgv.DataSource = DBConnection.Instance.ExecuteQuery(query, parameters);

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

        // ======================================== ПО ГРУППАМ
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

            var lblGroup = new Label { Text = "Группа:", Location = new Point(15, 25), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold) };
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
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10)
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
                    BackColor = Color.FromArgb(0, 86, 179),
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold)
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

        // ======================================== ПО СЕКЦИЯМ
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

            var lblSection = new Label { Text = "Секция:", Location = new Point(15, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold) };
            var cmbSection = new ComboBox { Name = "cmbSection", Location = new Point(80, 17), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            LoadSectionsToComboBox(cmbSection);

            var lblPeriod = new Label { Text = "Период:", Location = new Point(350, 20), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold) };
            var dtpStart = new DateTimePicker { Name = "dtpStart", Location = new Point(410, 17), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };
            var lblTo = new Label { Text = "по", Location = new Point(550, 20), AutoSize = true };
            var dtpEnd = new DateTimePicker { Name = "dtpEnd", Location = new Point(575, 17), Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Now };

            var btnShow = new Button
            {
                Name = "btnShow",
                Text = "📊 Показать",
                Location = new Point(720, 15),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(0, 86, 179),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10)
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
                    BackColor = Color.FromArgb(0, 86, 179),
                    ForeColor = Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold)
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
            try
            {
                int sectionID = 0;

                // ✅ Проверяем, выбрана ли конкретная секция или "Все секции"
                if (cmbSection.SelectedValue != null && cmbSection.SelectedValue != DBNull.Value)
                {
                    sectionID = cmbSection.SelectedValue is DataRowView rowView
                        ? Convert.ToInt32(rowView["SectionID"])
                        : Convert.ToInt32(cmbSection.SelectedValue);
                }

                var dgv = contentPanel.Controls.Find("dataGridView", true).FirstOrDefault() as DataGridView;
                if (dgv == null) return;

                // ✅ ИЗМЕНЁННЫЙ ЗАПРОС: работает и с конкретной секцией, и со всеми
                string query = "";
                SqlParameter[] parameters = null;

                if (sectionID == 0)
                {
                    // ✅ ВСЕ СЕКЦИИ
                    query = @"
                SELECT 
                    sec.SectionName AS [Секция],
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
                LEFT JOIN StudentSections ss ON s.StudentCardNumber = ss.StudentCardNumber AND ss.IsActive = 1
                LEFT JOIN Sections sec ON ss.SectionID = sec.SectionID
                LEFT JOIN Schedule sc ON sec.SectionID = sc.SectionID
                LEFT JOIN Attendance a ON s.StudentCardNumber = a.StudentCardNumber AND sc.ScheduleID = a.ScheduleID
                WHERE (a.VisitDate IS NULL OR a.VisitDate BETWEEN @StartDate AND @EndDate)
                GROUP BY sec.SectionName, s.StudentCardNumber, s.LastName, s.FirstName, f.FacultyName, s.GroupName
                ORDER BY sec.SectionName, [Посещаемость %] DESC";

                    parameters = new SqlParameter[] {
                new SqlParameter("@StartDate", dtpStart.Value.Date),
                new SqlParameter("@EndDate", dtpEnd.Value.Date)
            };
                }
                else
                {
                    // ✅ КОНКРЕТНАЯ СЕКЦИЯ
                    query = @"
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
                ORDER BY [Посещаемость %] DESC";

                    parameters = new SqlParameter[] {
                new SqlParameter("@SectionID", sectionID),
                new SqlParameter("@StartDate", dtpStart.Value.Date),
                new SqlParameter("@EndDate", dtpEnd.Value.Date)
            };
                }

                dgv.DataSource = DBConnection.Instance.ExecuteQuery(query, parameters);

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

        // ======================================== ЗАЯВКИ
        private void LoadStudentRequests()
        {
            headerLabel.Text = "🎓 Заявки студентов";
            contentPanel.Controls.Clear();

            // Заголовок с информацией
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(30, 15, 30, 15)
            };

            var lblTitle = new Label
            {
                Text = "🎓 Заявки студентов на запись в секции",
                Font = new System.Drawing.Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = GreenMain,
                Location = new Point(0, 10),
                AutoSize = true
            };

            var lblInfo = new Label
            {
                Text = "Ожидают обработки: 0 заявок",
                Name = "lblInfo",
                Font = new System.Drawing.Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(0, 45),
                AutoSize = true
            };

            var btnRefresh = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(850, 20),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = BlueAccent,
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadRequestsData();

            titlePanel.Controls.AddRange(new Control[] { lblTitle, lblInfo, btnRefresh });

            // Таблица заявок
            var dgvRequests = new DataGridView
            {
                Name = "dgvRequests",
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
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold)
                },
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvRequests.CellFormatting += (s, e) =>
            {
                if (dgvRequests.Columns[e.ColumnIndex].HeaderText == "Тип")
                {
                    if (e.Value?.ToString() == "📝 Вступление")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(220, 255, 220);
                        e.CellStyle.Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold);
                    }
                    else if (e.Value?.ToString() == "🚪 Выход")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(255, 240, 220);
                        e.CellStyle.Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Bold);
                    }
                }
            };

            // Панель кнопок
            var actionPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(30, 15, 30, 15)
            };

            var btnApprove = new Button
            {
                Name = "btnApprove",
                Text = "✅ Одобрить",
                Location = new Point(30, 15),
                Size = new Size(140, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.Click += BtnApprove_Click;

            var btnReject = new Button
            {
                Name = "btnReject",
                Text = "❌ Отклонить",
                Location = new Point(180, 15),
                Size = new Size(140, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnReject.FlatAppearance.BorderSize = 0;
            btnReject.Click += BtnReject_Click;

            // Включение кнопок при выборе
            dgvRequests.SelectionChanged += (s, e) =>
            {
                bool hasSelection = dgvRequests.SelectedRows.Count > 0;
                btnApprove.Enabled = hasSelection;
                btnReject.Enabled = hasSelection;
            };

            actionPanel.Controls.AddRange(new Control[] { btnApprove, btnReject });

            // Добавляем всё
            contentPanel.Controls.Add(dgvRequests);
            contentPanel.Controls.Add(actionPanel);
            contentPanel.Controls.Add(titlePanel);

            // Загружаем данные
            LoadRequestsData();
        }

        private void LoadRequestsData()
        {
            try
            {
                var dgv = contentPanel.Controls.Find("dgvRequests", true).FirstOrDefault() as DataGridView;
                var lblInfo = contentPanel.Controls.Find("lblInfo", true).FirstOrDefault() as Label;

                if (dgv == null) return;

                var data = DBConnection.Instance.ExecuteQuery(@"
            SELECT 
                r.RequestID,
                r.StudentCardNumber AS [Билет],
                s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.MiddleName, '') AS [Студент],
                s.GroupName AS [Группа],
                f.FacultyName AS [Факультет],
                sec.SectionName AS [Секция],
                sp.SportName AS [Вид спорта],
                CASE WHEN r.RequestType = 'Join' THEN '📝 Вступление' ELSE '🚪 Выход' END AS [Тип],
                r.RequestDate AS [Дата заявки]
            FROM StudentSectionRequests r
            JOIN Students s ON r.StudentCardNumber = s.StudentCardNumber
            JOIN Faculties f ON s.FacultyID = f.FacultyID
            JOIN Sections sec ON r.SectionID = sec.SectionID
            JOIN Sports sp ON sec.SportID = sp.SportID
            WHERE r.Status = 'Pending'
            ORDER BY r.RequestDate DESC");

                dgv.DataSource = data;

                if (lblInfo != null)
                    lblInfo.Text = $"Ожидают обработки: {data.Rows.Count} заявок";

                dgv.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заявок: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApprove_Click(object sender, EventArgs e)
        {
            var dgv = contentPanel.Controls.Find("dgvRequests", true).FirstOrDefault() as DataGridView;
            if (dgv == null || dgv.SelectedRows.Count == 0) return;

            var requestId = Convert.ToInt32(dgv.SelectedRows[0].Cells["RequestID"].Value);
            var studentName = dgv.SelectedRows[0].Cells["Студент"].Value.ToString();
            var requestType = dgv.SelectedRows[0].Cells["Тип"].Value.ToString();

            if (MessageBox.Show($"Одобрить заявку?\n\nСтудент: {studentName}\nДействие: {requestType}",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                var managerId = User.CurrentUser?.UserId ?? 0;
                DBConnection.Instance.ProcessRequest(requestId, true, managerId);
                MessageBox.Show("✅ Заявка одобрена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRequestsData();
                LoadDashboard(); // Обновить счётчик
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReject_Click(object sender, EventArgs e)
        {
            var dgv = contentPanel.Controls.Find("dgvRequests", true).FirstOrDefault() as DataGridView;
            if (dgv == null || dgv.SelectedRows.Count == 0) return;

            var requestId = Convert.ToInt32(dgv.SelectedRows[0].Cells["RequestID"].Value);
            var studentName = dgv.SelectedRows[0].Cells["Студент"].Value.ToString();

            using (var form = new RejectReasonForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var managerId = User.CurrentUser?.UserId ?? 0;
                        DBConnection.Instance.ProcessRequest(requestId, false, managerId, form.Reason);
                        MessageBox.Show("❌ Заявка отклонена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadRequestsData();
                        LoadDashboard();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ======================================== НАСТРОЙКИ
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


            // 1. Насыщенный синий (Royal Blue)
            AddSettingCard(settingsPanel, "👥 Управление пользователями", "Добавление, редактирование и удаление пользователей",
                Color.FromArgb(65, 105, 225), () => new PolesSU_Sports.Management.SettingsForms.UserManagementForm().ShowDialog());

            // 2. Бирюзовый / Морская волна (Переходный цвет)
            AddSettingCard(settingsPanel, "📊 Настройка отчётов", "Шаблоны и параметры генерации отчётов",
                Color.FromArgb(46, 170, 160), () => new PolesSU_Sports.Management.SettingsForms.ReportSettingsForm().ShowDialog());

            // 3. Свежий зеленый (Medium Sea Green)
            AddSettingCard(settingsPanel, "🎓 Студенты в секциях", "Добавление и удаление студентов из секций",
                Color.FromArgb(60, 179, 113), () => ShowSectionStudentsManager());
            contentPanel.Controls.Add(settingsPanel);
        }

        // ✅ НОВЫЙ МЕТОД: Управление студентами в секциях
        private void ShowSectionStudentsManager()
        {
            headerLabel.Text = "🎓 Студенты в секциях";
            contentPanel.Controls.Clear();

            // Панель фильтров
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(30, 15, 30, 15)
            };

            var lblSection = new Label
            {
                Text = "Выберите секцию:",
                Location = new Point(0, 10),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = GreenMain
            };

            var cmbSection = new ComboBox
            {
                Name = "cmbSection",
                Location = new Point(0, 35),
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            LoadSectionsToComboBox(cmbSection);
            cmbSection.SelectedIndexChanged += (s, e) => LoadSectionStudentsData();

            var btnLoad = new Button
            {
                Text = "📋 Загрузить",
                Location = new Point(420, 33),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = BlueAccent,
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            btnLoad.FlatAppearance.BorderSize = 0;
            btnLoad.Click += (s, e) => LoadSectionStudentsData();

            filterPanel.Controls.AddRange(new Control[] { lblSection, cmbSection, btnLoad });

            // Таблица студентов
            var dgvStudents = new DataGridView
            {
                Name = "dgvStudents",
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
                    Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold)
                },
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Панель кнопок
            var actionPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(30, 15, 30, 15)
            };

            var btnRemove = new Button
            {
                Name = "btnRemove",
                Text = "🗑️ Удалить из секции",
                Location = new Point(30, 15),
                Size = new Size(200, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += BtnRemoveStudent_Click;

            // Включение кнопки при выборе
            dgvStudents.SelectionChanged += (s, e) =>
            {
                btnRemove.Enabled = dgvStudents.SelectedRows.Count > 0;
            };

            actionPanel.Controls.Add(btnRemove);

            // Добавляем всё
            contentPanel.Controls.Add(dgvStudents);
            contentPanel.Controls.Add(actionPanel);
            contentPanel.Controls.Add(filterPanel);

            // Автозагрузка при открытии
            if (cmbSection.SelectedValue != null && Convert.ToInt32(cmbSection.SelectedValue) > 0)
            {
                LoadSectionStudentsData();
            }
        }

        private void LoadSectionStudentsData()
        {
            var cmbSection = contentPanel.Controls.Find("cmbSection", true).FirstOrDefault() as ComboBox;
            var dgv = contentPanel.Controls.Find("dgvStudents", true).FirstOrDefault() as DataGridView;

            if (cmbSection == null || dgv == null) return;

            int sectionID = 0;
            if (cmbSection.SelectedValue != null && cmbSection.SelectedValue != DBNull.Value)
            {
                sectionID = Convert.ToInt32(cmbSection.SelectedValue);
            }

            if (sectionID == 0)
            {
                dgv.DataSource = null;
                return;
            }

            try
            {
                var data = DBConnection.Instance.GetSectionStudents(sectionID);
                dgv.DataSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRemoveStudent_Click(object sender, EventArgs e)
        {
            var dgv = contentPanel.Controls.Find("dgvStudents", true).FirstOrDefault() as DataGridView;
            var cmbSection = contentPanel.Controls.Find("cmbSection", true).FirstOrDefault() as ComboBox;

            if (dgv == null || dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите студента из таблицы для исключения", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Исключить студента из секции?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                var studentCard = dgv.SelectedRows[0].Cells["StudentCardNumber"].Value.ToString();
                int sectionID = Convert.ToInt32(cmbSection.SelectedValue);

                // мягкое удаление - IsActive = 0
                DBConnection.Instance.ExecuteCommand(
                    "UPDATE StudentSections SET IsActive = 0 WHERE StudentCardNumber = @StudentCard AND SectionID = @SectionID",
                    new[] {
                new SqlParameter("@StudentCard", studentCard),
                new SqlParameter("@SectionID", sectionID)
                    });

                MessageBox.Show("✅ Студент исключен из секции", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadSectionStudentsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddSettingCard(FlowLayoutPanel parent, string title, string description, Color color, Action click)
        {
            var card = new Panel { Size = new Size(600, 100), Margin = new Padding(10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var t = new Label { Text = title, Location = new Point(20, 15), Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), ForeColor = color, AutoSize = true };
            var d = new Label { Text = description, Location = new Point(20, 45), Font = new System.Drawing.Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true };
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