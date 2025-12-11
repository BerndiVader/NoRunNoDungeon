using Godot;
using System;

public class LevelControlTimer : Node
{
    public static PackedScene countEffect;

    static LevelControlTimer()
    {
        countEffect=ResourceLoader.Load<PackedScene>("res://particles/SettingsEffect.tscn");
    }

    private SceneTreeTimer timer;
    private readonly float time;
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
        timer.Connect("timeout",this,nameof(Timeout));
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
                SettingsEffect count=countEffect.Instance<SettingsEffect>();
                count.chr=(int)(current+1).ToString()[0];
                World.instance.renderer.AddChild(count);
            }
        }
    }

    private void Timeout()
    {
        SettingsEffect count=countEffect.Instance<SettingsEffect>();
        count.chr=(int)(0).ToString()[0];        
        World.instance.renderer.AddChild(count);
        settings.Restore();
        QueueFree();
    }
}
