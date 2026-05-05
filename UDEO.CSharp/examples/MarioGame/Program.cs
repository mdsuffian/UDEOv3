using System.Diagnostics;
using UDEO.Core;
using UDEO.Core.Configuration;
using UDEO.Core.Logging;
using UDEO.Core.Models;
using UDEO.Experts;
using UDEO.Experts.BuiltIn;
using UDEO.Pipeline;
using UDEO.Store;

namespace UDEO.Mario;

/// <summary>
/// Super UDEO Mario — A console-based Mario platformer powered by the UDEO Expert Pipeline Framework.
/// Every game tick is processed through a deterministic expert pipeline.
/// All decisions are logged as auditable decision traces.
/// C# rewrite from MarioGame.ps1.
/// </summary>
public static class MarioGame
{
    #region Constants

    private const int GameWidth = 80;
    private const int GameHeight = 10;
    private const int FrameDelayMs = 50;
    private const int TotalLevels = 3;

    private static readonly char[] SolidChars = { '#', '[', ']', '|', 'B', '?', 'H' };
    private static readonly char[] PlatformChars = { '#', '[', ']', 'B', 'H' };

    #endregion

    #region Game State Types

    private sealed class GameState
    {
        public int LevelNum { get; set; } = 1;
        public int TotalLevels { get; set; } = TotalLevels;
        public int LevelsCompleted { get; set; }
        public int Score { get; set; }
        public int HighScore { get; set; }
        public int Lives { get; set; } = 3;
        public int Coins { get; set; }
        public string GameStatus { get; set; } = "PLAYING";
        public bool LevelComplete { get; set; }
        public int Frame { get; set; }

        public LevelData Level { get; set; } = new();
        public MarioData Mario { get; set; } = new();
        public List<EnemyData> Enemies { get; set; } = new();
        public PhysicsParams Physics { get; set; } = new();
        public InputIntent InputIntent { get; set; } = new();
        public string? LastKey { get; set; }
        public PipelineResult? PipelineResult { get; set; }
    }

    private sealed class LevelData
    {
        public int Width { get; set; } = GameWidth;
        public int Height { get; set; } = GameHeight;
        public char[][] Grid { get; set; } = Array.Empty<char[]>();
    }

    private sealed class MarioData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 1;
        public double Height { get; set; } = 1;
        public double Vy { get; set; }
        public bool OnGround { get; set; } = true;
        public bool IsJumping { get; set; }
        public bool IsBig { get; set; }
        public string Facing { get; set; } = "right";
        public int InvincibleFrames { get; set; }
        public int JumpCooldown { get; set; }
        public bool HeadBumpedBlock { get; set; }
    }

    private sealed class EnemyData
    {
        public string Type { get; init; } = "goomba";
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 1;
        public double Height { get; set; } = 1;
        public double Speed { get; set; } = 0.15;
        public bool Alive { get; set; } = true;
        public int Points { get; init; } = 200;
    }

    private sealed class PhysicsParams
    {
        public double Gravity { get; set; } = 0.3;
        public double MaxFallSpeed { get; set; } = 2.0;
        public double JumpPower { get; set; } = 1.8;
        public double BouncePower { get; set; } = 1.0;
        public double MoveSpeed { get; set; } = 0.3;
        public int InvincibleDuration { get; set; } = 60;
        public int JumpCooldownFrames { get; set; } = 5;
    }

    private sealed class InputIntent
    {
        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }
        public bool Jump { get; set; }
        public bool Quit { get; set; }
    }

    #endregion

    #region Level Definitions

    private static string[] GetLevel(int levelNum)
    {
        return levelNum switch
        {
            1 => new[]
            {
                "                                                                                ",
                "                                                                                ",
                "                                    o                                            ",
                "                                         o    o                                  ",
                "              ?    o            G                                      F          ",
                "   o                o       #####        o       G                               ",
                "          G                                                   o         ####      ",
                "      o          G                 o                              o               ",
                " |           ########         |         ########        |          ######        ",
                "################################################################################"
            },
            2 => new[]
            {
                "                                                                                ",
                "                                     o                                           ",
                "                            M                    o                              ",
                "              o                  G              #####                F           ",
                "   ?         ###      o                G                    o      #######        ",
                "       G              ###       o              K        G                       ",
                "  o        o                G           o                #####                  ",
                "      G          o                 G              o                o             ",
                " |        |           #####         |    |          ######         |    |        ",
                "################################################################################"
            },
            3 => new[]
            {
                "                                                                                ",
                "                                     F                                           ",
                "                                   #####                                         ",
                "              o                o         o                                       ",
                "   ?     ?         G    K              ##########        o      ?                ",
                "       G      o         G       o                G    K                          ",
                "  o        o                ########           K           o                     ",
                "      G          o                 G              o                o             ",
                " |   |    |         #####    |    |    |    |    ######    |    |    |    |      ",
                "################################################################################"
            },
            _ => GenerateRandomLevel()
        };
    }

    private static string[] GenerateRandomLevel()
    {
        var rng = new Random();
        var lines = new string[GameHeight];
        for (int r = 0; r < GameHeight; r++)
        {
            var chars = new char[GameWidth];
            for (int c = 0; c < GameWidth; c++)
            {
                chars[c] = r == 9
                    ? (rng.Next(1, 12) == 1 ? ' ' : '#')
                    : rng.Next(1, 100) switch
                    {
                        <= 2 => 'o',
                        <= 4 => 'G',
                        <= 5 => '|',
                        <= 6 => '?',
                        _ => ' '
                    };
            }
            lines[r] = new string(chars);
        }
        // Ensure flag exists
        var line8 = lines[8].ToCharArray();
        line8[70] = 'F';
        lines[8] = new string(line8);
        return lines;
    }

    #endregion

    #region Initialization

    private static GameState InitGameState(int levelNum)
    {
        var levelData = GetLevel(levelNum);
        var height = levelData.Length;
        var width = levelData[0].Length;

        var grid = levelData.Select(line => line.ToCharArray()).ToArray();

        // Find Mario start position
        int startX = 1;
        int startY = height - 2;
        for (int r = height - 2; r >= 0; r--)
        {
            if (r + 1 < height && IsPlatform(grid[r + 1][startX]))
            {
                startY = r;
                break;
            }
        }

        // Parse enemies from grid
        var enemies = new List<EnemyData>();
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                var cell = grid[r][c];
                if (cell == 'G' || cell == 'K')
                {
                    int groundY = r;
                    for (int gr = r; gr < height; gr++)
                    {
                        if (IsPlatform(grid[gr][c])) { groundY = gr - 1; break; }
                    }
                    enemies.Add(new EnemyData
                    {
                        Type = cell == 'G' ? "goomba" : "koopa",
                        X = c,
                        Y = groundY,
                        Speed = cell == 'G' ? 0.15 : 0.2,
                        Points = cell == 'G' ? 200 : 400
                    });
                }
            }
        }

        // Clear enemy chars from grid
        for (int r = 0; r < height; r++)
            for (int c = 0; c < width; c++)
                if (grid[r][c] == 'G' || grid[r][c] == 'K')
                    grid[r][c] = ' ';

        return new GameState
        {
            LevelNum = levelNum,
            Score = 0,
            Lives = 3,
            Coins = 0,
            Level = new LevelData { Width = width, Height = height, Grid = grid },
            Mario = new MarioData { X = startX, Y = startY, OnGround = true, Facing = "right" },
            Enemies = enemies,
            Physics = new PhysicsParams(),
            InputIntent = new InputIntent()
        };
    }

    #endregion

    #region Expert Pipeline

    private static UdeoPipeline CreateGamePipeline(ExecutionContext context)
    {
        var pipeline = new UdeoPipeline("MarioGame-Frame");
        pipeline.Context = context;

        pipeline.AddStep("mario.input", onFailure: FailurePolicy.Continue);
        pipeline.AddStep("mario.physics", onFailure: FailurePolicy.Continue);
        pipeline.AddStep("mario.move", onFailure: FailurePolicy.Continue);
        pipeline.AddStep("mario.enemy", onFailure: FailurePolicy.Continue);
        pipeline.AddStep("mario.interact", onFailure: FailurePolicy.Continue);
        pipeline.AddStep("mario.scoring", onFailure: FailurePolicy.Continue);

        return pipeline;
    }

    private static void RegisterMarioExperts()
    {
        RegisterMarioInputExpert();
        RegisterMarioPhysicsExpert();
        RegisterMarioMoveExpert();
        RegisterMarioEnemyExpert();
        RegisterMarioInteractExpert();
        RegisterMarioScoringExpert();
    }

    private static void RegisterMarioInputExpert()
    {
        var contract = new ExpertContract("mario.input", "Mario Input Handler", UdeoExpertType.Custom,
            (ctx, parameters) =>
            {
                var key = ctx.Data.TryGetValue("last_key", out var k) ? k?.ToString() : null;

                ctx.Data["input_intent"] = new InputIntent();

                if (key == null)
                    return ExpertResult.SuccessResult("VALID", "NO_INPUT");

                var intent = ctx.Data.TryGetValue("input_intent", out var ii) && ii is InputIntent intentObj
                    ? intentObj : new InputIntent();

                switch (key)
                {
                    case "LeftArrow" or "A": intent.MoveLeft = true; break;
                    case "RightArrow" or "D": intent.MoveRight = true; break;
                    case "Spacebar" or "W" or "UpArrow": intent.Jump = true; break;
                    case "Escape" or "Q": intent.Quit = true; break;
                }

                ctx.Data["input_intent"] = intent;
                return ExpertResult.SuccessResult("VALID", "INPUT_PROCESSED");
            })
        { TimeoutSeconds = 1 };
        ExpertRegistry.Instance.Register(contract);
    }

    private static void RegisterMarioPhysicsExpert()
    {
        var contract = new ExpertContract("mario.physics", "Mario Physics Engine", UdeoExpertType.Math,
            (ctx, parameters) =>
            {
                var mario = GetState<MarioData>(ctx, "mario") ?? new MarioData();
                var level = GetState<LevelData>(ctx, "level") ?? new LevelData();
                var intent = GetState<InputIntent>(ctx, "input_intent") ?? new InputIntent();
                var physics = GetState<PhysicsParams>(ctx, "physics") ?? new PhysicsParams();

                var grid = level.Grid;
                int width = level.Width, height = level.Height;

                // Gravity
                if (!mario.OnGround)
                    mario.Vy = Math.Min(mario.Vy + physics.Gravity, physics.MaxFallSpeed);

                // Jump
                if (intent.Jump && mario.OnGround && mario.JumpCooldown <= 0)
                {
                    mario.Vy = -physics.JumpPower;
                    mario.OnGround = false;
                    mario.IsJumping = true;
                    mario.JumpCooldown = physics.JumpCooldownFrames;
                }
                if (mario.JumpCooldown > 0) mario.JumpCooldown--;

                double newY = mario.Y + mario.Vy;

                // Ceiling clamp
                if (newY < 0) { newY = 0; mario.Vy = 0; }

                // Ground collision
                int floorY = (int)Math.Floor(newY + mario.Height);
                if (floorY >= height)
                {
                    newY = height - mario.Height;
                    mario.Vy = 0;
                    mario.OnGround = true;
                    mario.IsJumping = false;
                }
                else
                {
                    bool onSolid = false;
                    for (int cx = (int)Math.Floor(mario.X); cx < (int)Math.Ceiling(mario.X + mario.Width); cx++)
                    {
                        if (cx >= 0 && cx < width && floorY >= 0 && floorY < height && IsSolid(grid[floorY][cx]))
                        { onSolid = true; break; }
                    }
                    if (onSolid && mario.Vy >= 0)
                    {
                        newY = floorY - mario.Height;
                        mario.Vy = 0;
                        mario.OnGround = true;
                        mario.IsJumping = false;
                    }
                    else mario.OnGround = false;
                }

                // Ceiling collision
                mario.HeadBumpedBlock = false;
                if (mario.Vy < 0)
                {
                    int ceilY = (int)Math.Floor(newY);
                    if (ceilY >= 0)
                    {
                        for (int cx = (int)Math.Floor(mario.X); cx < (int)Math.Ceiling(mario.X + mario.Width); cx++)
                        {
                            if (cx >= 0 && cx < width && IsSolid(grid[ceilY][cx]))
                            {
                                newY = ceilY + 1;
                                mario.Vy = 0;
                                if (grid[ceilY][cx] == '?') mario.HeadBumpedBlock = true;
                                break;
                            }
                        }
                    }
                }

                mario.Y = newY;
                ctx.Data["mario"] = mario;
                return ExpertResult.SuccessResult("VALID", $"PHYSICS_UPDATED:vy={mario.Vy:F2}:ground={mario.OnGround}");
            })
        { TimeoutSeconds = 2 };
        ExpertRegistry.Instance.Register(contract);
    }

    private static void RegisterMarioMoveExpert()
    {
        var contract = new ExpertContract("mario.move", "Mario Movement Handler", UdeoExpertType.Custom,
            (ctx, parameters) =>
            {
                var mario = GetState<MarioData>(ctx, "mario") ?? new MarioData();
                var level = GetState<LevelData>(ctx, "level") ?? new LevelData();
                var intent = GetState<InputIntent>(ctx, "input_intent") ?? new InputIntent();
                var physics = GetState<PhysicsParams>(ctx, "physics") ?? new PhysicsParams();

                var grid = level.Grid;
                int width = level.Width, height = level.Height;
                double speed = physics.MoveSpeed;
                double dx = 0;

                if (intent.MoveLeft) dx = -speed;
                if (intent.MoveRight) dx = speed;

                if (dx == 0)
                    return ExpertResult.SuccessResult("VALID", "NO_MOVEMENT");

                mario.Facing = dx < 0 ? "left" : "right";
                double newX = mario.X + dx;

                // Screen bounds
                if (newX < 0) newX = 0;
                if (newX + mario.Width > width) newX = width - mario.Width;

                // Wall collision
                int leftCol = (int)Math.Floor(newX);
                int rightCol = (int)Math.Ceiling(newX + mario.Width) - 1;
                int topRow = (int)Math.Floor(mario.Y);

                for (int row = topRow; row < topRow + mario.Height; row++)
                {
                    if (row < 0 || row >= height) continue;
                    if (leftCol >= 0 && leftCol < width && IsSolid(grid[row][leftCol]))
                    { newX = leftCol + 1; dx = 0; break; }
                    if (rightCol >= 0 && rightCol < width && IsSolid(grid[row][rightCol]))
                    { newX = rightCol - mario.Width; dx = 0; break; }
                }

                mario.X = newX;
                ctx.Data["mario"] = mario;
                return ExpertResult.SuccessResult("VALID", $"MOVED:dx={dx}:x={mario.X:F2}");
            })
        { TimeoutSeconds = 2 };
        ExpertRegistry.Instance.Register(contract);
    }

    private static void RegisterMarioEnemyExpert()
    {
        var contract = new ExpertContract("mario.enemy", "Mario Enemy System", UdeoExpertType.Custom,
            (ctx, parameters) =>
            {
                var mario = GetState<MarioData>(ctx, "mario") ?? new MarioData();
                var level = GetState<LevelData>(ctx, "level") ?? new LevelData();
                var enemies = GetState<List<EnemyData>>(ctx, "enemies") ?? new List<EnemyData>();
                var physics = GetState<PhysicsParams>(ctx, "physics") ?? new PhysicsParams();

                var grid = level.Grid;
                int width = level.Width, height = level.Height;
                var rulesFired = new List<string>();

                foreach (var enemy in enemies)
                {
                    if (!enemy.Alive) continue;

                    double eSpeed = enemy.Speed;
                    double newEX = enemy.X + eSpeed;

                    int checkCol = eSpeed > 0
                        ? (int)Math.Ceiling(newEX + enemy.Width) - 1
                        : (int)Math.Floor(newEX);
                    int feetRow = (int)Math.Floor(enemy.Y + enemy.Height);
                    int bodyRow = (int)Math.Floor(enemy.Y);

                    bool hitWall = false;
                    if (checkCol < 0 || checkCol >= width || feetRow >= height)
                        hitWall = true;
                    else
                    {
                        if (bodyRow >= 0 && IsSolid(grid[bodyRow][checkCol]))
                            hitWall = true;
                        if (feetRow >= 0 && feetRow < height && !IsSolid(grid[feetRow][checkCol]))
                            hitWall = true;
                    }

                    if (hitWall) enemy.Speed = -enemy.Speed;
                    else enemy.X = newEX;

                    // Mario-Enemy collision
                    if (mario.InvincibleFrames <= 0)
                    {
                        double mL = mario.X, mR = mario.X + mario.Width;
                        double mT = mario.Y, mB = mario.Y + mario.Height;
                        double eL = enemy.X, eR = enemy.X + enemy.Width;
                        double eT = enemy.Y, eB = enemy.Y + enemy.Height;

                        if (mL < eR && mR > eL && mT < eB && mB > eT)
                        {
                            if (mario.Vy > 0 && mT < eT)
                            {
                                enemy.Alive = false;
                                mario.Vy = -physics.BouncePower;
                                ctx.Data["score"] = (GetState<int>(ctx, "score") + enemy.Points);
                                rulesFired.Add($"ENEMY_STOMPED:{enemy.Type}");
                            }
                            else if (!mario.IsBig)
                            {
                                ctx.Data["lives"] = (GetState<int>(ctx, "lives") - 1);
                                mario.InvincibleFrames = physics.InvincibleDuration;
                                rulesFired.Add("DAMAGE_TAKEN");
                            }
                            else
                            {
                                mario.IsBig = false;
                                mario.Height = 1;
                                mario.InvincibleFrames = physics.InvincibleDuration;
                                rulesFired.Add("POWER_DOWN");
                            }
                        }
                    }
                }

                if (mario.InvincibleFrames > 0) mario.InvincibleFrames--;

                ctx.Data["mario"] = mario;
                ctx.Data["enemies"] = enemies;

                int aliveCount = enemies.Count(e => e.Alive);
                string ruleStr = rulesFired.Count > 0 ? string.Join("|", rulesFired) : "NO_INTERACTION";
                return ExpertResult.SuccessResult("VALID", $"ENEMIES_UPDATED:alive={aliveCount}:{ruleStr}");
            })
        { TimeoutSeconds = 3 };
        ExpertRegistry.Instance.Register(contract);
    }

    private static void RegisterMarioInteractExpert()
    {
        var contract = new ExpertContract("mario.interact", "Mario Interaction Handler", UdeoExpertType.Custom,
            (ctx, parameters) =>
            {
                var mario = GetState<MarioData>(ctx, "mario") ?? new MarioData();
                var level = GetState<LevelData>(ctx, "level") ?? new LevelData();
                var score = GetState<int>(ctx, "score");
                var coins = GetState<int>(ctx, "coins");
                var grid = level.Grid;
                int width = level.Width, height = level.Height;
                var rulesFired = new List<string>();

                int startCol = (int)Math.Floor(mario.X);
                int endCol = (int)Math.Ceiling(mario.X + mario.Width) - 1;
                int startRow = (int)Math.Floor(mario.Y);
                int endRow = (int)Math.Ceiling(mario.Y + mario.Height) - 1;

                for (int row = startRow; row <= endRow; row++)
                {
                    for (int col = startCol; col <= endCol; col++)
                    {
                        if (row < 0 || row >= height || col < 0 || col >= width) continue;
                        var cell = grid[row][col];
                        switch (cell)
                        {
                            case 'o':
                                grid[row][col] = ' ';
                                score += 100;
                                coins++;
                                rulesFired.Add("COIN_COLLECTED");
                                break;
                            case 'M':
                                grid[row][col] = ' ';
                                mario.IsBig = true;
                                mario.Height = 2;
                                score += 1000;
                                rulesFired.Add("POWER_UP");
                                break;
                            case 'F':
                                score += 2000;
                                ctx.Data["level_complete"] = true;
                                rulesFired.Add("FLAG_REACHED");
                                break;
                        }
                    }
                }

                if (mario.HeadBumpedBlock)
                {
                    int headRow = (int)Math.Floor(mario.Y) - 1;
                    for (int col = startCol; col <= endCol; col++)
                    {
                        if (col >= 0 && col < width && headRow >= 0 && headRow < height && grid[headRow][col] == '?')
                        {
                            grid[headRow][col] = 'B';
                            score += 100;
                            coins++;
                            rulesFired.Add("BLOCK_HIT");
                        }
                    }
                    mario.HeadBumpedBlock = false;
                }

                ctx.Data["mario"] = mario;
                ctx.Data["level"] = level;
                ctx.Data["score"] = score;
                ctx.Data["coins"] = coins;

                string ruleStr = rulesFired.Count > 0 ? string.Join("|", rulesFired) : "NO_INTERACTION";
                return ExpertResult.SuccessResult("VALID", $"INTERACT:{ruleStr}");
            })
        { TimeoutSeconds = 2 };
        ExpertRegistry.Instance.Register(contract);
    }

    private static void RegisterMarioScoringExpert()
    {
        var contract = new ExpertContract("mario.scoring", "Mario Score Tracker", UdeoExpertType.Custom,
            (ctx, parameters) =>
            {
                var score = GetState<int>(ctx, "score");
                var coins = GetState<int>(ctx, "coins");
                var lives = GetState<int>(ctx, "lives");

                // Extra life every 100 coins
                if (coins >= 100)
                {
                    coins -= 100;
                    lives++;
                    ctx.Data["coins"] = coins;
                    ctx.Data["lives"] = lives;
                    UdeoLogger.Instance.Info($"EXTRA LIFE! Total lives: {lives}");
                }

                string state = "PLAYING";
                string decisionCode = "VALID";
                string ruleFired = "SCORE_UPDATED";

                if (ctx.Data.TryGetValue("level_complete", out var lc) && lc is true)
                {
                    state = "LEVEL_COMPLETE";
                    decisionCode = "APPROVED";
                    int completed = GetState<int>(ctx, "levels_completed") + 1;
                    ctx.Data["levels_completed"] = completed;
                    if (completed >= GetState<int>(ctx, "total_levels"))
                    {
                        state = "VICTORY";
                    }
                    ruleFired = $"LEVEL_COMPLETE:score={score}:lives={lives}";
                }
                else if (lives <= 0)
                {
                    state = "GAME_OVER";
                    decisionCode = "REJECTED";
                    ruleFired = $"GAME_OVER:score={score}";
                }

                ctx.Data["game_state"] = state;
                if (score > GetState<int>(ctx, "high_score"))
                    ctx.Data["high_score"] = score;

                return ExpertResult.SuccessResult(decisionCode, ruleFired);
            })
        { TimeoutSeconds = 1 };
        ExpertRegistry.Instance.Register(contract);
    }

    #endregion

    #region Rendering

    private static void RenderFrame(GameState state)
    {
        Console.Clear();

        // Title
        WriteColored("\n  SUPER UDEO MARIO  -  Powered by Expert Pipeline Framework",
            ConsoleColor.Cyan, ConsoleColor.Black);
        WriteColored("  " + new string('-', 76), ConsoleColor.DarkGray);

        // HUD
        string hud = string.Format("{0,-14} {1,-18} {2,-14} {3,-14} {4,10}",
            $"WORLD 1-{state.LevelNum}", $"SCORE: {state.Score}",
            $"COINS: {state.Coins}", $"LIVES: {state.Lives}",
            $"HIGH: {state.HighScore}");
        Console.WriteLine($"  {hud}");
        WriteColored("  " + new string('=', 76), ConsoleColor.DarkGray);

        // Render level
        var grid = state.Level.Grid;
        int height = state.Level.Height;
        int width = state.Level.Width;
        var mario = state.Mario;

        for (int r = 0; r < height; r++)
        {
            Console.Write("  ");
            for (int c = 0; c < width; c++)
            {
                char renderedChar = grid[r][c];
                ConsoleColor renderedColor = GetCellColor(renderedChar);

                // Mario overlap
                int mLeft = (int)Math.Floor(mario.X);
                int mTop = (int)Math.Floor(mario.Y);
                int mRight = (int)Math.Ceiling(mario.X + mario.Width) - 1;
                int mBottom = (int)Math.Ceiling(mario.Y + mario.Height) - 1;

                if (c >= mLeft && c <= mRight && r >= mTop && r <= mBottom)
                {
                    if (mario.InvincibleFrames > 0 && state.Frame % 6 < 3)
                    {
                        renderedChar = ' ';
                    }
                    else
                    {
                        renderedChar = 'M';
                        renderedColor = ConsoleColor.Red;
                    }
                }
                else
                {
                    // Enemy overlap
                    foreach (var enemy in state.Enemies)
                    {
                        if (!enemy.Alive) continue;
                        int eLeft = (int)Math.Floor(enemy.X);
                        int eTop = (int)Math.Floor(enemy.Y);
                        int eRight = (int)Math.Ceiling(enemy.X + enemy.Width) - 1;
                        if (c >= eLeft && c <= eRight && r == eTop)
                        {
                            renderedChar = enemy.Type == "goomba" ? 'G' : 'K';
                            renderedColor = enemy.Type == "goomba" ? ConsoleColor.DarkRed : ConsoleColor.Magenta;
                            break;
                        }
                    }
                }

                WriteChar(renderedChar, renderedColor);
            }
            Console.WriteLine();
        }

        // Footer
        WriteColored("  " + new string('=', 76), ConsoleColor.DarkGray);
        Console.WriteLine("  [Arrow Keys/WASD: Move]  [Space: Jump]  [Q/Esc: Quit]");

        // Decision trace (last 3)
        if (state.PipelineResult?.Trace?.Count > 0)
        {
            WriteColored("\n  Last Expert Decisions:", ConsoleColor.Cyan);
            var recent = state.PipelineResult.Trace.TakeLast(3);
            foreach (var t in recent)
            {
                var line = $"    {t.ExpertId,-18} {t.DecisionCode,-10} {t.RuleFired,-40} {t.ExecutionTimeMs,6:F2}ms";
                var color = t.DecisionCode switch
                {
                    "VALID" => ConsoleColor.DarkGray,
                    "APPROVED" => ConsoleColor.Green,
                    "REJECTED" => ConsoleColor.Red,
                    _ => ConsoleColor.White
                };
                WriteColored(line, color);
            }
        }
    }

    private static ConsoleColor GetCellColor(char cell)
    {
        return cell switch
        {
            '#' => ConsoleColor.DarkGreen,
            '[' or ']' => ConsoleColor.DarkYellow,
            '|' => ConsoleColor.DarkGreen,
            'B' => ConsoleColor.Gray,
            '?' => ConsoleColor.Yellow,
            'o' => ConsoleColor.Yellow,
            'M' => ConsoleColor.Red,
            'F' => ConsoleColor.Cyan,
            'G' => ConsoleColor.DarkRed,
            'K' => ConsoleColor.Magenta,
            'H' => ConsoleColor.DarkGray,
            _ => ConsoleColor.DarkGray
        };
    }

    #endregion

    #region Game Loop

    private static void GameLoop(GameState state)
    {
        var pipelineId = $"mario_session_{DateTime.Now:yyyyMMdd_HHmmss}";
        var context = new ExecutionContext(pipelineId);
        context.Data["mario"] = state.Mario;
        context.Data["level"] = state.Level;
        context.Data["enemies"] = state.Enemies;
        context.Data["physics"] = state.Physics;
        context.Data["score"] = state.Score;
        context.Data["lives"] = state.Lives;
        context.Data["coins"] = state.Coins;
        context.Data["high_score"] = state.HighScore;
        context.Data["levels_completed"] = state.LevelsCompleted;
        context.Data["total_levels"] = state.TotalLevels;
        context.Data["level_complete"] = false;
        context.Data["game_state"] = "PLAYING";

        var pipeline = CreateGamePipeline(context);

        bool quit = false;
        while (!quit)
        {
            state.Frame++;

            // Read input
            state.LastKey = ReadKey();
            if (state.LastKey is "Escape" or "Q")
                state.InputIntent.Quit = true;

            // Update context
            context.Data["mario"] = state.Mario;
            context.Data["level"] = state.Level;
            context.Data["enemies"] = state.Enemies;
            context.Data["physics"] = state.Physics;
            context.Data["input_intent"] = state.InputIntent;
            context.Data["last_key"] = state.LastKey;
            context.Data["score"] = state.Score;
            context.Data["lives"] = state.Lives;
            context.Data["coins"] = state.Coins;
            context.Data["high_score"] = state.HighScore;
            context.Data["levels_completed"] = state.LevelsCompleted;
            context.Data["total_levels"] = state.TotalLevels;
            context.Data["frame"] = state.Frame;

            // Run pipeline
            var result = pipeline.Run();
            state.PipelineResult = result;

            // Sync state back
            state.Mario = GetState<MarioData>(context, "mario") ?? state.Mario;
            state.Enemies = GetState<List<EnemyData>>(context, "enemies") ?? state.Enemies;
            state.Level = GetState<LevelData>(context, "level") ?? state.Level;
            state.Score = GetState<int>(context, "score");
            state.Lives = GetState<int>(context, "lives");
            state.Coins = GetState<int>(context, "coins");
            state.HighScore = GetState<int>(context, "high_score");
            state.LevelsCompleted = GetState<int>(context, "levels_completed");

            var gameStatus = context.Data.TryGetValue("game_state", out var gs) ? gs?.ToString() ?? "PLAYING" : "PLAYING";
            state.GameStatus = gameStatus;

            // Recreate pipeline for next frame
            pipeline = CreateGamePipeline(context);

            // Render
            RenderFrame(state);

            // Check game state
            switch (gameStatus)
            {
                case "GAME_OVER":
                    ShowGameOver(state, context);
                    quit = true;
                    break;
                case "LEVEL_COMPLETE":
                    ShowLevelComplete(state);
                    if (state.LevelsCompleted >= state.TotalLevels)
                    {
                        ShowVictory(state, context);
                        quit = true;
                    }
                    else
                    {
                        var newState = InitGameState(state.LevelNum + 1);
                        newState.Score = state.Score;
                        newState.Lives = state.Lives;
                        newState.HighScore = state.HighScore;
                        newState.Coins = state.Coins;
                        newState.LevelsCompleted = state.LevelsCompleted;

                        WrapState(newState, context);
                        pipeline = CreateGamePipeline(context);

                        ShowLevelIntro(newState);
                        state = newState;
                    }
                    break;
                case "VICTORY":
                    ShowVictory(state, context);
                    quit = true;
                    break;
            }

            if (state.InputIntent.Quit)
            {
                UdeoStore.Instance.Save(context);
                quit = true;
            }

            // Frame rate control
            Thread.Sleep(FrameDelayMs);
        }
    }

    private static void WrapState(GameState state, ExecutionContext context)
    {
        context.Data["mario"] = state.Mario;
        context.Data["level"] = state.Level;
        context.Data["enemies"] = state.Enemies;
        context.Data["physics"] = state.Physics;
        context.Data["input_intent"] = state.InputIntent;
        context.Data["score"] = state.Score;
        context.Data["lives"] = state.Lives;
        context.Data["coins"] = state.Coins;
        context.Data["high_score"] = state.HighScore;
        context.Data["levels_completed"] = state.LevelsCompleted;
        context.Data["total_levels"] = state.TotalLevels;
        context.Data["level_complete"] = false;
        context.Data["game_state"] = "PLAYING";
    }

    #endregion

    #region Screens

    private static void ShowIntro()
    {
        Console.Clear();
        Console.WriteLine(@"
  ========================================================================
  |                                                                      |
  |                    SUPER  UDEO  MARIO  BROS.                         |
  |                                                                      |
  |               Powered by UDEO Expert Pipeline Framework              |
  |                                                                      |
  |     Every jump, every coin, every stomp is an auditable              |
  |     decision processed through a deterministic expert pipeline.      |
  |                                                                      |
  |     CONTROLS:                                                        |
  |       Arrow Keys / WASD  -  Move Mario                               |
  |       Space / Up          -  Jump                                    |
  |       Q / Escape          -  Quit                                    |
  |                                                                      |
  |     Experts in your pipeline:                                        |
  |       mario.input    -  Keyboard input processing                    |
  |       mario.physics  -  Gravity & jump mechanics                     |
  |       mario.move     -  Horizontal movement & collision              |
  |       mario.enemy    -  Enemy AI & combat                            |
  |       mario.interact -  Coins, blocks, power-ups                     |
  |       mario.scoring  -  Score tracking & game state                  |
  |                                                                      |
  |                Press any key to start...                              |
  |                                                                      |
  ========================================================================
");
        WriteColored($"  UDEO v3.1.0 — {ExpertRegistry.Instance.Count} experts loaded", ConsoleColor.Cyan);
        Console.ReadKey(true);
    }

    private static void ShowLevelIntro(GameState state)
    {
        Console.Clear();
        Console.WriteLine($@"
  ========================================================================
  |                                                                      |
  |                       WORLD  1-{state.LevelNum}
  |                                                                      |
  |                   MARIO  x  {state.Lives}
  |                                                                      |
  |                Press any key to begin...                              |
  |                                                                      |
  ========================================================================
");
        Console.ReadKey(true);
    }

    private static void ShowGameOver(GameState state, ExecutionContext context)
    {
        Console.Clear();
        Console.WriteLine($@"
  ========================================================================
  |                                                                      |
  |                         G A M E   O V E R                            |
  |                                                                      |
  |                   Final Score: {state.Score}
  |                   High Score:  {state.HighScore}
  |                                                                      |
  ========================================================================
");
        UdeoStore.Instance.Save(context);
        WriteColored("  Decision trace saved to .udeo/store/", ConsoleColor.DarkGray);
        Console.WriteLine("\n  Press any key to exit...");
        Console.ReadKey(true);
    }

    private static void ShowLevelComplete(GameState state)
    {
        Console.Clear();
        Console.WriteLine($@"
  ========================================================================
  |                                                                      |
  |                    L E V E L   C O M P L E T E                       |
  |                                                                      |
  |                   Score: {state.Score}
  |                   Lives: {state.Lives}
  |                                                                      |
  ========================================================================
");
        Thread.Sleep(2000);
    }

    private static void ShowVictory(GameState state, ExecutionContext context)
    {
        Console.Clear();
        Console.WriteLine($@"
  ========================================================================
  |                                                                      |
  |              ***   C O N G R A T U L A T I O N S   ***               |
  |                                                                      |
  |                   YOU SAVED THE MUSHROOM KINGDOM!                    |
  |                                                                      |
  |                   Final Score: {state.Score}
  |                   Lives Remaining: {state.Lives}
  |                                                                      |
  |          Thank you for playing SUPER UDEO MARIO!                     |
  |          All decisions processed by UDEO Expert Pipeline             |
  |                                                                      |
  ========================================================================
");
        if (state.PipelineResult?.Trace?.Count > 0)
        {
            WriteColored("  Final Decision Trace:", ConsoleColor.Cyan);
            foreach (var t in state.PipelineResult.Trace)
                Console.WriteLine($"    {t.ExpertId,-20} {t.DecisionCode,-12} {t.RuleFired}");
        }

        UdeoStore.Instance.Save(context);
        Console.WriteLine("\n  Press any key to exit...");
        Console.ReadKey(true);
    }

    #endregion

    #region Input

    private static string? ReadKey()
    {
        try
        {
            if (!Console.KeyAvailable) return null;
            var key = Console.ReadKey(true);
            return key.Key.ToString();
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Helpers

    private static bool IsSolid(char cell) => SolidChars.Contains(cell);
    private static bool IsPlatform(char cell) => PlatformChars.Contains(cell);

    private static T? GetState<T>(ExecutionContext ctx, string key) where T : class
    {
        if (ctx.Data.TryGetValue(key, out var val) && val is T typed)
            return typed;
        return null;
    }

    private static int GetState<T>(ExecutionContext ctx, string key) where T : struct
    {
        if (ctx.Data.TryGetValue(key, out var val) && val is int intVal)
            return intVal;
        if (val != null && int.TryParse(val.ToString(), out var parsed))
            return parsed;
        return default;
    }

    private static void WriteColored(string text, ConsoleColor foreground, ConsoleColor background = ConsoleColor.Black)
    {
        var origFg = Console.ForegroundColor;
        var origBg = Console.BackgroundColor;
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = background;
        Console.WriteLine(text);
        Console.ForegroundColor = origFg;
        Console.BackgroundColor = origBg;
    }

    private static void WriteChar(char c, ConsoleColor color)
    {
        var orig = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(c);
        Console.ForegroundColor = orig;
    }

    #endregion

    #region Entry Point

    public static void Main(string[] args)
    {
        // Bootstrap UDEO
        Bootstrap();

        // Register mario experts
        RegisterMarioExperts();

        // Suppress logging for clean display
        UdeoLogger.Instance.Configure(quiet: true);

        // Show intro
        if (!args.Contains("--no-intro") && !args.Contains("-n"))
        {
            try { ShowIntro(); }
            catch (InvalidOperationException) { } // Non-interactive console
        }

        // Init and run
        var state = InitGameState(1);
        GameLoop(state);

        // Restore logging
        UdeoLogger.Instance.Configure(quiet: false);
    }

    private static void Bootstrap()
    {
        var workspace = Environment.GetEnvironmentVariable("UDEO_WORKSPACE")
                        ?? Directory.GetCurrentDirectory();
        Environment.SetEnvironmentVariable("UDEO_WORKSPACE", workspace);

        UdeoConfig.Instance.Reload();
        UdeoLogger.Instance.Configure(
            UdeoConfig.Instance.GetString("logLevel", "Error"),
            Path.Combine(workspace, ".udeo", "udeo.log"),
            UdeoConfig.Instance.GetBool("quiet", true));

        UdeoStore.Instance.Initialize(Path.Combine(workspace, ".udeo", "store"));

        // Register built-in experts needed
        ValidationExpert.Register();
        MathExpert.Register();
        RiskExpert.Register();
        HumanReviewExpert.Register();
    }

    #endregion
}
