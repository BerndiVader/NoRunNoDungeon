using Godot;
using System;

public class PlayerOld : KinematicBody2D
{
    static String ANIM_RUN="RUN";
    static String ANIM_JUMP="HIT";
    static World world=WorldUtils.world;

    [Export] public float Speed=100f;
    [Export] public Vector2 gravity=new Vector2(10f,200f);
    [Export] public Vector2 jump=new Vector2(-20f,-200f);

    AnimatedSprite animationController;
    CollisionShape2D collisionController;
    public Vector2 velocity=new Vector2(0f,0f);
    public Vector2 lastVelocity=new Vector2(0f,0f);
    bool isJumping=false;
    bool doubleJump=false;
    bool justJumped=false;
    long jumpStamp=0;

    public override void _Ready()
    {
        animationController=(AnimatedSprite)this.GetNode("AnimatedSprite");
        collisionController=(CollisionShape2D)this.GetNode("CollisionShape2D");
        animationController.Play(ANIM_RUN);
        this.AddToGroup("Players");

    }

    public override void _EnterTree() {
        Position=world.level.startingPoint.Position;
    }

    public override void _Process(float delta) {

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

        if(isJumping&&!IsOnWall()) {
            if(Input.IsKeyPressed((int)KeyList.S)) {
                gravityFactor=2f;
            } else if(velocity.y>0f&&Input.IsKeyPressed((int)KeyList.W)) {
                gravityFactor=0.4f;
            }
        }

        movement=movement.Normalized()*Speed;
        velocity.x=movement.x;
        velocity.y+=gravity.x*gravityFactor;
        velocity.y=Mathf.Min(velocity.y,gravity.y);

        bool onSlope=isOnSlope();

        if(justJumped){
            justJumped=false;
            MoveLocalX(velocity.x*delta,false);
            MoveLocalY(velocity.y*delta,false);
        } else {
            velocity+=MoveAndSlide(velocity,Vector2.Up,false,4);
        }

        if(Input.IsActionJustPressed("ui_up")&&isJumping&&!doubleJump) {
            doubleJump=true;
            velocity.y=jump.y;
            jumpStamp=DateTime.Now.Ticks;
        }

        if((IsOnFloor())) {
            if(DateTime.Now.Ticks-jumpStamp>80000) {
                doubleJump=false;
                isJumping=false;
                justJumped=false;
            }
            if(lastVelocity.y>300f) {
                world.renderer.shake+=lastVelocity.y*0.004f;
            } 
            if(Input.IsActionJustPressed("ui_up")) {
                justJumped=isJumping=true;
                animationController.Play(ANIM_JUMP);
                velocity.y=jump.y;
                jumpStamp=DateTime.Now.Ticks;
            } else if(animationController.Animation!=ANIM_RUN) {
                animationController.Play(ANIM_RUN);
            }
        }        

        if(Input.IsActionJustPressed("key_w")) {
            if(DateTime.Now.Ticks-jumpStamp<700000) {
                velocity.y+=jump.y*0.5f;
            }
        }


        if(Position.y>320f||Position.x<-20f) {
            reset();
            world.restartGame();
        }

        GD.Print(velocity.y);

        lastVelocity=velocity;
    }

    public void reset() {
        velocity=new Vector2(0f,0f);
        lastVelocity=new Vector2(0f,0f);
        isJumping=false;
        justJumped=false;
        doubleJump=false;
    }

    public bool isOnSlope() {
        int count=GetSlideCount();
        for(int i=0;i<count;i++){
            KinematicCollision2D collision=GetSlideCollision(i);
            if(collision.Collider==null||collision.ColliderId!=world.level.GetInstanceId()) continue;
            float angle=collision.Normal.AngleTo(Vector2.Up);
            return Mathf.Abs(angle)>=0.785298f;
        }
        return false;
    }

}
