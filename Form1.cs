﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
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
            panel2.Height = panel1.Height;
            panel3.Width = panel1.Width;
            panel3.Location = new Point(panel1.Location.X, panel1.Location.Y + panel1.Height);
            enj = new Engine();
            MakeBoard(flipped);
            CreateOptions();
            ulong[] BB = enj.Initialize(StartingPos);
            temporaryBitBoards = new ulong[12];
            PrintPic(new ulong[12]);
            PrintPic(BB);
            white = (Engine.Position & (1 << 4)) != 0;
            //PlayGame(enj);
            moveMade = false;
        }

        const int StartingPos = 3;
        const int MinimalMoveDelay = 1000; //ms
        const int AutoSaveFreq = 5;
        static readonly Color light = Color.LightGray;
        static readonly Color dark = Color.Brown;
        static readonly Color clicked_own = Color.LightGreen;
        static readonly Color clicked_opponent = Color.Red;
        static readonly Color allowed = Color.Green;
        static readonly Color last_move = Color.Yellow;
        ulong[] temporaryBitBoards;
        Engine enj;
        bool moveMade = true;
        bool flipped = false;
        public Button[] ButtonBoard = new Button[8 * 8];
        TextBox[] rankCoords = new TextBox[8];
        TextBox[] fileCoords = new TextBox[8];
        Button[] proms = new Button[4];
        Button aiw = new Button();
        Button aib = new Button();
        Button newg = new Button();
        TextBox Analys = new TextBox();

        public static string[] PieceImages = new string[]
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
        static Bitmap[] initImagesPieces()
        {
            Bitmap[] ip = new Bitmap[12];
            for (int i = 0; i < 12; ++i)
            {
                ip[i] = new Bitmap(Application.StartupPath + "\\Image\\" + PieceImages[i]);
            }
            return ip;
        }
        static readonly Bitmap[] ImagesPieces = initImagesPieces();/* new Bitmap[12]
        {
            new Bitmap(Application.StartupPath + "\\Image\\" + PieceImages[0]),
            new Bitmap(Application.StartupPath + "\\Image\\" + PieceImages[1]),
            new Bitmap(Application.StartupPath + "\\Image\\" + PieceImages[2]),
        };*/


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
                    new Point(panel1.Width / 9 + ((63 - i) & 0b111) * size, ((63 - i) >> 3) * size) :
                    new Point(panel1.Width / 9 + (i & 0b111) * size, (i >> 3) * size);


                //tag is according to index
                ButtonBoard[i].Tag = i;

                //text is determined by the normal notation rule
                ButtonBoard[i].Text = ((char)((i & 0b111) + 'a')).ToString() + ((8 - (i >> 3)).ToString());

                //click handler
                ButtonBoard[i].Click += Button_Click;
            }
            //coordinates - rank
            for (int i = 0; i < 8; ++i)
            {
                rankCoords[i] = new TextBox();
                var tx = rankCoords[i];
                panel1.Controls.Add(tx);

                tx.Font = new Font(tx.Font.FontFamily, panel1.Height / 16); //for some reason, /16 works pretty well 
                tx.Width = size / 2;
                tx.Text = (8 - i).ToString();
                tx.Location = flipped ?
                    new Point(panel1.Width / 9 - tx.Width,
                    (7 - i) * size + (size - tx.Height) / 2) : //fixes diff between button height and textbox height
                    new Point(panel1.Width / 9 - tx.Width,
                    i * size + (size - tx.Height) / 2);
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
                tx.Text = ((char)('a' + i)).ToString();
                tx.TextAlign = HorizontalAlignment.Center;
                tx.Location = flipped ?
                    new Point((8 - i) * size, panel1.Width - panel1.Width / 9) :
                    new Point((1 + i) * size, panel1.Width - panel1.Width / 9);
                tx.Enabled = false;
                tx.BringToFront();
            }
            panel2.Height = panel1.Height;
            panel2.Location = new Point(panel1.Location.X - panel2.Width, panel1.Location.Y);
            panel3.Width = panel1.Width * 8 / 9;
            panel3.Location = new Point(panel1.Location.X + panel1.Width / 9, panel1.Location.Y + panel1.Height);
            //endgame panel
            panel3.Controls.Add(KonecHry);
            KonecHry.Location = new Point(panel3.Location.X, panel3.Location.Y + panel3.Height - KonecHry.Height);
            KonecHry.Width = panel3.Width;
            KonecHry.Show();
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
            sav.Location = new Point(size * 0, size * 1);
            sav.Click += Save_Click;

            //new game
            panel2.Controls.Add(newg);
            newg.Height = 3 * size / 2;
            newg.Width = 3 * size / 2;
            newg.Text = "New game:\n" + (whitePlayer_AI ? "AI" + intelWhite.ToString() : "Player") + "\n vs \n" + (blackPlayer_AI ? "AI" + intelBlack.ToString() : "Player");
            newg.Location = new Point(size * 0, size * 4);
            newg.Click += NewGame_Click;

            //AI diff for white
            panel2.Controls.Add(aiw);
            aiw.Height = size / 2;
            aiw.Width = size / 2;
            aiw.Text = "AI" + intelWhite.ToString();
            aiw.Location = new Point(size * 0, size * 3);
            aiw.Tag = 0;
            aiw.Click += AISet_Click;

            //AI diff for black
            panel2.Controls.Add(aib);
            aib.Height = size / 2;
            aib.Width = size / 2;
            aib.Text = "AI" + intelBlack.ToString();
            aib.Location = new Point(size * 0, (int)(size * 3.5));
            aib.Tag = 1;
            aib.Click += AISet_Click;

            //AI increase
            var aiinc = new Button();
            panel2.Controls.Add(aiinc);
            aiinc.Height = size / 2;
            aiinc.Width = size / 2;
            aiinc.Text = "+";
            aiinc.Location = new Point((int)(size * 0.5), size * 3);
            aiinc.Click += AII_Click;

            var aidec = new Button();
            panel2.Controls.Add(aidec);
            aidec.Height = size / 2;
            aidec.Width = size / 2;
            aidec.Text = "-";
            aidec.Location = new Point((int)(size * 0.5), (int)(size * 3.5));
            aidec.Click += AID_Click;

            var plw = new Button();
            panel2.Controls.Add(plw);
            plw.Height = size / 2;
            plw.Width = 2 * size / 3;
            plw.Text = "Player";
            plw.Location = new Point(size * 1, (int)(size * 3));
            plw.Tag = 0;
            plw.Click += AsPlayer_Click;

            var plb = new Button();
            panel2.Controls.Add(plb);
            plb.Height = size / 2;
            plb.Width = 2 * size / 3;
            plb.Text = "Player";
            plb.Location = new Point(size * 1, (int)(size * 3.5));
            plb.Tag = 1;
            plb.Click += AsPlayer_Click;

            var hideanalys = new Button();
            panel2.Controls.Add(hideanalys);
            hideanalys.Height = size;
            hideanalys.Width = size;
            hideanalys.Text = "Hide move showing";
            hideanalys.Location = new Point(size * 0, (int)(size * 2));
            hideanalys.Tag = 1;
            hideanalys.Click += HideAna_Click;

            //promotions
            for (int i = 0; i < 4; ++i)
            {
                proms[i] = new Button();
                panel3.Controls.Add(proms[i]);
                proms[i].Height = size;
                proms[i].Width = size;
                proms[i].Location = new Point(size * (i), 0);
                proms[i].Hide();
                proms[i].Enabled = false;
                proms[i].Tag = i;
                proms[i].Click += Promot_Click;
            }

            //analysis box
            panel3.Controls.Add(Analys);
            Analys.Location = new Point(panel3.Width / 2, 0);
            Analys.Width = panel3.Width / 2;
            Analys.BringToFront();
            Analys.Multiline = true;
        }

        void ShowPromotion(int target, bool white)
        {
            ButtonBoard[target].BackColor = clicked_opponent;
            promot = -2;
            for (int i = 0; i < 4; ++i)
            {
                ImagePrint(proms[i], (white ? 5 : 11) - i);
                proms[i].Show();
                proms[i].Enabled = true;
            }
        }



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
        int targetSquare;
        int selectedPiece;
        int promot = -1;

        static int result = 0;


        bool whitePlayer_AI = false;
        bool blackPlayer_AI = true;

        int intelWhite = 4;
        int intelBlack = 4;
        int intelAI = 4;


        public int GoNextMove(Engine enj) //move made by computer
        {

            moveMade = false;
            int mov = enj.ComputersMove(white, gamelength, white ? intelWhite : intelBlack);
            string textforbox = enj.analysisEvaluation.ToString() + " ";
            for (int i = 0; i < enj.pvmoves.Length; ++i)
            {
                if (enj.pvmoves[i] == "Ka8-a8")
                    break;
                textforbox += enj.pvmoves[i] + " ";
            }
            Analys.Text = textforbox;
            //int x = enj.PlayersMove(white, gamelength, 0);
            notation = enj.Notation;
            gamelength += 1;
            white ^= true;
            bitbs = enj.GetBitBoards();
            PrintPic(bitbs);
            HighlightMove(mov);
            int y = enj.DetermineResult(white);
            moveMade = true;
            return y;
        }

        public int PlayNextMove(Engine enj, uint move) //move made by player
        {
            enj.PlayersMove(white, gamelength, move);
            notation = enj.Notation;
            gamelength += 1;
            white ^= true;
            bitbs = enj.GetBitBoards();
            PrintPic(bitbs);
            temporaryBitBoards = bitbs;
            result = enj.DetermineResult(white);
            if (result != 0)
            {
                Finish();
            }
            else if (enj.IsCheck_White() || enj.IsCheck_Black())
            {
                ShowCheck();
            }
            else
            {
                UnshowCheck();
            }
            moveMade = true;
            return result;
        }

        void HighlightMove(int move)
        { //only highlights the "from" and "to" squares
            int from = (move >> 16) & 255;
            int to = (move >> 8) & 255;
            if (from == to)
            { //castling - it is called on previous move, so white is the opposite...
                from = white ? 4 : 60;
                if (Engine.Bit(move, 5)) //qside
                {
                    to = from - 2;
                }
                else if (Engine.Bit(move, 6)) //kside
                {
                    to = from + 2;
                }
            }
            ButtonBoard[from].BackColor = last_move;
            ButtonBoard[to].BackColor = last_move;

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

            if (promot != -1)
            {
                return;
            }
            if (ButtonBoard[idx].BackColor == clicked_own)
            { //same piece - cancel the lighting
                //ButtonBoard[idx].BackColor = Engine.SqColor(idx) ? light:dark;
                for (int i = 0; i < 64; ++i)
                {
                    ButtonBoard[i].BackColor = Engine.SqColor(i) ? light : dark;
                }
                selectedPiece = -1;
                selectedSquare = -1;
            }
            else if (ButtonBoard[idx].BackColor == allowed)
            { //target square of current piece
                targetSquare = idx;
                if (selectedPiece == 1 && 0 <= idx && idx < 8) //promotion - make a choice for white
                {
                    ShowPromotion(idx, true);
                    return;
                }
                else if (selectedPiece == 7 && 58 <= idx && idx < 64) //promotion - make a choice for black
                {
                    ShowPromotion(idx, false);
                    return;
                }
                uint nextmove = enj.CompleteMove(selectedPiece, selectedSquare, idx, white);
                PlayNextMove(enj, nextmove);

                for (int i = 0; i < 64; ++i)
                {
                    ButtonBoard[i].BackColor = Engine.SqColor(i) ? light : dark;
                }

                ButtonBoard[selectedSquare].BackColor = last_move;
                ButtonBoard[idx].BackColor = last_move;
                selectedPiece = -1;
                selectedSquare = -1;
                return;
            }
            else
            { //new piece
                moveBitboard = enj.DisplayLegalMoves(idx, white);
                selectedSquare = idx;
                selectedPiece = enj.GetPiece(idx);
                ulong pointr = 1;
                for (int i = 0; i < 64; ++i)
                {
                    if ((pointr & moveBitboard) != 0) //if legal
                    {
                        ButtonBoard[i].BackColor = Color.Green;
                    }
                    else
                    { //set it as default
                        ButtonBoard[i].BackColor = Engine.SqColor(i) ? light : dark;
                    }
                    pointr <<= 1;
                }

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
            PrintPic(bitbs);*/
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
        void HideAna_Click(object sender, EventArgs e)
        {
            if (Analys.Visible)
            {
                Analys.Hide();
            }
            else
            {
                Analys.Show();
            }
            return;
        }
        void Save_Click(object sender, EventArgs e)
        {
            Engine.RewritePartia("partie.txt", SaveName.Text);
            return;
        }

        void Promot_Click(object sender, EventArgs e)
        {
            var b = (Button)sender;
            promot = (int)b.Tag;
            uint nextmove = enj.CompleteMove(selectedPiece, selectedSquare, targetSquare, white);
            nextmove |= ((uint)promot << 3);
            PlayNextMove(enj, nextmove);
            for (int i = 0; i < 64; ++i)
            {
                ButtonBoard[i].BackColor = Engine.SqColor(i) ? light : dark;
            }

            ButtonBoard[selectedSquare].BackColor = last_move;
            ButtonBoard[targetSquare].BackColor = last_move;
            selectedPiece = -1;
            selectedSquare = -1;
            targetSquare = -1;
            promot = -1;

            foreach (var p in proms)
            {
                p.Enabled = false;
                p.Hide();
            }
            return;
        }

        void AII_Click(object sender, EventArgs e)
        { //increases diff of AI
            intelAI += 1;
            if (intelAI > 9)
                intelAI = 1;
            aiw.Text = "AI" + intelAI.ToString();
            aib.Text = "AI" + intelAI.ToString();
            return;
        }

        void AID_Click(object sender, EventArgs e)
        { //decreases diff of AI
            intelAI -= 1;
            if (intelAI < 1)
                intelAI = 9;
            aiw.Text = "AI" + intelAI.ToString();
            aib.Text = "AI" + intelAI.ToString();
            return;
        }

        void AISet_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if ((int)b.Tag == 0)
            {
                whitePlayer_AI = true;
                intelWhite = intelAI;
            }
            else
            {
                blackPlayer_AI = true;
                intelBlack = intelAI;
            }
            newg.Text = "New game:\n" +
                (whitePlayer_AI ? "AI" + intelWhite.ToString() : "Player")
                + "\n vs \n" +
                (blackPlayer_AI ? "AI" + intelBlack.ToString() : "Player");
        }

        void AsPlayer_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if ((int)b.Tag == 0)
            {
                whitePlayer_AI = false;
            }
            else
            {
                blackPlayer_AI = false;
            }
            newg.Text = "New game:\n" +
                (whitePlayer_AI ? "AI" + intelWhite.ToString() : "Player")
                + "\n vs \n" +
                (blackPlayer_AI ? "AI" + intelBlack.ToString() : "Player");
        }

        void NewGame_Click(object sender, EventArgs e)
        {
            //just start the game, everything else is initialized
            timer1.Stop();
            enj = new Engine();
            ulong[] BB = enj.Initialize(StartingPos);
            temporaryBitBoards = new ulong[12];
            PrintPic(new ulong[12]);
            PrintPic(BB);
            white = (Engine.Position & (1 << 4)) != 0;
            //PlayGame(enj);
            result = 0;
            KonecHry.Hide();
            KonecHry.Text = "";
            foreach (var b in ButtonBoard)
            {
                b.Enabled = true;
            }
            timer1.Start();
            if ((white && whitePlayer_AI) || (white is false && blackPlayer_AI))
            {
                moveMade = true;
            }
            else
            {
                moveMade = false;
            }

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

            Bitmap originImage = ImagesPieces[piece]; //new Bitmap(Application.StartupPath + "\\Image\\" + PieceImages[piece]);
            originImage.MakeTransparent(Color.FromArgb(247, 247, 247));
            Image image = resizeImage(originImage, new Size(height, width));

            b.Image = image;


            /*b.ImageAlign = ContentAlignment.MiddleRight;
            b.TextAlign = ContentAlignment.MiddleLeft;
            // Give the button a flat appearance.
            b.FlatStyle = FlatStyle.Flat;*/
        }


        public void PrintPic(ulong[] bitboards)
        { //same as the Printout in engine function, but changes the names of buttons
            int n = -1;
            foreach (Button btn in ButtonBoard) //hope they are ordered in the same way
            {
                ++n;
                char piece = ' ';
                if (Engine.SqColor(n))
                {
                    ButtonBoard[n].BackColor = light;
                }
                else
                {
                    ButtonBoard[n].BackColor = dark;
                }

                //if occupied, write something here...
                for (int k = 0; k < bitboards.Length; ++k)
                {
                    ButtonBoard[n].Text = "";
                    if (Engine.Bit(bitboards[k], n))
                    {
                        piece = Engine.pieces[k];
                        //ButtonBoard[n].Text = piece.ToString();
                        //ButtonBoard[n].BackColor = PieceColors[k];
                        ImagePrint(ButtonBoard[n], k);
                        //ButtonBoard[n].BringToFront();
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
                        ButtonBoard[n].Image = null;
                    }
                    else
                    {
                        //ButtonBoard[n].Paint += new System.Windows.Forms.PaintEventHandler(Light_Paint);
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
            //if the last time ran is less than x seconds ago, skip the tick.
            if (moveMade is false || lastTimeRan > (System.DateTime.Now.Ticks - MinimalMoveDelay * 10000) || !continuing)
            {
                return;
            }
            if (white && whitePlayer_AI || (white is false && blackPlayer_AI))
            {
                if((gamelength+1) % (AutoSaveFreq*2) == 0)
                    Engine.RewritePartia("partie.txt", SaveName.Text);
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
                Engine.RewritePartia("partie.txt", SaveName.Text);
                Finish();
            }
        }

        private void ShowCheck()
        {
            timer1.Stop();
            KonecHry.Text = "Check!";
            Controls.Add(KonecHry);
            KonecHry.Show();
            KonecHry.BringToFront();
            timer1.Start();
        }
        private void UnshowCheck()
        {
            KonecHry.Text = "";
            Controls.Add(KonecHry);
            KonecHry.Hide();
        }
        private void Finish()
        {
            timer1.Stop();
            KonecHry.Text = "Konec hry " + results[result];
            Controls.Add(KonecHry);
            KonecHry.Show();
            KonecHry.BringToFront();
            foreach(var b in ButtonBoard)
            {
                b.Enabled = false;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
