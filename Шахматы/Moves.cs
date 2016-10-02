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
    class Moves
    {
        GameLogic GL = null;

        public Moves(GameLogic gl)
        {
            GL = gl;
        }

        public void refreshMoveList(bool clr)
        {
            GL.moveList = new List<MoveList>(); 
            GL.moveList = getMoveList(!clr);
            if (clr)
            {
                foreach (MoveList s in GL.moveList)
                {
                    GL.figPosAr[s.oldX, s.oldY] = null;
                    for (int i = 0; i < s.X.Count(); i++)
                    {
                        figure tempFigure = GL.figPosAr[s.X[i], s.Y[i]];
                        if (tempFigure != null) GL.figLiveList.Remove(tempFigure);
                        GL.figPosAr[s.X[i], s.Y[i]] = s.Figure;
                        bool ok = true;
                        IEnumerable<figure> figColorList = GL.figLiveList.Where(figure => figure.color);
                        foreach (figure k in figColorList)
                        {
                            int[] sc = { s.X[i], s.Y[i] };
                            if (true/*k != s.Figure*/) sc = chess.getPos(k, GL.grid);
                            int[] kingPos = chess.getPos(chess.black.king ,GL.grid);
                            if (s.Figure.type == "king") { kingPos[0] = s.X[i]; kingPos[1] = s.Y[i]; }
                            if (checkMoveToCheck(k, kingPos, sc)) // Ходы результат которых приведет к шаху // Ходы результат которых приведет к шаху
                            {
                                if (tempFigure != null) GL.figLiveList.Add(tempFigure);
                                GL.figPosAr[s.X[i], s.Y[i]] = tempFigure;
                                s.X.RemoveAt(i);
                                s.Y.RemoveAt(i);
                                i--;
                                ok = false;
                                break;
                            }
                        }
                        if (ok)
                        {
                            GL.figPosAr[s.X[i], s.Y[i]] = tempFigure;
                            if (tempFigure != null) GL.figLiveList.Add(tempFigure);
                        }
                    }
                    GL.figPosAr[s.oldX, s.oldY] = s.Figure;
                }
            }
            else
            {
                foreach (MoveList s in GL.moveList)
                {
                    GL.figPosAr[s.oldX, s.oldY] = null;
                    for (int i = 0; i < s.X.Count(); i++)
                    {
                        figure tempFigure = GL.figPosAr[s.X[i], s.Y[i]];
                        if (tempFigure != null) GL.figLiveList.Remove(tempFigure);
                        GL.figPosAr[s.X[i], s.Y[i]] = s.Figure;
                        bool ok = true;
                        IEnumerable<figure> figColorList = GL.figLiveList.Where(figure => !figure.color);
                        foreach (figure k in figColorList)
                        {
                            int[] sc = { s.X[i], s.Y[i] };
                            if ((true/*k != s.Figure*/))
                                sc = chess.getPos(k, GL.grid);
                            int[] kingPos = chess.getPos(chess.white.king, GL.grid);
                            if (s.Figure.type == "king") { kingPos[0] = s.X[i]; kingPos[1] = s.Y[i]; }
                            if (checkMoveToCheck(k, kingPos, sc)) // Ходы результат которых приведет к шаху
                            {
                                if (tempFigure != null) GL.figLiveList.Add(tempFigure);
                                GL.figPosAr[s.X[i], s.Y[i]] = tempFigure;
                                s.X.RemoveAt(i);
                                s.Y.RemoveAt(i);
                                i--;
                                ok = false;
                                break;
                            }
                        }
                        if (ok)
                        {
                            GL.figPosAr[s.X[i], s.Y[i]] = tempFigure;
                            if (tempFigure != null) GL.figLiveList.Add(tempFigure);
                        }
                    }
                    GL.figPosAr[s.oldX, s.oldY] = s.Figure;
                }
            }
            for (int i = 0; i < GL.moveList.Count; i++)
            {
                if (GL.moveList[i].X.Count == 0) { GL.moveList.RemoveAt(i); i--; }
            }
        }

        public List<MoveList> getMoveList(bool clr)
        {
            List<MoveList> ReturnList = new List<MoveList>();
            IEnumerable<figure> figColorList = GL.figLiveList.Where(figure => figure.color == clr);
            foreach (figure fig in figColorList)
            {
                int[] position = chess.getPos(fig, GL.grid);
                MoveList ml = new MoveList();
                ml.Figure = fig;
                ml.oldX = position[0];
                ml.oldY = position[1];
                switch (fig.type)
                {
                    case "pawn":
                        { 
                            if((fig.color && !GL.inverted) || (!fig.color && GL.inverted))
                                {
                                //Взятие на проходе направо.
                                int dop = 4;
                                //if (GL.inverted) dop = chess.ArrayInv[dop];
                                    if (position[0] == dop && position[0] + 1 < 9 && GL.figPosAr[position[0] + 1, position[1] - 1] == null && GL.StrokeHis.Count != 0 && GL.StrokeHis[GL.StrokeHis.Count - 1].Figure == GL.figPosAr[position[0] + 1, position[1]] && GL.StrokeHis[GL.StrokeHis.Count - 1].Figure.type == "pawn")
                                    {
                                        ml.X.Add(position[0] + 1);
                                        ml.Y.Add(position[1] - 1);
                                    }

                                    //Взятие на проходе налево.
                                    if (position[0] == dop && position[0] - 1 > 0 && GL.figPosAr[position[0] - 1, position[1] - 1] == null && GL.StrokeHis.Count != 0 && GL.StrokeHis[GL.StrokeHis.Count - 1].Figure == GL.figPosAr[position[0] - 1, position[1]] && GL.StrokeHis[GL.StrokeHis.Count - 1].Figure.type == "pawn")
                                    {
                                        ml.X.Add(position[0] - 1);
                                        ml.Y.Add(position[1] - 1);
                                    }

                                    if (position[0] + 1 < 9 && GL.figPosAr[position[0] + 1, position[1] - 1] != null && GL.figPosAr[position[0] + 1, position[1] - 1].color != fig.color) // БИТИЕ НА ПРАВО
                                    {
                                        ml.X.Add(position[0] + 1);
                                        ml.Y.Add(position[1] - 1);
                                    }
                                    if (position[0] - 1 > 0 && GL.figPosAr[position[0] - 1, position[1] - 1] != null && GL.figPosAr[position[0] - 1, position[1] - 1].color != fig.color) // БИТИЕ НА ЛЕВО
                                    {
                                        ml.X.Add(position[0] - 1);
                                        ml.Y.Add(position[1] - 1);
                                    }
                                    if (GL.figPosAr[position[0], position[1] - 1] == null) // Ход наверх
                                    {
                                        ml.X.Add(position[0]);
                                        ml.Y.Add(position[1] - 1);
                                    }
                                dop = 7;
                                //if (GL.inverted) dop = chess.ArrayInv[dop];
                                    if (position[1] == dop && GL.figPosAr[position[0], position[1] - 2] == null) // Ход на две клетки
                                    {
                                        ml.X.Add(position[0]);
                                        ml.Y.Add(position[1] - 2);
                                    }
                                    //if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 && GL.figPosAr[coord[0], coord[1]] != null) ||
                                    //    (coord[0] == position[0] && GL.figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) ||
                                    //    (GL.figPosAr[coord[0], coord[1]] == null && position[1] == 7 && coord[1] == position[1] - 2 && coord[0] == position[0])) ok = true;
                                    //if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 
                                    //    && GL.figPosAr[coord[0], coord[1]] != null && !GL.figPosAr[coord[0], coord[1]].color) 
                                    //    || (coord[0] == position[0] && GL.figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) || (position[1] == 7 && coord[1] == position[1] - 2)) ok = true;
                                }

                            else
                            {
                                int dop = 5;
                                if (position[0] == dop && position[0] + 1 < 9 && GL.figPosAr[position[0] + 1, position[1] + 1] == null && GL.StrokeHis.Count != 0 && GL.StrokeHis[GL.StrokeHis.Count - 1].Figure == GL.figPosAr[position[0] + 1, position[1]] && GL.StrokeHis[GL.StrokeHis.Count - 1].Figure.type == "pawn")
                                    {
                                        ml.X.Add(position[0] + 1);
                                        ml.Y.Add(position[1] + 1);
                                    }

                                    if (position[0] == dop && position[0] - 1 > 0 && GL.figPosAr[position[0] - 1, position[1] + 1] == null && GL.StrokeHis.Count != 0 && GL.StrokeHis[GL.StrokeHis.Count - 1].Figure == GL.figPosAr[position[0] - 1, position[1]] && GL.StrokeHis[GL.StrokeHis.Count - 1].Figure.type == "pawn")
                                    {
                                        ml.X.Add(position[0] - 1);
                                        ml.Y.Add(position[1] + 1);
                                    }
                                    if (position[0] + 1 < 9 && GL.figPosAr[position[0] + 1, position[1] + 1] != null && GL.figPosAr[position[0] + 1, position[1] + 1].color != fig.color) // БИТИЕ НА ПРАВО
                                    {
                                        ml.X.Add(position[0] + 1);
                                        ml.Y.Add(position[1] + 1);
                                    }
                                    if (position[0] - 1 > 0 && GL.figPosAr[position[0] - 1, position[1] + 1] != null && GL.figPosAr[position[0] - 1, position[1] + 1].color != fig.color) // БИТИЕ НА ЛЕВО
                                    {
                                        ml.X.Add(position[0] - 1);
                                        ml.Y.Add(position[1] + 1);
                                    }
                                    if (GL.figPosAr[position[0], position[1] + 1] == null) // Ход вниз
                                    {
                                        ml.X.Add(position[0]);
                                        ml.Y.Add(position[1] + 1);
                                }
                                dop = 2;
                                if (position[1] == dop && GL.figPosAr[position[0], position[1] + 2] == null) // Ход на две клетки
                                    {
                                        ml.X.Add(position[0]);
                                        ml.Y.Add(position[1] + 2);
                                    }
                                }
                        }
                        break;
                    case "bishop":
                        {
                            int i = position[0] + 1;
                            for (int j = position[1] + 1; j < 9; j++, i++)
                            {
                                if (i > 8) break;
                                if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (GL.figPosAr[i, j] != null) break;
                            }
                            i = position[0] + 1;
                            for (int j = position[1] - 1; j > 0; j--, i++)
                            {
                                if (i > 8) break;
                                if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (GL.figPosAr[i, j] != null) break;
                            }
                            i = position[0] - 1;
                            for (int j = position[1] + 1; j < 9; j++, i--)
                            {
                                if (i < 1) break;
                                if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (GL.figPosAr[i, j] != null) break;
                            }
                            i = position[0] - 1;
                            for (int j = position[1] - 1; j > 0; j--, i--)
                            {
                                if (i < 1) break;
                                if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (GL.figPosAr[i, j] != null) break;
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
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (GL.figPosAr[position[0] + x, position[1] + y] == null || GL.figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = -1; y = 2;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (GL.figPosAr[position[0] + x, position[1] + y] == null || GL.figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = 1; y = -2;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (GL.figPosAr[position[0] + x, position[1] + y] == null || GL.figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = -1; y = -2;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (GL.figPosAr[position[0] + x, position[1] + y] == null || GL.figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = 2; y = 1;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (GL.figPosAr[position[0] + x, position[1] + y] == null || GL.figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }

                            x = 2; y = -1;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (GL.figPosAr[position[0] + x, position[1] + y] == null || GL.figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }
                            x = -2; y = 1;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (GL.figPosAr[position[0] + x, position[1] + y] == null || GL.figPosAr[position[0] + x, position[1] + y].color != fig.color))
                            {
                                ml.X.Add(position[0] + x);
                                ml.Y.Add(position[1] + y);
                            }

                            x = -2; y = -1;
                            if (position[0] + x < 9 && position[0] + x > 0 && position[1] + y < 9 && position[1] + y > 0 && (GL.figPosAr[position[0] + x, position[1] + y] == null || GL.figPosAr[position[0] + x, position[1] + y].color != fig.color))
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
                                if (GL.figPosAr[j, position[1]] != null && GL.figPosAr[j, position[1]].color == fig.color) { break; }
                                ml.X.Add(j);
                                ml.Y.Add(position[1]);
                                if (GL.figPosAr[j, position[1]] != null) { break; }
                            }
                            for (int j = position[0] - 1; j > 0; j--)
                            {
                                if (GL.figPosAr[j, position[1]] != null && GL.figPosAr[j, position[1]].color == fig.color) { break; }
                                ml.X.Add(j);
                                ml.Y.Add(position[1]);
                                if (GL.figPosAr[j, position[1]] != null) { break; }
                            }
                            for (int j = position[1] + 1; j < 9; j++)
                            {
                                if (GL.figPosAr[position[0], j] != null && GL.figPosAr[position[0], j].color == fig.color) { break; }
                                ml.X.Add(position[0]);
                                ml.Y.Add(j);
                                if (GL.figPosAr[position[0], j] != null) { break; }
                            }
                            for (int j = position[1] - 1; j > 0; j--)
                            {
                                if (GL.figPosAr[position[0], j] != null && GL.figPosAr[position[0], j].color == fig.color) { break; }
                                ml.X.Add(position[0]);
                                ml.Y.Add(j);
                                if (GL.figPosAr[position[0], j] != null) { break; }
                            }
                            break;
                        }
                    case "queen":
                        {
                            int i = position[0] + 1;
                            for (int j = position[1] + 1; j < 9; j++, i++)
                            {
                                if (i > 8) break;
                                if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (GL.figPosAr[i, j] != null) break;
                            }
                            i = position[0] + 1;
                            for (int j = position[1] - 1; j > 0; j--, i++)
                            {
                                if (i > 8) break;
                                if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (GL.figPosAr[i, j] != null) break;
                            }
                            i = position[0] - 1;
                            for (int j = position[1] + 1; j < 9; j++, i--)
                            {
                                if (i < 1) break;
                                if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (GL.figPosAr[i, j] != null) break;
                            }
                            i = position[0] - 1;
                            for (int j = position[1] - 1; j > 0; j--, i--)
                            {
                                if (i < 1) break;
                                if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j].color == fig.color) break;
                                ml.X.Add(i);
                                ml.Y.Add(j);
                                if (GL.figPosAr[i, j] != null) break;
                            }



                            for (int j = position[0] + 1; j < 9; j++)
                            {
                                if (GL.figPosAr[j, position[1]] != null && GL.figPosAr[j, position[1]].color == fig.color) { break; }
                                ml.X.Add(j);
                                ml.Y.Add(position[1]);
                                if (GL.figPosAr[j, position[1]] != null) { break; }
                            }
                            for (int j = position[0] - 1; j > 0; j--)
                            {
                                if (GL.figPosAr[j, position[1]] != null && GL.figPosAr[j, position[1]].color == fig.color) { break; }
                                ml.X.Add(j);
                                ml.Y.Add(position[1]);
                                if (GL.figPosAr[j, position[1]] != null) { break; }
                            }
                            for (int j = position[1] + 1; j < 9; j++)
                            {
                                if (GL.figPosAr[position[0], j] != null && GL.figPosAr[position[0], j].color == fig.color) { break; }
                                ml.X.Add(position[0]);
                                ml.Y.Add(j);
                                if (GL.figPosAr[position[0], j] != null) { break; }
                            }
                            for (int j = position[1] - 1; j > 0; j--)
                            {
                                if (GL.figPosAr[position[0], j] != null && GL.figPosAr[position[0], j].color == fig.color) { break; }
                                ml.X.Add(position[0]);
                                ml.Y.Add(j);
                                if (GL.figPosAr[position[0], j] != null) { break; }
                            }
                            break;
                        }
                    case "king":
                        {
                            for (int i = -1; i < 2; i++)
                                if (i + position[0] < 9 && i + position[0] > 0)
                                    for (int j = -1; j < 2; j++)
                                    {
                                        if (j + position[1] < 9 && j + position[1] > 0 && (GL.figPosAr[i + position[0], j + position[1]] == null || GL.figPosAr[i + position[0], j + position[1]].color != fig.color))
                                        {
                                            ml.X.Add(i + position[0]);
                                            ml.Y.Add(j + position[1]);
                                        }

                                    }
                            if (!(bool)fig.other)
                            {
                                IEnumerable<figure> list = GL.figLiveList.Where(figure => figure.color != fig.color);
                                if ((fig.color && !(bool)chess.white.castle[1].other) || (!fig.color && !(bool)chess.black.castle[1].other))
                                {
                                    bool ok = true;
                                    for (int i = position[0] + 1; i <= 7; i++)
                                    {
                                        if (GL.figPosAr[i, position[1]] != null) { ok = false; break; }
                                    }
                                    if (ok)
                                    {
                                        for (int i = position[0]; i <= 7; i++)
                                        {
                                            foreach (figure s in list)
                                                if (checkMoveToCheck(s, chess.getPos(fig, GL.grid), chess.getPos(s, GL.grid))) break;
                                            if (i == 7)
                                            {
                                                ml.X.Add(7); ml.Y.Add(position[1]);
                                            }
                                        }
                                    }
                                }
                                if ((fig.color && !(bool)chess.white.castle[0].other) || (!fig.color && !(bool)chess.black.castle[0].other))
                                {
                                    bool ok = true;
                                    for (int i = position[0] - 1; i >= 3; i--)
                                    {
                                        if (GL.figPosAr[i, position[1]] != null) { ok = false; break; }
                                    }
                                    if (ok)
                                    {
                                        for (int i = position[0]; i >= 3; i--)
                                        {
                                            foreach (figure s in list)
                                                if (checkMoveToCheck(s, chess.getPos(fig, GL.grid), chess.getPos(s, GL.grid))) break;
                                            if (i == 3)
                                            {
                                                ml.X.Add(3); ml.Y.Add(position[1]);
                                            }
                                        }
                                    }
                                }
                                //TODO
                                //if ((bool)chess.white.castle[0].other) { ml.X.Add(3); ml.Y.Add(8); }
                                //if ((bool)chess.white.castle[1].other) { ml.X.Add(7); ml.Y.Add(8); }
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
            if (GL.figPosAr[coord[0], coord[1]].color == fig.color)
                return false;
            switch (fig.type)
            {
                case "pawn":
                    switch (fig.color)
                    {
                        case true:
                            {
                                if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 && GL.figPosAr[coord[0], coord[1]] != null)) return true;
                                //if (((coord[0] == position[0] - 1 || coord[0] == position[0] + 1) && coord[1] == position[1] - 1 
                                //    && GL.figPosAr[coord[0], coord[1]] != null && !GL.figPosAr[coord[0], coord[1]].color) 
                                //    || (coord[0] == position[0] && GL.figPosAr[coord[0], coord[1]] == null && coord[1] == position[1] - 1) || (position[1] == 7 && coord[1] == position[1] - 2)) ok = true;
                                break;
                            }

                        case false:
                            {
                                if (((coord[0] == position[0] + 1 || coord[0] == position[0] - 1) && coord[1] == position[1] + 1 && GL.figPosAr[coord[0], coord[1]] != null)) return true;
                                break;
                            }
                    }
                    break;
                case "bishop":
                    {
                        if ((coord[1] + coord[0] - position[0] != position[1] && coord[1] - coord[0] + position[0] != position[1])) return false;
                        int i = position[0];
                        if (coord[0] > position[0])
                        {
                            if (coord[1] > position[1])
                                for (int j = position[1]; j < 9; j++, i++)
                                {
                                    if (coord[0] == i && coord[1] == j) { return true; }
                                    if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j] != fig) { return false; }
                                }
                            else
                                for (int j = position[1]; j > 0; j--, i++)
                                {
                                    if (coord[0] == i && coord[1] == j) { return true; }
                                    if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j] != fig) { return false; }
                                }
                        }
                        else
                        {
                            if (coord[1] > position[1])
                                for (int j = position[1]; j < 9; j++, i--)
                                {
                                    if (coord[0] == i && coord[1] == j) { return true; }
                                    if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j] != fig) { return false; }
                                }
                            else
                                for (int j = position[1]; j > 0; j--, i--)
                                {
                                    if (coord[0] == i && coord[1] == j) { return true; }
                                    if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j] != fig) { return false; }
                                }
                        }
                        break;
                    }
                case "knight":
                    {
                        if ((((coord[0] == position[0] + 1 || coord[0] == position[0] - 1) && (coord[1] == position[1] + 2 || coord[1] == position[1] - 2))
                            || ((coord[0] == position[0] + 2 || coord[0] == position[0] - 2) && (coord[1] == position[1] + 1 || coord[1] == position[1] - 1)))
                            ) return true;
                        break;
                    }
                case "castle":
                    {
                        if ((coord[1] != position[1] && coord[0] != position[0])) return false;
                        if (coord[0] > position[0])
                        {
                            for (int j = position[0]; j < 9; j++)
                            {
                                if (coord[0] == j) { return true; }
                                if (GL.figPosAr[j, position[1]] != null && GL.figPosAr[j, position[1]] != fig) { return false; }
                            }
                        }
                        else if (coord[0] < position[0])
                        {
                            for (int j = position[0]; j > 0; j--)
                            {
                                if (coord[0] == j) { return true; }
                                if (GL.figPosAr[j, position[1]] != null && GL.figPosAr[j, position[1]] != fig) { return false; }
                            }
                        }
                        else if (coord[1] > position[1])
                        {
                            for (int j = position[1]; j < 9; j++)
                            {
                                if (coord[1] == j) { return true; }
                                if (GL.figPosAr[position[0], j] != null && GL.figPosAr[position[0], j] != fig) { return false; }
                            }
                        }
                        //if(coord[1] < position[1])
                        else
                        {
                            for (int j = position[1]; j > 0; j--)
                            {
                                if (coord[1] == j) { return true; }
                                if (GL.figPosAr[position[0], j] != null && GL.figPosAr[position[0], j] != fig) { return false; }
                            }
                        }
                        break;
                    }
                case "queen":
                    {
                        if ((coord[1] != position[1] && coord[0] != position[0]) && (coord[1] + coord[0] - position[0] != position[1] && coord[1] - coord[0] + position[0] != position[1])) return false;
                        int i = position[0];
                        if (coord[0] > position[0])
                        {
                            if (coord[1] > position[1])
                                for (int j = position[1]; j < 9; j++, i++)
                                {
                                    if (coord[0] == i && coord[1] == j) { return true; }
                                    if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j] != fig) { return false; }
                                }
                            else if (coord[1] < position[1])
                                for (int j = position[1]; j > 0; j--, i++)
                                {
                                    if (coord[0] == i && coord[1] == j) { return true; }
                                    if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j] != fig) { return false; }
                                }
                            else
                                for (int j = position[0]; j < 9; j++)
                                {
                                    if (coord[0] == j) { return true; }
                                    if (GL.figPosAr[j, position[1]] != null && GL.figPosAr[j, position[1]] != fig) { return false; }
                                }
                        }
                        else if (coord[0] < position[0])
                        {
                            if (coord[1] > position[1])
                                for (int j = position[1]; j < 9; j++, i--)
                                {
                                    if (coord[0] == i && coord[1] == j) { return true; }
                                    if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j] != fig) { return false; }
                                }
                            else if (coord[1] < position[1])
                                for (int j = position[1]; j > 0; j--, i--)
                                {
                                    if (coord[0] == i && coord[1] == j) { return true; }
                                    if (GL.figPosAr[i, j] != null && GL.figPosAr[i, j] != fig) { return false; }
                                }
                            else for (int j = position[0]; j > 0; j--)
                                {
                                    if (coord[0] == j) { return true; }
                                    if (GL.figPosAr[j, position[1]] != null && GL.figPosAr[j, position[1]] != fig) { return false; }
                                }
                        }
                        else if (coord[1] > position[1])
                        {
                            for (int j = position[1]; j < 9; j++)
                            {
                                if (coord[1] == j) { return true; }
                                if (GL.figPosAr[position[0], j] != null && GL.figPosAr[position[0], j] != fig) { return false; }
                            }
                        }
                        //if(coord[1] < position[1])
                        else
                        {
                            for (int j = position[1]; j > 0; j--)
                            {
                                if (coord[1] == j) { return true; }
                                if (GL.figPosAr[position[0], j] != null && GL.figPosAr[position[0], j] != fig) { return false; }
                            }
                        }
                        break;
                    }
                case "king":
                    {
                        if (-2 < coord[0] - position[0] && coord[0] - position[0] < 2 && -2 < coord[1] - position[1] && coord[1] - position[1] < 2) { return true; }
                        break;
                    }
            }
            return false;
        }

    }
}
