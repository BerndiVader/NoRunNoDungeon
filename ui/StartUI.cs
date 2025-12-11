using Godot;
using System;

public class StartUI : BaseUI
{
    private Button start, quit, options;

    public override void _Ready()
    {
        base._Ready();

        start=GetNode<Button>("Start");
        quit=GetNode<Button>("Quit");
        options=GetNode<Button>("Options");

        start.Connect("mouse_entered",this,nameof(PlaySfx),new Godot.Collections.Array(sfxHover));
        quit.Connect("mouse_entered",this,nameof(PlaySfx),new Godot.Collections.Array(sfxHover));
        options.Connect("mouse_entered",this,nameof(PlaySfx),new Godot.Collections.Array(sfxHover));

        start.Connect("button_down",this,nameof(PlaySfx),new Godot.Collections.Array(sfxButtons));
        quit.Connect("button_down",this,nameof(PlaySfx),new Godot.Collections.Array(sfxButtons));
        options.Connect("button_down",this,nameof(PlaySfx),new Godot.Collections.Array(sfxButtons));

        start.Connect("button_up",this,nameof(OnMouseSelected),new Godot.Collections.Array(0));
        quit.Connect("button_up",this,nameof(OnMouseSelected),new Godot.Collections.Array(3));
        options.Connect("button_up",this,nameof(OnMouseSelected),new Godot.Collections.Array(2));


        selected=0;
    }

    private void OnMouseSelected(int selected)
    {
        switch(selected)
        {
            case 0:
			    World.ChangeScene(ResourceUtils.world);
                QueueFree();
                break;
            case 1:
                World.ChangeScene(ResourceUtils.intro);
                QueueFree();
                break;
            case 2:
                OptionsUI options=OptionsPack.Instance<OptionsUI>();
                options.PauseMode=PauseModeEnum.Process;
                World.instance.uiLayer.AddChild(options);
                QueueFree();
                break;
            case 3:
                World.Quit();
                break;
        }
    }


}
