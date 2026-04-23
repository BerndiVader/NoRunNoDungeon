using Godot;
using Godot.Collections;
public class JoypadInput : InputController
{
    private int deviceId=-1;
    private bool justY,justA,justX,justB,justQ,justP;
    private bool jUp,jDown,jLeft,jRight;

    public JoypadInput(int deviceId)
    {
        this.deviceId=deviceId;
        justY=justA=justX=justB=false;
        jUp=jDown=jLeft=jRight=false;
    }
    
   public override bool Left()
    {
        return Input.IsJoyButtonPressed(deviceId,(int)JoystickList.DpadLeft);
    }
    public override bool Right()
    {
        return Input.IsJoyButtonPressed(deviceId,(int)JoystickList.DpadRight);
    }
    public override bool Up()
    {
        return Input.IsJoyButtonPressed(deviceId,(int)JoystickList.DpadUp);
    }
    public override bool Down()
    {
        return Input.IsJoyButtonPressed(deviceId,(int)JoystickList.DpadDown);
    }
    public override bool Jump()
    {
        return Input.IsJoyButtonPressed(deviceId,(int)JoystickList.XboxY);
    }
    public override bool JustJump()
    {
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.XboxY))
        {
            if(!justY)
            {
                return justY=true;
            }
            return false;
        }
        return justY=false;
    }

    public override bool Attack()
    {
        return Input.IsJoyButtonPressed(deviceId,(int)JoystickList.XboxB);
    }
    public override bool JustAttack()
    {
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.XboxB))
        {
            if(!justB)
            {
                return justB=true;
            }
            return false;
        }
        return justB=false;
    }
    public override bool Change()
    {
        return Input.IsJoyButtonPressed(deviceId,(int)JoystickList.XboxA);
    }
    public override bool JustChange()
    {
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.XboxA))
        {
            if(!justA)
            {
                return justA=true;
            }
            return false;
        }
        return justA=false;
    }

    public override bool JustAccept()
    {
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.XboxX))
        {
            if(!justX)
            {
                return justX=true;
            }
            return false;
        }
        return justX=false;
    }

    public override bool Pause()
    {
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.Start))
        {
            if(!justP)
            {
                return justP=true;
            }
            return false;
        }
        return justP=false;     
    }
    public override bool Quit()
    {
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.Select))
        {
            if(!justQ)
            {
                return justQ=true;
            }
            return false;
        }
        return justQ=false;        
    }

    public override bool JustLeft()
    {
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.DpadLeft))
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
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.DpadRight))
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
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.DpadUp))
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
        if(Input.IsJoyButtonPressed(deviceId,(int)JoystickList.DpadDown))
        {
            if(!jDown)
            {
                return jDown=true;
            }
            return false;
        }
        return jDown=false;
    }

    public override void Free()
    {
        return;
    }

}
