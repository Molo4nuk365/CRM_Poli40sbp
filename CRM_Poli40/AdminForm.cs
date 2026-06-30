using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CRM_Poli40
{
    public class AdminForm : Form
    {
        private ListBox lstPatients, lstDoctors, lstAppointments;

        public AdminForm(string adminName)
        {
            this.Text = "Администратор: " + adminName;
            this.Size = new Size(930, 640); // Увеличенный размер
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            // Логотип (опционально)
            try
            {
                PictureBox logo = new PictureBox
                {
                    Size = new Size(80, 80),
                    Location = new Point(780, 5),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(Application.StartupPath + "\\Images\\logo.png")
                };
                Controls.Add(logo);
            }
            catch { }

            Label lblTitle = new Label
            {
                Text = "Панель администратора",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.OrangeRed,
                Location = new Point(15, 12),
                AutoSize = true
            };
            Controls.Add(lblTitle);

            // Пациенты
            Label lblP = new Label { Text = "Пациенты:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(15, 55), AutoSize = true };
            lstPatients = new ListBox { Location = new Point(15, 80), Width = 280, Height = 180 }; // Увеличены ширина и высота
            // Врачи
            Label lblD = new Label { Text = "Врачи:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(320, 55), AutoSize = true };
            lstDoctors = new ListBox { Location = new Point(320, 80), Width = 280, Height = 180 };
            Controls.Add(lblP); Controls.Add(lstPatients);
            Controls.Add(lblD); Controls.Add(lstDoctors);

            // Кнопки управления
            Button btnAddDoctor = new Button
            {
                Text = "Добавить врача",
                Location = new Point(15, 270),
                Width = 280,
                Height = 32,
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddDoctor.Click += (s, e) => { new AddDoctorForm().ShowDialog(); LoadData(); };
            Controls.Add(btnAddDoctor);

            Button btnAddPatient = new Button
            {
                Text = "Добавить пациента",
                Location = new Point(15, 308),
                Width = 280,
                Height = 32,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddPatient.Click += (s, e) => { new AddPatientForm().ShowDialog(); LoadData(); };
            Controls.Add(btnAddPatient);

            // Удалить пациента
            Button btnDelPatient = new Button
            {
                Text = "Удалить пациента",
                Location = new Point(15, 346),
                Width = 280,
                Height = 32,
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDelPatient.Click += DeletePatient;
            Controls.Add(btnDelPatient);

            // Удалить врача
            Button btnDelDoctor = new Button
            {
                Text = "Удалить врача",
                Location = new Point(320, 270),
                Width = 280,
                Height = 32,
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDelDoctor.Click += DeleteDoctor;
            Controls.Add(btnDelDoctor);

            // Все записи
            Label lblA = new Label 
            { 
                Text = "Все записи:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(15, 395),
                AutoSize = true
            };
            lstAppointments = new ListBox
            { 
                Location = new Point(15, 420),
                Width = 585,
                Height = 100 
            };
            Controls.Add(lblA); Controls.Add(lstAppointments);

            // Кнопка обновления
            Button btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(620, 80),
                Width = 150,
                Height = 45,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += (s, e) => LoadData();
            Controls.Add(btnRefresh);

            LoadData();
        }

        private void LoadData()
        {
            lstPatients.Items.Clear();
            lstDoctors.Items.Clear();
            lstAppointments.Items.Clear();

            using (var conn = Database.OpenConnection())
            {
                // Пациенты (с Id)
                using (var cmd = new SqlCommand(
                    "SELECT Id, LastName, FirstName, MiddleName, OMS, Phone FROM Patients ORDER BY LastName", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string mid = reader.IsDBNull(3) ? "" : " " + reader.GetString(3);
                        string phone = reader.IsDBNull(5) ? "" : " | " + reader.GetString(5);
                        lstPatients.Items.Add(new PatientItem
                        {
                            Id = reader.GetInt32(0),
                            Text = $"{reader.GetString(1)} {reader.GetString(2)}{mid} | ОМС: {reader.GetString(4)}{phone}"
                        });
                    }
                }

                // Врачи (с Id)
                using (var cmd = new SqlCommand(
                    "SELECT Id, LastName, FirstName, MiddleName, Specialization, Office FROM Doctors ORDER BY LastName", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string mid = reader.IsDBNull(3) ? "" : " " + reader.GetString(3);
                        lstDoctors.Items.Add(new DoctorItem
                        {
                            Id = reader.GetInt32(0),
                            Text = $"{reader.GetString(1)} {reader.GetString(2)}{mid} | {reader.GetString(4)} | Каб. {reader.GetString(5)}"
                        });
                    }
                }

                // Все записи
                using (var cmd = new SqlCommand(
                    @"SELECT a.AppointmentDate, a.AppointmentTime, a.Status,
                             p.LastName + ' ' + p.FirstName, d.LastName + ' ' + d.FirstName
                      FROM Appointments a
                      JOIN Patients p ON a.PatientId = p.Id
                      JOIN Doctors d ON a.DoctorId = d.Id
                      ORDER BY a.AppointmentDate DESC, a.AppointmentTime DESC", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstAppointments.Items.Add(
                            $"[{reader.GetString(2)}] {reader.GetString(0)} {reader.GetString(1)} | {reader.GetString(3)} → {reader.GetString(4)}");
                    }
                }
            }
        }

        // Удаление пациента
        private void DeletePatient(object sender, EventArgs e)
        {
            if (lstPatients.SelectedItem == null)
            {
                MessageBox.Show("Выберите пациента");
                return;
            }
            var patient = (PatientItem)lstPatients.SelectedItem;
            if (MessageBox.Show($"Удалить пациента {patient.Text} и все его записи?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using (var conn = Database.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                using (var cmd1 = new SqlCommand("DELETE FROM Appointments WHERE PatientId = @id", conn, trans))
                {
                    cmd1.Parameters.AddWithValue("@id", patient.Id);
                    cmd1.ExecuteNonQuery();
                }
                using (var cmd2 = new SqlCommand("DELETE FROM Patients WHERE Id = @id", conn, trans))
                {
                    cmd2.Parameters.AddWithValue("@id", patient.Id);
                    cmd2.ExecuteNonQuery();
                }
                trans.Commit();
            }
            LoadData();
            MessageBox.Show("Пациент удалён");
        }

        // Удаление врача
        private void DeleteDoctor(object sender, EventArgs e)
        {
            if (lstDoctors.SelectedItem == null)
            {
                MessageBox.Show("Выберите врача");
                return;
            }
            var doc = (DoctorItem)lstDoctors.SelectedItem;
            if (MessageBox.Show($"Удалить врача {doc.Text} и все его записи?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using (var conn = Database.OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                using (var cmd1 = new SqlCommand("DELETE FROM Appointments WHERE DoctorId = @id", conn, trans))
                {
                    cmd1.Parameters.AddWithValue("@id", doc.Id);
                    cmd1.ExecuteNonQuery();
                }
                using (var cmd2 = new SqlCommand("DELETE FROM Doctors WHERE Id = @id", conn, trans))
                {
                    cmd2.Parameters.AddWithValue("@id", doc.Id);
                    cmd2.ExecuteNonQuery();
                }
                trans.Commit();
            }
            LoadData();
            MessageBox.Show("Врач удалён");
        }

        // Вспомогательные классы
        class PatientItem
        {
            public int Id;
            public string Text;
            public override string ToString() => Text;
        }

        class DoctorItem
        {
            public int Id;
            public string Text;
            public override string ToString() => Text;
        }
    }
}