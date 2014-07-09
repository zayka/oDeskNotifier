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
        private string configFilename = "config.cfg";
        private bool isRealClosing = false;



        public Form1() {
            InitializeComponent();
            database = new SQLiteBase("oDeskJobs.sqlite3");
            Settings.Load(configFilename);
            textBox_rss.Text = Settings.RSSURL;
            rssUrl = Settings.RSSURL;
            //rssUrl = "https://www.odesk.com/jobs/rss?c1[]=Software+Development&t[]=0&t[]=1&dur[]=0&dur[]=1&dur[]=13&dur[]=26&dur[]=none&wl[]=10&wl[]=30&wl[]=none&tba[]=0&tba[]=1-9&tba[]=10-&exp[]=1&exp[]=2&exp[]=3&amount[]=Min&amount[]=Max&q=NOT+%28android+OR+iphone%29&sortBy=s_ctime+desc";
        }

        private void Form1_Load(object sender, EventArgs e) {


               var jobs = GetCurretnList(rssUrl);
               jobs.Reverse();
               database.Update(jobs);
            /*
            var rect = Screen.AllScreens[0].WorkingArea;
            var p1 = new Point(rect.Right - SlidePopUp.WindowSize.Width, rect.Bottom);
            var p2 = new Point(rect.Right - SlidePopUp.WindowSize.Width, rect.Bottom - SlidePopUp.WindowSize.Height);
            var param = new SlidePopUp.Parameters(this, p1, p2);
            param.TweenDuration = 5;
            param.TotalDuration = 10000;
            param.Budget = 100;
            var p = new SlidePopUp(param);
            p.Show();
            */
            new Thread(MainThread).Start();
        }

        private List<oJob> GetCurretnList(string rssUrl) {
            string rss = ZWeb.RequestString(rssUrl, "GET");
            List<oJob> jobs = new List<oJob>();
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(rss);
            var nodes = xdoc.SelectNodes("//item");
            foreach (XmlElement node in nodes) {
                oJob j = new oJob();
                j.Link = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(node.SelectSingleNode("link").InnerText));
                j.Title = HttpUtility.HtmlDecode(node.SelectSingleNode("title").InnerText).Replace("'", "").Replace("- oDesk", ""); ;
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

                        var rect = Screen.AllScreens[0].WorkingArea;
                        var p1 = new Point(rect.Right - SlidePopUp.WindowSize.Width, rect.Bottom);
                        var p2 = new Point(rect.Right - SlidePopUp.WindowSize.Width, rect.Bottom - SlidePopUp.WindowSize.Height);

                        var param = new SlidePopUp.Parameters(this, p1, p2);
                        param.TweenDuration = 5;
                        param.TotalDuration = 10;
                        param.LabelLinkText = item.Title;
                        param.LabelLinkTag = item.Link;
                        param.LabelLinkTooltip = item.Description;
                        param.Budget = item.Budget;
                        var p = new SlidePopUp(param);
                        Invoke(new Action(() => p.Show()));
                        popups.Add(p);
                        Thread.Sleep(500);
                    }

                    var list = popups.Where(p => !p.IsDisposed);
                    while (list.Count() != 0) { Thread.Sleep(100); list = popups.Where(p => !p.IsDisposed); }

                    if (this.IsDisposed) break;
                    Thread.Sleep(period);

                }
                catch { }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Settings.RSSURL = rssUrl;
            Settings.Save();
            if (!isRealClosing&& e.CloseReason == CloseReason.UserClosing) {
                this.Hide();
                notifyIcon1.Visible = true;
                e.Cancel = true;
            }
        }

        private void textBox_rss_TextChanged(object sender, EventArgs e) {
            rssUrl = textBox_rss.Text;
        }

        private void Form1_Resize(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            isRealClosing = true;
            this.Close();
        }
    }
}
