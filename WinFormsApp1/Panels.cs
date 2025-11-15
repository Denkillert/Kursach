using RowingBaseAccounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public enum UserType
    {
        Visitor,
        Cashier,
        Manager,
        Admin
    }


    public class BaseSelectionForm : Form
    {
        private Form1 mainForm;
        private RowingBaseAccounting.Client currentClient;
        private UserType userType;
        public RowingBaseAccounting.RowingBase SelectedBase { get; private set; }

        public BaseSelectionForm(Form1 mainForm, RowingBaseAccounting.Client client = null, UserType userType = UserType.Visitor)
        {
            this.mainForm = mainForm;
            this.currentClient = client;
            this.userType = userType;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(600, 430);
            this.Text = "Выбор гребной базы";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Заголовок
            Label titleLabel = new Label()
            {
                Text = GetTitleText(),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Size = new Size(400, 40),
                Location = new Point(100, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // База 1
            Panel base1Panel = CreateBasePanel(1, "🏊 Гребная база №1",
                "📍 г.Пинск, ул. Центральная, 25", "📞 8-029-111-11-11", 100);

            // База 2
            Panel base2Panel = CreateBasePanel(2, "🚣 Гребная база №2",
                "📍 г.Пинск, ул. Иркутско-Пинской дивизии, 46", "📞 8-029-692-70-05", 220);

            // Кнопка назад
            Button backButton = new Button()
            {
                Text = "⬅️ Назад",
                Size = new Size(100, 35),
                Location = new Point(50, (this.Bottom - 80)),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (s, e) => { this.Close(); mainForm.Show(); };

            this.Controls.AddRange(new Control[] {
                titleLabel, base1Panel, base2Panel, backButton
            });
        }

        private string GetTitleText()
        {
            return userType switch
            {
                UserType.Visitor => "🏁 ВЫБЕРИТЕ ГРЕБНУЮ БАЗУ",
                UserType.Cashier => "📊 ВЫБЕРИТЕ БАЗУ ДЛЯ ПРОДАЖИ УСЛУГ",
                UserType.Manager => "👥 ВЫБЕРИТЕ БАЗУ ДЛЯ УПРАВЛЕНИЯ",
                UserType.Admin => "⚙️ ВЫБЕРИТЕ БАЗУ ДЛЯ АДМИНИСТРИРОВАНИЯ",
                _ => "🏁 ВЫБЕРИТЕ ГРЕБНУЮ БАЗУ"
            };
        }

        private Panel CreateBasePanel(int baseNumber, string title, string address, string phone, int top)
        {
            Panel panel = new Panel()
            {
                Size = new Size(500, 100),
                Location = new Point(50, top),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 248, 255),
                Cursor = Cursors.Hand,
                Tag = baseNumber
            };

            Label titleLabel = new Label()
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(0, 102, 204),
                Cursor = Cursors.Hand
            };

            Label addressLabel = new Label()
            {
                Text = address,
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 45),
                Size = new Size(300, 20),
                Cursor = Cursors.Hand
            };

            Label phoneLabel = new Label()
            {
                Text = phone,
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 65),
                Size = new Size(200, 20),
                Cursor = Cursors.Hand
            };

            // Добавляем обработчики ко всем элементам панели
            foreach (Control control in new Control[] { panel, titleLabel, addressLabel, phoneLabel })
            {
                control.Click += (s, e) => SelectBase(baseNumber);
                control.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(220, 240, 255);
                control.MouseLeave += (s, e) => panel.BackColor = Color.FromArgb(240, 248, 255);
            }

            panel.Controls.AddRange(new Control[] { titleLabel, addressLabel, phoneLabel });
            return panel;
        }

        private void SelectBase(int baseNumber)
        {
            RowingBaseAccounting.RowingBase selectedBase = baseNumber == 1 ?
                new RowingBaseAccounting.RowingBase(1, "Гребная база №1", "г.Пинск, ул. Центральная, 25", "8-029-111-11-11", "base1@mail.ru") :
                new RowingBaseAccounting.RowingBase(2, "Гребная база №2", "г.Пинск, ул. Иркутско-Пинской дивизии, 46", "8-029-692-70-05", "ysk_volna@mail.ru");

            this.SelectedBase = selectedBase;

            switch (userType)
            {
                case UserType.Visitor:
                    VisitorPanelForm visitorPanel = new VisitorPanelForm(currentClient, mainForm, selectedBase);
                    this.Hide();
                    visitorPanel.ShowDialog();
                    this.Close();
                    break;
                case UserType.Cashier:
                    CashierPanelForm cashierPanel = new CashierPanelForm(mainForm, selectedBase);
                    this.Hide();
                    cashierPanel.ShowDialog();
                    this.Close();
                    break;
                case UserType.Manager:
                    ManagerPanelForm managerPanel = new ManagerPanelForm(mainForm, selectedBase);
                    this.Hide();
                    managerPanel.ShowDialog();
                    this.Close();
                    break;
                case UserType.Admin:
                    AdminPanelForm adminPanel = new AdminPanelForm(mainForm, selectedBase);
                    this.Hide();
                    adminPanel.ShowDialog();
                    this.Close();
                    break;
            }
        }
    }

    // Форма входа посетителя
    public class VisitorRegistrationForm : Form
    {
        public RowingBaseAccounting.Client RegisteredClient { get; private set; }

        public VisitorRegistrationForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 550); // Увеличили высоту для пароля
            this.Text = "Регистрация посетителя";
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label titleLabel = new Label()
            {
                Text = "Регистрация нового посетителя",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(350, 40),
                Location = new Point(75, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            TextBox nameTextBox = new TextBox() { Location = new Point(150, 80), Size = new Size(250, 25) };
            TextBox phoneTextBox = new TextBox() { Location = new Point(150, 120), Size = new Size(250, 25) };
            TextBox emailTextBox = new TextBox() { Location = new Point(150, 160), Size = new Size(250, 25) };
            DateTimePicker dobPicker = new DateTimePicker() { Location = new Point(150, 200), Size = new Size(150, 25) };
            ComboBox typeComboBox = new ComboBox()
            {
                Location = new Point(150, 240),
                Size = new Size(150, 25),
                DataSource = new[] { "Студент", "Сотрудник", "Физ.лицо", "Юр.лицо" }
            };

            // НОВОЕ ПОЛЕ: Пароль
            Label passwordLabel = new Label() { Text = "Пароль*:", Location = new Point(50, 280), Size = new Size(100, 25) };
            TextBox passwordTextBox = new TextBox()
            {
                Location = new Point(150, 280),
                Size = new Size(250, 25),
                UseSystemPasswordChar = true,
                PlaceholderText = "Минимум 6 символов"
            };

            Button registerBtn = new Button()
            {
                Text = "Зарегистрироваться",
                Location = new Point(150, 330),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(0, 153, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            registerBtn.Click += (s, e) => RegisterClient(
                nameTextBox.Text, phoneTextBox.Text, emailTextBox.Text,
                typeComboBox.SelectedItem.ToString(), dobPicker.Value, passwordTextBox.Text);

            Button cancelBtn = new Button()
            {
                Text = "Отмена",
                Location = new Point(150, 380),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat
            };
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                titleLabel,
                new Label() { Text = "ФИО*:", Location = new Point(50, 80), Size = new Size(100, 25) },
                nameTextBox,
                new Label() { Text = "Телефон*:", Location = new Point(50, 120), Size = new Size(100, 25) },
                phoneTextBox,
                new Label() { Text = "Email:", Location = new Point(50, 160), Size = new Size(100, 25) },
                emailTextBox,
                new Label() { Text = "Дата рождения*:", Location = new Point(50, 200), Size = new Size(100, 25) },
                dobPicker,
                new Label() { Text = "Тип:", Location = new Point(50, 240), Size = new Size(100, 25) },
                typeComboBox,
                passwordLabel,
                passwordTextBox,
                registerBtn,
                cancelBtn
            });
        }

        private void RegisterClient(string name, string phone, string email, string type, DateTime dob, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Введите ФИО и телефон", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверяем, не зарегистрирован ли уже клиент с таким телефоном
            var existingClient = ClientManager.FindClientByPhone(phone);
            if (existingClient != null)
            {
                MessageBox.Show("Клиент с таким телефоном уже зарегистрирован", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var client = new RowingBaseAccounting.Client
            {
                Name = name,
                Phone = phone,
                Email = email,
                Type = type,
                DateOfBirth = dob,
                Password = password // Сохраняем пароль
            };

            RowingBaseAccounting.ClientManager.AddClient(client);
            RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");

            RegisteredClient = client;
            this.DialogResult = DialogResult.OK;
            MessageBox.Show($"Регистрация успешна! Ваш ID: {client.Id}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    // Форма входа посетителя
    public class VisitorLoginForm : Form
    {
        private Form1 mainForm;
        private TabControl tabControl;

        public VisitorLoginForm(Form1 mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 400);
            this.Text = "Вход для посетителя";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.MaximizeBox = false;

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Padding = new Point(10, 5);

            // Вкладка входа по ID
            TabPage idTab = new TabPage("По ID");
            InitializeIdTab(idTab);

            // Вкладка входа по телефону
            TabPage phoneTab = new TabPage("По телефону");
            InitializePhoneTab(phoneTab);

            tabControl.TabPages.Add(idTab);
            tabControl.TabPages.Add(phoneTab);

            Button backButton = new Button()
            {
                Text = "⬅️ Назад",
                Size = new Size(100, 35),
                Location = new Point(20, 320),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat
            };
            backButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            backButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { tabControl, backButton });
        }

        private void InitializeIdTab(TabPage tab)
        {
            tab.Padding = new Padding(20);

            Label idLabel = new Label() { Text = "ID клиента:", Location = new Point(50, 50), Size = new Size(100, 25) };
            TextBox idTextBox = new TextBox() { Location = new Point(150, 50), Size = new Size(200, 25) };

            Label passwordLabel = new Label() { Text = "Пароль:", Location = new Point(50, 100), Size = new Size(100, 25) };
            TextBox passwordTextBox = new TextBox() { Location = new Point(150, 100), Size = new Size(200, 25), UseSystemPasswordChar = true };

            Button loginBtn = new Button()
            {
                Text = "Войти",
                Size = new Size(120, 35),
                Location = new Point(150, 150),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(0, 153, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loginBtn.Click += (s, e) => ProcessIdLogin(idTextBox.Text, passwordTextBox.Text);

            Button registerBtn = new Button()
            {
                Text = "Зарегистрироваться",
                Size = new Size(180, 35),
                Location = new Point(150, 200),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            registerBtn.Click += (s, e) => ShowRegistrationForm();

            tab.Controls.AddRange(new Control[] {
            idLabel, idTextBox, passwordLabel, passwordTextBox, loginBtn, registerBtn
        });
        }

        private void InitializePhoneTab(TabPage tab)
        {
            tab.Padding = new Padding(20);

            Label phoneLabel = new Label() { Text = "Телефон:", Location = new Point(50, 50), Size = new Size(100, 25) };
            TextBox phoneTextBox = new TextBox() { Location = new Point(150, 50), Size = new Size(200, 25) };

            Label passwordLabel = new Label() { Text = "Пароль:", Location = new Point(50, 100), Size = new Size(100, 25) };
            TextBox passwordTextBox = new TextBox() { Location = new Point(150, 100), Size = new Size(200, 25), UseSystemPasswordChar = true };

            Button loginBtn = new Button()
            {
                Text = "Войти",
                Size = new Size(120, 35),
                Location = new Point(150, 150),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(0, 153, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loginBtn.Click += (s, e) => ProcessPhoneLogin(phoneTextBox.Text, passwordTextBox.Text);

            Button registerBtn = new Button()
            {
                Text = "Зарегистрироваться",
                Size = new Size(180, 35),
                Location = new Point(150, 200),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            registerBtn.Click += (s, e) => ShowRegistrationForm();

            tab.Controls.AddRange(new Control[] {
            phoneLabel, phoneTextBox, passwordLabel, passwordTextBox, loginBtn, registerBtn
        });
        }

        private void ProcessIdLogin(string id, string password)
        {
            if (int.TryParse(id, out int clientId))
            {
                var client = RowingBaseAccounting.ClientManager.GetClient(clientId);
                if (client != null && VerifyPassword(client, password))
                {
                    ShowBaseSelection(client);
                    return;
                }
            }
            MessageBox.Show("Неверный ID или пароль");
        }

        private void ProcessPhoneLogin(string phone, string password)
        {
            var client = RowingBaseAccounting.ClientManager.GetClients()
                .FirstOrDefault(c => c.Phone == phone);

            if (client != null && VerifyPassword(client, password))
            {
                ShowBaseSelection(client);
                return;
            }
            MessageBox.Show("Неверный телефон или пароль");
        }

        private bool VerifyPassword(RowingBaseAccounting.Client client, string password)
        {
            // Простая проверка пароля (в реальной системе нужно хеширование)
            return client.Password == password; // Временная логика
        }

        private void ShowBaseSelection(RowingBaseAccounting.Client client)
        {
            BaseSelectionForm baseSelectionForm = new BaseSelectionForm(mainForm, client);
            this.Hide();
            baseSelectionForm.ShowDialog();
            this.Close();
        }

        private void ShowRegistrationForm()
        {
            VisitorRegistrationForm registrationForm = new VisitorRegistrationForm();
            if (registrationForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"Регистрация успешна! Ваш ID: {registrationForm.RegisteredClient?.Id}\nПароль для входа: ваш номер телефона");
            }
        }
    }

    // Панель посетителя
    public class VisitorPanelForm : Form
    {
        private RowingBaseAccounting.Client currentClient;
        private Form1 mainForm;
        private DataGridView bookingsGrid;
        private RowingBaseAccounting.RowingBase currentBase;
        private MenuStrip mainMenu;
        private StatusStrip statusStrip;
        private TabControl tabControl;

        public VisitorPanelForm(RowingBaseAccounting.Client client, Form1 mainForm, RowingBaseAccounting.RowingBase selectedBase)
        {
            this.currentClient = client;
            this.mainForm = mainForm;
            this.currentBase = selectedBase;
            InitializeComponent();
            LoadBookings();
            UpdateStatusStrip();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 700);
            this.Text = $"Панель посетителя - {currentClient.Name} (ID: {currentClient.Id}) - {currentBase.Name}";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Главное меню
            CreateMainMenu();

            // Строка состояния
            CreateStatusStrip();

            // Основной контент
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Padding = new Point(20, 10);

            TabPage bookingsTab = new TabPage("📋 Мои бронирования");
            InitializeBookingsTab(bookingsTab);

            TabPage createBookingTab = new TabPage("➕ Создать бронирование");
            InitializeCreateBookingTab(createBookingTab);

            TabPage profileTab = new TabPage("👤 Мой профиль");
            InitializeProfileTab(profileTab);

            tabControl.TabPages.Add(bookingsTab);
            tabControl.TabPages.Add(createBookingTab);
            tabControl.TabPages.Add(profileTab);

            this.Controls.Add(tabControl);
            this.Controls.Add(mainMenu);
            this.Controls.Add(statusStrip);
        }

        private void InitializeCreateBookingTab(TabPage tab)
        {
            tab.BackColor = SystemColors.Control;

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);

            GroupBox bookingGroup = new GroupBox();
            bookingGroup.Text = " 📅 Создание нового бронирования";
            bookingGroup.Font = new Font("Arial", 11, FontStyle.Bold);
            bookingGroup.Dock = DockStyle.Top;
            bookingGroup.Height = 300;
            bookingGroup.Padding = new Padding(15);
            bookingGroup.Location = new Point(0, 70); // Смещено вниз для кнопки

            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 2;
            tableLayout.RowCount = 4;
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            // Выбор услуги
            Label serviceLabel = new Label()
            {
                Text = "Услуга:",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };
            ComboBox serviceComboBox = new ComboBox()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Дата и время
            Label dateLabel = new Label()
            {
                Text = "Дата и время:",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };
            DateTimePicker dateTimePicker = new DateTimePicker()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10),
                Value = DateTime.Now.AddDays(1),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm"
            };

            // Количество часов
            Label quantityLabel = new Label()
            {
                Text = "Количество часов:",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };
            NumericUpDown numericUpDown = new NumericUpDown()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10),
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };

            // Кнопка создания
            Button createBtn = new Button()
            {
                Text = "🎯 СОЗДАТЬ БРОНИРОВАНИЕ",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Arial", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Height = 45
            };
            createBtn.Click += (s, e) => CreateBooking(
                serviceComboBox.SelectedItem as RowingBaseAccounting.Service,
                dateTimePicker.Value,
                (double)numericUpDown.Value);

            tableLayout.Controls.Add(serviceLabel, 0, 0);
            tableLayout.Controls.Add(serviceComboBox, 1, 0);
            tableLayout.Controls.Add(dateLabel, 0, 1);
            tableLayout.Controls.Add(dateTimePicker, 1, 1);
            tableLayout.Controls.Add(quantityLabel, 0, 2);
            tableLayout.Controls.Add(numericUpDown, 1, 2);
            tableLayout.Controls.Add(createBtn, 0, 3);
            tableLayout.SetColumnSpan(createBtn, 2);

            bookingGroup.Controls.Add(tableLayout);

            // Заполнение услуг ДЛЯ ВЫБРАННОЙ БАЗЫ
            var services = RowingBaseAccounting.ServiceManager.GetServices(currentBase.Number)
                .Where(s => s.IsAvailableFor(currentClient))
                .ToList();
            serviceComboBox.DisplayMember = "Name";
            serviceComboBox.DataSource = services;

            // Панель быстрого доступа
            GroupBox quickAccessGroup = new GroupBox();
            quickAccessGroup.Text = " 🚀 Быстрый доступ";
            quickAccessGroup.Font = new Font("Arial", 11, FontStyle.Bold);
            quickAccessGroup.Dock = DockStyle.Top;
            quickAccessGroup.Height = 100;
            quickAccessGroup.Padding = new Padding(15);
            quickAccessGroup.Location = new Point(0, 380);

            FlowLayoutPanel quickPanel = new FlowLayoutPanel();
            quickPanel.Dock = DockStyle.Fill;
            quickPanel.FlowDirection = FlowDirection.LeftToRight;

            string[] quickServices = { "Тренажёрный зал", "Сауна", "Терраса" };
            foreach (string serviceName in quickServices)
            {
                Button quickBtn = new Button()
                {
                    Text = serviceName,
                    Size = new Size(120, 60),
                    BackColor = Color.FromArgb(0, 153, 255),
                    ForeColor = Color.White,
                    Font = new Font("Arial", 9),
                    FlatStyle = FlatStyle.Flat,
                    Tag = serviceName
                };
                quickBtn.Click += (s, e) =>
                {
                    var service = services.FirstOrDefault(svc => svc.Name.Contains(serviceName));
                    if (service != null)
                    {
                        serviceComboBox.SelectedItem = service;
                        dateTimePicker.Value = DateTime.Now.AddHours(2);
                    }
                };
                quickPanel.Controls.Add(quickBtn);
            }

            quickAccessGroup.Controls.Add(quickPanel);

            mainPanel.Controls.Add(bookingGroup);
            mainPanel.Controls.Add(quickAccessGroup);

            tab.Controls.Add(mainPanel);
        }

        // Остальные методы остаются без изменений...
        private void CreateMainMenu()
        {
            mainMenu = new MenuStrip();
            mainMenu.Dock = DockStyle.Top;

            // Меню "Файл"
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Обновить данные", null, (s, e) => LoadBookings());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("Сменить базу", null, (s, e) => SwitchBase());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("Выйти", null, (s, e) => { this.Close(); mainForm.Show(); });

            // Меню "Справка"
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");
            helpMenu.DropDownItems.Add("О программе", null, (s, e) => ShowAbout());
            helpMenu.DropDownItems.Add("Помощь", null, (s, e) => ShowHelp());

            mainMenu.Items.AddRange(new ToolStripMenuItem[] { fileMenu, helpMenu });
        }

        private void CreateStatusStrip()
        {
            statusStrip = new StatusStrip();
            statusStrip.Dock = DockStyle.Bottom;

            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = $"Вошел как: {currentClient.Name} | База: {currentBase.Name} | ID: {currentClient.Id}";
            statusLabel.Spring = true;

            ToolStripStatusLabel timeLabel = new ToolStripStatusLabel();
            timeLabel.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, timeLabel });
        }

        private void SwitchBase()
        {
            DialogResult result = MessageBox.Show(
                "Хотите сменить гребную базу?",
                "Смена базы",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                BaseSelectionForm baseSelection = new BaseSelectionForm(mainForm, currentClient);
                this.Hide();
                baseSelection.ShowDialog();
                this.Close();
            }
        }

        private void InitializeBookingsTab(TabPage tab)
        {
            // Панель инструментов
            ToolStrip toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;
            toolStrip.ImageScalingSize = new Size(20, 20);



            // Таблица бронирований
            bookingsGrid = new DataGridView();
            bookingsGrid.Dock = DockStyle.Fill;
            bookingsGrid.ReadOnly = true;
            bookingsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            bookingsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            bookingsGrid.RowHeadersVisible = false;
            bookingsGrid.BackgroundColor = SystemColors.Window;

            // ДОБАВИТЬ ЭТОТ ОБРАБОТЧИК
            bookingsGrid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0) // Проверяем, что кликнули по строке, а не по заголовку
                {
                    ShowBookingDetails(); // Вызываем метод при двойном клике
                }
            };

            // Контекстное меню для таблицы
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            gridContextMenu.Items.Add("Отменить бронирование", null, (s, e) => CancelSelectedBooking());
            gridContextMenu.Items.Add("Подробнее", null, (s, e) => ShowBookingDetails());
            bookingsGrid.ContextMenuStrip = gridContextMenu;

            // Группа статистики
            GroupBox statsGroup = new GroupBox();
            statsGroup.Text = "Статистика";
            statsGroup.Dock = DockStyle.Bottom;
            statsGroup.Height = 80;

            Label statsLabel = new Label();
            statsLabel.Dock = DockStyle.Fill;
            statsLabel.TextAlign = ContentAlignment.MiddleCenter;
            statsLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            statsLabel.ForeColor = Color.DarkBlue;

            statsGroup.Controls.Add(statsLabel);

            tab.Controls.AddRange(new Control[] { toolStrip, bookingsGrid, statsGroup });

            // Обновляем статистику при загрузке данных
            this.Load += (s, e) =>
            {
                UpdateBookingStats(statsLabel);
            };
            // Добавляем иконки (можно заменить на реальные изображения)
            ToolStripButton refreshBtn = new ToolStripButton("Обновить");
            refreshBtn.Click += (s, e) =>
            {
                LoadBookings(); this.Load += (s, e) =>
                {
                    UpdateBookingStats(statsLabel);
                };
            };

            ToolStripButton cancelBtn = new ToolStripButton("Отменить выбранное");
            cancelBtn.Click += (s, e) => CancelSelectedBooking();

            ToolStripButton deleteBtn = new ToolStripButton("Удалить отмененные");
            deleteBtn.Click += (s, e) => DeleteCancelledBookings();

            toolStrip.Items.AddRange(new ToolStripItem[] { refreshBtn, cancelBtn, deleteBtn });
        }


        private void InitializeProfileTab(TabPage tab)
        {
            tab.Padding = new Padding(20);

            GroupBox profileGroup = new GroupBox();
            profileGroup.Text = " 👤 Информация о профиле";
            profileGroup.Font = new Font("Arial", 11, FontStyle.Bold);
            profileGroup.Dock = DockStyle.Top;
            profileGroup.Height = 250;

            TableLayoutPanel profileLayout = new TableLayoutPanel();
            profileLayout.Dock = DockStyle.Fill;
            profileLayout.Padding = new Padding(15);
            profileLayout.ColumnCount = 2;
            profileLayout.RowCount = 6;

            string[] labels = { "ФИО:", "Тип клиента:", "Телефон:", "Email:", "Скидка:", "Дата регистрации:" };
            string[] values = {
                currentClient.Name,
                currentClient.Type,
                currentClient.Phone ?? "не указан",
                currentClient.Email ?? "не указан",
                $"{currentClient.DiscountPercent}%",
                currentClient.RegistrationDate.ToString("dd.MM.yyyy")
            };

            for (int i = 0; i < labels.Length; i++)
            {
                Label label = new Label()
                {
                    Text = labels[i],
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Height = 30
                };
                Label value = new Label()
                {
                    Text = values[i],
                    Font = new Font("Arial", 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Height = 30
                };

                profileLayout.Controls.Add(label, 0, i);
                profileLayout.Controls.Add(value, 1, i);
            }

            profileGroup.Controls.Add(profileLayout);
            tab.Controls.Add(profileGroup);
        }

        private void LoadBookings()
        {
            var clientBookings = RowingBaseAccounting.BookingManager.GetClientBookings(currentClient.Id);

            var displayData = clientBookings.Select(b => new
            {
                b.Id,
                Услуга = b.ServiceName,
                Дата_начала = b.StartTime.ToString("dd.MM.yyyy HH:mm"),
                Дата_окончания = b.EndTime.ToString("dd.MM.yyyy HH:mm"),
                Стоимость = $"{b.TotalCost:C2}",
                Статус = b.Status,
                Можно_отменить = (b.StartTime - DateTime.Now).TotalHours > 48 ? "Да" : "Нет",
                CanCancel = (b.StartTime - DateTime.Now).TotalHours > 48
            }).ToList();

            bookingsGrid.DataSource = displayData;
            bookingsGrid.Columns["CanCancel"].Visible = false;

            // Раскрашиваем строки по статусу
            foreach (DataGridViewRow row in bookingsGrid.Rows)
            {
                string status = row.Cells["Статус"].Value?.ToString() ?? "";
                if (status == "Отменено")
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                else if (status == "Активно")
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
            }

            UpdateStatusStrip();
        }

        private void CancelSelectedBooking()
        {
            if (bookingsGrid.CurrentRow != null)
            {
                int bookingId = (int)bookingsGrid.CurrentRow.Cells["Id"].Value;
                string serviceName = bookingsGrid.CurrentRow.Cells["Услуга"].Value.ToString();
                DateTime startTime = DateTime.Parse(bookingsGrid.CurrentRow.Cells["Дата_начала"].Value.ToString());
                bool canCancel = (bool)bookingsGrid.CurrentRow.Cells["CanCancel"].Value;

                if (!canCancel)
                {
                    TimeSpan timeLeft = startTime - DateTime.Now;
                    MessageBox.Show($"❌ Нельзя отменить бронирование '{serviceName}'.\n\n" +
                                  $"До начала осталось: {timeLeft.Days} дней {timeLeft.Hours} часов.\n" +
                                  $"Отмена возможна не позднее чем за 2 суток до начала.",
                                  "Отмена невозможна",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите отменить бронирование?\n\n" +
                    $"Услуга: {serviceName}\n" +
                    $"Время: {startTime:dd.MM.yyyy HH:mm}\n\n" +
                    $"Бронирование будет отменено, но останется в истории.",
                    "Подтверждение отмены",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (RowingBaseAccounting.BookingManager.CancelBooking(bookingId, currentClient.Id))
                    {
                        RowingBaseAccounting.FileManager.SaveToFile(
                            RowingBaseAccounting.BookingManager.GetBookings(), "bookings.txt");
                        LoadBookings();
                        MessageBox.Show("✅ Бронирование успешно отменено",
                                      "Отмена выполнена",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("❌ Не удалось отменить бронирование",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите бронирование для отмены",
                              "Внимание",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
            }
        }

        private void DeleteCancelledBookings()
        {
            var cancelledBookings = RowingBaseAccounting.BookingManager.GetClientBookings(currentClient.Id)
                .Where(b => b.Status == "Отменено")
                .ToList();

            if (cancelledBookings.Count == 0)
            {
                MessageBox.Show("Нет отмененных бронирований для удаления",
                              "Информация",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Вы действительно хотите удалить все отмененные бронирования?\n" +
                $"Будет удалено: {cancelledBookings.Count} записей",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Здесь должна быть реализация удаления отмененных бронирований
                MessageBox.Show("Функция удаления отмененных бронирований будет реализована в следующей версии",
                              "В разработке",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
        }

        private void UpdateStatusStrip()
        {
            var bookings = RowingBaseAccounting.BookingManager.GetClientBookings(currentClient.Id);
            int activeCount = bookings.Count(b => b.Status == "Активно");
            int cancelledCount = bookings.Count(b => b.Status == "Отменено");

            if (statusStrip.Items.Count > 0)
            {
                statusStrip.Items[0].Text = $"Вошел как: {currentClient.Name} | Активных бронирований: {activeCount} | Отмененных: {cancelledCount}";
                statusStrip.Items[1].Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            }
        }

        private void UpdateBookingStats(Label statsLabel)
        {
            var bookings = RowingBaseAccounting.BookingManager.GetClientBookings(currentClient.Id);
            int activeCount = bookings.Count(b => b.Status == "Активно");
            decimal totalSpent = bookings.Where(b => b.Status == "Активно").Sum(b => b.TotalCost);

            statsLabel.Text = $"Активных бронирований: {activeCount} | Общая стоимость: {totalSpent:C2}";
        }

        // Методы меню

        private void ShowBookingDetails()
        {
            if (bookingsGrid.CurrentRow != null)
            {
                string serviceName = bookingsGrid.CurrentRow.Cells["Услуга"].Value.ToString();
                string startTime = bookingsGrid.CurrentRow.Cells["Дата_начала"].Value.ToString();
                string endTime = bookingsGrid.CurrentRow.Cells["Дата_окончания"].Value.ToString();
                string cost = bookingsGrid.CurrentRow.Cells["Стоимость"].Value.ToString();
                string status = bookingsGrid.CurrentRow.Cells["Статус"].Value.ToString();

                MessageBox.Show(
                    $"Детали бронирования:\n\n" +
                    $"Услуга: {serviceName}\n" +
                    $"Начало: {startTime}\n" +
                    $"Окончание: {endTime}\n" +
                    $"Стоимость: {cost}\n" +
                    $"Статус: {status}",
                    "Детали бронирования",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "Система бронирования гребных баз\n\n" +
                "Версия 2.0\n",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "📋 Как пользоваться системой:\n\n" +
                "• Для создания бронирования перейдите на вкладку 'Создать бронирование'\n" +
                "• Для отмены бронирования выберите его в списке и нажмите 'Отменить'\n" +
                "• Отмена возможна за 48 часов до начала бронирования\n" +
                "• Просматривайте статистику в нижней части экрана\n\n" +
                "По всем вопросам обращайтесь в поддержку: 8-029-692-70-05",
                "Помощь",
                MessageBoxButtons.OK,
                MessageBoxIcon.Question);
        }

        private void CreateBooking(RowingBaseAccounting.Service service, DateTime startTime, double quantity)
        {
            if (service == null)
            {
                MessageBox.Show("Выберите услугу для бронирования",
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                return;
            }

            if (RowingBaseAccounting.BookingManager.CreateBooking(currentClient, service, startTime, quantity))
            {
                RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.BookingManager.GetBookings(), "bookings.txt");
                LoadBookings();
                MessageBox.Show("✅ Бронирование успешно создано!",
                              "Успех",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("❌ Не удалось создать бронирование. Возможные причины:\n\n" +
                              "• Услуга уже забронирована на это время\n" +
                              "• Превышено максимальное количество посетителей\n" +
                              "• Техническая ошибка",
                              "Ошибка бронирования",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }
    }

    // Форма входа персонала
    public class StaffLoginForm : Form
    {
        private Form1 mainForm;

        public StaffLoginForm(Form1 mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 400);
            this.Text = "Вход для персонала";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.MaximizeBox = false;

            Label titleLabel = new Label()
            {
                Text = "👥 Вход для персонала",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Size = new Size(400, 40),
                Location = new Point(50, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(0, 102, 204)
            };

            Panel loginPanel = new Panel()
            {
                Size = new Size(400, 200),
                Location = new Point(50, 100),
                BorderStyle = BorderStyle.None
            };

            TextBox loginTextBox = new TextBox()
            {
                Location = new Point(150, 30),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Введите логин"
            };

            TextBox passwordTextBox = new TextBox()
            {
                Location = new Point(150, 80),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 10),
                UseSystemPasswordChar = true,
                PlaceholderText = "Введите пароль"
            };

            Button loginBtn = new Button()
            {
                Text = "Войти",
                Size = new Size(150, 40),
                Location = new Point(125, 140),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 153, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loginBtn.Click += (s, e) => ProcessLogin(loginTextBox.Text, passwordTextBox.Text);

            Button backButton = new Button()
            {
                Text = "⬅️ Назад",
                Size = new Size(100, 35),
                Location = new Point(20, 320),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (s, e) => this.Close();

            loginBtn.MouseEnter += (s, e) => loginBtn.BackColor = Color.FromArgb(0, 133, 225);
            loginBtn.MouseLeave += (s, e) => loginBtn.BackColor = Color.FromArgb(0, 153, 255);

            loginPanel.Controls.AddRange(new Control[] {
                new Label() { Text = "Логин:", Location = new Point(50, 35), Size = new Size(80, 25), Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleLeft },
                loginTextBox,
                new Label() { Text = "Пароль:", Location = new Point(50, 85), Size = new Size(80, 25), Font = new Font("Segoe UI", 10), TextAlign = ContentAlignment.MiddleLeft },
                passwordTextBox,
                loginBtn
            });

            this.Controls.AddRange(new Control[] { titleLabel, loginPanel, backButton });
        }

        private void ProcessLogin(string login, string password)
        {
            var employee = RowingBaseAccounting.EmployeeManager.Authenticate(login, password);

            if (employee != null)
            {
                UserType userType = employee.Role switch
                {
                    "Admin" => UserType.Admin,
                    "Manager" => UserType.Manager,
                    "Analyst" => UserType.Cashier,
                    "администратор" => UserType.Admin, // На русском, если есть такие данные
                    "Менеджер" => UserType.Manager,    // На русском, если есть такие данные
                    "Кассир" => UserType.Cashier,    // На русском, если есть такие данные
                    _ => UserType.Cashier
                };

                BaseSelectionForm baseSelection = new BaseSelectionForm(mainForm, null, userType);
                this.Hide();
                baseSelection.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка входа",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Панель Кассира
    public class CashierPanelForm : Form
    {
        private Form1 mainForm;
        private RowingBaseAccounting.RowingBase currentBase;
        private DataGridView unpaidBookingsGrid;
        private DataGridView clientsGrid;

        public CashierPanelForm(Form1 mainForm, RowingBaseAccounting.RowingBase selectedBase)
        {
            this.mainForm = mainForm;
            this.currentBase = selectedBase;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 700);
            this.Text = $"💰 Панель кассира - {currentBase.Name}";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Главный TabControl
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Padding = new Point(20, 10);

            // Вкладка неоплаченных бронирований
            TabPage unpaidTab = new TabPage("📋 Неоплаченные бронирования");
            InitializeUnpaidTab(unpaidTab);

            // Вкладка пополнения баланса
            TabPage balanceTab = new TabPage("💳 Пополнение баланса");
            InitializeBalanceTab(balanceTab);

            tabControl.TabPages.Add(unpaidTab);
            tabControl.TabPages.Add(balanceTab);

            // Кнопка выхода
            Button logoutBtn = new Button()
            {
                Text = "Выйти",
                Size = new Size(80, 30),
                Location = new Point(900, 3),
                BackColor = Color.FromArgb(240, 240, 240),
                FlatStyle = FlatStyle.Flat
            };
            logoutBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            logoutBtn.Click += (s, e) => { this.Close(); mainForm.Show(); };

            this.Controls.AddRange(new Control[] { logoutBtn, tabControl });
        }

        private void InitializeUnpaidTab(TabPage tab)
        {
            tab.Padding = new Padding(10);

            // Панель инструментов
            ToolStrip toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;

            ToolStripButton refreshBtn = new ToolStripButton("Обновить");
            refreshBtn.Click += (s, e) => LoadUnpaidBookings();

            ToolStripButton confirmPaymentBtn = new ToolStripButton("Подтвердить оплату");
            confirmPaymentBtn.Click += (s, e) => ConfirmPayment();

            toolStrip.Items.AddRange(new ToolStripItem[] { refreshBtn, confirmPaymentBtn });

            // Таблица неоплаченных бронирований
            unpaidBookingsGrid = new DataGridView();
            unpaidBookingsGrid.Dock = DockStyle.Fill;
            unpaidBookingsGrid.ReadOnly = true;
            unpaidBookingsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            unpaidBookingsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // ДОБАВИТЬ ЭТОТ ОБРАБОТЧИК
            unpaidBookingsGrid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    ConfirmPayment(); // Вызываем метод подтверждения оплаты при двойном клике
                }
            };

            tab.Controls.AddRange(new Control[] { toolStrip, unpaidBookingsGrid });
        }

        private void InitializeBalanceTab(TabPage tab)
        {
            tab.Padding = new Padding(20);
            tab.BackColor = SystemColors.Control;

            GroupBox balanceGroup = new GroupBox();
            balanceGroup.Text = " 💰 Пополнение баланса клиента";
            balanceGroup.Font = new Font("Arial", 11, FontStyle.Bold);
            balanceGroup.Size = new Size(500, 200);
            balanceGroup.Location = new Point(150, 50);
            balanceGroup.Padding = new Padding(15);

            TableLayoutPanel balanceLayout = new TableLayoutPanel();
            balanceLayout.Dock = DockStyle.Fill;
            balanceLayout.ColumnCount = 2;
            balanceLayout.RowCount = 3;

            // Выбор клиента
            Label clientLabel = new Label()
            {
                Text = "Клиент:",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };
            ComboBox clientComboBox = new ComboBox()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Name"
            };

            // Сумма пополнения
            Label amountLabel = new Label()
            {
                Text = "Сумма (BYN):",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 10)
            };
            NumericUpDown amountNumeric = new NumericUpDown()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10),
                Minimum = 1,
                Maximum = 10000,
                Value = 100
            };

            // Кнопка пополнения
            Button addBalanceBtn = new Button()
            {
                Text = "💰 ПОПОЛНИТЬ БАЛАНС",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Arial", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            addBalanceBtn.Click += (s, e) => AddBalanceToClient(
                clientComboBox.SelectedItem as RowingBaseAccounting.Client,
                amountNumeric.Value);

            balanceLayout.Controls.Add(clientLabel, 0, 0);
            balanceLayout.Controls.Add(clientComboBox, 1, 0);
            balanceLayout.Controls.Add(amountLabel, 0, 1);
            balanceLayout.Controls.Add(amountNumeric, 1, 1);
            balanceLayout.Controls.Add(addBalanceBtn, 0, 2);
            balanceLayout.SetColumnSpan(addBalanceBtn, 2);

            balanceGroup.Controls.Add(balanceLayout);

            // Заполнение списка клиентов
            var clients = RowingBaseAccounting.ClientManager.GetClients();
            clientComboBox.DataSource = clients;

            // Таблица клиентов с балансом
            clientsGrid = new DataGridView();
            clientsGrid.Location = new Point(150, 270);
            clientsGrid.Size = new Size(500, 300);
            clientsGrid.ReadOnly = true;
            clientsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            clientsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Обновление таблицы клиентов
            Button refreshClientsBtn = new Button()
            {
                Text = "Обновить список",
                Size = new Size(150, 35),
                Location = new Point(400, 580),
                BackColor = Color.FromArgb(0, 153, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshClientsBtn.Click += (s, e) => LoadClients();

            tab.Controls.AddRange(new Control[] { balanceGroup, clientsGrid, refreshClientsBtn });
        }

        private void LoadData()
        {
            LoadUnpaidBookings();
            LoadClients();
        }

        private void LoadUnpaidBookings()
        {
            var unpaidBookings = RowingBaseAccounting.BookingManager.GetUnpaidBookings(currentBase.Number);
            var clients = RowingBaseAccounting.ClientManager.GetClients();

            var displayData = unpaidBookings.Select(b => new
            {
                b.Id,
                Клиент = clients.FirstOrDefault(c => c.Id == b.ClientId)?.Name ?? "Неизвестный",
                Услуга = b.ServiceName,
                Начало = b.StartTime.ToString("dd.MM.yyyy HH:mm"),
                Стоимость = $"{b.TotalCost} BYN",
                Статус = b.Status
            }).ToList();

            unpaidBookingsGrid.DataSource = displayData;
        }

        private void LoadClients()
        {
            var clients = RowingBaseAccounting.ClientManager.GetClients()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Type,
                    c.Phone,
                    Баланс = $"{c.Balance} BYN"
                }).ToList();

            clientsGrid.DataSource = clients;
        }

        private void ConfirmPayment()
        {
            if (unpaidBookingsGrid.CurrentRow != null)
            {
                int bookingId = (int)unpaidBookingsGrid.CurrentRow.Cells["Id"].Value;
                string clientName = unpaidBookingsGrid.CurrentRow.Cells["Клиент"].Value.ToString();
                string serviceName = unpaidBookingsGrid.CurrentRow.Cells["Услуга"].Value.ToString();
                string cost = unpaidBookingsGrid.CurrentRow.Cells["Стоимость"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Подтвердить оплату бронирования?\n\n" +
                    $"Клиент: {clientName}\n" +
                    $"Услуга: {serviceName}\n" +
                    $"Стоимость: {cost}",
                    "Подтверждение оплаты",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (RowingBaseAccounting.BookingManager.ConfirmPayment(bookingId))
                    {
                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.BookingManager.GetBookings(), "bookings.txt");
                        LoadUnpaidBookings();
                        MessageBox.Show("✅ Оплата подтверждена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void AddBalanceToClient(RowingBaseAccounting.Client client, decimal amount)
        {
            if (client == null)
            {
                MessageBox.Show("Выберите клиента", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Пополнить баланс клиента {client.Name} на {amount} BYN?",
                "Подтверждение пополнения",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (RowingBaseAccounting.ClientManager.AddBalanceToClient(client.Id, amount))
                {
                    RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");
                    LoadClients();
                    MessageBox.Show($"✅ Баланс клиента {client.Name} пополнен на {amount} BYN!\nНовый баланс: {client.Balance} BYN",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("❌ Ошибка при пополнении баланса", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        } 
    }

        // Панель Менеджера
        public class ManagerPanelForm : Form
        {
            private Form1 mainForm;
            private RowingBaseAccounting.RowingBase currentBase;
            private DataGridView bookingsGrid;
            private DataGridView clientsGrid;
            private DataGridView cashiersGrid;
            private DataGridView clientsBalanceGrid;

        public ManagerPanelForm(Form1 mainForm, RowingBaseAccounting.RowingBase selectedBase)
        {
            this.mainForm = mainForm;
            this.currentBase = selectedBase;
            InitializeComponent();
            LoadData();
        }


        private void InitializeComponent()
            {
                this.Size = new Size(1000, 700);
                this.Text = $"👥 Панель менеджера - {currentBase.Name}";
                this.StartPosition = FormStartPosition.CenterScreen;
                this.BackColor = Color.White;

                // Главный TabControl
                TabControl tabControl = new TabControl();
                tabControl.Dock = DockStyle.Fill;
                tabControl.Padding = new Point(20, 10);

                // Вкладка управления бронированиями
                TabPage bookingsTab = new TabPage("📋 Управление бронированиями");
                InitializeBookingsTab(bookingsTab);

                // Вкладка управления аналитиками
                TabPage analystsTab = new TabPage("📊 Управление аналитиками");
                InitializeAnalystsTab(analystsTab);

                // Вкладка управления клиентами
                TabPage clientsTab = new TabPage("👥 Управление клиентами");
                InitializeClientsTab(clientsTab);

                // Вкладка баланса
                TabPage balanceTab = new TabPage("💰 Управление балансом");
                InitializeBalanceTab(balanceTab);

                tabControl.TabPages.Add(bookingsTab);
                tabControl.TabPages.Add(analystsTab);
                tabControl.TabPages.Add(clientsTab);
                tabControl.TabPages.Add(balanceTab);

                // Кнопка выхода
                Button logoutBtn = new Button()
                {
                    Text = "Выйти",
                    Size = new Size(80, 30),
                    Location = new Point(900, 3),
                    BackColor = Color.FromArgb(240, 240, 240),
                    FlatStyle = FlatStyle.Flat
                };
                logoutBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                logoutBtn.Click += (s, e) => { this.Close(); mainForm.Show(); };

                this.Controls.AddRange(new Control[] { logoutBtn, tabControl });
            }

            private void InitializeBookingsTab(TabPage tab)
            {
                tab.Padding = new Padding(10);

                // Панель инструментов
                ToolStrip toolStrip = new ToolStrip();
                toolStrip.Dock = DockStyle.Top;

                ToolStripButton refreshBtn = new ToolStripButton("Обновить");
                refreshBtn.Click += (s, e) => LoadBookings();

                ToolStripButton cancelBtn = new ToolStripButton("Отменить бронирование");
                cancelBtn.Click += (s, e) => CancelBooking();

                ToolStripButton viewDetailsBtn = new ToolStripButton("Детали бронирования");
                viewDetailsBtn.Click += (s, e) => ShowBookingDetails();

                toolStrip.Items.AddRange(new ToolStripItem[] { refreshBtn, cancelBtn, viewDetailsBtn });

                // Таблица бронирований
                bookingsGrid = new DataGridView();
                bookingsGrid.Dock = DockStyle.Fill;
                bookingsGrid.ReadOnly = true;
                bookingsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                bookingsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                bookingsGrid.CellDoubleClick += (s, e) =>
                {
                if (e.RowIndex >= 0)
                {
                    ShowBookingDetails(); // Показ деталей бронирования
                }
                };

            tab.Controls.AddRange(new Control[] { toolStrip, bookingsGrid });
            }

            private void InitializeAnalystsTab(TabPage tab)
            {
                tab.Padding = new Padding(10);

                // Панель инструментов
                ToolStrip toolStrip = new ToolStrip();
                toolStrip.Dock = DockStyle.Top;

                ToolStripButton refreshBtn = new ToolStripButton("Обновить");
                refreshBtn.Click += (s, e) => LoadAnalysts();

                ToolStripButton addBtn = new ToolStripButton("Добавить аналитика");
                addBtn.Click += (s, e) => AddAnalyst();

                ToolStripButton deactivateBtn = new ToolStripButton("Деактивировать");
                deactivateBtn.Click += (s, e) => DeactivateAnalyst();

                toolStrip.Items.AddRange(new ToolStripItem[] { refreshBtn, addBtn, deactivateBtn });

                // Таблица аналитиков
                cashiersGrid = new DataGridView();
                cashiersGrid.Dock = DockStyle.Fill;
                cashiersGrid.ReadOnly = true;
                cashiersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                cashiersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                tab.Controls.AddRange(new Control[] { toolStrip, cashiersGrid });
            }

            private void InitializeClientsTab(TabPage tab)
            {
                tab.Padding = new Padding(10);

                // Панель инструментов
                ToolStrip toolStrip = new ToolStrip();
                toolStrip.Dock = DockStyle.Top;

                ToolStripButton refreshBtn = new ToolStripButton("Обновить");
                refreshBtn.Click += (s, e) => LoadClients();

                ToolStripButton viewBtn = new ToolStripButton("Просмотр деталей");
                viewBtn.Click += (s, e) => ShowClientDetails();

                ToolStripButton discountBtn = new ToolStripButton("Изменить скидку");
                discountBtn.Click += (s, e) => ChangeClientDiscount();

                toolStrip.Items.AddRange(new ToolStripItem[] { refreshBtn, viewBtn, discountBtn });

                // Таблица клиентов
                clientsGrid = new DataGridView();
                clientsGrid.Dock = DockStyle.Fill;
                clientsGrid.ReadOnly = true;
                clientsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                clientsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                clientsGrid.CellDoubleClick += (s, e) =>
                {
                if (e.RowIndex >= 0)
                {
                    ShowClientDetails(); // Показ деталей клиента
                }
                 };

            tab.Controls.AddRange(new Control[] { toolStrip, clientsGrid });
            }

        private void InitializeBalanceTab(TabPage tab)
        {
            tab.Padding = new Padding(10);

            // Панель инструментов
            ToolStrip balanceToolStrip = new ToolStrip();
            balanceToolStrip.Dock = DockStyle.Top;

            ToolStripButton refreshBtn = new ToolStripButton("Обновить");
            refreshBtn.Click += (s, e) => LoadClientsBalance();

            ToolStripButton addBalanceBtn = new ToolStripButton("Пополнить баланс");
            addBalanceBtn.Click += (s, e) => AddBalanceToSelectedClient();

            balanceToolStrip.Items.AddRange(new ToolStripItem[] { refreshBtn, addBalanceBtn });

            // Таблица клиентов с балансом
            clientsBalanceGrid = new DataGridView();
            clientsBalanceGrid.Dock = DockStyle.Fill;
            clientsBalanceGrid.ReadOnly = true;
            clientsBalanceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            clientsBalanceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            clientsBalanceGrid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    AddBalanceToSelectedClient(); // Пополнение баланса
                }
            };

            tab.Controls.AddRange(new Control[] { balanceToolStrip, clientsBalanceGrid });
        }


        private void LoadData()
        {
            LoadBookings();
            LoadAnalysts();
            LoadClients();
            LoadClientsBalance(); // НОВЫЙ ВЫЗОВ
        }

        // НОВЫЙ МЕТОД: Загрузка данных о балансе клиентов
        private void LoadClientsBalance()
        {
            var clients = RowingBaseAccounting.ClientManager.GetClients()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Type,
                    c.Phone,
                    Баланс = $"{c.Balance} BYN",
                    c.RegistrationDate
                }).ToList();

            clientsBalanceGrid.DataSource = clients;
        }

        // НОВЫЙ МЕТОД: Пополнение баланса выбранного клиента
        private void AddBalanceToSelectedClient()
        {
            if (clientsBalanceGrid.CurrentRow != null)
            {
                int clientId = (int)clientsBalanceGrid.CurrentRow.Cells["Id"].Value;
                string clientName = clientsBalanceGrid.CurrentRow.Cells["Name"].Value.ToString();
                string currentBalance = clientsBalanceGrid.CurrentRow.Cells["Баланс"].Value.ToString().Replace(" BYN", "");

                string amountStr = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Пополнение баланса для {clientName}\nТекущий баланс: {currentBalance} BYN\n\nВведите сумму пополнения (BYN):",
                    "Пополнение баланса",
                    "100");

                if (decimal.TryParse(amountStr, out decimal amount) && amount > 0)
                {
                    if (RowingBaseAccounting.ClientManager.AddBalanceToClient(clientId, amount))
                    {
                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");
                        LoadClientsBalance();
                        MessageBox.Show($"Баланс клиента {clientName} пополнен на {amount} BYN", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (!string.IsNullOrEmpty(amountStr))
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadBookings()
            {
                var bookings = RowingBaseAccounting.BookingManager.GetBookings(currentBase.Number);
                var clients = RowingBaseAccounting.ClientManager.GetClients();

                var displayData = bookings.Select(b => new
                {
                    b.Id,
                    Клиент = clients.FirstOrDefault(c => c.Id == b.ClientId)?.Name ?? "Неизвестный",
                    Услуга = b.ServiceName,
                    Начало = b.StartTime.ToString("dd.MM.yyyy HH:mm"),
                    Конец = b.EndTime.ToString("dd.MM.yyyy HH:mm"),
                    Стоимость = $"{b.TotalCost:C2}",
                    Статус = b.Status
                }).ToList();

                bookingsGrid.DataSource = displayData;
            }

            private void LoadAnalysts()
            {
                var analysts = RowingBaseAccounting.EmployeeManager.GetEmployees()
                    .Where(e => e.Role == "Analyst")
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.Login,
                        e.Phone,
                        e.Email,
                        Статус = e.IsActive ? "Активен" : "Неактивен"
                    }).ToList();

                cashiersGrid.DataSource = analysts;
            }

            private void LoadClients()
            {
                var clients = RowingBaseAccounting.ClientManager.GetClients()
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Type,
                        c.Phone,
                        Скидка = $"{c.DiscountPercent}%",
                        Дата_регистрации = c.RegistrationDate.ToString("dd.MM.yyyy")
                    }).ToList();

                clientsGrid.DataSource = clients;
            }

            // === РЕАЛИЗОВАННЫЕ МЕТОДЫ ===

            public void ShowBookingManagement()
            {
                if (this.Controls[1] is TabControl tabControl)
                {
                    tabControl.SelectedIndex = 0;
                }
            }

            public void ShowAnalystManagement()
            {
                if (this.Controls[1] is TabControl tabControl)
                {
                    tabControl.SelectedIndex = 1;
                }
            }

            public void ShowClientManagement()
            {
                if (this.Controls[1] is TabControl tabControl)
                {
                    tabControl.SelectedIndex = 2;
                }
            }

            private void CancelBooking()
            {
                if (bookingsGrid.CurrentRow != null)
                {
                    int bookingId = (int)bookingsGrid.CurrentRow.Cells["Id"].Value;
                    string serviceName = bookingsGrid.CurrentRow.Cells["Услуга"].Value.ToString();
                    string clientName = bookingsGrid.CurrentRow.Cells["Клиент"].Value.ToString();

                    DialogResult result = MessageBox.Show(
                        $"Отменить бронирование?\n\nКлиент: {clientName}\nУслуга: {serviceName}",
                        "Подтверждение отмены",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (RowingBaseAccounting.BookingManager.AdminCancelBooking(bookingId))
                        {
                            RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.BookingManager.GetBookings(), "bookings.txt");
                            LoadBookings();
                            MessageBox.Show("Бронирование отменено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }

            private void ShowBookingDetails()
            {
                if (bookingsGrid.CurrentRow != null)
                {
                    int bookingId = (int)bookingsGrid.CurrentRow.Cells["Id"].Value;
                    var booking = RowingBaseAccounting.BookingManager.GetBookings().FirstOrDefault(b => b.Id == bookingId);
                    var client = RowingBaseAccounting.ClientManager.GetClient(booking.ClientId);

                    string details = $"Детали бронирования:\n\n" +
                                   $"ID: {booking.Id}\n" +
                                   $"Клиент: {client.Name}\n" +
                                   $"Услуга: {booking.ServiceName}\n" +
                                   $"Начало: {booking.StartTime:dd.MM.yyyy HH:mm}\n" +
                                   $"Окончание: {booking.EndTime:dd.MM.yyyy HH:mm}\n" +
                                   $"Стоимость: {booking.TotalCost:C2}\n" +
                                   $"Статус: {booking.Status}";

                    MessageBox.Show(details, "Детали бронирования", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            private void AddAnalyst()
            {
                using (Form addForm = new Form())
                {
                    addForm.Size = new Size(400, 350);
                    addForm.Text = "Добавление аналитика";
                    addForm.StartPosition = FormStartPosition.CenterParent;

                    TextBox nameTextBox = new TextBox() { Location = new Point(150, 30), Size = new Size(200, 25) };
                    TextBox loginTextBox = new TextBox() { Location = new Point(150, 70), Size = new Size(200, 25) };
                    TextBox passwordTextBox = new TextBox() { Location = new Point(150, 110), Size = new Size(200, 25), UseSystemPasswordChar = true };
                    TextBox phoneTextBox = new TextBox() { Location = new Point(150, 150), Size = new Size(200, 25) };
                    TextBox emailTextBox = new TextBox() { Location = new Point(150, 190), Size = new Size(200, 25) };

                    Button saveBtn = new Button()
                    {
                        Text = "Сохранить",
                        Location = new Point(150, 240),
                        Size = new Size(100, 35),
                        DialogResult = DialogResult.OK
                    };

                    saveBtn.Click += (s, e) =>
                    {
                        if (string.IsNullOrEmpty(nameTextBox.Text) || string.IsNullOrEmpty(loginTextBox.Text))
                        {
                            MessageBox.Show("Заполните обязательные поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        var analyst = new RowingBaseAccounting.Employee
                        {
                            Name = nameTextBox.Text,
                            Login = loginTextBox.Text,
                            Password = passwordTextBox.Text,
                            Role = "Analyst",
                            Phone = phoneTextBox.Text,
                            Email = emailTextBox.Text,
                            IsActive = true
                        };

                        RowingBaseAccounting.EmployeeManager.AddEmployee(analyst);
                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.EmployeeManager.GetEmployees(), "employees.txt");
                        LoadAnalysts();
                        MessageBox.Show("Аналитик добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };

                    addForm.Controls.AddRange(new Control[] {
                    new Label() { Text = "ФИО*:", Location = new Point(50, 30), Size = new Size(100, 25) },
                    nameTextBox,
                    new Label() { Text = "Логин*:", Location = new Point(50, 70), Size = new Size(100, 25) },
                    loginTextBox,
                    new Label() { Text = "Пароль:", Location = new Point(50, 110), Size = new Size(100, 25) },
                    passwordTextBox,
                    new Label() { Text = "Телефон:", Location = new Point(50, 150), Size = new Size(100, 25) },
                    phoneTextBox,
                    new Label() { Text = "Email:", Location = new Point(50, 190), Size = new Size(100, 25) },
                    emailTextBox,
                    saveBtn
                });

                    addForm.ShowDialog();
                }
            }

            private void DeactivateAnalyst()
            {
                if (cashiersGrid.CurrentRow != null)
                {
                    int analystId = (int)cashiersGrid.CurrentRow.Cells["Id"].Value;
                    string analystName = cashiersGrid.CurrentRow.Cells["Name"].Value.ToString();

                    DialogResult result = MessageBox.Show(
                        $"Деактивировать аналитика {analystName}?",
                        "Подтверждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        var analyst = RowingBaseAccounting.EmployeeManager.GetEmployee(analystId);
                        if (analyst != null)
                        {
                            analyst.IsActive = false;
                            RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.EmployeeManager.GetEmployees(), "employees.txt");
                            LoadAnalysts();
                            MessageBox.Show("Аналитик деактивирован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }

            private void ShowClientDetails()
            {
                if (clientsGrid.CurrentRow != null)
                {
                    int clientId = (int)clientsGrid.CurrentRow.Cells["Id"].Value;
                    var client = RowingBaseAccounting.ClientManager.GetClient(clientId);

                    string details = $"Детали клиента:\n\n" +
                                   $"ID: {client.Id}\n" +
                                   $"ФИО: {client.Name}\n" +
                                   $"Тип: {client.Type}\n" +
                                   $"Телефон: {client.Phone}\n" +
                                   $"Email: {client.Email ?? "не указан"}\n" +
                                   $"Скидка: {client.DiscountPercent}%\n" +
                                   $"Дата регистрации: {client.RegistrationDate:dd.MM.yyyy}";

                    if (client.Type == "Юр.лицо")
                    {
                        details += $"\nКомпания: {client.CompanyName}\nИНН: {client.TaxId}";
                    }

                    MessageBox.Show(details, "Детали клиента", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            private void ChangeClientDiscount()
            {
                if (clientsGrid.CurrentRow != null)
                {
                    int clientId = (int)clientsGrid.CurrentRow.Cells["Id"].Value;
                    string clientName = clientsGrid.CurrentRow.Cells["Name"].Value.ToString();
                    string currentDiscount = clientsGrid.CurrentRow.Cells["Скидка"].Value.ToString().Replace("%", "");

                    string newDiscount = Microsoft.VisualBasic.Interaction.InputBox(
                        $"Установить скидку для клиента {clientName}:",
                        "Изменение скидки",
                        currentDiscount);

                    if (decimal.TryParse(newDiscount, out decimal discount) && discount >= 0 && discount <= 100)
                    {
                        RowingBaseAccounting.ClientManager.SetClientDiscount(clientId, discount);
                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");
                        LoadClients();
                        MessageBox.Show("Скидка обновлена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        // Панель администратора 
        public class AdminPanelForm : Form
        {
            private Form1 mainForm;
            private RowingBaseAccounting.RowingBase currentBase;
            private DataGridView bookingsGrid;
            private DataGridView servicesGrid;
            private DataGridView clientsGrid;
            private DataGridView staffGrid; // Добавлено новое поле
            private DataGridView clientsBalanceGrid;
            private MenuStrip mainMenu;
            private StatusStrip statusStrip;
            private ToolStrip toolStrip;
            private TabControl tabControl;

                public AdminPanelForm(Form1 mainForm, RowingBaseAccounting.RowingBase selectedBase)
                {
                    this.mainForm = mainForm;
                    this.currentBase = selectedBase;
                    InitializeComponent();
                    LoadAllData();
                    UpdateStatusStrip();
                }


        private void InitializeComponent()
            {
                this.Size = new Size(1200, 800);
                this.Text = $"⚙️ Панель администратора - {currentBase.Name}";
                this.StartPosition = FormStartPosition.CenterScreen;
                this.BackColor = Color.White;

                // ОСНОВНОЙ КОНТЕЙНЕР - TabControl должен быть ПЕРВЫМ и занимать ВСЁ пространство
                tabControl = new TabControl();
                tabControl.Dock = DockStyle.Fill;
                tabControl.Padding = new Point(20, 10);

                // Создаем вкладки
                TabPage bookingsTab = new TabPage("📋 Бронирования");
                InitializeBookingsTab(bookingsTab);

                TabPage servicesTab = new TabPage("🎯 Услуги");
                InitializeServicesTab(servicesTab);

                TabPage clientsTab = new TabPage("👥 Клиенты");
                InitializeClientsTab(clientsTab);

                TabPage staffManagementTab = new TabPage("📋 Управление персоналом");
                InitializeStaffManagementTab(staffManagementTab);

                TabPage balanceTab = new TabPage("💰 Управление балансом");
                InitializeBalanceTab(balanceTab);

                tabControl.TabPages.Add(bookingsTab);
                tabControl.TabPages.Add(servicesTab);
                tabControl.TabPages.Add(clientsTab);
                tabControl.TabPages.Add(staffManagementTab);
                tabControl.TabPages.Add(balanceTab);

                
                this.Controls.Add(tabControl);

                // Затем создаем остальные элементы
                CreateMainMenu();
                CreateStatusStrip();

                // Добавляем остальные элементы ПОСЛЕ TabControl
                this.Controls.Add(mainMenu);
                this.Controls.Add(toolStrip);
                this.Controls.Add(statusStrip);

            }

            private void CreateMainMenu()
            {
                mainMenu = new MenuStrip();
                mainMenu.Dock = DockStyle.Top;

                // Меню "Файл"
                ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
                fileMenu.DropDownItems.Add("Экспорт данных", null, (s, e) => ExportData());
                fileMenu.DropDownItems.Add("Импорт данных", null, (s, e) => ImportData());
                fileMenu.DropDownItems.Add("-");
                fileMenu.DropDownItems.Add("Выйти", null, (s, e) => { this.Close(); mainForm.Show(); });

                // Меню "Действия"
                ToolStripMenuItem actionsMenu = new ToolStripMenuItem("Действия");
                actionsMenu.DropDownItems.Add("Обновить все данные", null, (s, e) => LoadAllData());
                actionsMenu.DropDownItems.Add("Создать резервную копию", null, (s, e) => CreateBackup());

                // Меню "Справка"
                ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");
                helpMenu.DropDownItems.Add("Руководство администратора", null, (s, e) => ShowAdminHelp());
                helpMenu.DropDownItems.Add("О программе", null, (s, e) => ShowAbout());

                mainMenu.Items.AddRange(new ToolStripMenuItem[] { fileMenu, actionsMenu, helpMenu });
            }


            private void CreateStatusStrip()
            {
                statusStrip = new StatusStrip();
                statusStrip.Dock = DockStyle.Bottom;

                ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();
                statusLabel.Spring = true;

                ToolStripStatusLabel timeLabel = new ToolStripStatusLabel();
                timeLabel.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

                statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, timeLabel });
            }

            private void InitializeBookingsTab(TabPage tab)
            {
                tab.Padding = new Padding(10);

                // Панель инструментов для бронирований
                ToolStrip bookingsToolStrip = new ToolStrip();
                bookingsToolStrip.Dock = DockStyle.Top;

                ToolStripButton refreshBookingsBtn = new ToolStripButton("Обновить");
                refreshBookingsBtn.Click += (s, e) => LoadBookings();

                ToolStripButton cancelBookingBtn = new ToolStripButton("Отменить");
                cancelBookingBtn.Click += (s, e) => CancelSelectedBooking();

                ToolStripButton deleteBookingBtn = new ToolStripButton("Удалить");
                deleteBookingBtn.Click += (s, e) => DeleteSelectedBooking();

                ToolStripButton editBookingBtn = new ToolStripButton("Изменить");
                editBookingBtn.Click += (s, e) => EditSelectedBooking();

                bookingsToolStrip.Items.AddRange(new ToolStripItem[] {
        refreshBookingsBtn, cancelBookingBtn, deleteBookingBtn, editBookingBtn
    });

                // Таблица бронирований
                bookingsGrid = new DataGridView();
                bookingsGrid.Dock = DockStyle.Fill;
                bookingsGrid.ReadOnly = true;
                bookingsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                bookingsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                bookingsGrid.RowHeadersVisible = false;
                bookingsGrid.BackgroundColor = SystemColors.Window;
                bookingsGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                bookingsGrid.CellDoubleClick += (s, e) =>
                {
                if (e.RowIndex >= 0)
                {
                    ShowBookingDetails(); // Показ деталей бронирования
                }
                };

            // Контекстное меню
            ContextMenuStrip contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Отменить бронирование", null, (s, e) => CancelSelectedBooking());
                contextMenu.Items.Add("Удалить бронирование", null, (s, e) => DeleteSelectedBooking());
                contextMenu.Items.Add("Изменить время", null, (s, e) => EditSelectedBooking());
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Информация о клиенте", null, (s, e) => ShowClientInfo());
                bookingsGrid.ContextMenuStrip = contextMenu;

                tab.Controls.AddRange(new Control[] { bookingsToolStrip, bookingsGrid });
            }
        private void ShowBookingDetails()
        {
            if (bookingsGrid.CurrentRow != null)
            {
                int bookingId = (int)bookingsGrid.CurrentRow.Cells["Id"].Value;
                var booking = RowingBaseAccounting.BookingManager.GetBookings().FirstOrDefault(b => b.Id == bookingId);
                var client = RowingBaseAccounting.ClientManager.GetClient(booking.ClientId);

                string details = $"Детали бронирования:\n\n" +
                               $"ID: {booking.Id}\n" +
                               $"Клиент: {client.Name}\n" +
                               $"Услуга: {booking.ServiceName}\n" +
                               $"Начало: {booking.StartTime:dd.MM.yyyy HH:mm}\n" +
                               $"Окончание: {booking.EndTime:dd.MM.yyyy HH:mm}\n" +
                               $"Стоимость: {booking.TotalCost:C2}\n" +
                               $"Статус: {booking.Status}";

                MessageBox.Show(details, "Детали бронирования", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeServicesTab(TabPage tab)
            {
                tab.Padding = new Padding(10);

                // Панель инструментов для услуг
                ToolStrip servicesToolStrip = new ToolStrip();
                servicesToolStrip.Dock = DockStyle.Top;

                ToolStripButton refreshServicesBtn = new ToolStripButton("Обновить");
                refreshServicesBtn.Click += (s, e) => LoadServices();

                ToolStripButton addServiceBtn = new ToolStripButton("Добавить");
                addServiceBtn.Click += (s, e) => AddNewService();

                ToolStripButton editServiceBtn = new ToolStripButton("Редактировать");
                editServiceBtn.Click += (s, e) => EditSelectedService();

                ToolStripButton deleteServiceBtn = new ToolStripButton("Удалить");
                deleteServiceBtn.Click += (s, e) => DeleteSelectedService();

                servicesToolStrip.Items.AddRange(new ToolStripItem[] {
        refreshServicesBtn, addServiceBtn, editServiceBtn, deleteServiceBtn
    });

                // Таблица услуг
                servicesGrid = new DataGridView();
                servicesGrid.Dock = DockStyle.Fill;
                servicesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                servicesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                servicesGrid.RowHeadersVisible = false;
                servicesGrid.BackgroundColor = SystemColors.Window;
            servicesGrid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    EditSelectedService(); // Редактирование услуги
                }
            };

            tab.Controls.AddRange(new Control[] { servicesToolStrip, servicesGrid });
            }

            private void InitializeClientsTab(TabPage tab)
            {
                tab.Padding = new Padding(10);

                // Панель инструментов для клиентов
                ToolStrip clientsToolStrip = new ToolStrip();
                clientsToolStrip.Dock = DockStyle.Top;

                ToolStripButton refreshClientsBtn = new ToolStripButton("Обновить");
                refreshClientsBtn.Click += (s, e) => LoadClients();

                ToolStripButton addClientBtn = new ToolStripButton("Добавить клиента");
                addClientBtn.Click += (s, e) => AddNewClient();

                ToolStripButton deleteClientBtn = new ToolStripButton("Удалить клиента");
                deleteClientBtn.Click += (s, e) => DeleteSelectedClient();

                clientsToolStrip.Items.AddRange(new ToolStripItem[] { refreshClientsBtn, addClientBtn, deleteClientBtn });

                // Таблица клиентов
                clientsGrid = new DataGridView();
                clientsGrid.Dock = DockStyle.Fill;
                clientsGrid.ReadOnly = true;
                clientsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                clientsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                clientsGrid.RowHeadersVisible = false;
                clientsGrid.BackgroundColor = SystemColors.Window;
            clientsGrid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    ShowClientDetails(); // Показ деталей клиента
                }
            };

            // Контекстное меню для клиентов
            ContextMenuStrip clientContextMenu = new ContextMenuStrip();
                clientContextMenu.Items.Add("Просмотреть детали", null, (s, e) => ShowClientDetails());
                clientContextMenu.Items.Add("Изменить скидку", null, (s, e) => ChangeClientDiscount());
                clientsGrid.ContextMenuStrip = clientContextMenu;

                tab.Controls.AddRange(new Control[] { clientsToolStrip, clientsGrid });
            }


            // НОВЫЙ МЕТОД: Инициализация вкладки управления персоналом с таблицей
            private void InitializeStaffManagementTab(TabPage tab)
            {
                tab.Padding = new Padding(10);
                Button manageAccountsBtn = new Button()
                {
                    Text = "Управление аккаунтами",
                    Location = new Point(50, 400),
                    Size = new Size(150, 35),
                    BackColor = Color.FromArgb(0, 153, 255),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                manageAccountsBtn.Click += (s, e) => ManageAccounts();

                Button showStaffListBtn = new Button()
                {
                    Text = "Список сотрудников",
                    Location = new Point(220, 400),
                    Size = new Size(150, 35),
                    BackColor = Color.FromArgb(76, 175, 80),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                showStaffListBtn.Click += (s, e) => ShowStaffList();

                Button generateReportBtn = new Button()
                {
                    Text = "Отчет по персоналу",
                    Location = new Point(390, 400),
                    Size = new Size(150, 35),
                    BackColor = Color.FromArgb(255, 193, 7),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                generateReportBtn.Click += (s, e) => GenerateStaffReport();

                // Панель инструментов
                ToolStrip staffToolStrip = new ToolStrip();
                staffToolStrip.Dock = DockStyle.Top;

                ToolStripButton refreshBtn = new ToolStripButton("Обновить");
                refreshBtn.Click += (s, e) => LoadStaff();

                ToolStripButton addBtn = new ToolStripButton("Добавить");
                addBtn.Click += (s, e) => AddStaffMember();

                ToolStripButton editBtn = new ToolStripButton("Редактировать");
                editBtn.Click += (s, e) => EditSelectedStaff();

                ToolStripButton deactivateBtn = new ToolStripButton("Деактивировать");
                deactivateBtn.Click += (s, e) => DeactivateSelectedStaff();

                staffToolStrip.Items.AddRange(new ToolStripItem[] {
        refreshBtn, addBtn, editBtn, deactivateBtn
    });

                // Таблица сотрудников
                staffGrid = new DataGridView();
                staffGrid.Dock = DockStyle.Fill;
                staffGrid.ReadOnly = true;
                staffGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                staffGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                staffGrid.CellDoubleClick += (s, e) =>
                 {
                if (e.RowIndex >= 0)
                {
                    EditSelectedStaff(); // Редактирование сотрудника
                }
                 };


            tab.Controls.AddRange(new Control[] { staffToolStrip, staffGrid });
            }

            // НОВЫЙ МЕТОД: Редактирование выбранного сотрудника
            private void EditSelectedStaff()
            {
                if (staffGrid?.CurrentRow != null)
                {
                    string staffName = staffGrid.CurrentRow.Cells["Name"].Value?.ToString();
                    if (!string.IsNullOrEmpty(staffName))
                    {
                        var employee = RowingBaseAccounting.EmployeeManager.GetEmployees()
                            .FirstOrDefault(e => e.Name == staffName);

                        if (employee != null)
                        {
                            using (Form editForm = new Form())
                            {
                                editForm.Size = new Size(400, 400);
                                editForm.Text = "Редактирование сотрудника";
                                editForm.StartPosition = FormStartPosition.CenterParent;

                                TextBox nameTextBox = new TextBox()
                                {
                                    Location = new Point(150, 30),
                                    Size = new Size(200, 25),
                                    Text = employee.Name
                                };
                                TextBox loginTextBox = new TextBox()
                                {
                                    Location = new Point(150, 70),
                                    Size = new Size(200, 25),
                                    Text = employee.Login
                                };
                                ComboBox roleComboBox = new ComboBox()
                                {
                                    Location = new Point(150, 110),
                                    Size = new Size(200, 25),
                                    DataSource = new[] { "Кассир", "Менеджер", "Администратор" },
                                    DropDownStyle = ComboBoxStyle.DropDownList,
                                    SelectedItem = employee.Role
                                };
                                TextBox phoneTextBox = new TextBox()
                                {
                                    Location = new Point(150, 150),
                                    Size = new Size(200, 25),
                                    Text = employee.Phone
                                };
                                TextBox emailTextBox = new TextBox()
                                {
                                    Location = new Point(150, 190),
                                    Size = new Size(200, 25),
                                    Text = employee.Email
                                };
                                CheckBox activeCheckBox = new CheckBox()
                                {
                                    Location = new Point(150, 230),
                                    Size = new Size(200, 25),
                                    Text = "Активен",
                                    Checked = employee.IsActive
                                };

                                Button saveBtn = new Button()
                                {
                                    Text = "Сохранить",
                                    Location = new Point(150, 280),
                                    Size = new Size(100, 35),
                                    DialogResult = DialogResult.OK
                                };

                                Button cancelBtn = new Button()
                                {
                                    Text = "Отмена",
                                    Location = new Point(260, 280),
                                    Size = new Size(100, 35),
                                    DialogResult = DialogResult.Cancel
                                };

                                editForm.Controls.AddRange(new Control[] {
                            new Label() { Text = "ФИО:", Location = new Point(50, 30), Size = new Size(100, 25) },
                            nameTextBox,
                            new Label() { Text = "Логин:", Location = new Point(50, 70), Size = new Size(100, 25) },
                            loginTextBox,
                            new Label() { Text = "Роль:", Location = new Point(50, 110), Size = new Size(100, 25) },
                            roleComboBox,
                            new Label() { Text = "Телефон:", Location = new Point(50, 150), Size = new Size(100, 25) },
                            phoneTextBox,
                            new Label() { Text = "Email:", Location = new Point(50, 190), Size = new Size(100, 25) },
                            emailTextBox,
                            activeCheckBox,
                            saveBtn,
                            cancelBtn
                        });

                                editForm.AcceptButton = saveBtn;
                                editForm.CancelButton = cancelBtn;

                                if (editForm.ShowDialog() == DialogResult.OK)
                                {
                                    employee.Name = nameTextBox.Text;
                                    employee.Login = loginTextBox.Text;
                                    employee.Role = roleComboBox.SelectedItem.ToString();
                                    employee.Phone = phoneTextBox.Text;
                                    employee.Email = emailTextBox.Text;
                                    employee.IsActive = activeCheckBox.Checked;

                                    RowingBaseAccounting.FileManager.SaveToFile(
                                        RowingBaseAccounting.EmployeeManager.GetEmployees(), "employees.txt");
                                    LoadStaff();
                                    MessageBox.Show("Данные сотрудника обновлены", "Успех",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите сотрудника для редактирования", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // НОВЫЙ МЕТОД: Деактивация выбранного сотрудника
            private void DeactivateSelectedStaff()
            {
                if (staffGrid?.CurrentRow != null)
                {
                    string staffName = staffGrid.CurrentRow.Cells["Name"].Value?.ToString();
                    if (!string.IsNullOrEmpty(staffName))
                    {
                        var employee = RowingBaseAccounting.EmployeeManager.GetEmployees()
                            .FirstOrDefault(e => e.Name == staffName);

                        if (employee != null)
                        {
                            if (employee.Role == "Admin")
                            {
                                MessageBox.Show("Нельзя деактивировать администратора системы", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            DialogResult result = MessageBox.Show(
                                $"Вы уверены, что хотите {(employee.IsActive ? "деактивировать" : "активировать")} сотрудника {employee.Name}?",
                                "Подтверждение",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                            {
                                employee.IsActive = !employee.IsActive;
                                RowingBaseAccounting.FileManager.SaveToFile(
                                    RowingBaseAccounting.EmployeeManager.GetEmployees(), "employees.txt");
                                LoadStaff();
                                MessageBox.Show($"Сотрудник {employee.Name} {(employee.IsActive ? "активирован" : "деактивирован")}",
                                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите сотрудника", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

        private void InitializeBalanceTab(TabPage tab)
        {
            tab.Padding = new Padding(10);

            // Панель инструментов
            ToolStrip balanceToolStrip = new ToolStrip();
            balanceToolStrip.Dock = DockStyle.Top;

            ToolStripButton refreshBtn = new ToolStripButton("Обновить");
            refreshBtn.Click += (s, e) => LoadClientsBalance();

            ToolStripButton addBalanceBtn = new ToolStripButton("Пополнить баланс");
            addBalanceBtn.Click += (s, e) => AddBalanceToSelectedClient();

            ToolStripButton setBalanceBtn = new ToolStripButton("Установить баланс");
            setBalanceBtn.Click += (s, e) => SetClientBalance();

            balanceToolStrip.Items.AddRange(new ToolStripItem[] { refreshBtn, addBalanceBtn, setBalanceBtn });

            // Таблица клиентов с балансом
            clientsBalanceGrid = new DataGridView();
            clientsBalanceGrid.Dock = DockStyle.Fill;
            clientsBalanceGrid.ReadOnly = true;
            clientsBalanceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            clientsBalanceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            clientsBalanceGrid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    AddBalanceToSelectedClient(); // Пополнение баланса
                }
            };

            tab.Controls.AddRange(new Control[] { balanceToolStrip, clientsBalanceGrid });
        }

        private void AddBalanceToSelectedClient()
        {
            if (clientsBalanceGrid.CurrentRow != null)
            {
                int clientId = (int)clientsBalanceGrid.CurrentRow.Cells["Id"].Value;
                string clientName = clientsBalanceGrid.CurrentRow.Cells["Name"].Value.ToString();
                decimal currentBalance = (decimal)clientsBalanceGrid.CurrentRow.Cells["Текущий_баланс"].Value;

                string amountStr = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Пополнение баланса для {clientName}\nТекущий баланс: {currentBalance} BYN\n\nВведите сумму пополнения (BYN):",
                    "Пополнение баланса",
                    "100");

                if (decimal.TryParse(amountStr, out decimal amount) && amount > 0)
                {
                    if (RowingBaseAccounting.ClientManager.AddBalanceToClient(clientId, amount))
                    {
                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");
                        LoadClientsBalance();
                        MessageBox.Show($"✅ Баланс клиента {clientName} пополнен на {amount} BYN\nНовый баланс: {currentBalance + amount} BYN",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (!string.IsNullOrEmpty(amountStr))
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // НОВЫЙ МЕТОД: Установка конкретного значения баланса
        private void SetClientBalance()
        {
            if (clientsBalanceGrid.CurrentRow != null)
            {
                int clientId = (int)clientsBalanceGrid.CurrentRow.Cells["Id"].Value;
                string clientName = clientsBalanceGrid.CurrentRow.Cells["Name"].Value.ToString();
                decimal currentBalance = (decimal)clientsBalanceGrid.CurrentRow.Cells["Текущий_баланс"].Value;

                string newBalanceStr = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Установка баланса для {clientName}\nТекущий баланс: {currentBalance} BYN\n\nВведите новое значение баланса (BYN):",
                    "Установка баланса",
                    currentBalance.ToString());

                if (decimal.TryParse(newBalanceStr, out decimal newBalance) && newBalance >= 0)
                {
                    var client = RowingBaseAccounting.ClientManager.GetClient(clientId);
                    if (client != null)
                    {
                        // Для установки точного значения баланса
                        decimal difference = newBalance - client.Balance;
                        if (difference > 0)
                        {
                            RowingBaseAccounting.ClientManager.AddBalanceToClient(clientId, difference);
                        }
                        else if (difference < 0)
                        {
                            // Списание средств (только для администратора)
                            client.Balance = newBalance;
                        }

                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");
                        LoadClientsBalance();
                        MessageBox.Show($"✅ Баланс клиента {clientName} установлен: {newBalance} BYN",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (!string.IsNullOrEmpty(newBalanceStr))
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        // === ОСНОВНЫЕ МЕТОДЫ ДАННЫХ ===

        private void LoadAllData()
        {
            LoadBookings();
            LoadServices();
            LoadClients();
            LoadStaff();
            LoadClientsBalance(); // НОВЫЙ ВЫЗОВ
            UpdateStatusStrip();
        }

        // НОВЫЙ МЕТОД: Загрузка данных о балансе клиентов
        private void LoadClientsBalance()
        {
            var clients = RowingBaseAccounting.ClientManager.GetClients()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Type,
                    c.Phone,
                    Текущий_баланс = c.Balance,
                    Баланс = $"{c.Balance} BYN",
                    c.RegistrationDate
                }).ToList();

            clientsBalanceGrid.DataSource = clients;
        }

        private void UpdateStatusStrip()
            {
                var bookings = RowingBaseAccounting.BookingManager.GetBookings(currentBase.Number);
                int activeCount = bookings.Count(b => b.Status == "Активно");
                int totalCount = bookings.Count;

                if (statusStrip.Items.Count > 0)
                {
                    statusStrip.Items[0].Text = $"Администратор | Активных бронированных: {activeCount} | Всего: {totalCount}";
                    statusStrip.Items[1].Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                }
            }

            private void LoadBookings()
            {
                var bookings = RowingBaseAccounting.BookingManager.GetBookings(currentBase.Number);
                var clients = RowingBaseAccounting.ClientManager.GetClients();

                var displayData = bookings.Select(b => new
                {
                    b.Id,
                    Клиент = clients.FirstOrDefault(c => c.Id == b.ClientId)?.Name ?? "Неизвестный",
                    Услуга = b.ServiceName,
                    Начало = b.StartTime.ToString("dd.MM.yyyy HH:mm"),
                    Конец = b.EndTime.ToString("dd.MM.yyyy HH:mm"),
                    Стоимость = $"{b.TotalCost:C2}",
                    Статус = b.Status
                }).ToList();

                bookingsGrid.DataSource = displayData;
            }

            private void LoadServices()
            {
                var services = RowingBaseAccounting.ServiceManager.GetServices(currentBase.Number);
                var displayData = services.Select(s => new
                {
                    s.Name,
                    s.Price,
                    s.Unit,
                    s.ServiceType,
                    s.Description,
                    Только_для_юр_лиц = s.IsForLegalEntitiesOnly ? "Да" : "Нет",
                    Вместимость = s.Capacity > 0 ? s.Capacity.ToString() : "-"
                }).ToList();

                servicesGrid.DataSource = displayData;
            }

            private void LoadClients()
            {
                var clients = RowingBaseAccounting.ClientManager.GetClients();
                var displayData = clients.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Type,
                    c.Phone,
                    c.Email,
                    Скидка = $"{c.DiscountPercent}%",
                    Дата_регистрации = c.RegistrationDate.ToString("dd.MM.yyyy")
                }).ToList();

                clientsGrid.DataSource = displayData;
            }

            // НОВЫЙ МЕТОД: Загрузка данных сотрудников
            private void LoadStaff()
            {
                var staff = RowingBaseAccounting.EmployeeManager.GetEmployees();
                var displayData = staff.Select(e => new
                {
                    e.Id,
                    e.Login,
                    e.Name,
                    e.Role,
                    e.Phone,
                    e.Email,
                    e.HireDate,
                    Статус = e.IsActive ? "Активен" : "Неактивен"
                }).ToList();

                // Если staffGrid существует, обновляем его
                if (staffGrid != null)
                {
                    staffGrid.DataSource = displayData;
                }
            }

            // === МЕТОДЫ ДЛЯ КЛИЕНТОВ ===

            private void AddNewClient()
            {
                VisitorRegistrationForm registrationForm = new VisitorRegistrationForm();
                if (registrationForm.ShowDialog() == DialogResult.OK)
                {
                    LoadClients();
                    MessageBox.Show("Клиент успешно добавлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            private void DeleteSelectedClient()
            {
                if (clientsGrid.CurrentRow != null)
                {
                    int clientId = (int)clientsGrid.CurrentRow.Cells["Id"].Value;
                    string clientName = clientsGrid.CurrentRow.Cells["Name"].Value.ToString();

                    DialogResult result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить клиента {clientName}?",
                        "Подтверждение удаления",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        var client = RowingBaseAccounting.ClientManager.GetClient(clientId);
                        if (client != null)
                        {
                            RowingBaseAccounting.ClientManager.GetClients().Remove(client);
                            RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");
                            LoadClients();
                            MessageBox.Show("Клиент удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }

            private void ShowClientDetails()
            {
                if (clientsGrid.CurrentRow != null)
                {
                    int clientId = (int)clientsGrid.CurrentRow.Cells["Id"].Value;
                    var client = RowingBaseAccounting.ClientManager.GetClient(clientId);

                    if (client != null)
                    {
                        string clientInfo = $"Детальная информация о клиенте:\n\n" +
                                          $"ID: {client.Id}\n" +
                                          $"ФИО: {client.Name}\n" +
                                          $"Тип: {client.Type}\n" +
                                          $"Телефон: {client.Phone ?? "не указан"}\n" +
                                          $"Email: {client.Email ?? "не указан"}\n" +
                                          $"Скидка: {client.DiscountPercent}%\n" +
                                          $"Дата регистрации: {client.RegistrationDate:dd.MM.yyyy}";

                        if (client.Type == "Юр.лицо")
                        {
                            clientInfo += $"\nКомпания: {client.CompanyName}\nИНН: {client.TaxId}";
                        }

                        // Бронирования клиента
                        var clientBookings = RowingBaseAccounting.BookingManager.GetClientBookings(client.Id);
                        clientInfo += $"\n\nАктивные бронирования: {clientBookings.Count}";

                        MessageBox.Show(clientInfo, "Детали клиента", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }

            private void ChangeClientDiscount()
            {
                if (clientsGrid.CurrentRow != null)
                {
                    int clientId = (int)clientsGrid.CurrentRow.Cells["Id"].Value;
                    string clientName = clientsGrid.CurrentRow.Cells["Name"].Value.ToString();
                    string currentDiscount = clientsGrid.CurrentRow.Cells["Скидка"].Value.ToString().Replace("%", "");

                    string newDiscount = Microsoft.VisualBasic.Interaction.InputBox(
                        $"Установить скидку для клиента {clientName}:",
                        "Изменение скидки",
                        currentDiscount);

                    if (decimal.TryParse(newDiscount, out decimal discount) && discount >= 0 && discount <= 100)
                    {
                        RowingBaseAccounting.ClientManager.SetClientDiscount(clientId, discount);
                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");
                        LoadClients();
                        MessageBox.Show("Скидка обновлена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (!string.IsNullOrEmpty(newDiscount))
                    {
                        MessageBox.Show("Введите корректное значение скидки (0-100%)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            // === МЕТОДЫ ДЛЯ БРОНИРОВАНИЙ ===

            private void CancelSelectedBooking()
            {
                if (bookingsGrid.CurrentRow != null)
                {
                    int bookingId = (int)bookingsGrid.CurrentRow.Cells["Id"].Value;
                    string serviceName = bookingsGrid.CurrentRow.Cells["Услуга"].Value.ToString();
                    string clientName = bookingsGrid.CurrentRow.Cells["Клиент"].Value.ToString();

                    DialogResult result = MessageBox.Show(
                        $"Вы уверены, что хотите ОТМЕНИТЬ бронирование?\n\n" +
                        $"Клиент: {clientName}\n" +
                        $"Услуга: {serviceName}\n\n" +
                        $"Бронирование будет помечено как отмененное, но останется в системе.",
                        "ПОДТВЕРЖДЕНИЕ ОТМЕНЫ",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (RowingBaseAccounting.BookingManager.AdminCancelBooking(bookingId))
                        {
                            RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.BookingManager.GetBookings(), "bookings.txt");
                            LoadBookings();
                            MessageBox.Show("✅ Бронирование отменено", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }

            private void DeleteSelectedBooking()
            {
                if (bookingsGrid.CurrentRow != null)
                {
                    int bookingId = (int)bookingsGrid.CurrentRow.Cells["Id"].Value;
                    string serviceName = bookingsGrid.CurrentRow.Cells["Услуга"].Value.ToString();
                    string clientName = bookingsGrid.CurrentRow.Cells["Клиент"].Value.ToString();
                    string status = bookingsGrid.CurrentRow.Cells["Статус"].Value.ToString();

                    // Проверяем, можно ли удалять бронирование
                    if (status == "Активно")
                    {
                        MessageBox.Show("❌ Нельзя удалять активные бронирования!\n\n" +
                                      "Сначала отмените бронирование, затем удалите его.",
                                      "Ошибка удаления",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                        return;
                    }

                    DialogResult result = MessageBox.Show(
                        $"❌ ВНИМАНИЕ: ВЫ ПОЛНОСТЬЮ УДАЛЯЕТЕ БРОНИРОВАНИЕ ИЗ СИСТЕМЫ!\n\n" +
                        $"Клиент: {clientName}\n" +
                        $"Услуга: {serviceName}\n" +
                        $"Статус: {status}\n\n" +
                        $"Это действие невозможно отменить. Вы уверены, что хотите удалить это бронирование?",
                        "ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        if (RowingBaseAccounting.BookingManager.DeleteBooking(bookingId))
                        {
                            RowingBaseAccounting.FileManager.SaveToFile(
                                RowingBaseAccounting.BookingManager.GetBookings(), "bookings.txt");
                            LoadBookings();
                            MessageBox.Show("✅ Бронирование полностью удалено из системы", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("❌ Не удалось удалить бронирование", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

            private void EditSelectedBooking()
            {
                if (bookingsGrid.CurrentRow != null)
                {
                    int bookingId = (int)bookingsGrid.CurrentRow.Cells["Id"].Value;
                    string serviceName = bookingsGrid.CurrentRow.Cells["Услуга"].Value.ToString();
                    string clientName = bookingsGrid.CurrentRow.Cells["Клиент"].Value.ToString();
                    DateTime currentTime = DateTime.Parse(bookingsGrid.CurrentRow.Cells["Начало"].Value.ToString());

                    // Форма для изменения времени
                    Form editForm = new Form();
                    editForm.Size = new Size(400, 250);
                    editForm.Text = "Изменение бронирования";
                    editForm.StartPosition = FormStartPosition.CenterParent;

                    Label infoLabel = new Label()
                    {
                        Text = $"Клиент: {clientName}\nУслуга: {serviceName}",
                        Location = new Point(20, 20),
                        Size = new Size(350, 40)
                    };

                    Label dateLabel = new Label() { Text = "Новое время:", Location = new Point(20, 80), Size = new Size(100, 25) };
                    DateTimePicker newTimePicker = new DateTimePicker()
                    {
                        Location = new Point(120, 80),
                        Size = new Size(200, 25),
                        Value = currentTime,
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "dd.MM.yyyy HH:mm"
                    };

                    Label hoursLabel = new Label() { Text = "Часы:", Location = new Point(20, 120), Size = new Size(100, 25) };
                    NumericUpDown hoursNumeric = new NumericUpDown()
                    {
                        Location = new Point(120, 120),
                        Size = new Size(60, 25),
                        Minimum = 1,
                        Maximum = 10,
                        Value = 2
                    };

                    Button saveBtn = new Button()
                    {
                        Text = "Сохранить",
                        Location = new Point(150, 160),
                        Size = new Size(100, 30),
                        DialogResult = DialogResult.OK
                    };

                    Button cancelBtn = new Button()
                    {
                        Text = "Отмена",
                        Location = new Point(260, 160),
                        Size = new Size(100, 30),
                        DialogResult = DialogResult.Cancel
                    };

                    editForm.Controls.AddRange(new Control[] {
            infoLabel, dateLabel, newTimePicker, hoursLabel, hoursNumeric, saveBtn, cancelBtn
        });

                    editForm.AcceptButton = saveBtn;
                    editForm.CancelButton = cancelBtn;

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        RowingBaseAccounting.BookingManager.UpdateBooking(bookingId, newTimePicker.Value, (double)hoursNumeric.Value);
                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.BookingManager.GetBookings(), "bookings.txt");
                        LoadBookings();
                        MessageBox.Show("Бронирование обновлено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите бронирование для изменения", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            private void ShowClientInfo()
            {
                if (bookingsGrid.CurrentRow != null)
                {
                    string clientName = bookingsGrid.CurrentRow.Cells["Клиент"].Value.ToString();
                    var client = RowingBaseAccounting.ClientManager.GetClients()
                        .FirstOrDefault(c => c.Name == clientName);

                    if (client != null)
                    {
                        string clientInfo = $"Информация о клиенте:\n\n" +
                                          $"ФИО: {client.Name}\n" +
                                          $"Тип: {client.Type}\n" +
                                          $"Телефон: {client.Phone ?? "не указан"}\n" +
                                          $"Email: {client.Email ?? "не указан"}\n" +
                                          $"Скидка: {client.DiscountPercent}%\n" +
                                          $"Дата регистрации: {client.RegistrationDate:dd.MM.yyyy}";

                        if (client.Type == "Юр.лицо")
                        {
                            clientInfo += $"\nКомпания: {client.CompanyName}\nИНН: {client.TaxId}";
                        }

                        MessageBox.Show(clientInfo, "Информация о клиенте", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }

            // === МЕТОДЫ ДЛЯ УСЛУГ ===

            private void AddNewService()
            {
                Form addForm = new Form();
                addForm.Size = new Size(400, 500);
                addForm.Text = "Добавить новую услугу";

                TextBox nameTextBox = new TextBox() { Location = new Point(150, 30), Size = new Size(200, 25) };
                NumericUpDown priceNumeric = new NumericUpDown() { Location = new Point(150, 70), Size = new Size(100, 25), Minimum = 0, Maximum = 10000 };
                TextBox unitTextBox = new TextBox() { Location = new Point(150, 110), Size = new Size(100, 25), Text = "час" };
                TextBox descTextBox = new TextBox() { Location = new Point(150, 150), Size = new Size(200, 25) };
                ComboBox typeComboBox = new ComboBox()
                {
                    Location = new Point(150, 190),
                    Size = new Size(150, 25),
                    DataSource = new[] { "Тренажерный зал", "Площадка", "Терраса", "Сауна", "Бассейн", "Абонемент", "Хранение" }
                };
                CheckBox legalOnlyCheck = new CheckBox() { Location = new Point(150, 230), Size = new Size(200, 25), Text = "Только для юр.лиц" };
                NumericUpDown capacityNumeric = new NumericUpDown() { Location = new Point(150, 270), Size = new Size(100, 25), Minimum = 0, Maximum = 100 };

                Button saveBtn = new Button() { Text = "Сохранить", Location = new Point(150, 320), Size = new Size(100, 35) };
                saveBtn.Click += (s, e) =>
                {
                    MessageBox.Show("Функция добавления услуги будет реализована позже");
                    addForm.Close();
                };

                addForm.Controls.AddRange(new Control[] {
        new Label() { Text = "Название:", Location = new Point(50, 30), Size = new Size(100, 25) },
        nameTextBox,
        new Label() { Text = "Цена, BYN:", Location = new Point(50, 70), Size = new Size(100, 25) },
        priceNumeric,
        new Label() { Text = "Единица:", Location = new Point(50, 110), Size = new Size(100, 25) },
        unitTextBox,
        new Label() { Text = "Описание:", Location = new Point(50, 150), Size = new Size(100, 25) },
        descTextBox,
        new Label() { Text = "Тип услуги:", Location = new Point(50, 190), Size = new Size(100, 25) },
        typeComboBox,
        legalOnlyCheck,
        new Label() { Text = "Вместимость:", Location = new Point(50, 270), Size = new Size(100, 25) },
        capacityNumeric,
        saveBtn
    });

                addForm.ShowDialog();
            }

            private void EditSelectedService()
            {
                if (servicesGrid.CurrentRow != null)
                {
                    string serviceName = servicesGrid.CurrentRow.Cells["Name"].Value.ToString();
                    var service = RowingBaseAccounting.ServiceManager.GetService(serviceName, currentBase.Number);

                    if (service != null)
                    {
                        string newPrice = Microsoft.VisualBasic.Interaction.InputBox("Новая цена:", "Изменение цены", service.Price.ToString());
                        if (decimal.TryParse(newPrice, out decimal price))
                        {
                            RowingBaseAccounting.ServiceManager.UpdateServicePrice(serviceName, currentBase.Number, price);
                            LoadServices();
                            MessageBox.Show("Цена обновлена!");
                        }
                    }
                }
            }

            private void DeleteSelectedService()
            {
                if (servicesGrid.CurrentRow != null)
                {
                    string serviceName = servicesGrid.CurrentRow.Cells["Name"].Value.ToString();
                    DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить услугу '{serviceName}'?",
                        "Подтверждение удаления", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        MessageBox.Show("Функция удаления услуги будет реализована позже");
                    }
                }
            }

            // === МЕТОДЫ ДЛЯ ПЕРСОНАЛА ===

            private void AddStaffMember()
            {
                using (Form addStaffForm = new Form())
                {
                    addStaffForm.Size = new Size(500, 450);
                    addStaffForm.Text = "Добавление сотрудника";
                    addStaffForm.StartPosition = FormStartPosition.CenterParent;
                    addStaffForm.BackColor = Color.White;
                    addStaffForm.FormBorderStyle = FormBorderStyle.FixedDialog;

                    // Поля формы
                    TextBox nameTextBox = new TextBox() { Location = new Point(150, 30), Size = new Size(200, 25) };
                    TextBox loginTextBox = new TextBox() { Location = new Point(150, 70), Size = new Size(200, 25) };
                    TextBox passwordTextBox = new TextBox() { Location = new Point(150, 110), Size = new Size(200, 25), UseSystemPasswordChar = true };
                    ComboBox roleComboBox = new ComboBox()
                    {
                        Location = new Point(150, 150),
                        Size = new Size(200, 25),
                        DataSource = new[] { "Кассир", "Менеджер" }, // Убрал Администратора - его нельзя создавать
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    TextBox emailTextBox = new TextBox() { Location = new Point(150, 190), Size = new Size(200, 25) };
                    TextBox phoneTextBox = new TextBox() { Location = new Point(150, 230), Size = new Size(200, 25) };

                    Button saveBtn = new Button()
                    {
                        Text = "Сохранить",
                        Location = new Point(150, 280),
                        Size = new Size(100, 35),
                        BackColor = Color.FromArgb(76, 175, 80),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    Button cancelBtn = new Button()
                    {
                        Text = "Отмена",
                        Location = new Point(260, 280),
                        Size = new Size(100, 35),
                        BackColor = Color.FromArgb(240, 240, 240),
                        FlatStyle = FlatStyle.Flat
                    };

                    saveBtn.Click += (s, e) =>
                    {
                        if (string.IsNullOrEmpty(nameTextBox.Text) || string.IsNullOrEmpty(loginTextBox.Text) || string.IsNullOrEmpty(passwordTextBox.Text))
                        {
                            MessageBox.Show("Заполните обязательные поля: ФИО, Логин и Пароль", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Проверяем, не существует ли уже сотрудник с таким логином
                        var existingEmployee = RowingBaseAccounting.EmployeeManager.GetEmployees()
                            .FirstOrDefault(e => e.Login == loginTextBox.Text);

                        if (existingEmployee != null)
                        {
                            MessageBox.Show("Сотрудник с таким логином уже существует", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Создаем нового сотрудника
                        var newEmployee = new RowingBaseAccounting.Employee
                        {
                            Name = nameTextBox.Text,
                            Login = loginTextBox.Text,
                            Password = passwordTextBox.Text,
                            Role = roleComboBox.SelectedItem.ToString(),
                            Email = emailTextBox.Text,
                            Phone = phoneTextBox.Text,
                            IsActive = true
                        };

                        RowingBaseAccounting.EmployeeManager.AddEmployee(newEmployee);
                        RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.EmployeeManager.GetEmployees(), "employees.txt");
                        LoadStaff();

                        MessageBox.Show("Сотрудник успешно добавлен в систему!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        addStaffForm.DialogResult = DialogResult.OK;
                    };

                    cancelBtn.Click += (s, e) =>
                    {
                        addStaffForm.DialogResult = DialogResult.Cancel;
                    };

                    addStaffForm.Controls.AddRange(new Control[] {
            new Label() { Text = "ФИО*:", Location = new Point(50, 30), Size = new Size(100, 25) },
            nameTextBox,
            new Label() { Text = "Логин*:", Location = new Point(50, 70), Size = new Size(100, 25) },
            loginTextBox,
            new Label() { Text = "Пароль*:", Location = new Point(50, 110), Size = new Size(100, 25) },
            passwordTextBox,
            new Label() { Text = "Роль:", Location = new Point(50, 150), Size = new Size(100, 25) },
            roleComboBox,
            new Label() { Text = "Email:", Location = new Point(50, 190), Size = new Size(100, 25) },
            emailTextBox,
            new Label() { Text = "Телефон:", Location = new Point(50, 230), Size = new Size(100, 25) },
            phoneTextBox,
            saveBtn,
            cancelBtn
        });

                    addStaffForm.ShowDialog();
                }
            }

            private void ManageAccounts()
            {
                using (Form manageForm = new Form())
                {
                    manageForm.Size = new Size(700, 500);
                    manageForm.Text = "Управление аккаунтами персонала";
                    manageForm.StartPosition = FormStartPosition.CenterParent;
                    manageForm.BackColor = Color.White;

                    // Заголовок
                    Label titleLabel = new Label()
                    {
                        Text = "Список сотрудников",
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        Location = new Point(20, 20),
                        Size = new Size(300, 30),
                        ForeColor = Color.FromArgb(0, 102, 204)
                    };

                    // Таблица сотрудников
                    DataGridView staffGrid = new DataGridView()
                    {
                        Location = new Point(20, 60),
                        Size = new Size(640, 300),
                        ReadOnly = true,
                        BackgroundColor = SystemColors.Window
                    };

                    // Заполняем таблицу данными из EmployeeManager
                    var staffData = RowingBaseAccounting.EmployeeManager.GetEmployees()
                        .Select(e => new
                        {
                            ФИО = e.Name,
                            Должность = e.Role,
                            Логин = e.Login,
                            Статус = e.IsActive ? "Активен" : "Неактивен"
                        }).ToArray();

                    staffGrid.DataSource = staffData;

                    // Кнопки управления
                    Button editBtn = new Button()
                    {
                        Text = "✏️ Редактировать",
                        Size = new Size(150, 35),
                        Location = new Point(20, 380),
                        BackColor = Color.FromArgb(0, 153, 255),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    Button deactivateBtn = new Button()
                    {
                        Text = "🚫 Деактивировать",
                        Size = new Size(150, 35),
                        Location = new Point(180, 380),
                        BackColor = Color.FromArgb(255, 87, 34),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    Button resetPasswordBtn = new Button()
                    {
                        Text = "🔑 Сбросить пароль",
                        Size = new Size(150, 35),
                        Location = new Point(340, 380),
                        BackColor = Color.FromArgb(255, 193, 7),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    Button closeBtn = new Button()
                    {
                        Text = "Закрыть",
                        Size = new Size(100, 35),
                        Location = new Point(560, 380),
                        BackColor = Color.FromArgb(240, 240, 240),
                        FlatStyle = FlatStyle.Flat
                    };

                    editBtn.Click += (s, e) =>
                    {
                        if (staffGrid.CurrentRow != null)
                            MessageBox.Show($"Редактирование сотрудника: {staffGrid.CurrentRow.Cells[0].Value}", "Редактирование");
                    };

                    deactivateBtn.Click += (s, e) =>
                    {
                        if (staffGrid.CurrentRow != null)
                            MessageBox.Show($"Деактивация сотрудника: {staffGrid.CurrentRow.Cells[0].Value}", "Деактивация");
                    };

                    resetPasswordBtn.Click += (s, e) =>
                    {
                        if (staffGrid.CurrentRow != null)
                        {
                            string staffName = staffGrid.CurrentRow.Cells[0].Value.ToString();
                            var employee = RowingBaseAccounting.EmployeeManager.GetEmployees()
                                .FirstOrDefault(e => e.Name == staffName);

                            if (employee != null)
                            {
                                string newPassword = Microsoft.VisualBasic.Interaction.InputBox(
                                    $"Введите новый пароль для {staffName}:",
                                    "Сброс пароля",
                                    "");

                                if (!string.IsNullOrEmpty(newPassword))
                                {
                                    RowingBaseAccounting.EmployeeManager.ChangePassword(employee.Id, newPassword);
                                    RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.EmployeeManager.GetEmployees(), "employees.txt");
                                    MessageBox.Show("Пароль успешно изменен", "Успех",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    };

                    closeBtn.Click += (s, e) => manageForm.Close();

                    manageForm.Controls.AddRange(new Control[] {
            titleLabel, staffGrid, editBtn, deactivateBtn, resetPasswordBtn, closeBtn
        });

                    manageForm.ShowDialog();
                }
            }

            private void ShowStaffList()
            {
                var staff = RowingBaseAccounting.EmployeeManager.GetEmployees();
                string staffList = "Список сотрудников:\n\n";

                foreach (var employee in staff)
                {
                    staffList += $"{employee.Name} ({employee.Role}) - {employee.Login} - {(employee.IsActive ? "Активен" : "Неактивен")}\n";
                }

                MessageBox.Show(staffList, "Список сотрудников",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            private void GenerateStaffReport()
            {
                var staff = RowingBaseAccounting.EmployeeManager.GetEmployees();
                int totalStaff = staff.Count;
                int activeStaff = staff.Count(e => e.IsActive);
                int managers = staff.Count(e => e.Role == "Manager");
                int analysts = staff.Count(e => e.Role == "Analyst");
                int admins = staff.Count(e => e.Role == "Admin");

                string report = $"ОТЧЕТ ПО ПЕРСОНАЛУ\n\n" +
                               $"Всего сотрудников: {totalStaff}\n" +
                               $"Активных: {activeStaff}\n" +
                               $"Неактивных: {totalStaff - activeStaff}\n\n" +
                               $"По должностям:\n" +
                               $"• Администраторы: {admins}\n" +
                               $"• Менеджеры: {managers}\n" +
                               $"• Аналитики: {analysts}";

                MessageBox.Show(report, "Отчет по персоналу",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // === СТАТИСТИКА И ОТЧЕТЫ ===

            private void GenerateStatistics(TextBox statsTextBox, DateTime startDate, DateTime endDate)
            {
                var bookings = RowingBaseAccounting.BookingManager.GetBookingsByPeriod(startDate, endDate.AddDays(1), currentBase.Number);
                var services = RowingBaseAccounting.ServiceManager.GetServices(currentBase.Number);
                var clients = RowingBaseAccounting.ClientManager.GetClients();

                decimal totalRevenue = bookings.Sum(b => b.TotalCost);
                int totalBookings = bookings.Count;
                int activeClients = bookings.Select(b => b.ClientId).Distinct().Count();

                statsTextBox.Text = $"СТАТИСТИЧЕСКИЙ ОТЧЕТ\n";
                statsTextBox.Text += $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}\n";
                statsTextBox.Text += "".PadLeft(50, '=') + "\n\n";

                statsTextBox.Text += $"ОБЩАЯ СТАТИСТИКА:\n";
                statsTextBox.Text += $"Выручка: {totalRevenue:C2}\n";
                statsTextBox.Text += $"Количество бронирований: {totalBookings}\n";
                statsTextBox.Text += $"Уникальных клиентов: {activeClients}\n";
                statsTextBox.Text += $"Средний чек: {(totalBookings > 0 ? totalRevenue / totalBookings : 0):C2}\n\n";

                statsTextBox.Text += $"СТАТИСТИКА ПО УСЛУГАМ:\n";
                var serviceStats = bookings
                    .GroupBy(b => b.ServiceName)
                    .Select(g => new
                    {
                        Service = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(b => b.TotalCost),
                        Percentage = totalBookings > 0 ? (g.Count() * 100.0 / totalBookings) : 0
                    })
                    .OrderByDescending(x => x.Revenue);

                foreach (var stat in serviceStats)
                {
                    statsTextBox.Text += $"{stat.Service.PadRight(35)}: {stat.Count} бронир. ({stat.Percentage:F1}%) - {stat.Revenue:C2}\n";
                }

                statsTextBox.Text += $"\nСТАТИСТИКА ПО ТИПАМ КЛИЕНТОВ:\n";
                var clientTypeStats = bookings
                    .Join(clients, b => b.ClientId, c => c.Id, (b, c) => new { b.TotalCost, c.Type })
                    .GroupBy(x => x.Type)
                    .Select(g => new { Type = g.Key, Count = g.Count(), Revenue = g.Sum(x => x.TotalCost) });

                foreach (var stat in clientTypeStats)
                {
                    statsTextBox.Text += $"{stat.Type.PadRight(15)}: {stat.Count} бронир. - {stat.Revenue:C2}\n";
                }
            }

            // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

            private void ExportData()
            {
                MessageBox.Show("Функция экспорта данных будет реализована в следующей версии",
                              "В разработке", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            private void ImportData()
            {
                MessageBox.Show("Функция импорта данных будет реализована в следующей версии",
                              "В разработке", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            private void CreateBackup()
            {
                try
                {
                    // Сохраняем все данные
                    RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.ClientManager.GetClients(), "clients.txt");
                    RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.BookingManager.GetBookings(), "bookings.txt");
                    RowingBaseAccounting.FileManager.SaveToFile(RowingBaseAccounting.EmployeeManager.GetEmployees(), "employees.txt");

                    MessageBox.Show("Резервная копия создана успешно",
                                  "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании резервной копии: {ex.Message}",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private void ShowAdminHelp()
            {
                MessageBox.Show(
                    "📋 Руководство администратора:\n\n" +
                    "• Управление бронированиями - просмотр, отмена, удаление\n" +
                    "• Управление услугами - добавление, редактирование, удаление\n" +
                    "• Управление клиентами - просмотр информации о клиентах\n" +
                    "• Управление персоналом - добавление, редактирование сотрудников\n" +
                    "• Статистика - анализ работы базы\n\n" +
                    "Для экстренной помощи: 8-029-692-70-05",
                    "Руководство администратора",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Question);
            }

            private void ShowAbout()
            {
                MessageBox.Show(
                    "Система управления гребными базами\n\n" +
                    "Версия 2.0 - Панель администратора\n" +
                    "Контактная информация:\n" +
                    "Гребная база №2\n" +
                    "г.Пинск, ул. Иркутско-Пинской дивизии, 46\n" +
                    "Тел: 8-029-692-70-05",
                    "О программе",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        } 
}