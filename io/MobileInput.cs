using Godot;
using System;

public class MobileInput : InputController
{

    private Touch touch;
    private Buttons buttons;

    public MobileInput(Node scene)
    {
            touch=(Touch)ResourceUtils.touch.Instance();
            touch.ZIndex=2500;
            scene.AddChild(touch);

            buttons=(Buttons)ResourceUtils.buttons.Instance();
            buttons.ZIndex=2500;
            scene.AddChild(buttons);

            buttons.PauseMode=Node.PauseModeEnum.Process;
            scene.PauseMode=Node.PauseModeEnum.Process;
    }

    public override bool getLeft()
    {
        return touch.getValue().x<0f;
    }
    public override bool getRight()
    {
        return touch.getValue().x>0f;
    }
    public override bool getUp()
    {
        return touch.getValue().y<0f;
    }
    public override bool getDown()
    {
        return touch.getValue().y>0f;
    }

    public override bool getJump()
    {
        return buttons.jump.justPressed();
    }
    public override bool getAttack()
    {
        return buttons.o.justPressed();
    }
    public override bool getChange()
    {
        return buttons.x.justPressed();
    }

    public override void _free()
    {
        touch.QueueFree();
        buttons.QueueFree();
    }

    public override bool getPause()
    {
        return false;
    }

    public override bool getQuit()
    {
        return false;
    }

    public override bool getJustLeft()
    {
        throw new NotImplementedException();
    }

    public override bool getJustRight()
    {
        throw new NotImplementedException();
    }

    public override bool getJustUp()
    {
        throw new NotImplementedException();
    }

    public override bool getJustDown()
    {
        throw new NotImplementedException();
    }

    public override bool getJustAccept()
    {
        throw new NotImplementedException();
    }
}
