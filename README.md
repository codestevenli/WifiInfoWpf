# WifiInfoWpf - 网络工具箱

一个简洁现代的Windows网络工具箱，支持WiFi信息查看、有线网络信息、Ping、DNS查询、端口扫描、局域网设备发现等功能。

## 功能特性

- 📶 **WiFi信息** - 查看无线网络SSID、BSSID、信号强度、认证方式
- 🔌 **有线网络** - 查看网卡MAC地址、IP地址、子网掩码、网关
- ⚡ **测速** - 跳转至测速网站进行网速测试
- 📡 **Ping** - Ping测试工具，支持自定义目标和次数
- 🌐 **DNS查询** - 查询域名的DNS记录
- 🔎 **端口扫描** - 扫描目标主机的开放端口
- 📱 **设备发现** - 扫描局域网内的在线设备

## 技术栈

- **.NET 8** - WPF框架
- **C#** - 开发语言
- **原生Windows API** - 获取网络信息

## 界面预览

采用现代Win11风格设计：
- 圆角卡片式布局
- 简洁的图标导航
- 现代化的配色方案

## 运行要求

- Windows 10/11 系统
- 无需安装 .NET 运行时（已打包成单文件）

## 下载使用

### 方式一：直接运行EXE

下载发布版本中的 `WifiInfoWpf.exe`，双击即可运行。

### 方式二：源码运行

```bash
# 克隆项目
git clone https://github.com/codestevenli/WifiInfoWpf.git

# 进入目录
cd WifiInfoWpf

# 运行项目
dotnet run
```

### 方式三：自行打包

```bash
# 发布为单文件EXE
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 项目结构

```
WifiInfoWpf/
├── App.xaml              # 应用程序入口和样式
├── App.xaml.cs          # 后台代码
├── MainWindow.xaml      # 主窗口界面
├── MainWindow.xaml.cs   # 后台逻辑
├── WifiInfoWpf.csproj   # 项目文件
└── WifiInfoWpf.sln      # 解决方案
```

## 贡献

欢迎提交Issue和Pull Request！

## 许可证

MIT License
