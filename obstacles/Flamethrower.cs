using Godot;
using System;
using System.Threading.Tasks;

public class Flamethrower : Area2D,ISwitchable
{
    private static readonly Vector2[] shapesize=new Vector2[]
    {
        Vector2.Zero,
        new Vector2(2f,3f),
        new Vector2(4f,6f),
        new Vector2(5f,9f),
        new Vector2(7.5f,15.5f),
        new Vector2(8f,19.5f),
        new Vector2(8f,24f),
        Vector2.Zero,        
        Vector2.Zero,        
        Vector2.Zero
    };

    private static readonly Vector2 baseOffset=new Vector2(-25f,0f);

    private enum MODE
    {
        DEFAULT,
        DISTANCE,
        SWITCH,
    }

    [Export] private MODE mode=MODE.DEFAULT;
    [Export] private float distance=20f;
    [Export] private string switchID="";
    [Export] private int delay=2;

    private AnimatedSprite animation;
    private CollisionShape2D collision;
    private RectangleShape2D collisionRect=new RectangleShape2D();
    private Timer timer;
    private int animationIndex=0;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen), new Godot.Collections.Array(this));
        AddChild(notifier2D);

        animation=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        collision=GetNode<CollisionShape2D>(nameof(CollisionShape2D));
        collision.Shape=collisionRect;
        collisionRect.Extents=shapesize[animation.Frame];

        Connect("body_entered",this,nameof(OnBodyEntered));

        switch(mode)
        {
            case MODE.DEFAULT:
                animation.Connect("animation_finished",this,nameof(OnAnimEnd));
                animation.Connect("frame_changed",this,nameof(OnFrameChanged));
                animation.Frame=0;
                collisionRect.Extents=shapesize[animation.Frame];
                animation.Play("default");

                timer=new Timer();
                timer.OneShot=true;
                timer.WaitTime=delay;
                timer.Connect("timeout",this,nameof(OnTimeOut));
                AddChild(timer);
                break;
        }
    }

    private void OnBodyEntered(Node node)
    {
        if(node is Player)
        {
            node.EmitSignal(STATE.damage.ToString(),this,1f);
            SetDeferred("monitoring",false);
        }
    }

    private async void OnAnimEnd()
    {
        SetDeferred("monitoring",false);
        animation.Stop();
        timer.Start();
    }

    private void OnTimeOut()
    {
        SetDeferred("monitoring",true);
        animation.Frame=0;
        animation.Play("default");
        collisionRect.Extents=shapesize[animation.Frame];
    }

    private void OnFrameChanged()
    {
        collisionRect.Extents=shapesize[animation.Frame];
        Vector2 offset=baseOffset;

        if(collisionRect.Extents!=Vector2.Zero)
        {
            offset.x+=collisionRect.Extents.y;
        }
        collision.Position=offset;
    }

    public void SwitchCall(string id)
    {
        throw new NotImplementedException();
    }
}
