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

        struct Rectangle
        {
            public Vector2 centerPosition;
            public Vector2 size;
        }
        struct InputScheme
        {
            public KeyboardKey leftButton;
            public KeyboardKey rightButton;
            public KeyboardKey shootButton;
        }
        class Pad
        {
            public InputScheme input;
            public int score;
            public float speed;

            public Rectangle rect;
        }
        class Bullet
        {
            public Rectangle rect;
            public Vector2 speed;
            public bool isActive;
            public Color color;
        }
        class Enemy
        {
            public Rectangle rect;
            public Vector2 speed;
            public bool isActive;
            public Color color;
        }
		
        const float BulletCooldownSeconds = 0.3f;
        const int MaxNumBullets = 50;
        const int MaxNumEnemies = 50;
        static Bullet[] bullets = new Bullet[MaxNumBullets];
        static Enemy[] enemies = new Enemy[MaxNumEnemies];
        static Pad player1 = new Pad();
        static Random random = new Random();
        static float m_TimerBulletCooldown;
        static float m_TimerSpawnEnemy;
        static int numEnemiesKilled;
        static int numEnemiesThisLevel;
        static int numEnemiesToSpawn;
        static int numLives = 3;
        static bool IsGameOver;
        static bool IsWin;

        static Vector2 InitialPlayerPosition;

        public static void Main()
        {
            Raylib.InitWindow(800, 480, "C# Space Invaders");

            int screenSizeX = Raylib.GetScreenWidth();
            int screenSizeY = Raylib.GetScreenHeight();
            InitialPlayerPosition = new Vector2(screenSizeX / 2, screenSizeY - 10);

            {   // Set up player
                player1.rect.size = new Vector2(25, 25);
                player1.speed = 100;
                player1.rect.centerPosition = InitialPlayerPosition;
                player1.input = new InputScheme
                {
                    leftButton = KeyboardKey.KEY_A,
                    rightButton = KeyboardKey.KEY_D,
                    shootButton = KeyboardKey.KEY_SPACE,
                };
            }
            {   // init bullets
                for (int i = 0; i < MaxNumBullets; i++)
                {
                    bullets[i] = new Bullet
                    {
                        speed = new Vector2(0, 400),
                        rect = new Rectangle { size = new Vector2(5, 5) },
                    };
                }
            }
            {   // init enemies
                for (int i = 0; i < MaxNumEnemies; i++)
                {
                    enemies[i] = new Enemy
                    {
                        speed = new Vector2(0, 40),
                        rect = new Rectangle
                        {
                            centerPosition = new Vector2(random.Next(screenSizeX), -20),
                            size = new Vector2(20, 20)
                        },
                    };
                }
                numEnemiesToSpawn = numEnemiesThisLevel = 10;
            }

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

            if (IsGameOver || IsWin)
                return;


            {   // Update player movement
                if (Raylib.IsKeyDown(player1.input.rightButton))
                {
                    // Update position
                    player1.rect.centerPosition.X += (deltaTime * player1.speed);
                    // Clamp on right edge
                    if (player1.rect.centerPosition.X + (player1.rect.size.X / 2) > width)
                    {
                        player1.rect.centerPosition.X = width - (player1.rect.size.X / 2);
                    }
                }
                if (Raylib.IsKeyDown(player1.input.leftButton))
                {
                    // Update position
                    player1.rect.centerPosition.X -= (deltaTime * player1.speed);
                    // Clamp on left edge
                    if (player1.rect.centerPosition.X - (player1.rect.size.X / 2) < 0)
                    {
                        player1.rect.centerPosition.X = (player1.rect.size.X / 2);
                    }
                }
                if (HasHitTime(ref m_TimerBulletCooldown, deltaTime))
                {
                    if (Raylib.IsKeyDown(player1.input.shootButton))
                    {
                        for (int i = 0; i < MaxNumBullets; i++)
                        {
                            if (!bullets[i].isActive)
                            {
                                m_TimerBulletCooldown = BulletCooldownSeconds;
                                bullets[i].isActive = true;
                                {
                                    bullets[i].rect.centerPosition.X = player1.rect.centerPosition.X;
                                    bullets[i].rect.centerPosition.Y = player1.rect.centerPosition.Y + (player1.rect.size.Y / 4);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            {   // Update active bullets
                for (int i = 0; i < MaxNumBullets; i++)
                {
                    var bullet = bullets[i];
                    // Movement
                    if (bullet.isActive)
                    {
                        bullet.rect.centerPosition.Y -= bullet.speed.Y * deltaTime;

                        // Went off screen
                        if (bullet.rect.centerPosition.Y + (bullet.rect.size.Y / 2) <= 0)
                        {
                            bullet.isActive = false;
                        }
                    }
                }
            }
            {   // Update active enemies
                for (int i = 0; i < numEnemiesThisLevel; i++)
                {
                    var enemy = enemies[i];
                    // Movement
                    if (enemy.isActive)
                    {
                        enemy.rect.centerPosition.Y += enemy.speed.Y * deltaTime;

                        // Went off screen
                        if (enemy.rect.centerPosition.Y - (enemy.rect.size.Y / 2) >= height)
                        {
                            enemy.rect.centerPosition = new Vector2(random.Next(width), -20);
                        }
                        else
                        {
                            float enemyX = enemy.rect.centerPosition.X - (enemy.rect.size.X / 2);
                            float enemyY = enemy.rect.centerPosition.Y - (enemy.rect.size.Y / 2);
                            { // bullet | enemy collision 
                                for (int j = 0; j < MaxNumBullets; j++)
                                {
                                    var bullet = bullets[j];
                                    float bulletX = bullet.rect.centerPosition.X - (bullet.rect.size.X / 2);
                                    float bulletY = bullet.rect.centerPosition.Y - (bullet.rect.size.Y / 2);

                                    var hasCollisionX = bulletX + bullet.rect.size.X >= enemyX && enemyX + enemy.rect.size.X >= bulletX;
                                    var hasCollisionY = bulletY + bullet.rect.size.Y >= enemyY && enemyY + enemy.rect.size.Y >= bulletY;

                                    if (hasCollisionX && hasCollisionY)
                                    {
                                        bullet.isActive = false;
                                        enemy.isActive = false;
                                        {
                                            numEnemiesKilled++;
                                            IsWin = numEnemiesKilled >= numEnemiesThisLevel;
                                            break;
                                        }
                                    }
                                }
                            }
                            {   // player | enemy collision
                                float bulletX = player1.rect.centerPosition.X - (player1.rect.size.X / 2);
                                float bulletY = player1.rect.centerPosition.Y - (player1.rect.size.Y / 2);

                                var hasCollisionX = bulletX + player1.rect.size.X >= enemyX && enemyX + enemy.rect.size.X >= bulletX;
                                var hasCollisionY = bulletY + player1.rect.size.Y >= enemyY && enemyY + enemy.rect.size.Y >= bulletY;

                                if (hasCollisionX && hasCollisionY)
                                {
                                    enemy.isActive = false;
                                    {
                                        player1.rect.centerPosition = InitialPlayerPosition;
                                        numLives--;
                                        IsGameOver = numLives <= 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }	
			{	// Spawn enemies
                var canSpawn = HasHitInterval(ref m_TimerSpawnEnemy, 2f, deltaTime);
                for (int i = 0; i < MaxNumEnemies; i++)
                {
                    var enemy = enemies[i];
                    // Movement
                    if (!enemy.isActive)
                    {
                        if (canSpawn && numEnemiesToSpawn > 0)
                        {
                            numEnemiesToSpawn--;
                            enemy.isActive = true;
                            {
                                enemy.rect.centerPosition = new Vector2(random.Next(width), -20);
                                break;
                            }
                        }
                    }
                }
			}
        }

        private static void Draw()
        {
            int width = Raylib.GetScreenWidth();
            int height = Raylib.GetScreenHeight();
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.WHITE);

            {   // Draw Player ship
                Raylib.DrawRectangle((int)player1.rect.centerPosition.X - ((int)player1.rect.size.X / 2),
                                     (int)(player1.rect.centerPosition.Y - (player1.rect.size.Y / 2)),
                                     (int)player1.rect.size.X,
                                     (int)player1.rect.size.Y,
                                     Color.BLACK);
            }
            {   // Draw the bullets
                for (int i = 0; i < MaxNumBullets; i++)
                {
                    var bullet = bullets[i];
                    if (bullet.isActive)
                    {
                        Raylib.DrawRectangle((int)bullet.rect.centerPosition.X - ((int)bullet.rect.size.X / 2),
                                             (int)(bullet.rect.centerPosition.Y - (bullet.rect.size.Y / 2)),
                                             (int)bullet.rect.size.X,
                                             (int)bullet.rect.size.Y,
                                             Color.ORANGE);
                    }
                }
            }
            {   // Draw the enemies
                for (int i = 0; i < numEnemiesThisLevel; i++)
                {
                    var enemy = enemies[i];
                    if (enemy.isActive)
                    {
                        Raylib.DrawRectangle((int)enemy.rect.centerPosition.X - ((int)enemy.rect.size.X / 2),
                                             (int)(enemy.rect.centerPosition.Y - (enemy.rect.size.Y / 2)),
                                             (int)enemy.rect.size.X,
                                             (int)enemy.rect.size.Y,
                                             Color.BLUE);
                    }
                }
            }
            {   // Draw Info
                DrawText($"Lives {numLives}", TextAlignment.Left, 15, 5, 20);

                if (IsGameOver)
                {
                    DrawText($"Game Over", TextAlignment.Center, width / 2, height / 2, 50);
                }
                if (IsWin)
                {
                    DrawText($"You Won", TextAlignment.Center, width / 2, height / 2, 50);
                }
            }

            Raylib.EndDrawing();
        }

        private static unsafe void DrawText(string text, TextAlignment alignment, int posX, int posY, int fontSize)
        {
            Color fontColor = Color.DARKGRAY;
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

        private static bool HasHitTime(ref float timeRemaining, float deltaTime)
        {
            timeRemaining -= deltaTime;
            return timeRemaining <= 0;
        }

        private static bool HasHitInterval(ref float timeRemaining, float resetTime, float deltaTime)
        {
            timeRemaining -= deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = resetTime;
                return true;
            }
            return false;
        }
    }
}