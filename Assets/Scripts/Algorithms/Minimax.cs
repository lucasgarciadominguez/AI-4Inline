using ConnectFour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;



public class Minimax : MonoBehaviour
{
    //public class ScoringMove
    //{
    //    public int score;
    //    public int move;
    //    public ScoringMove(int score, int move)
    //    {
    //        this.score = score;
    //        this.move = move;
    //    }
    //}
    //public class AlgorithmStatistics
    //{
    //    public int ExpandedNodes { get; set; }
    //    public TimeSpan ExecutionTime { get; set; }

    //    public double ExecutionTimeMilliseconds
    //    {
    //        get { return ExecutionTime.TotalMilliseconds; }
    //    }
    //}
    public ScoringMove MinimaxAlgorithmIntegrated(Field field, int depth, bool maximizingPlayer, AlgorithmStatistics statistics)
    {
        statistics.ExpandedNodes++;

        List<int> possibleDrops = field.GetPossibleDrops();

        if (depth == 0)
        {
            return new ScoringMove(EvaluateField(field), 0);
        }
        if (field.CheckForWinner() || field.CheckForVictory())
        {
            return new ScoringMove(EvaluateField(field), 0);
        }

        if (maximizingPlayer)
        {
            int bestScore = int.MinValue;
            int column = 1;
            foreach (int col in possibleDrops)
            {
                Field clonedField = field.Clone();
                clonedField.DropInColumn(col, 2);

                ScoringMove newsScore = MinimaxAlgorithmIntegrated(clonedField, depth - 1, false, statistics);
                if (newsScore.score > bestScore)
                {
                    bestScore = newsScore.score;
                    column = col;
                }
            }

            return new ScoringMove(bestScore, column);
        }
        else
        {
            int bestScore = int.MaxValue;
            int column = 1;

            foreach (int col in possibleDrops)
            {
                Field clonedField = field.Clone();
                clonedField.DropInColumn(col, 1);

                ScoringMove score = MinimaxAlgorithmIntegrated(clonedField, depth - 1, true, statistics);
                if (score.score < bestScore)
                {
                    bestScore = score.score;
                    column = col;
                }
            }

            return new ScoringMove(bestScore, column);
        }
    }
    public ScoringMove MinimaxAlgorithmABIntegrated(Field field, int depth,int alpha,int beta, bool maximizingPlayer, AlgorithmStatistics statistics)
    {
        statistics.ExpandedNodes++;

        List<int> possibleDrops = field.GetPossibleDrops();

        if (depth == 0)
        {
            return new ScoringMove(EvaluateField(field), 0);
        }
        if (field.CheckForWinner() || field.CheckForVictory())
        {
            return new ScoringMove(EvaluateField(field), 0);
        }

        if (maximizingPlayer)
        {
            int bestScore = int.MinValue;
            int column = 1;
            foreach (int col in possibleDrops)
            {
                Field clonedField = field.Clone();
                clonedField.DropInColumn(col, 2);

                ScoringMove newsScore = MinimaxAlgorithmIntegrated(clonedField, depth - 1, false, statistics);
                if (newsScore.score > bestScore)
                {
                    bestScore = newsScore.score;
                    column = col;
                }
                // Poda alfa-beta
                alpha = Math.Max(alpha, bestScore);
                if (beta <= alpha)
                {
                    break;
                }
            }

            return new ScoringMove(bestScore, column);
        }
        else
        {
            int bestScore = int.MaxValue;
            int column = 1;

            foreach (int col in possibleDrops)
            {
                Field clonedField = field.Clone();
                clonedField.DropInColumn(col, 1);

                ScoringMove score = MinimaxAlgorithmIntegrated(clonedField, depth - 1, true, statistics);
                if (score.score < bestScore)
                {
                    bestScore = score.score;
                    column = col;
                }
                // Poda alfa-beta
                beta = Math.Min(beta, bestScore);
                if (beta <= alpha)
                {
                    break;
                }
            }

            return new ScoringMove(bestScore, column);
        }
    }
    public (ScoringMove move, AlgorithmStatistics statistics) RunAlgorithm(Field field, int depth, bool maximizingPlayer)
    {
        AlgorithmStatistics statistics = new AlgorithmStatistics();
        Stopwatch stopwatch = new Stopwatch();

        // Iniciar temporizador
        stopwatch.Start();

        // Ejecutar el algoritmo y obtener estadísticas
        ScoringMove result = MinimaxAlgorithmIntegrated(field, depth, maximizingPlayer, statistics);

        // Detener temporizador
        stopwatch.Stop();

        // Guardar estadísticas
        statistics.ExecutionTime = stopwatch.Elapsed;







        return (result, statistics);
    }
    public (ScoringMove move, AlgorithmStatistics statistics) RunAlgorithmAB(Field field, int depth,int alfa, int beta, bool maximizingPlayer)
    {
        AlgorithmStatistics statistics = new AlgorithmStatistics();
        Stopwatch stopwatch = new Stopwatch();
        
        // Iniciar temporizador
        stopwatch.Start();

        // Ejecutar el algoritmo y obtener estadísticas
        ScoringMove result = MinimaxAlgorithmABIntegrated(field, depth, alfa, beta, maximizingPlayer, statistics);

        // Detener temporizador
        stopwatch.Stop();

        // Guardar estadísticas
            statistics.ExecutionTime = stopwatch.Elapsed;

            
        
        



        return (result, statistics);
    }
    private int EvaluateField(Field field)
    {
        if (field.field[field.dropColumn, field.dropRow] == 1)
        {
            if (field.CheckForWinner())
            {
                return -4;
            }
            else if (field.ReturnLastColourPlayer() == 1)
            {
                return -field.CheckForVictoryNum();
            }
            else
            {
                return 0;
            }
        }
        else
        {
            if (field.CheckForWinner())
            {
                return 4;
            }
            else if (field.ReturnLastColourPlayer() == 2)
            {
                return field.CheckForVictoryNum();
            }
            else
            {
                return 0;
            }
        }

    }

}

