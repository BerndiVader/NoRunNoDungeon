using Godot;

public class PauseUI : BaseUI
{
    private Button cont, quit, main, options;

    public override void _Ready()
    {
        cont=GetNode<Button>("Continue");
        quit=GetNode<Button>("Quit");
        main=GetNode<Button>("Main");
        options=GetNode<Button>("Options");

        cont.Connect("mouse_entered",this,nameof(playSfx),new Godot.Collections.Array(sfxHover));
        quit.Connect("mouse_entered",this,nameof(playSfx),new Godot.Collections.Array(sfxHover));
        main.Connect("mouse_entered",this,nameof(playSfx),new Godot.Collections.Array(sfxHover));
        options.Connect("mouse_entered",this,nameof(playSfx),new Godot.Collections.Array(sfxHover));

        cont.Connect("button_down",this,nameof(playSfx),new Godot.Collections.Array(sfxButtons));
        quit.Connect("button_down",this,nameof(playSfx),new Godot.Collections.Array(sfxButtons));
        main.Connect("button_down",this,nameof(playSfx),new Godot.Collections.Array(sfxButtons));
        options.Connect("button_down",this,nameof(playSfx),new Godot.Collections.Array(sfxButtons));

        cont.Connect("button_up",this,nameof(onMouseSelected),new Godot.Collections.Array(0));
        quit.Connect("button_up",this,nameof(onMouseSelected),new Godot.Collections.Array(3));
        main.Connect("button_up",this,nameof(onMouseSelected),new Godot.Collections.Array(1));
        options.Connect("button_up",this,nameof(onMouseSelected),new Godot.Collections.Array(2));


        selected=0;
        base._Ready();
    }

    private void onMouseSelected(int selected)
    {
        switch(selected)
        {
            case 0:
                World.instance.resetGamestate();
                GetTree().Paused=false;
                QueueFree();
                break;
            case 1:
                GetTree().Paused=false;
                World.changeScene(ResourceUtils.intro);
                QueueFree();
                break;
            case 2:
                OptionsUI options=OptionsPack.Instance<OptionsUI>();
                options.back=PausePack;
                options.PauseMode=PauseModeEnum.Process;
                World.instance.uiLayer.AddChild(options);
                QueueFree();
                break;
            case 3:
                World.quit();
                break;
        }
    }

}
