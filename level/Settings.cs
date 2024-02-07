using Godot;
using System;

public class Settings
{
    public float speed,oSpeed;
    public Vector2 zoom,oZoom,oPosition,direction,oDirection;

    private WeakReference<Level> levelRef;

    public Settings(Level level) : this(level,Vector2.Zero) {}

    public Settings(Level level,Vector2 direction,float speed=-1f,float zoom=-1f)
    {
        levelRef=new WeakReference<Level>(level);
        this.zoom=new Vector2(zoom,zoom);
        this.speed=speed;
        oSpeed=level.Speed;
        oZoom=PlayerCamera.instance.Zoom;
        oPosition=PlayerCamera.instance.Position;
        oDirection=level.direction;
        this.direction=direction;

    }

    public void set()
    {
        if(levelRef.TryGetTarget(out Level level))
        {
            if(speed!=-1)
            {
                level.Speed=speed;
            }
            if(zoom.x!=-1f)
            {
                PlayerCamera.instance.Zoom=zoom;
                PlayerCamera.instance.GlobalPosition=Player.instance.GlobalPosition;
            }
            if(direction!=Vector2.Zero)
            {
                level.direction=direction;
            }
        }
    }

    public void restore()
    {
        if(levelRef.TryGetTarget(out Level level))
        {
            level.Speed=oSpeed;
            level.direction=oDirection;
            PlayerCamera.instance.Zoom=oZoom;
            PlayerCamera.instance.Position=oPosition;
        }
    }
}
