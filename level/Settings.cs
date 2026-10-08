using Godot;
using System;
using System.Collections.Generic;

public class Settings
{
    private static SceneTreeTween tween;

    private class Property
    {
        public readonly Vector2 direction;
        public readonly float speed;
        public readonly float zoom;
        public readonly Vector2 position;

        public Property()
        {
            direction=Vector2.Zero;
            speed=-1f;
            zoom=-1f;
            position=Vector2.Zero;
        }

        public Property(Vector2 direction,float speed,float zoom,Vector2 position)
        {
            this.direction=direction;
            this.speed=speed;
            this.zoom=zoom;
            this.position=position;
        }
    }

    private static readonly Queue<Property>PROPERTIES=new Queue<Property>();

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
        return settings.TryGetValue("Use",out object obj)&&obj is bool use&&use;
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
        if(restoreCalled||restoreOnly)
        {
            Restore();
        }
        else if(levelRef.TryGetTarget(out Level level))
        {
            level.settings=this;
            AddTweenProperty(level,new Property(direction,speed,zoom.x,Vector2.Zero));
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
                AddTweenProperty(level,new Property(prevDirection,prevSpeed,prevZoom.x,prevPosition));
            }
        }
    }

    private static void AddTweenProperty(Level level,Property props)
    {
        PROPERTIES.Enqueue(props);
        if(tween==null||!tween.IsValid()||!tween.IsRunning())
        {
            NewPropertyTween(level);
        }
    }

    public static void NewPropertyTween(Level level)
    {
        if(PROPERTIES.Count==0||!Godot.Object.IsInstanceValid(World.instance))
        {
            return;
        }

        Property props=PROPERTIES.Dequeue();
        float speed=props.speed;
        Vector2 direction=props.direction;
        Vector2 zoom=new Vector2(props.zoom,props.zoom);
        Vector2 position=props.position;

        if(!Godot.Object.IsInstanceValid(level))
        {
            level=World.level;
        }

        tween=World.instance.GetTree().CreateTween().SetParallel().BindNode(World.instance);

        if(speed!=-1)
        {
            tween.TweenProperty(level,"speed",speed,0.25f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
        }
        if(zoom.x!=-1f)
        {
            if(zoom.x>1f)
            {
                PlayerCamera.instance.LimitRight=(int)(PlayerCamera.instance.LimitRight*zoom.x);
                PlayerCamera.instance.LimitBottom=(int)(PlayerCamera.instance.LimitBottom*zoom.x);
            }
            else
            {
                PlayerCamera.instance.LimitRight=(int)World.RESOLUTION.x;
                PlayerCamera.instance.LimitBottom=(int)World.RESOLUTION.y;

            }
            if(position==Vector2.Zero)
            {
                PlayerCamera.instance.GlobalPosition=Player.instance.GlobalPosition;
            }
            else
            {
                PlayerCamera.instance.GlobalPosition=position;
            }
            tween.TweenProperty(PlayerCamera.instance,"zoom",zoom,0.25f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
        }
        if(direction!=Vector2.Zero)
        {
            level.direction=direction;
        }
        tween.Chain().TweenCallback(level,nameof(Level.OnSettingsTweenCompleted));
    }

    public static void Stop()
    {
        PROPERTIES.Clear();
        tween?.Kill();
    }

}
