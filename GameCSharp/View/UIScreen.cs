using Raylib_cs;
using GameCSharp.Model;
using GameCSharp.RL;

namespace GameCSharp.View;

public static class UIScreen
{
    public static void DrawHud(GameState gameState, TrainingMonitorSnapshot trainingSnapshot)
    {
        Raylib.DrawText($"Score: {gameState.Score}", 16, 16, 28, Color.White);
        Raylib.DrawText($"Lives: {gameState.Player.Health}", 16, 48, 28, Color.White);
        var controlsLine = trainingSnapshot.Enabled
            ? "Training: ON   Toggle: T   Speed: [ / ]"
            : "Move: A/D or Arrow Keys   Shoot: Space   Train: T";
        Raylib.DrawText(controlsLine, 16, 82, 24, Color.White);
    }

    public static void DrawGameOver(int screenWidth, int screenHeight, int score)
    {
        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 180));

        const int titleSize = 48;
        const int bodySize = 28;

        var title = "Game Over";
        var scoreLine = $"Final Score: {score}";
        var restartLine = "Press R to restart";

        var centerX = screenWidth / 2;
        var centerY = screenHeight / 2;

        Raylib.DrawText(title, centerX - (Raylib.MeasureText(title, titleSize) / 2), centerY - 64, titleSize, Color.White);
        Raylib.DrawText(scoreLine, centerX - (Raylib.MeasureText(scoreLine, bodySize) / 2), centerY + 4, bodySize, Color.Gold);
        Raylib.DrawText(restartLine, centerX - (Raylib.MeasureText(restartLine, bodySize) / 2), centerY + 42, bodySize, Color.White);
    }

    public static void DrawTrainingMonitor(TrainingMonitorSnapshot snapshot, int screenWidth, int screenHeight)
    {
        const int panelWidth = 320;
        const int panelHeight = 248;
        const int padding = 14;

        var panelX = screenWidth - panelWidth - 18;
        var panelY = 18;

        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(6, 10, 18, 220));
        Raylib.DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, new Color(80, 160, 255, 180));

        var titleColor = snapshot.Enabled ? Color.Lime : Color.LightGray;
        Raylib.DrawText(snapshot.Enabled ? "RL Training Monitor" : "RL Monitor (idle)", panelX + padding, panelY + 12, 24, titleColor);

        var lines = new[]
        {
            $"Episodes: {snapshot.EpisodesCompleted}",
            $"Episode reward: {snapshot.CurrentEpisodeReward:F2}",
            $"Last reward: {snapshot.LastEpisodeReward:F2}",
            $"Avg reward: {snapshot.AverageEpisodeReward:F2}",
            $"Best reward: {snapshot.BestEpisodeReward:F2}",
            $"Epsilon: {snapshot.ExplorationRate:F3}",
            $"Sim steps/frame: {snapshot.SimulationStepsPerFrame}",
            $"Train steps/sec: {snapshot.TrainingStepsPerSecond:F0}",
            $"Q entries: {snapshot.LearnedStates}",
            $"Total train steps: {snapshot.TotalTrainingSteps}",
        };

        for (var index = 0; index < lines.Length; index++)
        {
            Raylib.DrawText(lines[index], panelX + padding, panelY + 46 + (index * 18), 18, Color.White);
        }

        DrawRewardChart(snapshot, panelX + padding, panelY + 176, panelWidth - (padding * 2), 56);
    }

    private static void DrawRewardChart(TrainingMonitorSnapshot snapshot, int x, int y, int width, int height)
    {
        Raylib.DrawRectangleLines(x, y, width, height, new Color(90, 90, 120, 180));

        var rewards = snapshot.RecentEpisodeRewards;
        if (rewards.Count < 2)
        {
            Raylib.DrawText("Reward history appears here", x + 8, y + 18, 16, Color.Gray);
            return;
        }

        var minReward = rewards.Min();
        var maxReward = rewards.Max();
        var range = Math.Max(1f, maxReward - minReward);
        var baselineY = y + height - 1;
        var positiveY = y + (height / 2);

        Raylib.DrawLine(x, positiveY, x + width, positiveY, new Color(70, 70, 95, 180));

        for (var index = 1; index < rewards.Count; index++)
        {
            var previousX = x + (int)((index - 1) * (width - 1f) / (rewards.Count - 1f));
            var currentX = x + (int)(index * (width - 1f) / (rewards.Count - 1f));

            var previousBlend = (rewards[index - 1] - minReward) / range;
            var currentBlend = (rewards[index] - minReward) / range;

            var previousY = baselineY - (int)(previousBlend * (height - 4));
            var currentY = baselineY - (int)(currentBlend * (height - 4));

            Raylib.DrawLine(previousX, previousY, currentX, currentY, Color.Gold);
        }
    }
}