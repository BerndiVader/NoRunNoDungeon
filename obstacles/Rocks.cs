using Godot;

public class Rocks : PhysicsObject
{
    [Export] private float damage = 1f;
    [Export] private float rotation_speed = 1.5f;
    private float rSpeed = 1.5f;
    private Area2D collision;

    public override void _Ready()
    {
        base._Ready();

        collision = GetNode<Area2D>(nameof(Area2D));
        collision.Connect("body_entered", this, nameof(body_entered));
        collision.Connect("body_exited", this, nameof(body_exited));

        rSpeed = MathUtils.randBool() ? rotation_speed : rotation_speed*-1f;
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity += GRAVITY * delta;
        Rotation += rSpeed * delta;
        KinematicCollision2D collision = MoveAndCollide(velocity * delta);

        if (collision != null)
        {
            rSpeed = MathUtils.randBool() ? rotation_speed : rotation_speed * -1f;
            velocity = velocity.Bounce(Vector2.Up);

            Node node = (Node)collision.Collider;
            if (node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider = (Platform)node;
                velocity.x += collider.CurrentSpeed.x * 0.35f;
            }

        }

    }

    private void body_entered(Node node)
    {
        if (node.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            collision.SetDeferred("monitoring", false);
            node.EmitSignal(STATE.damage.ToString(), damage, this);
        }
    }
    
    private void body_exited(Node node)
    {
        if (node.IsInGroup(GROUPS.PLAYERS.ToString()))
        {
            collision.SetDeferred("monitoring", true);
        }
    }


}
