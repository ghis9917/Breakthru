using BreakthruBoardModel.Constants;
using BreakthruBoardModel.Game;
using BreakthruBoardModel.Utils;
using ChessBoardGUIApp;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BreakthruGUIApp
{
    public partial class GameWindow : Form
    {
        public Button[,] btnGrid;

        public Game myGame;

        public int ClickStage;

        public Point PreviousClickedCell;

        public GameWindow()
        {
            InitializeComponent();

            CreateBoard();
            
            PopulateGrid();
        }

        private void CreateBoard()
        {
            //PopUp window with side request to determine human player
            Color SideChoice;
            PlayerChoice AskSide = new PlayerChoice();
            DialogResult choice = AskSide.ShowDialog();
            if (choice == DialogResult.Yes)
            {
                SideChoice = Color.Gold;
            }
            else if (choice == DialogResult.No)
            {
                SideChoice = Color.Silver;
            }
            else 
            {
                SideChoice = Color.Gold;
            }

            //Initialize needed objects
            myGame = new Game(Const.InitialConfiguration, SideChoice);
            btnGrid = new Button[Const.SIZE, Const.SIZE];

            ClickStage = Const.NOT_CLICKED;
            PreviousClickedCell = new Point(-1, -1);

            listView1.View = View.Details;
            listView1.HeaderStyle = ColumnHeaderStyle.None;
            listView1.Columns.Add(new ColumnHeader { Width = (listView1.ClientSize.Width - SystemInformation.VerticalScrollBarWidth) });

            listView2.View = View.Details;
            listView2.HeaderStyle = ColumnHeaderStyle.None;
            listView2.Columns.Add(new ColumnHeader { Width = (listView1.ClientSize.Width - SystemInformation.VerticalScrollBarWidth) });
        }

        //Fill the panel with buttons
        private void PopulateGrid()
        {
            int buttonSize = panel1.Width / Const.SIZE;

            for (int i = 0; i < Const.SIZE; i++)
            {
                for (int j = 0; j < Const.SIZE; j++)
                {

                    Color bgCellColor = Color.Black;

                    
                    if (myGame.Board.Grid[i, j] != 0)
                    {
                        bgCellColor = Utils.PieceToColor(myGame.Board.Grid[i, j]);
                        if (myGame.Board.Grid[i, j] == Const.FLAGSHIP)
                        {
                            bgCellColor = Color.Orange;
                        }
                    }

                    btnGrid[i, j] = new Button();
                    btnGrid[i, j].Height = buttonSize;
                    btnGrid[i, j].Width = buttonSize;

                    //add click event
                    btnGrid[i, j].Click += Grid_Button_Click;

                    //place the button inside the panel
                    panel1.Controls.Add(btnGrid[i, j]);

                    //set position of button inside the panel and add a label to the button
                    btnGrid[i, j].Location = new Point(i * buttonSize, j * buttonSize);

                    //set thicker border for central 5x5 square
                    btnGrid[i, j].BackColor = bgCellColor;
                    btnGrid[i, j].FlatStyle = FlatStyle.Flat;
                    btnGrid[i, j].FlatAppearance.BorderColor = Color.White;
                    if (j >= 3 && j <= 7 && i >= 3 && i <= 7)
                    {
                        btnGrid[i, j].FlatAppearance.BorderSize = 4;
                    }
                    else
                    {
                        btnGrid[i, j].FlatAppearance.BorderSize = 1;
                    }


                    //Set tag to recognize pressed button later
                    btnGrid[i, j].Tag = new Point(i, j);
                }
            }
        }

        //Function that handles the click on the buttons and implements the process of showing each time the modifications made by the game
        private void Grid_Button_Click(object sender, EventArgs e)
        {
            if (myGame.GameStatus == Const.GAME_ON)
            {
                if (myGame.Turn.Player != myGame.Human)
                {
                    myGame.AI();//Let AI play

                    AddMove();//Add move played into the listView

                    ReDrawBoard();//Refresh GUI

                    return;
                }

                //Get button pressed
                Button clickedButton = (Button)sender;
                Point clickedSquare = (Point)clickedButton.Tag;

                if (
                    PreviousClickedCell == new Point(-1, -1) && //No piece has been clicked yet
                    myGame.Board.Grid[clickedSquare.X, clickedSquare.Y] != 0 && //The square clicked contains a piece
                    Utils.PieceToColor(myGame.Board.Grid[clickedSquare.X, clickedSquare.Y]) == myGame.Human && //The piece selected it the same color as the one of the human player
                    myGame.Human == myGame.Turn.Player) //It's the turn of the human player
                {
                    PreviousClickedCell = myGame.GetLegalMoves(clickedSquare);
                }
                else if (PreviousClickedCell != new Point(-1, -1) && myGame.Human == myGame.Turn.Player)
                {
                    if (!(myGame.LegalMoves[clickedSquare.X, clickedSquare.Y] == Const.CAPTURE_MOVE) && !(myGame.LegalMoves[clickedSquare.X, clickedSquare.Y] == Const.LEGAL_MOVE)) //The player makes a non-legal move
                    {
                        if (myGame.Board.Grid[clickedSquare.X, clickedSquare.Y] != 0 && Utils.PieceToColor(myGame.Board.Grid[clickedSquare.X, clickedSquare.Y]) == myGame.Human)
                        {
                            myGame.CleanLegalMoves();
                            PreviousClickedCell = myGame.GetLegalMoves(clickedSquare);
                        }
                        else {
                            myGame.CleanLegalMoves();
                            PreviousClickedCell = new Point(-1, -1); //go back to unclicked state for this ply
                        }
                    } else //The player makes a legal move
                    {
                        myGame.MakeMove(new Move(PreviousClickedCell, clickedSquare, myGame.Board.Grid[PreviousClickedCell.X, PreviousClickedCell.Y], myGame.Board.Grid[clickedSquare.X, clickedSquare.Y]));
                        AddMove();
                        button1.Enabled = false;
                        PreviousClickedCell = new Point(-1, -1);
                    }
                } else {
                    PreviousClickedCell = new Point(-1, -1);
                }

                //redraw colors to make changes visible
                ReDrawBoard();

                
            }
        }


        public void ReDrawBoard() {

            for (int i = 0; i < Const.SIZE; i++)
            {
                for (int j = 0; j < Const.SIZE; j++)
                {
                    if (myGame.Board.Grid[i, j] != Const.EMPTY_SQUARE)
                    {
                        btnGrid[i, j].BackColor = Utils.PieceToColor(myGame.Board.Grid[i, j]);
                        if (myGame.Board.Grid[i, j] == Const.FLAGSHIP)
                        {
                            btnGrid[i, j].BackColor = Color.Orange;
                        }
                    }
                    else
                    {
                        btnGrid[i, j].BackColor = Color.Black;
                    }

                    //Highlight legal moves and possible captures 
                    if (myGame.LegalMoves[i, j] == Const.LEGAL_MOVE)
                    {
                        btnGrid[i, j].BackColor = Color.Green;
                    }
                    if (myGame.LegalMoves[i, j] == Const.CAPTURE_MOVE)
                    {
                        btnGrid[i, j].BackColor = Color.Red;
                    }
                    
                }
            }
            if (myGame.GameStatus == Const.GAME_ON && myGame.Turn.Player != myGame.Human)
            {
                Refresh();
                myGame.AI();
                AddMove();
                ReDrawBoard();
            }
        }

        private void AddMove()
        {
            if (myGame.MovesHistory.Count > 0)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = myGame.MovesHistory.Last().Item1.ToString();
                if (Utils.PieceToColor(myGame.MovesHistory.Last().Item1.FromPiece) == Color.Gold)
                {
                        listView2.Items.Add(lvi);
                        listView1.Items.Add(new ListViewItem(""));
                }
                else if (Utils.PieceToColor(myGame.MovesHistory.Last().Item1.FromPiece) == Color.Silver)
                {
                        listView1.Items.Add(lvi);
                        listView2.Items.Add(new ListViewItem(""));
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (myGame.Turn.FirstMoveOfTheGame && myGame.Human == Color.Gold) {
                myGame.Turn.SetPlyCounter(0);
                myGame.Turn.Player = Color.Silver;
                myGame.Turn.FirstMoveOfTheGame = false;
                button1.Enabled = false;
                ReDrawBoard();
            }
        }
    }
}
