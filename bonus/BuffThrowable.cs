using Godot;
using System;

public class BuffThrowable : PhysicsObject
{
    protected static readonly AudioStream collideSfx=ResourceLoader.Load<AudioStream>("res://sounds/ingame/12_Player_Movement_SFX/42_Cling_climb_03.wav");
    protected static readonly PackedScene dustPack=ResourceLoader.Load<PackedScene>("res://gfx/Dust.tscn");

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
                velocity.x+=collider.CurrentSpeed.x*0.35f;
            }
        }
    }


    protected void PlayFx()
    {
        SfxPlayer sfx=new SfxPlayer();
        Dust dust=dustPack.Instance<Dust>();

        sfx.Stream=collideSfx;
        sfx.Position=Position;

        dust.Position=Position;
        dust.type=Dust.TYPE.FALL;

        World.level.AddChild(dust);
        World.level.AddChild(sfx);
    }

}
