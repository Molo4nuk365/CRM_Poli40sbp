using System;
using System.Drawing;
using System.Windows.Forms;

namespace CRM_Poli40
{
    public class LoginForm : Form
    {
        private TabControl tabControl;
        private TextBox adminLoginTextBox, adminPasswordTextBox;
        private TextBox doctorLoginTextBox, doctorPasswordTextBox;
        private TextBox patientOmsTextBox, patientPasswordTextBox;

        public LoginForm()
        {
            this.Text = "Поликлиника №40 — Вход";
            this.Size = new Size(450, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            // Логотип (убедитесь, что файл logo.png лежит в папке Images)
            try
            {
                PictureBox logo = new PictureBox
                {
                    Size = new Size(80, 80),
                    Location = new Point(185, 10),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(Application.StartupPath + "\\Images\\logo.png")
                };
                this.Controls.Add(logo);
            }
            catch { /* если нет логотипа – просто пропустим */ }

            Label titleLabel = new Label
            {
                Text = "Поликлиника №40",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                Location = new Point(120, 95),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            tabControl = new TabControl { Location = new Point(15, 140), Size = new Size(405, 230) };

            // Администратор
            TabPage adminTab = new TabPage("Администратор");
            AddField(adminTab, "Логин:", 20, out adminLoginTextBox);
            AddField(adminTab, "Пароль:", 60, out adminPasswordTextBox, true);
            Button btnAdmin = new Button
            {
                Text = "Войти",
                Location = new Point(100, 105),
                Width = 270,
                Height = 33,
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdmin.Click += (s, e) =>
            {
                var res = Database.LoginAdmin(adminLoginTextBox.Text.Trim(), adminPasswordTextBox.Text);
                if (res.id == -1) { MessageBox.Show("Неверный логин или пароль"); return; }
                Hide(); new AdminForm(res.fullName).ShowDialog(); Show();
            };
            adminTab.Controls.Add(btnAdmin);

            // Врач
            TabPage doctorTab = new TabPage("Врач");
            AddField(doctorTab, "Логин:", 20, out doctorLoginTextBox);
            AddField(doctorTab, "Пароль:", 60, out doctorPasswordTextBox, true);
            Button btnDoc = new Button
            {
                Text = "Войти",
                Location = new Point(100, 105),
                Width = 270,
                Height = 33,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDoc.Click += (s, e) =>
            {
                var res = Database.LoginDoctor(doctorLoginTextBox.Text.Trim(), doctorPasswordTextBox.Text);
                if (res.id == -1) { MessageBox.Show("Неверный логин или пароль"); return; }
                Hide(); new DoctorForm(res.id, res.ln + " " + res.fn).ShowDialog(); Show();
            };
            doctorTab.Controls.Add(btnDoc);

            // Пациент
            TabPage patientTab = new TabPage("Пациент");
            AddField(patientTab, "ОМС:", 20, out patientOmsTextBox);
            AddField(patientTab, "Пароль:", 60, out patientPasswordTextBox, true);
            Button btnPat = new Button
            {
                Text = "Войти",
                Location = new Point(100, 105),
                Width = 130,
                Height = 33,
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPat.Click += (s, e) =>
            {
                var res = Database.LoginPatient(patientOmsTextBox.Text.Trim(), patientPasswordTextBox.Text);
                if (res.id == -1) { MessageBox.Show("Неверный ОМС или пароль"); return; }
                Hide(); new PatientForm(res.id, res.ln + " " + res.fn).ShowDialog(); Show();
            };
            patientTab.Controls.Add(btnPat);
            Button btnReg = new Button
            {
                Text = "Регистрация",
                Location = new Point(240, 105),
                Width = 130,
                Height = 33,
                BackColor = Color.White,
                ForeColor = Color.Green,
                FlatStyle = FlatStyle.Flat
            };
            btnReg.Click += (s, e) => new RegisterForm().ShowDialog();
            patientTab.Controls.Add(btnReg);

            tabControl.TabPages.Add(adminTab);
            tabControl.TabPages.Add(doctorTab);
            tabControl.TabPages.Add(patientTab);
            this.Controls.Add(tabControl);
        }

        private void AddField(TabPage page, string labelText, int y, out TextBox textBox, bool isPassword = false)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(15, y), AutoSize = true };
            textBox = new TextBox { Location = new Point(100, y - 3), Width = 270 };
            if (isPassword) textBox.PasswordChar = '*';
            page.Controls.Add(lbl);
            page.Controls.Add(textBox);
        }
    }
}
