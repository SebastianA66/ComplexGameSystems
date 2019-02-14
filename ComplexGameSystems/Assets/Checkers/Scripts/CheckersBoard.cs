using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{


    public class CheckersBoard : MonoBehaviour
    {
        [Tooltip("Prefabs for Checker Pieces")]
        public GameObject whitePiecePrefab, blackPiecePrefab;
        [Tooltip("Where to attach the spawned pieces in the hierarchy")]
        public Transform checkersParent;
        public Vector3 boardOffset = new Vector3(-4.0f, 0.0f, -4.0f);
        public Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);
        public float rayDistance = 1000f;
        public LayerMask hitLayers;
        public Piece[,] pieces = new Piece[8, 8];

        /*
         * isHost = Is the player currently the host? (for networking)
         * isWhiteTurn + Is it currently thr player's turn or opponent?
         * hasKilled = Did the player's piece get killed?
         */

        private bool isWhiteTurn = true, hasKilled;
        private Vector2 mouseOver, startDrag, endDrag;

        // Detect selected piece
        private Piece selectedPiece = null;

        void Start()
        {
            GenerateBoard();
        }
        private void Update()
        {
            // Update the mouse over information
            MouseOver();
            // Is it currently white's turn?
            if (isWhiteTurn)
            {
                int x = (int)mouseOver.x;
                int y = (int)mouseOver.y;

                // If the mouse is pressed
                if (Input.GetMouseButtonDown(0))
                {
                    // Try selecting piece
                    selectedPiece = SelectPiece(x, y);
                    startDrag = new Vector2(x, y);
                }
                // If there is a selected piece
                if (selectedPiece)
                {
                    // Move the piece with mouse
                    DragPiece(selectedPiece);
                }

                // If button is released
                if (Input.GetMouseButtonUp(0))
                {
                    endDrag = new Vector2(x, y);
                    TryMove(startDrag, endDrag);
                    selectedPiece = null;
                }
            }
        }

        /// <summary>
        /// Generates a Checker Piece in specified coordinates
        /// </summary>
        /// <param name="x"> X location </param>
        /// <param name="y"> Y Location </param>
        public void GeneratePiece(int x, int y, bool isWhite)
        {
            // What prefab are we using (white or black) ?
            GameObject prefab = isWhite ? whitePiecePrefab : blackPiecePrefab;
            // Generate Instance of prefab
            GameObject clone = Instantiate(prefab, checkersParent);
            // Get the piece component
            Piece p = clone.GetComponent<Piece>();
            // Add piece to array
            p.x = x;
            p.y = y;
            // Reposition clone
            MovePiece(p, x, y);
        }

        /// <summary>
        /// Clears and re-generates entire board
        /// </summary>
        public void GenerateBoard()
        {
            // Generate White Team
            for (int y = 0; y < 3; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    // Generate Piece
                    GeneratePiece(oddRow ? x : x + 1, y, true);
                }
            }
            // Generate Black Team
            for (int y = 5; y < 8; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    // Generate Piece
                    GeneratePiece(oddRow ? x : x + 1, y, false);
                }
            }
        }

        /// <summary>
        /// Selects a piece on the 2D grid and returns it
        /// </summary>
        /// <param name="x"> X Coordinate </param>
        /// <param name="y"> Y Coordinate </param>
        /// <returns></returns>
        private Piece SelectPiece(int x, int y)
        {
            Piece result = null; // Default result of function to null


            // Check if X and Y is out of bounds
            if (OutOfBounds(x, y))
            {
                // Return result early
                return result;
            }

            // Get the piece at X and Y location
            Piece piece = pieces[x, y];
            // Check that it isn't null
            if (piece)
            {
                result = piece;
                
            }

            return result;
        }

        /// <summary>
        /// Moves a piece to another coordinate on a 2D grid
        /// </summary>
        /// <param name="p"> The piece to move </param>
        /// <param name="x"> X Location </param>
        /// <param name="y"> Y Location </param>
        private void MovePiece(Piece p, int x, int y)
        {
            pieces[p.x, p.y] = null; // Update new coordinate
            pieces[x, y] = p; // Reset old coordinate
            p.x = x;
            p.y = y;
            // Translate the piece to another location
            p.transform.localPosition = new Vector3(x, 0, y) + boardOffset + pieceOffset;
        


            // Translate the piece to another location
            p.transform.localPosition = new Vector3(x, 0, y) + boardOffset + pieceOffset;
        }

        /// <summary>
        /// Updating when the pieces have been dragged
        /// </summary>

        private void MouseOver()
        {

            // Perform Raycast from mouse position
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // If the ray hit the board
            if (Physics.Raycast(camRay, out hit, rayDistance, hitLayers))
            {
                // Convert mouse coordinates to 2D array coordinates
                mouseOver.x = (int)(hit.point.x - boardOffset.x);
                mouseOver.y = (int)(hit.point.z - boardOffset.z);
            }
            else // Otherwise
            {
                // Default to error (-1)
                mouseOver.x = -1;
                mouseOver.y = -1;
            }
        }

        /// <summary>
        /// Drags the selected piece using Raycast location
        /// </summary>
        /// <param name="selected"></param>
        private void DragPiece(Piece selected)
        {
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(camRay, out hit, rayDistance, hitLayers))
            {
                // Updates position of selected piece to hit point + offset
                selected.transform.position = hit.point + Vector3.up;
            }
        }

        /// <summary>
        /// Tries moving a piece from x1 + y1 to x2 + y2 coordinates
        /// </summary>
        /// <param name="x1"> First X </param>
        /// <param name="y1"> First Y </param>
        /// <param name="x2"> Second X </param>
        /// <param name="y2"> Second Y </param>
        private void TryMove(Vector2 start, Vector2 end)
        {
            int x1 = (int)start.x;
            int y1 = (int)start.y;
            int x2 = (int)end.x;
            int y2 = (int)end.y;

            // Record start Drag & end Drag
            startDrag = new Vector2(x1, y1);
            endDrag = new Vector2(x2, y2);

            // If there is a selected piece
            if (selectedPiece)
            {
                // Check if desired location is out of bounds
                if (OutOfBounds(x2, y2))
                {
                    // Move it back to the original (start)
                    MovePiece(selectedPiece, x1, y1);
                    return; // Exit function
                }

                // Check if it is a valid move
                if (ValidMove(start, end))
                {
                    // Replace end coordinates with our selected piece
                    MovePiece(selectedPiece, x2, y2);
                }
                else
                {
                    // Move it back to the original (start)
                    MovePiece(selectedPiece, x1, y1);
                }

                
            }           
                      
                              
                            
        }

        private bool OutOfBounds(int x, int y)
        {
            return x < 0 || x >= 8 || y < 0 || y >= 8;
        }

        private bool ValidMove(Vector2 start, Vector2 end)
        {
            int x1 = (int)start.x;
            int y1 = (int)start.y;
            int x2 = (int)end.x;
            int y2 = (int)end.y;

            if(start == end)
            {
                
                return true;
            }

            // If you are moving on top of another piece
            if(pieces[x2, y2])
            {
                // Can't let you do that Star Fox
                return false;
            }
            
            return true;
        }
    }
}
