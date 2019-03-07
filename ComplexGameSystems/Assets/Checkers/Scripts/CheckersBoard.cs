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
                    // Check for king if the move was successful
                    CheckForKing();
                }
                else
                {
                    // Move it back to the original (start)
                    MovePiece(selectedPiece, x1, y1);
                }

                EndTurn();
            }           
                      
                              
                            
        }

        /// <summary>
        /// Checks if given coordinates are out of the board range
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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

            // Rule #1 : Is the start the same as the end?
            if(start == end)
            {
                // You can move back where you were
                return true;
            }

            // Rule #2 : If you are moving on top of another piece
            if(pieces[x2, y2])
            {
                // Can't let you do that Star Fox
                return false;
            }

            // Rule 3
            int locationX = Mathf.Abs(x1 - x2);
            int locationY = y2 - y1;

            // Rule #3.1 : White piece rule
            if(selectedPiece.isWhite || selectedPiece.isKing)
            {
                // Check if we're moving diagonally right
                if(locationX == 1 && locationY == 1)
                {
                    // Outstanding move
                    return true;
                }
                //if Moving diagonlly left (two spaces)
                else if (locationX == 2 && locationY == 2)
                {
                    // Get the piece in between move
                    Piece pieceBetween = GetPieceBetween(start, end);
                    // If there is a piece between and the piece isn't the same colour
                    if(pieceBetween != null && pieceBetween.isWhite != selectedPiece.isWhite)
                    {
                        // Destroy the piece between
                        RemovePiece(pieceBetween);
                        // Can't let you do that Star Fox
                        return true;
                    }
                }
            }

            // Rule #3.2 : Black piece rule
            if (selectedPiece.isWhite || selectedPiece.isKing)
            {
                // Check if we're moving diagonally right
                if (locationX == 1 && locationY == -1)
                {
                    // Outstanding move
                    return true;
                }
                //if Moving diagonlly left (two spaces)
                else if (locationX == 2 && locationY == -2)
                {
                    // Get the piece in between move
                    Piece pieceBetween = GetPieceBetween(start, end);
                    // If there is a piece between and the piece isn't the same colour
                    if (pieceBetween != null && pieceBetween.isWhite != selectedPiece.isWhite)
                    {
                        // Destroy the piece between
                        RemovePiece(pieceBetween);
                        // Can't let you do that Star Fox
                        return true;
                    }

                }

                //print("X location" + XLocation + "Y Location" + YLocation);

            }
            return false;

        }

        /// <summary>
        /// Calculates & returns the piece between start and end locations
        /// </summary>
        /// <param name="start"> X Locations</param>
        /// <param name="end"></param>
        /// <returns></returns>
        private Piece GetPieceBetween(Vector2 start, Vector2 end)
        {
            int xIndex = (int)(start.x + end.x) / 2;
            int yIndex = (int)(start.y + end.y) / 2;
            return pieces[xIndex, yIndex];
        }

        /// <summary>
        /// Removes a piece from the board
        /// </summary>
        /// <param name="pieceToRemove"></param>
        private void RemovePiece(Piece pieceToRemove)
        {
            // Remove it from the array
            pieces[pieceToRemove.x, pieceToRemove.y] = null;
            // Destroy the gameobject
            DestroyImmediate(pieceToRemove.gameObject);
            
        }

        /// <summary>
        /// Runs after the turn has finished
        /// </summary>
        private void EndTurn()
        {
            
        }

        private void StartTurn()
        {
            List<Piece> piecesToMove = GetPossibleMoves();
            if(piecesToMove.Count != 0)
            {
                // Forced to move pieces
                print("Forced Piece");
            }
        }

        void CheckForKing()
        {
            // Get the end drag locations
            int x = (int)endDrag.x;
            int y = (int)endDrag.y;
            // Check if the selected piece needs to be kinged
            if (selectedPiece && !selectedPiece.isKing)
            {

                bool whiteNeedsKing = selectedPiece.isWhite && y == 7;
                bool blackNeedsKing = !selectedPiece.isWhite && y == 0;
                // If the selected piece is white and reached the end of the board
                if (selectedPiece.isWhite && y == 7)
                {
                    // The selected piece is kinged
                    selectedPiece.isKing = true;
                    // Run animations
                }

            }
        }
        
        /// <summary>
        /// Detect if there is a forced move for a given piece
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public bool IsForcedMove(Piece piece)
        {
           
            // Is the piece white or kinged
            if(piece.isWhite || piece.isKing)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for(int j = -1; j <= 1; j++)
                    {
                        if(i == 0 || j == 0)
                        {
                            continue;
                        }

                        // Creating a new X from piece coordinates using offset
                        int x1 = piece.x + i;
                        int y1 = piece.y + j;

                        if(OutOfBounds(x1, y1))
                        {
                            continue;
                        }

                        // Try getting the piece at coordinates
                        Piece detectedPiece = pieces[x1, y1];
                        // If there is a piece there and the piece isn't the same colour
                        if(detectedPiece != null && detectedPiece.isWhite != selectedPiece.isWhite)
                        {
                            int x2 = x1 + i;
                            int y2 = y1 + j;

                            if(OutOfBounds(x2, y2))
                            {
                                continue;
                            }

                            // Check if we can jump (if the cell next to it is empty)
                            Piece destinationCell = pieces[x2 + i, y2 + j];
                            if(destinationCell == null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }


            // If all else fails, it means there is no forced move for this piece
            return false;
        }
        
        public List<Piece> GetPossibleMoves()
        {
            List<Piece> forcedPieces = new List<Piece>();

            // Check the entire board
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Piece pieceToCheck = pieces[x, y];
                    if(pieceToCheck != null)
                    {
                        if(IsForcedMove(pieceToCheck))
                        {
                            forcedPieces.Add(pieceToCheck);
                        }
                    }
                }
            }

            return forcedPieces;
        }
    }
}
