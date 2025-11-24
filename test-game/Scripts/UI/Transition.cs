using Godot;
using System;
using System.Threading.Tasks;

public partial class Transition : CanvasLayer
{
	private AnimationPlayer anim;

	public override void _Ready()
	{
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		Hide();
	}

	public async Task PlayIn()
{
	Show();
	anim.Play("Transition");
	await ToSignal(anim, "animation_finished");
}

public async Task PlayOut()
{
	Show();
	anim.Seek(anim.CurrentAnimationLength, true); // phải Seek trước
	anim.Play("Transition_out");
	await ToSignal(anim, "animation_finished");
	Hide();
}
}
