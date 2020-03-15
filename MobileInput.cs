using Godot;
using System;

public class MobileInput : InputController
{

    Touch touch;
    Buttons buttons;

    public MobileInput(Node scene)
    {
            touch=(Touch)ResourceUtils.touch.Instance();
            touch.ZIndex=2000;
            scene.AddChild(touch);

            buttons=(Buttons)ResourceUtils.buttons.Instance();
            buttons.ZIndex=2000;
            scene.AddChild(buttons);
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

    public override void _free()
    {
        touch.QueueFree();
        buttons.QueueFree();
    }
}
