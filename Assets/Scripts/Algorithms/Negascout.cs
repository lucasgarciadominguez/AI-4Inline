using ConnectFour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Negascout : MonoBehaviour
{
    public ScoringMove NegascoutAB(Field board, int depth, int alfa, int beta, AlgorithmStatistics statistics)
    {
        statistics.ExpandedNodes++;
        int bestMove = 0;
        int bestScore = 0;
        int currentScore;
        ScoringMove scoringMove;
        Field newBoard;

        if (depth == 4 || board.CheckForWinner() || board.CheckForVictory())
        {
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

            // Primer movimiento (principal)
            int firstMove = possibleMoves[0];
            newBoard = board.Clone();
            if (board.playerTurn == 1)
            {
                newBoard.DropInColumn(firstMove, 2);

            }
            else
            {
                newBoard.DropInColumn(firstMove, 1);
            }

            scoringMove = NegascoutAB(newBoard, depth + 1, -beta, -alfa,statistics);

            bestScore = -scoringMove.score;
            bestMove = firstMove;

            for (int i = 1; i < possibleMoves.Count; i++)
            {
                int move = possibleMoves[i];
                newBoard = board.Clone();
                if (board.playerTurn == 1)
                {
                    newBoard.DropInColumn(move, 2);
                }
                else
                {
                    newBoard.DropInColumn(move, 1);
                }

                scoringMove = NegascoutAB(newBoard, depth + 1, -alfa - 1, -alfa, statistics);

                currentScore = -scoringMove.score;

                if (currentScore > alfa && currentScore < beta)
                {
                    scoringMove = NegascoutAB(newBoard, depth + 1, -beta, -currentScore, statistics);
                    currentScore = -scoringMove.score;
                }

                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestMove = move;
                }
                if (depth % 2 == 0)
                {
                    // Poda alfa-beta
                    alfa = Math.Max(alfa, bestScore);
                    if (beta <= alfa)
                    {
                        break;
                    }
                }
                else
                {
                    // Poda alfa-beta
                    beta = Math.Max(beta, bestScore);
                    if (beta <= alfa)
                    {
                        break;
                    }
                }


            }
        }
        scoringMove = new ScoringMove(bestScore, bestMove);
        return scoringMove;
    }


    public (ScoringMove move, AlgorithmStatistics statistics) RunAlgorithm(Field board, int depth, int alfa, int beta)
    {
        AlgorithmStatistics statistics = new AlgorithmStatistics();
        Stopwatch stopwatch = new Stopwatch();

        // Iniciar temporizador
        stopwatch.Start();

        // Ejecutar el algoritmo y obtener estadísticas
        ScoringMove result = NegascoutAB( board,  depth,  alfa, beta,statistics);

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
