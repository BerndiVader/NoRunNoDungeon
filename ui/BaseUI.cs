using Godot;
using System;
using System.IO;

public abstract class BaseUI : Panel
{
    protected static PackedScene OptionsPack=ResourceLoader.Load<PackedScene>("res://ui/Options.tscn");
    protected static PackedScene PausePack=ResourceLoader.Load<PackedScene>("res://ui/PauseUI.tscn");
    protected static PackedScene StartPack=ResourceLoader.Load<PackedScene>("res://ui/StartUI.tscn");
    protected static AudioStream sfxHover=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/001_Hover_01.wav");
    protected static AudioStream sfxClick=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/013_Confirm_03.wav");
    protected static AudioStream sfxButtons=ResourceLoader.Load<AudioStream>("res://sounds/ingame/10_UI_Menu_SFX/029_Decline_09.wav");
    protected int selected;
    protected Sprite sprite;
    protected Color color, colorSelected;
    [Export] protected float SCALE_SIZE=1f;

    public override void _Ready()
    {
        ImageTexture tex=new ImageTexture();
        Image image=new Image();
        image.Create((int)World.RESOLUTION.x,(int)World.RESOLUTION.y,false,Image.Format.Rgba8);
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

        if (PlayerCamera.instance != null)
        {
            RectScale = PlayerCamera.instance.Zoom * SCALE_SIZE;
            RectPosition *= RectScale;
            RectPosition += PlayerCamera.instance.GetCameraScreenCenter();    
        }
    }

    protected void playSfx(AudioStream effect)
    {
        SfxPlayer sfx=new SfxPlayer();
        sfx.Stream=effect;
        sfx.Position=RectPosition;
        AddChild(sfx);
        sfx.Play();
    }

}
