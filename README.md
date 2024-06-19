# Jumpserver用户登录信息按月统计

## 使用说明：

1. 在servers.json里可以预先定义服务器信息，交互模式选择对应服务器即可，debug模式使用servers.dev.json文件

2. 选择服务器后输入年月即可获取统计信息，默认值是选择上一个月

3. 可以命令行带参数执行，格式如下

   ```
   JmsUserLogonStatistics.exe 服务器url 服务器token 年 月
   
   例如：
   
    JmsUserLogonStatistics.exe http://jms.ops.local xxxx 2024 5
   ```
   
   

## 使用效果：

##### 选择预定服务器：

![](pics\1.png)



![](pics\2.png)



![](pics\3.png)



##### 选择手动输入：

![](pics\4.png)