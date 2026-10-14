using System;
using System.Collections.Generic;
using Godot;

public class LevelControl : Node2D,ISwitchable
{
    [Export] private float ACTIVATION_DISTANCE=-1f;
    [Export] private bool SET_LEVEL_POSITION=false;
    [Export] private bool SET_LEVEL_Y0=false;
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
    [Export] private Gamestate STATE=Gamestate.KEEP;

    protected Vector2 size;
    protected Settings settings;

    private bool activate;

    private float ACTIVATION_DISTANCE_SQRT;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier=GetNode<VisibilityNotifier2D>(nameof(VisibilityNotifier2D));
        notifier.Connect("screen_entered",this,nameof(OnScreenEntered));
        notifier.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));

        ACTIVATION_DISTANCE_SQRT=ACTIVATION_DISTANCE*ACTIVATION_DISTANCE;

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
            options["State"]=STATE;

            settings=new Settings(World.level,options);
        }

        if(SWITCH_ID!="")
        {
            AddToGroup(GROUPS.SWITCHABLES.ToString());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if(Player.instance.Teleport())
        {
            return;
        }

        if(ACTIVATION_DISTANCE==-1f)
        {
            activate=World.level.direction.x!=0f
                ?GlobalPosition.x*World.level.direction.x>=size.x*World.level.direction.x
                :World.level.direction.y==0f||GlobalPosition.y*World.level.direction.y>=size.y*World.level.direction.y;
        }
        else
        {
            activate=GlobalPosition.DistanceSquaredTo(Player.instance.GlobalPosition)<ACTIVATION_DISTANCE_SQRT;
        }

        if(activate)
        {
            if(settings.CallID!="")
            {
                GetTree().CallGroup(GROUPS.SWITCHABLES.ToString(),nameof(ISwitchable.SwitchCall),CALL_ID);
            }
            SetPhysicsProcess(false);

			if(SET_LEVEL_POSITION)
            {
                Vector2 offset=World.level.ToLocal(Player.instance.GlobalPosition);
                Vector2 screenPos=GetViewport().CanvasTransform*Player.instance.GlobalPosition;
                Vector2 leveloffset=offset-=screenPos;
                leveloffset+=World.level.Position;
                PlayerCamera.instance.FixedCamMode();
                PlayerCamera.instance.ResetSmoothing();                
                World.level.Position-=leveloffset;
                Player.instance.Position-=leveloffset;

                if(SET_LEVEL_Y0)
                {
                    SceneTreeTween tween=GetTree().CreateTween();
                    tween.TweenProperty(World.level,"position:y",0f,1f)
                        .SetTrans(Tween.TransitionType.Cubic)
                        .SetEase(Tween.EaseType.InOut);
                }

            }            
 
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

    protected virtual void OnScreenEntered()
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

    public virtual void SwitchCall(string id)
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
