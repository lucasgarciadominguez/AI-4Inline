using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;
using UnityEngine.SceneManagement;

namespace ConnectFour
{
    public class ScoringMove
    {
        public int score;
        public int move;
        public ScoringMove(int score, int move)
        {
            this.score = score;
            this.move = move;
        }
    }
    public class AlgorithmStatistics
    {
        public int ExpandedNodes { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        public double ExecutionTimeMilliseconds
        {
            get { return ExecutionTime.TotalMilliseconds; }
        }
    }
    public enum AlgorithmUsed
    {
        Minimax,
        MinimaxAB,
        Negamax,
        NegamaxAB,
        Negascout,
        Aspirational
    }
    public class GameController : MonoBehaviour
    {
        [Range(3, 8)]
        public int numRows = 4;
        [Range(3, 8)]
        public int numColumns = 4;
        [Range(1, 8)]
        public int parallelProcesses = 2;
        [Range(7, 10000)]
        public int MCTS_Iterations = 1000;

        [Tooltip("piezas para ganar.")]
        public int numPiecesToWin = 4;

        [Tooltip("Permitir diagonales")]
        public bool allowDiagonally = true;

        public float dropTime = 4f;

        public GameObject pieceRed;
        public GameObject pieceBlue;
        public GameObject pieceField;

        public GameObject winningText;
        public string playerWonText = "You Won!";
        public string playerLoseText = "You Lose!";
        public string drawText = "Draw!";

        public GameObject btnPlayAgain;
        bool btnPlayAgainTouching = false;
        Color btnPlayAgainOrigColor;
        Color btnPlayAgainHoverColor = new Color(255, 143, 4);

        GameObject gameObjectField;
        Minimax minimax;
        Negamax negamax;
        Negascout negascout;
        GameObject gameObjectTurn;
        Field field;

        bool isLoading = true;
        bool isDropping = false;
        bool mouseButtonPressed = false;

        bool gameOver = false;
        bool isCheckingForWinner = false;
        public AlgorithmUsed algorithmUsed;


        void Start()
        {
           
            int max = Mathf.Max(numRows, numColumns);

            if (numPiecesToWin > max)
                numPiecesToWin = max;

            CreateField();

            btnPlayAgainOrigColor = btnPlayAgain.GetComponent<Renderer>().material.color;
            switch (algorithmUsed)
            {
                case AlgorithmUsed.Minimax:
                    minimax = GetComponent<Minimax>();
                    break;
                case AlgorithmUsed.MinimaxAB:
                    minimax = GetComponent<Minimax>();
                    break;
                case AlgorithmUsed.Negamax:
                    negamax = GetComponent<Negamax>();
                    break;
                case AlgorithmUsed.NegamaxAB:
                    negamax = GetComponent<Negamax>();
                    break;
                case AlgorithmUsed.Negascout:
                    negascout = GetComponent<Negascout>();
                    break;
                case AlgorithmUsed.Aspirational:
                    negamax = GetComponent<Negamax>();
                    break;
                default:
                    break;
            }
        }

        void CreateField()
        {
            winningText.SetActive(false);
            btnPlayAgain.SetActive(false);

            isLoading = true;

            gameObjectField = GameObject.Find("Field");
            if (gameObjectField != null)
            {
                DestroyImmediate(gameObjectField);
            }
            gameObjectField = new GameObject("Field");

            field = new Field(numRows, numColumns, numPiecesToWin, allowDiagonally);

            for (int x = 0; x < numColumns; x++)
            {
                for (int y = 0; y < numRows; y++)
                {
                    GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
                    g.transform.parent = gameObjectField.transform;
                }
            }

            isLoading = false;
            gameOver = false;

            Camera.main.transform.position = new Vector3((numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f), Camera.main.transform.position.z);

            winningText.transform.position = new Vector3((numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) + 1, winningText.transform.position.z);

            btnPlayAgain.transform.position = new Vector3((numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) - 1, btnPlayAgain.transform.position.z);
        }
        GameObject SpawnPiece()
        {
            int apha= int.MinValue;
            int beta=int.MaxValue;

            AlgorithmStatistics statistics = new AlgorithmStatistics();
            Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ScoringMove move=new ScoringMove(99,99);
            if (!field.IsPlayersTurn)
            {
                int depth = 4; // Profundidad máxima para el algoritmo Minimax (puede ajustarse según la complejidad del juego)
                switch (algorithmUsed)
                {
                    case AlgorithmUsed.Minimax:
                        (move, statistics) = minimax.RunAlgorithm(field, depth, true);
                        stats(statistics, "Minimax");
                        break;
                    case AlgorithmUsed.MinimaxAB:
                        (move, statistics) = minimax.RunAlgorithmAB(field, depth, apha, beta, true);
                        stats(statistics, "MinimaxAB");
                        break;
                    case AlgorithmUsed.Negamax:
                        (move, statistics) = negamax.RunAlgorithmNega(field, 0);
                        stats(statistics, "Negamax");
                        break;
                    case AlgorithmUsed.NegamaxAB:
                        (move, statistics) = negamax.RunAlgorithmNegaAB(field, 0, apha, beta);
                        stats(statistics, "NegamaxAB");
                        break;
                    case AlgorithmUsed.Negascout:
                        (move, statistics) = negascout.RunAlgorithm(field, 0, apha, beta);
                        stats(statistics, "Negascout");
                        break;
                    case AlgorithmUsed.Aspirational:
                        (move, statistics) = negamax.RunAlgorithmApirational(field);
                        stats(statistics, "Aspirational");
                        break;
                    default:
                        break;
                }
                //int bestColumn = GetBestMoveForAI(field, depth);






                // Ahora puedes usar 'move' y 'statistics' según tus necesidades
                //stats(statistics);
                //ScoringMove move = negamax.NegamaxAB(field, 0,int.MinValue,int.MaxValue);

                Debug.Log(move.score+"," + move.move);
                if (move.move != -1)
                {
                    //field.DropInColumn(bestColumn);
                    GameObject g = Instantiate(field.IsPlayersTurn ? pieceBlue : pieceRed,
                        new Vector3(move.move, gameObjectField.transform.position.y + 1, 0),
                        Quaternion.identity) as GameObject;
                    return g;
                }

            }
            else
            {
                GameObject gameObject = Instantiate(field.IsPlayersTurn ? pieceBlue : pieceRed,
                new Vector3(Mathf.Clamp(spawnPos.x, 0, numColumns - 1), gameObjectField.transform.position.y + 1, 0),
                    Quaternion.identity) as GameObject;
                return gameObject;

            }

            return null;

        }
        static void stats(AlgorithmStatistics stats,String logaritm)
        {


            // Mostrar estadísticas en la consola
            Debug.Log($"lOGARITMO{logaritm}");
            Debug.Log($"Nodos expandidos: {stats.ExpandedNodes}");
            Debug.Log($"Tiempo de ejecución: {stats.ExecutionTimeMilliseconds}"+" ms");
        }
        void UpdatePlayAgainButton()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && hit.collider.name == btnPlayAgain.name)
            {
                btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;

                if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
                {
                    btnPlayAgainTouching = true;
                    int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

                    // Cargar la escena actual por su índice
                    SceneManager.LoadScene(currentSceneIndex);
                }
            }
            else
            {
                btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
            }

            if (Input.touchCount == 0)
            {
                btnPlayAgainTouching = false;
            }
        }

        void Update()
        {
            if (isLoading)
                return;

            if (isCheckingForWinner)
                return;

            if (gameOver)
            {
                winningText.SetActive(true);
                btnPlayAgain.SetActive(true);
                UpdatePlayAgainButton();
                return;
            }

            if (field.IsPlayersTurn)
            {
                if (gameObjectTurn == null)
                {
                    gameObjectTurn = SpawnPiece();
                }
                else
                {
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    gameObjectTurn.transform.position = new Vector3(Mathf.Clamp(pos.x, 0, numColumns - 1),
                        gameObjectField.transform.position.y + 1, 0);

                    if (Input.GetMouseButtonDown(0) && !mouseButtonPressed && !isDropping)
                    {
                        mouseButtonPressed = true;
                        StartCoroutine(dropPiece(gameObjectTurn));
                    }
                    else
                    {
                        mouseButtonPressed = false;
                    }
                }
            }
            else
            {
                if (gameObjectTurn == null)
                {
                    gameObjectTurn = SpawnPiece();
                }
                else
                {
                    if (!isDropping)
                        StartCoroutine(dropPiece(gameObjectTurn));
                }
            }
        }

        IEnumerator dropPiece(GameObject gObject)
        {
            isDropping = true;

            Vector3 startPosition = gObject.transform.position;
            Vector3 endPosition = new Vector3();

            int x = Mathf.RoundToInt(startPosition.x);
            startPosition = new Vector3(x, startPosition.y, startPosition.z);

            int y = field.DropInColumn(x);

            if (y != -1)
            {
                endPosition = new Vector3(x, y * -1, startPosition.z);

                GameObject g = Instantiate(gObject) as GameObject;
                gameObjectTurn.GetComponent<Renderer>().enabled = false;

                float distance = Vector3.Distance(startPosition, endPosition);

                float t = 0;
                while (t < 1)
                {
                    t += Time.deltaTime * dropTime * ((numRows - distance) + 1);
                    g.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                    yield return null;
                }

                g.transform.parent = gameObjectField.transform;
                DestroyImmediate(gameObjectTurn);
                StartCoroutine(Won());

                while (isCheckingForWinner)
                    yield return null;

                field.SwitchPlayer();
            }

            isDropping = false;
            yield return 0;
        }

        IEnumerator Won()
        {
            isCheckingForWinner = true;
            gameOver = field.CheckForWinner();

            if (gameOver)
            {
                winningText.GetComponent<TextMesh>().text = field.IsPlayersTurn ? playerWonText : playerLoseText;
            }
            else
            {
                if (!field.ContainsEmptyCell())
                {
                    gameOver = true;
                    winningText.GetComponent<TextMesh>().text = drawText;
                }
            }

            isCheckingForWinner = false;
            yield return 0;
        }
    }
}