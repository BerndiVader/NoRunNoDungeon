using Godot;
using System;

public class Settings
{
    private float speed,oSpeed;
    private  Vector2 zoom,oZoom,oPosition;

    private WeakReference<Level> levelRef;

    public Settings(Level level,float speed=-1f,float zoom=-1f)
    {
        this.levelRef=new WeakReference<Level>(level);
        this.zoom=new Vector2(zoom,zoom);
        this.speed=speed;
        oSpeed=level.Speed;
        oZoom=PlayerCamera.instance.Zoom;
        oPosition=PlayerCamera.instance.Position;

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
        }
    }

    public void restore()
    {
        if(levelRef.TryGetTarget(out Level level))
        {
            level.Speed=oSpeed;
            PlayerCamera.instance.Zoom=oZoom;
            PlayerCamera.instance.Position=oPosition;
        }
    }
}
