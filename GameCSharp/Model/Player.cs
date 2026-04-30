namespace GameCSharp.Model;

public sealed class Player : Entity
{
    private const float ShootIntervalSeconds = 0.22f;
    private float shootCooldownRemaining;

    public Player(float x, float y)
        : base(x, y, 52f, 52f)
    {
        Health = 3;
        WeaponLevel = 1;
        MoveSpeed = 420f;
    }

    public int Health { get; set; }

    public int WeaponLevel { get; }

    public float MoveSpeed { get; }

    public bool CanShoot => shootCooldownRemaining <= 0f;

    public void AdvanceCooldown(float deltaTime)
    {
        shootCooldownRemaining = MathF.Max(0f, shootCooldownRemaining - deltaTime);
    }

    public void ResetShootCooldown()
    {
        shootCooldownRemaining = ShootIntervalSeconds;
    }
}