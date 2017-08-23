using Dapper;
using Model;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace Service
{
    public sealed class TransactionService
    {
        private readonly string sourceConnectionString;
        private readonly string targetConnectionString;

        public TransactionService(string sourceConnectionString, string targetConnectionString)
        {
            this.sourceConnectionString = sourceConnectionString;
            this.targetConnectionString = targetConnectionString;
        }

        public bool MoveStudent(int studentId)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    var student = new Student();

                    using (var sqlConn = new SqlConnection(this.sourceConnectionString))
                    {
                        var students = sqlConn.Query<Student>(@"SELECT [Id], [Name] FROM [Student] WHERE [Id] = @Id", new { Id = studentId });

                        if (students == null || students.Count() != 1)
                        {
                            return false;
                        }

                        student = students.Single();

                        sqlConn.Execute(@"DELETE FROM [Student] WHERE [Id] = @Id AND [Name] = @Name", student);
                    }

                    using (var sqlConn = new SqlConnection(this.targetConnectionString))
                    {
                        sqlConn.Execute(@"INSERT INTO [Student]([Id], [Name]) VALUES (@Id, @Name)", student);
                    }

                    // 若在 complete 前發生例外，包含在 scope 內的交易將會被 roll back
                    scope.Complete();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}