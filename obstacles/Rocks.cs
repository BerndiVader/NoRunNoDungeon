using Godot;
using System;

public class Rocks : PhysicsObject
{
    private float rSpeed = 1.5f;
    private float rot = 0f;
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity += GRAVITY * delta;

        rot += rSpeed * delta;
        Rotation = rot;

        KinematicCollision2D collision=MoveAndCollide(velocity*delta);

        if (collision != null)
        {
            rSpeed = GD.Randf() > 0.5f ? 1.5f : -1.5f;
            velocity = velocity.Bounce(Vector2.Up);

            Node node = (Node)collision.Collider;
            if (node.IsInGroup(GROUPS.PLATFORMS.ToString()))
            {
                Platform collider = (Platform)node;
                velocity.x += collider.CurrentSpeed.x * 0.35f;
            }

        }

    }


}
