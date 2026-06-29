
--БД ДАННЫХ ПОЛИКЛИНИКА №40
-- Создание таблиц
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

-- Тестовые данные
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