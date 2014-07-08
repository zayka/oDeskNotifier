using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Collections;

namespace oDeskNotifier {   
    
    class SQLiteBase : IDisposable {
        static object locker = new object();
        string dbName = "";
        SQLiteConnection sqliteConnection;
        SQLiteConnection SQliteConnection {
            get {
                lock (locker) {
                    if (dbName == "") { throw new Exception("Database name empty"); }
                    if (sqliteConnection == null) {
                        sqliteConnection = new SQLiteConnection("URI=file:" + dbName);
                        sqliteConnection.Open();

                    }
                    return sqliteConnection;
                }
            }
        }

        public SQLiteBase(string dbName) {
            this.dbName = dbName;
        }

        public void Dispose() {
            if (sqliteConnection != null && sqliteConnection.State == System.Data.ConnectionState.Open)
                sqliteConnection.Close();
        }

        public Hashtable Exist(string query) {
            Hashtable t = new Hashtable();
            using (SQLiteCommand cmd = new SQLiteCommand(SQliteConnection)) {
                cmd.CommandText = query;
                var rdr = cmd.ExecuteReader();

                while (rdr.Read()) {

                    for (int i = 0; i < rdr.FieldCount; i++) {
                        t.Add(rdr.GetName(i), rdr.GetValue(i));
                    }
                }
            }
            return t;
        }

        public bool Delete<T>(T entity) where T : IDBEntity {
            string cmdQuery = entity.DeleteQuery();
            SQLiteCommand cmd = SQliteConnection.CreateCommand();
            cmd.CommandText = cmdQuery;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            return true;
        }

        public IEnumerable<T> Update<T>(IEnumerable<T> list) where T : IDBEntity {
            var inserted = new List<T>();
            using (SQLiteTransaction tr = SQliteConnection.BeginTransaction()) {
                foreach (var item in list) {
                    try {
                        string cmdQuery = "";
                        var table = Exist(item.ExistQuery());
                        if (table.Keys.Count > 0) {
                            cmdQuery = item.UpdateQuery();
                            item.Update(table);
                        }
                        else {
                            cmdQuery = item.InsertQuery();
                            inserted.Add(item);
                        }

                        SQLiteCommand cmd = SQliteConnection.CreateCommand();
                        cmd.CommandText = cmdQuery;

                        cmd.Transaction = tr;
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                    catch (Exception ex) { }
                }
                tr.Commit();
            }
            return inserted;
        }

        public List<Hashtable> Load(string tableName) {
            List<Hashtable> result = new List<Hashtable>();
            using (SQLiteCommand cmd = SQliteConnection.CreateCommand()) {

                cmd.CommandText = "SELECT * FROM " + tableName;
                var rdr = cmd.ExecuteReader();
                while (rdr.Read()) {
                    Hashtable t = new Hashtable();
                    for (int i = 0; i < rdr.FieldCount; i++) {
                        t.Add(rdr.GetName(i), rdr.GetValue(i));
                    }
                    result.Add(t);
                }

            }
            return result;
        }
    }

    interface IDBEntity {
        string ExistQuery();
        string UpdateQuery();
        string InsertQuery();
        void Update(Hashtable newElement);
        string DeleteQuery();
    }
}
