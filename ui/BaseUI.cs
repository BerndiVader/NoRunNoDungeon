using Godot;
using System;

public abstract class BaseUI : Panel
{
    protected static PackedScene OptionsPack=ResourceLoader.Load<PackedScene>("res://ui/Options.tscn");
    protected static PackedScene PausePack=ResourceLoader.Load<PackedScene>("res://ui/PauseUI.tscn");
    protected int selected;
    protected Sprite sprite;
    protected Color color, colorSelected;
    [Export] protected float SCALE_SIZE=1f;

    public override void _Ready()
    {
        ImageTexture tex=new ImageTexture();
        Image image=new Image();
        image.Create(680,440,false,Image.Format.Rgba8);
        image.Fill(new Color(0.5f,0.5f,0.5f,0.5f));
        tex.CreateFromImage(image);
        sprite = new Sprite
        {
            Texture = tex,
            Position = Vector2.Zero
        };
        CanvasItemMaterial material = new CanvasItemMaterial
        {
            BlendMode = CanvasItemMaterial.BlendModeEnum.Mul
        };
        sprite.Material=material;
        sprite.ZIndex=VisualServer.CanvasItemZMax-5;
        sprite.Scale=new Vector2(3f,3f);
        GetTree().CurrentScene.AddChild(sprite);
        color=new Color(1f,1f,1f,1f);
        colorSelected=new Color(1f,0f,0f,0.3f);

        RectScale=PlayerCamera.instance.Zoom*SCALE_SIZE;
        RectPosition*=RectScale;
        RectPosition+=PlayerCamera.instance.GetCameraScreenCenter();              
    
    }

}
