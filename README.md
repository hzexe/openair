# OpenAirLibrary
开源的国家空气数据获取客户端
[![Telegram Chat](https://img.shields.io/badge/Chat-Telegram-blue.svg)](https://t.me/hzexe)
[![license](https://img.shields.io/github/license/hzexe/openair.svg)](https://raw.githubusercontent.com/hzexe/openair/master/LICENSE)

|Package|Branch|Build|
|:-----:|:----:|:---:|
| [NuGet ![NuGet Release](https://img.shields.io/nuget/vpre/OpenAirLibrary.svg?label=OpenAirLibrary&maxAge=3600)](https://www.nuget.org/packages/OpenAirLibrary/) | `master` | [![Build status](https://hzexe.visualstudio.com/openair/_apis/build/status/openair-.NET%20Desktop-CI?branchName=master)](https://hzexe.visualstudio.com/openair/_build/latest?definitionId=2) |

### 功能

本项目可以获取[中国环境检测总站的空气数据](http://www.cnemc.cn/)<br />

* 当前数据内容包括:城市的实时空气数据，包括AQI、pm2.5、pm10、co、o3、no2、so2、首要污染物、生活建议、污染描述等数据<br />
* 城市所有空气检测点（空气站）的实时空气质量,除包含上面城市所有的基本数据，还有检测点名称，经度纬度等信息<br />
* 城市空气质量历史数据(2013年以来)<br />
* 检测点空气质量历史数据(2013年以来)<br />
* 等等更多………………

 ### 演示截图
![演示截图](https://raw.githubusercontent.com/hzexe/openair/master/example/example.gif)

### 安装使用
Package manager:

```powershell
Install-Package OpenAirLibrary
```

### 注意事项

* 因与服务端的通信是使用WCF衍生框架，所以需要在.net framework下使用功能才完整<br />
* 因环境总站的的**网址会发生变化**，当出现连接不上，或查不出新数据时需要先排除网址是否可用以及通过浏览器访问到的数据是否正常<br />
