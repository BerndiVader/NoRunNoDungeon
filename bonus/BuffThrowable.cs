using Godot;
using System;

public class BuffThrowable : PhysicsObject
{
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

}
