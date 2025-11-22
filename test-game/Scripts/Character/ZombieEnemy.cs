using Godot;
using System;

public partial class ZombieEnemy : CharacterBody2D
{
    [Export] public float Speed = 50f;
    [Export] public int MaxHP = 3;
    [Export] public float AttackCooldown = 1.5f;


    private int currentHP;
    private AnimatedSprite2D anim;
    private Node2D player;
    private Timer attackTimer;
    private bool isPlayerInRange = false;
    private float gravity = 400f;

    private int patrolStep = 0;
    private int patrolDirection = -1; // -1: trái, 1: phải
    private float patrolDuration = 1.5f;
    private Timer patrolTimer;

    public override void _Ready()
    {
        AddToGroup("zombie");
        patrolTimer = new Timer();
        patrolTimer.WaitTime = patrolDuration;
        patrolTimer.OneShot = true;
        AddChild(patrolTimer);
        patrolTimer.Timeout += OnPatrolStep;
        patrolTimer.Start();

        currentHP = MaxHP;
        anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Tìm player theo group
        if (player == null)
        {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0)
                player = players[0] as Node2D;
        }

        // Timer để cooldown tấn công
        attackTimer = new Timer();
        attackTimer.WaitTime = AttackCooldown;
        attackTimer.OneShot = true;
        AddChild(attackTimer);

        // Kết nối Area2D
        var area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (currentHP <= 0)
            return;

        if (player == null)
        {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0)
                player = players[0] as Node2D;
        }

        // Lấy velocity hiện tại ra biến tạm
        Vector2 velocity = Velocity;

        // Gravity
        velocity.Y += gravity * (float)delta;

        if (isPlayerInRange && player != null)
        {
            Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
            velocity.X = direction.X * Speed;
            anim.FlipH = direction.X < 0;
            anim.Play("Walk");

            if (GlobalPosition.DistanceTo(player.GlobalPosition) < 20f)
                TryAttack(player);
        }
        else
        {
            velocity.X = patrolDirection * Speed;
            anim.FlipH = patrolDirection < 0;
            anim.Play("Walk");
        }

        // Gán lại vào property Velocity
        Velocity = velocity;

        MoveAndSlide();
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            isPlayerInRange = true;
            patrolTimer.Stop(); // ngừng tuần tra
        }
    }

    private void OnBodyExited(Node body)
    {
        if (!IsInsideTree()) return; // tránh Start() khi zombie đã QueueFree

        if (body.IsInGroup("player"))
        {
            isPlayerInRange = false;

            if (patrolTimer.IsInsideTree())
                patrolTimer.Start();
        }
    }


    private void TryAttack(Node body)
    {
        if (attackTimer.IsStopped())
        {
            attackTimer.Start();
            anim.Play("Attack");

            // Gọi hàm TakeDamage của player
            if (body is MainCharacter playerChar)
            {
                playerChar.TakeDamage();
                GD.Print("Zombie attacked player!");
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;

        if (currentHP > 0)
        {
            anim.Play("Hurt");

            // Tùy bạn, có thể stun zombie 0.3s
            SetPhysicsProcess(false);
            GetTree().CreateTimer(0.3).Timeout += () =>
            {
                SetPhysicsProcess(true);
                anim.Play("Walk");
            };
        }
        else
        {
            Die();
        }
    }

    private void OnPatrolStep()
    {
        if (isPlayerInRange || currentHP <= 0)
            return;

        patrolStep++;

        // Sau 2 bước thì đổi hướng
        if (patrolStep >= 2)
        {
            patrolStep = 0;
            patrolDirection *= -1;
        }

        patrolTimer.Start();
    }

    private void Die()
    {
        Velocity = Vector2.Zero;
        anim.Play("Dead");
        SetPhysicsProcess(false);

        // Delay xoá 0.5s để animation kịp chạy
        GetTree().CreateTimer(2).Timeout += () =>
        {
            QueueFree();
        };
    }

}
