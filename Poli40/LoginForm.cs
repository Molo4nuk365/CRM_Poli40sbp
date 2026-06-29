using System;
using System.Drawing;
using System.Windows.Forms;

namespace Poli40
{
    public class LoginForm : Form
    {
        private TabControl tabControl;
        private TextBox txtAdminLogin, txtAdminPass;
        private TextBox txtDocLogin, txtDocPass;
        private TextBox txtPatOMS, txtPatPass;

        public LoginForm()
        {
            this.Text = "Поликлиника №40 — Вход";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            Label lblTitle = new Label
            {
                Text = "Поликлиника №40",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                Location = new Point(110, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            tabControl = new TabControl 
            { 
                Location = new Point(15, 70), Size = new Size(405, 270) 
            };

            // Администратор
            TabPage tabAdmin = new TabPage("Администратор");
            Label la1 = new Label { 
                Text = "Логин:", Location = new Point(15, 20), AutoSize = true 
            };
            txtAdminLogin = new TextBox 
            {
                Location = new Point(100, 17), Width = 270
            };
               Label la2 = new Label { Text = "Пароль:", Location = new Point(15, 55), AutoSize = true
            };
            txtAdminPass = new TextBox  
            { 
                Location = new Point(100, 52), Width = 270, PasswordChar = '*' 
            };
            Button btnAdmin = new Button 
            { Text = "Войти", Location = new Point(100, 95), Width = 270, Height = 33, BackColor = Color.OrangeRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat 
            };
            btnAdmin.Click += (s, e) => 
            {
                var a = Database.LoginAdmin(txtAdminLogin.Text.Trim(), txtAdminPass.Text);
                if (a == null) { MessageBox.Show("Неверные данные"); return; }
                this.Hide(); new AdminForm(a.Value.name).ShowDialog(); this.Show();
            };
            tabAdmin.Controls.AddRange(new Control[] { la1, txtAdminLogin, la2, txtAdminPass, btnAdmin });

            // Врач (логин + пароль)
            TabPage tabDoctor = new TabPage("Врач");
            Label ld1 = new Label 
            { 
                Text = "Логин:", Location = new Point(15, 20), AutoSize = true 
            };
            txtDocLogin = new TextBox { Location = new Point(100, 17), Width = 270 
            };
            Label ld2 = new Label { Text = "Пароль:", Location = new Point(15, 55), AutoSize = true 
            };
            txtDocPass = new TextBox { Location = new Point(100, 52), Width = 270, PasswordChar = '*' 
            };
            Button btnDoc = new Button { 
                Text = "Войти", Location = new Point(100, 95), Width = 270, Height = 33, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnDoc.Click += (s, e) =>
            {
                string login = txtDocLogin.Text.Trim();
                string pass = txtDocPass.Text;
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
                { MessageBox.Show("Введите логин и пароль"); return; }
                var d = Database.LoginDoctor(login, pass);
                if (d == null) { MessageBox.Show("Неверные данные"); return; }
                this.Hide(); new DoctorForm(d.Value.id, $"{d.Value.lastName} {d.Value.firstName}").ShowDialog(); this.Show();
            };
            tabDoctor.Controls.AddRange(new Control[] { ld1, txtDocLogin, ld2, txtDocPass, btnDoc });

            // Пациент
            TabPage tabPatient = new TabPage("Пациент");
            Label lp1 = new Label 
            { Text = "ОМС:", Location = new Point(15, 20), AutoSize = true 
            };
            txtPatOMS = new TextBox 
            { Location = new Point(100, 17), Width = 270 
            };
            Label lp2 = new Label 
            { Text = "Пароль:", Location = new Point(15, 55), AutoSize = true
            };
            txtPatPass = new TextBox
            { Location = new Point(100, 52), Width = 270, PasswordChar = '*' 
            };
            Button btnPat = new Button
            { 
                Text = "Войти", Location = new Point(100, 95), Width = 130, Height = 33, BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPat.Click += (s, e) =>
            {
                var p = Database.LoginPatient(txtPatOMS.Text.Trim(), txtPatPass.Text);
                if (p == null)
                { 
                    MessageBox.Show("Неверные данные"); return; 
                }
                this.Hide(); new PatientForm(p.Value.id, $"" +
                    $"{p.Value.lastName} {p.Value.firstName}")
                .ShowDialog(); 
                this.Show();
            };
            Button btnReg = new Button 
            {
                Text = "Регистрация", Location = new Point(240, 95), Width = 130, Height = 33, BackColor = Color.White, ForeColor = Color.Green, FlatStyle = FlatStyle.Flat
            };
            btnReg.Click += (s, e) => new RegisterForm().ShowDialog();
            tabPatient.Controls.AddRange(new Control[] 
            { 
                lp1, txtPatOMS, lp2, txtPatPass, btnPat, btnReg
            });

            tabControl.TabPages.Add(tabAdmin);
            tabControl.TabPages.Add(tabDoctor);
            tabControl.TabPages.Add(tabPatient);
            this.Controls.Add(tabControl);
        }
    }
}