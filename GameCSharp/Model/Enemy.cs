namespace GameCSharp.Model;

public sealed class Enemy : Entity
{
    public Enemy(float x, float y, float velocityX, float velocityY)
        : base(x, y, 42f, 42f)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
    }

    public int Worth { get; } = 100;
}