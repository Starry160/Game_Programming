# Interactive Solar System 作业快速完成指南

下面这套脚本已经覆盖 rubric 的核心要求：点击交互、镜头切换、视觉/音效反馈、返回主视角、轨道运动。

## 1) 场景结构建议

- `Sun`（太阳）
- `Earth`（至少一个行星）
- `Moon`（至少一个月亮）
- `Main Camera`
- `Directional Light`
- `GameManager`（空物体，挂管理脚本）
- `Canvas`（信息面板和返回按钮）

## 2) 挂载脚本

- 给 `Earth`、`Moon` 挂 `CelestialSelectable`
  - 填 `displayName`
  - 填 `kidFact`（儿童友好文案）
  - 赋值 `targetRenderer`
  - 可选：填 `clickSound`
- 给 `Earth`、`Moon` 挂 `OrbitMotion`
  - `orbitCenter` 分别指向 `Sun` 或 `Earth`
- 给 `Main Camera` 挂 `SolarCameraController`
- 给 `Canvas` 挂 `SolarUIController`
  - 绑定 `infoPanel`、`titleText`、`factText`、`hintText`
- 给 `GameManager` 挂 `SolarInteractionManager`
  - 绑定 `cameraController`、`uiController`、`audioSource`

## 3) UI 最小配置

- 创建 `Canvas`
- 创建一个 `Panel` 作为信息面板（初始可开着，运行后脚本会隐藏）
- 面板里放两个 `Text`：
  - 标题（星体名）
  - 科普短句（儿童语言）
- 创建一个 `Button` 文本为 `Return`
  - `OnClick()` 绑定到 `GameManager -> SolarInteractionManager.ReturnToOverviewByButton`
- 创建一个 `Text` 作为提示：`Press ESC or tap Return to overview.`

## 4) 演示流程（答辩 1 分钟）

1. 播放场景，展示太阳-地球-月球在旋转/公转。
2. 点击地球：镜头拉近，出现科普文案，出现闪光/缩放反馈，可有点击音效。
3. 点击月球：同样反馈。
4. 按 `ESC` 或点 `Return` 回到主视角。
5. 说明你用了哪些课堂概念：`materials + lighting + behaviors + audio + cameras`（至少四个）。

## 5) 对照 rubric 勾选

- [x] 行星和月球可点击，响应清晰
- [x] 相机聚焦切换，并能返回主视角
- [x] 至少一个视觉或音频反馈（这里两者都支持）
- [x] 包含旋转/公转行为
- [x] 使用四个以上课堂概念
- [x] 儿童友好短文本

## 6) 常见问题

- 点击没反应：
  - 目标物体必须有 Collider（SphereCollider/BoxCollider）
  - 主相机要能看到对象
- 文本不显示：
  - 检查 `SolarUIController` 的引用是否拖拽完整
- 镜头不回去：
  - 检查 `SolarInteractionManager` 是否存在且 `cameraController` 已绑定

