using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Text.RegularExpressions;

namespace oDeskNotifier {
    class oJob : IDBEntity {
        public int oDeskID;
        public string Link;
        public string Title;
        private string description;
        public string Description { get { return description; } set { ParseDescription(value); } }


        public bool IsFixed { get { return Budget != 0; } }
        public int Budget = 0;
        public DateTime Date;
        public int Created;

        public oJob() {
            Created = UnixTime.Now;
        }

        #region Equality
        public override int GetHashCode() {
            return Link.GetHashCode();
        }

        public override bool Equals(object obj) {
            oJob other = obj as oJob;
            if (other == null) return false;
            return other.oDeskID == this.oDeskID;
        }
        #endregion

        #region DataBase
        string IDBEntity.ExistQuery() {
            return "SELECT * FROM Jobs WHERE oDeskID = '" + oDeskID + "' LIMIT 1";
        }

        string IDBEntity.UpdateQuery() {
            return "UPDATE Jobs SET " +
                   " oDeskID='" + oDeskID + "', " +
                   " Link='" + Link + "', " +
                   " Title='" + Title + "', " +
                   " Description='" + Description + "', " +
                   " IsFixed='" + (IsFixed ? 1 : -1).ToString() + "', " +
                   " Budget='" + Budget + "', " +
                   " Date='" + UnixTime.DateTimeToUnixTimestamp(Date).ToString() + "'" +

                    " WHERE oDeskID='" + oDeskID + "'";
        }

        string IDBEntity.InsertQuery() {
            StringBuilder result = new StringBuilder(100);
            result.Append("INSERT INTO Jobs (");
            result.Append("oDeskId, Link, Title, Description, IsFixed, Budget, Date, Created) VALUES ('");
            result.Append(
                 String.Join("','", new object[] {     
                        oDeskID.ToString(),
                        Link,
                        Title.Replace("'","\""),
                        Description.Replace("'","\""),
                        (IsFixed ? 1 : -1).ToString() ,
                        Budget.ToString(),
                        UnixTime.DateTimeToUnixTimestamp(Date).ToString(),
                        Created
                   }));
            result.Append("')");
            return result.ToString();
        }

        void IDBEntity.Update(Hashtable newElement) {
            this.oDeskID = Utilities.GetInt(newElement["oDeskID"].ToString());
            this.Link = newElement["Link"].ToString();
            this.Title = newElement["Title"].ToString();
            this.description = newElement["Description"].ToString();
            this.Budget = Utilities.GetInt(newElement["Budget"].ToString());
            this.Date = UnixTime.UnixTimeStampToDateTime(Utilities.GetInt(newElement["Date"].ToString()));
            this.Created = Utilities.GetInt(newElement["Created"].ToString());
        }

        string IDBEntity.DeleteQuery() {
            return "DELETE FROM Jobs WHERE oDeskID = '" + oDeskID;
        }


        public static oJob Load(Hashtable newElement) {
            oJob j = new oJob();
            j.oDeskID = Utilities.GetInt(newElement["oDeskID"].ToString());
            j.Link = newElement["Link"].ToString();
            j.Title = newElement["Title"].ToString();
            j.Budget = Utilities.GetInt(newElement["Budget"].ToString());
            j.Date = UnixTime.UnixTimeStampToDateTime(Utilities.GetInt(newElement["Date"].ToString()));
            j.Created = Utilities.GetInt(newElement["Created"].ToString());
            j.description = newElement["Description"].ToString();
            return j;
        }
        #endregion

        private void ParseDescription(string desc) {
            Budget = GetBudget(desc);
            Date = GetDate(desc);
            oDeskID = GetID(desc);
            description = GetDescription(desc);
        }

        private int GetBudget(string desc) {
            var m = Regex.Match(desc, @"<b>Budget</b>: \$(\d+)");
            if (m.Success) return Utilities.GetInt(m.Groups[1].Value);
            return 0;
        }

        private DateTime GetDate(string desc) {
            var m = Regex.Match(desc, @">Posted On</b>: ([\w\W\s\S]+?)<b");
            if (m.Success) return DateTime.Parse(m.Groups[1].Value.Replace(" UTC", "Z"));
            return DateTime.Now;
        }

        private int GetID(string desc) {
            var m = Regex.Match(desc, @">ID</b>: (\d+)<b");
            if (m.Success) return Utilities.GetInt(m.Groups[1].Value);
            return 0;
        }

        private string GetDescription(string desc) {
            var body = Regex.Split(desc, "<b>");
            if (body.Count() > 0)
                return body[0];
            else
                return "";
        }

        public override string ToString() {
            return oDeskID + ":" + Title;
        }
    }
}
