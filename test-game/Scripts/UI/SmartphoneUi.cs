using Godot;
using System;

public partial class SmartphoneUi : Control
{
	// === NODES ===
	private Control musicScreen;
	private Control instructionScreen;
	private Control settingScreen;
	private Control iconMenu;
	private Button homeButton;

	private bool isOpen = false; // trạng thái điện thoại
	private TextureRect phonePanel; // node chính hiển thị smartphone

	// Vị trí hiển thị và vị trí ẩn
	private Vector2 showPosition = new Vector2(915, 390);   // tuỳ chỉnh theo màn hình
	private Vector2 hidePosition = new Vector2(1500, 390);  // ngoài màn hình bên phải

	public override void _Ready()
	{
		// === GET NODES ===
		iconMenu = GetNode<Control>("Panel/IconMenu");
		musicScreen = GetNode<Control>("Panel/MusicScreen");
		instructionScreen = GetNode<Control>("Panel/InstructionScreen");
		settingScreen = GetNode<Control>("Panel/SettingScreen");
		homeButton = GetNode<Button>("Panel/HomeButton");
		phonePanel = GetNode<TextureRect>("Panel"); // smartphone asset


		// === KHỞI TẠO ===
		ShowHome();
		Visible = false; // mặc định điện thoại ẩn
		phonePanel.Position = hidePosition; // bắt đầu ngoài màn hình

		// === KẾT NỐI NÚT ICON ===
		GetNode<TextureButton>("Panel/IconMenu/Music").Pressed += OnMusicPressed;
		GetNode<TextureButton>("Panel/IconMenu/Instruction").Pressed += OnInstructionPressed;
		GetNode<TextureButton>("Panel/IconMenu/Setting").Pressed += OnSettingPressed;

		// === NÚT HOME ===
		homeButton.Pressed += ShowHome;
	}

	public override void _Process(double delta)
	{

	}

	public void ShowPhone()
	{
		isOpen = true;
		ShowHome();

		phonePanel.Position = hidePosition; // bắt đầu ngoài màn hình
		Visible = true;

		var tween = CreateTween();
		tween.TweenProperty(phonePanel, "position", showPosition, 0.5f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
	}

	public void HidePhone()
	{
		isOpen = false;

		var tween = CreateTween();
		tween.TweenProperty(phonePanel, "position", hidePosition, 0.5f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.In);

		tween.Finished += () =>
		{
			Visible = false;
		};
	}

	private void ShowHome()
	{
		iconMenu.Visible = true;
		musicScreen.Visible = false;
		instructionScreen.Visible = false;
		settingScreen.Visible = false;
	}

	private void OnMusicPressed()
	{
		iconMenu.Visible = false;
		musicScreen.Visible = true;
		instructionScreen.Visible = false;
		settingScreen.Visible = false;
	}

	private void OnInstructionPressed()
	{
		iconMenu.Visible = false;
		musicScreen.Visible = false;
		instructionScreen.Visible = true;
		settingScreen.Visible = false;
	}

	private void OnSettingPressed()
	{
		iconMenu.Visible = false;
		musicScreen.Visible = false;
		instructionScreen.Visible = false;
		settingScreen.Visible = true;
	}
}
