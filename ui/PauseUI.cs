using Godot;
using System;

public class PauseUI : BaseUI
{
    private Button cont, quit, main, options, selectedButton;

    public override void _Ready()
    {
        cont=GetNode<Button>("Continue");
        quit=GetNode<Button>("Quit");
        main=GetNode<Button>("Main");
        options=GetNode<Button>("Options");

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
                sprite.QueueFree();
                World.instance.resetGamestate();
                GetTree().Paused=false;
                GetParent().QueueFree();
                break;
            case 1:
                GetTree().Paused=false;
                World.changeScene(ResourceUtils.intro);
                GetParent().QueueFree();
                break;
            case 2:
                OptionsUI pause=OptionsPack.Instance<OptionsUI>();
                pause.PauseMode=PauseModeEnum.Process;
                GetParent().AddChild(pause);
                sprite.QueueFree();
                QueueFree();
                break;
            case 3:
                World.quit();
                break;
        }
    }

}
