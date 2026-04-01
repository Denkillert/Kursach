using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PolesSU_Sports.Management;
using PolesSU_Sports;

namespace PolesSU_Sports.Management
{
    public partial class ManagerForm : Form
    {
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Label headerLabel;
        private Button currentActiveButton;

        // Список кнопок меню
        private List<Button> menuButtons = new List<Button>();

        public ManagerForm()
        {
            InitializeComponent();
            SetupModernUI();
            LoadDashboard(); // Загружаем дашборд по умолчанию
        }

        private void SetupModernUI()
        {
            this.Text = "PolessGU Sports: Панель Менеджера";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 240, 245); // Светло-серый фон

            // 1. Создаем боковую панель (Sidebar)
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.FromArgb(45, 55, 75), // Темно-синий
                Padding = new Padding(0, 20, 0, 0)
            };
            this.Controls.Add(sidebarPanel);

            // Логотип / Заголовок в сайдбаре
            var logoLabel = new Label
            {
                Text = "PolessGU\nSports",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(260, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top
            };
            sidebarPanel.Controls.Add(logoLabel);

            // 2. Создаем верхнюю панель (Header)
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };
            this.Controls.Add(headerPanel);

            headerLabel = new Label
            {
                Text = "Дашборд",
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                ForeColor = Color.FromArgb(45, 55, 75),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            headerPanel.Controls.Add(headerLabel);

            // Инфо о пользователе справа в хедере
            var userInfo = new Label
            {
                Text = "Менеджер: Иванов И.И.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(headerPanel.Width - 150, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            headerPanel.Controls.Add(userInfo);

            // 3. Основная область контента
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(240, 240, 245)
            };
            this.Controls.Add(contentPanel);

            // 4. Создаем кнопки меню
            CreateMenuButton("📊 Дашборд", LoadDashboard);
            CreateMenuButton("📑 Отчеты и Фильтры", LoadReports);
            CreateMenuButton("🎓 Заявки студентов", LoadStudentRequests);
            CreateMenuButton("⚙️ Настройки доступа", LoadSettings);

            // Разделитель внизу
            var separator = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(45, 55, 75)
            };
            sidebarPanel.Controls.Add(separator);

            var exitBtn = CreateStyledButton("Выйти", Color.FromArgb(192, 57, 43));
            exitBtn.Dock = DockStyle.Bottom;
            exitBtn.Height = 40;
            exitBtn.Margin = new Padding(10, 0, 10, 10);
            exitBtn.Click += (s, e) => {
                if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Close();
                    // Здесь логика возврата к логину
                    new LoginForm().Show();
                }
            };
            sidebarPanel.Controls.Add(exitBtn);
        }

        private void CreateMenuButton(string text, Action clickAction)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 55, 75),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => {
                if (btn != currentActiveButton)
                    btn.BackColor = Color.FromArgb(55, 65, 85);
            };
            btn.MouseLeave += (s, e) => {
                if (btn != currentActiveButton)
                    btn.BackColor = Color.FromArgb(45, 55, 75);
            };
            btn.Click += (s, e) => {
                SetActiveButton(btn);
                clickAction();
            };

            sidebarPanel.Controls.Add(btn);
            menuButtons.Add(btn);
        }

        private void SetActiveButton(Button btn)
        {
            if (currentActiveButton != null)
            {
                currentActiveButton.BackColor = Color.FromArgb(45, 55, 75);
                currentActiveButton.ForeColor = Color.FromArgb(200, 200, 200);
            }
            currentActiveButton = btn;
            btn.BackColor = Color.FromArgb(60, 70, 90);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private Button CreateStyledButton(string text, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // --- Методы загрузки контента ---

        private void LoadDashboard()
        {
            headerLabel.Text = "Обзор показателей";
            contentPanel.Controls.Clear();

            // Здесь можно добавить графики из DashboardForm
            var lbl = new Label
            {
                Text = "Здесь будут графики посещаемости и финансов (интеграция с DashboardForm)",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14)
            };
            contentPanel.Controls.Add(lbl);
        }

        private void LoadReports()
        {
            headerLabel.Text = "Генерация отчетов и фильтры";
            contentPanel.Controls.Clear();

            // Создаем панель фильтров
            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 10)
            };

            // Тип отчета
            var lblType = new Label { Text = "Тип отчета:", AutoSize = true, Margin = new Padding(5, 10, 5, 5) };
            var cmbType = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5, 5, 20, 5) };
            cmbType.Items.AddRange(new object[] { "Для ректората", "Для кафедры", "Финансовый", "Посещаемость" });
            cmbType.SelectedIndex = 0;

            // Даты
            var lblDate = new Label { Text = "Период:", AutoSize = true, Margin = new Padding(5, 10, 5, 5) };
            var dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Short, Margin = new Padding(5, 5, 5, 5) };
            var dtpEnd = new DateTimePicker { Format = DateTimePickerFormat.Short, Margin = new Padding(5, 5, 20, 5) };

            // Секция
            var lblSec = new Label { Text = "Секция:", AutoSize = true, Margin = new Padding(5, 10, 5, 5) };
            var cmbSec = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5, 5, 20, 5) };
            cmbSec.Items.AddRange(new object[] { "Все секции", "Футбол", "Волейбол", "Баскетбол", "Плавание" });
            cmbSec.SelectedIndex = 0;

            // Кнопка формирования
            var btnGenerate = CreateStyledButton("Сформировать отчет", Color.FromArgb(52, 152, 219));
            btnGenerate.AutoSize = true;
            btnGenerate.Padding = new Padding(15, 10, 15, 10);
            btnGenerate.Margin = new Padding(10, 5, 10, 5);

            btnGenerate.Click += (s, e) => {
                MessageBox.Show($"Формируем отчет: {cmbType.SelectedItem}\nПериод: {dtpStart.Value.ToShortDateString()} - {dtpEnd.Value.ToShortDateString()}\nСекция: {cmbSec.SelectedItem}", "Отчет готов", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Здесь вызов логики Reports.RectorReport и т.д.
            };

            filterPanel.Controls.AddRange(new Control[] { lblType, cmbType, lblDate, dtpStart, dtpEnd, lblSec, cmbSec, btnGenerate });
            contentPanel.Controls.Add(filterPanel);

            // Таблица предпросмотра (заглушка)
            var gridPlaceholder = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var gridLabel = new Label
            {
                Text = "Таблица данных отчета появится здесь после нажатия кнопки \"Сформировать\"",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            gridPlaceholder.Controls.Add(gridLabel);
            contentPanel.Controls.Add(gridPlaceholder);
        }

        private void LoadStudentRequests()
        {
            headerLabel.Text = "Заявки студентов";
            contentPanel.Controls.Clear();

            var stubPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var lbl = new Label
            {
                Text = "Раздел заявок студентов находится в разработке.\nФункционал будет добавлен в следующем обновлении.",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.OrangeRed
            };
            stubPanel.Controls.Add(lbl);
            contentPanel.Controls.Add(stubPanel);
        }

        private void LoadSettings()
        {
            headerLabel.Text = "Настройки доступа";
            contentPanel.Controls.Clear();

            // Заглушка, так как есть отдельная форма UserAccessForm
            var msg = new Label
            {
                Text = "Для управления доступом откроется отдельное окно.\n(Интеграция с UserAccessForm)",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(msg);

            // В реальности тут: new UserAccessForm().ShowDialog();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ManagerForm
            // 
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Name = "ManagerForm";
            this.ResumeLayout(false);
        }
    }
}