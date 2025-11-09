using Godot;
using System;

public class Rocks : PhysicsObject
{
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity+=GRAVITY*delta;

        KinematicCollision2D collision=MoveAndCollide(velocity*delta);

        if (collision != null)
        {
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
