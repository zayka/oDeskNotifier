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
        private int period = 10 * 1000;
        private string configFilename = "config.cfg";
        private string databaseFilename = "oDeskJobs.sqlite3";
        bool onHoverTextBox = false;


        public Form1() {

            InitializeComponent();
            Settings.Load(configFilename);
            textBox_rss.Text = Settings.RSSURL;
            rssUrl = Settings.RSSURL;
        }

        private void Form1_Load(object sender, EventArgs e) {

            if (File.Exists(databaseFilename))
                database = new SQLiteBase(databaseFilename);
            else { MessageBox.Show("Database file " + databaseFilename + " not found."); this.Close(); return; }

            var jobs = GetCurretnList(rssUrl);
            jobs.Reverse();
            database.Update(jobs);
            AddRow(jobs);
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
            if (dataGridView_jobGrid.CurrentCell != null) dataGridView_jobGrid.CurrentCell.Selected = false;
        }

        private List<oJob> GetCurretnList(string rssUrl) {
            string rss = ZWeb.RequestString(rssUrl, "GET");
            List<oJob> jobs = new List<oJob>();
            XmlDocument xdoc = new XmlDocument();
            try {
                xdoc.LoadXml(rss);
            }
            catch { MessageBox.Show("Bad RSS", "LOL"); }
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
                        var p1 = new Point(rect.Right, rect.Bottom - SlidePopUp.WindowSize.Height);
                        var p2 = new Point(rect.Right - SlidePopUp.WindowSize.Width, rect.Bottom - SlidePopUp.WindowSize.Height);

                        var param = new SlidePopUp.Parameters(this, p1, p2);
                        param.TweenDuration = 5;
                        param.TotalDuration = 10;
                        param.LabelLinkText = item.Title;
                        param.LabelLinkTag = item.Link;
                        param.LabelLinkTooltip = item.Description;
                        param.Budget = item.Budget;
                        var slidePopUp = new SlidePopUp(param);
                        Invoke(new Action(() => slidePopUp.Show()));
                        popups.ForEach(p => { if (p != null && !p.IsDisposed) { var pTmp = p.Displacement; pTmp.Y -= SlidePopUp.WindowSize.Height - 1; p.Displacement = pTmp; } }); //компилятор приказал так сделать (CS1690)
                        popups.Add(slidePopUp);
                        InsertRow(item);
                        Thread.Sleep(500);
                    }

                    var list = popups.Where(p => !p.IsDisposed);
                    while (list.Count() != 0) { Thread.Sleep(100); list = popups.Where(p => !p.IsDisposed); }

                    if (this.IsDisposed) break;

                    int time = 0;
                    int step = 1000;
                    while (time < period) { if (this.IsDisposed) return; time += step; Thread.Sleep(step); }

                }
                catch { }
            }
        }

        private void AddRow(oJob job) {
            var list = new List<oJob>();
            list.Add(job);
            AddRow(list);
        }

        private void AddRow(IEnumerable<oJob> list) {
            var rows = CreateRowArray(list);
            Threadsafe(() => {
                dataGridView_jobGrid.Rows.AddRange(rows.ToArray());
                if (dataGridView_jobGrid.CurrentCell != null) dataGridView_jobGrid.CurrentCell.Selected = false;
                dataGridView_jobGrid.Invalidate();
                dataGridView_jobGrid.Update();
            });
        }

        private void InsertRow(oJob job) {
            var list = new List<oJob>();
            list.Add(job);
            InsertRow(list);
        }

        private void InsertRow(IEnumerable<oJob> list) {
            Threadsafe(() => {
                var rows = CreateRowArray(list);
                for (int i = 0; i < rows.Length; i++) {
                    dataGridView_jobGrid.Rows.Insert(0, rows[i]);
                }
                dataGridView_jobGrid.CurrentCell.Selected = false;
            });
        }

        private DataGridViewRow[] CreateRowArray(IEnumerable<oJob> list) {
            int totaColumns = 50;
            object[] buffer = new object[totaColumns];
            int idx = 0;
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (var job in list) {
                idx = 0;
                buffer[idx++] = job.Date.ToString("dd.MM  HH:mm");
                buffer[idx++] = job.oDeskID;
                buffer[idx++] = job.Title;
                buffer[idx++] = job.Budget == 0 ? "Hourly" : job.Budget.ToString();
                rows.Add(new DataGridViewRow());
                rows[rows.Count - 1].CreateCells(dataGridView_jobGrid, buffer);
                rows[rows.Count - 1].Cells[0].Tag = job.Link;
            }
            return rows.ToArray();
        }

        private void Threadsafe(Action body) {
            if (this.InvokeRequired)
                Invoke(body);
            else body();
        }

        #region GUI
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Settings.RSSURL = rssUrl;
            Settings.Save();
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
            this.Close();
        }

        private void dataGridView_jobGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            var l1 = dataGridView_jobGrid.Rows.Cast<DataGridViewRow>().ToList();
            int col = e.ColumnIndex;
            if (col == dataGridView_jobGrid.Columns["Budget"].Index) {
                int order = dataGridView_jobGrid.Columns[col].HeaderCell.SortGlyphDirection == SortOrder.Descending ? -1 : 1;
                l1.Sort((t, o) => {
                    int c1 = 0;
                    int c2 = 0;
                    Int32.TryParse(t.Cells[col].Value.ToString(), out c1);
                    Int32.TryParse(o.Cells[col].Value.ToString(), out c2);
                    return order * c1.CompareTo(c2);
                });
                dataGridView_jobGrid.Rows.Clear();
                dataGridView_jobGrid.Rows.AddRange(l1.ToArray());
            }
        }

        private void dataGridView_jobGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) return;
            dataGridView_jobGrid.Rows[e.RowIndex].Selected = true;
        }

        private void dataGridView_jobGrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) return;
            dataGridView_jobGrid.Rows[e.RowIndex].Selected = false;
        }

        private void dataGridView_jobGrid_CellClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex == -1) return;
            System.Diagnostics.Process.Start(dataGridView_jobGrid[0, e.RowIndex].Tag.ToString());
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e) {
            var parent = sender as Control;
            var ctrl = parent.GetChildAtPoint(e.Location);
            if (ctrl == textBox_rss && !onHoverTextBox) { onHoverTextBox = true; textBox_rss.Enabled = true; return; }

            onHoverTextBox = false;
            textBox_rss.Enabled = false;
        }
        #endregion

    }
}
