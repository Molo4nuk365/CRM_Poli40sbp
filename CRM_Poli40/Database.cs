using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace CRM_Poli40
{
    public static class Database
    {
        //Строчка подклюсения MSSQLLocalDB (SSMS)
        private static readonly string masterConn = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=True;";
        private static readonly string dbConn = "Server=(localdb)\\MSSQLLocalDB;Database=Polyclinic40;Integrated Security=True;";

        public static SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(dbConn);
            conn.Open();
            return conn;
        }
         
        public static void Initialize()
        {
            // Создаём базу, если её нет
            using (var conn = new SqlConnection(masterConn))
            {
                conn.Open();
                new SqlCommand("IF DB_ID('Polyclinic40') IS NULL CREATE DATABASE Polyclinic40", conn).ExecuteNonQuery();
            }

            // Проверяем наличие таблиц
            bool tablesExist = false;
            using (var conn = OpenConnection())
            {
                tablesExist = (int)new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name = 'Admins'", conn).ExecuteScalar() > 0;
            }

            // Если таблиц нет – выполняем SQL-скрипт
            if (!tablesExist)
            {
                string sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.sql");
                if (!File.Exists(sqlFile))
                    throw new FileNotFoundException("Файл Database.sql не найден в папке с программой", sqlFile);

                using (var conn = OpenConnection())
                {
                    new SqlCommand(File.ReadAllText(sqlFile), conn).ExecuteNonQuery();
                }
            }

            //  ВСЕГДА хешируем пароли, если есть открытые
            using (var conn = OpenConnection())
            {
                HashAllPasswords(conn);
            }
        }

        private static void HashAllPasswords(SqlConnection conn)
        {
            HashTable(conn, "Admins", "Id");
            HashTable(conn, "Doctors", "Id");
            HashTable(conn, "Patients", "Id");
        }

        private static void HashTable(SqlConnection conn, string table, string idCol)
        {
            var toHash = new List<(int id, string plain)>();
            using (var cmd = new SqlCommand($"SELECT {idCol}, PasswordHash FROM {table} WHERE PasswordHash NOT LIKE '$2a$%'", conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    toHash.Add((r.GetInt32(0), r.GetString(1)));
            }
            foreach (var (id, plain) in toHash)
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(plain);
                using (var ucmd = new SqlCommand($"UPDATE {table} SET PasswordHash = @h WHERE {idCol} = @id", conn))
                {
                    ucmd.Parameters.AddWithValue("@h", hash);
                    ucmd.Parameters.AddWithValue("@id", id);
                    ucmd.ExecuteNonQuery();
                }
            }
        }

        //  Методы аутентификации 
        public static (int id, string fullName) LoginAdmin(string login, string password)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("SELECT Id, FullName, PasswordHash FROM Admins WHERE Login = @l", conn))
            {
                cmd.Parameters.AddWithValue("@l", login);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        try
                        {
                            if (BCrypt.Net.BCrypt.Verify(password, r.GetString(2)))
                                return (r.GetInt32(0), r.GetString(1));
                        }
                        catch 
                        {
                        
                        }
                    }
                }
            }
            return (-1, null);
        }

        public static (int id, string ln, string fn) LoginDoctor(string login, string password)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("SELECT Id, LastName, FirstName, PasswordHash FROM Doctors WHERE Login = @l", conn))
            {
                cmd.Parameters.AddWithValue("@l", login);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        try
                        {
                            if (BCrypt.Net.BCrypt.Verify(password, r.GetString(3)))
                                return (r.GetInt32(0), r.GetString(1), r.GetString(2));
                        }
                        catch 
                        { 
                        
                        }
                    }
                }
            }
            return (-1, null, null);
        }

        public static (int id, string ln, string fn) LoginPatient(string oms, string password)
        {
            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand("SELECT Id, LastName, FirstName, PasswordHash FROM Patients WHERE OMS = @o", conn))
            {
                cmd.Parameters.AddWithValue("@o", oms);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        try
                        {
                            if (BCrypt.Net.BCrypt.Verify(password, r.GetString(3)))
                                return (r.GetInt32(0), r.GetString(1), r.GetString(2));
                        }
                        catch { }
                    }
                }
            }
            return (-1, null, null);
        }
    }
}