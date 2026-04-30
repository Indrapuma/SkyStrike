using System.Drawing;
using System.Drawing.Drawing2D;
using GameCSharp.Model;

namespace GameCSharp.View;

public sealed class GameRenderer
{
    private readonly List<Point> stars = new();
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
            stars.Add(new Point(random.Next(Math.Max(1, width)), random.Next(Math.Max(1, height))));
        }
    }

    public void Render(Graphics graphics, GameState gameState, Size clientSize)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var background = new LinearGradientBrush(
            new Rectangle(Point.Empty, clientSize),
            Color.FromArgb(10, 16, 34),
            Color.FromArgb(0, 0, 0),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(background, new Rectangle(Point.Empty, clientSize));

        DrawStars(graphics);
        DrawPlayer(graphics, gameState.Player);
        DrawBullets(graphics, gameState.ActiveBullets);
        DrawEnemies(graphics, gameState.EnemyManager.ActiveEnemies);
        UIScreen.DrawHud(graphics, gameState);

        if (gameState.IsGameOver)
        {
            UIScreen.DrawGameOver(graphics, clientSize, gameState.Score);
        }
    }

    private void DrawStars(Graphics graphics)
    {
        using var starBrush = new SolidBrush(Color.FromArgb(120, 230, 240, 255));
        foreach (var star in stars)
        {
            graphics.FillEllipse(starBrush, star.X, star.Y, 2, 2);
        }
    }

    private static void DrawPlayer(Graphics graphics, Player player)
    {
        var points = new[]
        {
            new PointF(player.X + (player.Width / 2f), player.Y),
            new PointF(player.X + player.Width, player.Y + player.Height),
            new PointF(player.X + (player.Width / 2f), player.Y + player.Height - 12f),
            new PointF(player.X, player.Y + player.Height),
        };

        using var bodyBrush = new SolidBrush(Color.DeepSkyBlue);
        using var canopyBrush = new SolidBrush(Color.FromArgb(220, 235, 247, 255));
        graphics.FillPolygon(bodyBrush, points);
        graphics.FillEllipse(canopyBrush, player.X + 16f, player.Y + 12f, 20f, 14f);
    }

    private static void DrawBullets(Graphics graphics, IEnumerable<Bullet> bullets)
    {
        using var bulletBrush = new SolidBrush(Color.Gold);
        foreach (var bullet in bullets)
        {
            graphics.FillRectangle(bulletBrush, bullet.Bounds);
        }
    }

    private static void DrawEnemies(Graphics graphics, IEnumerable<Enemy> enemies)
    {
        using var hullBrush = new SolidBrush(Color.IndianRed);
        using var coreBrush = new SolidBrush(Color.FromArgb(255, 250, 208));

        foreach (var enemy in enemies)
        {
            graphics.FillEllipse(hullBrush, enemy.Bounds);
            graphics.FillRectangle(coreBrush, enemy.X + 12f, enemy.Y + 12f, 18f, 18f);
        }
    }
}