using Godot;
using System;

public class StartUI : BaseUI
{
    private Button start, quit, options;

    public override void _Ready()
    {
        start=GetNode<Button>("Start");
        quit=GetNode<Button>("Quit");
        options=GetNode<Button>("Options");

        start.Connect("mouse_entered",this,nameof(playSfx),new Godot.Collections.Array(sfxHover));
        quit.Connect("mouse_entered",this,nameof(playSfx),new Godot.Collections.Array(sfxHover));
        options.Connect("mouse_entered",this,nameof(playSfx),new Godot.Collections.Array(sfxHover));

        start.Connect("button_down",this,nameof(playSfx),new Godot.Collections.Array(sfxButtons));
        quit.Connect("button_down",this,nameof(playSfx),new Godot.Collections.Array(sfxButtons));
        options.Connect("button_down",this,nameof(playSfx),new Godot.Collections.Array(sfxButtons));

        start.Connect("button_up",this,nameof(onMouseSelected),new Godot.Collections.Array(0));
        quit.Connect("button_up",this,nameof(onMouseSelected),new Godot.Collections.Array(3));
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
			    World.changeScene(ResourceUtils.world);
                GetTree().Paused=false;
                GetParent().QueueFree();
                break;
            case 1:
                GetTree().Paused=false;
                World.changeScene(ResourceUtils.intro);
                GetParent().QueueFree();
                break;
            case 2:
                OptionsUI options=OptionsPack.Instance<OptionsUI>();
                options.PauseMode=PauseModeEnum.Process;
                GetParent().AddChild(options);
                sprite.QueueFree();
                QueueFree();
                break;
            case 3:
                World.quit();
                break;
        }
    }


}
