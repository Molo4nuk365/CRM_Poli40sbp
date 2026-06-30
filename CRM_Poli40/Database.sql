IF DB_ID('Polyclinic40') IS NULL
    CREATE DATABASE Polyclinic40;
GO

USE Polyclinic40;
GO

CREATE TABLE Admins (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Login NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL
);

CREATE TABLE Doctors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    LastName NVARCHAR(50) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    MiddleName NVARCHAR(50),
    Specialization NVARCHAR(100) NOT NULL,
    Office NVARCHAR(10),
    Login NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Token NVARCHAR(100) UNIQUE NOT NULL,
    TokenExpiryDate NVARCHAR(10) NOT NULL
);

CREATE TABLE Patients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    LastName NVARCHAR(50) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    MiddleName NVARCHAR(50),
    BirthDate NVARCHAR(10) NOT NULL,
    OMS NVARCHAR(16) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(200),
    PasswordHash NVARCHAR(255) NOT NULL
);

CREATE TABLE Appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentDate NVARCHAR(10) NOT NULL,
    AppointmentTime NVARCHAR(5) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Записан',
    CreatedAt NVARCHAR(20) DEFAULT CONVERT(VARCHAR, GETDATE(), 120),
    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    FOREIGN KEY (DoctorId) REFERENCES Doctors(Id)
);
GO

-- Начальные данные (с явным указанием Id через IDENTITY_INSERT)
SET IDENTITY_INSERT Admins ON;
INSERT INTO Admins (Id, Login, PasswordHash, FullName) 
VALUES (1, 'admin', 'admin123', N'Администратор');
SET IDENTITY_INSERT Admins OFF;

SET IDENTITY_INSERT Doctors ON;
INSERT INTO Doctors (Id, LastName, FirstName, MiddleName, Specialization, Office, Login, PasswordHash, Token, TokenExpiryDate) VALUES
(1, N'Иванов', N'Петр', N'Сергеевич', N'Терапевт', '101', 'ivanov', 'pass123', 'token-ivanov-2026', '2026-12-31'),
(2, N'Смирнова', N'Анна', N'Игоревна', N'Кардиолог', '205', 'smirnova', 'pass123', 'token-smirnova-2026', '2026-12-31'),
(3, N'Козлов', N'Дмитрий', N'Алексеевич', N'Хирург', '310', 'kozlov', 'pass123', 'token-kozlov-2026', '2026-12-31');
SET IDENTITY_INSERT Doctors OFF;

SET IDENTITY_INSERT Patients ON;
INSERT INTO Patients (Id, LastName, FirstName, MiddleName, BirthDate, OMS, Phone, Email, Address, PasswordHash) VALUES
(1, N'Петров', N'Сергей', N'Иванович', '1990-05-15', '1234567890123456', '+79161234567', 'petrov@mail.ru', N'Москва', 'pass123'),
(2, N'Сидорова', N'Мария', N'Петровна', '1985-10-20', '6543210987654321', '+79261234567', 'sidorova@mail.ru', N'Москва', 'pass123');
SET IDENTITY_INSERT Patients OFF;

SET IDENTITY_INSERT Appointments ON;
INSERT INTO Appointments (Id, PatientId, DoctorId, AppointmentDate, AppointmentTime, Status, CreatedAt) VALUES
(1, 1, 1, '2026-06-30', '10:00', N'Записан', CONVERT(VARCHAR, GETDATE(), 120)),
(2, 2, 2, '2026-06-30', '11:00', N'Записан', CONVERT(VARCHAR, GETDATE(), 120));
SET IDENTITY_INSERT Appointments OFF;
GO

