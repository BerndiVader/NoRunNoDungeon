using Godot;
using System;

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
    [Export] protected string switchID="";

    private const float BUMP_AREA_MARGIN=2f;
    private const float CAPSULE_MARGIN_X=8f;
    private const float CAPSULE_MARGIN_Y=4f;
    private const float PLAYER_ABOVE_OFFSET=-12f;

    private Vector2 extents=Vector2.Zero;
    protected Vector2 startPosition,lastPosition;
    public Vector2 CurrentSpeed;
    private Tween bumpTween;
    private Area2D bumpArea;
    protected bool playerOn=false;

    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        if(BUMP_STRENGTH+BUMP_STRENGTH_SOFT!=0f)
        {
            CreateBumpArea();
        }

        startPosition=lastPosition=Position;
        CurrentSpeed=new Vector2(0f,0f);

        AddToGroup(GROUPS.PLATFORMS.ToString());
    }

    private void CreateBumpArea()
    {
        CollisionShape2D collision=GetNode<CollisionShape2D>(nameof(CollisionShape2D));
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
        else
        {
            GD.PushWarning($"{Name}: unsupported shape {collision.Shape?.GetType().Name}, bump area uses default size.");
        }

        if(extents!=Vector2.Zero)
        {
            rect.Extents=extents;
        }

        bumpTween=new Tween();

        bumpArea=new Area2D
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
        }
    }

    public virtual void SwitchCall(string id)
    {
        return;
    }
}
