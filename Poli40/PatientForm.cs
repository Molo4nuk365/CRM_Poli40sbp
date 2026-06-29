using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace Poli40
{
    public class PatientForm : Form
    {
        private int patientId;
        private ListBox lstDoctors, lstAppointments;
        private DateTimePicker dtpDate;
        private ComboBox cmbTime;

        public PatientForm(int id, string name)
        {
            patientId = id;
            this.Text = "Пациент: " + name;
            this.Size = new Size(650, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            Label lblDoctors = new Label { Text = "Врачи поликлиники:", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            lstDoctors = new ListBox { Location = new Point(10, 38), Width = 300, Height = 160 };
            Label lblDate = new Label { Text = "Дата:", Location = new Point(10, 210), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new Point(55, 207), Width = 130, Format = DateTimePickerFormat.Short, MinDate = DateTime.Now };
            Label lblTime = new Label { Text = "Время:", Location = new Point(195, 210), AutoSize = true };
            cmbTime = new ComboBox { Location = new Point(250, 207), Width = 75, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTime.Items.AddRange(new[] { "09:00", "10:00", "11:00", "12:00", "14:00", "15:00", "16:00" });
            cmbTime.SelectedIndex = 0;
            Button btnBook = new Button { Text = "Записаться", Location = new Point(10, 248), Width = 315, Height = 35, BackColor = Color.OrangeRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBook.Click += BtnBook_Click;

            Label lblMy = new Label { Text = "Мои записи:", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(340, 10), AutoSize = true };
            lstAppointments = new ListBox { Location = new Point(340, 38), Width = 285, Height = 200 };
            Button btnCancel = new Button { Text = "Отменить запись", Location = new Point(340, 248), Width = 285, Height = 35, BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += BtnCancel_Click;

            this.Controls.AddRange(new Control[] { lblDoctors, lstDoctors, lblDate, dtpDate, lblTime, cmbTime, btnBook, lblMy, lstAppointments, btnCancel });
            LoadDoctors();
            LoadAppointments();
        }

        private void LoadDoctors()
        {
            lstDoctors.Items.Clear();
            using (var conn = Database.GetConnection())
            using (var cmd = new SQLiteCommand("SELECT Id, LastName, FirstName, MiddleName, Specialization, Office FROM Doctors", conn))
            using (var r = cmd.ExecuteReader())
                while (r.Read())
                    lstDoctors.Items.Add(new DoctorItem { Id = r.GetInt32(0), Text = $"{r.GetString(1)} {r.GetString(2)}{(r.IsDBNull(3) ? "" : " " + r.GetString(3))} | {r.GetString(4)} | Каб. {r.GetString(5)}" });
        }

        private void LoadAppointments()
        {
            lstAppointments.Items.Clear();
            using (var conn = Database.GetConnection())
            using (var cmd = new SQLiteCommand(@"SELECT a.Id, a.AppointmentDate, a.AppointmentTime, a.Status, d.LastName||' '||d.FirstName 
                FROM Appointments a JOIN Doctors d ON a.DoctorId=d.Id WHERE a.PatientId=@pid ORDER BY a.AppointmentDate DESC", conn))
            {
                cmd.Parameters.AddWithValue("@pid", patientId);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lstAppointments.Items.Add(new AppItem { Id = r.GetInt32(0), Text = $"[{r.GetString(3)}] {r.GetString(1)} {r.GetString(2)} — {r.GetString(4)}" });
            }
        }

        private void BtnBook_Click(object sender, EventArgs e)
        {
            if (lstDoctors.SelectedItem == null) { MessageBox.Show("Выберите врача"); return; }
            var doc = (DoctorItem)lstDoctors.SelectedItem;
            string date = dtpDate.Value.ToString("yyyy-MM-dd"), time = cmbTime.Text;
            using (var conn = Database.GetConnection())
            {
                using (var cmd = new SQLiteCommand("INSERT INTO Appointments (PatientId,DoctorId,AppointmentDate,AppointmentTime) VALUES (@p,@d,@dt,@tm)", conn))
                {
                    cmd.Parameters.AddWithValue("@p", patientId);
                    cmd.Parameters.AddWithValue("@d", doc.Id);
                    cmd.Parameters.AddWithValue("@dt", date);
                    cmd.Parameters.AddWithValue("@tm", time);
                    cmd.ExecuteNonQuery();
                }
            }
            LoadAppointments();
            MessageBox.Show("Запись создана!");
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (lstAppointments.SelectedItem == null) { MessageBox.Show("Выберите запись"); return; }
            var app = (AppItem)lstAppointments.SelectedItem;
            using (var conn = Database.GetConnection())
            using (var cmd = new SQLiteCommand("UPDATE Appointments SET Status='Отменен' WHERE Id=@id AND Status='Записан'", conn))
            {
                cmd.Parameters.AddWithValue("@id", app.Id);
                if (cmd.ExecuteNonQuery() > 0) MessageBox.Show("Запись отменена");
                else MessageBox.Show("Нельзя отменить");
            }
            LoadAppointments();
        }

        class DoctorItem { public int Id; public string Text; public override string ToString() => Text; }
        class AppItem { public int Id; public string Text; public override string ToString() => Text; }
    }
}