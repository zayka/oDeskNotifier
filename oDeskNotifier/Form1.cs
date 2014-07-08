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


namespace oDeskNotifier {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
           
            string result = "";
            //result = ZWeb.RequestString(textBox_rss.Text, "GET");
            
            
            using (StreamReader sr = new StreamReader("1.xml")) {
                result = sr.ReadToEnd();
            }
            result =result;
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(result);
            var nodes = xdoc.SelectNodes("//item");
            foreach (XmlElement node in nodes) {
                oJob j = new oJob();
                j.Link =  HttpUtility.UrlDecode(HttpUtility.HtmlDecode(node.SelectSingleNode("link").InnerText));
                j.Title =  HttpUtility.HtmlDecode(node.SelectSingleNode("title").InnerText);
                j.Description = HttpUtility.HtmlDecode(node.SelectSingleNode("description").InnerText).Replace("<br />","");
            }
        }

      
    }
}
