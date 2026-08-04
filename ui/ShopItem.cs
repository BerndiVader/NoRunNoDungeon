using Godot;
using System;

public class ShopItem : Button
{

    public override void _Ready()
    {
        
    }

    public override void _Notification(int what)
    {
        switch(what)
        {
            case Button.NotificationFocusEnter:
                RectScale=new Vector2(1.1f,1.1f);
                RectPivotOffset=RectSize/2f;
                break;
            case Button.NotificationFocusExit:
                RectScale=new Vector2(1f,1f);
                RectPivotOffset=RectSize/2f;
                break;
            case Button.NotificationResized:
                RectPivotOffset=RectSize/2f;
                break;
        }
    }


}
