using System;
using System.CodeDom.Compiler;
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
            InitializeComponent();
            panel1.Height = panel1.Width;
            panel2.Height = panel1.Width;
            enj = new Engine();
            MakeBoard(flipped);
            CreateOptions();
            ulong[] BB = enj.Initialize(3);
            temporaryBitBoards = new ulong[12];
            PrintPic(new ulong[12], false);
            PrintPic(BB, false);
            white = ( Engine.Position & (1 << 4)) != 0;
            //PlayGame(enj);
            moveMade = false;
        }


        static readonly Color light = Color.LightGray;
        static readonly Color dark = Color.Brown;
        static readonly Color clicked_own = Color.LightGreen;
        static readonly Color clicked_opponent = Color.Red;
        static readonly Color allowed = Color.Green;
        ulong[] temporaryBitBoards;
        Engine enj;
        bool moveMade = true;
        bool flipped = false;
        public Button[] ButtonBoard = new Button[8 * 8];
        TextBox[] rankCoords = new TextBox[8];
        TextBox[] fileCoords = new TextBox[8];

        
        private void MakeBoard(bool flipped)
        {
            //creates buttons
            int size = panel1.Width / 9;

            //print buttons
            for (int i = 0; i < 64; i++)
            {
                ButtonBoard[i] = new Button();
                ButtonBoard[i].Height = size;
                ButtonBoard[i].Width = size;
                ButtonBoard[i].BackColor = Engine.SqColor(i) ? dark : light;
                panel1.Controls.Add(ButtonBoard[i]);

                //sets location 
                ButtonBoard[i].Location = flipped ?
                    new Point(panel1.Width / 9 + ((63-i) & 0b111) * size, ((63-i) >> 3) * size) :
                    new Point(panel1.Width / 9 + (i & 0b111) * size, (i >> 3) * size);
                    

                //tag is according to index
                ButtonBoard[i].Tag = i;

                //text is determined by the normal notation rule
                ButtonBoard[i].Text = ((char)((i&0b111) + 'a')).ToString() + ((8-(i >> 3)).ToString());

                //click handler
                ButtonBoard[i].Click += Button_Click;
            }
            //coordinates - rank
            for (int i = 0; i < 8; ++i)
            {
                rankCoords[i] = new TextBox();
                var tx = rankCoords[i];
                panel1.Controls.Add(tx);

                tx.Font = new Font(tx.Font.FontFamily, panel1.Height/16); //for some reason, /16 works pretty well 
                tx.Width = size/2;
                tx.Text = (8 - i).ToString();
                tx.Location = flipped ?
                    new Point(panel1.Width / 9 - tx.Width, 
                    (7-i) * size + (size - tx.Height) / 2) : //fixes diff between button height and textbox height
                    new Point(panel1.Width / 9 - tx.Width, 
                    i * size + ( size - tx.Height) /2 );
                tx.Enabled = false;
                tx.BringToFront();
            }
            //coordinates - file (almost the same)
            for (int i = 0; i < 8; ++i)
            {
                fileCoords[i] = new TextBox();
                var tx = fileCoords[i];
                panel1.Controls.Add(tx);

                tx.Font = new Font(tx.Font.FontFamily, panel1.Height / 16);
                tx.Width = size; //in order to fill the whole button-space
                tx.Text = ((char)('a'+i)).ToString();
                tx.TextAlign = HorizontalAlignment.Center;
                tx.Location = flipped ?
                    new Point((8-i) * size, panel1.Width - panel1.Width / 9) :
                    new Point((1+i) * size, panel1.Width - panel1.Width / 9) ;
                tx.Enabled = false;
                tx.BringToFront();
            }
            /*var refbt = new Button();
            panel1.Controls.Add(refbt);
            refbt.Height = size;
            refbt.Width = size;
            refbt.BackColor = Color.Yellow;
            refbt.Location = new Point(0, 0);*/
        }

        private void CreateOptions()
        {
            int size = panel2.Width / 2;
            //board flipping
            var flp = new Button();
            panel2.Controls.Add(flp);
            flp.Height = size;
            flp.Width = size;
            flp.Text = "Flip \n" + (flipped ? "Black" : "White");
            flp.Location = new Point(size * 0, size * 0);
            flp.Click += Flip_Click;

            //saving notation
            var sav = new Button();
            panel2.Controls.Add(sav);
            sav.Height = size;
            sav.Width = size;
            sav.Text = "Save notation";
            sav.Location = new Point(size * 0, size * 3);
            sav.Click += Save_Click;
        }

        /*protected override void OnPaintBackground(PaintEventArgs e)
        {
            //empty implementation
        }*/

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
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
        bool white;
        ulong[] bitbs = new ulong[12];
        int selectedSquare;
        int selectedPiece;
        static int result = 0;







        public int GoNextMove(Engine enj)
        {
            moveMade = false;
            int x = enj.ComputersMove(white,gamelength, 5);
            //int x = enj.PlayersMove(white, gamelength, 0);
            notation = enj.Notation;
            gamelength += 1;
            white ^= true;
            bitbs = enj.GetBitBoards();
            PrintPic(bitbs,true);
            temporaryBitBoards = bitbs;
            moveMade = true;
            return x;
        }

        public int PlayNextMove(Engine enj, uint move)
        {
            int x = enj.PlayersMove(white, gamelength, move);
            notation = enj.Notation;
            gamelength += 1;
            white ^= true;
            bitbs = enj.GetBitBoards();
            PrintPic(bitbs, true);
            temporaryBitBoards = bitbs;
            moveMade = true;
            return x;
        }

        void Button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            // use Name or Tag of button
            PieceClicked((int)button.Tag);
            return;
        }
        public void PieceClicked(int idx)
        { //when a piece is clicked, it displays green squares where the piece can move

            //if the same piece h.b. already clicked, return the buttons back to normal;
            ulong moveBitboard;

            if (ButtonBoard[idx].BackColor == clicked_own)
            { //same piece - cancel the lighting
                moveBitboard = 0;
                //ButtonBoard[idx].BackColor = Engine.SqColor(idx) ? light:dark;
                selectedPiece = -1;
                selectedSquare = -1;
            }
            else if (ButtonBoard[idx].BackColor == allowed)
            { //target square of current piece
                moveBitboard = (ulong)1 << selectedSquare; //current square will be undercolored
                uint nextmove = enj.CompleteMove(selectedPiece, selectedSquare, idx, white);
                PlayNextMove(enj, nextmove);
                selectedPiece = -1;
                selectedSquare = -1;
            }
            else
            { //new piece
                moveBitboard = enj.DisplayLegalMoves(idx, white);
                selectedSquare = idx;
                selectedPiece = enj.GetPiece(idx);
            }
            ulong pointr = 1;
            for(int i = 0; i < 64; ++i)
            {
                if((pointr & moveBitboard) != 0) //if legal
                {
                    ButtonBoard[i].BackColor = Color.Green;
                }
                else
                { //set it as default
                    ButtonBoard[i].BackColor = Engine.SqColor(i) ? light:dark;
                }
                pointr <<= 1;
            }
        }

        void Flip_Click(object sender, EventArgs e)
        {
            //flips board
            flipped ^= true;
            Button b = (Button)(sender);
            b.Text = "Flip \n" + (flipped ? "Black" : "White");
            //by reorganizing buttons
            /*MakeBoard(flipped);
            bitbs = enj.GetBitBoards();
            PrintPic(bitbs, false);*/
            int size = panel1.Width / 9;
            if (flipped)
            {
                for (int i = 0; i < 64; ++i)
                {
                    ButtonBoard[i].Location =
                        new Point(panel1.Width / 9 + ((63 - i) & 0b111) * size, ((63 - i) >> 3) * size);
                }
                for (int i = 0; i < 8; ++i)
                {
                    rankCoords[i].Location =
                        new Point(panel1.Width / 9 - rankCoords[i].Width,
                        (7 - i) * size + (size - rankCoords[i].Height) / 2); //fixes diff between button height and textbox height
                    fileCoords[i].Location =
                        new Point((8 - i) * size, size * 8);
                }
            }
            
            else
            {
                for (int i = 0; i < 64; ++i)
                {
                    ButtonBoard[i].Location = 
                        new Point(panel1.Width / 9 + (i & 0b111) * size, (i >> 3) * size);
                }
                for (int i = 0; i < 8; ++i)
                {
                    rankCoords[i].Location = 
                        new Point(panel1.Width / 9 - rankCoords[i].Width,
                        i * size + (size - rankCoords[i].Height) / 2);
                    fileCoords[i].Location = 
                        new Point((1 + i) * size, size * 8);
                }
            }
            return;
        }

        void Save_Click(object sender, EventArgs e)
        {
            Engine.RewritePartia("partie.txt");
            return;
        }
        /*protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }*/
        public void ImagePrint(Button b, int piece)
        {
            const double ScaleRatio = 0.8;
            int width = (int)(b.Width * ScaleRatio);
            int height = (int)(b.Height * ScaleRatio);

            Bitmap originImage = new Bitmap(Application.StartupPath + "\\Image\\" + PieceImages[piece]);
            originImage.MakeTransparent(Color.FromArgb(247,247,247));
            Image image = resizeImage(originImage, new Size(height, width));
            
            b.Image = image;


            /*b.ImageAlign = ContentAlignment.MiddleRight;
            b.TextAlign = ContentAlignment.MiddleLeft;
            // Give the button a flat appearance.
            b.FlatStyle = FlatStyle.Flat;*/
        }
        public string[] PieceImages = new string[]
        {
            "Wking_light.png",
            "Wpawn_light.png",
            "Wknight_light.png",
            "Wbishop_light.png",
            "Wrook_light.png",
            "Wqueen_light.png",
            "Bking_light.png",
            "Bpawn_light.png",
            "Bknight_light.png",
            "Bbishop_light.png",
            "Brook_light.png",
            "Bqueen_light.png",
        };
        
        public void PrintPic(ulong[] bitboards, bool onlyChanged)
        { //same as the Printout in engine function, but changes the names of buttons
            int n = -1;
            foreach(Button btn in ButtonBoard) //hope they are ordered in the same way
            {
                ++n;
                char piece = ' ';
                //if occupied, write something here...
                for ( int k = 0; k < bitboards.Length; ++k)
                {
                    /*if(onlyChanged && (temporaryBitBoards[k] == bitbs[k]))
                    { //no need to rewrite what has not changed
                        piece = Engine.pieces[k];
                        continue; 
                    }*/
                    ButtonBoard[n].Text = "";
                    if (Engine.Bit(bitboards[k], n))
                    {
                        piece = Engine.pieces[k];
                        //ButtonBoard[n].Text = piece.ToString();
                        //ButtonBoard[n].BackColor = PieceColors[k];
                        ImagePrint(ButtonBoard[n], k);
                        //ButtonBoard[n].BringToFront();
                        /*switch (k)
                        {
                            
                            /*case 0:
                                ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Wking_Paint);
                                void Wking_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                {
                                    PrintPieceImage(ButtonBoard[n], "Wking_light.png", e);
                                }
                                break;
                            case 1:
                                ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Wpawn_Paint);
                                void Wpawn_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                {
                                    PrintPieceImage(ButtonBoard[n], "Wpawn_light.png", e);
                                }
                                break;
                            case 2:
                                /*ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Wknight_Paint);
                                void Wknight_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                {
                                    PrintPieceImage(ButtonBoard[n], "Wknight_light.png", e);
                                }
                                break;
                                //Button b = ButtonBoard[n];
                                ImagePrint(ButtonBoard[n], 2);
                                break;
                                /*case 3:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Wbishop_Paint);
                                    void Wbishop_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Wbishop_light.png", e);
                                    }
                                    break;
                                case 4:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Wrook_Paint);
                                    void Wrook_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Wrook_light.png", e);
                                    }
                                    break;
                                case 5:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Wqueen_Paint);
                                    void Wqueen_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Wqueen_light.png", e);
                                    }
                                    break;
                                case 6:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Bking_Paint);
                                    void Bking_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Bking_light.png", e);
                                    }
                                    break;
                                case 7:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Bpawn_Paint);
                                    void Bpawn_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Bpawn_light.png", e);
                                    }
                                    break;
                                case 8:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Bknight_Paint);
                                    void Bknight_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Bknight_light.png", e);
                                    }
                                    break;
                                case 9:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Bbishop_Paint);
                                    void Bbishop_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Bbishop_light.png", e);
                                    }
                                    break;
                                case 10:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Brook_Paint);
                                    void Brook_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Brook_light.png", e);
                                    }
                                    break;
                                case 11:
                                    ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Bqueen_Paint);
                                    void Bqueen_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                                    {
                                        PrintPieceImage(ButtonBoard[n], "Bqueen_light.png", e);
                                    }
                                    break;
                        }*/
                        break;
                    }
                    
                    //ButtonBoard[n].BackColor = SystemColors.Control;
                }
                if (piece == ' ')
                {
                    //if there is no piece
                    if (Engine.SqColor(n)) //if light
                    {
                        //ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Dark_Paint);
                        ButtonBoard[n].BackColor = light;
                        ButtonBoard[n].Image = null;
                    }
                    else
                    {
                        //ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Light_Paint);
                        ButtonBoard[n].BackColor = dark;
                        ButtonBoard[n].Image = null;
                    }
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
        static readonly string[] results =
        {
            "*",
            "1/2 - 1/2",
            "0 - 1",
            "1 - 0",

        };
        bool continuing = true;
        private long lastTimeRan = System.DateTime.Now.Ticks;

        private void timer1_Tick(object sender, EventArgs e) //tick time = 10
        {
            //if the last time ran is less than 3 seconds ago, skip the timer tick.
            if (moveMade is false || lastTimeRan > (System.DateTime.Now.Ticks - 10_000_000)|| !continuing)
            {
                return;
            }
            if (!white)
            {
                result = GoNextMove(enj);
            }
            else
            {
                return;
            }
            lastTimeRan = System.DateTime.Now.Ticks;
            
            
            //continuing = PlayGame(enj) == 0;
            if (result != 0)
            {
                Finish();
            }
        }
        private void Finish()
        {
            timer1.Stop();
            KonecHry.Text = "Konec hry " + results[result];
            Controls.Add(KonecHry);
            KonecHry.Show();
            KonecHry.BringToFront();
        }
        private void timer2_Tick(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
