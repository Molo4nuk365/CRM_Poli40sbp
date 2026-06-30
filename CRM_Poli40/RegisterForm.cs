using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CRM_Poli40
{
    public class RegisterForm : Form
    {
        private TextBox fullNameTextBox, passwordTextBox, omsTextBox, phoneTextBox, emailTextBox, addressTextBox;
        private DateTimePicker birthDatePicker;

        public RegisterForm()
        {
            this.Text = "Регистрация пациента";
            this.Size = new Size(450, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20;
            Label title = new Label
            {
                Text = "Регистрация нового пациента",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                Location = new Point(50, y),
                AutoSize = true
            };
            Controls.Add(title);
            y += 40;

            AddField("ФИО (через пробел):", ref y, out fullNameTextBox);
            AddField("Пароль:", ref y, out passwordTextBox, true);
            AddField("Полис ОМС:", ref y, out omsTextBox);
            AddDateField("Дата рождения:", ref y, out birthDatePicker);
            AddField("Телефон:", ref y, out phoneTextBox);
            AddField("Email:", ref y, out emailTextBox);
            AddField("Адрес:", ref y, out addressTextBox);
            y += 10;

            Button btnRegister = new Button
            {
                Text = "Зарегистрироваться",
                Location = new Point(150, y),
                Width = 260,
                Height = 36,
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnRegister.Click += BtnRegister_Click;
            Controls.Add(btnRegister);
            y += 42;

            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(150, y),
                Width = 260,
                Height = 33,
                BackColor = Color.White,
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => Close();
            Controls.Add(btnCancel);
        }

        private void AddField(string labelText, ref int y, out TextBox textBox, bool isPassword = false)
        {
            Label lbl = new Label 
            { 
                Text = labelText,
                Location = new Point(30, y),
                AutoSize = true 
            };
            textBox = new TextBox
            { 
                Location = new Point(150, y - 3), 
                Width = 260
            };

            if (isPassword) textBox.PasswordChar = '*';
            Controls.Add(lbl);
            Controls.Add(textBox);
            y += 35;
        }

        private void AddDateField(string labelText, ref int y, out DateTimePicker datePicker)
        {
            Label lbl = new Label 
            { 
                Text = labelText, Location = new Point(30, y),
                AutoSize = true
            };
            datePicker = new DateTimePicker
            {
                Location = new Point(150, y - 3),
                Width = 260,
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(1990, 1, 1)
            };
            Controls.Add(lbl);
            Controls.Add(datePicker);
            y += 35;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fullNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(passwordTextBox.Text) ||
                string.IsNullOrWhiteSpace(omsTextBox.Text))
            {
                MessageBox.Show("Заполните обязательные поля: ФИО, Пароль, ОМС");
                return;
            }
            if (omsTextBox.Text.Trim().Length != 16)
            {
                MessageBox.Show("Номер полиса ОМС должен содержать 16 цифр");
                return;
            }

            string[] parts = fullNameTextBox.Text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                MessageBox.Show("Введите как минимум Фамилию и Имя через пробел");
                return;
            }

            string lastName = parts[0];
            string firstName = parts[1];
            string middleName = parts.Length > 2 ? parts[2] : "";

            using (var conn = Database.OpenConnection())
            {
                // Проверка ОМС
                using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Patients WHERE OMS = @oms", conn))
                {
                    checkCmd.Parameters.AddWithValue("@oms", omsTextBox.Text.Trim());
                    if ((int)checkCmd.ExecuteScalar() > 0)
                    {
                        MessageBox.Show("Пациент с таким ОМС уже зарегистрирован");
                        return;
                    }
                }

                string hash = BCrypt.Net.BCrypt.HashPassword(passwordTextBox.Text);
                using (var insertCmd = new SqlCommand(
                    @"INSERT INTO Patients (LastName, FirstName, MiddleName, BirthDate, OMS, Phone, Email, Address, PasswordHash)
                      VALUES (@ln, @fn, @mn, @bd, @oms, @ph, @em, @ad, @pw)", conn))
                {
                    insertCmd.Parameters.AddWithValue("@ln", lastName);
                    insertCmd.Parameters.AddWithValue("@fn", firstName);
                    insertCmd.Parameters.AddWithValue("@mn", middleName);
                    insertCmd.Parameters.AddWithValue("@bd", birthDatePicker.Value.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@oms", omsTextBox.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@ph", phoneTextBox.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@em", emailTextBox.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@ad", addressTextBox.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@pw", hash);
                    insertCmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Регистрация успешно завершена!");
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}