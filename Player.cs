using Godot;
using System;

public class Player : KinematicBody2D
{
    static String ANIM_RUN="RUN";
    static String ANIM_JUMP="HIT";
    static World world=WorldUtils.getWorld();

    [Export] public float Speed=100f;
    [Export] public Vector2 gravity=new Vector2(10f,200f);
    [Export] public Vector2 jump=new Vector2(-20f,-200f);

    AnimatedSprite animationController;
    CollisionShape2D collisionController;
    Camera2D camera;
    Vector2 velocity=new Vector2(0f,0f);
    Vector2 lastVelocity=new Vector2(0f,0f);
    bool isJumping=false;
    long jumpStamp=0;

    public override void _Ready()
    {
        animationController=(AnimatedSprite)this.GetNode("AnimatedSprite");
        collisionController=(CollisionShape2D)this.GetNode("CollisionShape2D");
        animationController.Play(ANIM_RUN);
        camera=(Camera2D)GetNode("Camera2D");
        this.AddToGroup("Players");
    }

    public override void _PhysicsProcess(float delta) {

        Vector2 movement=new Vector2(0f,0f);
        float gravityFactor=1f;

        if(Input.IsKeyPressed((int)KeyList.A)) {
            if(!animationController.FlipH) {
                animationController.FlipH=true;
            }
            movement.x-=1;
        } else if (Input.IsKeyPressed((int)KeyList.D)) {
            if(animationController.FlipH) {
                animationController.FlipH=false;
            }
            movement.x+=1;
        } else if(animationController.FlipH) {
            animationController.FlipH=false;
        }

        if(!IsOnFloor()) {
            if(Input.IsKeyPressed((int)KeyList.S)) {
                gravityFactor=2f;
            } else if(isJumping&&velocity.y>0f&&Input.IsKeyPressed((int)KeyList.W)) {
                gravityFactor=0.2f;
            }
        }

        movement=movement.Normalized()*Speed;
        velocity.x=movement.x;
        velocity.y+=gravity.x*gravityFactor;
        velocity.y=Mathf.Min(velocity.y,gravity.y);

        velocity=MoveAndSlide(velocity,Vector2.Up);

        if(IsOnFloor()) {
            isJumping=false;
            if(lastVelocity.y>300f) {
                camera.shake+=lastVelocity.y*0.007f;
            } 
            if(Input.IsActionJustPressed("ui_up")) {
                isJumping=true;
                animationController.Play(ANIM_JUMP);
                velocity.y=jump.y;
                jumpStamp=DateTime.Now.Ticks;
            } else if(animationController.Animation!=ANIM_RUN) {
                animationController.Play(ANIM_RUN);
            }
        }

        if(isJumping==true&&Input.IsActionJustPressed("key_w")) {
            if(DateTime.Now.Ticks-jumpStamp<600000) {
                velocity.y+=jump.y;
            }
        }


        if(Position.y>camera.LimitBottom+50||Position.x<camera.Position.x-20f) {
            world.restartGame();
            reset();
        }

        lastVelocity=velocity;
    }

    public void reset() {
        velocity=new Vector2(0f,0f);
        lastVelocity=new Vector2(0f,0f);
        isJumping=false;        
    }

}
