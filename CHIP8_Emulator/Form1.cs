using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Media;
using System.Diagnostics;

namespace CHIP8_Emulator
{
    public partial class Form1 : Form
    {
        Bitmap screen;
        CPU chip8;

        Thread cpuThread;

        byte[] input;
        int CPUSpeed;

        Stopwatch watch;

        public Form1()
        {
            InitializeComponent();
            Init();
        }

        void Init()
        {
            watch = new Stopwatch();
            CPUSpeed = 1;
            screen = new Bitmap(64, 32);
            input = new byte[16];
            chip8 = new CPU();
            //chip8.LoadRAM(@"../../DAVID_TEST");
            chip8.LoadRAM(@"./c8games/BRIX");

            cpuThread = new Thread(RunCPU);
            cpuThread.Start();
        }

        void RunCPU()
        {
            watch.Start();
            while (chip8.running)
            {
                watch.Restart();
                chip8.Tick();
                chip8.SetInputflags(input);

                
                while (watch.ElapsedTicks < 3000) { }
                //Thread.Sleep(CPUSpeed);
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(screen, new RectangleF(10, 30, this.ClientSize.Width - 10, this.ClientSize.Height - 40));
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (chip8.drawFlag)
            {
                UpdateBitmap();
                this.Invalidate();
                chip8.drawFlag = false;
            }
        }

        void UpdateBitmap()
        {
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (chip8.display[i + j * 64] == 0)
                        screen.SetPixel(i, j, Color.Black);
                    else
                        screen.SetPixel(i, j, Color.White);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cpuThread.Abort();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            chip8.timeFlag = true;
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cpuThread.Suspend();
            chip8.Reset();
            cpuThread.Resume();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                cpuThread.Abort();

                chip8.LoadRAM(openFileDialog1.FileName);
                chip8.Reset();

                cpuThread = new Thread(RunCPU);
                cpuThread.Start();
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                    input[0] = 1;
                    break;
                case Keys.D1:
                    input[1] = 1;
                    break;
                case Keys.K:
                    input[2] = 1;
                    break;
                case Keys.D3:
                    input[3] = 1;
                    break;
                case Keys.J:
                    input[4] = 1;
                    break;
                case Keys.Space:
                    input[5] = 1;
                    break;
                case Keys.L:
                    input[6] = 1;
                    break;
                case Keys.D7:
                    input[7] = 1;
                    break;
                case Keys.I:
                    input[8] = 1;
                    break;
                case Keys.D9:
                    input[9] = 1;
                    break;
                case Keys.A:
                    input[10] = 1;
                    break;
                case Keys.B:
                    input[11] = 1;
                    break;
                case Keys.C:
                    input[12] = 1;
                    break;
                case Keys.D:
                    input[13] = 1;
                    break;
                case Keys.E:
                    input[14] = 1;
                    break;
                case Keys.F:
                    input[15] = 1;
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                    input[0] = 0;
                    break;
                case Keys.D1:
                    input[1] = 0;
                    break;
                case Keys.K:
                    input[2] = 0;
                    break;
                case Keys.D3:
                    input[3] = 0;
                    break;
                case Keys.J:
                    input[4] = 0;
                    break;
                case Keys.Space:
                    input[5] = 0;
                    break;
                case Keys.L:
                    input[6] = 0;
                    break;
                case Keys.D7:
                    input[7] = 0;
                    break;
                case Keys.I:
                    input[8] = 0;
                    break;
                case Keys.D9:
                    input[9] = 0;
                    break;
                case Keys.A:
                    input[10] = 0;
                    break;
                case Keys.B:
                    input[11] = 0;
                    break;
                case Keys.C:
                    input[12] = 0;
                    break;
                case Keys.D:
                    input[13] = 0;
                    break;
                case Keys.E:
                    input[14] = 0;
                    break;
                case Keys.F:
                    input[15] = 1;
                    break;
            }
        }

        private void speedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpeedControl dialog = new SpeedControl ();

            dialog.SetValue(CPUSpeed);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                CPUSpeed = dialog.GetValue();
            }
        }
    }
}
