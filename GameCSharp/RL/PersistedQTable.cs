namespace GameCSharp.RL;

public sealed class PersistedQTable
{
    public List<PersistedQValue> Entries { get; set; } = new();

    public long TotalTrainingSteps { get; set; }

    public int EpisodesCompleted { get; set; }

    public float ExplorationRate { get; set; } = 1f;

    public float BestEpisodeReward { get; set; }

    public float LastEpisodeReward { get; set; }

    public List<float> RecentEpisodeRewards { get; set; } = new();

    public DateTimeOffset SavedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PersistedQValue
{
    public RlState State { get; set; }

    public RlAction Action { get; set; }

    public float Value { get; set; }
}

public sealed class EpisodeMetricRow
{
    public DateTimeOffset TimestampUtc { get; set; }

    public int Episode { get; set; }

    public float Reward { get; set; }

    public int Score { get; set; }

    public int EpisodeSteps { get; set; }

    public float AverageReward { get; set; }

    public float BestReward { get; set; }

    public float ExplorationRate { get; set; }

    public long TotalTrainingSteps { get; set; }

    public int QEntries { get; set; }
}