using System.Windows.Forms;

namespace GameCSharp.Controller;

public sealed class InputController
{
    private bool restartRequested;

    public bool MoveLeft { get; private set; }

    public bool MoveRight { get; private set; }

    public bool Fire { get; private set; }

    public void HandleKeyDown(KeyEventArgs eventArgs)
    {
        switch (eventArgs.KeyCode)
        {
            case Keys.A:
            case Keys.Left:
                MoveLeft = true;
                break;
            case Keys.D:
            case Keys.Right:
                MoveRight = true;
                break;
            case Keys.Space:
                Fire = true;
                break;
            case Keys.R:
                restartRequested = true;
                break;
        }
    }

    public void HandleKeyUp(KeyEventArgs eventArgs)
    {
        switch (eventArgs.KeyCode)
        {
            case Keys.A:
            case Keys.Left:
                MoveLeft = false;
                break;
            case Keys.D:
            case Keys.Right:
                MoveRight = false;
                break;
            case Keys.Space:
                Fire = false;
                break;
        }
    }

    public bool ConsumeRestartRequested()
    {
        var shouldRestart = restartRequested;
        restartRequested = false;
        return shouldRestart;
    }
}