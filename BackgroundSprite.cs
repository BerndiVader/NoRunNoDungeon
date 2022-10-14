using Godot;
using System;

public class BackgroundSprite : Sprite
{

    public override void _Ready()
    {
        BackgroundLayer layer=GetParent<BackgroundLayer>();

        int id=layer.Name.Substr(layer.Name.Length-1,1).ToInt();
        
        string name=World.instance.tileSet.ResourcePath.BaseName();
        int tileSetId=name.Substr(name.Length-1,1).ToInt();
        tileSetId=tileSetId*12/4-3;

        Texture image=(Texture)ResourceUtils.bgTextures[id+tileSetId].Duplicate();
        this.Texture=image;

        Scale=new Vector2(World.instance.RESOLUTION/image.GetSize());
    }

}
