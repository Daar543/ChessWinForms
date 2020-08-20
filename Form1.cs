using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sachy_Obrazky
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            enj = new Engine();
            ulong[] BB = enj.Initialize(3);
            InitializeComponent();
            MakeBoard();
            PrintPic(BB);
            PlayGame(enj);
            
        }
        Engine enj;
        bool moveMade = true;
        public Button[] ButtonBoard = new Button[8 * 8];
        private void MakeBoard()
        {
            //creates buttons
            int size = panel1.Width / 8;
            panel1.Height = panel1.Width + 100;

            //print buttons
            for (int i = 0; i < 64; i++)
            {
                ButtonBoard[i] = new Button();
                ButtonBoard[i].Height = size;
                ButtonBoard[i].Width = size;

                panel1.Controls.Add(ButtonBoard[i]);

                //sets location 
                ButtonBoard[i].Location = new Point((i & 0b111) * size, (i >> 3) * size);

                //tag is according to index
                ButtonBoard[i].Tag = i;

                //text is determined by the normal notation rule
                ButtonBoard[i].Text = ((char)((i&0b111) + 'a')).ToString() + ((8-(i >> 3)).ToString());
            }

        }
        static readonly Color[] PieceColors = new Color[]
        {
            Color.White, //king
            Color.Yellow,
            Color.LightGreen,
            Color.LightBlue,
            Color.Orange, //rook
            Color.Pink,

            Color.Black,
            Color.Magenta, //pawn
            Color.DarkGreen,
            Color.DarkBlue,
            Color.DarkRed,
            Color.Purple, //queen
        };


        string notation = "";
        int gamelength = 0;
        bool white = true;
        ulong[] bitbs;

        public void PlayGame(Engine enj)
        {
            moveMade = false;
            notation = enj.OneMover(white, (uint)gamelength,gamelength, 5, notation);
            gamelength += 1;
            white ^= true;
            bitbs = enj.GetBitBoards();
            PrintPic(bitbs);
            moveMade = true;
        }
        public void PrintPic(ulong[] bitboards)
        { //same as the Printout in engine function, but changes the names of buttons
            int n = -1;
            foreach(Button btn in ButtonBoard) //hope they are ordered in the same way
            {
                ++n;
                char piece = ' ';
                //if occupied, write something here...
                for (int k = 0; k < bitboards.Length; k++)
                {
                    ButtonBoard[n].Text = "";
                    if (Engine.Bit(bitboards[k], n))
                    {
                        piece = Engine.pieces[k];
                        //ButtonBoard[n].Text = piece.ToString();
                        ButtonBoard[n].BackColor = PieceColors[k];
                        break;
                    }
                    
                    ButtonBoard[n].BackColor = SystemColors.Control;
                }
            }
            /*
            for (int i = 0; i < 8; i++) //i is for rows, j is for columns
            {
                for (int j = 0; j < 8; j++)
                {
                    char piece = ' ';
                    //if occupied, write something here...
                    for (int k = 0; k < bitboards.Length; k++)
                    {
                        if (Engine.Bit(bitboards[k], 8 * i + j))
                        {
                            piece = Engine.pieces[k];

                            break;
                        }
                    }
                }
            }
            */
        }

        private long lastTimeRan = System.DateTime.Now.Ticks;

        private void timer1_Tick(object sender, EventArgs e) //tick time = 10
        {
            //if the last time ran is less than 3 seconds ago, skip the timer tick.
            if (moveMade is false || lastTimeRan > (System.DateTime.Now.Ticks - 20_000_000))
            {
                return;
            }
            lastTimeRan = System.DateTime.Now.Ticks;
            PlayGame(enj);
        }
    }
}
