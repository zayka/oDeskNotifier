using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Drawing;

namespace oDeskNotifier
{
    static class Utilities {

        public static int GetInt(string source) {
            int tmp = 0;
            source = source.Replace(",", "").Replace(".", "");
            source = Regex.Match(source, "([\\d]+)").Groups[1].Value;
            Int32.TryParse(source, NumberStyles.Integer, CultureInfo.InvariantCulture, out tmp);
            return tmp;
        }

        private static bool GetBool(string source) {
            bool tmp = true;
            source = source.Trim().ToLower();
            Boolean.TryParse(source, out tmp);
            return tmp;
        }

        public static string ReadColumn(this SQLiteDataReader reader, string column, List<string> columns) {
            if (columns.Contains(column)) return reader[column].ToString();
            else return "";
        }
    }
}
