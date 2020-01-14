# UnityKitModule

Unity编辑器的右键菜单工具集。项目间复制、建立硬链接等

## 如何使用

用VS编译UnityKitModule，然后仅复制 UnityKitModule.dll 到任意一Unity项目，在菜单栏 Tools-> 安装UnityKit模块，安装即可。安装后即可删除项目中的该DLL文件。

或者直接打开 UnityDemo 工程（里面有已编译好的DLL文件），在菜单栏点击安装即可。

安装后对该电脑上的该 Unity 版本生效，就像Unity自带的功能一样~

## 功能

### Project

![snipaste_20180316_142435](Doc/snipaste_20180316_142435.png)

项目间资源复制粘贴功能

* 粘贴

* 复制 - 到编辑器

  只能在Unity编辑器粘贴文件。

* 工具箱 -> 复制 - 到剪贴板（仅 Windows）

  可直接在系统文件夹面板粘贴。

* 工具箱 -> 复制 - 到编辑器（导出包）

  通过导出Package方式，复制粘贴。粘贴时会触发导入。

快速建立文件夹硬链接

* 工具箱 -> 硬链接（Mac OS下为软链接）

查找引用

### Inspector面板（Context）

* Remove All Components
* Remove All Childs Components

### Hierarchy

