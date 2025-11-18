using Godot;
using System;

public partial class AmmoLabel : Label
{
	// Cho phép kéo thả node MainCharacter từ Inspector
	[Export]
	public NodePath PlayerPath;

	private MainCharacter player;

	public override void _Ready()
	{
		if (PlayerPath != null && PlayerPath != "")
		{
			player = GetNode<MainCharacter>(PlayerPath);
		}

		if (player == null)
		{
			GD.PrintErr("AmmoLabel: Không tìm thấy MainCharacter. Hãy kéo thả PlayerPath trong Inspector!");
		}
	}

	public override void _Process(double delta)
	{
		if (player != null)
		{
			// Update text mỗi frame
			Text = $"{player.CurrentAmmo} / {player.MaxAmmo}";
		}
	}
}
