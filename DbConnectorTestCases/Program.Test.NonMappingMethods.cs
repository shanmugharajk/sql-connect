using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DbConnect;
using DbType = DbConnect.DbType;

namespace DbConnectorTestCases
{
    public static partial class Program
    {

        #region DataTable, DataSet Test

        static void DataTableTest()
        {
            using (var con = new SqlConnection(ConString))
            {
                var dataTable = con.FillDataTable("select * from Department");
                var count = dataTable.Rows.Count;
                Console.WriteLine("\nExpected - {0}, Got - {1}", 5, count);
                Console.WriteLine("Expected - {0}, Got - {1}", "Closed", con.State);
            }
        }

        static void DataSetTest()
        {
            using (var con = new SqlConnection(ConString))
            {
                var dataSet = con.FillDataSet("select * from Department; select * from Employee");
                var count = dataSet.Tables.Count;

                var departmentCount = dataSet.Tables[0].Rows.Count;
                var employeeCount = dataSet.Tables[1].Rows.Count;

                Console.WriteLine("\nExpected - {0}, Got - {1}", 5, departmentCount);
                Console.WriteLine("Expected - {0}, Got - {1}", 4, employeeCount);
                Console.WriteLine("Expected - {0}, Got - {1}", 2, count);
                Console.WriteLine("Expected - {0}, Got - {1}", "Closed", con.State);
            }
        }

        #endregion

        #region Table Valued Parms

        private static void TableValuedParms()
        {
            //HERE DATATABLE NAME SHOULD BE THE TABLEVALUE TYPE NAME OTHERWISE WILL GET AN ERROR TYPENAME IS FOUND
            var dt = new DataTable { TableName = "DepartmentType" };
            dt.Columns.Add("DepartmentId", typeof(int));
            dt.Columns.Add("Name", typeof(string));

            var arrayName = new[] { "HR", "Dev", "DevOps", "Admin" };

            for (var i = 0; i <= 3; i++)
            {
                var row = dt.NewRow();
                row["DepartmentId"] = i + 2;
                row["Name"] = arrayName[i];
                dt.Rows.Add(row);
            }

            using (var con = new SqlConnection(ConString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@DepartmentDetails", dt, DbType.Structured, ParameterDirection.Input);
                parameters.Add("@ReturnValue", 0, direction: ParameterDirection.ReturnValue);
                con.Execute("spBulkInsert", parameters, CommandType.StoredProcedure);

                Console.WriteLine("\nExpected - {0}, Got - {1}", 4, parameters.Get<int>("@ReturnValue"));
            }
        }

        #endregion

        #region Parametrized Queries

        static void ParametrizedQueries()
        {
            using (var con = new SqlConnection(ConString))
            {
                //Insert statement with parameters
                var sql = "Insert into department (departmentid, name) values (@depId, @name)";

                var parameters = new DynamicParameters();
                parameters.Add("@depId", 1);
                parameters.Add("@name", "HR");
                parameters.Add("@name", "HR");
                var rowsAffected = con.Execute(sql, parameters);
                Console.WriteLine("\nExpected - {0}, Got - {1}\n", 1, rowsAffected);

                for (var i = 1; i < 5; i++)
                {
                    sql = "Insert into Employee (id, name, department, salary ) values (@id, @name, @depId, @salary)";
                    parameters = new DynamicParameters();
                    parameters.Add("@id", i);
                    switch (i)
                    {
                        case 1:
                            parameters.Add("@name", "sfk shan");
                            break;
                        case 2:
                            parameters.Add("@name", "Hari");
                            break;
                        case 3:
                            parameters.Add("@name", "Jothi");
                            break;
                        case 4:
                        case 5:
                            parameters.Add("@name", "Priya");
                            break;
                    }

                    parameters.Add("@depId", 1);
                    parameters.Add("@salary", 5000 + i * 100);
                    rowsAffected = con.Execute(sql, parameters);
                    Console.WriteLine("Expected - {0}, Got - {1}", 1, rowsAffected);
                }

                var ctr = 0;
                sql = @"select * from Employee where Name = @name";
                parameters = new DynamicParameters();
                parameters.Add("@name", "sfk shan");

                //Select statement with params
                //No need to specify if commandType: CommandType.Text coz its the default
                con.ExecuteReader(sql, reader =>
                {
                    while (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            //For test purpose only one row is taken for ctr is used
                            //In actual implementaion it wont be there
                            //For money the specified cast is decimal
                            if (ctr == 0)
                                Console.WriteLine("\nExpected - {0}, got {1}", 5100, reader.Get<int>("Salary"));
                            ctr++;
                        }
                        reader.NextResult();
                    }
                    ctr = 0;
                }, parameters, commandType: CommandType.Text);

                //Update statement with parameters
                sql = "update Employee set salary = @salary where id >= 1 ";
                parameters = new DynamicParameters();
                parameters.Add("@salary", 5000);
                var rowsAffected2 = con.Execute(sql, parameters);
                Console.WriteLine("\nExpected - {0}, Got - {1}", 4, rowsAffected2);
            }
        }

        #endregion

        #region Simple query execution

        /// <summary>
        /// Tests the Simple query retrievals
        /// </summary>
        static void QueryResultsetTest()
        {
            using (var con = new SqlConnection(ConString))
            {
                //Simple Select Query
                //Imp 1
                Action<DbDataReader> readDataFunc = reader =>
                {
                    while (reader.Read())
                    {
                        Console.WriteLine("\nExpected - {0} , Got - {1}", "HR", reader.Get<string>("name"));
                        break;
                    }
                };
                con.ExecuteReader("Select * from Department", readDataFunc);
                //Imp 2
                con.ExecuteReader("Select * from Department", ReadData);
                //Break the reader in between to check whether the con is closed or not
                var state = con.State;
                Console.WriteLine("\nExpected - {0}, Got - {1}", "Closed", state);

                //Multiple Resultset
                const string sql = @"Select * from Employee
                                     Select * from Department";
                con.ExecuteReader(sql, ReadDataMulti);
            }
        }

        static void ReadData(DbDataReader reader)
        {
            while (reader.Read())
            {
                Console.WriteLine("\nExpected - {0} , Got - {1}", "HR", reader.Get<string>("name"));
                break;
            }
        }

        static void ReadDataMulti(DbDataReader reader)
        {
            var ctr = 1;
            while (reader.HasRows)
            {
                while (reader.Read())
                {
                    Console.WriteLine(ctr == 1
                        ? string.Format("\nExpected - {0} , Got - {1}", "sfk shan", reader.Get<string>("Name")) //First result set - Employee
                        : string.Format("\nExpected - {0} , Got - {1}", "HR", reader.Get<string>("name")));//Second Result set - Department

                    break;
                }
                ctr++;
                reader.NextResult();
            }
            //Note : for test purpose it is skipped to iterate the rest rows
        }

        #endregion

        #region Initialization

        static void SetDefaults()
        {
            using (var con = new SqlConnection(ConString))
            {
                const string sql = @"Delete from Employee;
                            delete from Department";

                con.Execute(sql);
            }
        }

        #endregion
        
    }
}
