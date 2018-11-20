using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace XNAris
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<int[,]> Pieces = new List<int[,]>();

        const int BoardWidth = 10;
        const int BoardHeight = 20;
        const int MinoSize = 16;

        int StepTime = 300;
        int ElapsedTime = 0;
        int KeyboardElapsedTime = 0;

        int[,] Matrix;
        Vector2 MatrixLocation = Vector2.Zero;

        private Texture2D MinoSkin;

        Color[] MinoColors = {
                Color.Transparent,
                Color.Cyan,
                Color.Indigo,
                Color.Orange,
                Color.LimeGreen,
                Color.Purple,
                Color.Red
            }; // Indexes 0 through 7.

        Random rand = new Random();

        int[,] SpawnedPiece;
        Vector2 SpawnedPieceLocation;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public enum PlaceStates
        {
            CAN_PLACE, BLOCKED, OFFSCREEN
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Pieces.Add(new int[4, 4] {
                { 0, 0, 0, 0 },
                { 1, 1, 1, 1 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 }
            });  // I-Piece

            Pieces.Add(new int[3, 3] {
                { 1, 0, 0 },
                { 1, 1, 1 },
                { 0, 0, 0 }
            });  // J-Piece

            Pieces.Add(new int[3, 3] {
                { 0, 0, 1 },
                { 1, 1, 1 },
                { 0, 0, 0 }
            });  // J-Piece

            Pieces.Add(new int[2, 2] {
                { 1, 1 },
                { 1, 1 }
            });  // O-Piece

            Pieces.Add(new int[3, 3] {
                { 0, 1, 1 },
                { 1, 1, 0 },
                { 0, 0, 0 }
            });  // S-Piece

            Pieces.Add(new int[3, 3] {
                { 0, 1, 0 },
                { 1, 1, 1 },
                { 0, 0, 0 }
            });  // T-Piece

            Pieces.Add(new int[3, 3] {
                { 1, 1, 0 },
                { 0, 1, 1 },
                { 0, 0, 0 }
            });  // Z-Piece

            Matrix = new int[BoardWidth, BoardHeight];

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here.
            MinoSkin = Content.Load<Texture2D>("Monomino");
        }

        public void SpawnPiece()
        {
            int CCol = rand.Next(0, Pieces.Count);
            SpawnedPiece = (int[,])Pieces[CCol].Clone();

            int dim = SpawnedPiece.GetLength(0);

            for (int y = 0; y < dim; y++)
            {
                for (int x = 0; x < dim; x++)
                {
                    SpawnedPiece[x, y] *= (CCol + 1);
                }
            }

            SpawnedPieceLocation = Vector2.Zero;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            ElapsedTime += gameTime.ElapsedGameTime.Milliseconds;
            KeyboardElapsedTime += gameTime.ElapsedGameTime.Milliseconds;

            KeyboardState ks = Keyboard.GetState();
            if (KeyboardElapsedTime > 200)
            {
                if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.Right))
                {
                    Vector2 NewSpawnedPieceLocation = SpawnedPieceLocation + new Vector2(ks.IsKeyDown(Keys.Left) ? -1 : 1, 0);

                    PlaceStates ps = CanPlace(Matrix, SpawnedPiece, (int)NewSpawnedPieceLocation.X, (int)NewSpawnedPieceLocation.Y);
                    if (ps == PlaceStates.CAN_PLACE)
                    {
                        SpawnedPieceLocation = NewSpawnedPieceLocation;
                    }

                    KeyboardElapsedTime = 0;
                }

                if (ks.IsKeyDown(Keys.Z) || ks.IsKeyDown(Keys.X) || ks.IsKeyDown(Keys.C))
                {
                    int[,] newSpawnedPiece = Rotate(SpawnedPiece, ks.IsKeyDown(Keys.Z) || ks.IsKeyDown(Keys.C) ? true : false);

                    PlaceStates ps = CanPlace(Matrix, SpawnedPiece, (int)NewSpawnedPieceLocation.X, (int)NewSpawnedPieceLocation.Y);
                    if (ps == PlaceStates.CAN_PLACE)
                    {
                        SpawnedPiece = newSpawnedPiece;
                    }

                    KeyboardElapsedTime = 0;
                }

                if (ks.IsKeyDown(Keys.Down))
                {
                    ElapsedTime = StepTime + 1;
                    KeyboardElapsedTime = 175;
                }
            }

            if (ElapsedTime > StepTime)
            {
                Vector2 NewSpawnedPieceLocation = SpawnedPieceLocation + new Vector2(0, 1);

                PlaceStates ps = CanPlace(Matrix, SpawnedPiece, (int)NewSpawnedPieceLocation.X, (int)NewSpawnedPieceLocation.Y);
                if (ps != PlaceStates.CAN_PLACE)
                {
                    Place(Matrix, SpawnedPiece, (int)NewSpawnedPieceLocation.X, (int)NewSpawnedPieceLocation.Y);
                    SpawnPiece();

                    ps = CanPlace(Matrix, SpawnedPiece, (int)NewSpawnedPieceLocation.X, (int)NewSpawnedPieceLocation.Y);
                    if (ps == PlaceStates.BLOCKED)
                    {
                        this.Exit();
                    }
                } else
                {
                    SpawnedPieceLocation = NewSpawnedPieceLocation;
                }

                ElapsedTime = 0;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();

            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    Color tintColor = MinoColors[Matrix[x, y]];

                    if (Matrix[x, y] == 0)
                    {
                        tintColor = Color.Transparent;
                    }

                    spriteBatch.Draw(MinoSkin,
                        new Rectangle((int)MatrixLocation.X + x * MinoSize, (int)MatrixLocation.Y + y * MinoSize, MinoSize, MinoSize), tintColor);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public PlaceStates CanPlace(int[,] board, int[,] piece, int x, int y)
        {
            int sidelen = piece.GetLength(0);

            for (int px = 0; px < sidelen; px++)
            {
                for (int py = 0; py < sidelen; py++)
                {
                    int coordx = x + px;
                    int coordy = x + py;

                    if (piece[px, py] != 0)
                    {
                        if (coordx < 0 || coordx >= BoardWidth)
                        {
                            return PlaceStates.OFFSCREEN;
                        }
                        if (coordy >= BoardHeight || board[coordx, coordy] != 0)
                        {
                            return PlaceStates.BLOCKED;
                        }
                    }
                }
            }

            return PlaceStates.CAN_PLACE;
        }

        public void RemoveCompleteLines(int[,] board)
        {
            for (int y = BoardHeight - 1; y >= 0; y--)
            {
                bool isComplete = true;
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (board[x, y] == 0)
                    {
                        isComplete = false;
                    }
                }

                if (isComplete)
                {
                    for (int yc = y; yc > 0; yc--)
                    {
                        for (int x = 0; x < BoardWidth; x++)
                        {
                            board[x, yc] = board[x, yc - 1];
                        }
                    }

                    y++;
                }
            }
        }

        public void Place(int[,] board, int[,] piece, int x, int y)
        {
            int sidelen = piece.GetLength(0);

            for (int px = 0; px < sidelen; px++)
            {
                for (int py = 0; py < sidelen; py++)
                {
                    int coordx = x + px;
                    int coordy = y + py;

                    if (piece[px, py] != 0)
                    {
                        board[coordx, coordy] = piece[px, py];
                    }
                }
            }

            RemoveCompleteLines(board);
        }

        public int[,] Rotate(int[,] piece, bool left)
        {
            int sidelen = piece.GetLength(0);
            int[,] npiece = new int[sidelen, sidelen];

            for (int i = 0; i < sidelen; i++)
            {
                for (int j = 0; j < sidelen; j++)
                {
                    if (left)
                    {
                        npiece[j, i] = piece[i, sidelen - 1 - j];
                    } else
                    {
                        npiece[j, i] = piece[sidelen - 1 - i, j];
                    }
                }
            }

            return npiece;
        }
    }
}
