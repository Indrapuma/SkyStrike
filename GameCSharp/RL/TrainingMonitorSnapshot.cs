namespace GameCSharp.RL;

public sealed record TrainingMonitorSnapshot(
    bool Enabled,
    bool InferenceEnabled,
    bool HasLearnedPolicy,
    int SimulationStepsPerFrame,
    int EpisodesCompleted,
    float CurrentEpisodeReward,
    float LastEpisodeReward,
    float AverageEpisodeReward,
    float BestEpisodeReward,
    float ExplorationRate,
    long TotalTrainingSteps,
    float TrainingStepsPerSecond,
    int LearnedStates,
    IReadOnlyList<float> RecentEpisodeRewards);