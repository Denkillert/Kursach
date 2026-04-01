using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PolesSU_Sports.Shared.DB;

namespace PolesSU_Sports.Admin.Students
{
    public partial class StudentSectionForm : Form
    {
        public StudentSectionForm()
        {
            InitializeComponent();
            InitializeForm();
            LoadSections();
        }

        private ComboBox cmbSections;
        private DataGridView dgvStudents;
        private ComboBox cmbAvailableStudents;

        private void InitializeForm()
        {
            this.Text = "Запись студентов в секции";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "📋 Запись студентов в секции",
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            // Выбор секции
            Label lblSection = new Label
            {
                Text = "Выберите секцию:",
                Location = new Point(20, 60),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 10)
            };

            cmbSections = new ComboBox
            {
                Location = new Point(150, 57),
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            cmbSections.SelectedIndexChanged += (s, e) => LoadEnrolledStudents();

            // Список записанных студентов
            Label lblEnrolled = new Label
            {
                Text = "Записанные студенты:",
                Location = new Point(20, 100),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold)
            };

            dgvStudents = new DataGridView
            {
                Location = new Point(20, 130),
                Size = new Size(500, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White
            };

            
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StudentSectionID",
                HeaderText = "ID",
                Visible = false
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StudentCard",
                HeaderText = "Билет"
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "ФИО"
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GroupName",
                HeaderText = "Группа"
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EnrollmentDate",
                HeaderText = "Дата записи"
            });
            dgvStudents.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Статус"
            });

            // Панель кнопок
            Panel btnPanel = new Panel
            {
                Location = new Point(540, 130),
                Size = new Size(330, 300)
            };

            Label lblAddStudent = new Label
            {
                Text = "Добавить студента:",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold)
            };

            cmbAvailableStudents = new ComboBox
            {
                Location = new Point(10, 35),
                Size = new Size(310, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Button btnAdd = new Button
            {
                Text = "➕ Записать в секцию",
                Location = new Point(10, 70),
                Size = new Size(310, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            btnAdd.Click += BtnAdd_Click;

            Button btnRemove = new Button
            {
                Text = "🗑️ Исключить из секции",
                Location = new Point(10, 120),
                Size = new Size(310, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 10)
            };
            btnRemove.Click += BtnRemove_Click;

            Label lblInfo = new Label
            {
                Text = "Информация:",
                Location = new Point(10, 170),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold)
            };

            TextBox txtInfo = new TextBox
            {
                Location = new Point(10, 195),
                Size = new Size(310, 95),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245),
                Text = "• Выберите секцию сверху\n• В таблице показаны записанные студенты\n• Выберите студента из списка ниже\n• Нажмите \"Записать\" или \"Исключить\""
            };

            btnPanel.Controls.AddRange(new Control[] {
        lblAddStudent, cmbAvailableStudents, btnAdd, btnRemove,
        lblInfo, txtInfo
    });

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(20, 440),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
        lblTitle, lblSection, cmbSections, lblEnrolled,
        dgvStudents, btnPanel, btnClose
    });
        }

        private void LoadSections()
        {
            DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                SELECT SectionID, SectionName
                FROM Sections
                ORDER BY SectionName");

            cmbSections.DataSource = dt;
            cmbSections.DisplayMember = "SectionName";
            cmbSections.ValueMember = "SectionID";
        }

        private void LoadEnrolledStudents()
        {
            dgvStudents.Rows.Clear();

            if (cmbSections.SelectedValue == null || cmbSections.SelectedValue == DBNull.Value)
                return;

            int sectionID = 0;

            
            if (cmbSections.SelectedValue is DataRowView rowView)
            {
                var id = rowView["SectionID"];
                if (id != null && id != DBNull.Value)
                {
                    sectionID = Convert.ToInt32(id);
                }
                else
                {
                    return;
                }
            }
            else
            {
                sectionID = Convert.ToInt32(cmbSections.SelectedValue);
            }

            // Загружаем записанных студентов
            DataTable dt = DBConnection.Instance.ExecuteQuery(@"
        SELECT ss.StudentSectionID,
            s.StudentCardNumber AS [Билет],
            s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.MiddleName, '') AS [ФИО],
            s.GroupName AS [Группа],
            ss.EnrollmentDate AS [Дата записи],
            CASE WHEN ss.IsActive = 1 THEN 'Активна' ELSE 'Неактивна' END AS [Статус]
        FROM StudentSections ss
        JOIN Students s ON ss.StudentCardNumber = s.StudentCardNumber
        WHERE ss.SectionID = @SectionID
        ORDER BY s.LastName",
                new[] { new SqlParameter("@SectionID", sectionID) });

            foreach (DataRow row in dt.Rows)
            {
                dgvStudents.Rows.Add(
                    row["StudentSectionID"],
                    row["Билет"],
                    row["ФИО"],
                    row["Группа"],
                    row["Дата записи"],
                    row["Статус"]
                );
            }

            // Загружаем доступных студентов (не записанных в эту секцию)
            LoadAvailableStudents(sectionID);
        }

        private void LoadAvailableStudents(int sectionID)
        {
            DataTable dt = DBConnection.Instance.ExecuteQuery(@"
                SELECT s.StudentCardNumber,
                    s.LastName + ' ' + s.FirstName + ' ' + ISNULL(s.MiddleName, '') AS FullName
                FROM Students s
                WHERE s.StudentCardNumber NOT IN (
                    SELECT StudentCardNumber 
                    FROM StudentSections 
                    WHERE SectionID = @SectionID AND IsActive = 1
                )
                ORDER BY s.LastName",
                new[] { new SqlParameter("@SectionID", sectionID) });

            cmbAvailableStudents.DataSource = dt;
            cmbAvailableStudents.DisplayMember = "FullName";
            cmbAvailableStudents.ValueMember = "StudentCardNumber";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbSections.SelectedValue == null || cmbSections.SelectedValue == DBNull.Value)
            {
                MessageBox.Show("Выберите секцию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sectionID = 0;

            
            if (cmbSections.SelectedValue is DataRowView rowView)
            {
                var id = rowView["SectionID"];
                if (id != null && id != DBNull.Value)
                {
                    sectionID = Convert.ToInt32(id);
                }
            }
            else
            {
                sectionID = Convert.ToInt32(cmbSections.SelectedValue);
            }

            if (cmbAvailableStudents.SelectedValue == null || cmbAvailableStudents.SelectedValue == DBNull.Value)
            {
                MessageBox.Show("Выберите студента для записи", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string studentCard = cmbAvailableStudents.SelectedValue.ToString();

                // Проверяем, не записан ли уже
                var exists = DBConnection.Instance.ExecuteScalar(
                    "SELECT StudentSectionID FROM StudentSections WHERE StudentCardNumber = @Card AND SectionID = @Section",
                    new[] {
                new SqlParameter("@Card", studentCard),
                new SqlParameter("@Section", sectionID)
                    });

                if (exists != null && exists != DBNull.Value)
                {
                    MessageBox.Show("Студент уже записан в эту секцию", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Добавляем запись
                DBConnection.Instance.ExecuteCommand(@"
            INSERT INTO StudentSections (StudentCardNumber, SectionID, EnrollmentDate, IsActive)
            VALUES (@Card, @Section, GETDATE(), 1)",
                    new[] {
                new SqlParameter("@Card", studentCard),
                new SqlParameter("@Section", sectionID)
                    });

                MessageBox.Show("✅ Студент записан в секцию", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadEnrolledStudents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count == 0)
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
                int studentSectionID = Convert.ToInt32(dgvStudents.SelectedRows[0].Cells[0].Value);

                // мягкое удаление - IsActive = 0
                DBConnection.Instance.ExecuteCommand(
                    "UPDATE StudentSections SET IsActive = 0 WHERE StudentSectionID = @ID",
                    new[] { new SqlParameter("@ID", studentSectionID) });

                MessageBox.Show("✅ Студент исключен из секции", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadEnrolledStudents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}