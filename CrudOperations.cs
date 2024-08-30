
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SQLiteCrudLibrary
{
    public class CrudOperations<T> where T : new()
    {
        private SQLiteDbHelper _dbHelper;
        private string _tableName;
        private PropertyInfo[] _properties;

        public CrudOperations(string connectionString)
        {
            _dbHelper = new SQLiteDbHelper(connectionString);
            _tableName = typeof(T).Name;
            _properties = typeof(T).GetProperties();
        }

        public void CreateTable()
        {
            var columns = _properties
                .Select(p => $"{p.Name} {GetSqlType(p.PropertyType)}")
                .ToArray();
            string query = $"CREATE TABLE IF NOT EXISTS {_tableName} ({string.Join(", ", columns)});";

            _dbHelper.ExecuteNonQuery(query);
        }

        public void Insert(T entity)
        {
            var columnNames = _properties.Select(p => p.Name).ToArray();
            var columnValues = _properties.Select(p => $"@{p.Name}").ToArray();

            string query = $"INSERT INTO {_tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", columnValues)})";

            var parameters = GetParameters(entity);
            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        public List<T> ReadAll()
        {
            string query = $"SELECT * FROM {_tableName}";
            var dataTable = _dbHelper.ExecuteQuery(query);

            return ConvertToList(dataTable);
        }

        public void Update(T entity, object id)
        {
            var setClauses = _properties.Select(p => $"{p.Name} = @{p.Name}").ToArray();
            string query = $"UPDATE {_tableName} SET {string.Join(", ", setClauses)} WHERE Id = @Id";

            var parameters = GetParameters(entity);
            parameters.Add("@Id", id);
            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        public void Delete(object id)
        {
            string query = $"DELETE FROM {_tableName} WHERE Id = @Id";
            var parameters = new Dictionary<string, object>
            {
                { "@Id", id }
            };

            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        private Dictionary<string, object> GetParameters(T entity)
        {
            return _properties.ToDictionary(p => $"@{p.Name}", p => p.GetValue(entity, null) ?? DBNull.Value);
        }

        private List<T> ConvertToList(DataTable dataTable)
        {
            var list = new List<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                var obj = new T();

                foreach (var prop in _properties)
                {
                    if (dataTable.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                    }
                }

                list.Add(obj);
            }

            return list;
        }

        private string GetSqlType(Type type)
        {
            if (type == typeof(int) || type == typeof(long))
                return "INTEGER";
            if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
                return "REAL";
            if (type == typeof(bool))
                return "BOOLEAN";
            if (type == typeof(string))
                return "TEXT";
            if (type == typeof(DateTime))
                return "DATETIME";
            if (type.IsEnum)
                return "INTEGER";

            throw new NotSupportedException($"Type {type.Name} is not supported.");
        }
    }
}

