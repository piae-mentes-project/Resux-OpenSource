# 目录

- [piae-mentes-core](#piae-mentes-core)
  - [工程结构](#工程结构)
    - [Animation](#Animation)
    - [Editor](#Editor)
    - [Materials](#Materials)
    - [Plugins](#Plugins)
    - [Resources](#resources)
    - [Scripts](#scriptes)
    - [Sprites](#Sprites)
  - [依赖库](#依赖库)



# piae-mentes-core

游戏本体

已移除大部分**未完成/无用**内容，仅有简单直接的本地存档

游戏使用的曲、谱、图等资源通过assetbundle加载，具体加载方式可以查看加载逻辑



## 工程结构

大致的文件夹层次结构描述

### Animation

场景内动画，也包括timeline

### Editor

编辑器拓展

### Materials

材质

模糊、水效果、波浪效果等等都在里边

### Plugins

依赖库

- Android：安卓插件，用于调用安卓代码
- iOS：iOS插件，用于调用iOS代码

### RenderTextures

静态rt

### Resources

固定的游戏资源

- Font：字体
- Image：图
- Languages：本地化文本
- Materials：动态加载的材质
- Prefabs：预制体
- ScriptableAsset：脚本化资源
- Video：视频内容

### Scenes

所有场景

### Scripts

脚本

- ComponentExtension：组件扩展
- Configuration：配置
- Data：游戏数据
- GamePlay：主要玩法
- LevelData：曲谱数据
- Navigation：场景转换
- Platforms：多平台
- ScriptableObject：资源脚本
- Utils：各种工具

### Sprites

静态图片资源（不需要在游戏内动态加载）

### StreamingAssets

不加密资源路径，可以放基本不动的ab包和不敏感的其他数据



## 依赖库

Unity工程中使用的依赖库均需要放在**Assets/Plugins**下，只是在nuget下载并添加是没法用的。

nuget下载后会保存在Assets同级的Packages文件夹下，在这里找到对应版本的目标dll并复制到Plugins就可以了
