using System.Numerics;
using Raylib_cs;
using GameCSharp.Model;
using GameCSharp.RL;

namespace GameCSharp.View;

public sealed class GameRenderer
{
    private readonly List<Vector2> stars = new();
    private readonly Random random = new(42);

    public GameRenderer(int width, int height)
    {
        Resize(width, height);
    }

    public void Resize(int width, int height)
    {
        stars.Clear();

        var starCount = Math.Max(30, (width * height) / 14000);
        for (var index = 0; index < starCount; index++)
        {
            stars.Add(new Vector2(random.Next(Math.Max(1, width)), random.Next(Math.Max(1, height))));
        }
    }

    public void Render(GameState gameState, int screenWidth, int screenHeight, TrainingMonitorSnapshot trainingSnapshot)
    {
        DrawBackground(screenWidth, screenHeight);
        DrawStars();
        DrawPlayer(gameState.Player);
        DrawBullets(gameState.ActiveBullets);
        DrawEnemies(gameState.EnemyManager.ActiveEnemies);
        UIScreen.DrawHud(gameState, trainingSnapshot);
        UIScreen.DrawTrainingMonitor(trainingSnapshot, screenWidth, screenHeight);

        if (gameState.IsGameOver && !trainingSnapshot.Enabled)
        {
            UIScreen.DrawGameOver(screenWidth, screenHeight, gameState.Score);
        }
    }

    private static void DrawBackground(int screenWidth, int screenHeight)
    {
        var topColor = new Color(10, 16, 34, 255);
        var bottomColor = new Color(0, 0, 0, 255);

        for (var y = 0; y < screenHeight; y++)
        {
            var blend = screenHeight <= 1 ? 0f : y / (float)(screenHeight - 1);
            var color = ColorLerp(topColor, bottomColor, blend);
            Raylib.DrawLine(0, y, screenWidth, y, color);
        }
    }

    private void DrawStars()
    {
        var starColor = new Color(230, 240, 255, 120);
        foreach (var star in stars)
        {
            Raylib.DrawCircleV(star, 1.5f, starColor);
        }
    }

    private static void DrawPlayer(Player player)
    {
        var nose = new Vector2(player.X + (player.Width / 2f), player.Y);
        var rightWing = new Vector2(player.X + player.Width, player.Y + player.Height);
        var leftWing = new Vector2(player.X, player.Y + player.Height);
        var tail = new Vector2(player.X + (player.Width / 2f), player.Y + player.Height - 12f);

        Raylib.DrawTriangle(nose, rightWing, tail, Color.SkyBlue);
        Raylib.DrawTriangle(nose, tail, leftWing, Color.SkyBlue);
        Raylib.DrawEllipse((int)(player.X + 26f), (int)(player.Y + 19f), 10f, 7f, new Color(220, 235, 247, 255));
    }

    private static void DrawBullets(IEnumerable<Bullet> bullets)
    {
        foreach (var bullet in bullets)
        {
            Raylib.DrawRectangle((int)bullet.X, (int)bullet.Y, (int)bullet.Width, (int)bullet.Height, Color.Gold);
        }
    }

    private static void DrawEnemies(IEnumerable<Enemy> enemies)
    {
        foreach (var enemy in enemies)
        {
            Raylib.DrawCircle((int)(enemy.X + (enemy.Width / 2f)), (int)(enemy.Y + (enemy.Height / 2f)), enemy.Width / 2f, Color.Maroon);
            Raylib.DrawRectangle((int)(enemy.X + 12f), (int)(enemy.Y + 12f), 18, 18, Color.Beige);
        }
    }

    private static Color ColorLerp(Color start, Color end, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return new Color(
            (int)(start.R + ((end.R - start.R) * amount)),
            (int)(start.G + ((end.G - start.G) * amount)),
            (int)(start.B + ((end.B - start.B) * amount)),
            (int)(start.A + ((end.A - start.A) * amount)));
    }
}