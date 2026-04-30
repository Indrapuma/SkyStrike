using Raylib_cs;
using GameCSharp.RL;

namespace GameCSharp.Controller;

public sealed class InputController
{
    private bool manualMoveLeft;
    private bool manualMoveRight;
    private bool manualFire;
    private bool agentMoveLeft;
    private bool agentMoveRight;
    private bool agentFire;
    private bool restartRequested;
    private bool toggleTrainingRequested;
    private bool increaseTrainingSpeedRequested;
    private bool decreaseTrainingSpeedRequested;

    public bool UseAgentControl { get; private set; }

    public bool MoveLeft => UseAgentControl ? agentMoveLeft : manualMoveLeft;

    public bool MoveRight => UseAgentControl ? agentMoveRight : manualMoveRight;

    public bool Fire => UseAgentControl ? agentFire : manualFire;

    public void Poll()
    {
        manualMoveLeft = Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left);
        manualMoveRight = Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right);
        manualFire = Raylib.IsKeyDown(KeyboardKey.Space);

        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            restartRequested = true;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.T))
        {
            toggleTrainingRequested = true;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.RightBracket) || Raylib.IsKeyPressed(KeyboardKey.Equal) || Raylib.IsKeyPressed(KeyboardKey.KpAdd))
        {
            increaseTrainingSpeedRequested = true;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.LeftBracket) || Raylib.IsKeyPressed(KeyboardKey.Minus) || Raylib.IsKeyPressed(KeyboardKey.KpSubtract))
        {
            decreaseTrainingSpeedRequested = true;
        }
    }

    public void SetAgentControl(bool enabled)
    {
        UseAgentControl = enabled;
        if (!enabled)
        {
            ClearAgentAction();
        }
    }

    public void ApplyAgentAction(RlAction action)
    {
        agentMoveLeft = action is RlAction.MoveLeft or RlAction.MoveLeftShoot;
        agentMoveRight = action is RlAction.MoveRight or RlAction.MoveRightShoot;
        agentFire = action is RlAction.Shoot or RlAction.MoveLeftShoot or RlAction.MoveRightShoot;
    }

    public void ClearAgentAction()
    {
        agentMoveLeft = false;
        agentMoveRight = false;
        agentFire = false;
    }

    public bool ConsumeRestartRequested()
    {
        var shouldRestart = restartRequested;
        restartRequested = false;
        return shouldRestart;
    }

    public bool ConsumeToggleTrainingRequested()
    {
        var shouldToggle = toggleTrainingRequested;
        toggleTrainingRequested = false;
        return shouldToggle;
    }

    public bool ConsumeIncreaseTrainingSpeedRequested()
    {
        var shouldIncrease = increaseTrainingSpeedRequested;
        increaseTrainingSpeedRequested = false;
        return shouldIncrease;
    }

    public bool ConsumeDecreaseTrainingSpeedRequested()
    {
        var shouldDecrease = decreaseTrainingSpeedRequested;
        decreaseTrainingSpeedRequested = false;
        return shouldDecrease;
    }
}