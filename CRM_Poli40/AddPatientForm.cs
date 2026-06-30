using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CRM_Poli40
{
    public class AddPatientForm : Form
    {
        private TextBox txtLastName, txtFirstName, txtMiddleName, txtOMS, txtPhone, txtEmail, txtAddress, txtPassword;
        private DateTimePicker dtpBirthDate;

        public AddPatientForm()
        {
            this.Text = "Добавить пациента";
            this.Size = new Size(400, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 15, lx = 20, fx = 140, fw = 220;

            AddRow("Фамилия:", ref y, lx, fx, fw, out txtLastName);
            AddRow("Имя:", ref y, lx, fx, fw, out txtFirstName);
            AddRow("Отчество:", ref y, lx, fx, fw, out txtMiddleName);

            this.Controls.Add(new Label { Text = "Дата рождения:", Location = new Point(lx, y), AutoSize = true });
            dtpBirthDate = new DateTimePicker
            {
                Location = new Point(fx, y - 3),
                Width = fw,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(1990, 1, 1)
            };
            this.Controls.Add(dtpBirthDate);
            y += 35;

            AddRow("ОМС (16 цифр):", ref y, lx, fx, fw, out txtOMS);
            txtOMS.MaxLength = 16;
            AddRow("Телефон:", ref y, lx, fx, fw, out txtPhone);
            AddRow("Email:", ref y, lx, fx, fw, out txtEmail);
            AddRow("Адрес:", ref y, lx, fx, fw, out txtAddress);
            AddRow("Пароль:", ref y, lx, fx, fw, out txtPassword, true);
            y += 10;

            Button btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(fx, y),
                Width = fw,
                Height = 34,
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void AddRow(string label, ref int y, int lx, int fx, int fw, out TextBox tb, bool password = false)
        {
            this.Controls.Add(new Label 
            { 
                Text = label, 
                Location = new Point(lx, y),
                AutoSize = true 
            });
            tb = new TextBox
            { 
                Location = new Point(fx, y - 3),
                Width = fw 
            };

            if (password) tb.PasswordChar = '+';
            this.Controls.Add(tb);
            y += 35;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtOMS.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Фамилия, Имя, ОМС и Пароль обязательны");
                return;
            }
            if (txtOMS.Text.Trim().Length != 16)
            {
                MessageBox.Show("ОМС — 16 цифр");
                return;
            }

            try
            {
                using (var conn = Database.OpenConnection())
                {
                    // Проверка уникальности ОМС
                    var check = new SqlCommand("SELECT COUNT(*) FROM Patients WHERE OMS = @o", conn);
                    check.Parameters.AddWithValue("@o", txtOMS.Text.Trim());
                    if ((int)check.ExecuteScalar() > 0)
                    {
                        MessageBox.Show("Пациент с таким ОМС уже существует");
                        return;
                    }

                    string hash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text);
                    var cmd = new SqlCommand(
                        @"INSERT INTO Patients (LastName, FirstName, MiddleName, BirthDate, OMS, Phone, Email, Address, PasswordHash)
                          VALUES (@l, @f, @m, @b, @oms, @ph, @em, @ad, @pw)", conn);
                    cmd.Parameters.AddWithValue("@l", txtLastName.Text.Trim());
                    cmd.Parameters.AddWithValue("@f", txtFirstName.Text.Trim());
                    cmd.Parameters.AddWithValue("@m", txtMiddleName.Text.Trim());
                    cmd.Parameters.AddWithValue("@b", dtpBirthDate.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@oms", txtOMS.Text.Trim());
                    cmd.Parameters.AddWithValue("@ph", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@ad", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@pw", hash);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Пациент добавлен");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}