using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Resource = Шахматы.Properties.Resources;
using Bitmap = System.Drawing.Bitmap;
using System.IO;

namespace Шахматы
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public class GameLogic
    {
        public static GameLogic gl = null;
        chess ch = null;
        //public List<figure> figlist = new List<figure>();
        public figure[,] figPosAr = new figure[9,9]; 

        public GameLogic(Grid gr)
        {
            gl = this;
            ch = new chess(gr);
        }
        public void Drop(object sender, DragEventArgs e)
        {
            e.Data.GetFormats();
            int[] coord = new int[] { Grid.GetColumn((UIElement)sender), Grid.GetRow((UIElement)sender) };
            try
            {
                string codekey = (string)e.Data.GetData(DataFormats.Text);
                figure newFigure = null;
                switch (codekey[1])
                {
                    case 'Q':
                        {
                            if (codekey[0] == 'B') newFigure = new figure(chess.black.imgs.queen, "queen", false);
                            else if (codekey[0] == 'W') newFigure = new figure(chess.white.imgs.queen, "queen", true);
                            break;
                        }
                    case 'C':
                        {
                            if (codekey[0] == 'B') newFigure = new figure(chess.black.imgs.castle, "castle", false);
                            else if (codekey[0] == 'W') newFigure = new figure(chess.white.imgs.castle, "castle", true);
                            break;
                        }
                    case 'K':
                        {
                            if (codekey[0] == 'B') newFigure = new figure(chess.black.imgs.knight, "knight", false);
                            else if (codekey[0] == 'W') newFigure = new figure(chess.white.imgs.knight, "knight", true);
                            break;
                        }
                    case 'P':
                        {
                            if (codekey[0] == 'B') newFigure = new figure(chess.black.imgs.pawn, "pawn", false);
                            else if (codekey[0] == 'W') newFigure = new figure(chess.white.imgs.pawn, "pawn", true);
                            break;
                        }
                    case 'B':
                        {
                            if (codekey[0] == 'B') newFigure = new figure(chess.black.imgs.bishop, "bishop", false);
                            else if (codekey[0] == 'W') newFigure = new figure(chess.white.imgs.bishop, "bishop", true);
                            break;
                        }
                    default:
                        return;
                }
                ch.setPos(newFigure, coord, false);
            }
            catch
            {
                figure fig = (figure)e.Data.GetData("Шахматы.figure");
                if (checkMove(fig, coord))
                {
                    try
                    {
                        figPosAr[coord[0], coord[1]] = null;
                        //figlist.Remove((figure)sender);
                        ((figure)sender).dead = true;
                        ((figure)sender).Visibility = Visibility.Hidden;
                    }
                    catch { }
                    ch.setPos(fig, coord);
                }
            }
            //Image img = (Image)e.Data.GetData(forma);
            //Grid.SetColumn(img, Grid.GetColumn(rect));
            //Grid.SetRow(img, Grid.GetRow(rect));
        }

        public bool checkMove(figure fig, int[] coord)
        {
            bool ok = false;
            int[] position = ch.getPos(fig);
            if (position[0] == coord[0] && position[1] == coord[1]) return false;
            switch (fig.type)
            {
                case "pawn":
                    switch (fig.color)
                    {
                        case true:
                            {
                                if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 && figPosAr[coord[0], coord[1]] != null && !figPosAr[coord[0], coord[1]].color) || (coord[0] == position[0] && figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) || (position[1] == 7 && coord[1] == position[1] - 2)) ok = true;
                                //if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 
                                //    && figPosAr[coord[0], coord[1]] != null && !figPosAr[coord[0], coord[1]].color) 
                                //    || (coord[0] == position[0] && figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) || (position[1] == 7 && coord[1] == position[1] - 2)) ok = true;
                                break;
                            }

                        case false:
                            {
                                if (((coord[0] == position[0] + 1 || coord[0] == position[0] - 1) && coord[1] == position[1] + 1 && figPosAr[coord[0], coord[1]] != null && figPosAr[coord[0], coord[1]].color) || (coord[0] == position[0] && figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] + 1) || (position[1] == 2 && coord[1] == position[1] + 2)) ok = true;
                                break;
                            }
                    }
                    break;
                case "bishop":
                    {
                        if ((coord[1]+coord[0]-position[0] != position[1] && coord[1] - coord[0] + position[0] != position[1]) ||(figPosAr[coord[0], coord[1]] != null && figPosAr[coord[0], coord[1]].color == fig.color)) break;
                        int i = position[0];
                        if (coord[0] > position[0])
                        {
                            if (coord[1] > position[1])
                                for (int j = position[1]; j < 9; j++, i++)
                                {
                                    if (coord[0] == i && coord[1] == j) { ok = true; break; }
                                    if (figPosAr[i, j] != null && figPosAr[i, j] != fig) { ok = false; break; }
                                }
                            else
                                for (int j = position[1]; j > 0; j--, i++)
                                {
                                    if (coord[0] == i && coord[1] == j) { ok = true; break; }
                                    if (figPosAr[i, j] != null && figPosAr[i, j] != fig) { ok = false; break; }
                                }
                        }
                        else
                        {
                            if (coord[1] > position[1])
                                for (int j = position[1]; j < 9; j++, i--)
                                {
                                    if (coord[0] == i && coord[1] == j) { ok = true; break; }
                                    if (figPosAr[i, j] != null && figPosAr[i, j] != fig) { ok = false; break; }
                                }
                            else
                                for (int j = position[1]; j > 0; j--, i--)
                                {
                                    if (coord[0] == i && coord[1] == j) { ok = true; break; }
                                    if (figPosAr[i, j] != null && figPosAr[i, j] != fig) { ok = false; break; }
                                }
                        }
                        break;
                    }
                case "knight":
                    {
                        if ((figPosAr[coord[0], coord[1]] == null
                            || (figPosAr[coord[0], coord[1]] != null && figPosAr[coord[0], coord[1]].color != fig.color))
                            && (((coord[0] == position[0] + 1 || coord[0] == position[0] - 1) && (coord[1] == position[1] + 2 || coord[1] == position[1] - 2))
                            ||  ((coord[0] == position[0] + 2 || coord[0] == position[0] - 2) && (coord[1] == position[1] + 1 || coord[1] == position[1] - 1)))
                            ) ok = true;
                        break;
                    }
                case "castle":
                    {
                        if ((coord[1] != position[1] && coord[0] != position[0]) || (figPosAr[coord[0], coord[1]] != null && figPosAr[coord[0], coord[1]].color == fig.color)) break;
                        if (coord[0] > position[0])
                        {
                            for (int j = position[0]; j < 9; j++)
                            {
                                if (coord[0] == j) { ok = true; break; }
                                if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]] != fig) { ok = false; break; }
                            }
                        }
                        else if(coord[0] < position[0])
                        {
                            for (int j = position[0]; j > 0; j--)
                            {
                                if (coord[0] == j) { ok = true; break; }
                                if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]] != fig) { ok = false; break; }
                            }
                        }
                        else if(coord[1] > position[1])
                        {
                            for (int j = position[1]; j < 9; j++)
                            {
                                if (coord[1] == j) { ok = true; break; }
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j] != fig) { ok = false; break; }
                            }
                        }
                        //if(coord[1] < position[1])
                        else
                        {
                            for (int j = position[1]; j > 0; j--)
                            {
                                if (coord[1] == j) { ok = true; break; }
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j] != fig) { ok = false; break; }
                            }
                        }
                        break;
                    }
                case "queen":
                    {

                        break;
                    }
            }
            return ok;
        }

    }



    public class figure : Image
    {
        GameLogic gl = null;
        public bool dead = false;
        public string type = null;
        public bool color = false;

        public figure(BitmapImage img, string typ, bool clr)
        {
            color = clr;
            type = typ;
            gl = GameLogic.gl;
            //gl.figlist.Add(this);
            MouseDown += Img_MouseDown;
            Height = 80;
            Width = 80;
            Source = img;
            AllowDrop = true;
            Drop += Figure_Drop;
        }

        private void Figure_Drop(object sender, DragEventArgs e)
        {
            if (Grid.GetColumn((UIElement)sender) == Grid.GetColumn((figure)e.Data.GetData("Шахматы.figure")) && Grid.GetRow((UIElement)sender) == Grid.GetRow((figure)e.Data.GetData("Шахматы.figure"))) return;
            gl.Drop(sender, e);
        }

        public void Img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            figure fig = (figure)sender;
            DragDrop.DoDragDrop(fig, fig, DragDropEffects.Move);
        }
    }



    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            new GameWindow(true);
            this.Close();
        }
    }



    public class GameWindow
    {
        public GameWindow(bool White)
        {
            Window win = new Window();
            win.SizeToContent = SizeToContent.WidthAndHeight;
            Grid gr = new Grid();
            GameLogic gl = new GameLogic(gr);
            //gr.Height = 700;
            //gr.Width = 700;
            //win.Show();
            gr.HorizontalAlignment = HorizontalAlignment.Left;
            gr.VerticalAlignment = VerticalAlignment.Top;
            char code = '\u0041';
            for(int i = 0; i<9; i++)
            {
                gr.RowDefinitions.Add(new RowDefinition());
                gr.ColumnDefinitions.Add(new ColumnDefinition());
            }
            bool tableColor = true;
            for (int i = 1; i<9; i++, code++)
            {
                TextBlock tbA = new TextBlock();
                tbA.Text = Convert.ToString(code);
                gr.Children.Add(tbA);
                tbA.VerticalAlignment = VerticalAlignment.Center;
                tbA.TextAlignment = TextAlignment.Center;
                //tbA.MaxHeight = 15;
                Grid.SetRow(tbA, 0);
                Grid.SetColumn(tbA, i);

                TextBlock tb1 = new TextBlock();
                tb1.VerticalAlignment = VerticalAlignment.Center;
                tb1.TextAlignment = TextAlignment.Center;
                tb1.MinWidth = 20;
                tb1.Text = Convert.ToString(i);
                gr.Children.Add(tb1);
                Grid.SetColumn(tb1, 0);
                Grid.SetRow(tb1, i);

                for (int j = 1; j < 9; j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.AllowDrop = true;
                    rect.Drop += gl.Drop;
                    rect.Width = 80;
                    rect.Height = 80;
                    SolidColorBrush color = new SolidColorBrush();
                    Panel.SetZIndex(rect, -1);
                    Grid.SetColumn(rect, i);
                    Grid.SetRow(rect, j);
                    gr.Children.Add(rect);

                    if (tableColor)
                    {
                        color.Color = Color.FromRgb(175, 119, 92);
                        tableColor = false;
                    }
                    else
                    {
                        color.Color = Color.FromRgb(231, 189, 140);
                        tableColor = true;
                    }
                    rect.Fill = color;
                }
                tableColor = !tableColor;
            }
            win.Content = gr;
            win.Show();
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

        static BitmapImage BitmapToImageSource(Bitmap bitmap)
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

        Grid gr = null;
        public chess(Grid grid)
        {
            gr = grid;
            for (int i = 0; i < 8; i++)
            {
                setPos(black.pawn[i], new int[] { i + 1, 2 }, false);
                setPos(white.pawn[i], new int[] { i + 1, 7 }, false);
            }

            setPos(black.castle[0], new int[] { 1, 1 }, false);
            setPos(black.castle[1], new int[] { 8, 1 }, false);
            setPos(black.knight[0], new int[] { 2, 1 }, false);
            setPos(black.knight[1], new int[] { 7, 1 }, false);
            setPos(black.bishop[0], new int[] { 3, 1 }, false);
            setPos(black.bishop[1], new int[] { 6, 1 }, false);
            setPos(black.king, new int[] { 5, 1 }, false);
            setPos(black.queen, new int[] { 4, 1 }, false);

            setPos(white.castle[0], new int[] { 1, 8 }, false);
            setPos(white.castle[1], new int[] { 8, 8 }, false);
            setPos(white.knight[0], new int[] { 2, 8 }, false);
            setPos(white.knight[1], new int[] { 7, 8 }, false);
            setPos(white.bishop[0], new int[] { 3, 8 }, false);
            setPos(white.bishop[1], new int[] { 6, 8 }, false);
            setPos(white.king, new int[] { 5, 8 }, false);
            setPos(white.queen, new int[] { 4, 8 }, false);
        }

        public void setPos(figure Figure, int[] coord, bool posChange = true)
        {
            if(posChange)
            {
                int[] oldCoord = getPos(Figure);
                GameLogic.gl.figPosAr[oldCoord[0], oldCoord[1]] = null;
            }
            else
            {
                gr.Children.Add(Figure);
            }
            Grid.SetColumn(Figure, coord[0]);
            Grid.SetRow(Figure, coord[1]);
            GameLogic.gl.figPosAr[coord[0], coord[1]] = Figure;
        }

        //public figure getFigFromPos(int[] coord)
        //{
        //    foreach (figure s in GameLogic.gl.figlist)
        //    {
        //        if (s.position[0] == coord[0] && s.position[1] == coord[1]) return s;
        //    }
        //    return null;
        //}

        public int[] getPos(Image figure)
        {
            int[] coord = { Grid.GetColumn(figure), Grid.GetRow(figure) };
            return coord;
        }
    }
}
