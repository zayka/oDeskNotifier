using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace oDeskNotifier {
    static class Settings {

        public static string RSSURL = "https://www.odesk.com/jobs/rss?from=find-work";
      
        private static Hashtable config = new Hashtable();
        private static string configName = "config.cfg";

        public static void Load(string fileName) {
            config = new Hashtable();
            configName = fileName;
            InitHash();
            Load();
        }

        public static void Save() {
            InitHash();
            var list = config.Keys.Cast<string>().ToList().Select(key => new Tuple<string, object>(key, config[key])).ToList();
            list.Sort((t, o) => t.Item1.CompareTo(o.Item1));

            using (StreamWriter sw = new StreamWriter(configName)) {
                foreach (var key in list) {
                    sw.WriteLine(String.Format("{0,-30}", key.Item1) + "\t:\t" + key.Item2.ToString());
                }
            }
        }

        private static bool Load() {
            string configstring = "";
            if (File.Exists(configName)) {
                using (StreamReader sr = new StreamReader(configName)) {
                    configstring = sr.ReadToEnd() + ";";
                }
                MatchCollection mcol = Regex.Matches(configstring, "([_a-zA-Z0-9\\s]+?):([\\w\\W\\s\\S]+?)[\n;\r]+?");
                foreach (Match m in mcol) {
                    string paramName = m.Groups[1].Value.ToString().ToUpper().Trim();
                    string paramValue = m.Groups[2].Value.ToString().Trim();
                    config[paramName] = paramValue;
                }

                var fields = GetFields();

                object tmp = new object();
                foreach (var field in fields) {
                    field.SetTypeValue(config[field.Name]);
                }
                return true;
            }
            return false;
        }

        private static void InitHash() {
            config = new Hashtable();

            var fields = GetFields();

            object tmp = new object();
            foreach (var field in fields) {
                config[field.Name] = field.GetValue(tmp);
            }
        }

        private static FieldInfo[] GetFields() {
            FieldInfo[] fields;
            Type myType = typeof(Settings);
            fields = myType.GetFields(BindingFlags.Public | BindingFlags.Static);
            return fields;
        }

        private static int GetInt(string source) {
            int tmp = 0;
            source = source.Replace(",", "");
            Int32.TryParse(source, NumberStyles.Integer, CultureInfo.InvariantCulture, out tmp);
            return tmp;
        }

        private static bool GetBool(string source) {
            bool tmp = true;
            source = source.Trim().ToLower();
            Boolean.TryParse(source, out tmp);
            return tmp;
        }

        private static void SetTypeValue(this FieldInfo field, object value) {
            object tmp = new object();

            switch (field.FieldType.Name) {
                case "String":
                    field.SetValue(tmp, value.ToString());
                    break;
                case "Int32":
                    field.SetValue(tmp, GetInt(value.ToString()));
                    break;
                case "Boolean":
                    field.SetValue(tmp, GetBool(value.ToString()));
                    break;
                default:
                    break;
            }
        }
    }
}