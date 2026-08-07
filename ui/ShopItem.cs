using Godot;
using System;

public class ShopItem : Button
{

    public override void _Notification(int what)
    {
        switch(what)
        {
            case NotificationFocusEnter:
                RectScale=new Vector2(1.2f,1.2f);
                RectPivotOffset=RectSize/2f;
                break;
            case NotificationFocusExit:
                RectScale=new Vector2(1f,1f);
                RectPivotOffset=RectSize/2f;
                break;
            case NotificationResized:
                RectPivotOffset=RectSize/2f;
                break;
        }
    }


}
