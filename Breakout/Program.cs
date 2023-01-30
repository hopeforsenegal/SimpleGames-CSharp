using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

namespace HelloWorld
{
    static class Program
    {
        enum CollisionFace
        {
            None = -1,
            Left,
            Top,
            Right,
            Bottom,
        }

        class Brick
        {
            public int type;
            public bool isAlive;
        }

        class Ball
        {
            public Vector2 centerPosition;
            public Vector2 velocity;
            public float size;
        }

        class Pad
        {
            public InputScheme input;
            public int score;
            public float speed;

            public Vector2 centerPosition;
            public Vector2 size;
        }
        struct InputScheme
        {
            public KeyboardKey leftButton;
            public KeyboardKey rightButton;
        }

        public static class Dimensions
        {
            public const int BoardWidthInBricks = 12;
            public const int BoardHeightInBricks = 13;
            public const int BrickWidthInPixels = 64;
            public const int BrickHeightInPixels = 24;
        }

        // Start a little bit off the edges 
        public const int brickOffsetX = 16;
        public const int brickOffsetY = 16;

        static Vector2 InitialBallPosition;
        static Vector2 InitialBallVelocity;
        static Ball ball = new Ball();
        static Pad player1 = new Pad();
        static Brick[,] bricks = new Brick[Dimensions.BoardWidthInBricks, Dimensions.BoardHeightInBricks];

        public static void Main()
        {
            Raylib.InitWindow(800, 480, "C# Breakout");
            Raylib.SetTargetFPS(60);

            SetupGame();

            while (!Raylib.WindowShouldClose())
            {
                var dt = Raylib.GetFrameTime();
                Update(dt);
                Draw();
            }

            Raylib.CloseWindow();
        }

        private static void SetupGame()
        {
            int screenSizeX = Raylib.GetScreenWidth();
            int screenSizeY = Raylib.GetScreenHeight();

            // Set up the bricks
            for (int i = 0; i < Dimensions.BoardWidthInBricks; i++)
            {
                for (int j = 0; j < Dimensions.BoardHeightInBricks; j++)
                {
                    bricks[i, j] = new Brick();
                    bricks[i, j].type = new Random().Next(4);
                    bricks[i, j].isAlive = true;
                }
            }
            {   // Set up ball 
                InitialBallPosition = new Vector2(screenSizeX / 2, screenSizeY - 20);
                InitialBallVelocity = new Vector2(50, -25);
                ball.centerPosition = InitialBallPosition;
                ball.velocity = InitialBallVelocity;
                ball.size = 10;
            }
            {   // Set up player
                player1.size = new Vector2(50, 5);
                player1.speed = 100;
                player1.centerPosition = new Vector2(screenSizeX / 2, screenSizeY - 10);
                player1.input = new InputScheme
                {
                    leftButton = KeyboardKey.KEY_A,
                    rightButton = KeyboardKey.KEY_D
                };
            }
        }

        private static void Update(float deltaTime)
        {
            int height = Raylib.GetScreenHeight();
            int width = Raylib.GetScreenWidth();
            CollisionFace collisionFace = CollisionFace.None;

            // Updates
            {   // Update player
                if (Raylib.IsKeyDown(player1.input.rightButton))
                {
                    // Update position
                    player1.centerPosition.X += (deltaTime * player1.speed);
                    // Clamp on right edge
                    if (player1.centerPosition.X + (player1.size.X / 2) > width)
                    {
                        player1.centerPosition.X = width - (player1.size.X / 2);
                    }
                }
                if (Raylib.IsKeyDown(player1.input.leftButton))
                {
                    // Update position
                    player1.centerPosition.X -= (deltaTime * player1.speed);
                    // Clamp on left edge
                    if (player1.centerPosition.X - (player1.size.X / 2) < 0)
                    {
                        player1.centerPosition.X = (player1.size.X / 2);
                    }
                }
            }
            {   // Update ball
                ball.centerPosition.X += (deltaTime * ball.velocity.X);
                ball.centerPosition.Y += (deltaTime * ball.velocity.Y);
            }
            // Collisions
            {   // ball boundary collisions
                var isBallOnBottomScreenEdge = ball.centerPosition.Y > height;
                var isBallOnTopScreenEdge = ball.centerPosition.Y < 0;
                var isBallOnLeftRightScreenEdge = ball.centerPosition.X > width || ball.centerPosition.X < 0;
                if (isBallOnBottomScreenEdge)
                {
                    ball.centerPosition = InitialBallPosition;
                    ball.velocity = InitialBallVelocity;
                }
                if (isBallOnTopScreenEdge)
                {
                    ball.velocity.Y *= -1;
                }
                if (isBallOnLeftRightScreenEdge)
                {
                    ball.velocity.X *= -1;
                }
            }
            {   // ball brick collisions
                var hasHit = false;
                for (int i = 0; i < Dimensions.BoardWidthInBricks; i++)
                {
                    for (int j = 0; j < Dimensions.BoardHeightInBricks; j++)
                    {
                        var brick = bricks[i, j];
                        if (!brick.isAlive) continue;

                        // Coords
                        float brickX = brickOffsetX + (i * Dimensions.BrickWidthInPixels);
                        float brickY = brickOffsetY + (j * Dimensions.BrickHeightInPixels);

                        // Ball position
                        float ballX = ball.centerPosition.X - (ball.size / 2);
                        float ballY = ball.centerPosition.Y - (ball.size / 2);

                        // Center Brick
                        float brickCenterX = brickX + (Dimensions.BrickWidthInPixels / 2);
                        float brickCenterY = brickY + (Dimensions.BrickHeightInPixels / 2);

                        var hasCollisionX = ballX + ball.size >= brickX && brickX + Dimensions.BrickWidthInPixels >= ballX;
                        var hasCollisionY = ballY + ball.size >= brickY && brickY + Dimensions.BrickHeightInPixels >= ballY;

                        if (hasCollisionX && hasCollisionY)
                        {
                            brick.isAlive = false;
                            hasHit = true;

                            // Determine which face of the brick was hit
                            float ymin = Math.Max(brickY, ballY);
                            float ymax = Math.Min(brickY + Dimensions.BrickHeightInPixels, ballY + ball.size);
                            float ysize = ymax - ymin;
                            float xmin = Math.Max(brickX, ballX);
                            float xmax = Math.Min(brickX + Dimensions.BrickWidthInPixels, ballX + ball.size);
                            float xsize = xmax - xmin;
                            if (xsize > ysize && ball.centerPosition.Y > brickCenterY)
                            {
                                collisionFace = CollisionFace.Bottom;
                            }
                            else if (xsize > ysize && ball.centerPosition.Y <= brickCenterY)
                            {
                                collisionFace = CollisionFace.Top;
                            }
                            else if (xsize <= ysize && ball.centerPosition.X > brickCenterX)
                            {
                                collisionFace = CollisionFace.Right;
                            }
                            else if (xsize <= ysize && ball.centerPosition.X <= brickCenterX)
                            {
                                collisionFace = CollisionFace.Left;
                            }
                            else
                            {
                                Debug.Assert(false);
                            }

                            break;
                        }
                    }
                    if (hasHit) break;
                }
            }
            {   // Update ball after collision
                if (collisionFace != CollisionFace.None)
                {
                    switch (collisionFace, ball.velocity.X, ball.velocity.Y)	// I used to like this. but the if version seems clearer
                    {
                        case (CollisionFace.Top, > 0, > 0):
                        case (CollisionFace.Top, < 0, > 0):
                        case (CollisionFace.Bottom, > 0, < 0):
                        case (CollisionFace.Bottom, < 0, < 0):
                            ball.velocity.Y *= -1;
                            break;
                        case (CollisionFace.Left, > 0, > 0):
                        case (CollisionFace.Left, > 0, < 0):
                        case (CollisionFace.Right, < 0, > 0):
                        case (CollisionFace.Right, < 0, < 0):
                            ball.velocity.X *= -1;
                            break;
                        default:
                            break;
                    }
                }
            }
            {	// Update ball after pad collision
                if (DetectBallTouchesPad(ball, player1))
                {
                    var previousVelocity = ball.velocity;
                    var distanceX = ball.centerPosition.X - player1.centerPosition.X;
                    var percentage = distanceX / (player1.size.X / 2);
                    ball.velocity.X = InitialBallVelocity.X * percentage;
                    ball.velocity.Y *= -1;
                    var newVelocity = Vector2.Normalize(ball.velocity) * previousVelocity.Length() * 1.1f;
                    Debug.WriteLine(newVelocity);
                    ball.velocity = newVelocity;
                }
            }
            {   // Detect all bricks popped
                var hasAtLeastOneBrick = false;
                for (int i = 0; i < Dimensions.BoardWidthInBricks; i++)
                {
                    for (int j = 0; j < Dimensions.BoardHeightInBricks; j++)
                    {
                        var brick = bricks[i, j];
                        if (brick.isAlive)
                        {
                            hasAtLeastOneBrick = true;
                            break;    // NOTE: This needs to break all the way out to be a proper comparison of identical code execution
                        }
                    }
                }
                if (!hasAtLeastOneBrick)
                {
                    SetupGame();
                }
            }
        }

        private static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);

            {   // Draw Paddles
                Raylib.DrawRectangle((int)player1.centerPosition.X - ((int)player1.size.X / 2), (int)(player1.centerPosition.Y - (player1.size.Y / 2)), (int)player1.size.X, (int)player1.size.Y, Color.WHITE);
            }
            {   // Draw alive bricks
                for (int i = 0; i < Dimensions.BoardWidthInBricks; i++)
                {
                    for (int j = 0; j < Dimensions.BoardHeightInBricks; j++)
                    {
                        if (!bricks[i, j].isAlive) continue;

                        Raylib.DrawRectangle(brickOffsetX + (i * Dimensions.BrickWidthInPixels),
                                             brickOffsetY + (j * Dimensions.BrickHeightInPixels),
                                             Dimensions.BrickWidthInPixels, Dimensions.BrickHeightInPixels, TypeToColor(bricks[i, j].type));
                    }
                }
            }
            {   // Draw Ball
                Raylib.DrawRectangle((int)(ball.centerPosition.X - (ball.size / 2)), (int)(ball.centerPosition.Y - (ball.size / 2)), (int)ball.size, (int)ball.size, Color.WHITE);
            }

            Raylib.EndDrawing();
        }

        private static bool DetectBallTouchesPad(Ball ball, Pad pad)
        {
            float ballX = ball.centerPosition.X - (ball.size / 2);
            float ballY = ball.centerPosition.Y - (ball.size / 2);
            float padX = pad.centerPosition.X - (pad.size.X / 2);
            float padY = pad.centerPosition.Y - (pad.size.Y / 2);
            if (ballY + (ball.size / 2) >= padY
             && ballX >= padX && ballX <= padX + pad.size.X)
            {
                return true;
            }
            return false;
        }

        private static Color TypeToColor(int type)
        {
            return type switch { 0 => Color.WHITE, 1 => Color.RED, 2 => Color.GREEN, 3 => Color.BLUE };
        }
    }
}