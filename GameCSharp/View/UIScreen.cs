using System.Drawing;
using GameCSharp.Model;

namespace GameCSharp.View;

public static class UIScreen
{
    public static void DrawHud(Graphics graphics, GameState gameState)
    {
        using var hudBrush = new SolidBrush(Color.White);
        using var hudFont = new Font("Segoe UI", 14f, FontStyle.Bold);

        graphics.DrawString($"Score: {gameState.Score}", hudFont, hudBrush, 16f, 16f);
        graphics.DrawString($"Lives: {gameState.Player.Health}", hudFont, hudBrush, 16f, 44f);
        graphics.DrawString("Move: A/D or Arrow Keys   Shoot: Space", hudFont, hudBrush, 16f, 72f);
    }

    public static void DrawGameOver(Graphics graphics, Size clientSize, int score)
    {
        using var overlayBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
        using var titleBrush = new SolidBrush(Color.White);
        using var accentBrush = new SolidBrush(Color.Gold);
        using var titleFont = new Font("Segoe UI", 28f, FontStyle.Bold);
        using var bodyFont = new Font("Segoe UI", 16f, FontStyle.Regular);
        using var centeredFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };

        graphics.FillRectangle(overlayBrush, new Rectangle(Point.Empty, clientSize));

        var centerX = clientSize.Width / 2f;
        var centerY = clientSize.Height / 2f;

        graphics.DrawString("Game Over", titleFont, titleBrush, new PointF(centerX, centerY - 48f), centeredFormat);
        graphics.DrawString($"Final Score: {score}", bodyFont, accentBrush, new PointF(centerX, centerY + 4f), centeredFormat);
        graphics.DrawString("Press R to restart", bodyFont, titleBrush, new PointF(centerX, centerY + 42f), centeredFormat);
    }
}