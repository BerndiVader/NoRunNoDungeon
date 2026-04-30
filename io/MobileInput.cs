using Godot;
using System;
using System.Runtime.Serialization;

public class MobileInput : InputController
{

    private readonly Touch touch;
    private readonly Stick stick;
    private readonly Buttons buttons;

    private enum DIRECTIONS
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    private readonly bool[]states=new bool[4];

    public MobileInput(Node scene)
    {
        touch=ResourceUtils.touch.Instance<Touch>();
        touch.ZIndex=2500;
        scene.AddChild(touch);
        stick=touch.GetNode<Stick>(nameof(Stick));

        buttons=ResourceUtils.buttons.Instance<Buttons>();
        buttons.ZIndex=2500;
        scene.AddChild(buttons);

        buttons.PauseMode=Node.PauseModeEnum.Process;
        scene.PauseMode=Node.PauseModeEnum.Process;

        for(int i=0;i<states.Length;i++)
        {
            states[i]=false;
        }
    }

    public override void Poll()
    {
        return;
    }

    public override bool Left()
    {
        return stick.GetValue().x<0f;
    }
    public override bool Right()
    {
        return stick.GetValue().x>0f;
    }
    public override bool Up()
    {
        return stick.GetValue().y<-0.96f;
    }
    public override bool Down()
    {
        return stick.GetValue().y>0.96f;
    }

    public override bool Jump()
    {
        return buttons.jump.IsPressed();
    }
    public override bool JustJump()
    {
        return buttons.jump.JustPressed();
    }
    public override bool Attack()
    {
        return buttons.attack.IsPressed();
    }
    public override bool JustAttack()
    {
        return buttons.attack.JustPressed();
    }
    public override bool Change()
    {
        return buttons.change.IsPressed();
    }
    public override bool JustChange()
    {
        return buttons.change.JustPressed();
    }
        public override bool JustAccept()
    {
        return buttons.accept.JustPressed();
    }

    public override void Free()
    {
        touch.QueueFree();
        buttons.QueueFree();
    }

    public override bool Pause()
    {
        return false;
    }

    public override bool Quit()
    {
        return false;
    }

    public override bool JustLeft()
    {
        if(stick.GetValue().x<-0.96f)
        {
            if(!states[(int)DIRECTIONS.LEFT])
            {
                return states[(int)DIRECTIONS.LEFT]=true;
            }
            return false;
        }
        return states[(int)DIRECTIONS.LEFT]=false;
    }

    public override bool JustRight()
    {
        if(stick.GetValue().x>0.96f)
        {
            if(!states[(int)DIRECTIONS.RIGHT])
            {
                return states[(int)DIRECTIONS.RIGHT]=true;
            }
            return false;
        }
        return states[(int)DIRECTIONS.RIGHT]=false;
    }

    public override bool JustUp()
    {
        if(stick.GetValue().y<-0.96f)
        {
            if(!states[(int)DIRECTIONS.UP])
            {
                return states[(int)DIRECTIONS.UP]=true;
            }
            return false;
        }
        return states[(int)DIRECTIONS.UP]=false;
    }

    public override bool JustDown()
    {
        if(stick.GetValue().y>0.96f)
        {
            if(!states[(int)DIRECTIONS.DOWN])
            {
                return states[(int)DIRECTIONS.DOWN]=true;
            }
            return false;
        }
        return states[(int)DIRECTIONS.DOWN]=false;        
    }

    public override void Rumble(float value)
    {
        Input.VibrateHandheld(250);
    }

}
