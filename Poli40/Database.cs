using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace Poli40
{
    public static class Database
    {
        private static readonly string ConnectionString = "Data Source=Poli40.db;Version=3;";

        public static SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static void Initialize()
        {
            // ПРИНУДИТЕЛЬНО удаляем старую базу, если она есть
            if (File.Exists("Poli40.db"))
            {
                try
                {
                    File.Delete("Poli40.db");
                }
                catch
                {
                    MessageBox.Show("Закройте все окна программы и перезапустите её",
                        "Файл базы данных занят", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Создаём новую базу
            SQLiteConnection.CreateFile("Poli40.db");
            using (var conn = GetConnection())
            {
                CreateAll(conn);
            }
        }
        // Запуск SQLite
        private static void CreateAll(SQLiteConnection conn)
        {
            string sql = @"
                CREATE TABLE Admins (
                    Id INTEGER PRIMARY KEY,
                    Login TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL,
                    FullName TEXT NOT NULL
                );

                CREATE TABLE Doctors (
                    Id INTEGER PRIMARY KEY,
                    LastName TEXT NOT NULL,
                    FirstName TEXT NOT NULL,
                    MiddleName TEXT,
                    Specialization TEXT NOT NULL,
                    Office TEXT,
                    Login TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL
                );

                CREATE TABLE Patients (
                    Id INTEGER PRIMARY KEY,
                    LastName TEXT NOT NULL,
                    FirstName TEXT NOT NULL,
                    MiddleName TEXT,
                    BirthDate TEXT NOT NULL,
                    OMS TEXT UNIQUE NOT NULL,
                    Phone TEXT,
                    Email TEXT,
                    Address TEXT,
                    Password TEXT NOT NULL
                );

                CREATE TABLE Appointments (
                    Id INTEGER PRIMARY KEY,
                    PatientId INTEGER NOT NULL,
                    DoctorId INTEGER NOT NULL,
                    AppointmentDate TEXT NOT NULL,
                    AppointmentTime TEXT NOT NULL,
                    Status TEXT DEFAULT 'Записан',
                    CreatedAt TEXT DEFAULT (datetime('now','localtime')),
                    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
                    FOREIGN KEY (DoctorId) REFERENCES Doctors(Id)
                );

                INSERT INTO Admins VALUES (1, 'admin', 'admin123', 'Администратор');

                INSERT INTO Doctors VALUES 
                (1, 'Иванов', 'Петр', 'Сергеевич', 'Терапевт', '101', 'ivanov', 'pass123'),
                (2, 'Смирнова', 'Анна', 'Игоревна', 'Кардиолог', '205', 'smirnova', 'pass123'),
                (3, 'Козлов', 'Дмитрий', 'Алексеевич', 'Хирург', '310', 'kozlov', 'pass123');

                INSERT INTO Patients VALUES 
                (1, 'Петров', 'Сергей', 'Иванович', '1990-05-15', '1234567890123456', '+79161234567', 'petrov@mail.ru', 'Москва', 'pass123'),
                (2, 'Сидорова', 'Мария', 'Петровна', '1985-10-20', '6543210987654321', '+79261234567', 'sidorova@mail.ru', 'Москва', 'pass123');

                INSERT INTO Appointments VALUES 
                (1, 1, 1, '2026-06-30', '10:00', 'Записан', datetime('now','localtime')),
                (2, 2, 2, '2026-06-30', '11:00', 'Записан', datetime('now','localtime'));
            ";
            using (var cmd = new SQLiteCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        public static (int id, string name)? LoginAdmin(string login, string password)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand("SELECT Id, FullName FROM Admins WHERE Login=@l AND Password=@p", conn))
            {
                cmd.Parameters.AddWithValue("@l", login);
                cmd.Parameters.AddWithValue("@p", password);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) return (r.GetInt32(0), r.GetString(1));
            }
            return null;
        }

        public static (int id, string lastName, string firstName)? LoginDoctor(string login, string password)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand("SELECT Id, LastName, FirstName FROM Doctors WHERE Login=@l AND Password=@p", conn))
            {
                cmd.Parameters.AddWithValue("@l", login);
                cmd.Parameters.AddWithValue("@p", password);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) return (r.GetInt32(0), r.GetString(1), r.GetString(2));
            }
            return null;
        }

        public static (int id, string lastName, string firstName)? LoginPatient(string oms, string password)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand("SELECT Id, LastName, FirstName FROM Patients WHERE OMS=@o AND Password=@p", conn))
            {
                cmd.Parameters.AddWithValue("@o", oms);
                cmd.Parameters.AddWithValue("@p", password);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) return (r.GetInt32(0), r.GetString(1), r.GetString(2));
            }
            return null;
        }
    }
}