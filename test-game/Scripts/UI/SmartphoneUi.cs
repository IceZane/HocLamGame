using Godot;
using System;

public partial class SmartphoneUi : CanvasLayer
{
	// === NODES ===
	private Control musicScreen;
	private Control messageScreen;
	private Control settingScreen;
	private Control iconMenu;
	private Button homeButton;

	private bool isOpen = false; // trạng thái điện thoại
	private TextureRect phonePanel; // node chính hiển thị smartphone

	// Vị trí hiển thị và vị trí ẩn
	private Vector2 showPosition = new Vector2(900, 100);   // tuỳ chỉnh theo màn hình
	private Vector2 hidePosition = new Vector2(1600, 100);  // ngoài màn hình bên phải

	public override void _Ready()
	{
		// === GET NODES ===
		iconMenu = GetNode<Control>("Panel/IconMenu");
		musicScreen = GetNode<Control>("Panel/MusicScreen");
		messageScreen = GetNode<Control>("Panel/MessageScreen");
		settingScreen = GetNode<Control>("Panel/SettingScreen");
		homeButton = GetNode<Button>("Panel/HomeButton");
		phonePanel = GetNode<TextureRect>("Panel"); // smartphone asset

		// === KHỞI TẠO ===
		ShowHome();
		Visible = false; // mặc định điện thoại ẩn
		phonePanel.Position = hidePosition; // bắt đầu ngoài màn hình

		// === KẾT NỐI NÚT ICON ===
		GetNode<TextureButton>("Panel/IconMenu/Music").Pressed += OnMusicPressed;
		GetNode<TextureButton>("Panel/IconMenu/Message").Pressed += OnMessagePressed;
		GetNode<TextureButton>("Panel/IconMenu/Setting").Pressed += OnSettingPressed;

		// === NÚT HOME ===
		homeButton.Pressed += ShowHome;
	}

	public override void _Process(double delta)
	{

	}

	public void ShowPhone()
	{
		Visible = true;
		isOpen = true;
		ShowHome();

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
		messageScreen.Visible = false;
		settingScreen.Visible = false;
	}

	private void OnMusicPressed()
	{
		iconMenu.Visible = false;
		musicScreen.Visible = true;
		messageScreen.Visible = false;
		settingScreen.Visible = false;
	}

	private void OnMessagePressed()
	{
		iconMenu.Visible = false;
		musicScreen.Visible = false;
		messageScreen.Visible = true;
		settingScreen.Visible = false;
	}

	private void OnSettingPressed()
	{
		iconMenu.Visible = false;
		musicScreen.Visible = false;
		messageScreen.Visible = false;
		settingScreen.Visible = true;
	}
}
