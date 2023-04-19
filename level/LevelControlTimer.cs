using Godot;
using System;

public class LevelControlTimer : Node
{
    static PackedScene countEffect;

    static LevelControlTimer()
    {
        countEffect=ResourceLoader.Load<PackedScene>("res://particles/LevelTimerCount.tscn");
    }

    private SceneTreeTimer timer;
    private float time;
    private int current,last;
    private Settings settings;

    public LevelControlTimer():base() {}

    public LevelControlTimer(float time, Settings settings):base()
    {
        this.time=time;
        this.settings=settings;
    }
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcessInput(false);
        timer=GetTree().CreateTimer(time,false);
        timer.Connect("timeout",this,nameof(timeout));
        current=last=(int)timer.TimeLeft;
    }

    public override void _Process(float delta)
    {
        current=(int)timer.TimeLeft;
        if(current<last)
        {
            last=current;
            if(current<5)
            {
                LevelTimerCount count=countEffect.Instance<LevelTimerCount>();
                count.chr=current+1;
                World.instance.AddChild(count);
            }
        }
    }

    private void timeout()
    {
        LevelTimerCount count=countEffect.Instance<LevelTimerCount>();
        count.chr=0;        
        World.instance.AddChild(count);
        settings.restore();
        QueueFree();
    }
}
