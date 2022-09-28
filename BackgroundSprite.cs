using Godot;
using System;

public class BackgroundSprite : Sprite
{
    BackgroundLayer layer;

    public override void _Ready()
    {
        layer=GetParent() as BackgroundLayer;

        int id=layer.Name.Substr(layer.Name.Length-1,1).ToInt();
        
        string name=WorldUtils.world.tileSet.ResourcePath.BaseName();
        int tileSetId=name.Substr(name.Length-1,1).ToInt();
        tileSetId=tileSetId*12/4-3;

        Texture image=(Texture)ResourceUtils.bgTextures[id+tileSetId].Duplicate();
        this.Texture=image;

        Scale=new Vector2(WorldUtils.world.RESOLUTION/image.GetSize());
    }

}
