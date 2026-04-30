namespace GameCSharp.Model;

public sealed class EnemyManager
{
    private const int MaxActiveEnemies = 4;

    private readonly Random random = new();
    private float spawnElapsed;
    private float nextSpawnDelay;

    public EnemyManager()
    {
        ActiveEnemies = new List<Enemy>();
        ScheduleNextSpawn();
    }

    public List<Enemy> ActiveEnemies { get; }

    public void Reset()
    {
        ActiveEnemies.Clear();
        spawnElapsed = 0f;
        ScheduleNextSpawn();
    }

    public void Update(GameState gameState, float deltaTime)
    {
        spawnElapsed += deltaTime;
        if (spawnElapsed >= nextSpawnDelay && ActiveEnemies.Count < MaxActiveEnemies)
        {
            spawnElapsed = 0f;
            SpawnEnemy(gameState.WorldWidth);
            ScheduleNextSpawn();
        }

        for (var index = ActiveEnemies.Count - 1; index >= 0; index--)
        {
            var enemy = ActiveEnemies[index];
            enemy.Update(deltaTime);

            if (enemy.Left <= 0f || enemy.Right >= gameState.WorldWidth)
            {
                enemy.VelocityX *= -1f;
                enemy.X = Math.Clamp(enemy.X, 0f, gameState.WorldWidth - enemy.Width);
            }

            if (enemy.Top > gameState.WorldHeight)
            {
                ActiveEnemies.RemoveAt(index);
                gameState.DamagePlayer();
            }
        }
    }

    private void SpawnEnemy(float worldWidth)
    {
        const float horizontalPadding = 12f;
        var availableWidth = Math.Max(horizontalPadding, worldWidth - 42f - horizontalPadding);
        var x = horizontalPadding + (float)random.NextDouble() * availableWidth;
        var velocityY = 65f + (float)random.NextDouble() * 55f;
        var velocityX = -24f + (float)random.NextDouble() * 48f;

        ActiveEnemies.Add(new Enemy(x, -48f, velocityX, velocityY));
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnDelay = 1.35f + (float)random.NextDouble() * 0.95f;
    }
}