using Godot;
using System;

public class MobileInput : InputController
{

    private Touch touch;
    private Stick stick;
    private Buttons buttons;
    private bool jUp,jDown,jLeft,jRight;

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

        jUp=jDown=jLeft=jRight=false;
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
        return buttons.o.IsPressed();
    }
    public override bool JustAttack()
    {
        return buttons.o.JustPressed();
    }
    public override bool Change()
    {
        return buttons.x.IsPressed();
    }
    public override bool JustChange()
    {
        return buttons.x.JustPressed();
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
            if(!jLeft)
            {
                return jLeft=true;
            }
            return false;
        }
        return jLeft=false;
    }

    public override bool JustRight()
    {
        if(stick.GetValue().x>0.96f)
        {
            if(!jRight)
            {
                return jRight=true;
            }
            return false;
        }
        return jRight=false;
    }

    public override bool JustUp()
    {
        if(stick.GetValue().y<-0.96f)
        {
            if(!jUp)
            {
                return jUp=true;
            }
            return false;
        }
        return jUp=false;
    }

    public override bool JustDown()
    {
        if(stick.GetValue().y>0.96f)
        {
            if(!jDown)
            {
                return jDown=true;
            }
            return false;
        }
        return jDown=false;        
    }

    public override bool JustAccept()
    {
        throw new NotImplementedException();
    }
}
