using PolesSU_Sports.Lib.DB;
using System;
using System.Data;
using System.Windows.Forms;

namespace PolesSU_Sports.Admin.Report
{
    public partial class ReportForm : Form
    {
        private ComboBox cmbReportType;
        private ComboBox cmbFaculty;
        private DateTimePicker dtpDateFrom;
        private DateTimePicker dtpDateTo;
        private Button btnGenerate;
        private Button btnExport;
        private Button btnCancel;
        private DataGridView dgvReport;
        private Label lblResultCount;
        private Panel pnlDates;

        public ReportForm()
        {
            InitializeComponent();
            LoadFaculties();
        }

        private void InitializeComponent()
        {
            this.Text = "Генератор отчётов";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Панель фильтров
            Panel pnlFilters = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10)
            };

            int y = 15;
            int labelWidth = 120;
            int inputWidth = 200;
            int inputX = 140;

            // Тип отчёта
            CreateLabel("Тип отчёта:", 20, y, labelWidth, pnlFilters);
            cmbReportType = new ComboBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbReportType.Items.AddRange(new[] {
                "📊 Студенты по факультетам",
                "👨‍ Тренеры по квалификации",
                "⚽ Секции по видам спорта",
                "📋 Посещаемость за период",
                "🏆 Достижения студентов"
            });
            cmbReportType.SelectedIndex = 0;
            pnlFilters.Controls.Add(cmbReportType);
            y += 40;

            // Факультет (только для некоторых отчётов)
            CreateLabel("Факультет:", 20, y, labelWidth, pnlFilters);
            cmbFaculty = new ComboBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            cmbFaculty.SelectedIndexChanged += (s, e) => btnGenerate.Enabled = cmbFaculty.SelectedValue != null;
            pnlFilters.Controls.Add(cmbFaculty);
            y += 40;

            // Период (для посещаемости)
            CreateLabel("Период:", 20, y, labelWidth, pnlFilters);
            pnlDates = new Panel
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth * 2 + 20, 23)
            };
            dtpDateFrom = new DateTimePicker
            {
                Location = new System.Drawing.Point(0, 0),
                Size = new System.Drawing.Size(inputWidth, 23),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(-1)
            };
            dtpDateTo = new DateTimePicker
            {
                Location = new System.Drawing.Point(inputWidth + 10, 0),
                Size = new System.Drawing.Size(inputWidth, 23),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            pnlDates.Controls.AddRange(new Control[] { dtpDateFrom, dtpDateTo });
            pnlFilters.Controls.Add(pnlDates);

            // Кнопки
            Panel pnlButtons = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10)
            };

            btnGenerate = new Button
            {
                Text = "🔍 Сформировать",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(130, 30),
                BackColor = System.Drawing.Color.FromArgb(0, 86, 179),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerate.Click += BtnGenerate_Click;

            btnExport = new Button
            {
                Text = "📤 Экспорт в Excel",
                Location = new System.Drawing.Point(150, 10),
                Size = new System.Drawing.Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnExport.Click += BtnExport_Click;

            btnCancel = new Button
            {
                Text = "❌ Закрыть",
                Location = new System.Drawing.Point(310, 10),
                Size = new System.Drawing.Size(100, 30),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();

            pnlButtons.Controls.AddRange(new Control[] { btnGenerate, btnExport, btnCancel });

            // DataGridView для предпросмотра
            dgvReport = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = System.Drawing.Color.White
            };

            // Счётчик записей
            lblResultCount = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Text = "Записей: 0",
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
            };

            // Сборка формы
            this.Controls.Add(dgvReport);
            this.Controls.Add(lblResultCount);
            this.Controls.Add(pnlButtons);
            this.Controls.Add(pnlFilters);

            // Подписка на изменение типа отчёта
            cmbReportType.SelectedIndexChanged += CmbReportType_SelectedIndexChanged;
        }

        private Label CreateLabel(string text, int x, int y, int width, Control parent)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private void LoadFaculties()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT FacultyID, FacultyName FROM Faculties ORDER BY FacultyName");

                // Создаём список с элементом "Все факультеты"
                var facultyList = new List<ComboBoxItem>
        {
            new ComboBoxItem { Text = "Все факультеты", Value = null }
        };

                // Добавляем факультеты из БД
                foreach (DataRow row in dt.Rows)
                {
                    facultyList.Add(new ComboBoxItem
                    {
                        Text = row["FacultyName"].ToString(),
                        Value = row["FacultyID"]
                    });
                }

                cmbFaculty.DataSource = facultyList;
                cmbFaculty.DisplayMember = "Text";
                cmbFaculty.ValueMember = "Value";
                cmbFaculty.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки факультетов: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Включаем/отключаем фильтры в зависимости от типа отчёта
            string reportType = cmbReportType.SelectedItem?.ToString();

            cmbFaculty.Enabled = reportType?.Contains("Студенты") == true;
            pnlDates.Enabled = reportType?.Contains("Посещаемость") == true;

            btnGenerate.Enabled = !cmbFaculty.Enabled || cmbFaculty.SelectedValue != null;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                string reportType = cmbReportType.SelectedItem?.ToString();
                DataTable result = null;

                switch (reportType)
                {
                    case "📊 Студенты по факультетам":
                        result = GetStudentsByFacultyReport();
                        break;
                    case "👨‍ Тренеры по квалификации":
                        result = GetTrainersByQualificationReport();
                        break;
                    case "⚽ Секции по видам спорта":
                        result = GetSectionsBySportReport();
                        break;
                    case "📋 Посещаемость за период":
                        result = GetAttendanceReport();
                        break;
                    case "🏆 Достижения студентов":
                        result = GetAchievementsReport();
                        break;
                }

                if (result != null)
                {
                    dgvReport.DataSource = result;
                    lblResultCount.Text = $"Записей: {result.Rows.Count}";
                    btnExport.Enabled = result.Rows.Count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка формирования отчёта: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // МЕТОДЫ ФОРМИРОВАНИЯ ОТЧЁТОВ

        private DataTable GetStudentsByFacultyReport()
        {
            string query = @"
                SELECT 
                    f.FacultyName AS [Факультет],
                    s.GroupName AS [Группа],
                    s.Course AS [Курс],
                    s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.MiddleName, '') AS [ФИО],
                    s.StudentCardNumber AS [Номер билета],
                    s.Phone AS [Телефон],
                    s.Email AS [Email]
                FROM Students s
                JOIN Faculties f ON s.FacultyID = f.FacultyID";

            if (cmbFaculty.Enabled && cmbFaculty.SelectedValue != null)
            {
                query += " WHERE s.FacultyID = @FacultyID";
                return DBConnection.Instance.ExecuteQuery(query,
                    new[] { new Microsoft.Data.SqlClient.SqlParameter("@FacultyID", cmbFaculty.SelectedValue) });
            }

            query += " ORDER BY f.FacultyName, s.Course, s.GroupName";
            return DBConnection.Instance.ExecuteQuery(query);
        }

        private DataTable GetTrainersByQualificationReport()
        {
            return DBConnection.Instance.ExecuteQuery(@"
                SELECT 
                    Qualification AS [Квалификация],
                    LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS [ФИО],
                    Specialization AS [Специализация],
                    Phone AS [Телефон],
                    Email AS [Email],
                    HireDate AS [Дата приёма]
                FROM Trainers
                ORDER BY Qualification, LastName");
        }

        private DataTable GetSectionsBySportReport()
        {
            return DBConnection.Instance.ExecuteQuery(@"
                SELECT 
                    sp.SportName AS [Вид спорта],
                    sec.SectionName AS [Название секции],
                    t.LastName + ' ' + t.FirstName AS [Тренер],
                    sec.MaxStudents AS [Макс. студентов],
                    sec.PricePerMonth AS [Цена в месяц],
                    sec.Description AS [Описание]
                FROM Sections sec
                JOIN Sports sp ON sec.SportID = sp.SportID
                JOIN Trainers t ON sec.TrainerID = t.TrainerID
                ORDER BY sp.SportName, sec.SectionName");
        }

        private DataTable GetAttendanceReport()
        {
            return DBConnection.Instance.ExecuteQuery(@"
                SELECT 
                    s.StudentCardNumber AS [Номер билета],
                    s.LastName + ' ' + s.FirstName AS [Студент],
                    f.FacultyName AS [Факультет],
                    s.GroupName AS [Группа],
                    sec.SectionName AS [Секция],
                    a.VisitDate AS [Дата],
                    CASE WHEN a.Status = 1 THEN 'Присутствовал' ELSE 'Отсутствовал' END AS [Статус],
                    a.Notes AS [Примечание]
                FROM Attendance a
                JOIN Students s ON a.StudentCardNumber = s.StudentCardNumber
                JOIN Faculties f ON s.FacultyID = f.FacultyID
                JOIN Schedule sc ON a.ScheduleID = sc.ScheduleID
                JOIN Sections sec ON sc.SectionID = sec.SectionID
                WHERE a.VisitDate BETWEEN @DateFrom AND @DateTo
                ORDER BY a.VisitDate DESC, s.LastName",
                new[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@DateFrom", dtpDateFrom.Value.Date),
                    new Microsoft.Data.SqlClient.SqlParameter("@DateTo", dtpDateTo.Value.Date.AddDays(1).AddTicks(-1))
                });
        }

        private DataTable GetAchievementsReport()
        {
            return DBConnection.Instance.ExecuteQuery(@"
                SELECT 
                    s.StudentCardNumber AS [Номер билета],
                    s.LastName + ' ' + s.FirstName AS [Студент],
                    f.FacultyName AS [Факультет],
                    s.GroupName AS [Группа],
                    sec.SectionName AS [Секция],
                    ach.CompetitionName AS [Соревнование],
                    ach.CompetitionDate AS [Дата],
                    ach.Place AS [Место],
                    ach.AwardType AS [Награда],
                    ach.AwardDescription AS [Описание]
                FROM Achievements ach
                JOIN Students s ON ach.StudentCardNumber = s.StudentCardNumber
                JOIN Faculties f ON s.FacultyID = f.FacultyID
                LEFT JOIN Sections sec ON ach.SectionID = sec.SectionID
                ORDER BY ach.CompetitionDate DESC, ach.Place");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Открываем форму экспорта
            using (var exportForm = new ExportForm(dgvReport, cmbReportType.SelectedItem?.ToString()))
            {
                exportForm.ShowDialog();
            }
        }
    }

    // Вспомогательный класс 
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public override string ToString() => Text;
    }
}