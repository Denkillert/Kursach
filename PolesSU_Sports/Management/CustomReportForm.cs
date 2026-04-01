using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;

namespace PolesSU_Sports.Management
{
    public partial class CustomReportForm : Form
    {
        private ComboBox cmbTable;
        private CheckBox chkDistinct;
        private FlowLayoutPanel fldPanel;
        private TextBox txtWhere;
        private TextBox txtOrderBy;
        private ComboBox cmbOrderDir;
        private NumericUpDown numLimit;
        private CheckBox chkShowSQL;
        private DataGridView dgvResult;
        private Button btnGenerate;
        private Button btnExport;

        public CustomReportForm()
        {
            InitializeComponent();
            UpdateAvailableFields();
        }

        private void InitializeComponent()
        {
            this.Text = "⚙️ Конструктор отчёта";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);

            // === ВЕРХНЯЯ ПАНЕЛЬ: НАСТРОЙКИ ===
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 280,
                Padding = new Padding(15)
            };

            // 1. Выбор таблицы
            var grpTable = new GroupBox
            {
                Text = "📊 Выберите таблицу",
                Location = new Point(15, 10),
                Size = new Size(220, 120)
            };

            cmbTable = new ComboBox
            {
                Location = new Point(15, 25),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTable.Items.AddRange(new object[] {
                "🎓 Students (Студенты)",
                "📋 Attendance (Посещаемость)",
                "⚽ Sections (Секции)",
                "👨‍🏫 Trainers (Тренеры)",
                "🏛️ Faculties (Факультеты)",
                "🏆 Achievements (Достижения)"
            });
            cmbTable.SelectedIndex = 0;
            cmbTable.SelectedIndexChanged += (s, e) => UpdateAvailableFields();

            chkDistinct = new CheckBox
            {
                Text = "Только уникальные (DISTINCT)",
                Location = new Point(15, 60),
                AutoSize = true
            };

            grpTable.Controls.AddRange(new Control[] { cmbTable, chkDistinct });

            // 2. Выбор полей
            var grpFields = new GroupBox
            {
                Text = "📋 Поля для отображения",
                Location = new Point(225, 10),
                Size = new Size(500, 120)
            };

            fldPanel = new FlowLayoutPanel
            {
                Location = new Point(15, 25),
                Size = new Size(470, 80),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true
            };

            grpFields.Controls.Add(fldPanel);

            // 3. Фильтры
            var grpFilters = new GroupBox
            {
                Text = "🔍 Фильтры и сортировка",
                Location = new Point(15, 140),
                Size = new Size(710, 70)
            };

            var lblWhere = new Label { Text = "WHERE (условие):", Location = new Point(15, 25), AutoSize = true };
            txtWhere = new TextBox
            {
                Location = new Point(160, 22),
                Width = 350,
                PlaceholderText = "Например: Course = 3 AND FacultyID = 1"
            };

            var lblOrder = new Label { Text = "ORDER BY (сортировка):", Location = new Point(15, 50), AutoSize = true };
            txtOrderBy = new TextBox
            {
                Location = new Point(160, 47),
                Width = 200,
                PlaceholderText = "Например: LastName"
            };

            cmbOrderDir = new ComboBox
            {
                Location = new Point(365, 47),
                Width = 70,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbOrderDir.Items.AddRange(new object[] { "ASC (по возрастанию)", "DESC (по убыванию)" });
            cmbOrderDir.SelectedIndex = 0;

            grpFilters.Controls.AddRange(new Control[] { lblWhere, txtWhere, lblOrder, txtOrderBy, cmbOrderDir });

            // 4. Дополнительно
            var grpExtra = new GroupBox
            {
                Text = "📌 Дополнительно",
                Location = new Point(15, 220),
                Size = new Size(710, 50)
            };

            var lblLimit = new Label { Text = "Максимум записей:", Location = new Point(15, 22), AutoSize = true };
            numLimit = new NumericUpDown
            {
                Location = new Point(135, 19),
                Width = 70,
                Minimum = 1,
                Maximum = 10000,
                Value = 100
            };

            chkShowSQL = new CheckBox
            {
                Text = "Показать SQL-запрос перед выполнением",
                Location = new Point(220, 21),
                AutoSize = true
            };

            grpExtra.Controls.AddRange(new Control[] { lblLimit, numLimit, chkShowSQL });

            topPanel.Controls.AddRange(new Control[] { grpTable, grpFields, grpFilters, grpExtra });

            // === КНОПКИ ===
            var btnPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(15, 10, 15, 10)
            };

            btnGenerate = new Button
            {
                Text = "📄 Сформировать отчёт",
                Location = new Point(15, 8),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGenerate.Click += BtnGenerate_Click;

            btnExport = new Button
            {
                Text = "💾 Экспорт в CSV",
                Location = new Point(205, 8),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Enabled = false
            };
            btnExport.Click += BtnExport_Click;

            var btnClose = new Button
            {
                Text = "❌ Закрыть",
                Location = new Point(365, 8),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            btnPanel.Controls.AddRange(new Control[] { btnGenerate, btnExport, btnClose });

            // === ТАБЛИЦА РЕЗУЛЬТАТОВ ===
            var tablePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 0, 15, 15)
            };

            dgvResult = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect,
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
                RowHeadersVisible = false,
                MultiSelect = false
            };
            // ✅ Убираем синее выделение
            dgvResult.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 230, 230);
            dgvResult.DefaultCellStyle.SelectionForeColor = Color.FromArgb(45, 55, 75);
            dgvResult.EnableHeadersVisualStyles = false;

            tablePanel.Controls.Add(dgvResult);

            // === ДОБАВЛЯЕМ КОНТРОЛЫ ===
            this.Controls.Add(tablePanel);
            this.Controls.Add(btnPanel);
            this.Controls.Add(topPanel);
        }

        private void UpdateAvailableFields()
        {
            fldPanel.Controls.Clear();

            var tableFields = new Dictionary<string, string[]>
            {
                ["🎓 Students (Студенты)"] = new[] {
                    "StudentCardNumber (Билет)", "LastName (Фамилия)", "FirstName (Имя)",
                    "MiddleName (Отчество)", "BirthDate (Дата рождения)", "Phone (Телефон)",
                    "Email (Email)", "FacultyID (Факультет)", "GroupName (Группа)",
                    "Course (Курс)", "EnrollmentDate (Дата зачисления)"
                },
                ["📋 Attendance (Посещаемость)"] = new[] {
                    "AttendanceID (ID)", "StudentCardNumber (Билет)", "ScheduleID (Расписание)",
                    "VisitDate (Дата)", "Status (Статус)", "Notes (Примечание)"
                },
                ["⚽ Sections (Секции)"] = new[] {
                    "SectionID (ID)", "SectionName (Название)", "SportID (Вид спорта)",
                    "TrainerID (Тренер)", "MaxStudents (Макс. студентов)",
                    "PricePerMonth (Цена)", "Description (Описание)"
                },
                ["👨‍🏫 Trainers (Тренеры)"] = new[] {
                    "TrainerID (ID)", "DocumentNumber (Документ)", "LastName (Фамилия)",
                    "FirstName (Имя)", "MiddleName (Отчество)", "BirthDate (Дата рождения)",
                    "Phone (Телефон)", "Email (Email)", "Qualification (Квалификация)",
                    "Specialization (Специализация)", "HireDate (Дата приёма)"
                },
                ["🏛️ Faculties (Факультеты)"] = new[] {
                    "FacultyID (ID)", "FacultyName (Название)", "DeanName (Декан)", "Phone (Телефон)"
                },
                ["🏆 Achievements (Достижения)"] = new[] {
                    "AchievementID (ID)", "StudentCardNumber (Билет)", "SectionID (Секция)",
                    "CompetitionName (Соревнование)", "CompetitionDate (Дата)",
                    "Place (Место)", "AwardType (Награда)", "AwardDescription (Описание)"
                }
            };

            string selectedTable = cmbTable.SelectedItem?.ToString() ?? "";

            if (tableFields.TryGetValue(selectedTable, out string[] fields))
            {
                foreach (var field in fields)
                {
                    var chk = new CheckBox
                    {
                        Text = field,
                        AutoSize = true,
                        Margin = new Padding(3),
                        Font = new Font("Segoe UI", 8),
                        Tag = field.Split('(')[0].Trim()  // Сохраняем имя поля без описания
                    };
                    // По умолчанию выбираем ID и Name поля
                    chk.Checked = field.Contains("ID") || field.Contains("Название") || field.Contains("Фамилия");
                    fldPanel.Controls.Add(chk);
                }
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            var tableMap = new Dictionary<string, string>
            {
                ["🎓 Students (Студенты)"] = "Students",
                ["📋 Attendance (Посещаемость)"] = "Attendance",
                ["⚽ Sections (Секции)"] = "Sections",
                ["👨‍🏫 Trainers (Тренеры)"] = "Trainers",
                ["🏛️ Faculties (Факультеты)"] = "Faculties",
                ["🏆 Achievements (Достижения)"] = "Achievements"
            };

            string selectedTable = cmbTable.SelectedItem?.ToString() ?? "";
            if (!tableMap.TryGetValue(selectedTable, out string tableName))
            {
                MessageBox.Show("⚠️ Выберите таблицу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Собираем выбранные поля
            var selectedFields = new List<string>();
            foreach (CheckBox chk in fldPanel.Controls)
            {
                if (chk.Checked && chk.Tag != null)
                {
                    selectedFields.Add(chk.Tag.ToString());
                }
            }

            if (selectedFields.Count == 0)
            {
                MessageBox.Show("⚠️ Выберите хотя бы одно поле", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Формируем запрос
            string distinct = chkDistinct.Checked ? "DISTINCT " : "";
            string fields = string.Join(", ", selectedFields);
            string whereClause = txtWhere.Text.Trim();
            string orderBy = txtOrderBy.Text.Trim();
            string orderDir = cmbOrderDir.SelectedIndex == 0 ? "ASC" : "DESC";
            int limit = (int)numLimit.Value;

            string query = $"SELECT {distinct}TOP {limit} {fields} FROM {tableName}";

            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                query += $" WHERE {whereClause}";
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                query += $" ORDER BY {orderBy} {orderDir}";
            }

            // Показываем SQL если нужно
            if (chkShowSQL.Checked)
            {
                if (MessageBox.Show($"📋 SQL-запрос:\n\n{query}\n\nПродолжить?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }

            try
            {
                DataTable result = DBConnection.Instance.ExecuteQuery(query);
                dgvResult.DataSource = result;
                btnExport.Enabled = result.Rows.Count > 0;

                if (result.Rows.Count == 0)
                {
                    MessageBox.Show("⚠️ Нет данных по заданным условиям", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"✅ Загружено {result.Rows.Count} записей", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка выполнения запроса:\n{ex.Message}\n\nЗапрос:\n{query}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvResult.DataSource is not DataTable data || data.Rows.Count == 0)
            {
                MessageBox.Show("⚠️ Нет данных для экспорта", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string tableName = cmbTable.SelectedItem?.ToString()?.Replace(" ", "_") ?? "Otchet";
            string fileName = $"Otchet_{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}";

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV (Excel)|*.csv";
                sfd.FileName = fileName;
                sfd.InitialDirectory = System.IO.Path.Combine(Application.StartupPath, "Отчёты");

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportToCSV(data, sfd.FileName);
                    MessageBox.Show($"✅ Отчёт сохранён:\n{sfd.FileName}", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (MessageBox.Show("Открыть файл?", "Отчёт готов",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(sfd.FileName), System.IO.Path.GetFileName(sfd.FileName));
                    }
                }
            }
        }

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

            System.IO.File.WriteAllText(filePath, csvContent.ToString(), Encoding.GetEncoding("windows-1251"));
        }
    }
}