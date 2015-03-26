using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace CHIP8_Emulator
{
    class CPU
    {
        static Random randy = new Random();

        public bool running;

        byte[] RAM;
        byte[] Vx;
        ushort I;

        byte DT, ST;

        ushort PC;
        byte SP;
        ushort[] Stack;

        public bool drawFlag;
        public bool timeFlag;
        byte[] inputFlags;

        ushort opcode;

        public byte[] display;

        public CPU()
        {
            RAM = new byte[4096];
            Vx = new byte[16];

            PC = 0x200;
            Stack = new ushort[16];

            inputFlags = new byte[16];

            drawFlag = false;
            timeFlag = false;
            display = new byte[64 * 32];
        }

        public void Reset()
        {
            Vx = new byte[16];
            PC = 0x200;
            Stack = new ushort[16];

            inputFlags = new byte[16];

            DT = 0x00;
            ST = 0x00;

            opcode = 0x00;
            I = 0x00;

            drawFlag = false;
            timeFlag = false;
            display = new byte[64 * 32];
        }

        public void LoadRAM(string fileName)
        {
            byte[] temp = File.ReadAllBytes(fileName);
            byte[] fontset = File.ReadAllBytes(@"../../fontset");

            for (int i = 0; i < 80; i++)
                RAM[i] = fontset[i];

            for (int i = 0; i < temp.Length; i++)
                RAM[0x200 + i] = temp[i];

            running = true;
        }

        public void SetInputflags(byte[] input)
        {
            inputFlags = input;
        }

        public void Tick()
        {
            opcode = (ushort)(RAM[PC] << 8 | RAM[PC + 1]);

            #region Execution
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x00E0:
                            display = new byte[64 * 32];
                            drawFlag = true;
                            PC += 2;
                            break;
                        case 0x00EE:
                            SP--;
                            PC = Stack[SP];
                            PC += 2;
                            break;
                        case 0x0000:
                            break;
                        default:
                            MessageBox.Show("Unknown Opcode "+opcode);
                            //running = false;
                            break;
                    }
                    break;

                case 0x1000:
                    PC = (ushort)(opcode & 0x0FFF);
                    break;

                case 0x2000:
                    Stack[SP] = PC;
                    SP++;
                    PC = (ushort)(opcode & 0x0FFF);
                    break;

                case 0x3000:
                    if (Vx[(opcode & 0x0F00) >> 8] == (byte)(opcode & 0x00FF))
                        PC += 2;
                    PC += 2;
                    break;

                case 0x4000:
                    if (Vx[(opcode & 0x0F00) >> 8] != (byte)(opcode & 0x00FF))
                        PC += 2;
                    PC += 2;
                    break;

                case 0x5000:
                    if (Vx[(opcode & 0x0F00) >> 8] == Vx[(opcode & 0x00F0) >> 4])
                        PC += 2;
                    PC += 2;
                    break;

                case 0x6000:
                    Vx[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
                    PC += 2;
                    break;

                case 0x7000:
                    Vx[(opcode & 0x0F00) >> 8] = (byte)(Vx[(opcode & 0x0F00) >> 8] + (opcode & 0x00FF));
                    PC += 2;
                    break;

                case 0x8000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000:
                            Vx[(opcode & 0x0F00) >> 8] = Vx[(opcode & 0x00F0) >> 4];
                            break;

                        case 0x0001:
                            Vx[(opcode & 0x0F00) >> 8] = (byte)(Vx[(opcode & 0x0F00) >> 8] | Vx[(opcode & 0x00F0) >> 4]);
                            break;

                        case 0x0002:
                            Vx[(opcode & 0x0F00) >> 8] = (byte)(Vx[(opcode & 0x0F00) >> 8] & Vx[(opcode & 0x00F0) >> 4]);
                            break;

                        case 0x0003:
                            Vx[(opcode & 0x0F00) >> 8] = (byte)(Vx[(opcode & 0x0F00) >> 8] ^ Vx[(opcode & 0x00F0) >> 4]);
                            break;

                        case 0x0004:
                            int i = (Vx[(opcode & 0x0F00) >> 8] + Vx[(opcode & 0x00F0) >> 4]);
                            Vx[0xF] = (i > 0xFF) ? (byte)0x01 : (byte)0x00;
                            Vx[(opcode & 0x0F00) >> 8] = (byte)(i & 0xFF);
                            break;

                        case 0x0005:
                            Vx[0xF] = (Vx[(opcode & 0x0F00) >> 8] >= Vx[(opcode & 0x00F0) >> 4]) ? (byte)0x01 : (byte)0x00;
                            Vx[(opcode & 0x0F00) >> 8] = (byte)(Vx[(opcode & 0x0F00) >> 8] - Vx[(opcode & 0x00F0) >> 4]);
                            break;

                        case 0x0006:
                            Vx[0xF] = (byte)(Vx[(opcode & 0x0F00) >> 8] & 0x01);
                            Vx[(opcode & 0x0F00) >> 8] >>= 1;
                            break;

                        case 0x0007:
                            Vx[0xF] = (Vx[(opcode & 0x00F0) >> 4] >= Vx[(opcode & 0x0F00) >> 8]) ? (byte)0x01 : (byte)0x00;
                            Vx[(opcode & 0x0F00) >> 8] = (byte)(Vx[(opcode & 0x00F0) >> 4] - Vx[(opcode & 0x0F00) >> 8]);
                            break;

                        case 0x000E:
                            Vx[0xF] = (byte)(Vx[(opcode & 0x0F00) >> 8] >> 7); //Shift 7 to get 8th Bit to Pos1
                            Vx[(opcode & 0x0F00) >> 8] <<= 1;
                            break;

                        default:
                            MessageBox.Show("Unknown Opcode "+opcode);
                            //running = false;
                            break;
                    }
                    PC += 2;
                    break;

                case 0x9000:
                    if (Vx[(opcode & 0x0F00) >> 8] != Vx[(opcode & 0x00F0) >> 4])
                        PC += 2;
                    PC += 2;
                    break;

                case 0xA000:
                    I = (ushort)(opcode & 0x0FFF);
                    PC += 2;
                    break;

                case 0xB000:
                    PC = (ushort)(Vx[0x0] + (opcode & 0x0FFF));
                    break;

                case 0xC000:
                    Vx[(opcode & 0x0F00) >> 8] = (byte)(randy.Next(0, 256) & (opcode & 0x00FF));
                    PC += 2;
                    break;

                case 0xD000:
                    Vx[0xF] = 0;
                    for (int row = 0; row < (opcode & 0x000F); row++)
                    {
                        for (int column = 0; column < 8; column++)
                        {
                            if ((RAM[I + row] & (0x80 >> column)) != 0)
                            {
                                if (display[(Vx[(opcode & 0x0F00) >> 8] + column +
                                    (Vx[(opcode & 0x00F0) >> 4]+row) * 64)%2048] == 1)
                                    Vx[0xF] = 1;

                                display[(Vx[(opcode & 0x0F00) >> 8] + column +
                                    (Vx[(opcode & 0x00F0) >> 4]+row) * 64)%2048] ^= 1;
                            }
                        }
                    }

                    drawFlag = true;
                    PC += 2;
                    break;

                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x009E:
                            if (inputFlags[Vx[(opcode & 0x0F00) >> 8]] == 0x01)
                                PC += 2;
                            break;
                        case 0x00A1:
                            if (inputFlags[Vx[(opcode & 0x0F00) >> 8]] == 0x00)
                                PC += 2;
                            break;
                        default:
                            MessageBox.Show("Unknown Opcode "+opcode);
                            //running = false;
                            break;
                    }

                    PC += 2;
                    break;

                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x0007:
                            Vx[(opcode & 0x0F00) >> 8] = DT;
                            break;

                        case 0x000A:
                            //TODO: Implement this opcode.
                            break;

                        case 0x0015:
                            DT = Vx[(opcode & 0x0F00) >> 8];
                            break;

                        case 0x0018:
                            ST = Vx[(opcode & 0x0F00) >> 8];
                            new Thread(new ParameterizedThreadStart(Beep)).Start(ST);
                            break;

                        case 0x001E:
                            Vx[0xF] = (I + Vx[(opcode & 0x0F00) >> 8] > 0xFFF) ? (byte)0x01 : (byte)0x00;
                            I = (ushort)(I + Vx[(opcode & 0x0F00) >> 8]);
                            break;

                        case 0x0029:
                            I = (ushort)((Vx[(opcode & 0x0F00) >> 8] & 0x0F) * 5);
                            break;

                        case 0x0033:
                            RAM[I] = (byte)(Vx[(opcode & 0x0F00) >> 8] / 100);
                            RAM[I + 1] = (byte)((Vx[(opcode & 0x0F00) >> 8] / 10) % 10);
                            RAM[I + 2] = (byte)(Vx[(opcode & 0x0F00) >> 8] % 10);
                            break;

                        case 0x0055:
                            for (byte i = 0; i <= ((opcode & 0x0F00) >> 8); i++)
                                RAM[I + i] = Vx[i];

                            I = (ushort)(I + ((opcode & 0x0F00) >> 8) + 1);
                            break;

                        case 0x0065:
                            for (byte i = 0; i <= ((opcode & 0x0F00) >> 8); i++)
                                Vx[i] = RAM[I + i];

                            I = (ushort)(I + ((opcode & 0x0F00) >> 8) + 1);
                            break;

                        default:
                            MessageBox.Show("Unknown Opcode "+opcode);
                            //running = false;
                            break;
                    }
                    PC += 2;
                    break;

                default:
                    MessageBox.Show("Unknown Opcode "+opcode);
                    //running = false;
                    break;

            }
            #endregion


            if (timeFlag)
            {
                if (DT > 0x00)
                    DT--;

                if (ST > 0x00)
                    ST--;

                timeFlag = false;
            }


            //for (int i = 0; i < 16; i++)
            //{
            //    Console.Write("V" + i + ": " + Vx[i] + ", ");
            //}
            //Console.WriteLine("I: " + I + ",  PC: " + PC);
            //MessageBox.Show(Vx[0].ToString() + "  VF: " + Vx[0xF].ToString()+"  I: "+I.ToString());
        }


        void Beep(object duration)
        {
            Console.Beep(1000, Convert.ToInt32(duration)*1000/60);
        }
    }
}
