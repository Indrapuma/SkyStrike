namespace GameCSharp.Model;

public sealed class EnemyManager
{
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
        if (spawnElapsed >= nextSpawnDelay)
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
        var velocityY = 110f + (float)random.NextDouble() * 130f;
        var velocityX = -60f + (float)random.NextDouble() * 120f;

        ActiveEnemies.Add(new Enemy(x, -48f, velocityX, velocityY));
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnDelay = 0.45f + (float)random.NextDouble() * 0.7f;
    }
}