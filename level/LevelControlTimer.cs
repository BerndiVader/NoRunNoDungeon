using Godot;
using System;
using System.Linq.Expressions;

public class LevelControlTimer : Node,ISwitchable
{
    public static PackedScene countEffect=ResourceLoader.Load<PackedScene>("res://particles/SettingsEffect.tscn");

    private SceneTreeTimer timer;
    private readonly float time;
    private int current,last;
    private Settings settings;
    private string switchID="";

    public LevelControlTimer():base() {}

    public LevelControlTimer(float time, Settings settings,string id):base()
    {
        this.time=time;
        this.settings=settings;
        switchID=id;

        if(switchID!="")
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }
    }
    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        if(switchID=="")
        {
            timer=GetTree().CreateTimer(time,false);
            timer.Connect("timeout",this,nameof(Timeout));
            current=last=(int)timer.TimeLeft;
        }
        else
        {
            SetProcess(false);
        }
    }

    public override void _PhysicsProcess(float delta)
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
        count.chr="0"[0];
        World.instance.renderer.AddChild(count);
        settings.Restore();
        QueueFree();
    }

    public void SwitchCall(string id)
    {
        if(id==switchID)
        {
            SettingsEffect count=countEffect.Instance<SettingsEffect>();
            count.chr=">"[0];
            World.instance.renderer.AddChild(count);
            settings.Restore();
            QueueFree();
        }
    }
}
