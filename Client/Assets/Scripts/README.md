# 目录简述
用于存放Unity的C#脚本

## 全局用脚本（此根目录内脚本）
文件 | 描述
--|--
`AudioPlayManager` | 音频播放管理，包括播放bgm、音效等
`GlobalManager` | 全局的管理脚本，用于一些全局初始化或方法的注册、调用等等，也可用于系统代码通信（Android、iOS）
`SystemTimeCounter` | 计时脚本，用于在本地检查是否有时间减速类异常（可能没写对）

目录 | 描述
--|--
`ComponentExtension` | 组件扩展
`UserAccount` | 玩家数据、存档管理相关
`GamePlay` | 玩法逻辑，核心的音游功能
`Network` | 网络交互相关代码（包括与TapTap平台交互）
`Platforms` | 与平台（Android、iOS）相关的代码
`ScriptableObject` | 可序列化脚本，用于存储游戏内**不可（被玩家）更改**的配置资源
`Shader` | 游戏内部分特效（比如水纹）的制作
`UIComponent` | UI组件，与UI相关但又不直接管理整个Scene的代码
`UIContoller` | UI控制器相关脚本，直接与整个Scene相关
`Utils` | 实用工具、扩展等
`Diagnostic` | 便于开发人员排查错误、诊断程序的代码（例如Logger或与Bugly的对接）
`Data` | 管理可编辑配置的代码（例如`保存配置数据`，或者玩家的`偏好设置`）
`Configuration` | 常量的配置数据
`LevelData` | 与关卡相关的结构，例如关卡的描述信息，或者谱面的结构
`Navigation` | 程序内**界面导航**相关的代码


## 未整理的目录
```
Feature  游戏功能
```