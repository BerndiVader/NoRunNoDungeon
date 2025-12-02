using Godot;
using System;

public class MobileInput : InputController
{

    private Touch touch;
    private Stick stick;
    private Buttons buttons;

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
    }

    public override bool getLeft()
    {
        return stick.getValue().x<0f;
    }
    public override bool getRight()
    {
        return stick.getValue().x>0f;
    }
    public override bool getUp()
    {
        return stick.getValue().y<0f;
    }
    public override bool getDown()
    {
        return stick.getValue().y>0f;
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
        return buttons.c.justPressed();
    }

    public override bool getJustRight()
    {
        return buttons.o.justPressed();
    }

    public override bool getJustUp()
    {
        return buttons.jump.justPressed();
    }

    public override bool getJustDown()
    {
        return buttons.x.justPressed();
    }

    public override bool getJustAccept()
    {
        throw new NotImplementedException();
    }
}
