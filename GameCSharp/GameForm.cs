using System.Drawing;
using System.Windows.Forms;
using GameCSharp.Controller;
using GameCSharp.Model;
using GameCSharp.View;

namespace GameCSharp;

public sealed class GameForm : Form
{
    private readonly GameState gameState;
    private readonly InputController inputController;
    private readonly GameEngine gameEngine;
    private readonly GameRenderer renderer;

    public GameForm()
    {
        Text = "SkyStrike MVC";
        ClientSize = new Size(960, 720);
        MinimumSize = new Size(640, 480);
        BackColor = Color.Black;
        DoubleBuffered = true;
        KeyPreview = true;
        StartPosition = FormStartPosition.CenterScreen;

        gameState = new GameState(ClientSize.Width, ClientSize.Height);
        inputController = new InputController();
        renderer = new GameRenderer(ClientSize.Width, ClientSize.Height);
        gameEngine = new GameEngine(gameState, inputController, RequestFrame);

        KeyDown += (_, eventArgs) => inputController.HandleKeyDown(eventArgs);
        KeyUp += (_, eventArgs) => inputController.HandleKeyUp(eventArgs);
        Resize += (_, _) => HandleResize();
        FormClosed += (_, _) => gameEngine.Dispose();

        gameEngine.Start();
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        base.OnPaint(eventArgs);
        renderer.Render(eventArgs.Graphics, gameState, ClientSize);
    }

    private void HandleResize()
    {
        gameState.Resize(ClientSize.Width, ClientSize.Height);
        renderer.Resize(ClientSize.Width, ClientSize.Height);
        Invalidate();
    }

    private void RequestFrame()
    {
        if (!IsDisposed)
        {
            Invalidate();
        }
    }
}