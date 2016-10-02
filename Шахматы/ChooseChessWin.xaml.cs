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
using System.Windows.Shapes;

namespace Шахматы
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class ChooseChessWin : Window
    {
        Image checkImg = new Image();
        public int numSelect = 1;
        public ChooseChessWin(bool color)
        {
            InitializeComponent();
            Image[] img = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                img[i] = new Image();
                img[i].Height = 80;
                img[i].Width = 80;
                img[i].MouseLeftButtonDown += (sender, e) => 
                { int num = Grid.GetColumn((UIElement)sender); numSelect = num; Grid.SetColumn(checkImg, num); };
            }
            if (color)
            {
                img[0].Source = chess.white.imgs.knight;
                img[1].Source = chess.white.imgs.queen;
                img[2].Source = chess.white.imgs.castle;
                img[3].Source = chess.white.imgs.bishop;
                //gr.Children.Add();
            }
            else
            {
                img[0].Source = chess.black.imgs.knight;
                img[1].Source = chess.black.imgs.queen;
                img[2].Source = chess.black.imgs.castle;
                img[3].Source = chess.black.imgs.bishop;
                //gr.Children.Add();
            }
            for(int i = 0; i<4; i++)
            {
                gr.Children.Add(img[i]);
                Grid.SetColumn(img[i], i);
            }
            checkImg.Source = chess.BitmapToImageSource(Шахматы.Properties.Resources.Галка);
            checkImg.Width = 20;
            checkImg.Height = 20;
            checkImg.Margin = new Thickness(6,0,0,3);
            checkImg.HorizontalAlignment = HorizontalAlignment.Left;
            checkImg.VerticalAlignment = VerticalAlignment.Bottom;
            gr.Children.Add(checkImg);
            Grid.SetColumn(checkImg, 1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
