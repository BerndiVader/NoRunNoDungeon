using Godot;

public class Settings
{
    private float speed;
    private  Vector2 zoom;

    private Level level;

    public Settings(Level level)
    {
        speed=level.Speed;
        zoom=PlayerCamera.instance.Zoom;
        this.level=level;
    }

    public void restore()
    {
        level.Speed=speed;
        PlayerCamera.instance.Zoom=zoom;
    }
}
