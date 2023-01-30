// See http
using Raylib_cs;

namespace HelloWorld
{
    static class Program
    {
        public static void Main()
        {
            Raylib.InitWindow(800, 480, "C# Sample");

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);

                Raylib.DrawText("Hello, world!", 12, 12, 20, Color.WHITE);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}