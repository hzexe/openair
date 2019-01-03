# openair
开源的国家空气数据获取客户端
=================================
[![Build status](https://hzexe.visualstudio.com/openair/_apis/build/status/openair-.NET%20Desktop-CI)](https://hzexe.visualstudio.com/openair/_build/latest?definitionId=2)

作用
--------------------------------
本项目可以获取[中国环境检测总站的空气数据](http://113.108.142.147:20035/emcpublish/),也包含restful的数据分发服务<br />

当前数据内容包括:城市的实时空气数据，包括AQI、pm2.5、pm10、co、o3、no2、so2、首要污染物、生活建议、污染描述等数据<br />
城市所有空气检测点（空气站）的实时空气质量,除包含上面城市所有的基本数据，还有检测点名称，经度纬度等信息<br />
城市空气质量历史数据(2013年以来)<br />
检测点空气质量历史数据(2013年以来)<br />

主要工程描述
----------------------------------
###com.Hzexe.Air.API
空气数据的web分发方式，基于web api，比如要给app提供数据支持，就有可能会用到；<br />
其中有个在线版的演示：[https://api.air.hzexe.com/help](https://api.air.hzexe.com/help)<br />
需要修改web.config中数据连接方式<br />
```xml
  <connectionStrings>
    <add name="mysql1" connectionString="Data Source=(localdb)\ProjectsV12;Initial Catalog=openair;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False"
       providerName="System.Data.SqlClient"/>
  </connectionStrings>
```
###com.Hzexe.Air.Fetch
封装了获取空气数据，并写入数据库，一气呵成，需要更改app.config中的数据连接，并且确保app.config中的内容在可执行文件的配置文件(xxx.exe.config或web.config)中能找得到这些配置<br />
需要修改web.config中数据连接方式
```xml
  <connectionStrings>
    <add name="mysql1" connectionString="Data Source=(localdb)\ProjectsV12;Initial Catalog=openair;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False"
       providerName="System.Data.SqlClient"/>
  </connectionStrings>
```
###example
这是一个控制台工程的示例，里面描述了如何获取基本的数据，以及初始化数据和填充数据<br />
需要修改web.config中数据连接方式
```xml
  <connectionStrings>
    <add name="mysql1" connectionString="Data Source=(localdb)\ProjectsV12;Initial Catalog=openair;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False"
       providerName="System.Data.SqlClient"/>
  </connectionStrings>
```
注意事项
-----------------------------------
1.com.Hzexe.Air.Fetch工程会初始化数据库，因此，连接数据库的用户的权限至少要有create\drop database、table的权限，其它项目如com.Hzexe.Air.API只是读取这个数据库里的数据;<br />
2.可使用的数据库类型不限于localdb，在mysql,localdb,sqlserver测试通过，使用其它数据库需除了更改上面提到的数据连接文件，还需要在app.config或web.config添加相应数据库在entity framework相关节点的驱动，并且把驱动程序集放在程序的执行目录。<br />
3.有问题请发Issues，尚未提供其它途径的技术支持<br />

其它
-----------------------------------
在线演示<br />
[http://air.hzexe.com/](http://air.hzexe.com/)
