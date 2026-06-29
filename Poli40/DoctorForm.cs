using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace Poli40
{
    public class DoctorForm : Form
    {
        private int doctorId;
        private DateTimePicker dtpDate;
        private ListBox lstSchedule;

        public DoctorForm(int id, string name)
        {
            doctorId = id;
            this.Text = "Врач: " + name;
            this.Size = new Size(530, 410);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            Label lblDate = new Label { Text = "Дата приёма:", Location = new Point(10, 12), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new Point(120, 9), Width = 130, Format = DateTimePickerFormat.Short };
            dtpDate.ValueChanged += (s, e) => LoadSchedule();

            Label lblSch = new Label { Text = "Расписание:", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(10, 45), AutoSize = true };
            lstSchedule = new ListBox { Location = new Point(10, 72), Width = 500, Height = 210 };

            Button btnComplete = new Button { Text = "Был на приёме", Location = new Point(10, 298), Width = 160, Height = 38, BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Tag = "Завершен" };
            btnComplete.Click += BtnStatus;
            Button btnCancel = new Button { Text = "Отменён", Location = new Point(180, 298), Width = 160, Height = 38, BackColor = Color.Gold, FlatStyle = FlatStyle.Flat, Tag = "Отменен" };
            btnCancel.Click += BtnStatus;
            Button btnNoShow = new Button { Text = "Не явился", Location = new Point(350, 298), Width = 160, Height = 38, BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Tag = "Не явился" };
            btnNoShow.Click += BtnStatus;

            this.Controls.AddRange(new Control[] { lblDate, dtpDate, lblSch, lstSchedule, btnComplete, btnCancel, btnNoShow });
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            lstSchedule.Items.Clear();
            using (var conn = Database.GetConnection())
            using (var cmd = new SQLiteCommand(@"SELECT a.Id, a.AppointmentTime, a.Status, p.LastName||' '||p.FirstName 
                FROM Appointments a JOIN Patients p ON a.PatientId=p.Id WHERE a.DoctorId=@did AND a.AppointmentDate=@d ORDER BY a.AppointmentTime", conn))
            {
                cmd.Parameters.AddWithValue("@did", doctorId);
                cmd.Parameters.AddWithValue("@d", dtpDate.Value.ToString("yyyy-MM-dd"));
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lstSchedule.Items.Add(new AppItem { Id = r.GetInt32(0), Text = $"[{r.GetString(2)}] {r.GetString(1)} — {r.GetString(3)}" });
            }
        }

        private void BtnStatus(object sender, EventArgs e)
        {
            if (lstSchedule.SelectedItem == null) { MessageBox.Show("Выберите запись"); return; }
            var app = (AppItem)lstSchedule.SelectedItem;
            string status = ((Button)sender).Tag.ToString();
            using (var conn = Database.GetConnection())
            using (var cmd = new SQLiteCommand("UPDATE Appointments SET Status=@s WHERE Id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@s", status);
                cmd.Parameters.AddWithValue("@id", app.Id);
                cmd.ExecuteNonQuery();
            }
            LoadSchedule();
        }

        class AppItem { public int Id; public string Text; public override string ToString() => Text; }
    }
}