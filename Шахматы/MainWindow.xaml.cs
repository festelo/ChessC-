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
        public List<figure> figlist = new List<figure>();

        public GameLogic(Grid gr)
        {
            gl = this;
            ch = new chess(gr);
        }
        public void Drop(object sender, DragEventArgs e)
        {
            e.Data.GetFormats();
            int[] coord = new int[] { Grid.GetColumn((UIElement)sender), Grid.GetRow((UIElement)sender) };
            figure fig = (figure)e.Data.GetData("Шахматы.figure");
            if(checkMove(fig,coord)) ch.setPos(fig, coord);
            if (true)
            {
                fig.dead = true;
                fig.Visibility = Visibility.Hidden;
            }
            //Image img = (Image)e.Data.GetData(forma);
            //Grid.SetColumn(img, Grid.GetColumn(rect));
            //Grid.SetRow(img, Grid.GetRow(rect));
        }

        public bool checkMove(figure fig, int[] coord)
        {
            bool ok = false;
            int[] position = ch.getPos(fig);
            switch (fig.type)
            {
                case "pawn":
                    switch (fig.color)
                    {
                        case "white":
                            if (coord[0] == position[0] && ch.getFigFromPos(coord) == null && coord[1] == position[1] - 1 || position[1] == 7 && coord[1] == position[1] - 2) ok = true;
                            break;

                        case "black":
                            if (coord[0] == position[0] && coord[1] == position[1] + 1 || position[1] == 2 && coord[1] == position[1] + 2) ok = true;
                            break;
                    }
                    break;
                    
            }
            return ok;
        }

    }



    public class figure : Image
    {
        GameLogic gl = null;
        public int[] position = new int[2];
        public bool dead = false;
        public string type = null;
        public string color = null;

        public figure(BitmapImage img, string typ, string clr)
        {
            color = clr;
            type = typ;
            gl = GameLogic.gl;
            gl.figlist.Add(this);
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
                    pawn[i] = new figure(imgs.pawn, "pawn", "white");
                }
                return pawn;
            }
            static public figure[] pawn = pawnPrep();
            static public figure[] bishop = { new figure(imgs.bishop, "bishop", "white"), new figure(imgs.bishop, "bishop", "white") };
            static public figure[] knight = { new figure(imgs.knight, "knight", "white"), new figure(imgs.knight, "knight", "white") };
            static public figure[] castle = { new figure(imgs.castle, "castle", "white"), new figure(imgs.castle, "castle", "white") };
            static public figure queen = new figure(imgs.queen, "queen", "white");
            static public figure king = new figure(imgs.king, "king", "white");
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
                    pawn[i] = new figure(imgs.pawn, "pawn", "black");
                }
                return pawn;
            }
            static public figure[] pawn = pawnPrep();
            static public figure[] bishop = { new figure(imgs.bishop, "bishop", "black"), new figure(imgs.bishop, "bishop", "black") };
            static public figure[] knight = { new figure(imgs.knight, "knight", "black"), new figure(imgs.knight, "knight", "black") };
            static public figure[] castle = { new figure(imgs.castle, "castle", "black"), new figure(imgs.castle, "castle", "black") };
            static public figure queen = new figure(imgs.queen, "queen", "black");
            static public figure king = new figure(imgs.king, "king", "black");
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
                gr.Children.Add(black.pawn[i]);
                gr.Children.Add(white.pawn[i]);
                setPos(black.pawn[i], new int[] { i + 1, 2 });
                setPos(white.pawn[i], new int[] { i + 1, 7 });
            }
            for (int i = 0; i < 2; i++)
            {
                gr.Children.Add(black.castle[i]);
                gr.Children.Add(black.knight[i]);
                gr.Children.Add(black.bishop[i]);
                gr.Children.Add(white.castle[i]);
                gr.Children.Add(white.knight[i]);
                gr.Children.Add(white.bishop[i]);
            }
            gr.Children.Add(white.queen);
            gr.Children.Add(white.king);
            gr.Children.Add(black.queen);
            gr.Children.Add(black.king);

            setPos(black.castle[0], new int[] { 1, 1 });
            setPos(black.castle[1], new int[] { 8, 1 });
            setPos(black.knight[0], new int[] { 2, 1 });
            setPos(black.knight[1], new int[] { 7, 1 });
            setPos(black.bishop[0], new int[] { 3, 1 });
            setPos(black.bishop[1], new int[] { 6, 1 });
            setPos(black.king, new int[] { 5, 1 });
            setPos(black.queen, new int[] { 4, 1 });

            setPos(white.castle[0], new int[] { 1, 8 });
            setPos(white.castle[1], new int[] { 8, 8 });
            setPos(white.knight[0], new int[] { 2, 8 });
            setPos(white.knight[1], new int[] { 7, 8 });
            setPos(white.bishop[0], new int[] { 3, 8 });
            setPos(white.bishop[1], new int[] { 6, 8 });
            setPos(white.king, new int[] { 5, 8 });
            setPos(white.queen, new int[] { 4, 8 });
        }

        public void setPos(figure Figure, int[] coord)
        {
            Grid.SetColumn(Figure, coord[0]);
            Grid.SetRow(Figure, coord[1]);
            Figure.position = coord;
        }

        public figure getFigFromPos(int[] coord)
        {
            foreach (figure s in GameLogic.gl.figlist)
            {
                if (s.position[0] == coord[0] && s.position[1] == coord[1]) return s;
            }
            return null;
        }

        public int[] getPos(Image figure)
        {
            int[] coord = { Grid.GetColumn(figure), Grid.GetRow(figure) };
            return coord;
        }
    }
}
