using Godot;
using System;

public class Player : KinematicBody2D
{
    static String ANIM_RUN="RUN";
    static String ANIM_JUMP="HIT";
    static World world=WorldUtils.world;

    [Export] public float GRAVITY=500f;
    [Export] public float FLOOR_ANGLE_TOLERANCE=40f;
    [Export] public float WALK_FORCE=600f;
    [Export] public float WALK_MIN_SPEED=10f;
    [Export] public float WALK_MAX_SPEED=200f;
    [Export] public float STOP_FORCE=1300f;
    [Export] public float JUMP_SPEED=200f;
    [Export] public float JUMP_MAX_AIRBORNE_TIME=0.2f;

    [Export] public float SLIDE_STOP_VELOCITY=1f;
    [Export] public float SLIDE_STOP_MIN_TRAVEL=1f;

    Vector2 velocity=new Vector2(0f,0f);
    float onAirTime=100f;
    bool jumping=false;
    bool doubleJump=false;
    bool justJumped=false;
    bool prevJumpPressed=false;

    float slopeAngle=0f;
    Vector2 lastVelocity=new Vector2(0f,0f);

    public float lastFallingSpeed=0f;

    AnimatedSprite animationController;
    CollisionShape2D collisionController;

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


    public override void _PhysicsProcess(float delta) {
        if(world.level==null) return;
        Vector2 force=new Vector2(0,GRAVITY);
        bool left=Input.IsKeyPressed((int)KeyList.A);
        bool right=Input.IsKeyPressed((int)KeyList.D);
        bool jump=Input.IsKeyPressed((int)KeyList.Up);

        bool stop=true;

        if(left){
            if(velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED) {
                force.x-=WALK_FORCE;
                stop=false;
            }
        } else if(right){
            if(velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED) {
                force.x+=WALK_FORCE;
                stop=false;
            }
        }

        if(stop){
            float vSign=Mathf.Sign(velocity.x);
            float vLen=Mathf.Abs(velocity.x);

            vLen-=STOP_FORCE*delta;
            if(vLen<0f){
                vLen=0f;
            }
            velocity.x=vLen*vSign;
        }

        if(velocity.y!=0) lastFallingSpeed=velocity.y;

        velocity+=GetFloorVelocity()*delta;
        velocity+=force*delta;

        if(justJumped){
            MoveLocalX(velocity.x*delta);
            MoveLocalY(velocity.y*delta);
            justJumped=false;
        } else {
            Vector2 snap=jumping?new Vector2(0f,0f):new Vector2(0,10);
            velocity=MoveAndSlideWithSnap(velocity,snap,Vector2.Up,false,4,0.785398f,true);
        }

        if(GetSlideCount()>0) {
            for(int i=0;i<GetSlideCount();i++){
                KinematicCollision2D collision=GetSlideCollision(i);
            }

        }

        if(IsOnCeiling()) {
            if(lastVelocity.y<-150f) {
                world.renderer.shake+=Mathf.Abs(lastVelocity.y*0.004f);
            }        
        }

        if(IsOnFloor()){
            onAirTime=0f;
            if(lastVelocity.y>300f) {
                world.renderer.shake+=lastVelocity.y*0.004f;
            }        
        }
        
        if(jumping&&velocity.y>0f) {
            animationController.Play(ANIM_RUN);
            doubleJump=jumping=false;
        }

        if(jump&&jumping&&!prevJumpPressed&&!doubleJump) {
            doubleJump=true;
            velocity.y=-JUMP_SPEED;
            animationController.Play(ANIM_JUMP);
        }

        if(onAirTime<JUMP_MAX_AIRBORNE_TIME&&jump&&!prevJumpPressed&&!jumping) {
            if(isOnSlope()) {
                if(slopeAngle<0) {
                    velocity.x+=100;
                } else if(slopeAngle<1f) {
                    velocity.x-=100;
                }
            }
            velocity.y=-JUMP_SPEED;
            animationController.Play(ANIM_JUMP);
            justJumped=jumping=true;
        }

        onAirTime+=delta;
        prevJumpPressed=jump;

        if(Position.y>320f||Position.x<-20f) {
            reset();
            world.restartGame();
        }

        lastVelocity=velocity;
    }

    public void reset() {
        Vector2 velocity=new Vector2(0f,0f);
        onAirTime=100f;
        jumping=false;
        doubleJump=false;
        justJumped=false;
        prevJumpPressed=false;
        slopeAngle=0f;
        lastVelocity=new Vector2(0f,0f);        
    }

    public bool isOnSlope() {
        int count=GetSlideCount();
        ulong id=world.level.GetInstanceId();
        for(int i=0;i<count;i++){
            KinematicCollision2D collision=GetSlideCollision(i);
            if(collision.Collider==null||collision.ColliderId!=id) continue;
            slopeAngle=collision.Normal.AngleTo(Vector2.Up);
            return Mathf.Abs(slopeAngle)>=0.785298f;
        }
        return false;
    }

}
