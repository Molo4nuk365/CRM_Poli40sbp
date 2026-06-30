using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CRM_Poli40
{
    public class PatientForm : Form
    {
        private int patientId;
        private ListBox lstDoctors, lstAppointments;
        private DateTimePicker dtpDate;
        private ComboBox cmbTime;

        public PatientForm(int id, string fullName)
        {
            patientId = id;
            this.Text = "Пациент: " + fullName;
            this.Size = new Size(650, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            // Логотип
            try
            {
                PictureBox logo = new PictureBox
                {
                    Size = new Size(40, 40),
                    Location = new Point(590, 5),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(Application.StartupPath + "\\Images\\logo.png")
                };
                Controls.Add(logo);
            }
            catch { }

            // Список врачей
            Label lblDoc = new Label { Text = "Врачи:", Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            lstDoctors = new ListBox { Location = new Point(10, 38), Width = 300, Height = 160 };
            Label lblDate = new Label { Text = "Дата:", Location = new Point(10, 210), AutoSize = true };
            dtpDate = new DateTimePicker
            {
                Location = new Point(55, 207),
                Width = 130,
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Now
            };
            Label lblTime = new Label { Text = "Время:", Location = new Point(195, 210), AutoSize = true };
            cmbTime = new ComboBox
            {
                Location = new Point(250, 207),
                Width = 75,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTime.Items.AddRange(new[] { "09:00", "10:00", "11:00", "12:00", "14:00", "15:00", "16:00" });
            cmbTime.SelectedIndex = 0;

            Button btnBook = new Button
            {
                Text = "Записаться",
                Location = new Point(10, 248),
                Width = 315,
                Height = 35,
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBook.Click += (s, e) => BookAppointment();

            // Мои записи
            Label lblAppt = new Label
            { 
                Text = "Мои записи:", 
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(340, 10),
                AutoSize = true 
            };
            lstAppointments = new ListBox 
            { 
                Location = new Point(340, 38), Width = 285, Height = 200
            };
            Button btnCancel = new Button
            {
                Text = "Отменить",
                Location = new Point(340, 248),
                Width = 285,
                Height = 35,
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => CancelAppointment();

            Controls.Add(lblDoc); Controls.Add(lstDoctors);
            Controls.Add(lblDate); Controls.Add(dtpDate);
            Controls.Add(lblTime); Controls.Add(cmbTime);
            Controls.Add(btnBook);
            Controls.Add(lblAppt); Controls.Add(lstAppointments);
            Controls.Add(btnCancel);

            LoadDoctors();
            LoadAppointments();
        }

        private void LoadDoctors()
        {
            lstDoctors.Items.Clear();
            using (var conn = Database.OpenConnection())
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
        }

        private void LoadAppointments()
        {
            lstAppointments.Items.Clear();
            using (var conn = Database.OpenConnection())
            using (var cmd = new SqlCommand(
                @"SELECT a.Id, a.AppointmentDate, a.AppointmentTime, a.Status, d.LastName + ' ' + d.FirstName
                  FROM Appointments a JOIN Doctors d ON a.DoctorId = d.Id
                  WHERE a.PatientId = @pid
                  ORDER BY a.AppointmentDate DESC, a.AppointmentTime DESC", conn))
            {
                cmd.Parameters.AddWithValue("@pid", patientId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstAppointments.Items.Add(new AppItem
                        {
                            Id = reader.GetInt32(0),
                            Text = $"[{reader.GetString(3)}] {reader.GetString(1)} {reader.GetString(2)} — {reader.GetString(4)}"
                        });
                    }
                }
            }
        }

        private void BookAppointment()
        {
            if (lstDoctors.SelectedItem == null)
            {
                MessageBox.Show("Выберите врача");
                return;
            }
            var doc = (DoctorItem)lstDoctors.SelectedItem;
            string date = dtpDate.Value.ToString("yyyy-MM-dd");
            string time = cmbTime.Text;

            using (var conn = Database.OpenConnection())
            using (var cmd = new SqlCommand(
                "INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate, AppointmentTime) VALUES (@pid, @did, @d, @t)", conn))
            {
                cmd.Parameters.AddWithValue("@pid", patientId);
                cmd.Parameters.AddWithValue("@did", doc.Id);
                cmd.Parameters.AddWithValue("@d", date);
                cmd.Parameters.AddWithValue("@t", time);
                cmd.ExecuteNonQuery();
            }
            LoadAppointments();
            MessageBox.Show("Запись создана");
        }

        //  отмена записи – убрано условие на статус "Записан"
        private void CancelAppointment()
        {
            if (lstAppointments.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }
            var app = (AppItem)lstAppointments.SelectedItem;
            using (var conn = Database.OpenConnection())
            using (var cmd = new SqlCommand(
                "UPDATE Appointments SET Status = N'Отменен' WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", app.Id);
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                    MessageBox.Show("Не удалось отменить запись (возможно, она уже удалена)");
                else
                {
                    LoadAppointments();
                    MessageBox.Show("Запись отменена");
                }
            }
        }

        class DoctorItem 
        { 
            public int Id;
            public string Text;
            public override string ToString() => Text;
        }
        class AppItem
        { 
            public int Id;
            public string Text;
            public override string ToString() => Text;
        }
    }
}