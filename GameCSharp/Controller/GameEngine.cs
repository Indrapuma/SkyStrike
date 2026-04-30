using System.Windows.Forms;
using GameCSharp.Model;

namespace GameCSharp.Controller;

public sealed class GameEngine : IDisposable
{
    private const float DeltaTime = 1f / 60f;

    private readonly GameState gameState;
    private readonly InputController inputController;
    private readonly Action requestFrame;
    private readonly System.Windows.Forms.Timer gameTimer;

    public GameEngine(GameState gameState, InputController inputController, Action requestFrame)
    {
        this.gameState = gameState;
        this.inputController = inputController;
        this.requestFrame = requestFrame;

        gameTimer = new System.Windows.Forms.Timer
        {
            Interval = 16,
        };
        gameTimer.Tick += (_, _) => Tick();
    }

    public void Start()
    {
        gameTimer.Start();
    }

    public void Dispose()
    {
        gameTimer.Stop();
        gameTimer.Dispose();
    }

    private void Tick()
    {
        if (gameState.IsGameOver)
        {
            if (inputController.ConsumeRestartRequested())
            {
                gameState.Reset();
            }

            requestFrame();
            return;
        }

        UpdatePlayer();
        gameState.UpdateBullets(DeltaTime);
        gameState.EnemyManager.Update(gameState, DeltaTime);
        ResolveCollisions();
        requestFrame();
    }

    private void UpdatePlayer()
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

        gameState.Player.X += direction * gameState.Player.MoveSpeed * DeltaTime;
        gameState.Player.X = Math.Clamp(gameState.Player.X, 0f, gameState.WorldWidth - gameState.Player.Width);

        gameState.Player.AdvanceCooldown(DeltaTime);
        if (inputController.Fire && gameState.Player.CanShoot)
        {
            gameState.FireBullet();
            gameState.Player.ResetShootCooldown();
        }
    }

    private void ResolveCollisions()
    {
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
    }
}