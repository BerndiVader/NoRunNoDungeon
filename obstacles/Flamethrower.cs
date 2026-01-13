using Godot;
using System;

public class Flamethrower : Area2D,ISwitchable
{
    private static AudioStream flameFx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_human_special_atk_2.wav");
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
    [Export] private bool damageMonster=false;
    [Export] private float FieldOfVision=45f;

    private AnimatedSprite animation;
    private CollisionShape2D collision;
    private RectangleShape2D collisionRect=new RectangleShape2D();
    private AudioStreamPlayer2D sfxPlayer=new AudioStreamPlayer2D();
    private CPUParticles2D particles;
    private Timer timer;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);

        VisibilityNotifier2D notifier2D=new VisibilityNotifier2D();
        notifier2D.Connect("screen_exited",World.instance,nameof(World.OnObjectExitedScreen),new Godot.Collections.Array(this));
        AddChild(notifier2D);

        animation=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        collision=GetNode<CollisionShape2D>(nameof(CollisionShape2D));
        collision.Shape=collisionRect;
        collisionRect.Extents=shapesize[animation.Frame];

        Connect("body_entered",this,nameof(OnBodyEntered));

        sfxPlayer.Stream=flameFx;
        AddChild(sfxPlayer);

        timer=new Timer
        {
            OneShot=true,
            WaitTime=delay
        };
        timer.Connect("timeout",this,nameof(OnTimeOut));
        AddChild(timer);

        particles=GetNode<CPUParticles2D>(nameof(CPUParticles2D));
        particles.Direction=Vector2.Left.Rotated(Rotation);

        animation.Connect("animation_finished",this,nameof(OnAnimEnd));
        animation.Connect("frame_changed",this,nameof(OnFrameChanged));
        animation.Frame=0;
        collisionRect.Extents=shapesize[animation.Frame];

        switch(mode)
        {
            case MODE.DEFAULT:
                animation.Play("default");
                break;
            case MODE.DISTANCE:
                SetPhysicsProcess(true);
            break;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if(InFOV())
        {
            float d=GlobalPosition.DistanceTo(Player.instance.GlobalPosition);
            if(d<distance)
            {
                animation.Play("default");
                SetPhysicsProcess(false);
            }
        }
    }

    private bool InFOV()
    {
        Vector2 distance=(Player.instance.GlobalPosition-collision.GlobalPosition).Normalized();
        Vector2 direction=Vector2.Right.Rotated(Rotation);

        float angle=Mathf.Rad2Deg(direction.AngleTo(distance));
        return Mathf.Abs(angle)<=FieldOfVision/2f;
    }


    private void OnBodyEntered(Node node)
    {
        if(node is Player)
        {
            node.EmitSignal(STATE.damage.ToString(),this,1f);
            SetDeferred("monitoring",false);
        }
    }

    private void OnAnimEnd()
    {
        SetDeferred("monitoring",false);
        animation.Stop();
        timer.Start();
        particles.Emitting=true;
    }

    private void OnTimeOut()
    {
        SetDeferred("monitoring",true);
        animation.Frame=0;
        collisionRect.Extents=shapesize[animation.Frame];
        particles.Emitting=false;

        switch(mode)
        {
            case MODE.DEFAULT:
                animation.Play("default");
                break;
            case MODE.DISTANCE:
                SetPhysicsProcess(true);
                break;
        }

    }

    private void OnFrameChanged()
    {
        if(animation.Frame==1)
        {
            sfxPlayer.Play();
        }
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
