using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Drawing2D;

namespace oDeskNotifier {

    public partial class SlidePopUp : Form {
        public class Parameters {
            public Point From;
            public Point To;
            public Form Parent;
            public float TweenDuration = 1;
            public float TotalDuration = 2;
            public zTween.EaseType easeType = zTween.EaseType.EaseOutExpo;

            //data
            public string LabelLinkText = "Text";
            public string LabelLinkTag = "http://google.com";
            public string LabelLinkTooltip = "This is a text";

            public int Budget = 0;


            public Parameters(Form parent, Point from, Point to) {
                this.Parent = parent;
                this.From = from;
                this.To = to;
            }
        }

        private Form parent;
        private int time = 0;
        private int step = 50;
        private bool isClose = false;

        private Point from;
        private Point to;
        private float tweenDuration = 3;
        private float totalDuration = 3;

        public static Size WindowSize = new Size(221, 95);
        public Point Displacement= new Point(0,0);


        public SlidePopUp(Parameters param) {
            this.Opacity = 0.5;
            
            this.Location = param.From;
            InitializeComponent();
            this.Size = WindowSize;

            this.parent = param.Parent;
            this.from = param.From;
            this.to = param.To;
            this.tweenDuration = param.TweenDuration * 1000;
            this.totalDuration = param.TotalDuration * 1000;

            if (param.Budget > 0) label_budget.Text = "Budget: $" + param.Budget;
            else label_budget.Text = "Budget: Hourly";

            this.Location = from;
            new Thread(TweenMove).Start();

            linkLabel_Title.Text = param.LabelLinkText;
            linkLabel_Title.Tag = param.LabelLinkTag;
            ToolTip t = new ToolTip();
            t.AutoPopDelay = 5000;
            t.InitialDelay = 1000;
            t.ReshowDelay = 500;
            t.ShowAlways = true;
            t.SetToolTip(this.linkLabel_Title, param.LabelLinkTooltip);
        }

        private void button1_Click(object sender, EventArgs e) {
            isClose = true;
            this.Close();
        }

        private void TweenMove(object o) {
            time = 0;
            while (time < tweenDuration) {
                if (isClose) break;
                var value = 1f * time / (tweenDuration);
                var currentX = zTween.easeOutExpo(from.X, to.X, value);
                var currentY = zTween.easeOutExpo(from.Y, to.Y, value);


                var pos = Location;
                pos.X = (int)Math.Round(currentX)+Displacement.X;
                pos.Y = (int)Math.Round(currentY)+Displacement.Y;

               
                try {
                    if (this.InvokeRequired) {
                        Invoke(new Action(() => this.Opacity = value + 0.5));
                        Invoke(new Action(() => { if (!this.IsDisposed) this.Location = pos; }));
                    }
                    else this.Location = pos;
                }
                catch { }



                time += step;
                Thread.Sleep(step);
            }

            while (time < totalDuration) { if (isClose||parent.IsDisposed) break;  time += step; Thread.Sleep(step); }

            try {
                if (!isClose) Invoke(new Action(() => { if (!this.IsDisposed) this.Close(); }));
            }
            catch { }

        }

        private void linkLabel_Title_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            System.Diagnostics.Process.Start(linkLabel_Title.Tag.ToString());
        }

        private void SlidePopUp_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                isClose = true;
                this.Close();
            }
        }      
    }
}
