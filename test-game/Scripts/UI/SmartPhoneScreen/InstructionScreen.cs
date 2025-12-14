using Godot;
using System;

public partial class InstructionScreen : Control
{
	public override void _Ready()
	{
		// Kết nối sự kiện cho các nút
		GetNode<Button>("InstructionList/Basic").Pressed += () => ShowSection("Basic");
		GetNode<Button>("InstructionList/Inventory").Pressed += () => ShowSection("Inventory");
		GetNode<Button>("InstructionList/Weapon").Pressed += () => ShowSection("Weapon");
		GetNode<Button>("InstructionList/NPC").Pressed += () => ShowSection("NPC");

		GetNode<TextureButton>("InstructionTitle/BackButton").Pressed += OnBackPressed;

		ShowMenu(); // Khởi động ở menu chính
	}

	private void ShowMenu()
	{
		GetNode<VBoxContainer>("InstructionList").Visible = true;

		GetNode<ColorRect>("BasicPanel").Visible = false;
		GetNode<ColorRect>("InventoryPanel").Visible = false;
		GetNode<ColorRect>("WeaponPanel").Visible = false;
		GetNode<ColorRect>("NPCPanel").Visible = false;

		GetNode<TextureButton>("InstructionTitle/BackButton").Visible = false;
		GetNode<Label>("InstructionTitle/TitleLabel").Text = "Instruction";
	}

	private void ShowSection(string section)
	{
		GetNode<VBoxContainer>("InstructionList").Visible = false;

		GetNode<ColorRect>("BasicPanel").Visible = section == "Basic";
		GetNode<ColorRect>("InventoryPanel").Visible = section == "Inventory";
		GetNode<ColorRect>("WeaponPanel").Visible = section == "Weapon";
		GetNode<ColorRect>("NPCPanel").Visible = section == "NPC";

		GetNode<TextureButton>("InstructionTitle/BackButton").Visible = true;
		GetNode<Label>("InstructionTitle/TitleLabel").Text = section;
	}

	private void OnBackPressed()
	{
		ShowMenu();
	}
}
