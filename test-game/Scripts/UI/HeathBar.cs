using Godot;
using System;
using System.Linq;

public partial class HeathBar : AnimatedSprite2D
{
	private MainCharacter player;
	private string lastAnim = "";

	public override void _Ready()
	{
		Stop();
		SpeedScale = 0f;
		AddToGroup("healthbar");

		// Tìm nhân vật theo group
		var players = GetTree().GetNodesInGroup("player");
		if (players.Count > 0)
			player = players[0] as MainCharacter;

		if (player == null)
		{
			GD.PrintErr("Không tìm thấy MainCharacter trong group 'player'!");
			return;
		}

		UpdateHealth(player.GetHP());
	}

	public override void _Process(double delta)
	{
		if (player == null)
			return;

		UpdateHealth(player.GetHP());
	}

	private void UpdateHealth(int hp)
	{
		// Map HP sang tên animation
		string animName = hp switch
		{
			0 => "0 HP",
			1 => "1 HP",
			2 => "2 HP",
			3 => "3 HP", 
			4 => "Full HP", // nếu bạn dùng Full HP cho 4
			_ => "0 HP"
		};

		// Chỉ đổi nếu khác animation hiện tại
		if (Animation != animName)
		{
			Animation = animName;
			Frame = 0;
			lastAnim = animName;
		}
	}
}
