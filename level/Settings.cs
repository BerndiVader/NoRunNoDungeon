using Godot;
using System;

public class Settings
{
    private float speed,prevSpeed;
    private Vector2 zoom,prevZoom,prevPosition,direction,prevDirection;
    private bool restoreCalled=false;
    public bool autoRestore=false;
    public bool restoreToDefault=false;
    public bool noStop=false;
    public string CallID="";
    private readonly WeakReference<Level>levelRef;

    public Settings(Level level) : this(level,Vector2.Zero) {}

    public Settings(Level level,Vector2 direction,float speed=-1f,float zoom=-1f,bool autoRestore=false,bool restoreToDefault=false,bool noStop=false)
    {
        levelRef=new WeakReference<Level>(level);
        this.zoom=new Vector2(zoom,zoom);
        this.speed=speed;
        this.direction=direction;
        this.autoRestore=autoRestore;
        this.restoreToDefault=restoreToDefault;
        this.noStop=noStop;

        prevSpeed=level.Speed;
        prevZoom=PlayerCamera.instance.Zoom;
        prevPosition=PlayerCamera.instance.Position;
        prevDirection=level.direction;
    }

    public void Set()
    {
        if(restoreCalled)
        {
            Restore();
        }
        else if(levelRef.TryGetTarget(out Level level))
        {
            level.settings=this;
            
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

    public void Restore()
    {
        restoreCalled=true;
        if(levelRef.TryGetTarget(out Level level))
        {
            if(restoreToDefault)
            {
                level.DEFAULT_SETTING.Restore();
            }
            else
            {
                level.Speed=prevSpeed;
                level.direction=prevDirection;
                PlayerCamera.instance.Zoom=prevZoom;
                PlayerCamera.instance.Position=prevPosition;
            }
        }
    }

}
