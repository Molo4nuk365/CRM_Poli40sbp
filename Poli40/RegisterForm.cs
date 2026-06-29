using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace Poli40
{
    public class RegisterForm : Form
    {
        private TextBox txtFullName, txtPassword, txtOMS, txtPhone, txtEmail, txtAddress;
        private DateTimePicker dtpBirthDate;

        public RegisterForm()
        {
            this.Text = "Регистрация пациента";
            this.Size = new Size(450, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20, lx = 30, fx = 150, fw = 260;
            this.Controls.Add(new Label { Text = "Регистрация нового пациента", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.OrangeRed, Location = new Point(50, y), AutoSize = true });
            y += 40;

            AddRow("ФИО (через пробел):", ref y, lx, fx, fw, out txtFullName);
            AddRow("Пароль:", ref y, lx, fx, fw, out txtPassword, true);
            AddRow("Полис ОМС:", ref y, lx, fx, fw, out txtOMS); txtOMS.MaxLength = 16;

            this.Controls.Add(new Label { Text = "Дата рождения:", Location = new Point(lx, y), AutoSize = true });
            dtpBirthDate = new DateTimePicker { Location = new Point(fx, y - 3), Width = fw, Format = DateTimePickerFormat.Short, Value = new DateTime(1990, 1, 1) };
            this.Controls.Add(dtpBirthDate);
            y += 35;

            AddRow("Телефон:", ref y, lx, fx, fw, out txtPhone);
            AddRow("Email:", ref y, lx, fx, fw, out txtEmail);
            AddRow("Адрес:", ref y, lx, fx, fw, out txtAddress);
            y += 10;

            Button btnReg = new Button { Text = "Зарегистрироваться", Location = new Point(fx, y), Width = fw, Height = 36, BackColor = Color.OrangeRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnReg.Click += BtnRegister_Click;
            this.Controls.Add(btnReg);
            y += 42;

            Button btnCancel = new Button { Text = "Отмена", Location = new Point(fx, y), Width = fw, Height = 33, BackColor = Color.White, ForeColor = Color.Gray, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void AddRow(string label, ref int y, int lx, int fx, int fw, out TextBox tb, bool password = false)
        {
            this.Controls.Add(new Label { Text = label, Location = new Point(lx, y), AutoSize = true });
            tb = new TextBox { Location = new Point(fx, y - 3), Width = fw };
            if (password) tb.PasswordChar = '●';
            this.Controls.Add(tb);
            y += 35;
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text) || string.IsNullOrWhiteSpace(txtOMS.Text))
            { MessageBox.Show("Заполните ФИО, Пароль, ОМС"); return; }
            if (txtOMS.Text.Trim().Length != 16) { MessageBox.Show("ОМС — 16 цифр"); return; }
            var parts = txtFullName.Text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) { MessageBox.Show("Фамилия и Имя обязательны"); return; }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    var check = new SQLiteCommand("SELECT COUNT(*) FROM Patients WHERE OMS=@oms", conn);
                    check.Parameters.AddWithValue("@oms", txtOMS.Text.Trim());
                    if ((long)check.ExecuteScalar() > 0) { MessageBox.Show("Такой ОМС уже есть"); return; }

                    var cmd = new SQLiteCommand(@"INSERT INTO Patients (LastName,FirstName,MiddleName,BirthDate,OMS,Phone,Email,Address,Password) 
                        VALUES (@l,@f,@m,@b,@o,@ph,@em,@ad,@p)", conn);
                    cmd.Parameters.AddWithValue("@l", parts[0]);
                    cmd.Parameters.AddWithValue("@f", parts[1]);
                    cmd.Parameters.AddWithValue("@m", parts.Length > 2 ? parts[2] : "");
                    cmd.Parameters.AddWithValue("@b", dtpBirthDate.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@o", txtOMS.Text.Trim());
                    cmd.Parameters.AddWithValue("@ph", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@ad", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", txtPassword.Text);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Регистрация успешна!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }
    }
}