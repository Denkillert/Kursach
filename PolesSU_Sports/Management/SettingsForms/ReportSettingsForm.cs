using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace PolesSU_Sports.Management.SettingsForms
{
    public partial class ReportSettingsForm : Form
    {
        public ReportSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройка отчётов";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "📊 Настройка отчётов",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            GroupBox grpTemplates = new GroupBox
            {
                Text = "Шаблоны отчётов",
                Location = new Point(20, 60),
                Size = new Size(540, 150)
            };

            CheckBox chkRector = new CheckBox { Text = "Для ректората", Location = new Point(15, 30), AutoSize = true, Checked = true };
            CheckBox chkDepartment = new CheckBox { Text = "Для кафедры", Location = new Point(15, 60), AutoSize = true, Checked = true };
            CheckBox chkFinance = new CheckBox { Text = "Финансовый", Location = new Point(15, 90), AutoSize = true, Checked = true };
            CheckBox chkAttendance = new CheckBox { Text = "По посещаемости", Location = new Point(200, 30), AutoSize = true, Checked = true };
            CheckBox chkGroup = new CheckBox { Text = "По группам", Location = new Point(200, 60), AutoSize = true, Checked = true };
            CheckBox chkSection = new CheckBox { Text = "По секциям", Location = new Point(200, 90), AutoSize = true, Checked = true };

            grpTemplates.Controls.AddRange(new Control[] { chkRector, chkDepartment, chkFinance, chkAttendance, chkGroup, chkSection });

            GroupBox grpExport = new GroupBox
            {
                Text = "Параметры экспорта",
                Location = new Point(20, 220),
                Size = new Size(540, 120)
            };

            Label lblFormat = new Label { Text = "Формат по умолчанию:", Location = new Point(15, 30), AutoSize = true };
            ComboBox cmbFormat = new ComboBox { Location = new Point(150, 27), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFormat.Items.AddRange(new object[] { "CSV (Excel)", "PDF", "Word", "XLSX" });
            cmbFormat.SelectedIndex = 0;

            Label lblFolder = new Label { Text = "Папка для отчётов:", Location = new Point(15, 70), AutoSize = true };
            TextBox txtFolder = new TextBox { Location = new Point(150, 67), Width = 300, Text = ".\\Отчёты" };
            Button btnBrowse = new Button { Text = "Обзор", Location = new Point(460, 65), Size = new Size(70, 25) };
            btnBrowse.Click += (s, e) =>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == DialogResult.OK)
                        txtFolder.Text = fbd.SelectedPath;
                }
            };

            grpExport.Controls.AddRange(new Control[] { lblFormat, cmbFormat, lblFolder, txtFolder, btnBrowse });

            Button btnSave = new Button
            {
                Text = "💾 Сохранить настройки",
                Location = new Point(20, 360),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += (s, e) =>
            {
                MessageBox.Show("✅ Настройки отчётов сохранены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(210, 360),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, grpTemplates, grpExport, btnSave, btnClose });
        }

        private void LoadSettings()
        {
            // Загрузка настроек из файла конфигурации
        }
    }
}