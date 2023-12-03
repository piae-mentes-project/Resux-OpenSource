# 目录

- [Build](#Build)
- [Resources](#Resources)
  - [Default](#Default)
    - [Audio](#Audio)
    - [Image](#Image)
  - [Chapters](#Chapters)
    - [格式介绍](#格式介绍)
      - [分包](#分包)
      - [目录](目录)

# ResourcesBuilder
该仓库是本体使用的资源仓库，用于打包assetbundle资源

除音效外，所有音源均改为mp3压缩。但如果有追加资源，可随意使用wav、ogg等较高质量的压缩格式。

# Build

使用编辑器工具分别对安卓和iOS打包即可

- **Assets/Build AssetBundle Android**：安卓资源打包
- **Assets/Build AssetBundle iOS**：iOS资源打包

打包后的目录存放在**./AssetBundles/[平台名]**下。（与./Assets目录同级）

# Resources

资源

## Default

默认资源

### Audio

音频

- BGM：游戏所有bgm
- Effect：游戏所有音效

### Image

游戏图片，比如曲绘之类的

- Cover：封面图
  - MusicGroup：章节封面图
    - CHAPTER_[id].png：某章节的id对应的封面图

## Chapters

章节资源

### 格式介绍

#### 分包

每个章节单独分包，即设置不同的包名。一般包名为**“chaper”+id**的形式，以便本体程序读取，例如chapter1、chapter999等。



特别的，**chapter1000是教程**，内部仅含一套教程内容。

#### 目录

- Chaper***（该目录下的一级目录，名字仅为方便分辨）
  - Cover：该章节所有曲绘
    - **[曲名]_Cover**.png
  - Music：该章节所有歌曲
    - **[曲名]**.ogg（也可以用mp3、wav格式）
  - Score：该章节所有谱面
    - **[曲名]**：某个歌曲对应的所有谱面
      - **[曲名]_[难度].json**：该曲对应难度的谱面文件

请注意上述文件名称的格式，如有错误将无法正常找到并加载。
