/*
 * Author  : sfk 
 * Licence : Public
*/

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;


namespace DbConnect
{
    /// <summary>
    /// Class that extends IDbConnection and provides facility to make operation with the datatabases supported by ADO.Net
    /// </summary>
    public static partial class DbConnect
    {
        /// <summary>
        /// Executes Parametrized/Non-Parametrized sql statements, Stored procedure which dont have return result sets
        /// Suited for Insert/Update/Delete/ Stored Procedures without results with In/Out params
        /// </summary>
        /// <param name="con">Connection object</param>
        /// <param name="sql">sql staement</param>
        /// <param name="parameters">parameters</param>
        /// <param name="commandType">Command Type</param>
        /// <returns></returns>
        public static int Execute(this IDbConnection con, string sql, DynamicParameters parameters = null, CommandType? commandType = null)
        {
            int rowsAffected;
            IDbCommand cmd = null;

            var wasClosed = con.State == ConnectionState.Closed;

            try
            {

                if (wasClosed) con.Open();

                cmd = CommandDefnition(con, sql, parameters, commandType);
                rowsAffected = cmd.ExecuteNonQuery();

                if (parameters != null)
                    ((IDynamicParameters)parameters).CallBack(cmd);
            }
            finally
            {
                if (wasClosed) con.Close();
                if (cmd != null) cmd.Dispose();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Reads the data and returns in IDataReader object
        /// </summary>
        /// <param name="con">Connection object</param>
        /// <param name="sql">sql staement</param>
        /// <param name="readDataFunc">Action delegate to perform read operation for the reader</param>
        /// <param name="parameters">parameters</param>
        /// <param name="commandType">Command Type</param>
        public static void ExecuteReader(this IDbConnection con, string sql, Action<DbDataReader> readDataFunc, DynamicParameters parameters = null, CommandType? commandType = null)
        {
            IDbCommand cmd = null;
            var wasClosed = con.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) con.Open();

                cmd = CommandDefnition(con, sql, parameters, commandType);
                readDataFunc((DbDataReader)cmd.ExecuteReader());

                if (parameters != null)
                    ((IDynamicParameters)parameters).CallBack(cmd);
            }
            finally
            {
                if (wasClosed) con.Close();
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Fills Result in DataTable and returns it
        /// Suited for only query with single resultset
        /// </summary>
        /// <param name="con">Connection object</param>
        /// <param name="sql">sql staement</param>
        /// <param name="parameters">parameters</param>
        /// <param name="commandType">Command Type</param>
        /// <returns>DataTable</returns>
        public static DataTable FillDataTable(this IDbConnection con, string sql, DynamicParameters parameters = null, CommandType? commandType = null)
        {
            IDbCommand cmd = null;
            var table = new DataTable();
            var wasClosed = con.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) con.Open();

                cmd = CommandDefnition(con, sql, parameters, commandType);
                table.Load(cmd.ExecuteReader());

                if (parameters != null)
                    ((IDynamicParameters)parameters).CallBack(cmd);

                return table;
            }
            finally
            {
                if (wasClosed) con.Close();
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Fills DataSet for the given Query
        /// </summary>
        /// <param name="con">Connection object</param>
        /// <param name="sql">sql staement</param>
        /// <param name="parameters">parameters</param>
        /// <param name="commandType">Command Type</param>
        /// <returns></returns>
        public static DataSet FillDataSet(this IDbConnection con, string sql, DynamicParameters parameters = null, CommandType? commandType = null)
        {
            IDbCommand cmd = null;
            var dataSet = new DataSet();
            var wasClosed = con.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) con.Open();

                var adapter = GetAdapter(con);
                cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                adapter.SelectCommand = cmd;
                adapter.Fill(dataSet);

                if (parameters != null)
                    ((IDynamicParameters)parameters).CallBack(cmd);
            }
            finally
            {
                if (wasClosed) con.Close();
                if (cmd != null) cmd.Dispose();
            }
            return dataSet;
        }

        /// <summary>
        /// Executes Query and returns scalar value results
        /// </summary>
        /// <typeparam name="T">Type to cast</typeparam>
        /// <param name="con">Connectoion object</param>
        /// <param name="sql">Sql query</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="commandType">CommandType</param>
        /// <returns>Scalar Result Of type T</returns>
        public static T ExecuteScalar<T>(this IDbConnection con, string sql, DynamicParameters parameters = null, CommandType? commandType = null)
        {
            IDbCommand cmd = null;
            object result;
            var wasClosed = con.State == ConnectionState.Closed;
            try
            {
                if (wasClosed) con.Open();

                cmd = CommandDefnition(con, sql, parameters, commandType);
                result = cmd.ExecuteScalar();

                if (parameters != null)
                    ((IDynamicParameters)parameters).CallBack(cmd);
            }
            finally
            {
                if (wasClosed) con.Close();
                if (cmd != null) cmd.Dispose();
            }
            return (T)(result);
        }

        /// <summary>
        /// Creates a IDbCommand object for the connetcion with all necessary parameters to execute the query
        /// </summary>
        private static IDbCommand CommandDefnition(IDbConnection con, string sql, DynamicParameters parameters = null, CommandType? commandType = null)
        {
            var cmd = con.CreateCommand();
            var cmdType = commandType ?? CommandType.Text;
            cmd.CommandType = cmdType;
            cmd.CommandText = sql;
            if (parameters != null)
            {
                ((IDynamicParameters)parameters).AddParameters(cmd);
            }
            return cmd;
        }

        /// Reference :- http://stackoverflow.com/questions/10723558/instantiate-idataadapter-from-instance-of-idbconnection
        /// <summary>
        /// Gets the instance of IDbDataAdapter from the connection
        /// </summary>
        private static IDbDataAdapter GetAdapter(IDbConnection con)
        {
            var assembly = con.GetType().Assembly;
            var @namespace = con.GetType().Namespace;

            // Assumes the factory is in the same namespace
            var factoryType = assembly.GetTypes().Where(x => x.Namespace == @namespace).Single(x => x.IsSubclassOf(typeof(DbProviderFactory)));

            // SqlClientFactory and OleDbFactory both have an Instance field.
            var instanceFieldInfo = factoryType.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

            if (instanceFieldInfo == null) throw new Exception("Couldn't find the data provider.. Specify the correct one in the connection string");

            var factory = (DbProviderFactory)instanceFieldInfo.GetValue(null);
            return factory.CreateDataAdapter();
        }
    }
}
