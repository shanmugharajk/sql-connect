
using System;
using System.Data;

namespace DbConnect
{
    public static partial class DbConnect
    {
        public static T Get<T>(this IDataRecord record, string name)
        {
            var index = record.GetOrdinal(name);
            var val = record[index];

            return Parse<T>(val);
        }

        private static T Parse<T>(object val)
        {
            try
            {
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                if (val == DBNull.Value)
                {
                    if (default(T) != null)
                    {
                        throw new ApplicationException("Attempting to cast a DBNull to a non nullable type!");
                    }
                    return default(T);
                }
                throw new ApplicationException("Invalid Cast !");
            }
        }
    }
}
