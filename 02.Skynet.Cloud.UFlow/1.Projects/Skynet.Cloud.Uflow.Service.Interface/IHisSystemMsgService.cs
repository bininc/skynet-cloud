/************************************************************************************
* Copyright (c) 2019-07-11 12:03:03 优网科技 All Rights Reserved.
* CLR版本： .Standard 2.x
* 公司名称：优网科技
* 命名空间：UWay.Skynet.Cloud.Uflow.Service.Interface
* 文件名：  UWay.Skynet.Cloud.Uflow.Service.Interface.HisSystemMsg.cs
* 版本号：  V1.0.0.0
* 唯一标识：c2de37b3-38aa-4e4a-99f5-2d188690710b
* 创建人：  magic.s.g.xie
* 电子邮箱：xiesg@uway.cn
* 创建时间：2019-07-11 12:03:03 
* 描述：用户接收的系统消息 
* 
* 
* =====================================================================
* 修改标记 
* 修改时间：2019-07-11 12:03:03 
* 修改人： 谢韶光
* 版本号： V1.0.0.0
* 描述：
* 
* 
* 
* 
************************************************************************************/



namespace   UWay.Skynet.Cloud.Uflow.Service.Interface
{
   using System;
   using UWay.Skynet.Cloud.Data;

   using UWay.Skynet.Cloud.Request;
   using UWay.Skynet.Cloud.Uflow.Entity;
   using System.Collections.Generic;
   /// <summary>
   /// 用户接收的系统消息服务接口类
   /// </summary>
   public interface IHisSystemMsgService
   {
      /// <summary>
      /// 添加用户接收的系统消息{用户接收的系统消息}对象(即:一条记录
      /// </summary>
      long Add(HisSystemMsg  hisSystemMsg);
      /// <summary>
      /// 添加用户接收的系统消息{用户接收的系统消息}对象(即:一条记录
      /// </summary>
      void Add(IList<HisSystemMsg>  hisSystemMsgs);
      /// <summary>
      /// 更新用户接收的系统消息{用户接收的系统消息}对象(即:一条记录
      /// </summary>
      int Update(HisSystemMsg  hisSystemMsg);
      /// <summary>
      /// 删除用户接收的系统消息{用户接收的系统消息}对象(即:一条记录
      /// </summary>
      int Delete(string[] idArrays );
      /// <summary>
      /// 获取指定的用户接收的系统消息{用户接收的系统消息}对象(即:一条记录
      /// </summary>
      HisSystemMsg GetById(string id);
      /// <summary>
      /// 获取所有的用户接收的系统消息{用户接收的系统消息}对象(即:一条记录
      /// </summary>
      DataSourceResult Page(DataSourceRequest request);
   }
}
