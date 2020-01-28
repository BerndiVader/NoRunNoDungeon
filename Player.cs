using Godot;
using System;

public class Player : KinematicBody2D
{
    static String ANIM_RUN="RUN";
    static String ANIM_JUMP="HIT";
    static World world=WorldUtils.getWorld();

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
        this.AddToGroup("Players");
    }

    public override void _PhysicsProcess(float delta) {

        Vector2 movement=new Vector2(0,0);

        if(Input.IsKeyPressed((int)KeyList.A)) {
            animationController.FlipH=true;
            movement.x-=1;
        } else if (Input.IsKeyPressed((int)KeyList.D)) {
            animationController.FlipH=false;
            movement.x+=1;
        } else {
            animationController.FlipH=false;
        }

        movement=movement.Normalized()*Speed;
        velocity.x=movement.x;
        velocity.y+=gravity.x;
        velocity.y=Mathf.Min(velocity.y,gravity.y);

        Vector2 result=MoveAndSlide(velocity,Vector2.Up);
        if(result.y==0) velocity.y=0;

        if(IsOnFloor()) {
            if(Input.IsActionJustPressed("ui_up")) {
                animationController.Play(ANIM_JUMP);
                velocity.y=jump.y;
            } else if(animationController.GetAnimation()!=ANIM_RUN) {
                animationController.Play(ANIM_RUN);
            }
        }

    	Vector2 position=Position;
        position.x=Mathf.Clamp(Position.x,camera.LimitLeft,camera.LimitRight);
        SetPosition(position);

        if(Position.y>camera.LimitBottom+50) world.restart();

    }

}
