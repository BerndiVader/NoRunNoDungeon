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

    [Export] protected PLATFORMSTATE platformState=PLATFORMSTATE.NORMAL;
    [Export] protected string switchID="";

    protected float damage=1f;
    protected float health=1f;
    private Vector2 extents=Vector2.Zero;
    protected Vector2 startPosition,lastPosition;
    protected Area2D bumpArea=new Area2D();
    protected CollisionShape2D areaCollision=new CollisionShape2D();
    public Vector2 CurrentSpeed;
    private readonly Tween bump;
    protected bool playerOn=false;

    public Platform():base() 
    {
        areaCollision.Shape=new RectangleShape2D();
        bumpArea.Name="bump";
        bumpArea.AddChild(areaCollision);
        bump=new Tween();
    }


    public override void _Ready()
    {
        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        CollisionShape2D collision=GetNode<CollisionShape2D>(nameof(CollisionShape2D));
        bumpArea.CollisionLayer=1;
        bumpArea.CollisionMask=1;
        bumpArea.Connect("body_entered",this,nameof(BumpAreaEntered));
        bumpArea.Connect("body_exited",this,nameof(BumpAreaExited));
        if(collision.Shape is RectangleShape2D shape)
        {
            bumpArea.Position=collision.Position;
            RectangleShape2D rect=areaCollision.Shape as RectangleShape2D;
            rect.Extents=new Vector2(shape.Extents.x+2f,shape.Extents.y+2f);
            extents=rect.Extents;
        }
        AddChild(bumpArea);
        AddChild(bump);
        
        startPosition=lastPosition=Position;
        CurrentSpeed=new Vector2(0f,0f);
        
        AddToGroup(GROUPS.PLATFORMS.ToString());
    }

    protected virtual void BumpAreaEntered(Node node)
    {
        if(node is Player player)
        {
            Vector2 diff=player.GlobalPosition-GlobalPosition;
            if(Mathf.Abs(diff.x)<=extents.x)
            {
                Vector2 direction=diff.y>0f?Vector2.Up:Vector2.Down;
                Vector2 target=Position+direction*4f;
                playerOn=direction==Vector2.Down;

                if(!bump.IsActive())
                {
                    bump.InterpolateProperty(this,"position:y",Position.y,target.y,0.1f,Tween.TransitionType.Expo,Tween.EaseType.Out);
                    bump.InterpolateProperty(this,"position:y",target.y,Position.y,0.2f,Tween.TransitionType.Cubic,Tween.EaseType.In,0.1f);
                    bump.Start();
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
