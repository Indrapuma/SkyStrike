namespace GameCSharp.Model;

public static class Physics
{
    public static bool Intersects(Entity first, Entity second)
    {
        return first.Left < second.Right
            && first.Right > second.Left
            && first.Top < second.Bottom
            && first.Bottom > second.Top;
    }
}