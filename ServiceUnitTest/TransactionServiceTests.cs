using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using System.Data.SqlClient;

namespace Service.UnitTest
{
    [TestClass]
    public sealed class TransactionServiceTests
    {
        private string sourceConnectionString = @"Server=SERVER01;Database=TransactionLAB;User Id=sa;Password=Pa$$w0rd;";
        private string targetConnectionString = @"Server=SERVER02;Database=TransactionLAB;User Id=sa;Password=Pa$$w0rd;";

        private TransactionService service;

        private Student student01 = new Student() { Id = 1, Name = "Echo", };
        private Student student02 = new Student() { Id = 2, Name = "John", };
        private Student student03 = new Student() { Id = 3, Name = "David", };

        [TestInitialize]
        public void TestInitialize()
        {
            this.service = new TransactionService(this.sourceConnectionString, this.targetConnectionString);

            // Delete Source Students
            this.DeleteStudent(this.sourceConnectionString);
            // Delete Target Students
            this.DeleteStudent(this.targetConnectionString);

            // Create Source Students
            this.CreateStudent(this.sourceConnectionString, student01);
            this.CreateStudent(this.sourceConnectionString, student02);
            this.CreateStudent(this.sourceConnectionString, student03);
            // Create Target Students
            this.CreateStudent(this.targetConnectionString, student03);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Delete Source Students
            this.DeleteStudent(this.sourceConnectionString);
            // Delete Target Students
            this.DeleteStudent(this.targetConnectionString);
        }

        private void CreateStudent(string connectionString, Student student)
        {
            using (var sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();

                var cmd = new SqlCommand(@"INSERT INTO [Student]([Id], [Name]) VALUES (" + student.Id + ", '" + student.Name + "')", sqlConn);

                cmd.ExecuteNonQuery();
            }
        }

        private void DeleteStudent(string connectionString)
        {
            using (var sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();

                var cmd = new SqlCommand(@"DELETE FROM [Student]", sqlConn);

                cmd.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void MoveStudentTest_傳入student01的Id_預期成功_預期來源資料庫無student01_預期目標資料庫有student01()
        {
            var expected = true;

            var expectedSource = 0;
            var expectedTarget = 1;

            var actual = this.service.MoveStudent(this.student01.Id);

            var actualSource = this.SearchStudent(this.sourceConnectionString, this.student01);
            var actualTarget = this.SearchStudent(this.targetConnectionString, this.student01);

            Assert.AreEqual(expected, actual);

            Assert.AreEqual(expectedSource, actualSource);
            Assert.AreEqual(expectedTarget, actualTarget);
        }

        [TestMethod]
        public void MoveStudentTest_傳入student03的Id_預期失敗_預期來源資料庫有student03_預期目標資料庫有student03()
        {
            var expected = false;

            var expectedSource = 1;
            var expectedTarget = 1;

            var actual = this.service.MoveStudent(this.student03.Id);

            var actualSource = this.SearchStudent(this.sourceConnectionString, this.student03);
            var actualTarget = this.SearchStudent(this.targetConnectionString, this.student03);

            Assert.AreEqual(expected, actual);

            Assert.AreEqual(expectedSource, actualSource);
            Assert.AreEqual(expectedTarget, actualTarget);
        }

        private int SearchStudent(string connectionString, Student student)
        {
            using (var sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();

                var cmd = new SqlCommand(@"SELECT COUNT([Id]) FROM [Student] WHERE [Id] = " + student.Id + " AND [Name] = '" + student.Name + "'", sqlConn);

                return (int)cmd.ExecuteScalar();
            }
        }
    }
}