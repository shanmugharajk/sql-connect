/*
 * Author  : sfk 
 * Licence : Public
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace DbConnect
{
    public static partial class DbConnect
    {
        /// <summary>
        /// Contains the DbType for the CLR Type
        /// </summary>
        static readonly Dictionary<Type, DbType> TypeMap;

        static DbConnect()
        {
            TypeMap = new Dictionary<Type, DbType>();
            TypeMap[typeof(byte)] = DbType.Byte;
            TypeMap[typeof(sbyte)] = DbType.SByte;
            TypeMap[typeof(short)] = DbType.Int16;
            TypeMap[typeof(ushort)] = DbType.UInt16;
            TypeMap[typeof(int)] = DbType.Int32;
            TypeMap[typeof(uint)] = DbType.UInt32;
            TypeMap[typeof(long)] = DbType.Int64;
            TypeMap[typeof(ulong)] = DbType.UInt64;
            TypeMap[typeof(float)] = DbType.Single;
            TypeMap[typeof(double)] = DbType.Double;
            TypeMap[typeof(decimal)] = DbType.Decimal;
            TypeMap[typeof(bool)] = DbType.Boolean;
            TypeMap[typeof(string)] = DbType.String;
            TypeMap[typeof(char)] = DbType.StringFixedLength;
            TypeMap[typeof(Guid)] = DbType.Guid;
            TypeMap[typeof(DateTime)] = DbType.DateTime;
            TypeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            TypeMap[typeof(TimeSpan)] = DbType.Time;
            TypeMap[typeof(byte[])] = DbType.Binary;
            TypeMap[typeof(byte?)] = DbType.Byte;
            TypeMap[typeof(sbyte?)] = DbType.SByte;
            TypeMap[typeof(short?)] = DbType.Int16;
            TypeMap[typeof(ushort?)] = DbType.UInt16;
            TypeMap[typeof(int?)] = DbType.Int32;
            TypeMap[typeof(uint?)] = DbType.UInt32;
            TypeMap[typeof(long?)] = DbType.Int64;
            TypeMap[typeof(ulong?)] = DbType.UInt64;
            TypeMap[typeof(float?)] = DbType.Single;
            TypeMap[typeof(double?)] = DbType.Double;
            TypeMap[typeof(decimal?)] = DbType.Decimal;
            TypeMap[typeof(bool?)] = DbType.Boolean;
            TypeMap[typeof(char?)] = DbType.StringFixedLength;
            TypeMap[typeof(Guid?)] = DbType.Guid;
            TypeMap[typeof(DateTime?)] = DbType.DateTime;
            TypeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            TypeMap[typeof(TimeSpan?)] = DbType.Time;
            TypeMap[typeof(object)] = DbType.Object;
            TypeMap[typeof (DataTable)] = DbType.Structured;
        }

        internal const string LinqBinary = "System.Data.Linq.Binary";

        /// <summary>
        /// Used to find the DbType from the param value type if not specified by the User
        /// </summary>
        /// <param name="type">CLR Type of param value</param>
        /// <param name="name">Param Name</param>
        /// <param name="demand">To ensure if it is present else throw error</param>
        /// <returns>DbType</returns>
        internal static DbType LookDbType(Type type, string name, bool demand)
        {
            DbType dbType;

            if (type.IsEnum && !TypeMap.ContainsKey(type))
            {
                type = Enum.GetUnderlyingType(type);
            }
            if (TypeMap.TryGetValue(type, out dbType))
            {
                return dbType;
            }
            if (type.FullName == LinqBinary)
            {
                return DbType.Binary;
            }

            if (demand)
                throw new NotSupportedException(string.Format("The member {0} of type {1} cannot be used as a parameter value", name, type.FullName));
            return DbType.Object;
        }
    }
}
