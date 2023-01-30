using Raylib_cs;
using System.Numerics;

namespace HelloWorld
{
    static class Program
    {
        enum TextAlignment
        {
            Left,
            Center,
            Right
        }

        class Ball
        {
            public Vector2 position;
            public Vector2 velocity;
            public float size;
        }
		
        struct InputScheme
        {
            public KeyboardKey upButton;
            public KeyboardKey downButton;
        }

        class Pad
        {
            public InputScheme input;
            public int score;
            public Vector2 velocity;

            public Vector2 position;
            public Vector2 size;
        }
		
        static Ball ball = new Ball();
        static Pad player1 = new Pad();
        static Pad player2 = new Pad();
        static Pad[] players = new[] { player1, player2 };

        static Vector2 InitialBallPosition;

        public static void Main()
        {
            Raylib.InitWindow(800, 480, "C# Pong");
            Raylib.SetTargetFPS(60);

            int screenSizeX = Raylib.GetScreenWidth();
            int screenSizeY = Raylib.GetScreenHeight();

            InitialBallPosition = new Vector2(screenSizeX / 2, screenSizeY / 2);
            ball.velocity = new Vector2(50, 25);
            ball.position = InitialBallPosition;
            ball.size = 10;
            player2.size = player1.size = new Vector2(5, 50);
            player2.velocity = player1.velocity = Vector2.One * 100;
            player1.position = new Vector2(0 + 5, screenSizeY / 2);
            player2.position = new Vector2(screenSizeX - 5 - player2.size.X, screenSizeY / 2);
            player1.input = new InputScheme
            {
                upButton = KeyboardKey.KEY_W,
                downButton = KeyboardKey.KEY_S
            };
            player2.input = new InputScheme
            {
                upButton = KeyboardKey.KEY_I,
                downButton = KeyboardKey.KEY_K
            };

            while (!Raylib.WindowShouldClose())
            {
                var dt = Raylib.GetFrameTime();
                Update(dt);
                Draw();
            }

            Raylib.CloseWindow();
        }

        private static void Update(float deltaTime)
        {
            int height = Raylib.GetScreenHeight();
            int width = Raylib.GetScreenWidth();
            {   // Update pads
                foreach (var pad in players)
                {
                    if (Raylib.IsKeyDown(pad.input.downButton))
                    {
                        // Update position
                        pad.position.Y += (deltaTime * pad.velocity.Y);
                        // Clamp on bottom edge
                        if (pad.position.Y + (pad.size.Y / 2) > height)
                        {
                            pad.position.Y = height - (pad.size.Y / 2);
                        }
                    }
                    if (Raylib.IsKeyDown(pad.input.upButton))
                    {
                        // Update position
                        pad.position.Y -= (deltaTime * pad.velocity.Y);
                        // Clamp on top edge
                        if (pad.position.Y - (pad.size.Y / 2) < 0)
                        {
                            pad.position.Y = (pad.size.Y / 2);
                        }
                    }
                }
            }
            {   // Update ball
                ball.position.X += (deltaTime * ball.velocity.X);
                ball.position.Y += (deltaTime * ball.velocity.Y);
            }
            {   // Check collisions
                foreach (var pad in players)
                {
                    var isDetectBallTouchesPad = DetectBallTouchesPad(ball, pad);
                    if (isDetectBallTouchesPad)
                    {
                        ball.velocity.X *= -1;
                    }
                }
                var isBallOnTopBottomScreenEdge = ball.position.Y > height || ball.position.Y < 0;
                var isBallOnRightScreenEdge = ball.position.X > width;
                var isBallOnLeftScreenEdge = ball.position.X < 0;
                if (isBallOnTopBottomScreenEdge)
                {
                    ball.velocity.Y *= -1;
                }
                if (isBallOnLeftScreenEdge)
                {
                    ball.position = InitialBallPosition;
                    player2.score += 1;
                }
                if (isBallOnRightScreenEdge)
                {
                    ball.position = InitialBallPosition;
                    player1.score += 1;
                }
            }
        }

        private static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);

            {   // Draw Court Line
                const float LineThinkness = 2f;
                var x = Raylib.GetScreenWidth() / 2f;
                var from = new Vector2 { X = x, Y = 5 };
                var to = new Vector2 { X = x, Y = Raylib.GetScreenHeight() - 5f };
                Raylib.DrawLineEx(from, to, LineThinkness, Color.LIGHTGRAY);
            }
            {   // Draw Scores
                DrawText(player1.score.ToString(), TextAlignment.Right, (Raylib.GetScreenWidth() / 2) - 10, 10, 20);
                DrawText(player2.score.ToString(), TextAlignment.Left, (Raylib.GetScreenWidth() / 2) + 10, 10, 20);
            }
            {   // Draw Paddles
                foreach (var pad in players)
                {
                    Raylib.DrawRectangle((int)(pad.position.X - (pad.size.X / 2)), (int)(pad.position.Y - (pad.size.Y / 2)), (int)pad.size.X, (int)pad.size.Y, Color.WHITE);
                }
            }
            {   // Draw Ball
                Raylib.DrawRectangle((int)(ball.position.X - (ball.size / 2)), (int)(ball.position.Y - (ball.size / 2)), (int)ball.size, (int)ball.size, Color.WHITE);
            }

            Raylib.EndDrawing();
        }

        private static bool DetectBallTouchesPad(Ball ball, Pad pad)
        {
            if (ball.position.X >= pad.position.X && ball.position.X <= pad.position.X + pad.size.X)
            {
                if (ball.position.Y >= pad.position.Y - (pad.size.Y / 2)
                 && ball.position.Y <= pad.position.Y + (pad.size.Y / 2))
                {
                    return true;
                }
            }
            return false;
        }

        private static unsafe void DrawText(string text, TextAlignment alignment, int posX, int posY, int fontSize)
        {
            Color fontColor = Color.LIGHTGRAY;
            {
                var scoreAsBytes = BytesFromString(text);
                fixed (sbyte* fixedPtr = scoreAsBytes)
                {
                    var offsetFontTextLength = Raylib.TextFormat(fixedPtr);
                    if (alignment == TextAlignment.Left)
                    {
                        Raylib.DrawText(fixedPtr, posX, posY, fontSize, fontColor);
                    }
                    else if (alignment == TextAlignment.Center)
                    {
                        var scoreSizeLeft = Raylib.MeasureText(offsetFontTextLength, fontSize);
                        Raylib.DrawText(fixedPtr, posX - (scoreSizeLeft / 2), posY, fontSize, fontColor);
                    }
                    else if (alignment == TextAlignment.Right)
                    {
                        var scoreSizeLeft = Raylib.MeasureText(offsetFontTextLength, fontSize);
                        Raylib.DrawText(fixedPtr, posX - scoreSizeLeft, posY, fontSize, fontColor);
                    }
                }
            }
        }

        private static sbyte[] BytesFromString(string str)
        {
            return Array.ConvertAll(str.GetUTF8Bytes(), c => Convert.ToSByte(c));
        }
    }
}