using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Resource = Шахматы.Properties.Resources;
using Bitmap = System.Drawing.Bitmap;
using System.IO;
using System.Media;
using System;
using System.Linq;

namespace Шахматы
{

    public class MoveList
    {
        public int oldX = -1;
        public int oldY = -1;

        public List<int> X = new List<int>();
        public List<int> Y = new List<int>();
        public figure Figure = null;
    }

    public class StrokeData
    {
        public bool CustomDeath = false;
        public object oldOther = null;

        public int DeathX = -1;
        public int DeathY = -1;

        public int oldX = -1;
        public int oldY = -1;

        public int X = -1;
        public int Y = -1;
        public figure Figure = null;
        public figure FigureDeath = null;
    }
    public class DethGrid
    {
        Grid grid = null;
        public DethGrid(Grid gr)
        {
            for (int i = 0; i < 2; i++)
            {
                grid = gr;
                GR[i] = new Grid();
                gr.Children.Add(GR[i]);
                Grid.SetRow(GR[i], 4 + i);
                GR[i].HorizontalAlignment = HorizontalAlignment.Left;
                GR[i].VerticalAlignment = VerticalAlignment.Top;

                for (int j = 0; j < 2; j++)
                {
                    RowDefinition RW = new RowDefinition();
                    RW.Height = new GridLength(20.0);
                    GR[i].RowDefinitions.Add(RW);
                }
                for (int j = 0; j < 5; j++)
                {
                    ColumnDefinition CD = new ColumnDefinition();
                    CD.Width = new GridLength(20.0);
                    GR[i].ColumnDefinitions.Add(CD);
                }

                pawnGR[i] = new Grid();
                pawnGR[i].HorizontalAlignment = HorizontalAlignment.Left;
                pawnGR[i].VerticalAlignment = VerticalAlignment.Top;

                GR[i].Children.Add(pawnGR[i]);
                Grid.SetColumn(pawnGR[i], 3);
                Grid.SetColumnSpan(pawnGR[i], 2);
                Grid.SetRow(pawnGR[i], 2);

                for (int j = 0; j < 4; j++)
                {
                    RowDefinition RW = new RowDefinition();
                    RW.Height = new GridLength(10.0);
                    pawnGR[i].RowDefinitions.Add(RW);
                    ColumnDefinition CD = new ColumnDefinition();
                    CD.Width = new GridLength(10.0);
                    pawnGR[i].ColumnDefinitions.Add(CD);
                }
            }
        }

        public void Add(figure fig)
        {
            int clrnum = 0;
            if (!fig.color) clrnum = 1;
            Type t = null;
            if(fig.other != null) t = fig.other.GetType();
            if (fig.type == "pawn" || (t != null && t.Equals(typeof(string)) && (string)fig.other == "pawn"))
            {
                fig.Height = 10;
                fig.Width = 10;
                pawnGR[clrnum].Children.Add(fig);
                Grid.SetRow(fig, pawnY[clrnum]);
                Grid.SetColumn(fig, pawnX[clrnum]);
                if (pawnY[clrnum] == 1) pawnY[clrnum] = 0;
                else { pawnY[clrnum] = 1; pawnX[clrnum]--; }
            }
            else if (fig.type == "queen")
            {

                fig.Height = 20;
                fig.Width = 20;
                GR[clrnum].Children.Add(fig); Grid.SetColumn(fig, 4); Grid.SetRow(fig, 0);
            }
            else
            {
                fig.Height = 20;
                fig.Width = 20;
                for (int i = 0; i < 3; i++)
                {
                    if (types[clrnum, i] == null) { types[clrnum, i] = fig.type; GR[clrnum].Children.Add(fig); Grid.SetColumn(fig, i); Grid.SetRow(fig, 0); break; }
                    if (types[clrnum, i] == fig.type) { GR[clrnum].Children.Add(fig); Grid.SetColumn(fig, i); Grid.SetRow(fig, 1); break; }
                }
            }
        }

        public void Remove(figure fig)
        {
            int clrnum = 0;
            if (!fig.color) clrnum = 1;
            fig.Height = 80;
            fig.Width = 80;
            if (fig.type == "pawn" || (string)fig.other == "pawn")
            {
                if (pawnY[clrnum] == 0) pawnY[clrnum] = 1;
                else { pawnY[clrnum] = 0; pawnX[clrnum]++; }
                pawnGR[clrnum].Children.Remove(fig);
            }
            else if (fig.type == "queen") { GR[clrnum].Children.Remove(fig); }
            else { if (Grid.GetRow(fig) == 0) types[clrnum, Grid.GetColumn(fig)] = null; GR[clrnum].Children.Remove(fig); }

        }
        int[] pawnX = { 3, 3 };
        int[] pawnY = { 1, 1 };
        string[,] types = new string[2, 3];

        Grid[] GR = new Grid[2];
        Grid[] pawnGR = new Grid[2];
    }



    public class figure : Image
    {
        GameLogic gl = null;
        public bool dead = false;
        public string type = null;
        public bool color = false;
        public object other = null;

        public figure(BitmapImage img, string typ, bool clr)
        {
            color = clr;
            type = typ;
            gl = GameLogic.gl;
            gl.figLiveList.Add(this);
            MouseDown += Img_MouseDown;
            Height = 80;
            Width = 80;
            Source = img;
            AllowDrop = true;
            Drop += Figure_Drop;
            if (typ == "king" || typ == "castle") other = false;
        }

        private void Figure_Drop(object sender, DragEventArgs e)
        {
            if (Grid.GetColumn((UIElement)sender) == Grid.GetColumn((figure)e.Data.GetData("Шахматы.figure")) && Grid.GetRow((UIElement)sender) == Grid.GetRow((figure)e.Data.GetData("Шахматы.figure"))) return;
            gl.Drop(sender, e);
        }

        public void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((GameLogic.blockMode == 1 && ((figure)sender).color) || (GameLogic.blockMode == 2 && !((figure)sender).color))
            {
                figure fig = (figure)sender;
                DragDrop.DoDragDrop(fig, fig, DragDropEffects.Move);
            }
        }
    }

    public class chess
    {

        public class white
        {
            public class imgs
            {
                static public BitmapImage pawn = BitmapToImageSource(Resource.БПешка);
                static public BitmapImage bishop = BitmapToImageSource(Resource.БСлон);
                static public BitmapImage knight = BitmapToImageSource(Resource.БКонь);
                static public BitmapImage castle = BitmapToImageSource(Resource.БЛадья);
                static public BitmapImage queen = BitmapToImageSource(Resource.БФерзь);
                static public BitmapImage king = BitmapToImageSource(Resource.БКороль);
            }
            static public figure[] pawnPrep()
            {
                figure[] pawn = new figure[8];
                for (int i = 0; i < 8; i++)
                {
                    pawn[i] = new figure(imgs.pawn, "pawn", true);
                }
                return pawn;
            }
            static public figure[] pawn = pawnPrep();
            static public figure[] bishop = { new figure(imgs.bishop, "bishop", true), new figure(imgs.bishop, "bishop", true) };
            static public figure[] knight = { new figure(imgs.knight, "knight", true), new figure(imgs.knight, "knight", true) };
            static public figure[] castle = { new figure(imgs.castle, "castle", true), new figure(imgs.castle, "castle", true) };
            static public figure queen = new figure(imgs.queen, "queen", true);
            static public figure king = new figure(imgs.king, "king", true);
        }
        public class black
        {
            public class imgs
            {
                static public BitmapImage pawn = BitmapToImageSource(Resource.ЧПешка);
                static public BitmapImage bishop = BitmapToImageSource(Resource.ЧСлон);
                static public BitmapImage knight = BitmapToImageSource(Resource.ЧКонь);
                static public BitmapImage castle = BitmapToImageSource(Resource.ЧЛадья);
                static public BitmapImage queen = BitmapToImageSource(Resource.ЧФерзь);
                static public BitmapImage king = BitmapToImageSource(Resource.ЧКороль);
            }
            static public figure[] pawnPrep()
            {
                figure[] pawn = new figure[8];
                for (int i = 0; i < 8; i++)
                {
                    pawn[i] = new figure(imgs.pawn, "pawn", false);
                }
                return pawn;
            }
            static public figure[] pawn = pawnPrep();
            static public figure[] bishop = { new figure(imgs.bishop, "bishop", false), new figure(imgs.bishop, "bishop", false) };
            static public figure[] knight = { new figure(imgs.knight, "knight", false), new figure(imgs.knight, "knight", false) };
            static public figure[] castle = { new figure(imgs.castle, "castle", false), new figure(imgs.castle, "castle", false) };
            static public figure queen = new figure(imgs.queen, "queen", false);
            static public figure king = new figure(imgs.king, "king", false);
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static int[] ArrayInv = { -1, 8, 7, 6, 5, 4, 3, 2, 1};
        public bool inverted = false;

        Grid gr = null;
        public chess(Grid grid)
        {
            gr = grid;
            for (int i = 0; i < 8; i++)
            {
                setPos(black.pawn[i], new int[] { i + 1, 2 },  gr, false);
                setPos(white.pawn[i], new int[] { i + 1, 7 },  gr, false);
            }

            setPos(black.castle[0], new int[] { 1, 1 },  gr, false);
            setPos(black.castle[1], new int[] { 8, 1 },  gr, false);
            setPos(black.knight[0], new int[] { 2, 1 },  gr, false);
            setPos(black.knight[1], new int[] { 7, 1 },  gr, false);
            setPos(black.bishop[0], new int[] { 3, 1 },  gr, false);
            setPos(black.bishop[1], new int[] { 6, 1 },  gr, false);
            setPos(black.king, new int[] { 5, 1 },  gr, false);
            setPos(black.queen, new int[] { 4, 1 },  gr, false);

            setPos(white.castle[0], new int[] { 1, 8 }, gr, false);
            setPos(white.castle[1], new int[] { 8, 8 }, gr, false);
            setPos(white.knight[0], new int[] { 2, 8 }, gr, false);
            setPos(white.knight[1], new int[] { 7, 8 }, gr, false);
            setPos(white.bishop[0], new int[] { 3, 8 }, gr, false);
            setPos(white.bishop[1], new int[] { 6, 8 }, gr, false);
            setPos(white.king, new int[] { 5, 8 }, gr, false);
            setPos(white.queen, new int[] { 4, 8 }, gr, false);
        }

        static SoundPlayer sp = new SoundPlayer(Resource.stroke1);
        static public void setPos(figure Figure, int[] coord, Grid gr, bool posChange = true, bool other = true)
        {
            if (posChange)
            {
                int[] oldCoord = getPos(Figure, gr);
                GameLogic.gl.figPosAr[oldCoord[0], oldCoord[1]] = null;
                if (other) sp.Play();
            }
            else
            {
                if (other) gr.Children.Add(Figure);
            }
            Grid.SetColumn(Figure, coord[0]);
            Grid.SetRow(Figure, coord[1]);
            GameLogic.gl.figPosAr[coord[0], coord[1]] = Figure;
        }



        static public int[] getPos(Image figure, Grid gr)
        {
            int[] coord = { Grid.GetColumn(figure), Grid.GetRow(figure) };
            return coord;
        }

        static public void Invert(List<figure> figList, Grid gr, figure[,] figPosAr)
        {
            foreach (figure s in figList)
            {
                int[] posOld = getPos(s, gr);
                if(figPosAr[posOld[0], posOld[1]] == s)
                    setPos(s, new int[] { posOld[0], ArrayInv[posOld[1]] }, gr, true, false);
                else
                    setPos(s, new int[] { posOld[0], ArrayInv[posOld[1]] }, gr, false, false);
            }
        }

        static public void PawnTrans(figure fig, int num)
        {
            switch (num)
            {
                case 0:
                    {
                        fig.type = "knight";
                        if (fig.color)
                            fig.Source = white.imgs.knight;
                        else
                            fig.Source = black.imgs.knight;
                        break;
                    }
                case 1:
                    {
                        fig.type = "queen";
                        if (fig.color)
                            fig.Source = white.imgs.queen;
                        else
                            fig.Source = black.imgs.queen;
                        break;
                    }
                case 2:
                    {
                        fig.type = "castle";
                        if (fig.color)
                            fig.Source = chess.white.imgs.castle;
                        else
                            fig.Source = chess.black.imgs.castle;
                        break;
                    }
                case 3:
                    {
                        fig.type = "bishop";
                        if (fig.color)
                            fig.Source = chess.white.imgs.bishop;
                        else
                            fig.Source = chess.black.imgs.bishop;
                        break;
                    }
            }
            fig.other = "pawn";
        }
    }
}
