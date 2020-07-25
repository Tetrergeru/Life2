using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Life
{
    class LifeForm : System.Windows.Forms.Form
    {
        private World world;
        private System.Windows.Forms.PictureBox pb;
        private System.Windows.Forms.PictureBox lens;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonDrawStyle;
        private bool drawStyle = true;
        private System.Windows.Forms.Timer timerDraw;
        private System.Threading.Thread t;
        private string txt = "";
        private Bitmap worldBit;
        private Bitmap lensBit;
        private int lensX = -1;
        private int lensY = -1;
        private TextBox infoBox;
        private int chBotX = -1;
        private int chBotY = -1;
        public LifeForm(int width,int height)
        {
            this.Width = Life.width * Life.botSize + 18 + 50 + 200;
            this.Height = Life.height * Life.botSize + 48;

            worldBit = new Bitmap(1, 1);
            lensBit = new Bitmap(1, 1);

            world = new World(Life.width, Life.height, Life.nmb, 0);
            t = new System.Threading.Thread(new System.Threading.ThreadStart(world.RunThr));
            t.Start();

            pb = new System.Windows.Forms.PictureBox();
            pb.SetBounds(0, 0, width * Life.botSize, height * Life.botSize);
            pb.MouseClick += WorldClick;
            this.Controls.Add(pb);

            lens = new System.Windows.Forms.PictureBox();
            lens.SetBounds(width * Life.botSize + 50, 0, 200, 200);
            lens.MouseClick += LensClick;
            this.Controls.Add(lens);

            infoBox = new TextBox()
            {
                Left = width * Life.botSize + 50,
                Top = 200,
                Multiline = true,
                Width = 200,
                Height = 200,
                BackColor = Color.Black,
                ForeColor = Color.LawnGreen,
                ReadOnly = true,
                AcceptsTab = true,
            };
            infoBox.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(this.infoBox);

            this.timerDraw = new System.Windows.Forms.Timer();
            timerDraw.Interval = 20;
            timerDraw.Tick += OnTimerDraw;
            timerDraw.Start();

            this.buttonStart = new System.Windows.Forms.Button
            {
                Text = "Run",
                Width = 50,
                Height = 30,
                Top = 0,
                Left = Life.width * Life.botSize
            };
            this.buttonStart.Click += Start;
            this.Controls.Add(this.buttonStart);

            this.buttonDrawStyle = new System.Windows.Forms.Button
            {
                Text = "Food",
                Width = 50,
                Height = 30,
                Top = 40,
                Left = Life.width * Life.botSize
            };
            this.buttonDrawStyle.Click += DrawStyle;
            this.Controls.Add(this.buttonDrawStyle);

            this.FormClosed += LifeFormClosed;
        }

        private void LifeFormClosed(object sender, FormClosedEventArgs e)
        {
            t.Abort();
            Application.Exit();
        }
        
        private void LensClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (lensX > -1)
                {
                    chBotX = this.lensX - 20 + e.X / 5;
                    chBotY = this.lensY - 20 + e.Y / 5;
                    this.infoBox.Text = world.GetInfo(this.lensX - 20 + e.X / 5, lensY - 20 + e.Y / 5);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (lensX > -1 && chBotX != -1)
                {
                    this.infoBox.Text = world.GetRelation(chBotX, chBotY, this.lensX - 20 + e.X / 5, lensY - 20 + e.Y / 5);
                }
            }
        }

        private void WorldClick(object sender, MouseEventArgs e)
        {
            if (world.start)
            {
                Start(null,null);
                System.Threading.Thread.Sleep(10);
            }
            lensBit.Dispose();
            lensBit = new Bitmap(200, 200);
            lensX = e.X / Life.botSize;
            lensY = e.Y / Life.botSize;
            world.DrawLens(lensBit, e.X / Life.botSize, e.Y / Life.botSize);
            lens.Image = lensBit;
        }

        private void Start(object sender, EventArgs e)
        {
            this.buttonStart.Text = !world.start ? "Pause" : "Run";
            world.start = !world.start;
        }
        private void DrawStyle(object sender, EventArgs e)
        {
            this.drawStyle = !this.drawStyle;
            this.buttonDrawStyle.Text = this.drawStyle ? "Food" : "Energy";
        }
        private void OnTimerDraw(object sender, EventArgs e)
        {
            if (world != null)
            {
                txt = world.GetIteration.ToString();
                this.Text = txt +" : "+ world.time.ToString();
                worldBit.Dispose();
                worldBit = new Bitmap(world.GetWidth*Life.botSize,world.GetHeight*Life.botSize);
                world.Draw(worldBit, Life.botSize, drawStyle);
                this.pb.Image = worldBit;
            }
        }
    }
}
