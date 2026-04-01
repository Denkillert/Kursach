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
        private Panel contentPanel;
        private Panel menuPanel;
        private Panel headerPanel;
        private DataGridView currentDgv;
        private string currentTableType;
        private TextBox txtSearch;
        private DateTimePicker dtpDate;
        private Panel filterPanel;

        public AdminForm()
        {
            InitializeComponent();
            this.Text = "ПолесГУ Спорт — Панель администратора";
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
            CreateMenuButton("🎓 Студенты", 140, () => ShowGrid("Студенты"));
            CreateMenuButton("🏆 Тренеры", 275, () => ShowGrid("Тренеры"));
            CreateMenuButton("⚽ Секции", 410, () => ShowGrid("Секции"));
            CreateMenuButton("📋 Посещаемость", 545, () => ShowGrid("Посещаемость"));

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

            //  КАРТОЧКИ СТАТИСТИКИ 
            FlowLayoutPanel statsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10),
                Height = 180,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            AddCard(statsPanel, "🏛️ Факультеты", Count("SELECT COUNT(*) FROM Faculties"), Color.FromArgb(0, 122, 204));
            AddCard(statsPanel, "👨‍🏫 Тренеры", Count("SELECT COUNT(*) FROM Trainers"), Color.FromArgb(40, 167, 69));
            AddCard(statsPanel, "🎓 Студенты", Count("SELECT COUNT(*) FROM Students"), Color.FromArgb(255, 193, 7));
            AddCard(statsPanel, "⚽ Секции", Count("SELECT COUNT(*) FROM Sections"), Color.FromArgb(220, 53, 69));
            AddCard(statsPanel, "📋 Посещаемость", Count("SELECT COUNT(*) FROM Attendance"), Color.Gray);

            // КНОПКИ
            FlowLayoutPanel buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(30),
                BackColor = Color.White
            };
                       
            Button btnFaculty = new Button
            {
                Text = "🏛️ Управление факультетами",
                Size = new Size(200, 70),
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            btnFaculty.Click += (s, e) => new PolesSU_Sports.Admin.Forms.Dictionary.FacultyForm().ShowDialog();
            buttonsPanel.Controls.Add(btnFaculty);

            Button btnSport = new Button
            {
                Text = "⚽ Управление видами спорта",
                Size = new Size(200, 70),
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            btnSport.Click += (s, e) => new PolesSU_Sports.Admin.Forms.Dictionary.SportForm().ShowDialog();
            buttonsPanel.Controls.Add(btnSport);

            Button btnSchedule = new Button
            {
                Text = "📅 Управление расписанием",
                Size = new Size(200, 70),
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            btnSchedule.Click += (s, e) => new PolesSU_Sports.Admin.Forms.Dictionary.ScheduleForm().ShowDialog();
            buttonsPanel.Controls.Add(btnSchedule);

            
            Button btnReports = new Button
            {
                Text = "🔍 Создать отчет",
                Size = new Size(200, 70),
                Margin = new Padding(10),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnReports.Click += (s, e) => new PolesSU_Sports.Admin.Report.ReportForm().ShowDialog();
            buttonsPanel.Controls.Add(btnReports);

            
            Button btnAccounts = new Button
            {
                Text = "🔐 Управление аккаунтами",
                Size = new Size(200, 70),
                Margin = new Padding(10),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAccounts.Click += (s, e) => new PolesSU_Sports.Admin.Forms.Dictionary.AccountForm().ShowDialog();
            buttonsPanel.Controls.Add(btnAccounts);
            
            Button btnAchievements = new Button
            {
                Text = "🏆 Достижения студентов",
                Size = new Size(200, 70),
                Margin = new Padding(10),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
           
            Button btnEnrollment = new Button
            {
                Text = "📋 Запись в секции",
                Size = new Size(200, 70),
                Margin = new Padding(10),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnEnrollment.Click += (s, e) => new PolesSU_Sports.Admin.Students.StudentSectionForm().ShowDialog();
            buttonsPanel.Controls.Add(btnEnrollment);
            btnAchievements.Click += (s, e) => ShowAchievements();
            buttonsPanel.Controls.Add(btnAchievements);

            contentPanel.Controls.Add(buttonsPanel);
            contentPanel.Controls.Add(statsPanel);
        }

        private void AddCard(FlowLayoutPanel p, string title, string val, Color c)
        {
            Panel card = new Panel
            {
                Size = new Size(180, 130),
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

        // ============================================================ ТАБЛИЦЫ С ПОИСКОМ И CRUD
        private void ShowGrid(string tableType)
        {
            currentTableType = tableType;
            contentPanel.Controls.Clear();

            // ПАНЕЛЬ ПОИСКА
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            Label lblSearch = new Label
            {
                Text = "🔍 Поиск:",
                Location = new Point(10, 12),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 9)
            };

            txtSearch = new TextBox
            {
                Location = new Point(70, 10),
                Size = new Size(250, 23),
                Font = new Font("Microsoft Sans Serif", 9),
                PlaceholderText = "Введите для поиска..."
            };
            txtSearch.TextChanged += (s, e) => ApplySearch();

            Button btnClearSearch = new Button
            {
                Text = "❌ Очистить",
                Location = new Point(330, 9),
                Size = new Size(80, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 8)
            };
            btnClearSearch.Click += (s, e) => { txtSearch.Clear(); ApplySearch(); };

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnClearSearch });

            // ПАНЕЛЬ ФИЛЬТРОВ (для посещаемости - ОДНА ДАТА)
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(250, 250, 250),
                Visible = (tableType == "Посещаемость")
            };

            if (tableType == "Посещаемость")
            {
                Label lblDate = new Label
                {
                    Text = "📅 Дата посещения:",
                    Location = new Point(10, 15),
                    AutoSize = true,
                    Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold)
                };

                dtpDate = new DateTimePicker
                {
                    Location = new Point(160, 12),
                    Size = new Size(150, 23),
                    Format = DateTimePickerFormat.Short,
                    Font = new Font("Microsoft Sans Serif", 9)
                };
                dtpDate.Value = DateTime.Now;

                Button btnLoadDate = new Button
                {
                    Text = "📋 Загрузить",
                    Location = new Point(320, 10),
                    Size = new Size(100, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 9)
                };
                btnLoadDate.Click += (s, e) => LoadGridData();

                Button btnToday = new Button
                {
                    Text = "📅 Сегодня",
                    Location = new Point(430, 10),
                    Size = new Size(90, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 167, 69),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 9)
                };
                btnToday.Click += (s, e) => { dtpDate.Value = DateTime.Now; LoadGridData(); };

                filterPanel.Controls.AddRange(new Control[] { lblDate, dtpDate, btnLoadDate, btnToday });
            }

            // ПАНЕЛЬ КНОПОК 
            Panel btnPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            Button btnRefresh = CreateCrudButton("🔄 Обновить", 10, () => LoadGridData());

            // Для посещаемости - другие кнопки
            if (tableType == "Посещаемость")
            {
                Button btnAddAttendance = CreateCrudButton("➕ Отметить", 120, OnAddAttendance, Color.FromArgb(40, 167, 69));
                Button btnEditAttendance = CreateCrudButton("✏️ Изменить", 230, OnEditAttendance);
                Button btnDeleteAttendance = CreateCrudButton("🗑️ Удалить", 340, OnDeleteAttendance, Color.FromArgb(220, 53, 69));

                btnPanel.Controls.AddRange(new Control[] { btnRefresh, btnAddAttendance, btnEditAttendance, btnDeleteAttendance });
            }
            else
            {
                Button btnAdd = CreateCrudButton("➕ Добавить", 120, OnAdd, Color.FromArgb(40, 167, 69));
                Button btnEdit = CreateCrudButton("✏️ Изменить", 230, OnEdit);
                Button btnDelete = CreateCrudButton("🗑️ Удалить", 340, OnDelete, Color.FromArgb(220, 53, 69));

                btnPanel.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnEdit, btnDelete });
            }

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
                    Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold)
                }
            };

            contentPanel.Controls.Add(currentDgv);
            contentPanel.Controls.Add(btnPanel);
            contentPanel.Controls.Add(filterPanel);
            contentPanel.Controls.Add(searchPanel);

            LoadGridData();
        }

        private Button CreateCrudButton(string text, int x, Action click, Color? bgColor = null)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, 10),
                Size = new Size(110, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 9),
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
                btn.ForeColor = Color.Black;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            }
            btn.Click += (s, e) => click();
            return btn;
        }

        // ============================================================ ПОИСК
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

        // ============================================================ ЗАГРУЗКА ДАННЫХ
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
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================================ CRUD - ОБЫЧНЫЕ ТАБЛИЦЫ
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
                    default:
                        MessageBox.Show("Добавление не реализовано", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    default:
                        MessageBox.Show("Редактирование не реализовано", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // ============================================================ CRUD - ПОСЕЩАЕМОСТЬ
        private void OnAddAttendance()
        {
            // Открываем форму выбора студентов для отметки посещения
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
            contentPanel.Controls.Clear();

            Panel btnPanel = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };

            Button btnAdd = new Button { Text = "➕ Добавить достижение", Location = new Point(10, 10), Size = new Size(180, 30) };
            btnAdd.Click += (s, e) =>
            {
                using (var form = new PolesSU_Sports.Admin.Forms.Dictionary.Achievements.AchievementForm())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                        LoadAchievements();
                }
            };

            btnPanel.Controls.Add(btnAdd);

            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                BackgroundColor = Color.White
            };

            contentPanel.Controls.Add(dgv);
            contentPanel.Controls.Add(btnPanel);

            LoadAchievements();
        }

        private void LoadAchievements()
        {
            DataGridView dgv = null;
            foreach (Control c in contentPanel.Controls)
            {
                if (c is DataGridView) { dgv = c as DataGridView; break; }
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

        // ============================================================ РЕСАЙЗ
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (headerPanel != null)
            {
                headerPanel.Size = new Size(this.Width, 60);
                foreach (Control c in headerPanel.Controls)
                    if (c is Button btn && btn.Text.Contains("Выйти"))
                        btn.Location = new Point(this.Width - 120, 15);
            }
            if (menuPanel != null)
            {
                menuPanel.Location = new Point(0, 63);
                menuPanel.Size = new Size(this.Width, 50);
            }
            if (contentPanel != null)
            {
                contentPanel.Location = new Point(0, 116);
                contentPanel.Size = new Size(this.Width, this.Height - 116);
            }
        }
    }
}