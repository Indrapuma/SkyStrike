from __future__ import annotations

import csv
from pathlib import Path

import matplotlib.pyplot as plt


REPO_ROOT = Path(__file__).resolve().parent.parent
CSV_PATH = REPO_ROOT / "artifacts" / "rl" / "episode_metrics.csv"
PLOT_PATH = REPO_ROOT / "artifacts" / "rl" / "episode_metrics_plot.png"


def load_rows(csv_path: Path) -> dict[str, list[float]]:
    if not csv_path.exists():
        raise FileNotFoundError(f"Metrics CSV not found: {csv_path}")

    columns: dict[str, list[float]] = {
        "episode": [],
        "reward": [],
        "average_reward": [],
        "score": [],
        "epsilon": [],
        "q_entries": [],
    }

    with csv_path.open("r", encoding="utf-8", newline="") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            columns["episode"].append(float(row["episode"]))
            columns["reward"].append(float(row["reward"]))
            columns["average_reward"].append(float(row["average_reward"]))
            columns["score"].append(float(row["score"]))
            columns["epsilon"].append(float(row["epsilon"]))
            columns["q_entries"].append(float(row["q_entries"]))

    return columns


def main() -> None:
    metrics = load_rows(CSV_PATH)
    episodes = metrics["episode"]

    plt.style.use("seaborn-v0_8-darkgrid")
    fig, axes = plt.subplots(2, 2, figsize=(14, 9), constrained_layout=True)
    fig.suptitle("SkyStrike RL Training Metrics", fontsize=18, fontweight="bold")

    reward_ax = axes[0, 0]
    reward_ax.plot(episodes, metrics["reward"], label="Episode Reward", color="#f59e0b", linewidth=1.6)
    reward_ax.plot(episodes, metrics["average_reward"], label="Average Reward", color="#2563eb", linewidth=2.0)
    reward_ax.set_title("Reward Progress")
    reward_ax.set_xlabel("Episode")
    reward_ax.set_ylabel("Reward")
    reward_ax.legend()

    score_ax = axes[0, 1]
    score_ax.plot(episodes, metrics["score"], color="#16a34a", linewidth=1.8)
    score_ax.set_title("Score Per Episode")
    score_ax.set_xlabel("Episode")
    score_ax.set_ylabel("Score")

    epsilon_ax = axes[1, 0]
    epsilon_ax.plot(episodes, metrics["epsilon"], color="#7c3aed", linewidth=1.8)
    epsilon_ax.set_title("Exploration Rate")
    epsilon_ax.set_xlabel("Episode")
    epsilon_ax.set_ylabel("Epsilon")

    q_ax = axes[1, 1]
    q_ax.plot(episodes, metrics["q_entries"], color="#dc2626", linewidth=1.8)
    q_ax.set_title("Q-Table Size")
    q_ax.set_xlabel("Episode")
    q_ax.set_ylabel("Entries")

    PLOT_PATH.parent.mkdir(parents=True, exist_ok=True)
    fig.savefig(PLOT_PATH, dpi=150)
    plt.close(fig)

    print(f"Saved plot to: {PLOT_PATH}")


if __name__ == "__main__":
    main()