using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using PolesSU_Sports.Shared.DB;
using PolesSU_Sports.Shared.Model;

namespace PolesSU_Sports.Admin.Sections
{
    public partial class SectionForm : Form
    {
        private bool isEditMode = false;
        private int currentSectionID = 0;

        // Элементы управления
        private TextBox txtSectionName;
        private ComboBox cmbSport;
        private ComboBox cmbTrainer;
        private NumericUpDown numMaxStudents;
        private NumericUpDown numPricePerMonth;
        private TextBox txtDescription;
        private TextBox txtRequirements;
        private Button btnSave;
        private Button btnCancel;

        public SectionForm()
        {
            InitializeComponent();
            LoadSports();
            LoadTrainers();
        }

        // Конструктор для редактирования
        public SectionForm(int sectionID) : this()
        {
            isEditMode = true;
            currentSectionID = sectionID;
            LoadSectionData(sectionID);
            this.Text = "Редактирование секции";
        }

        private void InitializeComponent()
        {
            this.Text = "Добавление секции";
            this.Size = new System.Drawing.Size(550, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;
            int labelWidth = 150;
            int inputWidth = 320;
            int inputX = 180;

            // Название секции
            CreateLabel("Название секции*:", 20, y, labelWidth);
            txtSectionName = CreateTextBox(inputX, y, inputWidth, "Волейбол (мужчины)");
            y += 45;

            // Вид спорта
            CreateLabel("Вид спорта*:", 20, y, labelWidth);
            cmbSport = new ComboBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbSport);
            y += 45;

            // Тренер
            CreateLabel("Тренер*:", 20, y, labelWidth);
            cmbTrainer = new ComboBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbTrainer);
            y += 45;

            // Максимум студентов
            CreateLabel("Макс. студентов:", 20, y, labelWidth);
            numMaxStudents = new NumericUpDown
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                Minimum = 0,
                Maximum = 100,
                Value = 20
            };
            this.Controls.Add(numMaxStudents);
            y += 45;

            // Цена в месяц
            CreateLabel("Цена в месяц (BYN):", 20, y, labelWidth);
            numPricePerMonth = new NumericUpDown
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 23),
                Minimum = 0,
                Maximum = 1000,
                DecimalPlaces = 2,
                Value = 0.00m
            };
            this.Controls.Add(numPricePerMonth);
            y += 45;

            // Описание
            CreateLabel("Описание:", 20, y, labelWidth);
            txtDescription = new TextBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtDescription);
            y += 75;

            // Требования
            CreateLabel("Требования:", 20, y, labelWidth);
            txtRequirements = new TextBox
            {
                Location = new System.Drawing.Point(inputX, y),
                Size = new System.Drawing.Size(inputWidth, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtRequirements);
            y += 85;

            // Кнопки
            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new System.Drawing.Point(180, y),
                Size = new System.Drawing.Size(110, 40),
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10)
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new System.Drawing.Point(300, y),
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

        private void LoadSports()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT SportID, SportName FROM Sports ORDER BY SportName");

                cmbSport.DataSource = dt;
                cmbSport.DisplayMember = "SportName";
                cmbSport.ValueMember = "SportID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки видов спорта: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTrainers()
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT TrainerID, LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS FullName FROM Trainers ORDER BY LastName");

                cmbTrainer.DataSource = dt;
                cmbTrainer.DisplayMember = "FullName";
                cmbTrainer.ValueMember = "TrainerID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки тренеров: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSectionData(int sectionID)
        {
            try
            {
                DataTable dt = DBConnection.Instance.ExecuteQuery(
                    "SELECT * FROM Sections WHERE SectionID = @SectionID",
                    new[] { new SqlParameter("@SectionID", sectionID) });

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    txtSectionName.Text = row["SectionName"].ToString();
                    cmbSport.SelectedValue = row["SportID"];
                    cmbTrainer.SelectedValue = row["TrainerID"];
                    numMaxStudents.Value = row["MaxStudents"] != DBNull.Value ? Convert.ToDecimal(row["MaxStudents"]) : 0;
                    numPricePerMonth.Value = Convert.ToDecimal(row["PricePerMonth"]);
                    txtDescription.Text = row["Description"].ToString();
                    txtRequirements.Text = row["Requirements"].ToString();
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
            if (string.IsNullOrWhiteSpace(txtSectionName.Text))
            {
                MessageBox.Show("Введите название секции", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbSport.SelectedValue == null)
            {
                MessageBox.Show("Выберите вид спорта", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbTrainer.SelectedValue == null)
            {
                MessageBox.Show("Выберите тренера", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@SectionName", txtSectionName.Text),
                    new SqlParameter("@SportID", cmbSport.SelectedValue),
                    new SqlParameter("@TrainerID", cmbTrainer.SelectedValue),
                    new SqlParameter("@MaxStudents", numMaxStudents.Value > 0 ? (object)numMaxStudents.Value : DBNull.Value),
                    new SqlParameter("@PricePerMonth", numPricePerMonth.Value),
                    new SqlParameter("@Description", string.IsNullOrEmpty(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text),
                    new SqlParameter("@Requirements", string.IsNullOrEmpty(txtRequirements.Text) ? (object)DBNull.Value : txtRequirements.Text)
                };

                if (isEditMode)
                {
                    // Обновление
                    DBConnection.Instance.ExecuteCommand(@"
                        UPDATE Sections SET
                            SectionName = @SectionName,
                            SportID = @SportID,
                            TrainerID = @TrainerID,
                            MaxStudents = @MaxStudents,
                            PricePerMonth = @PricePerMonth,
                            Description = @Description,
                            Requirements = @Requirements
                        WHERE SectionID = @SectionID",
                        AddParameter(parameters, "@SectionID", currentSectionID));

                    MessageBox.Show("✅ Секция успешно обновлена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Добавление
                    DBConnection.Instance.ExecuteCommand(@"
                        INSERT INTO Sections (SectionName, SportID, TrainerID, MaxStudents, PricePerMonth, Description, Requirements)
                        VALUES (@SectionName, @SportID, @TrainerID, @MaxStudents, @PricePerMonth, @Description, @Requirements)",
                        parameters);

                    MessageBox.Show("✅ Секция успешно добавлена!", "Успех",
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

        // Вспомогательный метод для добавления параметра
        private SqlParameter[] AddParameter(SqlParameter[] parameters, string name, object value)
        {
            SqlParameter[] newParams = new SqlParameter[parameters.Length + 1];
            parameters.CopyTo(newParams, 0);
            newParams[parameters.Length] = new SqlParameter(name, value);
            return newParams;
        }
    }
}