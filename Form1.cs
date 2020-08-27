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
            temporaryBitBoards = new ulong[12];
            PrintPic(BB,false);
            PlayGame(enj);
            
        }
        ulong[] temporaryBitBoards;
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
                ButtonBoard[i].BackColor = Engine.SqColor(i) ? Color.White : Color.Brown;
                panel1.Controls.Add(ButtonBoard[i]);

                //sets location 
                ButtonBoard[i].Location = new Point((i & 0b111) * size, (i >> 3) * size);

                //tag is according to index
                ButtonBoard[i].Tag = i;

                //text is determined by the normal notation rule
                ButtonBoard[i].Text = ((char)((i&0b111) + 'a')).ToString() + ((8-(i >> 3)).ToString());
            }

        }

        /*protected override void OnPaintBackground(PaintEventArgs e)
        {
            //empty implementation
        }*/

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        
        private void PrintPieceImage(Button b, string imgName, PaintEventArgs e)
        { //prints an image of a piece on given button
          // Assign an image to the button.

            const double ScaleRatio = 0.8;
            int width = (int) (b.Width* ScaleRatio);
            int height = (int) (b.Height * ScaleRatio);

            Image originImage = new Bitmap(Application.StartupPath + "\\Image\\" + imgName);
            Image image = resizeImage(originImage, new Size(height, width));

            // Make the destination rectangle 30 percent wider and
            // 30 percent taller than the original image.
            // Put the upper-left corner of the destination
            // rectangle at (150, 20).
            
            RectangleF destinationRect = new RectangleF(
                (b.Width - width) / 2,
                (b.Height - height) / 2,
                1 * width,
                1 * height);

            // Draw a portion of the image. Scale that portion of the image
            // so that it fills the destination rectangle.
            RectangleF sourceRect = new RectangleF(0, 0, 1 * width, 1 * height);
            e.Graphics.DrawImage(
                image,
                destinationRect,
                sourceRect,
                GraphicsUnit.Pixel);

            /*b.Image = Image.FromFile(Application.StartupPath + "\\Image\\"+imgName);
            // Align the image and text on the button.
            b.ImageAlign = ContentAlignment.MiddleRight;
            b.TextAlign = ContentAlignment.MiddleLeft;
            // Give the button a flat appearance.
            b.FlatStyle = FlatStyle.Flat;*/
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
        ulong[] bitbs = new ulong[12];
        

        public void PlayGame(Engine enj)
        {
            moveMade = false;
            notation = enj.OneMover(white, (uint)gamelength,gamelength, 4, notation);
            gamelength += 1;
            white ^= true;
            bitbs = enj.GetBitBoards();
            PrintPic(bitbs,true);
            temporaryBitBoards = bitbs;
            moveMade = true;
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
                    if (!Engine.SqColor(n)) //if dark
                    {
                        //ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Dark_Paint);
                        ButtonBoard[n].BackColor = Color.Brown;
                        ButtonBoard[n].Image = null;
                    }
                    else
                    {
                        //ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Light_Paint);
                        ButtonBoard[n].BackColor = Color.White;
                        ButtonBoard[n].Image = null;
                    }
                    void Dark_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                    {
                            PrintPieceImage(ButtonBoard[n], "Empty_dark.png", e);
                    }
                    void Light_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                    {
                        PrintPieceImage(ButtonBoard[n], "Empty_light.png", e);
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

        private long lastTimeRan = System.DateTime.Now.Ticks;

        private void timer1_Tick(object sender, EventArgs e) //tick time = 10
        {
            //if the last time ran is less than 3 seconds ago, skip the timer tick.
            if (moveMade is false || lastTimeRan > (System.DateTime.Now.Ticks - 10_000_000))
            {
                return;
            }
            lastTimeRan = System.DateTime.Now.Ticks;
            PlayGame(enj);
        }
    }
}
