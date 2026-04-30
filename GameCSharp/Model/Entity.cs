using System.Drawing;

namespace GameCSharp.Model;

public abstract class Entity
{
    protected Entity(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float X { get; set; }

    public float Y { get; set; }

    public float Width { get; }

    public float Height { get; }

    public float VelocityX { get; set; }

    public float VelocityY { get; set; }

    public float Left => X;

    public float Top => Y;

    public float Right => X + Width;

    public float Bottom => Y + Height;

    public RectangleF Bounds => new(X, Y, Width, Height);

    public virtual void Update(float deltaTime)
    {
        X += VelocityX * deltaTime;
        Y += VelocityY * deltaTime;
    }
}