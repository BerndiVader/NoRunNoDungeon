using Godot;

public class LevelControl : Node2D
{
    [Export] private float Speed=-1f;
    [Export] private Vector2 Direction=Vector2.Zero;
    [Export] private float Zoom=-1f;
    [Export] private int Timeout=-1;
    [Export] private bool Restore=false;
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
    }

    public override void _Process(float delta)
    {
        if(GlobalPosition.x<=size.x)
        {
            SetProcess(false);
            if(!Restore)
            {
                if(settings==null)
                {
                    settings=new Settings(World.level,Direction,Speed,Zoom);
                }
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
        SettingsEffect effect=LevelControlTimer.countEffect.Instance<SettingsEffect>();
        effect.chr=(int)"!"[0];
        effect.scale=15f;
        World.instance.renderer.AddChild(effect);
        
        SetProcess(true);
    }

}
