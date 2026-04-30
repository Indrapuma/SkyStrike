namespace GameCSharp.Model;

public sealed class GameState
{
    public GameState(float worldWidth, float worldHeight)
    {
        ActiveBullets = new List<Bullet>();
        EnemyManager = new EnemyManager();
        WorldWidth = worldWidth;
        WorldHeight = worldHeight;
        Player = CreatePlayer();
    }

    public Player Player { get; private set; }

    public List<Bullet> ActiveBullets { get; }

    public EnemyManager EnemyManager { get; }

    public int Score { get; private set; }

    public float WorldWidth { get; private set; }

    public float WorldHeight { get; private set; }

    public bool IsGameOver => Player.Health <= 0;

    public void Resize(float worldWidth, float worldHeight)
    {
        WorldWidth = Math.Max(320f, worldWidth);
        WorldHeight = Math.Max(240f, worldHeight);
        Player.X = Math.Clamp(Player.X, 0f, WorldWidth - Player.Width);
        Player.Y = Math.Min(Player.Y, WorldHeight - Player.Height - 24f);
    }

    public void Reset()
    {
        Score = 0;
        ActiveBullets.Clear();
        EnemyManager.Reset();
        Player = CreatePlayer();
    }

    public void FireBullet()
    {
        if (IsGameOver)
        {
            return;
        }

        var bulletX = Player.X + (Player.Width / 2f) - 3f;
        var bulletY = Player.Y - 18f;
        ActiveBullets.Add(new Bullet(bulletX, bulletY));
    }

    public void UpdateBullets(float deltaTime)
    {
        for (var index = ActiveBullets.Count - 1; index >= 0; index--)
        {
            var bullet = ActiveBullets[index];
            bullet.Update(deltaTime);
            if (bullet.Bottom < 0f)
            {
                ActiveBullets.RemoveAt(index);
            }
        }
    }

    public void AddScore(int points)
    {
        Score += points;
    }

    public void DamagePlayer()
    {
        if (!IsGameOver)
        {
            Player.Health = Math.Max(0, Player.Health - 1);
        }
    }

    private Player CreatePlayer()
    {
        var x = (WorldWidth / 2f) - 26f;
        var y = WorldHeight - 92f;
        return new Player(x, y);
    }
}