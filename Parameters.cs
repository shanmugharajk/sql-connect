/*
 * Author  : sfk 
 * Licence : Public
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DbConnect
{
    /// <summary>
    /// Class that holds bag of parameters
    /// </summary>
    public class DynamicParameters : IDynamicParameters
    {
        private const int DefaultLength = 4000;

        /// <summary>
        /// Parameter Info Template
        /// </summary>
        public class ParameterInfo
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public ParameterDirection ParameterDirection { get; set; }
            public DbType? DbType { get; set; }
            public int? Size { get; set; }
        }

        /// <summary>
        /// Dictionary hold the parameter details
        /// </summary>
        private readonly Dictionary<string, ParameterInfo> parameters = new Dictionary<string, ParameterInfo>();

        /// <summary>
        /// Adds parameters necessary for the query execution
        /// </summary>
        /// <param name="name"> Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="dbType">DbType</param>
        /// <param name="direction">Parameter direction</param>
        /// <param name="size">Parameter Size</param>
        public void Add(string name, object value, DbType? dbType = null, ParameterDirection? direction = null, int? size = null)
        {
            parameters[Clean(name)] = new ParameterInfo()
            {
                Name = Clean(name),
                Value = value,
                ParameterDirection = direction ?? ParameterDirection.Input,
                DbType = dbType,
                Size = size
            };
        }

        /// <summary>
        /// Cleans the parameter name by removing these characters ['@', ':', '?'] 
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>name as cleaned</returns>
        static string Clean(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                switch (name[0])
                {
                    case '@':
                    case ':':
                    case '?':
                        return name.Substring(1);
                }
            }
            return name;
        }

        /// <summary>
        /// Interface implemetaion for AddParameters
        /// </summary>
        /// <param name="cmd"></param>
        void IDynamicParameters.AddParameters(IDbCommand cmd)
        {
            AddParameters(cmd);
        }

        /// <summary>
        /// Interface implementation for CallBack
        /// </summary>
        /// <param name="cmd"></param>
        void IDynamicParameters.CallBack(IDbCommand cmd)
        {
            Callback(cmd);
        }

        /// <summary>
        /// Adds Params to the cmd before its executed
        /// </summary>
        /// <param name="cmd"></param>
        private void AddParameters(IDbCommand cmd)
        {
            foreach (var paramToAdd in parameters.Values)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = paramToAdd.Name;
                p.Direction = paramToAdd.ParameterDirection;
                paramToAdd.Value = paramToAdd.Value ?? DBNull.Value;
                p.Value = paramToAdd.Value;

                var dbType = paramToAdd.DbType ?? DbConnect.LookDbType(paramToAdd.Value.GetType(), paramToAdd.Name, true);

                if (dbType == DbType.Structured)
                {
                    SetTablevaluedParam(p, paramToAdd);
                    cmd.Parameters.Add(p);
                    continue;
                }

                p.DbType = (System.Data.DbType)dbType;

                var s = paramToAdd.Value as string;
                if (s != null && s.Length <= DefaultLength)
                {
                    p.Size = DefaultLength;
                }

                if (paramToAdd.Size != null)
                {
                    p.Size = (int)paramToAdd.Size;
                }

                cmd.Parameters.Add(p);
            }
        }

        /// <summary>
        /// Sets the table valued parameter
        /// </summary>
        /// <param name="param">Parameter Object from the Command</param>
        /// <param name="info">Parameter info user supplied to add</param>
        private static void SetTablevaluedParam(IDbDataParameter param, ParameterInfo info)
        {
            var sqlParam = param as SqlParameter;

            if (sqlParam == null) throw new Exception("Failed to add the parameter !!");

            if (info.Value == null) throw new Exception("Table valued parameter cannot be null !!");

            var typeName = ((DataTable)info.Value).TableName;

            if (String.IsNullOrWhiteSpace(typeName)) throw new Exception("Expects TableValue Type Name as DataTable Name, didn't supplied in the param. Please provide that !!");

            sqlParam.TypeName = ((DataTable)info.Value).TableName;
            sqlParam.Value = (DataTable)info.Value;
            sqlParam.SqlDbType = SqlDbType.Structured;
        }

        /// <summary>
        /// Callback called after query execution to collect the out params
        /// </summary>
        /// <param name="cmd"></param>
        private void Callback(IDbCommand cmd)
        {
            foreach (IDbDataParameter param in cmd.Parameters)
            {
                parameters[param.ParameterName].Value = param.Value;
            }
        }

        /// <summary>
        /// Gets the Out parameter with type T
        /// </summary>
        /// <typeparam name="T">Type T in which we need data</typeparam>
        /// <param name="name">Parameter Name</param>
        /// <returns>Value as T</returns>
        public T Get<T>(string name)
        {
            var val = parameters[Clean(name)].Value;
            if (val == DBNull.Value)
            {
                if (default(T) != null)
                {
                    throw new ApplicationException("Attempting to cast a DBNull to a non nullable type!");
                }
                return default(T);
            }
            return (T)val;
        }
    }
}