namespace GameCSharp.Controller;

public sealed class GameStepResult
{
    public int ScoreDelta { get; set; }

    public int DamageTaken { get; set; }

    public int EnemiesDestroyed { get; set; }

    public bool BulletFired { get; set; }

    public bool GameOver { get; set; }

    public bool ResetTriggered { get; set; }
}