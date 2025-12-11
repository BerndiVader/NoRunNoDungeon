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
        return stick.GetValue().y<0f;
    }
    public override bool Down()
    {
        return stick.GetValue().y>0f;
    }

    public override bool Jump()
    {
        return buttons.jump.JustPressed();
    }
    public override bool Attack()
    {
        return buttons.o.JustPressed();
    }
    public override bool Change()
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
        return buttons.c.JustPressed();
    }

    public override bool JustRight()
    {
        return buttons.o.JustPressed();
    }

    public override bool JustUp()
    {
        return buttons.jump.JustPressed();
    }

    public override bool JustDown()
    {
        return buttons.x.JustPressed();
    }

    public override bool JustAccept()
    {
        throw new NotImplementedException();
    }
}
