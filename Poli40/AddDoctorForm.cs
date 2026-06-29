using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace Poli40
{
    public class AddDoctorForm : Form
    {
        private TextBox txtLastName, txtFirstName, txtMiddleName, txtSpecialization, txtOffice, txtLogin, txtPassword;

        public AddDoctorForm()
        {
            this.Text = "Добавить врача";
            this.Size = new Size(400, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 15, lx = 20, fx = 140, fw = 220;

            AddRow("Фамилия:", ref y, lx, fx, fw, out txtLastName);
            AddRow("Имя:", ref y, lx, fx, fw, out txtFirstName);
            AddRow("Отчество:", ref y, lx, fx, fw, out txtMiddleName);
            AddRow("Специализация:", ref y, lx, fx, fw, out txtSpecialization);
            AddRow("Кабинет:", ref y, lx, fx, fw, out txtOffice);
            AddRow("Логин:", ref y, lx, fx, fw, out txtLogin);
            AddRow("Пароль:", ref y, lx, fx, fw, out txtPassword, true);
            y += 10;

            Button btnSave = new Button { Text = "Сохранить", Location = new Point(fx, y), Width = fw, Height = 34, BackColor = Color.ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void AddRow(string label, ref int y, int lx, int fx, int fw, out TextBox tb, bool password = false)
        {
            this.Controls.Add(new Label { Text = label, Location = new Point(lx, y), AutoSize = true });
            tb = new TextBox { Location = new Point(fx, y - 3), Width = fw };
            if (password) tb.PasswordChar = '●';
            this.Controls.Add(tb);
            y += 35;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtSpecialization.Text) || string.IsNullOrWhiteSpace(txtLogin.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Фамилия, Имя, Специализация, Логин и Пароль обязательны");
                return;
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    var check = new SQLiteCommand("SELECT COUNT(*) FROM Doctors WHERE Login=@log", conn);
                    check.Parameters.AddWithValue("@log", txtLogin.Text.Trim());
                    if ((long)check.ExecuteScalar() > 0) { MessageBox.Show("Врач с таким логином уже существует"); return; }

                    var cmd = new SQLiteCommand(@"INSERT INTO Doctors (LastName,FirstName,MiddleName,Specialization,Office,Login,Password) 
                        VALUES (@l,@f,@m,@s,@o,@log,@p)", conn);
                    cmd.Parameters.AddWithValue("@l", txtLastName.Text.Trim());
                    cmd.Parameters.AddWithValue("@f", txtFirstName.Text.Trim());
                    cmd.Parameters.AddWithValue("@m", txtMiddleName.Text.Trim());
                    cmd.Parameters.AddWithValue("@s", txtSpecialization.Text.Trim());
                    cmd.Parameters.AddWithValue("@o", txtOffice.Text.Trim());
                    cmd.Parameters.AddWithValue("@log", txtLogin.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", txtPassword.Text);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Врач добавлен");
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }
    }
}