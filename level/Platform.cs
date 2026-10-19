using Godot;
using System.Linq;

public class Platform : StaticBody2D,ISwitchable
{
    protected enum PLATFORMSTATE
    {
        NORMAL,
        ONETIME,
        SWITCH,
        ONPLAYER,
    }

    [Export(PropertyHint.Enum,"")] protected PLATFORMSTATE platformState=PLATFORMSTATE.NORMAL;
    [Export(PropertyHint.Range,"0,4,0.5")] protected float BUMP_STRENGTH=4f;
    [Export(PropertyHint.Range,"0,4,0.5")] protected float BUMP_STRENGTH_SOFT=1f;
    [Export] protected bool HIDEABLE=false;
    [Export] protected bool IS_HIDING=false;
    [Export(PropertyHint.Enum,"0,1,3")] protected PLATFORMSTATE HIDE_USAGE=PLATFORMSTATE.NORMAL;
    [Export] protected float HIDE_TIMER=5f;

    [Export] protected string switchID="";

    private const float BUMP_AREA_MARGIN=2f;
    private const float CAPSULE_MARGIN_X=8f;
    private const float CAPSULE_MARGIN_Y=4f;
    private const float PLAYER_ABOVE_OFFSET=-12f;
    private const float HIDE_TWEEN_TIME=1f;

    private Vector2 extents=Vector2.Zero;
    private Tween bumpTween;

    protected bool playerOn=false;
    protected Vector2 startPosition,lastPosition;
    
    public Vector2 CurrentSpeed;

    protected CollisionShape2D collision;
    protected Timer hidingTimer;

    private Sprite[]sprites;
    private Tween hideTween;
    private float hideAmount=0f;
    private bool isHiding;
    private bool wasActive=false;


    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        collision=GetNode<CollisionShape2D>(nameof(CollisionShape2D));

        if(BUMP_STRENGTH+BUMP_STRENGTH_SOFT!=0f)
        {
            CreateBumpArea();
        }
        if(HIDEABLE)
        {
            isHiding=IS_HIDING;
            CreateHideable();
        }

        startPosition=lastPosition=Position;
        CurrentSpeed=new Vector2(0f,0f);

        AddToGroup(GROUPS.PLATFORMS.ToString());
    }

    private void CreateHideable()
    {
        sprites=GetChildren().OfType<Sprite>().ToArray();
        Shader hideShader=GD.Load<Shader>("res://shaders/HideGhost.gdshader");
        foreach(Sprite sprite in sprites)
        {
            sprite.Material=new ShaderMaterial{Shader=hideShader};
            sprite.Visible=true;
        }

        hideTween=new Tween();
        AddChild(hideTween);

        hidingTimer=new Timer();
        hidingTimer.Connect("timeout",this,nameof(SwitchHiding));
        hidingTimer.OneShot=true;
        AddChild(hidingTimer);

        isHiding=!isHiding;
        SwitchHiding();

        switch(HIDE_USAGE)
        {
            case PLATFORMSTATE.ONETIME:
            case PLATFORMSTATE.NORMAL:
                hidingTimer.WaitTime=HIDE_TIMER;
                hidingTimer.Start();
                break;
            default:
                break;
        }
    }

    private void SetHideAmount(float value)
    {
        hideAmount=value;
        foreach(Sprite sprite in sprites)
        {
            ((ShaderMaterial)sprite.Material).SetShaderParam("hide_amount",value);
        }
    }

    private void TweenHideAmount(float target,float duration)
    {
        if(duration<=0f)
        {
            SetHideAmount(target);
            return;
        }

        hideTween.RemoveAll();
        hideTween.InterpolateMethod(this,nameof(SetHideAmount),hideAmount,target,duration,Tween.TransitionType.Linear,Tween.EaseType.InOut);
        hideTween.Start();
    }

    private void SwitchHiding()
    {
        if(isHiding&&playerOn)
        {
            hidingTimer.WaitTime=0.5f;
            hidingTimer.Start();
            return;
        }

        float duration=wasActive?HIDE_TWEEN_TIME:0f;
        wasActive=true;
        isHiding=!isHiding;
        collision.Disabled=isHiding;
        TweenHideAmount(isHiding?1f:0f,duration);
        
        if(HIDE_USAGE==PLATFORMSTATE.NORMAL)
        {
            hidingTimer.WaitTime=HIDE_TIMER;
            hidingTimer.Start();
        }
    }

    private void CreateBumpArea()
    {
        RectangleShape2D rect=new RectangleShape2D();

        if(collision.Shape is RectangleShape2D shape)
        {
            extents=shape.Extents+Vector2.One*BUMP_AREA_MARGIN;
        }
        else if(collision.Shape is CapsuleShape2D capsule)
        {
            float width=capsule.Radius*2f;
            float height=capsule.Height;
            float cos=Mathf.Abs(Mathf.Cos(collision.Rotation));
            float sin=Mathf.Abs(Mathf.Sin(collision.Rotation));
            extents=new Vector2((width*cos+height*sin)*0.5f+CAPSULE_MARGIN_X,(width*sin+height*cos)*0.5f+CAPSULE_MARGIN_Y);
        }

        if(extents!=Vector2.Zero)
        {
            rect.Extents=extents;
        }

        Area2D bumpArea=new Area2D
        {
            Name="bump",
            CollisionLayer=1,
            CollisionMask=1,
            Position=collision.Position
        };
        bumpArea.AddChild(new CollisionShape2D{Shape=rect});
        bumpArea.Connect("body_entered",this,nameof(BumpAreaEntered));
        bumpArea.Connect("body_exited",this,nameof(BumpAreaExited));
        AddChild(bumpArea);

        bumpTween=new Tween();
        AddChild(bumpTween);
    }

    protected virtual void BumpAreaEntered(Node node)
    {
        if(node is Player player)
        {
            Vector2 diff=player.GlobalPosition-GlobalPosition;
            if(Mathf.Abs(diff.x)<=extents.x||diff.y<PLAYER_ABOVE_OFFSET)
            {
                Vector2 direction=diff.y>0f?Vector2.Up:Vector2.Down;
                float strength=direction==Vector2.Down?BUMP_STRENGTH_SOFT:BUMP_STRENGTH;
                Vector2 target=Position+direction*strength;
                playerOn=direction==Vector2.Down;

                if(HIDEABLE)
                {
                    HideAreaEntered();
                }

                if(!bumpTween.IsActive())
                {
                    bumpTween.InterpolateProperty(this,"position:y",Position.y,target.y,0.1f,Tween.TransitionType.Expo,Tween.EaseType.Out);
                    bumpTween.InterpolateProperty(this,"position:y",target.y,Position.y,0.2f,Tween.TransitionType.Cubic,Tween.EaseType.In,0.1f);
                    bumpTween.Start();
                }
            }
        }
    }

    protected virtual void BumpAreaExited(Node node)
    {
        if(node is Player)
        {
            playerOn=false;

            if(HIDEABLE)
            {
                HideareaExited();
            }
        }
    }

    protected void HideAreaEntered()
    {
        if(hidingTimer.TimeLeft>0f)
        {
            return;
        }
        if(HIDE_USAGE==PLATFORMSTATE.ONPLAYER)
        {
            hidingTimer.WaitTime=HIDE_TIMER;
            hidingTimer.Start();
        }
    }

    protected void HideareaExited()
    {
    }

    protected bool BumpActive()
    {
        return bumpTween.IsActive();
    }

    public virtual void SwitchCall(string id)
    {
        return;
    }
}
