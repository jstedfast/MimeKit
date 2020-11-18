//
// SQLServerCertificateDatabase.cs
//
// Author: Rob Blackin <rob@5-9z.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace MimeKit.Cryptography
{
    public class SQLServerCertificateDatabase : SqlCertificateDatabase
    {
        public SQLServerCertificateDatabase(DbConnection connection, string password) : base(connection, password)
        {
        }

        protected override void AddTableColumn(DbConnection connection, DataTable table, DataColumn column)
        {
            var statement = new StringBuilder("ALTER TABLE ");
            int primaryKeys = table.PrimaryKey?.Length ?? 0;

            statement.Append(table.TableName);
            statement.Append(" ADD COLUMN ");
            Build(statement, table, column, ref primaryKeys);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = statement.ToString();
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        protected override void CreateTable(DbConnection connection, DataTable table)
        {
            var statement = new StringBuilder($"if not exists (select * from sysobjects where name='{table.TableName}' and xtype='U') ");
            int primaryKeys = 0;

            statement.Append($"Create table {table.TableName} (");

            foreach (DataColumn column in table.Columns)
            {
                Build(statement, table, column, ref primaryKeys);
                statement.Append(", ");
            }

            if (table.Columns.Count > 0)
                statement.Length -= 2;

            statement.Append(')');

            using (var command = connection.CreateCommand())
            {
                command.CommandText = statement.ToString();
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }
        static void Build(StringBuilder statement, DataTable table, DataColumn column, ref int primaryKeys)
        {
            statement.Append(column.ColumnName);
            statement.Append(' ');

            if (column.DataType == typeof(long) || column.DataType == typeof(int))
            {
                if (column.AutoIncrement)
                    statement.Append("int identity(1,1)");
                else
                    statement.Append("int");
            }
            else if (column.DataType == typeof(bool))
            {
                statement.Append("bit");
            }
            else if (column.DataType == typeof(byte[]))
            {
                statement.Append($"varbinary(4096)");
            }
            else if (column.DataType == typeof(string))
            {
                statement.Append("varchar(256)");
            }
            else
            {
                throw new NotImplementedException();
            }

            bool isPrimaryKey = false;
            if (table != null && table.PrimaryKey != null && primaryKeys < table.PrimaryKey.Length)
            {
                for (int i = 0; i < table.PrimaryKey.Length; i++)
                {
                    if (column == table.PrimaryKey[i])
                    {
                        statement.Append(" PRIMARY KEY Clustered");
                        isPrimaryKey = true;
                        primaryKeys++;
                        break;
                    }
                }
            }

            if (column.Unique && !isPrimaryKey)
                statement.Append(" UNIQUE");

            if (!column.AllowDBNull)
                statement.Append(" NOT NULL");
        }
        protected override IList<DataColumn> GetTableColumns(DbConnection connection, string tableName)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"select top 1 * from {tableName}";
                using (var reader = command.ExecuteReader())
                {
                    var columns = new List<DataColumn>();
                    var table = reader.GetSchemaTable();
                    foreach (DataRow row in table.Rows)
                    {
                        columns.Add(new DataColumn { ColumnName = row.Field<string>("ColumnName") });
                    }

                    return columns;
                }
            }
        }

        protected override void CreateIndex(DbConnection connection, string tableName, string[] columnNames)
        {
            var indexName = GetIndexName(tableName, columnNames);
            var query = string.Format("IF NOT EXISTS (Select 8 from sys.indexes where name='{0}' and object_id=OBJECT_ID('{1}')) CREATE INDEX {0} ON {1}({2})", indexName, tableName, string.Join(", ", columnNames));

            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        protected override void RemoveIndex(DbConnection connection, string tableName, string[] columnNames)
        {
            var indexName = GetIndexName(tableName, columnNames);
            var query = string.Format("IF EXISTS (Select 8 from sys.indexes where name='{0}' and object_id=OBJECT_ID('{1}')) DROP INDEX {0} ON {1}", indexName, tableName);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }
    }
}
