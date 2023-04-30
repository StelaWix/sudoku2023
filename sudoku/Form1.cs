using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Common;
using System.Collections;


namespace sudoku
{
    public partial class Form1 : Form
    {
        /// <summary>
        public static string connectString = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source =бд.mdb";
        /// </summary>
        private OleDbConnection myConnection;
        System.Timers.Timer timer;
        int h;
        int m;
        int s;
        int TimeRaiting;
        string LevelGame;
        public Form1()
        {
            InitializeComponent();
            
            createCells();
            startNewGame();
            
            
            myConnection = new OleDbConnection(connectString);
            myConnection.Open();

        }

        SudokuCell[,] cells = new SudokuCell[9, 9];

        private void Form1_Load(object sender, EventArgs e)
        {
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += OnTimeEvent;
        }
        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                s += 1;
                if (s == 60)
                {
                    s = 0;
                    m += 1;
                }
                if (m == 60)
                {
                    m = 0;
                    h += 1;
                }
                TimeRaiting = s + (m * 60) + (h * 60);
                textBox1.Text = string.Format("{0}:{1}:{2}", h.ToString().PadLeft(2, '0'), m.ToString().PadLeft(2, '0'), s.ToString().PadLeft(2, '0'));
            }));
        }
        private void createCells()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                { 
                    cells[i, j] = new SudokuCell();
                    cells[i, j].Font = new Font(SystemFonts.DefaultFont.FontFamily, 20);
                    cells[i, j].Size = new Size(40, 40);
                    cells[i, j].ForeColor = SystemColors.ControlDarkDark;
                    cells[i, j].Location = new Point(i * 40, j * 40);
                    cells[i, j].BackColor = ((i / 3) + (j / 3)) % 2 == 0 ? SystemColors.Control : Color.LightGray;
                    cells[i, j].FlatStyle = FlatStyle.Flat;
                    cells[i, j].FlatAppearance.BorderColor = Color.Black;
                    cells[i, j].X = i;
                    cells[i, j].Y = j;

                    cells[i, j].KeyPress += cell_keyPressed;

                    SudokuCell.Controls.Add(cells[i, j]);
                }
            }
        }

        private void cell_keyPressed(object sender, KeyPressEventArgs e)
        {
            var cell = sender as SudokuCell;

            if (cell.IsLocked)
                return;

            int value;

            if (int.TryParse(e.KeyChar.ToString(), out value))
            {
                if (value == 0)
                    cell.Clear();
                else
                    cell.Text = value.ToString();

                cell.ForeColor = SystemColors.ControlDarkDark;
            }
        }

        private void startNewGame()
        {
            loadValues();
            s = 0;
            m = 0;
            h = 0;
            var hintsCount = 0;

            if (beginnerLevel.Checked)
                hintsCount = 45;
            else if (IntermediateLevel.Checked)
                hintsCount = 30;
            else if (AdvancedLevel.Checked)
                hintsCount = 15;

            showRandomValuesHints(hintsCount);
        }

        private void showRandomValuesHints(int hintsCount)
        {
            // Показывать значение в случайных ячейках
            // Количество подсказок зависит от выбранного игроком уровня
            for (int i = 0; i < hintsCount; i++)
            {
                var rX = random.Next(9);
                var rY = random.Next(9);

                // Оформите ячейки с подсказками по-другому и
                // заблокируйте ячейку, чтобы игрок не мог отредактировать значение
                cells[rX, rY].Text = cells[rX, rY].Value.ToString();
                cells[rX, rY].ForeColor = Color.Black;
                cells[rX, rY].IsLocked = true;
            }
        }

        private void loadValues()
        {
           
            foreach (var cell in cells)
            {
                cell.Value = 0;
                cell.Clear();
            }

          
            findValueForNextCell(0, -1);
        }

        Random random = new Random();

        private bool findValueForNextCell(int i, int j)
        {
           
            if (++j > 8)
            {
                j = 0;

                if (++i > 8)
                    return true;
            }

            var value = 0;
            var numsLeft = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        
            do
            {
                if (numsLeft.Count < 1)
                {
                    cells[i, j].Value = 0;
                    
                    return false;
                }

                value = numsLeft[random.Next(0, numsLeft.Count)];
                cells[i, j].Value = value;

                numsLeft.Remove(value);
            }
            while (!isValidNumber(value, i, j) || !findValueForNextCell(i, j));

            return true;
        }

        private bool isValidNumber(int value, int x, int y)
        {
            for (int i = 0; i < 9; i++)
            {
                
                if (i != y && cells[x, i].Value == value)
                    return false;

                if (i != x && cells[i, y].Value == value)
                    return false;
            }

            for (int i = x - (x % 3); i < x - (x % 3) + 3; i++)
            {
                for (int j = y - (y % 3); j < y - (y % 3) + 3; j++)
                {
                    if (i != x && j != y && cells[i, j].Value == value)
                        return false;
                }
            }

            return true;
        }

        private void checkButton_Click_1(object sender, EventArgs e)
        {
            var wrongCells = new List<SudokuCell>();

            foreach (var cell in cells)
            {
                if (!string.Equals(cell.Value.ToString(), cell.Text))
                {
                    wrongCells.Add(cell);
                }
            }

            

            if (wrongCells.Any())
            {
                wrongCells.ForEach(x => x.ForeColor = Color.Red);
                MessageBox.Show("Wrong inputs / неверно");
            }
            else
            {
                MessageBox.Show("You Wins / Победа");
                if (beginnerLevel.Checked)
                {
                    LevelGame = "Beginner";
                }
                if (IntermediateLevel.Checked)
                {
                    LevelGame = "Intermediate";
                }
                if (AdvancedLevel.Checked)
                {
                    LevelGame = "Advanced";
                }
                string query = "INSERT INTO'" + LevelGame + "'(s) VALUES('" + TimeRaiting + "')";

            }
            timer.Stop();
           
           

        }



 

        private void newGameButton_Click_1(object sender, EventArgs e)
        {
            startNewGame();
            timer.Start();
            if (beginnerLevel.Checked)
            {
                OleDbCommand command = new OleDbCommand("SELECT MIN(s) FROM [Beginner]", myConnection);
                command.ExecuteNonQuery();
                
                OleDbDataReader reader = command.ExecuteReader();
                
                label1.Text = "";

                while (reader.Read())
                {
                    label1.Text = reader[0].ToString()+" сек";
                }

          
                reader.Close();



            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
            Application.DoEvents(); 
        }

        private void button1_Click(object sender, EventArgs e)
        {

            decisionGame();
           
        }

        private void clearButton_Click_1(object sender, EventArgs e)
        {
            foreach (var cell in cells)
            {
                if (cell.IsLocked == false)
                    cell.Clear();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
        private void decisionGame()
        {
            // Показывать значение в случайных ячейках
            // Количество подсказок зависит от выбранного игроком уровня
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var rX = i;
                    var rY = j;



                    // Оформите ячейки с подсказками по-другому и
                    // заблокируйте ячейку, чтобы игрок не мог отредактировать значение
                    cells[rX, rY].Text = cells[rX, rY].Value.ToString();
                    cells[rX, rY].ForeColor = Color.Black;
                    cells[rX, rY].IsLocked = true;
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

