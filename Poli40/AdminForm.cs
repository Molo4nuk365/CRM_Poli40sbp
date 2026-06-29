using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace Poli40
{
    public class AdminForm : Form
    {
        private ListBox lstPatients, lstDoctors, lstAppointments;

        public AdminForm(string adminName)
        {
            this.Text = "Администратор: " + adminName;
            this.Size = new Size(870, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            Label lblTitle = new Label { Text = "Панель администратора", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.OrangeRed, Location = new Point(15, 12), AutoSize = true };

            // Пациенты
            Label lblPat = new Label { Text = "Пациенты:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(15, 55), AutoSize = true };
            lstPatients = new ListBox { Location = new Point(15, 80), Width = 260, Height = 160 };
            Button btnAddPatient = new Button { Text = "Добавить пациента", Location = new Point(15, 250), Width = 125, Height = 30, BackColor = Color.ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddPatient.Click += (s, e) => { new AddPatientForm().ShowDialog(); LoadAll(); };
            Button btnDelPatient = new Button { Text = "Удалить пациента", Location = new Point(150, 250), Width = 125, Height = 30, BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelPatient.Click += BtnDelPatient_Click;

            // Врачи
            Label lblDoc = new Label { Text = "Врачи:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(295, 55), AutoSize = true };
            lstDoctors = new ListBox { Location = new Point(295, 80), Width = 260, Height = 160 };
            Button btnAddDoctor = new Button { Text = "Добавить врача", Location = new Point(295, 250), Width = 120, Height = 30, BackColor = Color.ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddDoctor.Click += (s, e) => { new AddDoctorForm().ShowDialog(); LoadAll(); };
            Button btnDelDoc = new Button { Text = "Удалить врача", Location = new Point(425, 250), Width = 130, Height = 30, BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelDoc.Click += BtnDelDoc_Click;

            // Записи
            Label lblApps = new Label { Text = "Все записи:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(15, 295), AutoSize = true };
            lstAppointments = new ListBox { Location = new Point(15, 320), Width = 540, Height = 125 };
            Button btnRefresh = new Button { Text = "Обновить", Location = new Point(580, 80), Width = 100, Height = 32, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => LoadAll();

            this.Controls.AddRange(new Control[] {
                lblTitle, lblPat, lstPatients, btnAddPatient, btnDelPatient,
                lblDoc, lstDoctors, btnAddDoctor, btnDelDoc,
                lblApps, lstAppointments, btnRefresh
            });

            LoadAll();
        }

        private void LoadAll()
        {
            lstPatients.Items.Clear();
            lstDoctors.Items.Clear();
            lstAppointments.Items.Clear();

            using (var conn = Database.GetConnection())
            {
                // Пациенты
                using (var cmd = new SQLiteCommand("SELECT Id, LastName, FirstName, MiddleName, OMS, Phone FROM Patients", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string mid = r.IsDBNull(2) ? "" : " " + r.GetString(2);
                        string phone = r.IsDBNull(5) ? "" : " | " + r.GetString(5);
                        lstPatients.Items.Add(new PatientItem { Id = r.GetInt32(0), Text = $"{r.GetString(1)} {r.GetString(0)}{mid} | ОМС: {r.GetString(4)}{phone}" });
                    }
                }

                // Врачи
                using (var cmd = new SQLiteCommand("SELECT Id, LastName, FirstName, MiddleName, Specialization, Office FROM Doctors", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string mid = r.IsDBNull(3) ? "" : " " + r.GetString(3);
                        lstDoctors.Items.Add(new DoctorItem { Id = r.GetInt32(0), Text = $"{r.GetString(1)} {r.GetString(2)}{mid} | {r.GetString(4)} | Каб. {r.GetString(5)}" });
                    }
                }

                // Записи
                using (var cmd = new SQLiteCommand(@"SELECT a.AppointmentDate, a.AppointmentTime, a.Status, 
                    p.LastName||' '||p.FirstName, d.LastName||' '||d.FirstName 
                    FROM Appointments a 
                    JOIN Patients p ON a.PatientId = p.Id 
                    JOIN Doctors d ON a.DoctorId = d.Id 
                    ORDER BY a.AppointmentDate DESC", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        lstAppointments.Items.Add($"[{r.GetString(2)}] {r.GetString(0)} {r.GetString(1)} | {r.GetString(3)} → {r.GetString(4)}");
                }
            }
        }

        private void BtnDelPatient_Click(object sender, EventArgs e)
        {
            if (lstPatients.SelectedItem == null) { MessageBox.Show("Выберите пациента"); return; }
            var pat = (PatientItem)lstPatients.SelectedItem;
            if (MessageBox.Show($"Удалить пациента и все его записи?\n{pat.Text}", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            using (var conn = Database.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                new SQLiteCommand("DELETE FROM Appointments WHERE PatientId=" + pat.Id, conn).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM Patients WHERE Id=" + pat.Id, conn).ExecuteNonQuery();
                trans.Commit();
            }
            LoadAll();
        }

        private void BtnDelDoc_Click(object sender, EventArgs e)
        {
            if (lstDoctors.SelectedItem == null) { MessageBox.Show("Выберите врача"); return; }
            var doc = (DoctorItem)lstDoctors.SelectedItem;
            if (MessageBox.Show($"Удалить врача и все его записи?\n{doc.Text}", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            using (var conn = Database.GetConnection())
            using (var trans = conn.BeginTransaction())
            {
                new SQLiteCommand("DELETE FROM Appointments WHERE DoctorId=" + doc.Id, conn).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM Doctors WHERE Id=" + doc.Id, conn).ExecuteNonQuery();
                trans.Commit();
            }
            LoadAll();
        }

        class PatientItem { public int Id; public string Text; public override string ToString() => Text; }
        class DoctorItem { public int Id; public string Text; public override string ToString() => Text; }
    }
}