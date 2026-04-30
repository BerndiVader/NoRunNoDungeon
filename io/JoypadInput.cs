using System.Linq;
using Godot;
using Godot.Collections;
public class JoypadInput : InputController
{
    private int deviceId=-1;

    private enum INPUT
    {
        LEFT,
        RIGHT,
        UP,
        DOWN,
        XBOXY,
        XBOXB,
        XBOXA,
        XBOXX,
        START,
        SELECT
    }

    private readonly int[] inputs =
    {
        (int)JoystickList.DpadLeft,
        (int)JoystickList.DpadRight,
        (int)JoystickList.DpadUp,
        (int)JoystickList.DpadDown,
        (int)JoystickList.XboxY,
        (int)JoystickList.XboxB,
        (int)JoystickList.XboxA,
        (int)JoystickList.XboxX,
        (int)JoystickList.Start,
        (int)JoystickList.Select
    };

    private readonly bool[]current;
    private readonly bool[]previous;
    
    public JoypadInput(int deviceId)
    {
        this.deviceId=deviceId;

        current=new bool[inputs.Length];
        previous=new bool[inputs.Length];
    }

    public override void Poll()
    {
        for(int i=0;i<inputs.Length;i++)
        {
            previous[i]=current[i];
            current[i]=Input.IsJoyButtonPressed(deviceId,inputs[i]);
        }
    }

    public override bool Left()
    {
        return current[(int)INPUT.LEFT];
    }

    public override bool JustLeft()
    {
        return current[(int)INPUT.LEFT]&&!previous[(int)INPUT.LEFT];
    }

    public override bool Right()
    {
        return current[(int)INPUT.RIGHT];
    }

    public override bool JustRight()
    {
        return current[(int)INPUT.RIGHT]&&!previous[(int)INPUT.RIGHT];
    }

    public override bool Up()
    {
        return current[(int)INPUT.UP];
    }

    public override bool JustUp()
    {
        return current[(int)INPUT.UP]&&!previous[(int)INPUT.UP];
    }

    public override bool Down()
    {
        return current[(int)INPUT.DOWN];
    }

    public override bool JustDown()
    {
        return current[(int)INPUT.DOWN]&&!previous[(int)INPUT.DOWN];
    }

    public override bool Jump()
    {
        return current[(int)INPUT.XBOXY];
    }

    public override bool JustJump()
    {
        return current[(int)INPUT.XBOXY]&&!previous[(int)INPUT.XBOXY];
    }

    public override bool Attack()
    {
        return current[(int)INPUT.XBOXX];
    }
    public override bool JustAttack()
    {
        return current[(int)INPUT.XBOXX]&&!previous[(int)INPUT.XBOXX];
    }

    public override bool Change()
    {
        return current[(int)INPUT.XBOXA];
    }
    public override bool JustChange()
    {
        return current[(int)INPUT.XBOXA]&&!previous[(int)INPUT.XBOXA];
    }

    public override bool JustAccept()
    {
        return current[(int)INPUT.XBOXB]&&!previous[(int)INPUT.XBOXB];
    }

    public override bool Pause()
    {
        return current[(int)INPUT.START]&&!previous[(int)INPUT.START];
    }

    public override bool Quit()
    {
        return current[(int)INPUT.SELECT]&&!previous[(int)INPUT.SELECT];
    }

    public override void Rumble(float value)
    {
        Input.StartJoyVibration(deviceId,1f,value,0.25f);
        return;
    }

    public override void Free()
    {
        return;
    }

}
