namespace GameCSharp.RL;

public readonly record struct RlState(
    int PlayerLane,
    int EnemyLane,
    int EnemyDepth,
    int EnemyCount,
    int HasBullet,
    int CanShoot,
    int DangerLane,
    int HealthBucket);