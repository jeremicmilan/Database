using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Database
{
    public class Table
    {
        [XmlElement]
        public string TableName;

        [XmlElement]
        public List<int> Values;

        public bool IsDirty;

        public Table() { }

        public Table(string tableName, List<int> values = null)
        {
            TableName = tableName;
            Values = values ?? new List<int>();
            IsDirty = true;
        }

        protected Database Database { get => Database.Get(); }

        public const string Empty = "<empty>";

        public void InsertRow(int value, bool redo = false)
        {
            if (Values.Contains(value))
            {
                throw new Exception(string.Format("Insert failed. Value {0} already exists in table {1}.", value, TableName));
            }

            Values.Add(value);
            IsDirty = true;

            if (!redo)
            {
                Database.LogManager.PersistLogRecord(new LogRecordTableRowInsert(TableName, value));
            }
        }

        public void DeleteRow(int value, bool redo = false)
        {
            if (!Values.Contains(value))
            {
                throw new Exception(string.Format("Delete failed. Value {0} does not exist in table {1}.", value, TableName));
            }

            Values.Remove(value);
            IsDirty = true;

            if (!redo)
            {
                Database.LogManager.PersistLogRecord(new LogRecordTableRowDelete(TableName, value));
            }
        }

        public void Clean()
        {
            IsDirty = false;
        }

        public void Print()
        {
            Console.WriteLine(ToString());
        }

        public string Serialize()
        {
            XmlSerializer tableSerializer = new XmlSerializer(typeof(Table));
            StringWriter stringWriter = new StringWriter();
            tableSerializer.Serialize(stringWriter, this);
            return new string(stringWriter.ToString().Where(c => !Environment.NewLine.Contains(c)).ToArray());
        }

        public static Table Deserialize(string tableString)
        {
            XmlSerializer tableSerializer = new XmlSerializer(typeof(Table));
            return (Table)tableSerializer.Deserialize(new StringReader(tableString));
        }

        public override string ToString()
        {
            return TableName + ":" + (Values.Any() ? string.Join(",", Values) : Empty);
        }

        public static Table Parse(string tableString)
        {
            string[] tableStringParts = tableString.Split(":");
            string tableName = tableStringParts[0];
            string valuesString = tableStringParts[1];
            return new Table(tableName, ParseValues(valuesString));
        }

        public static List<int> ParseValues(string valuesString)
        {
            return valuesString != Empty ?
                valuesString.Split(',').Select(value => int.Parse(value.Trim())).ToList() :
                new List<int>();
        }

        // Note that we are currently using a suboptimal implementation of writing to file.
        // In our current implementation Table object maps to Table, Column and Page in a conventional database.
        // Table would have multiple logical columns, while on the physical level Table would have multiple Pages.
        // Pages are of a fixed size (8KB for example) making random access writes in database files optimal.
        // However, in our implementation we are reading the whole file and only changing the desired table
        // which is a line in the database file.
        //
        public void WriteToFile(string filePath)
        {
            Utility.FileCreateIfNeeded(filePath);

            string tableString = ToString();
            string[] lines = Utility.FileReadAllLines(filePath);

            int index = Array.FindIndex(lines, (line) => Table.Parse(line).TableName == TableName);
            if (index == -1)
            {
                lines = lines.Append(tableString).ToArray();
            }
            else
            {
                lines[index] = tableString;
            }

            Utility.FileWriteAllLines(filePath, lines);
        }
    }
}
