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
using System.Windows.Input;

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
        public List<MoveList> moveList = new List<MoveList>();
        public Grid grid = null;
        public bool inverted = false;

        Socket handler = null;
        DethGrid dg = null;
        TextBlock chat = null;
        Button btnRet = null;
        Moves moves = null;
        GameWindow thisWin = null;
        public GameLogic(Grid gr, bool clr, GameWindow gw, Socket ip = null,  bool host = false)
        {
            thisWin = gw;
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
            moves = new Moves(this);

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
            btnRet.Width = 80;
            btnRet.Content = "Отменить ход";
            btnRet.HorizontalAlignment = HorizontalAlignment.Left;
            btnRet.Click += (sender, e) => { if (handler == null) strokeBack(); else { chatSend("bc"); btnRet.IsEnabled = false; } };

            Button btnRefr = new Button();
            btnRefr.Height = 27;
            btnRefr.Width = 20;
            btnRefr.HorizontalAlignment = HorizontalAlignment.Right;
            Image img = new Image();
            img.Source = chess.BitmapToImageSource(Шахматы.Properties.Resources.refresh);
            btnRefr.Content = img;
            btnRefr.Click += (sender, e) => 
            { inverted = !inverted; chess.Invert(figLiveList, gr, figPosAr); if (StrokeHis.Count != 0) moves.refreshMoveList(StrokeHis[StrokeHis.Count - 1].Figure.color); else moveList = moves.getMoveList(true); };

            grInf.Children.Add(btnRet);
            Grid.SetRow(btnRet, 1);
            grInf.Children.Add(btnRefr);
            Grid.SetRow(btnRefr, 1);

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
                Button sendBtn = new Button();

                sendBox.HorizontalAlignment = HorizontalAlignment.Left;
                sendBox.Height = 20;
                sendBox.Width = 70;
                sendBox.KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        sendBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        sendBox.Focus();
                    }
                };

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
            moveList = moves.getMoveList(true);
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
                    if(inverted)
                        chess.setPos(StrokeHis[i].Figure, new int[] { StrokeHis[i].oldX, chess.ArrayInv[StrokeHis[i].oldY] }, grid);
                    else
                        chess.setPos(StrokeHis[i].Figure, new int[] { StrokeHis[i].oldX, StrokeHis[i].oldY }, grid);
                    StrokeHis[i].Figure.other = StrokeHis[i].oldOther;
                    if (StrokeHis[i].FigureDeath != null)
                    {
                        if (StrokeHis[i].CustomDeath)
                        {
                            if (inverted)
                                chess.setPos(StrokeHis[i].FigureDeath, new int[] { StrokeHis[i].DeathX, chess.ArrayInv[StrokeHis[i].DeathY] }, grid, true, false);
                            else
                                chess.setPos(StrokeHis[i].FigureDeath, new int[] { StrokeHis[i].DeathX, StrokeHis[i].DeathY }, grid, true, false);
                        }
                        else
                        {
                            dg.Remove(StrokeHis[i].FigureDeath);
                            if (inverted)
                                chess.setPos(StrokeHis[i].FigureDeath, new int[] { StrokeHis[i].DeathX, chess.ArrayInv[StrokeHis[i].DeathY] }, grid, false);
                            else
                                chess.setPos(StrokeHis[i].FigureDeath, new int[] { StrokeHis[i].DeathX, StrokeHis[i].DeathY }, grid, false);
                            figLiveList.Add(StrokeHis[i].FigureDeath);
                        }
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
                    moves.refreshMoveList(refrColor);
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
                    if (inverted)
                    {
                        arr[1] = chess.ArrayInv[arr[1]];
                        arr[3] = chess.ArrayInv[arr[3]];
                    }
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            textLog.Text = "Ваш ход";

                            if (figPosAr[arr[0], arr[1]].type == "pawn" && (figPosAr[arr[0], arr[1]].color && arr[3] == 1) || (!figPosAr[arr[0], arr[1]].color && arr[3] == 8))
                                chess.PawnTrans(figPosAr[arr[0], arr[1]], arr[4]);
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
            int[] oldcoord = chess.getPos(fig, grid);
            int[] move = makeMove(fig, coord);
            if (move[0] != -1)
            {
                if (handler != null)
                {
                    Thread th = new Thread(new ParameterizedThreadStart(chatSend));
                    th.IsBackground = true;
                    if (inverted)
                    {
                        oldcoord[1] = chess.ArrayInv[oldcoord[1]];
                        coord[1] = chess.ArrayInv[coord[1]];
                    }
                    if(move[1] != -1)
                        th.Start("s" + oldcoord[0] + "" + oldcoord[1] + "" + coord[0] + "" + coord[1] + "" + move[1]);
                    else
                        th.Start("s" + oldcoord[0] + "" + oldcoord[1] + "" + coord[0] + "" + coord[1]);
                    blockMode = 0;
                }
                else
                {
                    if (!fig.color) { blockMode = 1; if (move[0] == 0) textLog.Text = "Ход белых"; }
                    else { blockMode = 2; if (move[0] == 0) textLog.Text = "Ход черных"; }
                }
            }
            //Image img = (Image)e.Data.GetData(forma);
            //Grid.SetColumn(img, Grid.GetColumn(rect));
            //Grid.SetRow(img, Grid.GetRow(rect));
        }

        int[] makeMove(figure fig, int[] coord, bool fast = false)
        {
            int[] returnNum = { -1, -1 };
            int[] oldcoord;
            figure oldfig = figPosAr[coord[0], coord[1]];
            if (!fast)
            {
                oldcoord = chess.getPos(fig, grid);
                bool ok = false;
                foreach (MoveList s in moveList)
                {
                    if (fig == s.Figure)
                    {
                        for (int i = 0; i < s.X.Count(); i++)
                        {
                            if (s.X[i] == coord[0] && s.Y[i] == coord[1])
                            { chess.setPos(s.Figure, coord, grid); ok = true; break; }
                        }
                        break;
                    }
                }
                if (!ok)
                {
                    return returnNum;
                }
                if (fig.type == "pawn" && ((fig.color && coord[1] == 1) || (!fig.color && coord[1] == 8)))
                {
                    ChooseChessWin ccw = new ChooseChessWin(fig.color);
                    ccw.Owner = thisWin;
                    ccw.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    ccw.ShowDialog();
                    chess.PawnTrans(fig, ccw.numSelect);
                    returnNum[1] = ccw.numSelect;
                }
            }
            else
            {
                oldcoord = new int[] { coord[2], coord[3] };
                chess.setPos(fig, coord, grid);
            }
            StrokeData sd = new StrokeData();
            sd.oldOther = fig.other;
            if (fig.type == "king")
            {
                if (!(bool)fig.other)
                {
                    fig.other = true;
                    if (coord[0] == 3)
                    {
                        sd.CustomDeath = true;
                        if (fig.color)
                        {
                            int pos = 8;
                            if (inverted) pos = chess.ArrayInv[pos];
                            int[] CastlePos = chess.getPos(chess.white.castle[0], grid);
                            if (inverted)
                            {
                                CastlePos[1] = chess.ArrayInv[CastlePos[1]];
                            }
                            sd.DeathX = CastlePos[0];
                            sd.DeathY = CastlePos[1];
                            chess.setPos(chess.white.castle[0], new int[] { 4, pos }, grid, true, false);
                            sd.FigureDeath = chess.white.castle[0];
                        }
                        
                        else
                        {
                            int pos = 1;
                            if (inverted) pos = chess.ArrayInv[pos];
                            int[] CastlePos = chess.getPos(chess.black.castle[0], grid);
                            if (inverted)
                            {
                                CastlePos[1] = chess.ArrayInv[CastlePos[1]];
                            }
                            sd.DeathX = CastlePos[0];
                            sd.DeathY = CastlePos[1];
                            chess.setPos(chess.black.castle[0], new int[] { 4, pos }, grid, true, false);
                            sd.FigureDeath = chess.black.castle[0];
                        }
                    }
                    else if (coord[0] == 7)
                    {
                        sd.CustomDeath = true;

                        if (fig.color)
                        {
                            int pos = 8;
                            if (inverted) pos = chess.ArrayInv[pos];
                            int[] CastlePos = chess.getPos(chess.white.castle[1], grid);
                            if(inverted)
                            {
                                CastlePos[1] = chess.ArrayInv[CastlePos[1]];
                            }
                            sd.DeathX = CastlePos[0];
                            sd.DeathY = CastlePos[1];
                            chess.setPos(chess.white.castle[1], new int[] { 6, pos }, grid, true, false);
                            sd.FigureDeath = chess.white.castle[1];
                        }

                        else
                        {
                            int pos = 1;
                            if (inverted) pos = chess.ArrayInv[pos];
                            int[] CastlePos = chess.getPos(chess.black.castle[1], grid);
                            if (inverted)
                            {
                                CastlePos[1] = chess.ArrayInv[CastlePos[1]];
                            }
                            sd.DeathX = CastlePos[0];
                            sd.DeathY = CastlePos[1];
                            chess.setPos(chess.black.castle[1], new int[] { 6, pos }, grid, true, false);
                            sd.FigureDeath = chess.black.castle[1];
                        }
                    }
                }
            }
            if ((fig.color && (fig == chess.white.castle[0] || fig == chess.white.castle[1] )) || (!fig.color && ( fig == chess.black.castle[0] || fig == chess.black.castle[1])) && !(bool)fig.other) fig.other = true;
            if (inverted)
            {
                sd.oldY = chess.ArrayInv[oldcoord[1]];
                sd.Y = chess.ArrayInv[coord[1]];
            }
            else
            {
                sd.oldY = oldcoord[1];
                sd.Y = coord[1];
            }
            sd.X = coord[0];
            sd.oldX = oldcoord[0];
            sd.Figure = fig;

            if (oldfig != null)
            {
                //figlist.Remove((figure)sender);
                grid.Children.Remove(oldfig);
                dg.Add(oldfig);
                sd.FigureDeath = oldfig;
                sd.DeathX = coord[0];
                if(inverted)
                    sd.DeathY = chess.ArrayInv[coord[1]];
                else
                    sd.DeathY = coord[1];
                figLiveList.Remove(oldfig);
            }
            else if (fig.type == "pawn")
            {
                int z = -1;
                if (fig.color) z = 1;
                if (StrokeHis.Count != 0 && StrokeHis[StrokeHis.Count - 1].Figure == figPosAr[coord[0], coord[1] + z] && StrokeHis[StrokeHis.Count - 1].Figure.type == "pawn")
                {
                    grid.Children.Remove(figPosAr[coord[0], coord[1] - 1]);
                    dg.Add(figPosAr[coord[0], coord[1] - 1]);
                    sd.FigureDeath = figPosAr[coord[0], coord[1] - 1];
                    sd.DeathX = coord[0];
                    if (inverted)
                        sd.DeathY = chess.ArrayInv[coord[1] + z];
                    else
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
                    if (moves.checkMoveToCheck(s, chess.getPos(chess.black.king, grid), chess.getPos(s, grid)))
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
                    if (moves.checkMoveToCheck(s, chess.getPos(chess.white.king, grid), chess.getPos(s, grid)))
                    {
                        check = true;
                        if (handler == null) textLog.Text = "Белым объявлен\nшах";
                        else { textLog.Text = "Вам объявлен\nшах"; }
                        break;
                    }
                }
            }
            returnNum[0] = 0;
            if (handler == null || fast) moves.refreshMoveList(fig.color);
            if (moveList.Count == 0)
            {
                if (check) textLog.Text = "МАТ";
                else textLog.Text = "ПАТ";
                returnNum[0] = 1;
            }
            if (check) returnNum[0] = 1;
            return returnNum;
        }

    }
}
