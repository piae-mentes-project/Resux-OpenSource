# 目录简述
从原`Scripts/Assets`迁移至此。     
与`Helper`的代码作用大致相同，但是更注重于数据的载入和管理



常用的加载器被封装在ImageLoader、AudioLoader，包含常用的以及章节用的音频和图像加载器

- AssetBundleDM：ab包加载的静态管理类，和AssetBundleLoader泛型类搭配使用
- AssetBundleResourceLoader：ab包加载的泛型类，可以get资源、unload包，可以异步加载资源，且存在资源缓存。当缓存中不存在资源时会以同步的方式加载
- AssetsFilePathDM：ab包路径静态类，纯粹存放路径以及获取路径的方法
- AudioLoader：音频加载器，会默认加载默认的ab资源包，并会显式加载章节包。同一时间只会加载一个章节包，多余的会在一定时间后被AssetBundleDM释放
- ImageLoader：同上，但是是加载图的
- MapLoader：同上，但不存在默认包，并且是加载谱面的
- ResourcesLoader：Resources的资源加载器，泛型类，与ab包的加载差不多，也拥有缓存，但没有DM的管理