using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using PolesSU_Sports.Lib.DB;
using PolesSU_Sports.Lib.Model;

namespace PolesSU_Sports.Admin.Trainers
{
    public partial class TrainerForm : Form
    {
        private bool isEditMode = false;
        private string currentDocumentNumber = null;  

        // Элементы управления
        private TextBox txtDocumentNumber;
        private TextBox txtLastName;
        private TextBox txtFirstName;
        private TextBox txtMiddleName;
        private DateTimePicker dtpBirthDate;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtQualification;
        private TextBox txtSpecialization;
        private Button btnSave;
        private Button btnCancel;

        public TrainerForm()
        {
            InitializeComponent();
        }

        // Конструктор для редактирования
        public TrainerForm(string documentNumber) : this()
        {
            isEditMode = true;
            currentDocumentNumber = documentNumber;  // ✅ Теперь присваиваем string
            LoadTrainerData(documentNumber);
            this.Text = "Редактирование тренера";
            txtDocumentNumber.Enabled = false;  // Нельзя менять номер документа
        }

        private void InitializeComponent()
        {
            this.Text = "Добавление тренера";
            this.Size = new System.Drawing.Size(500, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;
            int labelWidth = 130;
            int inputWidth = 280;
            int inputX = 160;

            // Номер документа
            CreateLabel("Номер документа*:", 20, y, labelWidth);
            txtDocumentNumber = CreateTextBox(inputX, y, inputWidth, "АВ1234567");
            y += 45;

            // Фамилия
            CreateLabel("Фамилия*:", 20, y, labelWidth);
            txtLastName = CreateTextBox(inputX, y, inputWidth, "Ковалёв");
            y += 45;

            // Имя
            CreateLabel("Имя*:", 20, y, labelWidth);
            txtFirstName = CreateTextBox(inputX, y, inputWidth, "Александр");
            y += 45;

            // Отчество
            CreateLabel("Отчество:", 20, y, labelWidth);
            txtMiddleName = CreateTextBox(inputX, y, inputWidth, "Викторович");
            y += 45;

            // Дата рождения
            CreateLabel("Дата рождения*:", 20, y, labelWidth);
            dtpBirthDate = new DateTimePicker
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddYears(-30)
            };
            this.Controls.Add(dtpBirthDate);
            y += 45;

            // Телефон
            CreateLabel("Телефон:", 20, y, labelWidth);
            txtPhone = CreateTextBox(inputX, y, inputWidth, "+37529XXXXXXX");
            y += 45;

            // Email
            CreateLabel("Email:", 20, y, labelWidth);
            txtEmail = CreateTextBox(inputX, y, inputWidth, "trainer@polessgu.by");
            y += 45;

            // Квалификация
            CreateLabel("Квалификация:", 20, y, labelWidth);
            txtQualification = CreateTextBox(inputX, y, inputWidth, "Высшая категория");
            y += 45;

            // Специализация
            CreateLabel("Специализация:", 20, y, labelWidth);
            txtSpecialization = CreateTextBox(inputX, y, inputWidth, "Волейбол, Баскетбол");
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

        private void LoadTrainerData(string documentNumber)
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT * FROM Trainers WHERE DocumentNumber = @DocumentNumber",
                    new[] { new SqlParameter("@DocumentNumber", documentNumber) });

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    txtDocumentNumber.Text = row["DocumentNumber"].ToString();
                    txtLastName.Text = row["LastName"].ToString();
                    txtFirstName.Text = row["FirstName"].ToString();
                    txtMiddleName.Text = row["MiddleName"] != DBNull.Value ? row["MiddleName"].ToString() : "";
                    dtpBirthDate.Value = Convert.ToDateTime(row["BirthDate"]);
                    txtPhone.Text = row["Phone"] != DBNull.Value ? row["Phone"].ToString() : "";
                    txtEmail.Text = row["Email"] != DBNull.Value ? row["Email"].ToString() : "";
                    txtQualification.Text = row["Qualification"] != DBNull.Value ? row["Qualification"].ToString() : "";
                    txtSpecialization.Text = row["Specialization"] != DBNull.Value ? row["Specialization"].ToString() : "";
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
            if (string.IsNullOrWhiteSpace(txtDocumentNumber.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Заполните обязательные поля (номер документа, фамилия, имя)",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@DocumentNumber", txtDocumentNumber.Text),  // ✅ Добавляем @DocumentNumber
                    new SqlParameter("@LastName", txtLastName.Text),
                    new SqlParameter("@FirstName", txtFirstName.Text),
                    new SqlParameter("@MiddleName", string.IsNullOrEmpty(txtMiddleName.Text) ? (object)DBNull.Value : txtMiddleName.Text),
                    new SqlParameter("@BirthDate", dtpBirthDate.Value),
                    new SqlParameter("@Phone", string.IsNullOrEmpty(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text),
                    new SqlParameter("@Email", string.IsNullOrEmpty(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text),
                    new SqlParameter("@Qualification", string.IsNullOrEmpty(txtQualification.Text) ? (object)DBNull.Value : txtQualification.Text),
                    new SqlParameter("@Specialization", string.IsNullOrEmpty(txtSpecialization.Text) ? (object)DBNull.Value : txtSpecialization.Text)
                };

                if (isEditMode)
                {
                    // ✅ Обновление: НЕ обновляем DocumentNumber, только WHERE по нему
                    DBConnection.Instance.ExecuteCommand(@"
                        UPDATE Trainers SET
                            LastName = @LastName,
                            FirstName = @FirstName,
                            MiddleName = @MiddleName,
                            BirthDate = @BirthDate,
                            Phone = @Phone,
                            Email = @Email,
                            Qualification = @Qualification,
                            Specialization = @Specialization
                        WHERE DocumentNumber = @DocumentNumber",
                        parameters);  // ✅ parameters уже содержит @DocumentNumber

                    MessageBox.Show("✅ Тренер успешно обновлён!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Добавление нового тренера
                    DBConnection.Instance.ExecuteCommand(@"
                        INSERT INTO Trainers (DocumentNumber, LastName, FirstName, MiddleName, BirthDate, Phone, Email, Qualification, Specialization)
                        VALUES (@DocumentNumber, @LastName, @FirstName, @MiddleName, @BirthDate, @Phone, @Email, @Qualification, @Specialization)",
                        parameters);

                    MessageBox.Show("✅ Тренер успешно добавлен!", "Успех",
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