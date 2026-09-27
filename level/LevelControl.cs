using System;
using System.Collections.Generic;
using Godot;

public class LevelControl : Node2D,ISwitchable
{
    [Export] private float SPEED=-1f;
    [Export] private Vector2 DIRECTION=Vector2.Zero;
    [Export] private float ZOOM=-1f;
    [Export] private int TIMEOUT=-1;
    [Export] private bool RESTORE=false;
    [Export] private bool RESTORE_TO_DEFAULT=false;
    [Export] private bool AUTO_RESTORE=false;
    [Export] private bool NO_STOP=false;
    [Export] private bool SIGNAL=true;
    [Export] private string SWITCH_ID="";
    [Export] private string CALL_ID="";

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
            Godot.Collections.Dictionary<string,object>options=Settings.DEFAULT_LEVEL_SETTINGS.Duplicate();
            options["Dir"]=DIRECTION;
            options["Speed"]=SPEED;
            options["Zoom"]=ZOOM;
            options["AutoRestore"]=AUTO_RESTORE;
            options["NoStop"]=NO_STOP;
            options["CallID"]=CALL_ID;
            options["RestoreToDefault"]=RESTORE_TO_DEFAULT;

            settings=new Settings(World.level,options);
        }

        if(SWITCH_ID!="")
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if(GlobalPosition.x<=size.x)
        {
            SetPhysicsProcess(false);
            if(!RESTORE)
            {
                settings.Set();
                
                if(TIMEOUT!=-1||SWITCH_ID!="")
                {
                    World.level.AddChild(new LevelControlTimer(TIMEOUT,settings,SWITCH_ID));
                }
            }
            else
            {
                if(settings.CallID!="")
                {
                    GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(ISwitchable.SwitchCall),CALL_ID);
                }
                if(RESTORE_TO_DEFAULT)
                {
                    World.level.DEFAULT_SETTING.Restore();
                }
                else
                {
                    World.level.settings.Restore();
                }
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
        if(SIGNAL)
        {
            SettingsEffect effect=LevelControlTimer.COUNT_EFFECT.Instance<SettingsEffect>();
            effect.chr=(int)"!"[0];
            effect.scale=15f;
            World.instance.renderer.AddChild(effect);
        }
        
        SetPhysicsProcess(true);
    }

    public void SwitchCall(string id)
    {
        if(id==SWITCH_ID)
        {
            SettingsEffect count=LevelControlTimer.COUNT_EFFECT.Instance<SettingsEffect>();
            count.chr=">"[0];
            World.instance.renderer.AddChild(count);
            if(RESTORE_TO_DEFAULT)
            {
                World.level.DEFAULT_SETTING.Restore();
            }
            else
            {
                World.level.settings.Restore();
            }
            SWITCH_ID="";
            CallDeferred("queue_free");
        }
    }
}
