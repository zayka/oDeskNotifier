using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZWebUtilities;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Threading;


namespace oDeskNotifier {

    public partial class Form1 : Form {

        private string rssUrl = "";
        private SQLiteBase database;
        private int period = 50 * 1000;

        public Form1() {
            InitializeComponent();
            database = new SQLiteBase("oDeskJobs.sqlite3");

            rssUrl = "https://www.odesk.com/jobs/rss?t[]=0&t[]=1&dur[]=0&dur[]=1&dur[]=13&dur[]=26&dur[]=none&wl[]=10&wl[]=30&wl[]=none&tba[]=0&tba[]=1-9&tba[]=10-&exp[]=1&exp[]=2&exp[]=3&amount[]=Min&amount[]=Max&sortBy=relevance+desc";
        }

        private void Form1_Load(object sender, EventArgs e) {


            //   var jobs = GetCurretnList(rssUrl);
            //    database.Update(jobs);

            new Thread(MainThread).Start();
        }

        private List<oJob> GetCurretnList(string rssUrl) {
            //string rss = "";
            string rss = ZWeb.RequestString(rssUrl, "GET");
            /*
            using (StreamReader sr = new StreamReader("1.xml")) {
                rss = sr.ReadToEnd();
            }
            */
            List<oJob> jobs = new List<oJob>();
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(rss);
            var nodes = xdoc.SelectNodes("//item");
            foreach (XmlElement node in nodes) {
                oJob j = new oJob();
                j.Link = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(node.SelectSingleNode("link").InnerText));
                j.Title = HttpUtility.HtmlDecode(node.SelectSingleNode("title").InnerText).Replace("'", "");
                j.Description = HttpUtility.HtmlDecode(node.SelectSingleNode("description").InnerText).Replace("<br />", "").Replace("'", "");
                jobs.Add(j);
            }
            return jobs;
        }

        private void MainThread() {
            while (true) {
                try {
                    var jobs = GetCurretnList(rssUrl);
                    jobs.Reverse();

                    var inserted = database.Update(jobs);

                    List<SlidePopUp> popups = new List<SlidePopUp>();


                    foreach (var item in inserted) {

                        Point p1 = new Point(0, 0);
                        Point p2 = new Point(200, 0);

                        var p = new SlidePopUp(this, p1, p2, 5);
                        Invoke(new Action(() => p.Show()));
                        popups.Add(p);
                        Thread.Sleep(500);
                    }

                    var list = popups.Where(p => !p.IsDisposed);
                    while (list.Count() != 0) { Thread.Sleep(100); list = popups.Where(p => p.IsDisposed); }
                    Thread.Sleep(period);
                    if (this.IsDisposed) break;
                }
                catch { }
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            new Thread(MainThread).Start();
        }


    }
}
