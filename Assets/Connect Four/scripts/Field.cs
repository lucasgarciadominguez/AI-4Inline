using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ConnectFour
{
    public class Field
    {
        enum Piece
        {
            Empty = 0,
            Blue = 1,
            Red = 2
        }

        private int numRows;

        public int NumRows
        {
            get { return numRows; }
        }

        private int numColumns;

        public int NumColumns
        {
            get { return numColumns; }
        }

        private int numPiecesToWin;

        private bool allowDiagonally = true;

        public int[,] field;

        public bool isPlayersTurn=true;
        public int playerTurn = 1;

        public bool IsPlayersTurn
        {
            get { return isPlayersTurn; }
            set { isPlayersTurn = value; }
        }

        private int piecesNumber = 0;

        public int PiecesNumber
        {
            get { return piecesNumber; }
        }

        // memorize last move
        public int dropColumn;
        public int dropRow;

        // Field constructor
        public Field(int numRows, int numColumns, int numPiecesToWin, bool allowDiagonally)
        {
            this.numRows = numRows;
            this.numColumns = numColumns;
            this.numPiecesToWin = numPiecesToWin;
            this.allowDiagonally = allowDiagonally;

            isPlayersTurn =true;

            field = new int[numColumns, numRows];
            for (int x = 0; x < numColumns; x++)
            {
                for (int y = 0; y < numRows; y++)
                {
                    field[x, y] = (int)Piece.Empty;
                }
            }

            dropColumn = 0;
            dropRow = 0;
        }

        public Field(int numRows, int numColumns, int numPiecesToWin, bool allowDiagonally, bool isPlayersTurn, int piecesNumber, int[,] field)
        {
            this.numRows = numRows;
            this.numColumns = numColumns;
            this.numPiecesToWin = numPiecesToWin;
            this.allowDiagonally = allowDiagonally;
            this.isPlayersTurn = isPlayersTurn;
            this.piecesNumber = piecesNumber;

            this.field = new int[numColumns, numRows];
            for (int x = 0; x < numColumns; x++)
            {
                for (int y = 0; y < numRows; y++)
                {
                    this.field[x, y] = field[x, y];
                }
            }
        }

        // Devuelve la lista de casillas donde el jugador puede agregar una pieza
        public Dictionary<int, int> GetPossibleCells()
        {
            Dictionary<int, int> possibleCells = new Dictionary<int, int>();
            for (int x = 0; x < numColumns; x++)
            {
                for (int y = numRows - 1; y >= 0; y--)
                {
                    if (field[x, y] == (int)Piece.Empty)
                    {
                        possibleCells.Add(x, y);
                        break;
                    }
                }
            }
            return possibleCells;
        }

        // Devuelve la lista de columnas donde el jugador puede soltar una pieza.
        public List<int> GetPossibleDrops()
        {
            List<int> possibleDrops = new List<int>();
            for (int x = 0; x < numColumns; x++)
            {
                for (int y = numRows - 1; y >= 0; y--)
                {
                    if (field[x, y] == (int)Piece.Empty)
                    {
                        possibleDrops.Add(x);
                        break;
                    }
                }
            }
            return possibleDrops;
        }

        public int GetRandomMove()
        {
            List<int> moves = GetPossibleDrops();

            if (moves.Count > 0)
            {
                System.Random r = new System.Random();
                return moves[r.Next(0, moves.Count)];
            }
            return -1;
        }

        // Suelta  en la columna i, regresa a la fila donde cae
        public int DropInColumn(int col)
        {
            for (int i = numRows - 1; i >= 0; i--)
            {
                if (field[col, i] == 0)
                {
                    field[col, i] = isPlayersTurn ? (int)Piece.Blue : (int)Piece.Red;
                    piecesNumber += 1;
                    dropColumn = col;
                    dropRow = i;
                    return i;
                }
            }
            return -1;
        }
        public int DropInColumn(int col,int num)
        {
           ChangeTurn(num);
            for (int i = numRows - 1; i >= 0; i--)
            {
                if (field[col, i] == 0)
                {
                    field[col, i] = playerTurn;
                    piecesNumber += 1;
                    dropColumn = col;
                    dropRow = i;
                    return i;
                }
            }
            return -1;
        }
        public void ChangeTurn(int num)
        {
            if (num == 1)
            {
                playerTurn = 1;
                IsPlayersTurn = true;
            }
            else if (num == 2) 
            {
                playerTurn = 2;
                isPlayersTurn = false;
            }
        }

        // Cambio de jugador
        public void SwitchPlayer()
        {
            isPlayersTurn = !isPlayersTurn;
        }

        // Comprueba si alguien ha ganado
        public bool CheckForWinner()
        {
            for (int x = 0; x < numColumns; x++)
            {
                for (int y = 0; y < numRows; y++)
                {
                    int layermask = isPlayersTurn ? (1 << 8) : (1 << 9);

                    if (field[x, y] != (isPlayersTurn ? (int)Piece.Blue : (int)Piece.Red))
                    {
                        continue;
                    }

                    // Raycast para comprobar
                    RaycastHit[] hitsHorz = Physics.RaycastAll(
                                                new Vector3(x, y * -1, 0),
                                                Vector3.right,
                                                numPiecesToWin - 1,
                                                layermask);

                    // devuelve true lo cual siginifica victoria
                    if (hitsHorz.Length == numPiecesToWin - 1)
                    {
                        return true;
                    }

                    // otro raycast par comprobar verticalmente
                    RaycastHit[] hitsVert = Physics.RaycastAll(
                                                new Vector3(x, y * -1, 0),
                                                Vector3.up,
                                                numPiecesToWin - 1,
                                                layermask);

                    if (hitsVert.Length == numPiecesToWin - 1)
                    {
                        return true;
                    }

                    // lo mismo en diagonal
                    if (allowDiagonally)
                    {
                        float length = Vector2.Distance(new Vector2(0, 0), new Vector2(numPiecesToWin - 1, numPiecesToWin - 1));

                        RaycastHit[] hitsDiaLeft = Physics.RaycastAll(
                                                     new Vector3(x, y * -1, 0),
                                                     new Vector3(-1, 1),
                                                     length,
                                                     layermask);

                        if (hitsDiaLeft.Length == numPiecesToWin - 1)
                        {
                            return true;
                        }

                        RaycastHit[] hitsDiaRight = Physics.RaycastAll(
                                                      new Vector3(x, y * -1, 0),
                                                      new Vector3(1, 1),
                                                      length,
                                                      layermask);

                        if (hitsDiaRight.Length == numPiecesToWin - 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public int ReturnLastColourPlayer()
        {
            int colour = field[dropColumn, dropRow];
            if (colour == 0)
            {
                return 0;
            }
            else if (colour == 1)
            {
                return 1;
            }
            else if (colour == 2)
            {
                return 2;
            }
            return -1;
        }
        public int CheckForVictoryNum()
        {
            int colour = field[dropColumn, dropRow];
            if (colour == 0)
            {
                return 0;
            }

            bool bottomDirection = true;
            int currentAlignment = 1;

            //comprueba verticalmente
            for (int i = 1; i <= numPiecesToWin; i++)
            {
                if (bottomDirection && dropRow + i < NumRows)
                {
                    if (field[dropColumn, dropRow + i] == colour)
                        currentAlignment++;
                    else
                        bottomDirection = false;
                }


            }
            if (currentAlignment>1)
            {
                return currentAlignment;
            }

            bool rightDirection = true;
            bool leftDirection = true;
            currentAlignment = 1;

            //comprueba horizontal
            for (int i = 1; i <= numPiecesToWin; i++)
            {
                if (rightDirection && dropColumn + i < numColumns)
                {
                    if (field[dropColumn + i, dropRow] == colour)
                        currentAlignment++;
                    else
                        rightDirection = false;
                }

                if (leftDirection && dropColumn - i >= 0)
                {
                    if (field[dropColumn - i, dropRow] == colour)
                        currentAlignment++;
                    else
                        leftDirection = false;
                }

            }
            if (currentAlignment > 1)
            {
                return currentAlignment;
            }
            //comprueba diagonal
            if (allowDiagonally)
            {
                bool upRightDirection = true;
                bool bottomLeftDirection = true;
                currentAlignment = 1;

                for (int i = 1; i <= numPiecesToWin; i++)
                {
                    if (upRightDirection && dropColumn + i < numColumns && dropRow + i < numRows)
                    {
                        if (field[dropColumn + i, dropRow + i] == colour)
                            currentAlignment++;
                        else
                            upRightDirection = false;
                    }

                    if (bottomLeftDirection && dropColumn - i >= 0 && dropRow - i >= 0)
                    {
                        if (field[dropColumn - i, dropRow - i] == colour)
                            currentAlignment++;
                        else
                            bottomLeftDirection = false;
                    }

                }
                if (currentAlignment > 1)
                {
                    return currentAlignment;
                }
                bool upLeftDirection = true;
                bool bottomRightDirection = true;
                currentAlignment = 1;

                for (int i = 1; i <= numPiecesToWin; i++)
                {
                    if (upLeftDirection && dropColumn + i < numColumns && dropRow - i >= 0)
                    {
                        if (field[dropColumn + i, dropRow - i] == colour)
                            currentAlignment++;
                        else
                            upLeftDirection = false;
                    }

                    if (bottomRightDirection && dropColumn - i >= 0 && dropRow + i < numRows)
                    {
                        if (field[dropColumn - i, dropRow + i] == colour)
                            currentAlignment++;
                        else
                            bottomRightDirection = false;
                    }

                }
                if (currentAlignment > 1)
                {
                    return currentAlignment;
                }
            }

            return 0;
        }
        // comprobador de victoria
        public bool CheckForVictory()
        {
            int colour = field[dropColumn, dropRow];
            if (colour == 0)
            {
                return false;
            }

            bool bottomDirection = true;
            int currentAlignment = 1;

            //comprueba verticalmente
            for (int i = 1; i <= numPiecesToWin; i++)
            {
                if (bottomDirection && dropRow + i < NumRows)
                {
                    if (field[dropColumn, dropRow + i] == colour)
                        currentAlignment++;
                    else
                        bottomDirection = false;
                }

                if (currentAlignment >= numPiecesToWin)
                    return true;
            }

            bool rightDirection = true;
            bool leftDirection = true;
            currentAlignment = 1;

            //comprueba horizontal
            for (int i = 1; i <= numPiecesToWin; i++)
            {
                if (rightDirection && dropColumn + i < numColumns)
                {
                    if (field[dropColumn + i, dropRow] == colour)
                        currentAlignment++;
                    else
                        rightDirection = false;
                }

                if (leftDirection && dropColumn - i >= 0)
                {
                    if (field[dropColumn - i, dropRow] == colour)
                        currentAlignment++;
                    else
                        leftDirection = false;
                }

                if (currentAlignment >= numPiecesToWin)
                    return true;
            }

            //comprueba diagonal
            if (allowDiagonally)
            {
                bool upRightDirection = true;
                bool bottomLeftDirection = true;
                currentAlignment = 1;

                for (int i = 1; i <= numPiecesToWin; i++)
                {
                    if (upRightDirection && dropColumn + i < numColumns && dropRow + i < numRows)
                    {
                        if (field[dropColumn + i, dropRow + i] == colour)
                            currentAlignment++;
                        else
                            upRightDirection = false;
                    }

                    if (bottomLeftDirection && dropColumn - i >= 0 && dropRow - i >= 0)
                    {
                        if (field[dropColumn - i, dropRow - i] == colour)
                            currentAlignment++;
                        else
                            bottomLeftDirection = false;
                    }

                    if (currentAlignment >= numPiecesToWin)
                        return true;
                }

                bool upLeftDirection = true;
                bool bottomRightDirection = true;
                currentAlignment = 1;

                for (int i = 1; i <= numPiecesToWin; i++)
                {
                    if (upLeftDirection && dropColumn + i < numColumns && dropRow - i >= 0)
                    {
                        if (field[dropColumn + i, dropRow - i] == colour)
                            currentAlignment++;
                        else
                            upLeftDirection = false;
                    }

                    if (bottomRightDirection && dropColumn - i >= 0 && dropRow + i < numRows)
                    {
                        if (field[dropColumn - i, dropRow + i] == colour)
                            currentAlignment++;
                        else
                            bottomRightDirection = false;
                    }

                    if (currentAlignment >= numPiecesToWin)
                        return true;
                }
            }

            return false;
        }
        public bool IsValidPosition(int x,int y)
        {
            if (x>-1&&x<6&&y > -1&&y<6)
                return true;
            else
                return false;
        }

        // Comprobamos si hay casillas libres
        public bool ContainsEmptyCell()
        {
            return (piecesNumber < numRows * numColumns);
        }

        // clonamos el campo
        public Field Clone()
        {
            return new Field(numRows, numColumns, numPiecesToWin, allowDiagonally, isPlayersTurn, piecesNumber, field);
        }

        public String ToString()
        {
            String str = "Player";
            str += isPlayersTurn ? "1\n" : "2\n";
            for (int y = 0; y < numRows; y++)
            {
                for (int x = 0; x < numColumns; x++)
                {
                    str += (field[x, y]).ToString();
                }
                str += "\n";
            }
            return str;
        }
    }
}
