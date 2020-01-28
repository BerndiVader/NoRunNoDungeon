using Godot;
using System;

public class Player : KinematicBody2D
{
    static String ANIM_RUN="RUN";
    static String ANIM_JUMP="HIT";

    public World world;

    [Export] public float Speed=100f;
    [Export] public Vector2 gravity=new Vector2(10f,200f);
    public float jumpForce=0f;
    [Export] public Vector2 jump=new Vector2(-20f,-200);

    private AnimatedSprite animationController;
    private Camera2D camera;
    private Vector2 velocity=new Vector2(0,0);

    public override void _Ready()
    {
        animationController=(AnimatedSprite)this.GetNode("AnimatedSprite");
        animationController.Play(ANIM_RUN);
        camera=(Camera2D)GetNode("Camera2D");
    }

    public override void _PhysicsProcess(float delta) {

        Vector2 movement=new Vector2(0,0);

        if(Input.IsKeyPressed((int)KeyList.A)) movement.x-=1;
        if(Input.IsKeyPressed((int)KeyList.D)) movement.x+=1;

        movement=movement.Normalized()*Speed;
        velocity.x=movement.x;
        velocity.y+=gravity.x;
        velocity.y=Mathf.Min(velocity.y,gravity.y);

        Vector2 result=MoveAndSlide(velocity);
        if(result.y==0) velocity.y=0;

        if((Mathf.Abs(velocity.y)==0&&IsOnWall())) {
            if(animationController.GetAnimation()==ANIM_JUMP) animationController.Play(ANIM_RUN);
            if(Input.IsKeyPressed((int)KeyList.Up)) {
                animationController.Play(ANIM_JUMP);
                velocity.y=jump.y;
            }  
        }

    	Vector2 position=Position;
        position.x=Mathf.Clamp(Position.x,camera.LimitLeft,camera.LimitRight);
        SetPosition(position);

        if(Position.y>camera.LimitBottom+50) world.restart();

    }

}
