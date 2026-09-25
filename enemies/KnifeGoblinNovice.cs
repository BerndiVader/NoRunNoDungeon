using Godot;

public class KnifeGoblinNovice : KnifeGoblin
{

    [Export] private float MAX_THINK_TIME=0.50f;
    private float thinking=0f;

    protected override void Alert(float delta)
    {
        thinking+=delta;
        if(thinking>MAX_THINK_TIME&&!noSnap)
        {
            thinking=0f;
            aura.Emitting=false;
            weapon.Attack();
            OnAttack();
        }

        Navigation(delta);
    }

    protected override void Attack(float delta)
    {
        if(!weapon.IsPlaying())
        {
            OnIdle();
        }
        
        Navigation(delta);
    }

    protected override void Idle(float delta)
    {
        if(playerCast.IsColliding()&&!weapon.IsPlaying())
        {
            OnAlert();
        }

        if(DistanceSquaredToPlayer()<activationDistanceSqrd)
        {
            if(facing.x!=Mathf.Sign(Player.instance.GlobalPosition.x-GlobalPosition.x))
            {
                FlipH();
                OnStroll();
            }
            else if(rayCast2D.IsColliding())
            {
                OnStroll();
            }
        }
        Navigation(delta);
    }

    protected override void Stroll(float delta)
    {
        if(playerCast.IsColliding()&&DistanceSquaredToPlayer()<100f&&!weapon.IsPlaying())
        {
            OnAlert();
        }

        if(!rayCast2D.IsColliding())
        {
            OnIdle();
            Navigation(delta);
            return;
        }

		Vector2 force=new Vector2(FORCE);
		if(facing==Vector2.Left&&velocity.x<=WALK_MIN_SPEED&&velocity.x>-WALK_MAX_SPEED)
		{
			force.x-=WALK_FORCE;
		} 
		else if(facing==Vector2.Right&&velocity.x>=-WALK_MIN_SPEED&&velocity.x<WALK_MAX_SPEED)
		{
			force.x+=WALK_FORCE;
		}
		else
		{
			velocity=StopX(velocity,delta);
		}

		velocity+=force*delta;
		velocity=MoveAndSlideWithSnap(velocity,noSnap?Vector2.Zero:snap,Vector2.Up,false,4,0.785398f,true);
        noSnap=false;
    }

    protected override void OnAlert()
    {
        onDelay=false;
        if(state!=STATE.alert)
        {
            aura.Restart();
            lastState=state;
            state=STATE.alert;
            goal=Alert;
        }
    }

}
