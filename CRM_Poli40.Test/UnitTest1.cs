using System;
using System.Data.Entity;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CRM_Poli40.Tests
{
    [TestClass]
    public class ShortTests
    {
        [AssemblyInitialize]
        public static void Init(TestContext ctx) => Database.Initialize();

        // Тест 1: Вход администратора (успех)
        [TestMethod]
        public void AdminLogin_OK()
        {
            var r = Database.LoginAdmin("admin", "admin123");
            Assert.IsTrue(r.id > 0);
        }

        // Тест 2: Вход администратора (неудача)
        [TestMethod]
        public void AdminLogin_Fail()
        {
            var r = Database.LoginAdmin("", "");
            Assert.AreEqual(0, r.id);
        }

        // Тест 3: Вход пациента по ОМС (успех)
        [TestMethod]
        public void PatientLogin_OK()
        {
            var r = Database.LoginPatient("1234567890123456", "pass123");
            Assert.IsTrue(r.id > 0);
        }
    }
}