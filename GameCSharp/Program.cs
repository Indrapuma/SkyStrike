using Raylib_cs;
using GameCSharp.Controller;
using GameCSharp.Model;
using GameCSharp.RL;
using GameCSharp.View;

namespace GameCSharp;

internal static class Program
{
	private static void Main(string[] args)
	{
		const int initialWidth = 960;
		const int initialHeight = 720;

		var inferenceMode = args.Any(argument => string.Equals(argument, "--inference", StringComparison.OrdinalIgnoreCase));

		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
		Raylib.InitWindow(initialWidth, initialHeight, "SkyStrike MVC");
		Raylib.SetTargetFPS(60);

		var gameState = new GameState(initialWidth, initialHeight);
		var inputController = new InputController();
		var gameEngine = new GameEngine(gameState, inputController);
		var trainer = new RlTrainer(gameState, gameEngine, inputController);
		var renderer = new GameRenderer(initialWidth, initialHeight);

		if (inferenceMode)
		{
			trainer.SetInferenceEnabled(true);
			if (!trainer.HasLearnedPolicy)
			{
				Console.WriteLine($"No saved Q-table found at '{trainer.ModelPath}'. Inference mode will idle until a model is trained.");
			}
		}

		try
		{
			while (!Raylib.WindowShouldClose())
			{
				if (Raylib.IsWindowResized())
				{
					var width = Math.Max(320, Raylib.GetScreenWidth());
					var height = Math.Max(240, Raylib.GetScreenHeight());
					gameState.Resize(width, height);
					renderer.Resize(width, height);
				}

				inputController.Poll();
				if (!trainer.InferenceEnabled && inputController.ConsumeToggleTrainingRequested())
				{
					trainer.ToggleEnabled();
				}

				if (!trainer.InferenceEnabled && inputController.ConsumeIncreaseTrainingSpeedRequested())
				{
					trainer.IncreaseSimulationSpeed();
				}

				if (!trainer.InferenceEnabled && inputController.ConsumeDecreaseTrainingSpeedRequested())
				{
					trainer.DecreaseSimulationSpeed();
				}

				var frameTime = Raylib.GetFrameTime();
				if (trainer.InferenceEnabled)
				{
					trainer.UpdateInference(frameTime);
				}
				else if (trainer.Enabled)
				{
					trainer.Update(frameTime);
				}
				else
				{
					gameEngine.Update(frameTime);
				}

				Raylib.BeginDrawing();
				renderer.Render(gameState, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), trainer.GetMonitorSnapshot());
				Raylib.EndDrawing();
			}
		}
		finally
		{
			trainer.FlushArtifacts();
			Raylib.CloseWindow();
		}
	}
}
