
# -*- coding: utf-8 -*-
"""生成 developer/app 的代码结构思维导图 PNG。
改完代码后重跑：python3 tools/gen_mindmap.py
"""
import os
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "..", "代码结构思维导图.png")

FONT = next(p for p in [
    "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
    "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc",
    "/usr/share/fonts/truetype/arphic/uming.ttc",
] if os.path.exists(p))
_cache = {}
def font(sz):
    if sz not in _cache: _cache[sz] = ImageFont.truetype(FONT, sz)
    return _cache[sz]

TREE = ("developer/app · 贪吃蛇", [
 ("① 入口  program.cs", [
   ("GameSettingLoader.Load()", [("读 Config/GameSettings.json", []), ("JsonSerializer → GameSettings", [])]),
   ("new GameState()", [("GameRunningState = GameOn", [])]),
   ("GameRegisterSystem.UseRegister()", [
     ("Register() → Task.Run", [("PlayerRegisterInterface\n.PrintPlayerRegisterInterface()", [
        ("new Player(id) → SetName", []), ("PlayerLists.Add / CurrentPlayer", [])])]),
     ("打印 登陆成功 / 失败", [])]),
   ("HomeScreen.ScreenFuncSystem", [("new GamePrepareArgs(settings)", [
       ("new GameWorld(settings)", [("BuildWalls / FillFood", [])]),
       ("GameWorld.AddPlayer(account)", []),
       ("new KeyBoardController(Button, P1, Wasd, E)", []),
       ("new QuitController(Button, world)", []),
       ("new CollisionController(Collision)", [])])]),
 ]),
 ("② 界面 / 流程\nInterface/Game.Screen", [
   ("HomeScreen", [("WaitKey() → Console.ReadKey", []),
     ("while hstate == Choosegamemode", [("1 → GameStart.ChooseGameMode", []), ("2 → GameSetMenu.SetMenu", []), ("3 → GameExitGame.ExitGame", [])])]),
   ("GameStart.ChooseGameMode(args)", [
     ("args.BeginRound()", [("FuncSystem.GameRunSystemAsync  单人", []), ("LocalDoubleSystem.GameRunSystemAsync  双人", [])]),
     ("args.EndRound() → 解绑事件", [])]),
   ("GamePrepareArgs", [
     ("PumpInput()  两个模式共用", [("Button.TouchClick(key)", [("事件 → KeyBoardController ×N", []), ("事件 → QuitController", [])])]),
     ("BeginRound / EndRound", []),
     ("EnableGuestInput(P2)  仅双人", [])]),
   ("GameSetMenu.SetMenu(args)", [("ChangeMapSize → GameWorld.Resize", []), ("ChangeSpeed → TickMilliseconds", []),
     ("ChangeDuration → SetTime.Apply", []), ("Rename → Player.SetName", [])]),
   ("GameExitGame.ExitGame", [("DoExitGame → 是 → GameOff", [])]),
   ("RoomPlayerLists（0.8 雏形）", [("AddPlayer / IsFull", [])]),
 ]),
 ("③ 玩法循环  Services", [
   ("FuncSystem（单人）", [("args.PumpInput()", []), ("LocalInput.Direction", []),
     ("Skills.MoveMode / Collision.TouchCollision", []), ("DefeatSystem.Result", [])]),
   ("LocalDoubleSystem（本机双人）", [("args.PumpInput()", []),
     ("LocalInput.Direction / GuestInput.Direction", []),
     ("Skills.MoveMode ×2 → EatFood ×2", [("FoodAt / RemoveFood\nCallAddScore / GenerateFood", [])]),
     ("IsDead → MarkGameOver", []), ("EndSystem → MultiRules.Winner", [])]),
   ("DefeatSystem.Result", [("得分 / 长度 / 存活 / 按键返回", [])]),
 ]),
 ("④ 世界模型  World", [
   ("GameWorld", [("AddPlayer / FindPlayer / ResetPlayers", []), ("Restart / Resize / UpdateTime / UpdateFood", []),
     ("GenerateFood / RemoveFood / FoodAt", []), ("Snapshot() → WorldSnapShot（未调用）", [])]),
   ("Player", [("MSnake / Skills / MScore / IsTouchButton", []), ("ResetForNewGame / RecordSurvivalTime", [])]),
   ("FoodItem", []), ("WorldSnapShot / PlayerSnapShot / FoodSnapShot", []), ("MapPrinter.PrintMap", []),
 ]),
 ("⑤ 组件  Models / Collision / Score / KeyBoard", [
   ("SnakeHead", [("Snake: List<Point>", []), ("Head / Tail / Nextdir", [])]),
   ("Skills", [("MoveMode → Move（冲刺 2 格）", []), ("Reset", [])]),
   ("Button（事件源）", [("TouchClick → 事件 Clicked", [])]),
   ("KeyBoardController", [("布局 Wasd / Arrows", []), ("方向 + 冲刺键 → 某个 Player", [])]),
   ("QuitController", [("Esc → world.QuitGame()", [])]),
   ("Collision → CollisionController", [("TouchCollision → 事件 CollisionWhat", []), ("撞墙/自撞 → MarkGameOver", []), ("吃食物 → CallAddScore / GenerateFood", [])]),
   ("Score → PlayerScore", [("CallAddScore → AddScore", [])]),
 ]),
 ("⑥ 配置 / 基础设施", [
   ("GameSettings", [("MapConfig / FoodConfig", []), ("NetWorkConfig（MQTT）", []), ("SkillConfig（未接线）", [])]),
   ("SetTime.Options / PrintOptions / Apply", []), ("State.Point / State.Direction", []),
   ("GameState 枚举", [("HomeScreenState / GameOnAndOffState", []), ("InterfaceState / GamingState（未用）", [])]),
   ("Template.cs（空壳）", []),
 ]),
])

PALETTE = [("#2563eb", "#eaf1ff"), ("#0891b2", "#e3f7fa"), ("#7c3aed", "#f2eaff"),
           ("#c2410c", "#fff0e6"), ("#059669", "#e6f7f0"), ("#be123c", "#ffe9ef")]
SIZES = {0: 28, 1: 20, 2: 15, 3: 13, 4: 12}
PADX, PADY, LSP = 16, 9, 5
GAP_V, GAP_X, GAP_ROOT = 10, 46, 90

class Node:
    def __init__(self, text, children, depth, color, fill):
        self.text, self.lines = text, text.split("\n")
        self.depth, self.color, self.fill = depth, color, fill
        self.sz = SIZES[min(depth, 4)]
        f = font(self.sz)
        self.w = int(max(max(f.getlength(l) for l in self.lines), max(f.getbbox(l)[2] for l in self.lines))) + PADX * 2
        self.h = (self.sz + LSP) * len(self.lines) + PADY * 2
        self.children = [Node(t, c, depth + 1, color, fill) for t, c in children]
        self.x = self.y = 0
        self.sub = 0

root = Node(TREE[0], TREE[1], 0, "#0f172a", "#0f172a")
for i, b in enumerate(root.children):
    b.color, b.fill = PALETTE[i][0], PALETTE[i][1]
    def paint(n):
        for c in n.children:
            c.color = b.color; c.fill = "#ffffff"; paint(c)
    paint(b)

left, right = root.children[:3], root.children[3:]

def measure_sub(n):
    n.sub = max(n.h, sum(measure_sub(c) for c in n.children) + GAP_V * max(0, len(n.children) - 1))
    return n.sub

acc = {}
def col_width(n, depth, acc):
    acc[depth] = max(acc.get(depth, 0), n.w)
    for c in n.children: col_width(c, depth + 1, acc)
for b in root.children: col_width(b, 1, acc)
colx, x = {}, 0
for d in sorted(acc):
    colx[d] = x; x += acc[d] + GAP_X
side_w = x

def layout_branch(n, y0, xoff):
    n.x = colx[n.depth] + xoff
    if not n.children:
        n.y = y0 + n.sub / 2; return
    cy = y0
    for c in n.children:
        layout_branch(c, cy, xoff); cy += c.sub + GAP_V
    n.y = (n.children[0].y + n.children[-1].y) / 2

def mirror(n, axis):
    n.x = axis - (n.x + n.w)
    for c in n.children: mirror(c, axis)

for b in root.children: measure_sub(b)
lh_h = sum(b.sub for b in left) + GAP_V * (len(left) - 1)
rh_h = sum(b.sub for b in right) + GAP_V * (len(right) - 1)
canvas_h = max(lh_h, rh_h)
root.x = side_w + GAP_ROOT
root.y = canvas_h / 2

cy = root.y - lh_h / 2
for b in left:
    layout_branch(b, cy, 0); mirror(b, root.x - GAP_ROOT); cy += b.sub + GAP_V
cy = root.y - rh_h / 2
for b in right:
    layout_branch(b, cy, root.x + root.w + GAP_ROOT); cy += b.sub + GAP_V

minx = miny = 1e9; maxx = maxy = -1e9
def bbox(n):
    global minx, miny, maxx, maxy
    minx = min(minx, n.x); maxx = max(maxx, n.x + n.w)
    miny = min(miny, n.y - n.h / 2); maxy = max(maxy, n.y + n.h / 2)
    for c in n.children: bbox(c)
bbox(root)

M = 30
OX, OY = M - minx, M - miny
W, H = int(maxx - minx + 2 * M), int(maxy - miny + 2 * M) + 26
img = Image.new("RGB", (W, H), "#ffffff")
d = ImageDraw.Draw(img)

def edge(n):
    for c in n.children:
        if c.x >= n.x:
            x1, y1 = n.x + n.w + OX, n.y + OY
            x2, y2 = c.x + OX, c.y + OY
            d.line([(x1, y1), (x1 + GAP_X / 2, y1), (x1 + GAP_X / 2, y2), (x2 - 2, y2)], fill=c.color, width=2)
            d.polygon([(x2, y2), (x2 - 7, y2 - 4), (x2 - 7, y2 + 4)], fill=c.color)
        else:
            x1, y1 = n.x + OX, n.y + OY
            x2, y2 = c.x + c.w + OX, c.y + OY
            d.line([(x1, y1), (x1 - GAP_X / 2, y1), (x1 - GAP_X / 2, y2), (x2 + 2, y2)], fill=c.color, width=2)
            d.polygon([(x2, y2), (x2 + 7, y2 - 4), (x2 + 7, y2 + 4)], fill=c.color)
        edge(c)

def draw_node(n):
    d.rounded_rectangle([n.x + OX, n.y + OY - n.h / 2, n.x + OX + n.w, n.y + OY + n.h / 2],
                        radius=9, fill=n.fill, outline=n.color, width=2)
    f = font(n.sz); lhgt = n.sz + LSP
    ty = n.y + OY - lhgt * len(n.lines) / 2 + 3
    for line in n.lines:
        tw = f.getlength(line)
        d.text((n.x + OX + (n.w - tw) / 2, ty), line, font=f, fill="#ffffff" if n.depth == 0 else "#0f172a")
        ty += lhgt
    for c in n.children: draw_node(c)

edge(root)
for b in root.children: draw_node(b)
draw_node(root)
d.text((M, H - 22), "生成自 developer/app 源码   箭头 = 调用 / 包含   输入统一走 Button 事件（单人 / 双人同一段）", font=font(12), fill="#64748b")
img.save(OUT)
print("saved", OUT, img.size)
