using Godot;
using System;

public class MobileInput : InputController
{

    Touch touch;
    Buttons buttons;

    public MobileInput()
    {
            touch=(Touch)ResourceUtils.touch.Instance();
            touch.ZIndex=2000;
            WorldUtils.world.AddChild(touch);

            buttons=(Buttons)ResourceUtils.buttons.Instance();
            buttons.ZIndex=2000;
            WorldUtils.world.AddChild(buttons);
    }

    public override bool getLeft()
    {
        return touch.getValue().x<0f;
    }
    public override bool getRight()
    {
        return touch.getValue().x>0f;
    }
    public override bool getJump()
    {
        return buttons.jump.justPressed();
    }
}
