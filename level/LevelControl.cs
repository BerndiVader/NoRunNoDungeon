using Godot;

public class LevelControl : Node2D
{
    [Export] private float Speed=-1f;
    [Export] private Vector2 Direction=Vector2.Zero;
    [Export] private float Zoom=-1f;
    [Export] private int Timeout=-1;
    [Export] private bool Restore=false;
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
        /*
            float dist=0f;
            if (World.level.direction.x!=0f)
            {
                dist=Mathf.Abs(size.x-GlobalPosition.x);
            }
            else if (World.level.direction.y!=0f)
            {
                dist=Mathf.Abs(size.y-GlobalPosition.y);
            }
        */

        if(GlobalPosition.x<size.x)
        {
            SetProcess(false);
            if(!Restore)
            {
                settings=new Settings(World.level,Direction,Speed,Zoom);
                settings.Set();
                if(Timeout!=-1)
                {
                    World.level.AddChild(new LevelControlTimer(Timeout,settings));
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
        Speed=settings.speed;
        Zoom=settings.zoom.x;
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
