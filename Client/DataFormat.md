# 各种数据文件的存储格式

## 目录

- [本地数据的目录结构](#本地数据目录结构)
- [音乐信息配置存储格式](#音乐信息配置存储格式)



### 本地数据目录结构

- 根目录
  - GamePreSetting.conf



### 音乐信息配置存储格式

- 文件名为“MusicDataConfig”
- 文件位于Assets/Resources/ScriptableAsset/
- 配置分章节（MusicGroup），章节内有若干音乐配置（MusicConfig）
- 章节配置格式如下：
  - id：从0起至999，不可超过三位数
  - name：章节名，是个本地化Key
  - music configs：音乐配置信息列表

- 音乐配置内容格式如下：
  - id：音乐的唯一id，格式为“**1+三位数章节id+二位数曲目id**”，章节id不足三位的向前补0
  - song name：音乐名，不是本地化key（暂时不需要本地化）
  - sub title：音乐的副标题，可空着
  - artist name：曲师
  - illustration name：曲绘画师
  - bpm range：bpm的范围，min和max
  - music preview range：选曲时的预览段起止时间，单位是**ms**
  - chart infos：谱面信息
    - level：难度，填写1-999，对应0.1%到99.9%（大于二位数不显示小数，所以是99%）
    - designer：谱师



