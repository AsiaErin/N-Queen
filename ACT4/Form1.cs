using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6;
        SixState startState;
        SixState currentState;
        int moveCounter;

        int[,] hTable;
        ArrayList bMoves;
        Object chosenMove;

        // Simulated Annealing variables
        double temperature = 100.0;
        double coolingRate = 0.99;
        double minTemperature = 0.01;
        Random random = new Random();

        public Form1()
        {
            InitializeComponent();

            side = pictureBox1.Width / n;

            startState = randomSixState();
            currentState = new SixState(startState);

            updateUI();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void updateUI()
        {
            pictureBox2.Refresh();

            label3.Text = "Attacking pairs: " + getAttackingPairs(currentState);
            label4.Text = "Moves: " + moveCounter;
            hTable = getHeuristicTableForPossibleMoves(currentState);
            bMoves = getBestMoves(hTable);

            listBox1.Items.Clear();
            foreach (Point move in bMoves)
            {
                listBox1.Items.Add(move);
            }

            if (bMoves.Count > 0)
                chosenMove = chooseMove(bMoves);
            label2.Text = "Chosen move: " + chosenMove;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    if (j == currentState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private SixState randomSixState()
        {
            return new SixState(random.Next(n),
                                random.Next(n),
                                random.Next(n),
                                random.Next(n),
                                random.Next(n),
                                random.Next(n));
        }

        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;

            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf + 1; tar < n; tar++)
                {
                    if (f.Y[rf] == f.Y[tar])
                        attackers++;

                    if (f.Y[tar] == f.Y[rf] + tar - rf)
                        attackers++;

                    if (f.Y[rf] == f.Y[tar] + tar - rf)
                        attackers++;
                }
            }

            return attackers;
        }

        private int[,] getHeuristicTableForPossibleMoves(SixState thisState)
        {
            int[,] hStates = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    SixState possible = new SixState(thisState);
                    possible.Y[i] = j;
                    hStates[i, j] = getAttackingPairs(possible);
                }
            }

            return hStates;
        }

        private ArrayList getBestMoves(int[,] heuristicTable)
        {
            ArrayList bestMoves = new ArrayList();
            int bestHeuristicValue = heuristicTable[0, 0];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (bestHeuristicValue > heuristicTable[i, j])
                    {
                        bestHeuristicValue = heuristicTable[i, j];
                        bestMoves.Clear();
                        if (currentState.Y[i] != j)
                            bestMoves.Add(new Point(i, j));
                    }
                    else if (bestHeuristicValue == heuristicTable[i, j])
                    {
                        if (currentState.Y[i] != j)
                            bestMoves.Add(new Point(i, j));
                    }
                }
            }
            label5.Text = "Possible Moves (H=" + bestHeuristicValue + ")";
            return bestMoves;
        }

        private Object chooseMove(ArrayList possibleMoves)
        {
            if (possibleMoves.Count == 0) return null;

            Point bestMove = (Point)possibleMoves[0];
            int bestHeuristic = getAttackingPairs(executeMovePreview(bestMove));

            foreach (Point move in possibleMoves)
            {
                SixState previewState = executeMovePreview(move);
                int currentHeuristic = getAttackingPairs(previewState);

                if (acceptMove(bestHeuristic, currentHeuristic))
                {
                    bestMove = move;
                    bestHeuristic = currentHeuristic;
                }
            }
            return bestMove;
        }

        private SixState executeMovePreview(Point move)
        {
            SixState previewState = new SixState(currentState);
            previewState.Y[move.X] = move.Y;
            return previewState;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private bool acceptMove(int currentHeuristic, int newHeuristic)
        {
            if (newHeuristic < currentHeuristic) return true;

            double acceptanceProbability = Math.Exp((currentHeuristic - newHeuristic) / temperature);
            return random.NextDouble() < acceptanceProbability;
        }

        private void executeMove(Point move)
        {
            currentState.Y[move.X] = move.Y;
            moveCounter++;
            chosenMove = null;
            temperature *= coolingRate;
            updateUI();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (getAttackingPairs(currentState) > 0 && chosenMove != null && temperature > minTemperature)
            {
                executeMove((Point)chosenMove);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startState = randomSixState();
            currentState = new SixState(startState);
            moveCounter = 0;
            temperature = 100.0;  // Reset temperature
            updateUI();
            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (getAttackingPairs(currentState) > 0 && temperature > minTemperature)
            {
                executeMove((Point)chosenMove);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

