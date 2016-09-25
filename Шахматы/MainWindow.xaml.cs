using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

namespace Шахматы
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        bool online = false;
        Thread myThread = null;
        Socket sendSock = null;
        bool color = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!online)
            {
                new GameWindow(true);
                Close();
            }
            else
            {
                try
                {
                    // Буфер для входящих данных
                    if (textBox.Text == "") { MessageBox.Show("Введите ID или закройте экспандер для оффлайн игры"); return; }
                    button.IsEnabled = false;
                    byte[] bytes = new byte[1024];

                    // Соединяемся с удаленным устройством

                    // Устанавливаем удаленную точку для сокета

                    byte[] msg = null;
                    color = true;
                    if (radioButton.IsChecked.Value) { msg = Encoding.UTF8.GetBytes(textBox.Text + "-b"); color = false; }
                    else msg = Encoding.UTF8.GetBytes(textBox.Text + "-wh");
                    sendSock.Send(msg);

                    // Получаем ответ от сервера

                    // Используем рекурсию для неоднократного вызова SendMessageFromSocket()

                    // Освобождаем сокет
                }
                catch (Exception ex)
                {
                    button.IsEnabled = true;
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        public static string IP = "127.0.0.1";
        private void InternetServer()
        {
            IPAddress ipAddr = IPAddress.Parse(IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11224);

            sendSock = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            try
            {
                sendSock.Connect(ipEndPoint);
            }
            catch
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                textBlockID.Text = "Ошибка подключения к серверу!";
                            }
                            );
                return;
            }
            byte[] bytes = new byte[1024];
            int bytesRec = sendSock.Receive(bytes);
            string IDtext = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                textBlockID.Text = "Ваш ID: " + IDtext;
                            }
                            );

            try
            {

                // Начинаем слушать соединения
                while (true)
                {

                    // Программа приостанавливается, ожидая входящее соединение

                    // Мы дождались клиента, пытающегося с нами соединиться

                    bytes = new byte[128];
                    bytesRec = sendSock.Receive(bytes);

                    string data = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    if (data == "A")
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart)delegate ()
                                        {
                                            new GameWindow(color, sendSock); Close();
                                        }); return;
                    }
                    else if (data == "ider")
                    {
                        MessageBox.Show("Неверный ID");
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart)delegate ()
                                        { button.IsEnabled = true; });
                        continue;
                    }

                    else if (data == "n")
                    {
                        MessageBox.Show("Вам отказали.");
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart)delegate ()
                                        { button.IsEnabled = true; });
                        continue;
                    }

                    string ID = data.Split('-')[0];
                    string clr = data.Split('-')[1];
                    color = true;
                    string MSG = " черных";
                    if (clr == "wh") { color = false; MSG = " белых."; }
                    // Отправляем ответ клиенту
                    MessageBoxResult MSGresult = MessageBox.Show("Пользователь " + ID + " отправил запрос сыграть за" + MSG, "Соединение", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (MSGresult == MessageBoxResult.Yes)
                    {
                        sendSock.Send(Encoding.UTF8.GetBytes("yes"));
                        //sendSock.Shutdown(SocketShutdown.Both);
                        //handler.Close();
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                new GameWindow(color, sendSock, true);
                                Close();
                            }
                            );
                        return;
                    }
                    else
                    {
                        sendSock.Send(Encoding.UTF8.GetBytes("n"));
                    }
                }
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
                //Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void expander_Expanded(object sender, RoutedEventArgs e)
        {
            online = !online;
            if (online)
            {

                myThread = new Thread(InternetServer); myThread.IsBackground = true; myThread.SetApartmentState(ApartmentState.STA); myThread.Start();
            }
            else
            {
                //myThread.Abort;
                //sListener.Shutdown(SocketShutdown.Both);
                sendSock.Close();
            }
        }
    }



    public class GameWindow
    {
        public GameWindow(bool gamecolor, Socket online = null, bool host = false)
        {
            Window win = new Window();
            win.ResizeMode = ResizeMode.CanMinimize;
            win.SizeToContent = SizeToContent.WidthAndHeight;
            RenderOptions.SetBitmapScalingMode(win, BitmapScalingMode.HighQuality);
            Grid gr = new Grid();
            GameLogic gl = new GameLogic(gr, gamecolor, online, host);
            //gr.Height = 700;
            //gr.Width = 700;
            //win.Show();
            gr.HorizontalAlignment = HorizontalAlignment.Left;
            gr.VerticalAlignment = VerticalAlignment.Top;
            char code = '\u0041';
            for (int i = 0; i < 9; i++)
            {
                gr.RowDefinitions.Add(new RowDefinition());
                gr.ColumnDefinitions.Add(new ColumnDefinition());
            }
            gr.ColumnDefinitions.Add(new ColumnDefinition());
            bool tableColor = true;
            for (int i = 1; i < 9; i++, code++)
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
                tb1.Text = Convert.ToString(9 - i);
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

        //public figure getFigFromPos(int[] coord)
        //{
        //    foreach (figure s in GameLogic.gl.figlist)
        //    {
        //        if (s.position[0] == coord[0] && s.position[1] == coord[1]) return s;
        //    }
        //    return null;
        //}
    }
