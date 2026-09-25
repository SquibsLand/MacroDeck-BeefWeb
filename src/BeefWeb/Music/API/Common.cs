using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BeefWeb.Music.API
{
    internal interface IColumns
    {
        static abstract string[] ColumnsQuery { get; }
    }
    internal abstract class Columns<TSelf>
        where TSelf : class, IColumns
    {
        private int index;

        private int GetIndex()
        {
            return index++;
        }
        private List<JsonElement> columns;
        public Columns(List<JsonElement> columns)
        {
            ArgumentNullException.ThrowIfNull(columns);
            if(columns.Count == 0)
            {
                this.columns = new();
            }
            else if (columns.Count != TSelf.ColumnsQuery.Length)
            {
                throw new ArgumentException(
                    $"Expected {TSelf.ColumnsQuery.Length} columns, " +
                    $"but received {columns.Count}.",
                    nameof(columns));
            }
            this.columns = columns;
        }
        public static string GetColumnQuery()
        {
            return string.Join(",", TSelf.ColumnsQuery);
        }

        protected string NextString(string fallback = "")
        {
            int index = GetIndex();
            if (index < columns.Count) return columns[index].GetString() ?? fallback;
            else return fallback;
        }
        protected int? NextInt()
        {
            int index = GetIndex();
            if (index < columns.Count)
            {
                JsonElement element = columns[index];

                string value = element.ToString();

                if (IsUnknown(value)) return null;

                if (int.TryParse(value, out int number))
                {
                    return number;
                }
               
            } 
            return null;

        }
        private static bool IsUnknown(string value) => value == "?" || value == "";
    }
}
