using ConnectFour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Debug = UnityEngine.Debug;

public class Negamax : MonoBehaviour
{
    int previousScore;
    int windowRange;



    public ScoringMove NegamaxAlgorithm(Field board, int depth, AlgorithmStatistics statistics)
    {
        statistics.ExpandedNodes++;
        byte bestMove = 0;
        int bestScore = 0;

        int currentScore;

        ScoringMove scoringMove;
        Field newBoard;

        // Comprobar si hemos terminado de hacer recursión, por 2 posibles motivos:
        //  1. hemos llegado a una jugada terminal.
        //  2. hemos alcanzado la máxima profundidad que nos permite nuestra inteligencia.
        if (depth == 4 || board.CheckForWinner() || board.CheckForVictory())
        {
            // En los niveles impares el valor de la evaluación se invierte, para ajustarlo al comportamiento de negamax
            if (depth % 2 == 0)
            {
                scoringMove = new ScoringMove(EvaluateField(board), 0);
            }
            else
            {
                scoringMove = new ScoringMove(-EvaluateField(board), 0);
            }
        }


        else
        {
            bestScore = int.MinValue;

            List<int> possibleMoves;
            possibleMoves = board.GetPossibleDrops();

            foreach (byte move in possibleMoves)
            {
                newBoard = board.Clone();
                if (board.playerTurn==1)
                {
                    newBoard.DropInColumn(move, 2);
                }
                else
                {
                    newBoard.DropInColumn(move, 1);

                }

                // Recursividad
                scoringMove = NegamaxAlgorithm(newBoard, depth + 1, statistics);


                // Invertimos el signo para que, en cada nivel de profundidad, haga efecto el “nega” de Negamax
                currentScore = -scoringMove.score;

                // Actualizar mejor score si obtenemos una jugada mejor.
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestMove = move;
                }
            }
            scoringMove = new ScoringMove(bestScore, bestMove);
        }
        return scoringMove;
    }

    public (ScoringMove move, AlgorithmStatistics statistics) RunAlgorithmNega(Field board, int depth)
    {
        AlgorithmStatistics statistics = new AlgorithmStatistics();
        Stopwatch stopwatch = new Stopwatch();

        // Iniciar temporizador
        stopwatch.Start();

        // Ejecutar el algoritmo y obtener estadísticas
        ScoringMove result = NegamaxAlgorithm(board, depth,statistics);

        // Detener temporizador
        stopwatch.Stop();

        // Guardar estadísticas
        statistics.ExecutionTime = stopwatch.Elapsed;







        return (result, statistics);
    }
    public (ScoringMove move, AlgorithmStatistics statistics) RunAlgorithmApirational(Field board)
    {
        AlgorithmStatistics statistics = new AlgorithmStatistics();
        Stopwatch stopwatch = new Stopwatch();

        // Iniciar temporizador
        stopwatch.Start();

        // Ejecutar el algoritmo y obtener estadísticas
        ScoringMove result = AspirationSearch( board,statistics);

        // Detener temporizador
        stopwatch.Stop();

        // Guardar estadísticas
        statistics.ExecutionTime = stopwatch.Elapsed;







        return (result, statistics);
    }
    public (ScoringMove move, AlgorithmStatistics statistics) RunAlgorithmNegaAB(Field board, int depth, int alfa, int beta)
    {
        AlgorithmStatistics statistics = new AlgorithmStatistics();
        Stopwatch stopwatch = new Stopwatch();

        // Iniciar temporizador
        stopwatch.Start();

        // Ejecutar el algoritmo y obtener estadísticas
        ScoringMove result = NegamaxAB( board,  depth,  alfa,  beta, statistics);

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


    // En la primera llamada a esta función, usar alfa igual a MINUS_INFINITE, y beta INFINITE. 
    public ScoringMove NegamaxAB(Field board, int depth, int alpha, int beta, AlgorithmStatistics statistics)
    {
        statistics.ExpandedNodes++;
        int bestMove = -1; // Cambiado a -1 para indicar que aún no se ha seleccionado ningún movimiento
        int bestScore = int.MinValue;
        ScoringMove scoringMove;

        // Comprobar si hemos terminado la recursión debido a una jugada terminal o alcanzado la máxima profundidad
        if (depth == 4 || board.CheckForWinner() || board.CheckForVictory())
        {
            // En los niveles impares el valor de la evaluación se invierte, para ajustarlo al comportamiento de negamax
            if (depth % 2 == 0)
            {
                return new ScoringMove(EvaluateField(board), 0);
            }
            else
            {
                return new ScoringMove(-EvaluateField(board), 0);
            }
        }
        else
        {
            List<int> possibleMoves = board.GetPossibleDrops();

            foreach (int move in possibleMoves)
            {
                Field newBoard = board.Clone();
                newBoard.DropInColumn(move, depth % 2 == 0 ? 2 : 1);

                // Recursividad. Invertimos alfa y beta, y sus signos, al igual que ya hacemos en negamax con el score, 
                // para alternar el comportamiento en los niveles Min y Max.
                scoringMove = NegamaxAB(newBoard, depth + 1, -beta, -alpha, statistics);

                int currentScore = -scoringMove.score;

                // Actualizar mejor score y mejor movimiento si obtenemos una jugada mejor.
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestMove = move;
                }
                if (depth%2==0)
                {
                    // Poda alfa-beta
                    alpha = Math.Max(alpha, bestScore);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                else
                {
                    // Poda alfa-beta
                    beta = Math.Min(beta, bestScore);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }


            }

            return new ScoringMove(bestScore, bestMove);
        }
    }
    public ScoringMove AspirationSearch(Field board, AlgorithmStatistics statistics)
    {
        statistics.ExpandedNodes++;
        int alfa, beta;
        ScoringMove move;
        if (previousScore != 0)
        {
            alfa = previousScore - windowRange;
            beta = previousScore + windowRange;
            while (true)
            {
                move = NegamaxAB(board, 0, alfa, beta,statistics);
                if (move.score <= alfa) alfa = int.MinValue;
                else if (move.score >= beta) beta = int.MaxValue;
                else break;
            }
            previousScore = move.score;
        }
        else
        {
            move = NegamaxAB(board, 0, int.MinValue, int.MaxValue, statistics);
            previousScore = move.score;
        }
        return move;
    }


}
