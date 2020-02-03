using Godot;
using System;

public class BackgroundSprite : Sprite
{
    BackgroundLayer layer;
    int id;
    Texture image;

    public override void _Ready()
    {
        layer=(BackgroundLayer)GetParent();
        id=layer.Name.Substr(layer.Name.Length-1,1).ToInt();
        image=(Texture)ResourceUtils.bgTextures[id].Duplicate();
        this.Texture=image;
        Scale=new Vector2(1.6f,1.6f);
        ZIndex=layer.ZIndex;
    }


}
