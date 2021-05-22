using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Database
{
    public class Table
    {
        [XmlElement]
        public string TableName;

        [XmlElement]
        public List<int> Values;

        public Table() { }

        public Table(string tableName, List<int> values = null)
        {
            TableName = tableName;
            Values = values ?? new List<int>();
        }

        protected Database Database { get => Database.Get(); }

        public void Insert(int value, bool redo = false)
        {
            if (Values.Contains(value))
            {
                throw new Exception(string.Format("Insert failed. Value {0} already exists in table {1}.", value, TableName));
            }

            Values.Add(value);

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTableInsert(TableName, value);
                Database.LogManager.WriteLogRecordToDisk(logRecord);
            }
        }

        public void Print()
        {
            Console.WriteLine("-------- (start) TABLE: " + TableName);
            
            if (Values.Any())
            {
                Console.WriteLine(string.Join(", ", Values));
            }
            else
            {
                Console.WriteLine("<empty>");
            }

            Console.WriteLine("--------  (end)  TABLE: " + TableName);
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
    }
}
