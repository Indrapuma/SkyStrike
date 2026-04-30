using System.Diagnostics;
using GameCSharp.Controller;
using GameCSharp.Model;

namespace GameCSharp.RL;

public sealed class RlTrainer
{
    private static readonly RlAction[] AllActions = Enum.GetValues<RlAction>();

    private readonly GameState gameState;
    private readonly GameEngine gameEngine;
    private readonly InputController inputController;
    private readonly Dictionary<(RlState State, RlAction Action), float> qTable = new();
    private readonly Queue<float> recentEpisodeRewards = new();
    private readonly Random random = new(7);
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();

    private readonly int laneCount = 7;
    private float currentEpisodeReward;
    private int currentEpisodeSteps;
    private float lastEpisodeReward;
    private float bestEpisodeReward = float.MinValue;
    private float explorationRate = 1f;
    private long totalTrainingSteps;
    private int episodesCompleted;
    private int simulationStepsPerFrame = 20;

    public RlTrainer(GameState gameState, GameEngine gameEngine, InputController inputController)
    {
        this.gameState = gameState;
        this.gameEngine = gameEngine;
        this.inputController = inputController;

        LoadPersistedModel();
        RlArtifacts.EnsureMetricsFile();
    }

    public bool Enabled { get; private set; }

    public bool InferenceEnabled { get; private set; }

    public bool HasLearnedPolicy => qTable.Count > 0;

    public string ModelPath => RlArtifacts.ModelPath;

    public void ToggleEnabled()
    {
        Enabled = !Enabled;
        if (Enabled)
        {
            InferenceEnabled = false;
            inputController.SetAgentControl(true);
        }
        else
        {
            FlushArtifacts();
            inputController.SetAgentControl(false);
            inputController.ClearAgentAction();
        }
    }

    public void SetInferenceEnabled(bool enabled)
    {
        InferenceEnabled = enabled;
        if (enabled)
        {
            Enabled = false;
            inputController.SetAgentControl(true);
        }
        else
        {
            inputController.SetAgentControl(false);
            inputController.ClearAgentAction();
        }
    }

    public void IncreaseSimulationSpeed()
    {
        simulationStepsPerFrame = Math.Min(200, simulationStepsPerFrame + 5);
    }

    public void DecreaseSimulationSpeed()
    {
        simulationStepsPerFrame = Math.Max(1, simulationStepsPerFrame - 5);
    }

    public void Update(float frameTime)
    {
        if (!Enabled)
        {
            return;
        }

        var simulationDelta = Math.Min(frameTime, 1f / 60f);
        if (simulationDelta <= 0f)
        {
            simulationDelta = 1f / 60f;
        }

        for (var stepIndex = 0; stepIndex < simulationStepsPerFrame; stepIndex++)
        {
            if (gameState.IsGameOver)
            {
                CompleteEpisode();
                gameState.Reset();
            }

            var state = CaptureState(gameState);
            var previousDistance = GetNearestEnemyDistance(gameState);
            var action = ChooseAction(state);

            inputController.ApplyAgentAction(action);
            var result = gameEngine.Update(simulationDelta);
            var nextState = CaptureState(gameState);
            var nextDistance = GetNearestEnemyDistance(gameState);
            var reward = CalculateReward(result, action, previousDistance, nextDistance);

            Learn(state, action, reward, nextState, result.GameOver);

            currentEpisodeReward += reward;
            currentEpisodeSteps++;
            totalTrainingSteps++;

            if (result.GameOver)
            {
                CompleteEpisode();
                gameState.Reset();
            }
        }
    }

    public void UpdateInference(float frameTime)
    {
        if (!InferenceEnabled)
        {
            return;
        }

        if (gameState.IsGameOver)
        {
            gameState.Reset();
        }

        var simulationDelta = Math.Min(frameTime, 1f / 30f);
        if (simulationDelta <= 0f)
        {
            simulationDelta = 1f / 60f;
        }

        if (HasLearnedPolicy)
        {
            var action = ChooseBestAction(CaptureState(gameState));
            inputController.ApplyAgentAction(action);
        }
        else
        {
            inputController.ClearAgentAction();
        }

        gameEngine.Update(simulationDelta);
    }

    public TrainingMonitorSnapshot GetMonitorSnapshot()
    {
        var averageReward = recentEpisodeRewards.Count == 0 ? 0f : recentEpisodeRewards.Average();
        var bestReward = bestEpisodeReward == float.MinValue ? 0f : bestEpisodeReward;
        var elapsedSeconds = Math.Max(1e-6, stopwatch.Elapsed.TotalSeconds);
        var trainingStepsPerSecond = (float)(totalTrainingSteps / elapsedSeconds);
        return new TrainingMonitorSnapshot(
            Enabled,
            InferenceEnabled,
            HasLearnedPolicy,
            simulationStepsPerFrame,
            episodesCompleted,
            currentEpisodeReward,
            lastEpisodeReward,
            averageReward,
            bestReward,
            explorationRate,
            totalTrainingSteps,
            trainingStepsPerSecond,
            qTable.Count,
            recentEpisodeRewards.ToArray());
    }

    public void FlushArtifacts()
    {
        SavePersistedModel();
    }

    private void CompleteEpisode()
    {
        episodesCompleted++;
        lastEpisodeReward = currentEpisodeReward;
        bestEpisodeReward = Math.Max(bestEpisodeReward, currentEpisodeReward);
        recentEpisodeRewards.Enqueue(currentEpisodeReward);
        if (recentEpisodeRewards.Count > 60)
        {
            recentEpisodeRewards.Dequeue();
        }

        var averageReward = recentEpisodeRewards.Count == 0 ? 0f : recentEpisodeRewards.Average();
        var bestReward = bestEpisodeReward == float.MinValue ? 0f : bestEpisodeReward;

        RlArtifacts.AppendEpisodeMetric(new EpisodeMetricRow
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            Episode = episodesCompleted,
            Reward = currentEpisodeReward,
            Score = gameState.Score,
            EpisodeSteps = currentEpisodeSteps,
            AverageReward = averageReward,
            BestReward = bestReward,
            ExplorationRate = explorationRate,
            TotalTrainingSteps = totalTrainingSteps,
            QEntries = qTable.Count,
        });

        SavePersistedModel();

        currentEpisodeReward = 0f;
        currentEpisodeSteps = 0;
        explorationRate = Math.Max(0.05f, explorationRate * 0.995f);
        inputController.ClearAgentAction();
    }

    private RlAction ChooseAction(RlState state)
    {
        if (random.NextDouble() < explorationRate)
        {
            return AllActions[random.Next(AllActions.Length)];
        }

        var bestValue = float.NegativeInfinity;
        var bestAction = RlAction.Idle;
        foreach (var action in AllActions)
        {
            var qValue = GetQValue(state, action);
            if (qValue > bestValue)
            {
                bestValue = qValue;
                bestAction = action;
            }
        }

        return bestAction;
    }

    private RlAction ChooseBestAction(RlState state)
    {
        var bestValue = float.NegativeInfinity;
        var bestAction = RlAction.Idle;
        foreach (var action in AllActions)
        {
            var qValue = GetQValue(state, action);
            if (qValue > bestValue)
            {
                bestValue = qValue;
                bestAction = action;
            }
        }

        return bestAction;
    }

    private void Learn(RlState state, RlAction action, float reward, RlState nextState, bool terminal)
    {
        const float learningRate = 0.14f;
        const float discountFactor = 0.96f;

        var current = GetQValue(state, action);
        var nextBest = terminal ? 0f : AllActions.Max(nextAction => GetQValue(nextState, nextAction));
        var updated = current + (learningRate * (reward + (discountFactor * nextBest) - current));
        qTable[(state, action)] = updated;
    }

    private float CalculateReward(GameStepResult result, RlAction action, float previousDistance, float nextDistance)
    {
        var reward = 0.015f;

        reward += result.EnemiesDestroyed * 4.5f;
        reward -= result.DamageTaken * 6f;

        if (result.BulletFired)
        {
            reward -= 0.03f;
        }

        if (!float.IsNaN(previousDistance) && !float.IsNaN(nextDistance))
        {
            reward += (previousDistance - nextDistance) * 0.012f;
        }

        if (result.GameOver)
        {
            reward -= 10f;
        }

        if (action == RlAction.Idle && gameState.EnemyManager.ActiveEnemies.Count > 0)
        {
            reward -= 0.01f;
        }

        return reward;
    }

    private RlState CaptureState(GameState state)
    {
        var playerCenterX = state.Player.X + (state.Player.Width / 2f);
        var playerLane = Quantize(playerCenterX, state.WorldWidth, laneCount);

        var nearestEnemy = state.EnemyManager.ActiveEnemies
            .OrderBy(enemy => Math.Abs((enemy.X + (enemy.Width / 2f)) - playerCenterX) + (state.WorldHeight - enemy.Y))
            .FirstOrDefault();

        var enemyLane = nearestEnemy is null ? laneCount : Quantize(nearestEnemy.X + (nearestEnemy.Width / 2f), state.WorldWidth, laneCount);
        var enemyDepth = nearestEnemy is null ? 0 : Quantize(nearestEnemy.Y, state.WorldHeight, 5);
        var enemyCount = Math.Min(3, state.EnemyManager.ActiveEnemies.Count);
        var hasBullet = state.ActiveBullets.Count > 0 ? 1 : 0;
        var canShoot = state.Player.CanShoot ? 1 : 0;
        var dangerLane = nearestEnemy is not null && nearestEnemy.Y > (state.WorldHeight * 0.58f) ? enemyLane : laneCount;
        var healthBucket = Math.Max(0, state.Player.Health - 1);

        return new RlState(playerLane, enemyLane, enemyDepth, enemyCount, hasBullet, canShoot, dangerLane, healthBucket);
    }

    private float GetNearestEnemyDistance(GameState state)
    {
        if (state.EnemyManager.ActiveEnemies.Count == 0)
        {
            return float.NaN;
        }

        var playerCenterX = state.Player.X + (state.Player.Width / 2f);
        return state.EnemyManager.ActiveEnemies.Min(enemy => Math.Abs((enemy.X + (enemy.Width / 2f)) - playerCenterX));
    }

    private float GetQValue(RlState state, RlAction action)
    {
        return qTable.TryGetValue((state, action), out var value) ? value : 0f;
    }

    private void LoadPersistedModel()
    {
        var persisted = RlArtifacts.LoadModel();

        qTable.Clear();
        foreach (var entry in persisted.Entries)
        {
            qTable[(entry.State, entry.Action)] = entry.Value;
        }

        totalTrainingSteps = persisted.TotalTrainingSteps;
        episodesCompleted = persisted.EpisodesCompleted;
        lastEpisodeReward = persisted.LastEpisodeReward;
        bestEpisodeReward = persisted.BestEpisodeReward == 0f && persisted.Entries.Count == 0
            ? float.MinValue
            : persisted.BestEpisodeReward;
        explorationRate = Math.Clamp(persisted.ExplorationRate, 0.05f, 1f);

        recentEpisodeRewards.Clear();
        foreach (var reward in persisted.RecentEpisodeRewards.TakeLast(60))
        {
            recentEpisodeRewards.Enqueue(reward);
        }
    }

    private void SavePersistedModel()
    {
        if (qTable.Count == 0)
        {
            return;
        }

        var persisted = new PersistedQTable
        {
            TotalTrainingSteps = totalTrainingSteps,
            EpisodesCompleted = episodesCompleted,
            ExplorationRate = explorationRate,
            BestEpisodeReward = bestEpisodeReward == float.MinValue ? 0f : bestEpisodeReward,
            LastEpisodeReward = lastEpisodeReward,
            RecentEpisodeRewards = recentEpisodeRewards.ToList(),
            SavedAtUtc = DateTimeOffset.UtcNow,
            Entries = qTable.Select(entry => new PersistedQValue
            {
                State = entry.Key.State,
                Action = entry.Key.Action,
                Value = entry.Value,
            }).ToList(),
        };

        RlArtifacts.SaveModel(persisted);
    }

    private static int Quantize(float value, float maxValue, int bucketCount)
    {
        if (bucketCount <= 1 || maxValue <= 0f)
        {
            return 0;
        }

        var normalized = Math.Clamp(value / maxValue, 0f, 0.9999f);
        return (int)(normalized * bucketCount);
    }
}