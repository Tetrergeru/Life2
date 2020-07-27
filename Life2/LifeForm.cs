using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Life2
{
    class LifeForm : Form
    {
        private readonly World world;
        
        private readonly PictureBox worldPictureBox;
        private readonly PictureBox lensPictureBox;
        
        private Bitmap worldBitmap;
        private Bitmap lensBitmap;
        
        private readonly Button buttonStart;
        private readonly Button buttonDrawStyle;
        private readonly TextBox infoBox;
        private readonly Thread worldThread;
        
        private bool drawStyle = true;
        private string txt = "";
        private int lensX = -1;
        private int lensY = -1;
        private int chBotX = -1;
        private int chBotY = -1;
        
        public LifeForm(int width, int height)
        {
            Width = Life.Width * Life.BotSize + 18 + 50 + 200;
            Height = Life.Height * Life.BotSize + 48;

            worldBitmap = new Bitmap(1, 1);
            lensBitmap = new Bitmap(1, 1);

            world = new World(Life.Width, Life.Height, Life.StartupBotCount, 0);
            worldThread = new Thread(world.RunThread);
            worldThread.Start();
 
            worldPictureBox = new PictureBox();
            worldPictureBox.SetBounds(0, 0, width * Life.BotSize, height * Life.BotSize);
            worldPictureBox.MouseClick += WorldClick;
            Controls.Add(worldPictureBox);

            lensPictureBox = new PictureBox();
            lensPictureBox.SetBounds(width * Life.BotSize + 50, 0, 200, 200);
            lensPictureBox.MouseClick += LensPictureBoxClick;
            Controls.Add(lensPictureBox);

            infoBox = new TextBox
            {
                Left = width * Life.BotSize + 50,
                Top = 200,
                Multiline = true,
                Width = 200,
                Height = 200,
                BackColor = Color.Black,
                ForeColor = Color.LawnGreen,
                ReadOnly = true,
                AcceptsTab = true,
                ScrollBars = ScrollBars.Vertical
            };
            Controls.Add(infoBox);

            var timerDraw = new Timer { Interval = 20 };
            timerDraw.Tick += OnTimerDraw;
            timerDraw.Start();

            buttonStart = new Button
            {
                Text = "Run",
                Width = 50,
                Height = 30,
                Top = 0,
                Left = Life.Width * Life.BotSize
            };
            buttonStart.Click += Start;
            Controls.Add(buttonStart);

            buttonDrawStyle = new Button
            {
                Text = "Food",
                Width = 50,
                Height = 30,
                Top = 40,
                Left = Life.Width * Life.BotSize
            };
            buttonDrawStyle.Click += DrawStyle;
            Controls.Add(buttonDrawStyle);

            FormClosed += LifeFormClosed;
        }

        private void LifeFormClosed(object sender, FormClosedEventArgs e)
        {
            worldThread.Abort();
            Application.Exit();
        }
        
        private void LensPictureBoxClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (lensX < 0)
                    return;
                
                chBotX = lensX - 20 + e.X / 5;
                chBotY = lensY - 20 + e.Y / 5;
                infoBox.Text = world.GetInfo(lensX - 20 + e.X / 5, lensY - 20 + e.Y / 5);
            }
            else if (e.Button == MouseButtons.Right)
                if (lensX > -1 && chBotX != -1)
                    infoBox.Text = world.GetRelation(chBotX, chBotY, lensX - 20 + e.X / 5, lensY - 20 + e.Y / 5);
        }

        private void WorldClick(object sender, MouseEventArgs e)
        {
            if (world.start)
            {
                Start(null,null);
                Thread.Sleep(10);
            }
            lensBitmap.Dispose();
            lensBitmap = new Bitmap(200, 200);
            lensX = e.X / Life.BotSize;
            lensY = e.Y / Life.BotSize;
            world.DrawLens(lensBitmap, e.X / Life.BotSize, e.Y / Life.BotSize);
            lensPictureBox.Image = lensBitmap;
        }

        private void Start(object sender, EventArgs e)
        {
            buttonStart.Text = !world.start ? "Pause" : "Run";
            world.start = !world.start;
        }
        
        private void DrawStyle(object sender, EventArgs e)
        {
            drawStyle = !drawStyle;
            buttonDrawStyle.Text = drawStyle ? "Food" : "Energy";
        }
        
        private void OnTimerDraw(object sender, EventArgs e)
        {
            if (world != null)
            {
                txt = world.GetIteration.ToString();
                Text = txt +" : "+ world.time;
                worldBitmap.Dispose();
                worldBitmap = new Bitmap(world.GetWidth*Life.BotSize,world.GetHeight*Life.BotSize);
                world.Draw(worldBitmap, Life.BotSize, drawStyle);
                worldPictureBox.Image = worldBitmap;
            }
        }
    }
}
