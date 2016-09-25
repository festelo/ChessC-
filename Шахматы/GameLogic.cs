using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Windows.Threading;

namespace Шахматы
{
    public class GameLogic
    {
        public static GameLogic gl = null;
        chess ch = null;
        public static int blockMode = 1;
        bool color = false;
        TextBlock textLog = null;

        public figure[,] figPosAr = new figure[9, 9];
        public List<figure> figLiveList = new List<figure>();
        public List<StrokeData> StrokeHis = new List<StrokeData>();

        Socket handler = null;
        DethGrid dg = null;
        Grid grid = null;
        TextBlock chat = null;
        Button btnRet = null;
        List<MoveList> moveList = new List<MoveList>();
        List<MoveList> moveWithChessList = new List<MoveList>();
        public GameLogic(Grid gr, bool clr, Socket ip = null, bool host = false)
        {
            color = clr;
            gl = this;
            grid = gr;
            ch = new chess(gr);
            Grid grInf = new Grid();
            gr.Children.Add(grInf);
            Grid.SetColumn(grInf, 9);
            Grid.SetRow(grInf, 1);
            Grid.SetRowSpan(grInf, 8);
            grInf.HorizontalAlignment = HorizontalAlignment.Left;
            grInf.VerticalAlignment = VerticalAlignment.Top;

            for (int i = 0; i < 8; i++) grInf.RowDefinitions.Add(new RowDefinition());

            Rectangle rectS = new Rectangle();
            rectS.Width = 100;
            rectS.Height = 40;
            SolidColorBrush colorS = new SolidColorBrush();
            colorS.Color = Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF5);
            rectS.Fill = colorS;
            rectS.Stroke = new SolidColorBrush(Colors.Black);
            rectS.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(rectS, 0);
            grInf.Children.Add(rectS);

            for (int i = 0; i < 2; i++)
            {
                Rectangle rectSS = new Rectangle();
                rectSS.Width = 100;
                rectSS.Height = 40;
                SolidColorBrush colorSS = new SolidColorBrush();
                rectSS.Fill = colorS;
                rectSS.Stroke = new SolidColorBrush(Colors.Black);
                rectSS.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetRow(rectSS, 4 + i);
                grInf.Children.Add(rectSS);
            }
            dg = new DethGrid(grInf);
            btnRet = new Button();
            btnRet.Height = 27;
            btnRet.Content = "Отменить ход";
            btnRet.Click += (sender, e) => { if (handler == null) strokeBack(); else { chatSend("bc"); btnRet.IsEnabled = false; } };
            grInf.Children.Add(btnRet);
            Grid.SetRow(btnRet, 1);

            if (ip != null)
            {
                handler = ip;

                Rectangle rc = new Rectangle();
                rc.Width = 100;
                rc.Height = 130;
                rc.Fill = colorS;
                rc.Stroke = new SolidColorBrush(Colors.Black);
                rc.VerticalAlignment = VerticalAlignment.Top;
                ScrollViewer sv = new ScrollViewer();
                chat = new TextBlock();
                chat.Text = "Connected\n";
                chat.TextWrapping = TextWrapping.Wrap;
                //sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                sv.Margin = new Thickness(3);
                sv.Height = 115;
                sv.Width = 94;
                sv.Content = chat;
                grInf.Children.Add(rc);
                grInf.Children.Add(sv);
                Grid.SetRow(sv, 6);
                Grid.SetRow(rc, 6);
                Grid.SetZIndex(rc, -1);
                TextBox sendBox = new TextBox();
                sendBox.HorizontalAlignment = HorizontalAlignment.Left;
                sendBox.Height = 20;
                sendBox.Width = 70;

                Button sendBtn = new Button();
                sendBtn.Content = "Send";
                sendBtn.Height = 20;
                sendBtn.Width = 30;
                sendBtn.Click += (sender, e) =>
                {
                    Thread thre = new Thread(chatSend);
                    thre.Start("-" + sendBox.Text);
                    chat.Text += "U: " + sendBox.Text + "\n";
                    sendBox.Text = "";
                };
                sendBtn.HorizontalAlignment = HorizontalAlignment.Right;

                grInf.Children.Add(sendBox);
                grInf.Children.Add(sendBtn);
                Grid.SetRow(sendBox, 7);
                Grid.SetRow(sendBtn, 7);

                Thread th = new Thread(chatReceive);
                th.IsBackground = true;
                th.Start();

                Button btnNoth = new Button();
                btnNoth.Height = 27;
                btnNoth.Content = "Ничья";
                grInf.Children.Add(btnNoth);
                Grid.SetRow(btnNoth, 2);

                Button btnWH = new Button();
                btnWH.Height = 27;
                btnWH.Content = "Сдаться";
                grInf.Children.Add(btnWH);
                Grid.SetRow(btnWH, 3);
            }
            textLog = new TextBlock();
            textLog.Text = "Ваш ход ";
            textLog.TextAlignment = TextAlignment.Center;
            textLog.VerticalAlignment = VerticalAlignment.Center;
            textLog.Width = 100;
            textLog.TextWrapping = TextWrapping.Wrap;
            grInf.Children.Add(textLog);
            Grid.SetRow(textLog, 0);
            if (!clr)
            {
                blockMode = 0;
                textLog.Text = "Ход соперника";
            }
            moveList = getMoveList(true);
        }

        private void strokeBack()
        {

            try
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    if (StrokeHis.Count() == 0) return;
                    int i = StrokeHis.Count() - 1;
                    ch.setPos(StrokeHis[i].Figure, new int[] { StrokeHis[i].oldX, StrokeHis[i].oldY });
                    if (StrokeHis[i].FigureDeath != null)
                    {
                        dg.Remove(StrokeHis[i].FigureDeath);
                        ch.setPos(StrokeHis[i].FigureDeath, new int[] { StrokeHis[i].DeathX, StrokeHis[i].DeathY }, false);
                        figLiveList.Add(StrokeHis[i].FigureDeath);
                    }
                    if (handler == null)
                    {
                        if (StrokeHis[i].Figure.color) { blockMode = 1; textLog.Text = "Ход белых"; }
                        else { blockMode = 2; textLog.Text = "Ход черных"; }
                    }
                    else
                    {
                        if (StrokeHis[i].Figure.color == color) { if (color) blockMode = 1; else blockMode = 2; textLog.Text = "Ваш ход"; }
                        //else if (StrokeHis[i].Figure.color && !color) { blockMode = 2; textLog.Text = "Ваш ход"; }
                        else { blockMode = 0; textLog.Text = "Ход соперника"; }
                    }
                    bool refrColor = !StrokeHis[i].Figure.color;
                    StrokeHis.RemoveAt(StrokeHis.Count() - 1);
                    refreshMoveList(refrColor);
                }
                );
            }
            catch { }
        }

        private void chatReceive()
        {
            byte[] bytes = new byte[1024];
            int bytesRec = -1;
            while (true)
            {
                try
                {
                    bytesRec = handler.Receive(bytes);
                }
                catch
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            textLog.Text = "Соединение\nразорвано";
                            blockMode = 0;
                        }
                        );
                }
                string msg = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                string TextToCB = "";

                if (msg[0] == 's')
                {
                    int[] arr = msg.Remove(0, 1).Select(x => x - '0').ToArray();
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            if (figPosAr[arr[0], arr[1]].type == "king" && arr[1] == arr[3])
                            {
                                if (arr[2] == 3)
                                {
                                    ch.setPos(figPosAr[arr[2] - 1, arr[3]], new int[] { arr[2] + 1, arr[3] });
                                }
                                else if (arr[2] == 7)
                                {
                                    ch.setPos(figPosAr[arr[2] + 1, arr[3]], new int[] { arr[2] - 1, arr[3] });
                                }
                            }
                            textLog.Text = "Ваш ход";
                            makeMove(figPosAr[arr[0], arr[1]], new int[] { arr[2], arr[3], arr[0], arr[1] }, true);
                        }
                    );
                    if (!color) blockMode = 2;
                    else blockMode = 1;
                    continue;
                }
                if (msg[0] == '-') TextToCB = "En: " + msg.Remove(0, 1);
                else if (msg == "bc")
                {
                    MessageBoxResult MSGresult = MessageBox.Show("Противник хочет отменить ход.", "Отмена хода", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (MSGresult == MessageBoxResult.Yes)
                    {
                        chatSend("bcy");
                        strokeBack();
                    }
                    else
                    {
                        chatSend("bcn");
                    }
                    continue;
                }
                else if (msg == "bcn")
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        btnRet.IsEnabled = true;
                    }
                    ); continue;
                }
                else if (msg == "bcy")
                {
                    strokeBack();
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        btnRet.IsEnabled = true;
                    }
                    );
                    continue;
                }
                else if (msg == "okw") TextToCB = "Sucks";
                else if (msg == "oks") TextToCB = "Sucks";
                else if (msg == "err") TextToCB = "ERROR";
                else if (msg == "errcon")
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
(ThreadStart)delegate ()
{
    textLog.Text = "Противник отключился";
    blockMode = 0;
}
);
                    return;
                }
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        chat.Text += TextToCB + "\n";
                    }
                );
            }
        }

        public void chatSend(object Text)
        {
            if (((string)Text)[0] == 's')
            {

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        textLog.Text = "Отправка хода";
                    }
                );
            }
            try
            {
                handler.Send(Encoding.UTF8.GetBytes((string)Text));
                if (((string)Text)[0] == 's')
                {

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            textLog.Text = "Ход\nпротивника";
                        }
                    );
                }
            }
            catch
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        textLog.Text = "Соединение\nразорвано";
                    }
                    );
                }
                catch { }
                return;
            }
        }


        public void Drop(object sender, DragEventArgs e)
        {
            e.Data.GetFormats();
            int[] coord = new int[] { Grid.GetColumn((UIElement)sender), Grid.GetRow((UIElement)sender) };
            figure fig = (figure)e.Data.GetData("Шахматы.figure");
            int[] oldcoord = ch.getPos(fig);
            int move = makeMove(fig, coord);
            if (move != -1)
            {
                if (handler != null)
                {
                    Thread th = new Thread(new ParameterizedThreadStart(chatSend));
                    th.IsBackground = true;
                    th.Start("s" + oldcoord[0] + "" + oldcoord[1] + "" + coord[0] + "" + coord[1]);
                    blockMode = 0;
                }
                else
                {
                    if (!fig.color) { blockMode = 1; if (move == 0) textLog.Text = "Ход белых"; }
                    else { blockMode = 2; if (move == 0) textLog.Text = "Ход черных"; }
                }
            }
            //Image img = (Image)e.Data.GetData(forma);
            //Grid.SetColumn(img, Grid.GetColumn(rect));
            //Grid.SetRow(img, Grid.GetRow(rect));
        }

        int makeMove(figure fig, int[] coord, bool fast = false)
        {
            int[] oldcoord;
            figure oldfig = figPosAr[coord[0], coord[1]];
            if (!fast)
            {
                oldcoord = ch.getPos(fig);
                bool ok = false;
                foreach (MoveList s in moveList)
                {
                    if (fig == s.Figure)
                    {
                        for (int i = 0; i < s.X.Count(); i++)
                        {
                            if (s.X[i] == coord[0] && s.Y[i] == coord[1])
                            { ch.setPos(s.Figure, coord); ok = true; break; }
                        }
                        break;
                    }
                }
                if (!ok)
                    return -1;
            }
            else {
                oldcoord = new int[] { coord[2], coord[3] };
                ch.setPos(fig, coord);
            }
            StrokeData sd = new StrokeData();
            sd.oldX = oldcoord[0];
            sd.oldY = oldcoord[1];
            sd.X = coord[0];
            sd.Y = coord[1];
            sd.Figure = fig;
            
            if(oldfig != null)
            {
                //figlist.Remove((figure)sender);
                grid.Children.Remove(oldfig);
                dg.Add(oldfig);
                sd.FigureDeath = oldfig;
                sd.DeathX = coord[0];
                sd.DeathY = coord[1];
                figLiveList.Remove(oldfig);
            }
            else if(fig.type == "pawn")
            {
                int z = -1;
                if (fig.color) z = 1;
                if (StrokeHis.Count != 0 && StrokeHis[StrokeHis.Count - 1].Figure == figPosAr[coord[0], coord[1] + z] && StrokeHis[StrokeHis.Count - 1].Figure.type == "pawn")
                {
                    grid.Children.Remove(figPosAr[coord[0], coord[1] - 1]);
                    dg.Add(figPosAr[coord[0], coord[1] - 1]);
                    sd.FigureDeath = figPosAr[coord[0], coord[1] - 1];
                    sd.DeathX = coord[0];
                    sd.DeathY = coord[1] + z;
                    figLiveList.Remove(figPosAr[coord[0], coord[1] - 1]);
                }
            }
            StrokeHis.Add(sd);
            bool check = false;
            IEnumerable<figure> figColorList = figLiveList.Where(figure => figure.color == fig.color);
            if (fig.color)
            {
                foreach (figure s in figColorList)
                {
                    if (checkMoveToCheck(s, ch.getPos(chess.black.king), ch.getPos(s)))
                    {
                        check = true;
                        if (handler == null) textLog.Text = "Чёрным объявлен шах";
                        else { textLog.Text = "Вам объявлен\nшах"; }
                        break;
                    }
                }
            }
            else
            {
                foreach (figure s in figColorList)
                {
                    if (checkMoveToCheck(s, ch.getPos(chess.white.king), ch.getPos(s)))
                    {
                        check = true;
                        if (handler == null) textLog.Text = "Белым объявлен\nшах";
                        else { textLog.Text = "Вам объявлен\nшах"; }
                        break;
                    }
                }
            }

            if(handler == null || fast)refreshMoveList(fig.color);
            if (moveList.Count == 0)
            {
                if (check) textLog.Text = "МАТ";
                else textLog.Text = "ПАТ";
                return 1;
            }
            if (check) return 1;
            return 0;
        }

        void refreshMoveList(bool clr)
        {
            moveWithChessList = getMoveList(!clr);
            moveList = moveWithChessList;
            if (clr)
            {
                foreach (MoveList s in moveList)
                {
                    figPosAr[s.oldX, s.oldY] = null;
                    for (int i = 0; i < s.X.Count(); i++)
                    {
                        figure tempFigure = figPosAr[s.X[i], s.Y[i]];
                        if (tempFigure != null) figLiveList.Remove(tempFigure);
                        figPosAr[s.X[i], s.Y[i]] = s.Figure;
                        bool ok = true;
                        IEnumerable<figure> figColorList = figLiveList.Where(figure => figure.color);
                        foreach (figure k in figColorList)
                        {
                            int[] sc = { s.X[i], s.Y[i] };
                            if (true/*k != s.Figure*/) sc = ch.getPos(k);
                            int[] kingPos = ch.getPos(chess.black.king);
                            if (s.Figure.type == "king") { kingPos[0] = s.X[i]; kingPos[1] = s.Y[i]; }
                            if (checkMoveToCheck(k, kingPos, sc)) // Ходы результат которых приведет к шаху // Ходы результат которых приведет к шаху
                            {
                                if (tempFigure != null) figLiveList.Add(tempFigure);
                                figPosAr[s.X[i], s.Y[i]] = tempFigure;
                                s.X.RemoveAt(i);
                                s.Y.RemoveAt(i);
                                i--;
                                ok = false;
                                break;
                            }
                        }
                        if (ok)
                        {
                            figPosAr[s.X[i], s.Y[i]] = tempFigure;
                            if (tempFigure != null) figLiveList.Add(tempFigure);
                        }
                    }
                    figPosAr[s.oldX, s.oldY] = s.Figure;
                }
            }
            else
            {
                foreach (MoveList s in moveList)
                {
                    figPosAr[s.oldX, s.oldY] = null;
                    for (int i = 0; i < s.X.Count(); i++)
                    {
                        figure tempFigure = figPosAr[s.X[i], s.Y[i]];
                        if (tempFigure != null) figLiveList.Remove(tempFigure);
                        figPosAr[s.X[i], s.Y[i]] = s.Figure;
                        bool ok = true;
                        IEnumerable<figure> figColorList = figLiveList.Where(figure => !figure.color);
                        foreach (figure k in figColorList)
                        {
                            int[] sc = { s.X[i], s.Y[i] };
                            if ((true/*k != s.Figure*/))
                                sc = ch.getPos(k);
                            int[] kingPos = ch.getPos(chess.white.king);
                            if (s.Figure.type == "king") { kingPos[0] = s.X[i]; kingPos[1] = s.Y[i]; }
                            if (checkMoveToCheck(k, kingPos, sc)) // Ходы результат которых приведет к шаху
                            {
                                if (tempFigure != null) figLiveList.Add(tempFigure);
                                figPosAr[s.X[i], s.Y[i]] = tempFigure;
                                s.X.RemoveAt(i);
                                s.Y.RemoveAt(i);
                                i--;
                                ok = false;
                                break;
                            }
                        }
                        if (ok)
                        {
                            figPosAr[s.X[i], s.Y[i]] = tempFigure;
                            if (tempFigure != null) figLiveList.Add(tempFigure);
                        }
                    }
                    figPosAr[s.oldX, s.oldY] = s.Figure;
                }
            }
            for (int i = 0; i < moveList.Count; i++)
            {
                if (moveList[i].X.Count == 0) { moveList.RemoveAt(i); i--; }
            }
        }

        public List<MoveList> getMoveList(bool clr)
        {
            List<MoveList> ReturnList = new List<MoveList>();
            IEnumerable<figure> figColorList = figLiveList.Where(figure => figure.color == clr);
            foreach (figure fig in figColorList)
            {
                int[] position = ch.getPos(fig);
                MoveList ml = new MoveList();
                ml.Figure = fig;
                ml.oldX = position[0];
                ml.oldY = position[1];
                switch (fig.type)
                {
                    case "pawn":
                        switch (fig.color)
                        {
                            case true:
                                {
                                    //Взятие на проходе направо.
                                    if (position[0] + 1 < 9 && figPosAr[position[0] + 1, position[1] -1] == null && StrokeHis.Count != 0 && StrokeHis[StrokeHis.Count-1].Figure == figPosAr[position[0] + 1, position[1]] && StrokeHis[StrokeHis.Count - 1].Figure.type == "pawn")
                                    {
                                        ml.X.Add(position[0] + 1);
                                        ml.Y.Add(position[1] - 1);
                                    }

                                    //Взятие на проходе налево.
                                    if (position[0] - 1 > 0 && figPosAr[position[0] - 1, position[1] - 1] == null && StrokeHis.Count != 0 && StrokeHis[StrokeHis.Count - 1].Figure == figPosAr[position[0] - 1, position[1]] && StrokeHis[StrokeHis.Count - 1].Figure.type == "pawn")
                                    {
                                        ml.X.Add(position[0] - 1);
                                        ml.Y.Add(position[1] - 1);
                                    }

                                    if (position[0] + 1 < 9 && figPosAr[position[0] + 1, position[1] - 1] != null && figPosAr[position[0] + 1, position[1] - 1].color != fig.color) // БИТИЕ НА ПРАВО
                                    {
                                        ml.X.Add(position[0] + 1);
                                        ml.Y.Add(position[1] - 1);
                                    }
                                    if (position[0] - 1 > 0 && figPosAr[position[0] - 1, position[1] - 1] != null && figPosAr[position[0] - 1, position[1] - 1].color != fig.color) // БИТИЕ НА ЛЕВО
                                    {
                                        ml.X.Add(position[0] - 1);
                                        ml.Y.Add(position[1] - 1);
                                    }
                                    if (figPosAr[position[0], position[1] - 1] == null) // Ход наверх
                                    {
                                        ml.X.Add(position[0]);
                                        ml.Y.Add(position[1] - 1);
                                    }
                                    if (position[1] == 7 && figPosAr[position[0], position[1] - 2] == null) // Ход на две клетки
                                    {
                                        ml.X.Add(position[0]);
                                        ml.Y.Add(position[1] - 2);
                                    }
                                    //if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 && figPosAr[coord[0], coord[1]] != null) ||
                                    //    (coord[0] == position[0] && figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) ||
                                    //    (figPosAr[coord[0], coord[1]] == null && position[1] == 7 && coord[1] == position[1] - 2 && coord[0] == position[0])) ok = true;
                                    //if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 
                                    //    && figPosAr[coord[0], coord[1]] != null && !figPosAr[coord[0], coord[1]].color) 
                                    //    || (coord[0] == position[0] && figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) || (position[1] == 7 && coord[1] == position[1] - 2)) ok = true;
                                    break;
                                }

                            case false:
                                {
                                    if (position[0] + 1 < 9 && figPosAr[position[0] + 1, position[1] + 1] == null && StrokeHis.Count != 0 && StrokeHis[StrokeHis.Count - 1].Figure == figPosAr[position[0] + 1, position[1]] && StrokeHis[StrokeHis.Count - 1].Figure.type == "pawn")
                                    {
                                        ml.X.Add(position[0] + 1);
                                        ml.Y.Add(position[1] + 1);
                                    }

                                    if (position[0] - 1 > 0 && figPosAr[position[0] - 1, position[1] + 1] == null && StrokeHis.Count != 0 && StrokeHis[StrokeHis.Count - 1].Figure == figPosAr[position[0] - 1, position[1]] && StrokeHis[StrokeHis.Count - 1].Figure.type == "pawn")
                                    {
                                        ml.X.Add(position[0] - 1);
                                        ml.Y.Add(position[1] + 1);
                                    }
                                    if (position[0] + 1 < 9 && figPosAr[position[0] + 1, position[1] + 1] != null && figPosAr[position[0] + 1, position[1] + 1].color != fig.color) // БИТИЕ НА ПРАВО
                                    {
                                        ml.X.Add(position[0] + 1);
                                        ml.Y.Add(position[1] + 1);
                                    }
                                    if (position[0] - 1 > 0 && figPosAr[position[0] - 1, position[1] + 1] != null && figPosAr[position[0] - 1, position[1] + 1].color != fig.color) // БИТИЕ НА ЛЕВО
                                    {
                                        ml.X.Add(position[0] - 1);
                                        ml.Y.Add(position[1] + 1);
                                    }
                                    if (figPosAr[position[0], position[1] + 1] == null) // Ход вниз
                                    {
                                        ml.X.Add(position[0]);
                                        ml.Y.Add(position[1] + 1);
                                    }
                                    if (position[1] == 2 && figPosAr[position[0], position[1] + 2] == null) // Ход на две клетки
                                    {
                                        ml.X.Add(position[0]);
                                        ml.Y.Add(position[1] + 2);
                                    }
                                    break;
                                }
                        }
                        break;
                    case "bishop":
                        {
                            int i = position[0] + 1;
                            for (int j = position[1] + 1; j < 9; j++, i++)
                            {
                                if (i > 8) break;
                                if (figPosAr[i, j] != null && figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (figPosAr[i, j] != null) break;
                            }
                            i = position[0] + 1;
                            for (int j = position[1] - 1; j > 0; j--, i++)
                            {
                                if (i > 8) break;
                                if (figPosAr[i, j] != null && figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (figPosAr[i, j] != null) break;
                            }
                            i = position[0] - 1;
                            for (int j = position[1] + 1; j < 9; j++, i--)
                            {
                                if (i < 1) break;
                                if (figPosAr[i, j] != null && figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (figPosAr[i, j] != null) break;
                            }
                            i = position[0] - 1;
                            for (int j = position[1] - 1; j > 0; j--, i--)
                            {
                                if (i < 1) break;
                                if (figPosAr[i, j] != null && figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (figPosAr[i, j] != null) break;
                            }
                            break;
                        }
                    case "knight":
                        {
                            /*
                             * ВСЕ ХОДЫ КОНЯ
                            position[0] + 1, position[1] + 2; 
                            position[0] - 1, position[1] + 2;

                            position[0] + 1, position[1] - 2;
                            position[0] - 1, position[1] - 2
                            
                            position[0] + 2, position[1] + 1;
                            position[0] + 2, position[1] - 1;

                            position[0] - 2, position[1] + 1;
                            position[0] - 2, position[1] - 1;
                            */
                            int x = 1;
                            int y = 2;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (figPosAr[position[0] + x, position[1] + y] == null || figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = -1; y = 2;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (figPosAr[position[0] + x, position[1] + y] == null || figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = 1; y = -2;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (figPosAr[position[0] + x, position[1] + y] == null || figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = -1; y = -2;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (figPosAr[position[0] + x, position[1] + y] == null || figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = 2; y = 1;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (figPosAr[position[0] + x, position[1] + y] == null || figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }

                            x = 2; y = -1;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (figPosAr[position[0] + x, position[1] + y] == null || figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = -2; y = 1;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (figPosAr[position[0] + x, position[1] + y] == null || figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }

                            x = -2; y = -1;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (figPosAr[position[0] + x, position[1] + y] == null || figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            break;
                        }
                    case "castle":
                        {
                            for (int j = position[0] + 1; j < 9; j++)
                            {
                                if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]].color == fig.color) { break; }
                                ml.X.Add(j);
                                ml.Y.Add(position[1]);
                                if (figPosAr[j, position[1]] != null) { break; }
                            }
                            for (int j = position[0] - 1; j > 0; j--)
                            {
                                if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]].color == fig.color) { break; }
                                ml.X.Add(j);
                                ml.Y.Add(position[1]);
                                if (figPosAr[j, position[1]] != null) { break; }
                            }
                            for (int j = position[1] + 1; j < 9; j++)
                            {
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j].color == fig.color) { break; }
                                ml.X.Add(position[0]);
                                ml.Y.Add(j);
                                if (figPosAr[position[0], j] != null) { break; }
                            }
                            for (int j = position[1] - 1; j > 0; j--)
                            {
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j].color == fig.color) { break; }
                                ml.X.Add(position[0]);
                                ml.Y.Add(j);
                                if (figPosAr[position[0], j] != null) { break; }
                            }
                            break;
                        }
                    case "queen":
                        {
                            int i = position[0] + 1;
                            for (int j = position[1] + 1; j < 9; j++, i++)
                            {
                                if (i > 8) break;
                                if (figPosAr[i, j] != null && figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (figPosAr[i, j] != null) break;
                            }
                            i = position[0] + 1;
                            for (int j = position[1] - 1; j > 0; j--, i++)
                            {
                                if (i > 8) break;
                                if (figPosAr[i, j] != null && figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (figPosAr[i, j] != null) break;
                            }
                            i = position[0] - 1;
                            for (int j = position[1] + 1; j < 9; j++, i--)
                            {
                                if (i < 1) break;
                                if (figPosAr[i, j] != null && figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (figPosAr[i, j] != null) break;
                            }
                            i = position[0] - 1;
                            for (int j = position[1] - 1; j > 0; j--, i--)
                            {
                                if (i < 1) break;
                                if (figPosAr[i, j] != null && figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (figPosAr[i, j] != null) break;
                            }



                            for (int j = position[0] + 1; j < 9; j++)
                            {
                                if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]].color == fig.color) { break; }
                                ml.X.Add(j);
                                ml.Y.Add(position[1]);
                                if (figPosAr[j, position[1]] != null) { break; }
                            }
                            for (int j = position[0] - 1; j > 0; j--)
                            {
                                if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]].color == fig.color) { break; }
                                ml.X.Add(j);
                                ml.Y.Add(position[1]);
                                if (figPosAr[j, position[1]] != null) { break; }
                            }
                            for (int j = position[1] + 1; j < 9; j++)
                            {
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j].color == fig.color) { break; }
                                ml.X.Add(position[0]);
                                ml.Y.Add(j);
                                if (figPosAr[position[0], j] != null) { break; }
                            }
                            for (int j = position[1] - 1; j > 0; j--)
                            {
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j].color == fig.color) { break; }
                                ml.X.Add(position[0]);
                                ml.Y.Add(j);
                                if (figPosAr[position[0], j] != null) { break; }
                            }
                            break;
                        }
                    case "king":
                        {
                            for (int i = -1; i < 2; i++)
                                if (i + position[0] < 9 && i + position[0] > 0)
                                    for (int j = -1; j < 2; j++)
                                    {
                                        if (j + position[1] < 9 && j + position[1] > 0 && (figPosAr[i + position[0], j + position[1]] == null || figPosAr[i + position[0], j + position[1]].color != fig.color))
                                        {
                                            ml.X.Add(i + position[0]);
                                            ml.Y.Add(j + position[1]);
                                        }

                                    }
                            if ((bool)fig.other)
                            {
                                if (fig.color)
                                {
                                    //TODO
                                    //if ((bool)chess.white.castle[0].other) { ml.X.Add(3); ml.Y.Add(8); }
                                    //if ((bool)chess.white.castle[1].other) { ml.X.Add(7); ml.Y.Add(8); }
                                }
                                else
                                {
                                    //if ((bool)chess.black.castle[0].other) { ml.X.Add(3); ml.Y.Add(1); }
                                    //if ((bool)chess.black.castle[1].other) { ml.X.Add(7); ml.Y.Add(1); }
                                }
                            }
                            //if (-2 < coord[0] - position[0] && coord[0] - position[0] < 2 && -2 < coord[1] - position[1] && coord[1] - position[1] < 2) { ok = true; fig.other = true; }
                            break;
                        }
                }
                if (ml.X.Count != 0)
                    ReturnList.Add(ml);
            }
            return ReturnList;
        }

        public bool checkMoveToCheck(figure fig, int[] coord, int[] position)
        {
            bool ok = false;
            if (figPosAr[coord[0], coord[1]].color == fig.color)
                return false;
            switch (fig.type)
            {
                case "pawn":
                    switch (fig.color)
                    {
                        case true:
                            {
                                if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 && figPosAr[coord[0], coord[1]] != null) ||
                                    (coord[0] == position[0] && figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) ||
                                    (figPosAr[coord[0], coord[1]] == null && position[1] == 7 && coord[1] == position[1] - 2 && coord[0] == position[0])) ok = true;
                                //if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 
                                //    && figPosAr[coord[0], coord[1]] != null && !figPosAr[coord[0], coord[1]].color) 
                                //    || (coord[0] == position[0] && figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) || (position[1] == 7 && coord[1] == position[1] - 2)) ok = true;
                                break;
                            }

                        case false:
                            {
                                if (((coord[0] == position[0] + 1 || coord[0] == position[0] - 1) && coord[1] == position[1] + 1 && figPosAr[coord[0], coord[1]] != null) ||
                                    (coord[0] == position[0] && figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] + 1) ||
                                    (figPosAr[coord[0], coord[1]] == null && position[1] == 2 && coord[1] == position[1] + 2 && coord[0] == position[0])) ok = true;
                                break;
                            }
                    }
                    break;
                case "bishop":
                    {
                        if ((coord[1] + coord[0] - position[0] != position[1] && coord[1] - coord[0] + position[0] != position[1])) break;
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
                        if ((((coord[0] == position[0] + 1 || coord[0] == position[0] - 1) && (coord[1] == position[1] + 2 || coord[1] == position[1] - 2))
                            || ((coord[0] == position[0] + 2 || coord[0] == position[0] - 2) && (coord[1] == position[1] + 1 || coord[1] == position[1] - 1)))
                            ) ok = true;
                        break;
                    }
                case "castle":
                    {
                        if ((coord[1] != position[1] && coord[0] != position[0])) break;
                        if (coord[0] > position[0])
                        {
                            for (int j = position[0]; j < 9; j++)
                            {
                                if (coord[0] == j) { ok = true; fig.other = true; break; }
                                if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]] != fig) { break; }
                            }
                        }
                        else if (coord[0] < position[0])
                        {
                            for (int j = position[0]; j > 0; j--)
                            {
                                if (coord[0] == j) { ok = true; fig.other = true; break; }
                                if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]] != fig) { break; }
                            }
                        }
                        else if (coord[1] > position[1])
                        {
                            for (int j = position[1]; j < 9; j++)
                            {
                                if (coord[1] == j) { ok = true; fig.other = true; break; }
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j] != fig) { break; }
                            }
                        }
                        //if(coord[1] < position[1])
                        else
                        {
                            for (int j = position[1]; j > 0; j--)
                            {
                                if (coord[1] == j) { ok = true; fig.other = true; break; }
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j] != fig) { break; }
                            }
                        }
                        break;
                    }
                case "queen":
                    {
                        if ((coord[1] != position[1] && coord[0] != position[0]) && (coord[1] + coord[0] - position[0] != position[1] && coord[1] - coord[0] + position[0] != position[1])) break;
                        int i = position[0];
                        if (coord[0] > position[0])
                        {
                            if (coord[1] > position[1])
                                for (int j = position[1]; j < 9; j++, i++)
                                {
                                    if (coord[0] == i && coord[1] == j) { ok = true; break; }
                                    if (figPosAr[i, j] != null && figPosAr[i, j] != fig) { ok = false; break; }
                                }
                            else if (coord[1] < position[1])
                                for (int j = position[1]; j > 0; j--, i++)
                                {
                                    if (coord[0] == i && coord[1] == j) { ok = true; break; }
                                    if (figPosAr[i, j] != null && figPosAr[i, j] != fig) { ok = false; break; }
                                }
                            else
                                for (int j = position[0]; j < 9; j++)
                                {
                                    if (coord[0] == j) { ok = true; fig.other = true; break; }
                                    if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]] != fig) { break; }
                                }
                        }
                        else if (coord[0] < position[0])
                        {
                            if (coord[1] > position[1])
                                for (int j = position[1]; j < 9; j++, i--)
                                {
                                    if (coord[0] == i && coord[1] == j) { ok = true; break; }
                                    if (figPosAr[i, j] != null && figPosAr[i, j] != fig) { ok = false; break; }
                                }
                            else if (coord[1] < position[1])
                                for (int j = position[1]; j > 0; j--, i--)
                                {
                                    if (coord[0] == i && coord[1] == j) { ok = true; break; }
                                    if (figPosAr[i, j] != null && figPosAr[i, j] != fig) { ok = false; break; }
                                }
                            else for (int j = position[0]; j > 0; j--)
                                {
                                    if (coord[0] == j) { ok = true; fig.other = true; break; }
                                    if (figPosAr[j, position[1]] != null && figPosAr[j, position[1]] != fig) { break; }
                                }
                        }
                        else if (coord[1] > position[1])
                        {
                            for (int j = position[1]; j < 9; j++)
                            {
                                if (coord[1] == j) { ok = true; fig.other = true; break; }
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j] != fig) { break; }
                            }
                        }
                        //if(coord[1] < position[1])
                        else
                        {
                            for (int j = position[1]; j > 0; j--)
                            {
                                if (coord[1] == j) { ok = true; fig.other = true; break; }
                                if (figPosAr[position[0], j] != null && figPosAr[position[0], j] != fig) { break; }
                            }
                        }
                        break;
                    }
                case "king":
                    {
                        if (-2 < coord[0] - position[0] && coord[0] - position[0] < 2 && -2 < coord[1] - position[1] && coord[1] - position[1] < 2) { ok = true; fig.other = true; }
                        else if (!(bool)fig.other && coord[1] == position[1])
                        {
                            if (coord[0] == 3 && figPosAr[coord[0] - 1, coord[1]] != null && figPosAr[coord[0] - 1, coord[1]].type == "castle" && !(bool)figPosAr[coord[0] - 1, coord[1]].other)
                            {
                                for (int i = 4; i > 1; i--)
                                {
                                    if (figPosAr[coord[0], coord[1]] != null) return false;
                                }
                                ok = true;
                                fig.other = true;
                                ch.setPos(figPosAr[coord[0] - 1, coord[1]], new int[] { coord[0] + 1, coord[1] });
                            }
                            else if (coord[0] == 7 && figPosAr[coord[0] + 1, coord[1]] != null && figPosAr[coord[0] + 1, coord[1]].type == "castle" && !(bool)figPosAr[coord[0] + 1, coord[1]].other && figPosAr[6, coord[1]] == null && figPosAr[7, coord[1]] == null)
                            { ok = true; fig.other = true; ch.setPos(figPosAr[coord[0] + 1, coord[1]], new int[] { coord[0] - 1, coord[1] }); }
                        }
                        break;
                    }
            }
            return ok;
        }

    }
}
