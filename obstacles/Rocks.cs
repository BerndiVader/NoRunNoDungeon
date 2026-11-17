using Godot;

public class Rocks : PhysicsObject
{
    [Export] private float DAMAGE=1f;
    [Export] private float ROTATION_SPEED=1.5f;

    private float rSpeed=1.5f;
    private Area2D collision;
    private AnimatedSprite sprite;

    public override void _Ready()
    {
        base._Ready();

        sprite=GetNode<AnimatedSprite>(nameof(AnimatedSprite));
        sprite.Play();

        collision=GetNode<Area2D>(nameof(Area2D));
        collision.Connect("body_entered",this,nameof(BodyEntered));
        collision.Connect("body_exited",this,nameof(BodyExited));

        rSpeed=MathUtils.RandBool()?ROTATION_SPEED:ROTATION_SPEED*-1f;
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity+=GRAVITY*delta;
        Rotation+=rSpeed*delta;
        KinematicCollision2D collision=MoveAndCollide(velocity*delta);

        if(collision!=null)
        {
            rSpeed=MathUtils.RandBool()?ROTATION_SPEED:ROTATION_SPEED*-1f;
            velocity=velocity.Bounce(Vector2.Up);

            if(collision.Collider is Platform platform)
            {
                Vector2 rest=collision.Remainder.Slide(collision.Normal);
                MoveAndCollide(rest);
                velocity.x=platform.Velocity.x;                
            }
        }

    }

    private void BodyEntered(Node node)
    {
        if(node.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            collision.SetDeferred("monitoring",false);
            node.EmitSignal(STATE.damage.ToString(),this,DAMAGE,false);
        }
    }
    
    private void BodyExited(Node node)
    {
        if(node.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            collision.SetDeferred("monitoring",true);
        }
    }

}
