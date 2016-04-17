namespace NeoMapleStory.Packet
{
    public class RecvOpcodes
    {
        public static readonly short
// GENERAL---
Pong = 0x13,
ClientError = 0xFF,
// 异常数据---
StrangeData = 0x15,

// LOGIN*
LoginPassword = 0x01,
ServerlistRerequest = 0xFF,
// 请求服务器列表
ServerlistRequest = 0x02,
// 许可协议回复
LicenseRequest = 0x03,
// 选择性别
SetGender = 0x04,
// 服务器状态
ServerstatusRequest = 0x05,
// 请求人物列表
CharlistRequest = 0x09,
AfterLogin = 0xFF,
RegisterPin = 0xFF,
ToWorldlist = 0xFF,
ViewAllCharRequest = 0xFF,
ViewAllCharConnect = 0xFF,
ViewAllChar = 0xFF,
// 开始游戏
CharSelect = 0x0A,
// 检查人物名字
CheckCharName = 0x0C,
// 创建人物
CreateChar = 0x11,
// 错误日志
ErrorLog = 0x14,
// 日志
Relog = 0x19,

// CHANNEL
// 登陆请求
PlayerLoggedin = 0x0B,
// 更换地图
ChangeMap = 0x21,
// 更换频道
ChangeChannel = 0x22,
// 进入商城
EnterCashShop = 0x23,
// 人物移动
MovePlayer = 0x24,
// 取消椅子
CancelChair = 0x25,
// 使用椅子
UseChair = 0x26,
// 近距离攻击
CloseRangeAttack = 0x28,
// 远距离攻击
RangedAttack = 0x29,
// 魔法攻击
MagicAttack = 0x2A,
// 能量攻击
PassiveEnergy = 0x2B,
//
EnergyChargeAttack = 0xFF,
// 获取伤害
TakeDamage = 0x2C,
// 普通聊天
GeneralChat = 0x2D,
// 关闭黑板
CloseChalkboard = 0x2E,
// 人物面部表情
FaceExpression = 0x2F,
// 使用物品效果
UseItemeffect = (short)(FaceExpression + 1),
// ---
ReviveItem = 0x31,
// 怪物卡片
MonsterBookCover = 0x35,
// NPC交谈
NpcTalk = 0x36,
// NPC详细交谈
NpcTalkMore = 0x38,
// NPC商店
NpcShop = 0x3A,
// 仓库
Storage = 0x3B,
// 雇佣商店
HiredMerchantRequest = 0x3C,
// 送货员
DueyAction = 0x3E,
// 物品整理
ItemSort = 0x42,
// 物品排序
ItemSort2 = 0x43,
// 物品移动
ItemMove = 0x44,
// 使用物品
UseItem = 0x45,
// 取消物品结果
CancelItemEffect = 0x46,
// 使用召唤包
UseSummonBag = 0x48,
// 宠物食品
PetFood = 0x49,
// 坐骑食品
UseMountFood = 0x4A,
// ---
ScriptedItem = 0x4B,
// 使用现金物品
UseCashItem = 0x4C,
// 使用扑捉物品
UseCatchItem = 0x4D,
// 使用技能书
UseSkillBook = 0x4F,
// 使用回城卷
UseReturnScroll = 0x52,
// 使用砸卷
UseUpgradeScroll = 0x53,


// 分发能力点
DistributeAp = 0x54,
// 自动分发能力点
DistributeAutoAp = 0x55,
// 自动回复HP/MP
HealOverTime = 0x56,
// 分发技能点
DistributeSp = 0x57,
// 特殊移动
SpecialMove = 0x58,
// 取消增益效果
CancelBuff = 0x59,
// 技能效果
SkillEffect = 0x5A,
// 金币掉落
MesoDrop = 0x5B,
// 给人气
GiveFame = 0x5C,
// 返回人物信息
CharInfoRequest = 0x5E,
// 召唤宠物
SpawnPet = 0x5F,
// 取消负面效果
CancelDebuff = 0x60,
// 特殊地图移动---
ChangeMapSpecial = 0x61,
// 使用时空门---
UseInnerPortal = 0x62,
// 缩地石---
TrockAddMap = 0x63,
// 技能宏---
SkillMacro = 0x66,
// 钓鱼
UseFishingItem = 0x67,
// 任务动作---
QuestAction = 0x68,
// 效果开关---
EffectOnOff = 0x69,
//
ThrowBomb = 0xFF,
// 报告玩家
ReportPlayer = 0xFF,
// 锻造技能
MakerSkill = 0x71,
// 组队/家族聊天
MultiChat = 0x74,
// 悄悄话
Whisper = 0x75,
// 聊天招待
Messenger = 0x76,
//
SpouseChat = 0xFF,
// 玩家互动
PlayerInteraction = 0x77,
// 开设组队
PartyOperation = 0x78,
// 拒绝组队邀请
DenyPartyRequest = 0x79,
// 开设家族
GuildOperation = 0x7A,
// 拒绝家族邀请
DenyGuildRequest = 0x7B,
// 好友操作
BuddylistModify = 0x7E,
// 小纸条
NoteAction = 0x7F,
// 使用门
UseDoor = 0x81,
// 改变键盘布局
ChangeKeymap = 0x83,
// 戒指
RingAction = 0x85,
// 家族联盟
AllianceOperation = 0x89,
// 家族BBS
BbsOperation = 0xFF,
// 进入拍卖
EnterMts = 0x8D,
//
Solomon = 0xFF,
// 打开学院
OpenFamily = 0x91,
// 添加学院
AddFamily = 0x92,
AcceptFamily = 0x93,
UseFamily = 0x94,



// 召唤兽说话
SummonTalk = 0x9E,
// 战神伤害
MobDamaged = 0x9F,






// 宠物移动
MovePet = 0xA5,
// 宠物自动吃药
PetAutoPot = 0x90,
// 宠物说话
PetChat = 0xA6,
// 宠物命令
PetCommand = 0xA7,
// 宠物拣取
PetLoot = 0xA8,


// 召唤兽移动---
MoveSummon = 0xAD,
// 召唤兽动作---
SummonAttack = 0xAE,
// 召唤兽伤害---
DamageSummon = 0xB0,

// 怪物移动
MoveLife = 0xB7,
// 自动攻击
AutoAggro = 0xB8,
// 
MobDamageMob = 0xBD,
// 怪物炸弹
MonsterBomb = 0xBC,


// NPC说话
NpcAction = 0xC0,
// 物品拣起
ItemPickup = 0xC6,
// 伤害反映
DamageReactor = 0xC9,
// 碰触反映
TouchReactor = 0xCA,
//
Hypnotize = 0xFF,
// 怪物嘉年华
MonsterCarnival = 0xFF,
// 地图状态
ObjectRequest = 0xD7,
// 组队搜索请求
PartySearchRegister = 0xD9,
PartySearchStart = 0xDA,

// 人物数据更新
PlayerUpdate = 0xE0,
// 金锤子
ViciousHammer = 0xFF,
// 点卷确认
TouchingCs = 0xE8,
// 购买物品
CashShop = 0xE9,
// 使用兑换券
CouponCode = 0xEA,
// 冒险岛TV
Mapletv = 0xFF,
// 拍卖系统
MtsOp = 0xFB,

// 聊天系统
ChatRoomSystem = 0x104;
    }
}
