namespace GameCSharp.Model;

public sealed class Bullet : Entity
{
    public Bullet(float x, float y)
        : base(x, y, 6f, 18f)
    {
        VelocityY = -520f;
    }
}