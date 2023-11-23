# 目录简述
存放一些方便其他代码编写的实用性代码，减少重复工作。



- AsyncUtils：异步工具，包含一些常用的异步方法，方便异步调用
- CoroutineUtils：协程工具，包含一些常用的协程方法，减少重复编写，同时带有全局的协程方法的引用，方便非mono脚本使用协程（如果必要）
- ExtendEventTrigger：event trigger的简单扩展，将面板挂载转为动态的监听，可以在代码内添加监听用的方法，方便编写脚本
- TextureBlur：一次性的图像模糊脚本，减少使用shader持续计算模糊的无用开销
- UserLocalSettings：本地设置（PlayerRef）的简单封装，让注册表值的存取带上默认值且自动Save
- Utils：一些常用方法的工具类，减少重复代码