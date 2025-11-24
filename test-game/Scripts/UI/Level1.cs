using Godot;
using System;

public partial class Level1 : Node
{
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout"); // đợi canvas ổn định

		var transition = GetNode<Transition>("/root/Transition");
		await transition.PlayOut();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
