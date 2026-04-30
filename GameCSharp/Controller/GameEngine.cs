using GameCSharp.Model;

namespace GameCSharp.Controller;

public sealed class GameEngine
{
    private readonly GameState gameState;
    private readonly InputController inputController;

    public GameEngine(GameState gameState, InputController inputController)
    {
        this.gameState = gameState;
        this.inputController = inputController;
    }

    public GameStepResult Update(float deltaTime)
    {
        var result = new GameStepResult();

        if (gameState.IsGameOver)
        {
            if (inputController.ConsumeRestartRequested())
            {
                gameState.Reset();
                result.ResetTriggered = true;
            }

            result.GameOver = gameState.IsGameOver;
            return result;
        }

        var clampedDeltaTime = Math.Min(deltaTime, 1f / 30f);
        var healthBefore = gameState.Player.Health;

        result.BulletFired = UpdatePlayer(clampedDeltaTime);
        gameState.UpdateBullets(clampedDeltaTime);
        gameState.EnemyManager.Update(gameState, clampedDeltaTime);
        result.EnemiesDestroyed = ResolveCollisions();
        result.ScoreDelta = result.EnemiesDestroyed * 100;
        result.DamageTaken = Math.Max(0, healthBefore - gameState.Player.Health);
        result.GameOver = gameState.IsGameOver;
        return result;
    }

    private bool UpdatePlayer(float deltaTime)
    {
        var direction = 0f;
        if (inputController.MoveLeft)
        {
            direction -= 1f;
        }

        if (inputController.MoveRight)
        {
            direction += 1f;
        }

        gameState.Player.X += direction * gameState.Player.MoveSpeed * deltaTime;
        gameState.Player.X = Math.Clamp(gameState.Player.X, 0f, gameState.WorldWidth - gameState.Player.Width);

        gameState.Player.AdvanceCooldown(deltaTime);
        if (inputController.Fire && gameState.Player.CanShoot)
        {
            gameState.FireBullet();
            gameState.Player.ResetShootCooldown();
            return true;
        }

        return false;
    }

    private int ResolveCollisions()
    {
        var enemiesDestroyed = 0;

        for (var bulletIndex = gameState.ActiveBullets.Count - 1; bulletIndex >= 0; bulletIndex--)
        {
            var bullet = gameState.ActiveBullets[bulletIndex];
            var bulletConsumed = false;

            for (var enemyIndex = gameState.EnemyManager.ActiveEnemies.Count - 1; enemyIndex >= 0; enemyIndex--)
            {
                var enemy = gameState.EnemyManager.ActiveEnemies[enemyIndex];
                if (!Physics.Intersects(bullet, enemy))
                {
                    continue;
                }

                gameState.ActiveBullets.RemoveAt(bulletIndex);
                gameState.EnemyManager.ActiveEnemies.RemoveAt(enemyIndex);
                gameState.AddScore(enemy.Worth);
                enemiesDestroyed++;
                bulletConsumed = true;
                break;
            }

            if (bulletConsumed)
            {
                continue;
            }
        }

        for (var enemyIndex = gameState.EnemyManager.ActiveEnemies.Count - 1; enemyIndex >= 0; enemyIndex--)
        {
            var enemy = gameState.EnemyManager.ActiveEnemies[enemyIndex];
            if (!Physics.Intersects(enemy, gameState.Player))
            {
                continue;
            }

            gameState.EnemyManager.ActiveEnemies.RemoveAt(enemyIndex);
            gameState.DamagePlayer();
        }

        return enemiesDestroyed;
    }
}