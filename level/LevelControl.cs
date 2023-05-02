using Godot;
using System;

public class LevelControl : Node2D
{
    [Export] private float Speed=-1f;
    [Export] private float Zoom=-1f;
    [Export] private int Timeout=-1;
    [Export] private bool Restore=false;
    private VisibilityNotifier2D notifier;
    private float xSize;
    private Settings settings; 

    public override void _Ready()
    {
        notifier=new VisibilityNotifier2D();
        notifier.Connect("screen_entered",this,nameof(onScreenEntered));
        notifier.Connect("screen_exited",World.instance,nameof(World.onObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier);

        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        xSize=GetViewportRect().Size.x;
    }

    public override void _Process(float delta)
    {
        float x=(Position+World.level.GlobalPosition).x;
        if(x<=xSize*0.5)
        {
            SetProcess(false);
            if(!Restore)
            {
                settings=new Settings(World.level,Speed,Zoom);
                settings.set();
                if(Timeout!=-1)
                {
                    World.level.AddChild(new LevelControlTimer(Timeout,settings));
                }
            }
            else
            {
                World.level.settings.restore();
            }
            QueueFree();
        }

    }

    public void setMonsterControlled(Settings settings)
    {
        this.Speed=settings.speed;
        this.Zoom=settings.zoom.x;
    }

    private void onScreenEntered()
    {
        SettingsEffect effect=LevelControlTimer.countEffect.Instance<SettingsEffect>();
        effect.chr=(int)"!"[0];
        effect.scale=15f;
        World.instance.renderer.AddChild(effect);
        
        SetProcess(true);
    }

}
