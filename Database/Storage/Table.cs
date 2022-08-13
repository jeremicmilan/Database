using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public class Table
    {
        public string TableName { get; set; }

        public List<Page> Pages { get; private set; }

        public IReadOnlyList<int> Values => Pages.SelectMany(page => page.Values).ToList().AsReadOnly();
        public int LogSequenceNumberMax => Pages.Max(page => page.LogSequenceNumberMax);

        public Table()
        { }

        private Table(string tableName)
        {
            TableName = tableName;
        }

        public Table(string tableName, List<Page> pages)
            : this(tableName)
        {
            Pages = pages;
        }

        private const string Empty = Page.Empty;

        private static void TableOperation(
            Func<bool> preConditionCheck,
            string preConditionFailureMessage,
            Action tableOperation)
        {
            if (preConditionCheck())
            {
                throw new Exception(preConditionFailureMessage);
            }

            tableOperation();
        }

        public void InsertRow(int value)
        {
            TableOperation(
                preConditionCheck : () => Values.Contains(value),
                preConditionFailureMessage: string.Format("Insert failed. Value {0} already exists in table {1}.", value, TableName),
                tableOperation: () => AddValue(value));
        }

        private void AddValue(int value)
        {
            foreach (Page page in Pages)
            {
                if (!page.IsPageFull)
                {
                    page.AddValue(value);
                    return;
                }
            }

            Page newPage = Page.CreateAndProcess(TableName);
            newPage.AddValue(value);
            AddPageToCache(newPage);
        }

        public void DeleteRow(int value)
        {
            TableOperation(
                preConditionCheck: () => !Values.Contains(value),
                preConditionFailureMessage: string.Format("Delete failed. Value {0} does not exist in table {1}.", value, TableName),
                tableOperation: () => RemoveValue(value));
        }

        private void RemoveValue(int value)
        {
            foreach (Page page in Pages)
            {
                if (page.Values.Contains(value))
                {
                    page.RemoveValue(value);
                    return;
                }
            }

            throw new Exception(String.Format("Cannot remove value {0} as it does not exist in table {1}.", value, TableName));
        }

        public static List<int> ParseValues(string valuesString, bool emptySupported = true)
        {
            if (valuesString == Empty)
            {
                if (emptySupported)
                {
                    return new List<int>();
                }
                else
                {
                    throw new Exception("Clause <empty> not supported for this statement.");
                }
            }

            return valuesString.Split(',').Select(value => int.Parse(value.Trim())).ToList();
        }

        public void AddPageToCache(Page page)
        {
            if (Pages.Any(p => p.PageId == page.PageId))
            {
                throw new Exception("Table already containes page: " + page.ToString());
            }

            Pages.Add(page);
        }

        public void Print()
        {
            Utility.LogMessage(ToString());
        }

        public override string ToString()
        {
            return TableName + ":" + (Values.Any() ? string.Join(",", Values) : Empty);
        }
    }
}
