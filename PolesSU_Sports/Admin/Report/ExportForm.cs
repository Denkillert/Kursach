using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace PolesSU_Sports.Admin.Report
{
    public partial class ExportForm : Form
    {
        private DataGridView dgvSource;
        private string reportName;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnExport;
        private Button btnCancel;
        private ProgressBar progressBar;
        private Label lblStatus;

        public ExportForm(DataGridView sourceGrid, string reportTitle)
        {
            dgvSource = sourceGrid;
            reportName = reportTitle?.Replace("📊 ", "").Replace("👨‍ ", "").Replace("⚽ ", "").Replace("📋 ", "").Replace("🏆 ", "") ?? "Отчёт";
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Экспорт отчёта";
            this.Size = new System.Drawing.Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 30;

            // Путь к файлу
            Label lblPath = new Label
            {
                Text = "Сохранить в файл:",
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            txtFilePath = new TextBox
            {
                Location = new System.Drawing.Point(130, y),
                Size = new System.Drawing.Size(250, 23),
                Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"{reportName}_{DateTime.Now:yyyy-MM-dd}.xlsx"),
                ReadOnly = true
            };

            btnBrowse = new Button
            {
                Text = "📁 Обзор...",
                Location = new System.Drawing.Point(390, y),
                Size = new System.Drawing.Size(80, 23),
                FlatStyle = FlatStyle.Flat
            };
            btnBrowse.Click += BtnBrowse_Click;

            y += 50;

            // Прогресс
            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(450, 23),
                Visible = false
            };

            lblStatus = new Label
            {
                Location = new System.Drawing.Point(20, y + 30),
                Size = new System.Drawing.Size(450, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                ForeColor = System.Drawing.Color.Gray
            };

            y += 70;

            // Кнопки
            btnExport = new Button
            {
                Text = "📤 Экспортировать",
                Location = new System.Drawing.Point(130, y),
                Size = new System.Drawing.Size(110, 40),
                BackColor = System.Drawing.Color.FromArgb(0, 86, 179),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExport.Click += BtnExport_Click;

            btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new System.Drawing.Point(250, y),
                Size = new System.Drawing.Size(110, 40),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblPath, txtFilePath, btnBrowse, progressBar, lblStatus, btnExport, btnCancel });
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                FileName = Path.GetFileName(txtFilePath.Text),
                InitialDirectory = Path.GetDirectoryName(txtFilePath.Text)
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dlg.FileName;
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                btnExport.Enabled = false;
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "Экспорт...";

                // Простой экспорт в CSV
                ExportToCsv(txtFilePath.Text.Replace(".xlsx", ".csv"));

                lblStatus.Text = "✅ Готово!";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                if (MessageBox.Show("Файл успешно экспортирован!\nОткрыть файл?", "Экспорт завершён",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{txtFilePath.Text.Replace(".xlsx", ".csv")}\"");
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Ошибка: " + ex.Message;
                lblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show("Ошибка экспорта: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExport.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void ExportToCsv(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Заголовки столбцов
                for (int i = 0; i < dgvSource.Columns.Count; i++)
                {
                    writer.Write($"\"{dgvSource.Columns[i].HeaderText}\"");
                    if (i < dgvSource.Columns.Count - 1) writer.Write(";");
                }
                writer.WriteLine();

                // Данные
                foreach (DataGridViewRow row in dgvSource.Rows)
                {
                    if (row.IsNewRow) continue;

                    for (int i = 0; i < dgvSource.Columns.Count; i++)
                    {
                        string value = row.Cells[i].Value?.ToString()?.Replace("\"", "\"\"") ?? "";
                        writer.Write($"\"{value}\"");
                        if (i < dgvSource.Columns.Count - 1) writer.Write(";");
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}