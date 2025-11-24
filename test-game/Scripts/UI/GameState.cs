using Godot;
using System;

public partial class GameState : Node
{
	// Lưu đường dẫn level hiện tại
	public string LastLevelPath = "";

	// Cờ trạng thái pause
	public bool IsPaused = false;

	// Bạn có thể thêm các biến khác nếu cần
	public int PlayerScore = 0;
	public int PlayerLives = 3;
}
