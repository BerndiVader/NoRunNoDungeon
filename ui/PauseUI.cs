using Godot;
using System;

public class PauseUI : Panel
{
    private Button cont, quit, main, selectedButton;
    private int selected;
    private InputController input;
    private Sprite sprite;
    private Color color, colorSelected;

    public override void _Ready()
    {
        ImageTexture tex=new ImageTexture();
        Image image=new Image();
        image.Create(680,440,false,Image.Format.Rgba8);
        image.Fill(new Color(0.5f,0.5f,0.5f,0.5f));
        tex.CreateFromImage(image);
        sprite=new Sprite();
        sprite.Texture=tex;
        sprite.Position=Vector2.Zero;
        CanvasItemMaterial material=new CanvasItemMaterial();
        material.BlendMode=CanvasItemMaterial.BlendModeEnum.Mul;
        sprite.Material=material;
        sprite.ZIndex=VisualServer.CanvasItemZMax-5;
        sprite.Scale=new Vector2(3f,3f);

        GetTree().CurrentScene.AddChild(sprite);

        color=new Color(1f,1f,1f,1f);
        colorSelected=new Color(1f,0f,0f,0.3f);

        cont=GetNode<Button>("Continue");
        quit=GetNode<Button>("Quit");
        main=GetNode<Button>("Main");

        selected=0;
        changeState();

        RectScale=PlayerCamera.instance.Zoom;
        RectPosition*=RectScale;
        RectPosition+=PlayerCamera.instance.GetCameraScreenCenter();    

        input=ResourceUtils.getInputController(this);
    }

    public override void _Process(float delta)
    {
        if(input.getJustDown())
        {
            selected++;
            if(selected>2)
            {
                selected=0;
            }
            changeState();
        } else if(input.getJustUp())
        {
            selected--;
            if(selected<0)
            {
                selected=2;
            }
            changeState();
        } else if(input.getJustAccept())
        {
            cont.SelfModulate=color;
            main.SelfModulate=color;
            quit.SelfModulate=color;
            selectedButton.SelfModulate=colorSelected;
            switch(selected)
            {
                case 0:
                    sprite.QueueFree();
                    World.instance.resetGamestate();
                    GetTree().Paused=false;
                    GetParent().QueueFree();
                    break;
                case 1:
                    sprite.QueueFree();
                    GetTree().Paused=false;
                    World.changeScene(ResourceUtils.intro);
                    GetParent().QueueFree();
                    break;
                case 2:
                    World.quit();
                    break;
            }
        }
    }

    private void changeState()
    {
        switch(selected)
        {
            case 0:
                cont.Pressed=true;
                main.Pressed=false;
                quit.Pressed=false;
                selectedButton=cont;
                break;
            case 1:
                cont.Pressed=false;
                main.Pressed=true;
                quit.Pressed=false;
                selectedButton=main;
                break;
            case 2:
                cont.Pressed=false;
                main.Pressed=false;
                quit.Pressed=true;
                selectedButton=quit;
                break;
        }        
    }

}
