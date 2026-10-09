# Sprintin

基于Unity的键盘音游，雏形出自我学习pygame时堆的屎山代码音游，现使用这个玩法进行Unity的初学，在ai帮助下火速出品。

## 玩法

- **4 轨下落式音符**：D / F / J / K 分别对应四条轨道
- **Tap**：普通单点音符
- **Hold**：长按音符，需按住并在尾部后松手
- **Ground**：地面障碍物，通过空格跳跃躲避
- 跳跃高度和滞空时间随BPM自适应，支持变BPM、变流速、谱面伸缩等特效
- 推荐使用左手中指D、左手食指F、右手食指J、右手中指K、双拇指空格的姿势游玩

## 游戏操作

- **标题页面**：按下esc退出，其他按键进入选曲
- **选曲页面**：按下esc返回标题页面，按下tab进入设置，W/S切换歌曲，A/D切换曲包，Q/E切换难度，enter选中并开始游玩
- **设置页面**：按下esc保存并返回选曲页面，W/S切换设置项，A/D变更当前设置
- **游玩页面**：DFJK控制4条轨道，空格跳跃躲避障碍，长按R重新开始，长按Esc返回选曲页面
- **结算页面**：按下Esc返回选曲页面，按下R重新开始
- 详见画面底部按键提示

## 技术要点

本部分内容由ai总结：
- **音频同步**：基于 `AudioSettings.dspTime` 实时计算 `ElapsedMs`，不使用帧时间累加避免误差。音乐通过 `PlayScheduled` 精确调度播放，支持用户偏移、谱面偏移叠加，对跳过开头做交叉淡入渐强
- **延迟校准**：DSP 时钟驱动的校准面板——定时播放提示音 → 用户拍空格 → 测量 `pressTime - targetTime` 得到偏差，采集多轮后剔除一个最大极端值取均值，同时支持手动微调，存入 `PlayerPrefs`
- **谱面变速与伸缩**：`SpeedTimeline` 支持任意时间点流速切换，音符 z 坐标通过分段积分统一计算位置。`stretch` 拉伸特效在指定时段以线性 / 缓入（si）/ 缓出（so）改变流速，Hold 长度也随变速和伸缩实时调整
- **BPM 自适应跳跃状态机**：闲置→上升→悬停/冲突→下落 状态机，用抛物线模拟物理曲线。滞空时长由 BPM 设定动态变化，高 BPM 时冲突态取上升下落较小值避免视觉违和，提前着地窗口内允许连跳
- **三级判定与计分**：Perfect（50ms）/ Great（100ms）/ Bad（150ms），Hold 头尾独立判定，尾部有 100ms 提前松手容限。分值权重 Perfect=3 / Great=2 / Bad=1，归一化为 1000000 满分制。同毫秒多押自动标记 `isDual` 并切换高亮材质，判定结果 UI 附带渐大→缩回→淡出动画与颜色区分，连击数 ≥10 才显示
- **场景转场**：`DontDestroyOnLoad` 常驻的 `SceneTransitionManager`，实现 Outro（面板从上方滑入）→ Loading（异步加载场景）→ Intro（面板向下方滑出）状态机。支持封面的异步缓存就绪等待再触发 Intro，冷启动首次加载时绕过正常流转逻辑
- **键盘输入系统**：`MenuInputManager` 基于 `Keyboard.current` 直接轮询，非 Unity Input System Actions。全键位绑定：WASD 四方向选择、Enter 确认、Esc 返回、Tab 设置、C 校准、Space 校准拍击、Q/E 切换难度。方向键按住支持长按加速——初始触发后进入慢速重复 → 持续按住后进入快速重复，松开时复位
- **选曲系统**：JSON 三层曲库（`root.json` → `pack.json` → `song.json`），`ScrollRect` + Lerp 平滑吸附滚动，选中项放大、非选中项缩小。歌曲条目显示封面缩略图与难度指示器。封面在选曲面板打开展示前异步预加载到字典缓存，切换时直接取缓存无闪烁。音频预览支持 streaming 流式加载，按 `viewbegin`/`viewend` 切段循环播放，带淡入淡出和循环间隔
- **标题与面板动画**：`TitlePanelAnimation` 六阶段序列：FadeIn → SlideIn（easeOutQuad 标题/地面 parallax 分离下滑）→ Idle → SlideOut（easeInQuad）→ WaitingForFadeOut → FadeOut。`KeyHintAnimation` 正弦波呼吸闪烁，任意按键触发渐变消失。菜单面板切换通过 `TransitionPanelAnimation` 统一转场
- **成绩持久化**：`RecordManager` 以 JSON 文件（`records.json`）存储三层结构：曲包 → 歌曲 → 难度，记录最高分、最大连击、全连标记。结算时自动更新覆盖
- **设置系统**：`Option` 抽象基类 + `EnumOption` / `NumericOption` 派生类，设置项带 `persistKey`、默认值、范围，由 `OptionListManager` 在面板打开时自动加载，面板关闭时自动保存。选项列表为吸附滚动 + `MenuInputManager` 控制
- **自定义谱面格式**：`.spr` 文本格式，五遍 `Tokenizer` 解析：收集 token → 按时间分组 → 解析 effect（speed/stretch/text）→ 标记多押 → 追加 BPM 默认值。支持带引号字符串值。谱面元素：BPM（小节线 BPM 与跳跃 BPM 分离）、Tap、Hold、Ground、Speed、Stretch、Text。小节线按 `beatsPerBar` 从 BPM 变化点前后自动推算生成并去重
- **背景对象池**：`EnvironmentManager` 按 `pieceCount` 预实例化多种块，按 `weightedPrefab` 权重随机选取。循环滚动复用——超出可见范围回收，池空时动态扩容。提前 `visibleBuffer` 距离生成，按 `_environmentDistance` 统一滚动
- **游玩 UI**：显示歌曲封面 + 曲名 + 艺术家（运行时实时读取 `song.json`），难度缩写（EZ/NM/HD/RS）和数值，封面背景色按难度变化（绿/黄/红/紫）。谱面内置 `text` 效果支持淡入→保持→淡出自定义文字。Ground 音符在时序上增加 25ms 视觉延迟改善手感。中文字体使用思源黑体 SDF + 3500 常用字字表


## 谱面格式（.spr）

```
@bpm=120.0;
@offset=0;

(0, 120.0, 4.0);           # BPM 变化: ms, bpm, beatsPerBar （其中beatsPerBar为0时变化跳跃bpm，其余为变化小节线bpm）
[1000, 2];                  # Tap: ms, key(1-4)
[2000, 1, 4000];            # Hold: ms, key, endMs
[3000, 0];                  # Ground: ms, key=0
{5000, "speed", 2.0};         # 谱面变速: ms, "speed", multiplier
{5500, "stretch", 5750, 1.5, si};       # 谱面伸缩: ms, "stretch", endms, multiplier, s|si|so
{6000, "text", 500, 2000, 500, "这是一句话"};  # 文字特效: ms, "text", fadein, duration, fadeout, "<content>"
```

## 其他

用 Unity 打开项目后打开 `MenuScene` 场景即可运行

内置一首教程音乐，初次打开游戏应该会直接进入。美术风格比较炸裂，不会设计……

AI时代冲击太大了，这个游戏我到现在两个月，从不会unity做到我觉得能玩并且我觉得还挺好玩的地步，说慢那我觉得学得很快，说快那好像也只是个24h的水平。于是我根本不知道自己的能力在什么档次，有多大竞争力。甚至不知道要不要学这个，要学其他的啥，或许只是自我安慰的手段，烦。