using Godot;
using System;

public abstract class BuffThrowable : PhysicsObject
{
    protected static readonly AudioStream sfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/PickUp/Retro PickUp 18.wav");

    [Export] protected Vector2 VELOCITY=Vector2.Zero;
    [Export] protected bool DeBuff=false;

    protected Sprite sprite;

    public override void _Ready()
    {
        base._Ready();
        GetNode<Area2D>(nameof(Area2D)).Connect("body_entered",this,nameof(OnBodyEntered));        
        sprite=GetNode<Sprite>(nameof(Sprite));

        velocity=VELOCITY;
    }


    public override void _PhysicsProcess(float delta)
    {
        velocity+=GRAVITY*delta;
        KinematicCollision2D collision=MoveAndCollide(velocity*delta);

        if(collision!=null)
        {
            Vector2 bounce=velocity.Bounce(collision.Normal)*FRICTION;

            if((bounce-velocity).Length()>80f)
            {
                PlayFx();
            }

            velocity=bounce;

            Node node=(Node)collision.Collider;
            if(node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider=(Platform)node;
                velocity.x+=collider.CurrentSpeed.x*0.36f;
            }
        }
    }

    protected void PlayFx()
    {
        PotionGrounded particles=PotionGrounded.Create();
        particles.Position=new Vector2(Position.x,Position.y+5f);

        World.level.AddChild(particles);
    }

    public abstract void Apply();

    protected virtual void OnBodyEntered(Node body) 
    {
        if(body is Player)
        {
            ItemTaken taken=ResourceUtils.particles[(int)PARTICLES.ITEMTAKEN].Instance<ItemTaken>();
            taken.Scale=new Vector2(0.5f,0.5f);
            taken.Position=Position;
            taken.Texture=sprite.Texture;
            taken.sfx=sfx;
            World.level.CallDeferred("add_child",taken);            
            
            Apply();
            CallDeferred("queue_free");
        }
    }
}
