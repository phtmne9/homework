import cv2
import numpy as np

WIDTH, HEIGHT, FPS, SECONDS = 480, 320, 20, 6


def build(path="sample.mp4"):
    writer = cv2.VideoWriter(path, cv2.VideoWriter_fourcc(*"mp4v"), FPS, (WIDTH, HEIGHT))
    total = FPS * SECONDS

    for i in range(total):
        frame = np.full((HEIGHT, WIDTH, 3), 230, dtype=np.uint8)
        cv2.rectangle(frame, (0, HEIGHT - 60), (WIDTH, HEIGHT), (190, 190, 190), -1)

        x = int(40 + (WIDTH - 120) * (i / total))
        cv2.rectangle(frame, (x, 180), (x + 70, 250), (60, 90, 200), -1)

        angle = 2 * np.pi * i / FPS
        cx = int(WIDTH / 2 + 110 * np.cos(angle))
        cy = int(110 + 50 * np.sin(angle))
        cv2.circle(frame, (cx, cy), 26, (70, 160, 70), -1)

        if i > total // 2:
            cv2.rectangle(frame, (30, 40), (110, 110), (40, 40, 40), -1)

        writer.write(frame)

    writer.release()

    return path, total


if __name__ == "__main__":
    path, total = build()
    print(f"создано видео {path}: {total} кадров, {FPS} fps, {WIDTH}x{HEIGHT}")
