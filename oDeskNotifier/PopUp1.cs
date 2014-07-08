﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace oDeskNotifier {

    public partial class PopUp1 : Form {
        private Form parent;
        private int time = 0;
        private int step = 50;
        private bool isClose = false;

        private Point from;
        private Point to;
        private float duration = 3;



        public PopUp1(Form parent, Point from, Point to, float duration) {
            InitializeComponent();
            this.parent = parent;
            this.from = from;
            this.to = to;
            this.duration = duration * 1000;

            parent.Focus();
            new Thread(TweenMove).Start();
        }

        private void button1_Click(object sender, EventArgs e) {
            isClose = true;
            this.Close();
        }

        private void TweenMove(object o) {
            time = 0;
            while (time < duration) {
                var value = 1f * time / (duration);
                var currentX = zTween.easeOutExpo(from.X, to.X, value);
                var currentY = zTween.easeOutExpo(from.Y, to.Y, value);


                var pos = Location;
                pos.X = (int)Math.Round(currentX);
                pos.Y = (int)Math.Round(currentY);
                try {
                    Invoke(new Action(() => Location = pos));
                }
                catch { }
                time += step;
                Thread.Sleep(step);
            }
            if (!isClose) Invoke(new Action(() => this.Close()));

        }
    }
}
