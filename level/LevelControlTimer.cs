using Godot;
using System;

public class LevelControlTimer : Node
{
    private SceneTreeTimer timer;
    private float time;
    private int current,last;
    private Settings settings;
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
                GD.Print(current+1);
            }
        }
    }

    private void timeout()
    {
        settings.restore();
        QueueFree();
    }
}
