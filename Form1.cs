using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace пасьянс
{
    public partial class Form1 : Form
    {
        const string helpfile = @"help\mod3.chm";
        const int ROW_COUNT = 2;
        const int COL_COUNT = 9;
        const int LEFT = 40;
        const int RANK2_ROW = 0;
        const int BOTTOM_ROW = 1;
        const int TOP = 50;
        const int CARD_WIDTH = 72;
        const int CARD_HEIGHT = 100;
        const int H_SHIFT = CARD_WIDTH + 10;
        const int V_SHIFT = CARD_HEIGHT + 30;
        const int MIN_DIST = 30;
        const int MAX_DIST = 24;

        private bool newGame;
        private Form2 aboutFrm = new Form2();
        private Form3 helpFrm = new Form3();

        private Pile[,] Piles;

        private Deck deck;
        private Undo undoList;

        private bool drag;
        private bool view;
        private Graphics grf;
        private SolidBrush backBrush;

        private Card dragCard;
        private int dragX, dragY;
        private int deltaX, deltaY;
        private int dragRow = 0;
        private int dragCol = 0;
        private int dropRow = 0;
        private int dropCol = 0;
        private int dragPos = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void ShowAll(Graphics g)
        {
            int dy = 0;
            // отображение стопок карт
            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int col = 0; col < COL_COUNT; col++)
                {
                    if (Piles[row, col].Count == 0)
                    {
                        if (col < 4 || col > 6)
                        {
                            imageList1.Draw(g, Piles[0, col].X, Piles[0, col].Y, 54);
                        }
                    }

                    if (Piles[row, col].Count > 0)
                    {
                        for (int pos = 0; pos < Piles[row, col].Count; pos++)
                        {
                            if (view && row == dragRow && col == dragCol)
                            {
                                imageList1.Draw(g, Piles[row, col].X, Piles[row, col].Y + dy, Piles[row, col][pos].ImageIndex); // рисуем карту                              
                                if (pos == dragPos)
                                {
                                    dy += MAX_DIST;
                                }
                                else
                                {
                                    dy += MIN_DIST;
                                }
                            }
                            else
                            {
                                if (row == 1)
                                {
                                    imageList1.Draw(g, Piles[row, col].X, Piles[row, col].Y + pos * MIN_DIST, Piles[row, col][pos].ImageIndex); // рисуем карту
                                } else if (row == 0)
                                {
                                    imageList1.Draw(g, Piles[row, col].X, Piles[row, col].Y, Piles[row, col][pos].ImageIndex);
                                }
                            }
                        }
                    }
                }
            }


            // прорисовка перемещаемой карты
            if (drag)
            {
                imageList1.Draw(g, dragX, dragY, dragCard.ImageIndex);
            }
        }



        private void NewGame()
        {
            newGame = false;
            grf = this.CreateGraphics();
            this.Refresh();

            // создаем список ходов для возможности отмены 
            undoList = new Undo();

            // создаем колоду 
            deck = new Deck();
            // перемешиваем колоду
            deck.Shuffle();

            // создаем стопки карт          
            Piles = new Pile[ROW_COUNT, COL_COUNT];
            backBrush = new SolidBrush(this.BackColor);
            newGame = true;

            // инициализируем стопки

            Card card = new Card(0, 0);
            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int col = 0; col < COL_COUNT; col++)
                {
                    Piles[row, col] = new Pile();
                    Piles[row, col].X = LEFT + H_SHIFT * col;
                    Piles[row, col].Y = TOP + V_SHIFT * row;
                    if (row == 1)
                    {
                        card = deck.GetCard();
                        Piles[row, col].AddCard(card);
                        card = deck.GetCard();
                        Piles[row, col].AddCard(card);
                        card = deck.GetCard();
                        Piles[row, col].AddCard(card);
                        card = deck.GetCard();
                        Piles[row, col].AddCard(card);
                        card = deck.GetCard();
                        Piles[row, col].AddCard(card);

                    }
                }
            }
            deck.AddCard(new Card(4, 1));
            deck.AddCard(new Card(4, 0));
            deck.Shuffle();
            for (int col = 0; col < COL_COUNT; col++)
            {
                card = deck.GetCard();
                {
                    Piles[1, col].AddCard(card);
                }
            }

            ShowAll(grf);

        }

        private Card CaptureCard(int x, int y)
        {
            Card card = null;
            for (int col = 0; col < COL_COUNT; col++)
            {
                if (x >= Piles[0, col].X && x <= Piles[0, col].X + CARD_WIDTH)  // попали в стопку
                {
                    dragCol = col;
                    // перемещать можно только последнюю карту стопки
                    for (int row = 0; row < ROW_COUNT; row++)
                    {
                        if (Piles[row, col].Count > 0)
                        {
                            if (row == 1)
                            {
                                if (y >= Piles[row, col].Y + (Piles[row, col].Count - 1) * MIN_DIST
                                  && y <= Piles[row, col].Y + (Piles[row, col].Count - 1) * MIN_DIST + CARD_HEIGHT)
                                {
                                    dragRow = row;
                                    dragX = Piles[row, col].X;
                                    dragY = Piles[row, col].Y + (Piles[row, col].Count - 1) * MIN_DIST;
                                    card = Piles[row, col].GetCard();
                                    return card;
                                }
                            }
                            else if (row == 0)
                            {
                                if (y >= Piles[row, col].Y
                                && y <= Piles[row, col].Y + CARD_HEIGHT)
                                {
                                    dragRow = row;
                                    dragX = Piles[row, col].X;
                                    dragY = Piles[row, col].Y;
                                    card = Piles[row, col].GetCard();
                                    return card;
                                }
                            }
                        }
                    }
                }
            }
            return card;
        }

        private bool Intersected(int x1, int y1, int x2, int y2)
        {
            if (((x2 <= x1 && x1 <= x2 + 40) || (x1 <= x2 && x2 <= x1 + CARD_WIDTH)) &&
               ((y2 <= y1 && y1 <= y2 + CARD_HEIGHT) || (y1 <= y2 && y2 <= y1 + CARD_HEIGHT)))
            {
                return true;
            }
            return false;
        }

        private bool CanDrop(int x, int y)
        {
            Card card;

            if (drag)
            {
                for (int row = 0; row < ROW_COUNT; row++)
                {
                    for (int col = 0; col < COL_COUNT; col++)
                    {
                        if (Piles[row, col].Count > 0) // в стопке есть карты
                        {
                            if (row == 1)
                            {
                                card = Piles[row, col][Piles[row, col].Count - 1];
                                if (Intersected(x, y, Piles[row, col].X, Piles[row, col].Y + (Piles[row, col].Count - 1) * MIN_DIST))
                                {
                                    dropRow = row;
                                    dropCol = col;
                                    if (dragCard.Suit == 4 && card.Suit != 4)
                                    {
                                        return true;
                                    }
                                    if (card.Suit == 4 && dragCard.Suit != 4)
                                    {
                                        return true;
                                    }
                                    if ((dragCard.Suit % 2 != card.Suit % 2) && (dragCard.Rank == (card.Rank - 1)))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else if (row == 0 && col < 4)
                            {
                                card = Piles[row, col][Piles[row, col].Count - 1];
                                if (Intersected(x, y, Piles[row, col].X, Piles[row, col].Y))
                                {
                                    dropRow = row;
                                    dropCol = col;
                                    if ((dragCard.Suit == card.Suit) && (dragCard.Rank == (card.Rank + 1)))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        else  // попали на пустую стопку
                        {
                            if (row == 1)
                            {
                                if (Intersected(x, y, Piles[row, col].X, Piles[row, col].Y + (Piles[row, col].Count - 1) * MIN_DIST)) // проверяем попадание на пустую стопку
                                {
                                    dropRow = row;
                                    dropCol = col;
                                    if (dragCard.Suit != 4)
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (row != BOTTOM_ROW)
                            {
                                if (Intersected(x, y + MIN_DIST, Piles[row, col].X, Piles[row, col].Y + MIN_DIST)) // проверяем попадание на пустую стопку
                                {
                                    dropRow = row;
                                    dropCol = col;
                                    if ((dragCard.Rank == 0 && dropCol < 4 && dragCard.Suit < 4) || (dragCard.Suit == 4 && dropCol > 6))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool IsVictory()
        {
            int end = 0;
            for (int i = 0; i < COL_COUNT; i++)
            {
                if (Piles[BOTTOM_ROW, i].Count == 0)
                    end += 1;
            }
            if (end == 9)
            {
                return true;
            }
            else
                return false;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (newGame == true)
            {
                if (e.Button == MouseButtons.Left)
                {

                    dragCard = CaptureCard(e.X, e.Y); // перемещаемая карта
                    if (dragCard != null)
                    {
                        deltaX = e.X - dragX;
                        deltaY = e.Y - dragY;
                        drag = true;
                        // ShowAll(grf);
                    }

                }
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (newGame == true)
            {
                if (drag)
                {
                    if (CanDrop(dragX, dragY))
                    {
                        Piles[dropRow, dropCol].AddCard(dragCard);  // перемещаем на новое место
                        if (dropRow != dragRow || dragCol != dropCol)
                        {
                            undoList.Save(Piles[dragRow, dragCol], Piles[dropRow, dropCol]);  // сохраняем ход для истории
                            отменитьХодToolStripMenuItem.Enabled = true;
                        }
                    }
                    else
                    {
                        Piles[dragRow, dragCol].AddCard(dragCard);  // возвращаем на старое место
                    }
                }
                dragCard = null;
                dragRow = 0;
                dragPos = 0;
                dragCol = 0;
                dropRow = 0;
                dropCol = 0;
                dragX = 0;
                dragY = 0;
                drag = false;
                view = false;

                if (IsVictory()) MessageBox.Show("Вы выиграли!", "Джокер-пасьянс6х9");
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                dragX = e.X - deltaX;
                dragY = e.Y - deltaY;
            }
            this.Invalidate();
        }

    
        //перерисовка формы
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (newGame) ShowAll(e.Graphics);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void играToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void новаяИграToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
        }

        private void отменитьХодToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (newGame)
            {
                if (undoList.Count > 0)
                {

                    if (undoList[undoList.Count - 1].From != deck)
                    {
                        undoList.Restore();
                    }
                    else
                    {
                        while (undoList.Count > 0 && undoList[undoList.Count - 1].From == deck)
                        {
                            undoList.Restore();
                        }
                    }
                    for (int i = 0; i < COL_COUNT; i++)
                    {
                        if (Piles[BOTTOM_ROW, i].Count == 0) undoList.Restore();
                    }
                    if (undoList.Count == 0) отменитьХодToolStripMenuItem.Enabled = false;
                    this.Invalidate();
                }
            }
        }

        private void закончитьИгруToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
        }

        private void разработчикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 newfrm = new Form3();
            newfrm.Show();
        }

        private void правилаИгрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 newfrm = new Form2();
            newfrm.Show();
        }
    }
}
