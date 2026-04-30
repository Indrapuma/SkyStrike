using System.Globalization;
using System.Text.Json;

namespace GameCSharp.RL;

public static class RlArtifacts
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static string ArtifactsDirectory => Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "rl");

    public static string ModelPath => Path.Combine(ArtifactsDirectory, "qtable.json");

    public static string MetricsPath => Path.Combine(ArtifactsDirectory, "episode_metrics.csv");

    public static PersistedQTable LoadModel()
    {
        if (!File.Exists(ModelPath))
        {
            return new PersistedQTable();
        }

        var json = File.ReadAllText(ModelPath);
        return JsonSerializer.Deserialize<PersistedQTable>(json, JsonOptions) ?? new PersistedQTable();
    }

    public static void SaveModel(PersistedQTable model)
    {
        Directory.CreateDirectory(ArtifactsDirectory);
        var json = JsonSerializer.Serialize(model, JsonOptions);
        File.WriteAllText(ModelPath, json);
    }

    public static void EnsureMetricsFile()
    {
        Directory.CreateDirectory(ArtifactsDirectory);
        if (!File.Exists(MetricsPath))
        {
            const string header = "timestamp_utc,episode,reward,score,episode_steps,average_reward,best_reward,epsilon,total_training_steps,q_entries\n";
            File.WriteAllText(MetricsPath, header);
        }
    }

    public static void AppendEpisodeMetric(EpisodeMetricRow row)
    {
        EnsureMetricsFile();

        var fields = new[]
        {
            row.TimestampUtc.ToString("O", CultureInfo.InvariantCulture),
            row.Episode.ToString(CultureInfo.InvariantCulture),
            row.Reward.ToString("F4", CultureInfo.InvariantCulture),
            row.Score.ToString(CultureInfo.InvariantCulture),
            row.EpisodeSteps.ToString(CultureInfo.InvariantCulture),
            row.AverageReward.ToString("F4", CultureInfo.InvariantCulture),
            row.BestReward.ToString("F4", CultureInfo.InvariantCulture),
            row.ExplorationRate.ToString("F6", CultureInfo.InvariantCulture),
            row.TotalTrainingSteps.ToString(CultureInfo.InvariantCulture),
            row.QEntries.ToString(CultureInfo.InvariantCulture),
        };

        File.AppendAllText(MetricsPath, string.Join(',', fields) + Environment.NewLine);
    }
}