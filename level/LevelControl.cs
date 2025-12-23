using System;
using Godot;

public class LevelControl : Node2D,ISwitchable
{
    [Export] private float Speed=-1f;
    [Export] private Vector2 Direction=Vector2.Zero;
    [Export] private float Zoom=-1f;
    [Export] private int Timeout=-1;
    [Export] private bool Restore=false;
    [Export] private bool Signal=true;
    [Export] private string switchID="";

    private VisibilityNotifier2D notifier;
    private Vector2 size;
    private Settings settings; 

    public override void _Ready()
    {
        notifier=new VisibilityNotifier2D();
        notifier.Connect("screen_entered",this,nameof(OnScreenEntered));
        notifier.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier);

        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        size=GetViewportRect().Size*0.5f;
        if(settings==null)
        {
            settings=new Settings(World.level,Direction,Speed,Zoom);
        }

        if(switchID!="")
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if(GlobalPosition.x<=size.x)
        {
            SetPhysicsProcess(false);
            if(!Restore)
            {
                settings.Set();
                if(Timeout!=-1||switchID!="")
                {
                    World.level.AddChild(new LevelControlTimer(Timeout,settings,switchID));
                }
            }
            else
            {
                World.level.settings.Restore();
            }
            QueueFree();
        }

    }

    public void SetMonsterControlled(Settings settings)
    {
        this.settings=settings;
    }

    private void OnScreenEntered()
    {
        if(Signal)
        {
            SettingsEffect effect=LevelControlTimer.countEffect.Instance<SettingsEffect>();
            effect.chr=(int)"!"[0];
            effect.scale=15f;
            World.instance.renderer.AddChild(effect);
        }
        
        SetPhysicsProcess(true);
    }

    public void SwitchCall(string id)
    {
        if(id==switchID)
        {
            SettingsEffect count=LevelControlTimer.countEffect.Instance<SettingsEffect>();
            count.chr=">"[0];
            World.instance.renderer.AddChild(count);
            World.level.settings.Restore();
            switchID="";
            CallDeferred("queue_free");
        }
    }
}
