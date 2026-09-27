using Godot;
using System;

public class Settings
{
    public static readonly Godot.Collections.Dictionary<string,object> DEFAULT_LEVEL_SETTINGS=new Godot.Collections.Dictionary<string,object>()
    {
        {"Use",false},
        {"Dir",Vector2.Zero},
        {"Speed",-1.0f},
        {"Zoom",-1.0f},
        {"RestoreToDefault",false},
        {"DefaultOnly",false},
        {"RestoreOnly",false},
        {"AutoRestore",false},
        {"NoStop",false},
        {"CallID",string.Empty}
    };

    public static void Populate(Godot.Collections.Dictionary<string,object>settings)
    {
        foreach(string key in DEFAULT_LEVEL_SETTINGS.Keys)
        {
            if(!settings.ContainsKey(key))
            {
                settings[key]=DEFAULT_LEVEL_SETTINGS[key];
            }
        }
    }

    public static bool Usable(Godot.Collections.Dictionary<string,object>settings)
    {
        return (bool)settings["Use"];
    }

    private bool use,defaultOnly,restoreOnly;
    private float speed,prevSpeed;
    private readonly Vector2 zoom,prevZoom,direction,prevDirection,prevPosition;
    public bool autoRestore;
    public bool restoreToDefault;
    public bool noStop=false;
    public string CallID="";

    private readonly WeakReference<Level>levelRef;
    public bool restoreCalled=false;

    public Settings(Level level,Godot.Collections.Dictionary<string,object>settings)
    {
        prevSpeed=level.speed;
        prevZoom=PlayerCamera.instance.Zoom;
        prevPosition=PlayerCamera.instance.Position;
        prevDirection=level.direction;        

        levelRef=new WeakReference<Level>(level);

        use=(bool)settings["Use"];
        direction=(Vector2)settings["Dir"];
        speed=(float)settings["Speed"];
        zoom=new Vector2((float)settings["Zoom"],(float)settings["Zoom"]);
        restoreToDefault=(bool)settings["RestoreToDefault"];
        defaultOnly=(bool)settings["DefaultOnly"];
        restoreOnly=(bool)settings["RestoreOnly"];
        autoRestore=(bool)settings["AutoRestore"];
        noStop=(bool)settings["NoStop"];
        CallID=(string)settings["CallID"];
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
                level.DEFAULT_SETTING.Restore();
            }
            else if(restoreOnly)
            {
                level.settings.Restore();
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
                PlayerCamera.instance.Position=prevPosition;

                /*

                tween.TweenProperty(PlayerCamera.instance,"position",prevPosition,0.5f)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.InOut);
                    
                */

            }
        }
    }

}
