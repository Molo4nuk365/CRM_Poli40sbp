using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CRM_Poli40
{
    public class DoctorForm : Form
    {
        private int doctorId;
        private DateTimePicker dtpDate;
        private ListBox lstSchedule;

        public DoctorForm(int id, string fullName)
        {
            doctorId = id;
            this.Text = "Врач: " + fullName;
            this.Size = new Size(530, 410);
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
                    Location = new Point(475, 5),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(Application.StartupPath + "\\Images\\logo.png")
                };
                Controls.Add(logo);
            }
            catch { }

            Label lblDate = new Label 
            { 
                Text = "Дата приёма:", Location = new Point(10, 12), AutoSize = true
            };
            
            dtpDate = new DateTimePicker

            {
                Location = new Point(120, 9),
                Width = 130,
                Format = DateTimePickerFormat.Short
            };
            dtpDate.ValueChanged += (s, e) => LoadSchedule();
            Controls.Add(lblDate);
            Controls.Add(dtpDate);

            lstSchedule = new ListBox
            { 
                Location = new Point(10, 40),
                Width = 500, 
                Height = 230 };
                Controls.Add(lstSchedule);

            Button btnDone = new Button
            {
                Text = "Был",
                Location = new Point(10, 285),
                Width = 160,
                Height = 35,
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = "Завершен"
            };
            Button btnCancel = new Button
            {
                Text = "Отменён",
                Location = new Point(180, 285),
                Width = 160,
                Height = 35,
                BackColor = Color.Gold,
                FlatStyle = FlatStyle.Flat,
                Tag = "Отменен"
            };
            Button btnMiss = new Button
            {
                Text = "Не явился",
                Location = new Point(350, 285),
                Width = 160,
                Height = 35,
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = "Не явился"
            };
            btnDone.Click += ChangeStatus;
            btnCancel.Click += ChangeStatus;
            btnMiss.Click += ChangeStatus;
            Controls.Add(btnDone);
            Controls.Add(btnCancel);
            Controls.Add(btnMiss);

            LoadSchedule();
        }

        private void LoadSchedule()
        {
            lstSchedule.Items.Clear();
            using (var conn = Database.OpenConnection())
            using (var cmd = new SqlCommand(
                @"SELECT a.Id, a.AppointmentTime, a.Status, p.LastName + ' ' + p.FirstName
                  FROM Appointments a JOIN Patients p ON a.PatientId = p.Id
                  WHERE a.DoctorId = @did AND a.AppointmentDate = @date
                  ORDER BY a.AppointmentTime", conn))
            {
                cmd.Parameters.AddWithValue("@did", doctorId);
                cmd.Parameters.AddWithValue("@date", dtpDate.Value.ToString("yyyy-MM-dd"));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstSchedule.Items.Add(new AppItem
                        {
                            Id = reader.GetInt32(0),
                            Text = $"[{reader.GetString(2)}] {reader.GetString(1)} — {reader.GetString(3)}"
                        });
                    }
                }
            }
        }

        private void ChangeStatus(object sender, EventArgs e)
        {
            if (lstSchedule.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }
            string newStatus = ((Button)sender).Tag.ToString();
            var app = (AppItem)lstSchedule.SelectedItem;
            using (var conn = Database.OpenConnection())
            using (var cmd = new SqlCommand("UPDATE Appointments SET Status = @st WHERE Id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@st", newStatus);
                cmd.Parameters.AddWithValue("@id", app.Id);
                cmd.ExecuteNonQuery();
            }
            LoadSchedule();
        }

        class AppItem
        {
            public int Id;
            public string Text;
            public override string ToString() => Text;
        }
    }
}