using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using PolesSU_Sports.Lib.Model;
using PolesSU_Sports.Lib.DB;

namespace PolesSU_Sports.Admin.Students
{
   partial class StudentForm : Form
    {
        private bool isEditMode = false;
        private string currentStudentCardNumber = null;

        // Элементы управления
        private TextBox txtStudentCard;
        private TextBox txtLastName;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private DateTimePicker dtpBirthDate;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private ComboBox cmbFaculty;
        private TextBox txtGroupName;
        private NumericUpDown numCourse;
        private Button btnSave;
        private Button btnCancel;

        public StudentForm()
        {
            InitializeComponent();
            LoadFaculties();
        }

        // Конструктор для редактирования
        public StudentForm(string studentCardNumber) : this()
        {
            isEditMode = true;
            currentStudentCardNumber = studentCardNumber;
            LoadStudentData(studentCardNumber);
            this.Text = "Редактирование студента";
            txtStudentCard.Enabled = false; // Нельзя менять номер билета
        }

        private void InitializeComponent()
        {
            this.Text = "Добавление студента";
            this.Size = new System.Drawing.Size(500, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;
            int labelWidth = 130;
            int inputWidth = 280;
            int inputX = 160;

            // Номер студенческого
            CreateLabel("Номер билета*:", 20, y, labelWidth);
            txtStudentCard = CreateTextBox(inputX, y, inputWidth, "ФК-2025-001");
            y += 45;

            // Фамилия
            CreateLabel("Фамилия*:", 20, y, labelWidth);
            txtLastName = CreateTextBox(inputX, y, inputWidth, "Иванов");
            y += 45;

            // Имя
            CreateLabel("Имя*:", 20, y, labelWidth);
            txtFirstName = CreateTextBox(inputX, y, inputWidth, "Иван");
            y += 45;

            // Отчество
            CreateLabel("Отчество:", 20, y, labelWidth);
            txtMiddleName = CreateTextBox(inputX, y, inputWidth, "Иванович");
            y += 45;

            // Дата рождения
            CreateLabel("Дата рождения*:", 20, y, labelWidth);
            dtpBirthDate = new DateTimePicker
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddYears(-18)
            };
            this.Controls.Add(dtpBirthDate);
            y += 45;

            // Телефон
            CreateLabel("Телефон:", 20, y, labelWidth);
            txtPhone = CreateTextBox(inputX, y, inputWidth, "+37529XXXXXXX");
            y += 45;

            // Email
            CreateLabel("Email:", 20, y, labelWidth);
            txtEmail = CreateTextBox(inputX, y, inputWidth, "student@polessgu.by");
            y += 45;

            // Факультет
            CreateLabel("Факультет*:", 20, y, labelWidth);
            cmbFaculty = new ComboBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbFaculty);
            y += 45;

            // Группа
            CreateLabel("Группа*:", 20, y, labelWidth);
            txtGroupName = CreateTextBox(inputX, y, inputWidth, "ФК-11");
            y += 45;

            // Курс
            CreateLabel("Курс*:", 20, y, labelWidth);
            numCourse = new NumericUpDown
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                Minimum = 1,
                Maximum = 6,
                Value = 1
            };
            this.Controls.Add(numCourse);
            y += 60;

            // Кнопки
            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new System.Drawing.Point(160, y),
                Size = new System.Drawing.Size(110, 40),
                BackColor = System.Drawing.Color.FromArgb(0, 86, 179),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10)
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new System.Drawing.Point(280, y),
                Size = new System.Drawing.Size(110, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });

            // Обработка Enter
            this.AcceptButton = btnSave;
        }

        private Label CreateLabel(string text, int x, int y, int width)
        {
            Label lbl = new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 9)
            };
            this.Controls.Add(lbl);
            return lbl;
        }

        private TextBox CreateTextBox(int x, int y, int width, string placeholder = "")
        {
            TextBox txt = new TextBox
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, 23),
                Tag = placeholder
            };
            txt.GotFocus += (s, e) => { if (txt.Text == placeholder) txt.Text = ""; };
            txt.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txt.Text)) txt.Text = placeholder; };
            this.Controls.Add(txt);
            return txt;
        }

        private void LoadFaculties()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT FacultyID, FacultyName FROM Faculties ORDER BY FacultyName");

                cmbFaculty.DataSource = dt;
                cmbFaculty.DisplayMember = "FacultyName";
                cmbFaculty.ValueMember = "FacultyID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки факультетов: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudentData(string studentCardNumber)
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT * FROM Students WHERE StudentCardNumber = @StudentCardNumber",
                    new[] { new SqlParameter("@StudentCardNumber", studentCardNumber) });

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    txtStudentCard.Text = row["StudentCardNumber"].ToString();
                    txtLastName.Text = row["LastName"].ToString();
                    txtFirstName.Text = row["FirstName"].ToString();
                    txtMiddleName.Text = row["MiddleName"].ToString();
                    dtpBirthDate.Value = Convert.ToDateTime(row["BirthDate"]);
                    txtPhone.Text = row["Phone"].ToString();
                    txtEmail.Text = row["Email"].ToString();
                    cmbFaculty.SelectedValue = row["FacultyID"];
                    txtGroupName.Text = row["GroupName"].ToString();
                    numCourse.Value = Convert.ToDecimal(row["Course"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtStudentCard.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Заполните обязательные поля (номер билета, фамилия, имя)",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbFaculty.SelectedValue == null)
            {
                MessageBox.Show("Выберите факультет", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@StudentCardNumber", txtStudentCard.Text),
                    new SqlParameter("@LastName", txtLastName.Text),
                    new SqlParameter("@FirstName", txtFirstName.Text),
                    new SqlParameter("@MiddleName", string.IsNullOrEmpty(txtMiddleName.Text) ? (object)DBNull.Value : txtMiddleName.Text),
                    new SqlParameter("@BirthDate", dtpBirthDate.Value),
                    new SqlParameter("@Phone", string.IsNullOrEmpty(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text),
                    new SqlParameter("@Email", string.IsNullOrEmpty(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text),
                    new SqlParameter("@FacultyID", cmbFaculty.SelectedValue),
                    new SqlParameter("@GroupName", txtGroupName.Text),
                    new SqlParameter("@Course", (int)numCourse.Value)
                };

                if (isEditMode)
                {
                    // Обновление
                    DBConnection.Instance.ExecuteCommand(@"
                        UPDATE Students SET
                            LastName = @LastName,
                            FirstName = @FirstName,
                            MiddleName = @MiddleName,
                            BirthDate = @BirthDate,
                            Phone = @Phone,
                            Email = @Email,
                            FacultyID = @FacultyID,
                            GroupName = @GroupName,
                            Course = @Course
                        WHERE StudentCardNumber = @StudentCardNumber", parameters);

                    MessageBox.Show("✅ Студент успешно обновлён!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Добавление
                    DBConnection.Instance.ExecuteCommand(@"
                        INSERT INTO Students (StudentCardNumber, LastName, FirstName, MiddleName, BirthDate, Phone, Email, FacultyID, GroupName, Course)
                        VALUES (@StudentCardNumber, @LastName, @FirstName, @MiddleName, @BirthDate, @Phone, @Email, @FacultyID, @GroupName, @Course)", parameters);

                    MessageBox.Show("✅ Студент успешно добавлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Ошибка сохранения: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}