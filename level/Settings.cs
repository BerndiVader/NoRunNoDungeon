using Godot;
using System;

public class Settings
{
    private float speed,prevSpeed;
    private readonly Vector2 zoom,prevZoom,prevPosition,direction,prevDirection;
    private bool restoreCalled=false;
    public bool autoRestore;
    public bool restoreToDefault;
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

        prevSpeed=level.speed;
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

            SceneTreeTween tween=level.GetTree().CreateTween().SetParallel().BindNode(level);

            if(speed!=-1)
            {
                tween.TweenProperty(level,"speed",speed,0.5f)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.InOut);
            }
            if(zoom.x!=-1f)
            {
                PlayerCamera.instance.GlobalPosition=Player.instance.GlobalPosition;
                tween.TweenProperty(PlayerCamera.instance,"zoom",zoom,0.5f)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.InOut);
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
                level.DEFAULT_SETTING.Set();
            }
            else
            {
                level.direction=prevDirection;

                SceneTreeTween tween=level.GetTree().CreateTween().SetParallel().BindNode(level);
                tween.TweenProperty(level,"speed",prevSpeed,0.5f)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.InOut);
                tween.TweenProperty(PlayerCamera.instance,"zoom",prevZoom,0.5f)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.InOut);
                tween.TweenProperty(PlayerCamera.instance,"position",prevPosition,0.5f)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.InOut);

            }
        }
    }

}
