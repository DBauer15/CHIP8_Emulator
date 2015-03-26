using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CHIP8_Emulator
{
    public partial class SpeedControl : Form
    {
        public SpeedControl()
        {
            InitializeComponent();
        }

        public void SetValue(int value)
        {
            trackBar1.Value = value;
        }

        public int GetValue()
        {
            return trackBar1.Value;
        }
    }
}
