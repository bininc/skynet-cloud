﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWay.Skynet.Cloud.Data;
using System.Linq.Expressions;
using System.Data;


using UWay.Skynet.Cloud.Data.Common;
using System.IO;
using System.Data.Common;


namespace UWay.Skynet.Cloud.Data
{
    public abstract class ObjectRepository : IDisposable
    {
        IDbContext dbContext;
        public ObjectRepository(IDbContext dbcontext)
        {
            dbContext = dbcontext;
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    disposing = true;
                }
            }
            this.disposed = true;
        }

        /// <summary>
        /// 启用Ado.net事务
        /// </summary>
        /// <param name="action"></param>
        /// <param name="isolationLevel"></param>
        public void UsingTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            dbContext.UsingTransaction(action, isolationLevel);
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <!--普通查询-->
        /// CreateQuery<OrgnizationInfo>().Where(m => m.Invalid == 0).OrderBy(m => m.SeqNo)
        /// <!--分页查询-->
        /// CreateQuery< LoginLockLog>().Where(condition.ToLambda<LoginLockLog>()).OrderByDescending(p => p.CreateDate).Paging(pagination);
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>返回实体查询信息，不执行</returns>
        protected IQueryable<T> CreateQuery<T>()
        {
            return dbContext.Set<T>();
        }

        //protected IQueryable<T> CreateQueryAynsc<T>()
        //{
            
        //}


        /// <summary>
        /// 根据主键查询实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="id">主键列表</param>
        /// <returns>实体</returns>
        protected T GetByID<T>(object id)
        {
            return dbContext.Set<T>().Get(id);
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <param name="instances"></param>
        /// <param name="fnOperation"></param>
        /// <example>
        /// <code>
        /// <![CDATA[ int n = 10;
        /// var custs = Enumerable.Range(1, n).Select(
        ///    i => new
        ///    {
        ///        CustomerID = "XX" + i,
        ///        CompanyName = "Company" + i,
        ///        ContactName = "Contact" + i,
        ///        City = "Seattle",
        ///       Country = "USA"
        ///   });
        /// using(var r = new CustomerRepository(Unity.Container))
        /// {
        ///     r.Batch<Customer, Customer>(custs,(u, c) => u.Insert(c));
        /// }
        /// ]]
        /// </code>
        /// </example>
        /// <returns></returns>
        protected IEnumerable<int> Batch<T, M>(IEnumerable<M> instances, Expression<Func<IRepository<T>, M, int>> fnOperation)
        {
            return dbContext.Set<T>().Batch<M>(instances, fnOperation);
        }


        /// <summary>
        /// 添加实体，返回影响行数
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="t">实体</param>
        /// <returns>影响行数</returns>
        protected int Add<T>(T t)
        {
            return dbContext.Set<T>().Insert(t);
        }

        /// <summary>
        /// 添加实体，返回指定类型
        /// </summary>
        /// <typeparam name="T">增加的实体类型</typeparam>
        /// <typeparam name="S">返回类型</typeparam>
        /// <param name="instance">实体</param>
        /// <param name="resultSeletor">返回选择的实体类型</param>
        /// <![CDATA[
        ///  Add<UserInfo, int>(new User(){UserNo = "xxxx",UserName="1132" ...}, p => p.UserID)
        /// ]]>
        /// <returns>返回指定类型</returns>

        protected S Add<T, S>(object instance, Expression<Func<T, S>> resultSeletor)
        {
            return dbContext.Set<T>().Insert<T, S>(instance, resultSeletor);
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="t">实体</param>
        /// <returns></returns>
        protected int Update<T>(T t)
        {
            return dbContext.Set<T>().Update(t);
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="instance">实体</param>
        /// <param name="updateCheck">更行条件</param>
        /// <![CDATA[
        /// Update<Entity.CategoryItem>(new { Invalid =DataDeleteStatusEnum.Invalid }, c => idArray.Contains(c.CategoryItemId));
        /// ]]>
        /// <returns></returns>
        protected int Update<T>(object instance, Expression<Func<T, bool>> updateCheck)
        {
            return dbContext.Set<T>().Update(instance, updateCheck);
        }


        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="t">实体类</param>
        /// <returns></returns>
        protected int Delete<T>(object t)
        {
            return dbContext.Set<T>().Delete(t);
        }

        ///// <summary>
        ///// 批量删除
        ///// </summary>
        ///// <typeparam name="T">实体类型</typeparam>
        ///// <param name="instance">实体类</param>
        ///// <param name="deleteCheck">删除条件</param>
        ///// <returns></returns>
        //public int Delete<T>(object instance, Expression<Func<T, bool>> deleteCheck)
        //{
        //    return dbContext.Set<T>().Delete(instance, deleteCheck);
        //}

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="deleteCheck">条件</param>
        /// <![CDATA[
        /// Delete<OrgnizationInfo>(p => idArray.Contains(p.OrgID))
        /// ]]>
        /// <returns></returns>
        protected int Delete<T>(Expression<Func<T, bool>> deleteCheck)
        {
            return dbContext.Set<T>().Delete(deleteCheck);
        }


        /// <summary>
        /// 执行无查询SQL
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="namedParameters">参数，可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        /// <param name="isAutoClose">是否自动关闭连接</param>
        /// <![CDATA[
        /// ExecuteNonQuery("DELETE UFA_USER_INFO WHERE UserNo = @UserNo",new {UserNo = 0});
        /// ]]>
        /// <returns>影响函数</returns>
        protected int ExecuteNonQuery(string sql, dynamic namedParameters = null, bool isAutoClose = true)
        {
            return dbContext.DbHelper.ExecuteNonQuery(sql, namedParameters, isAutoClose);
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="sql">存储过程+参数</param>
        /// <param name="namedParameters">参数，可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        /// <param name="isAutoClose">是否自动关闭连接</param>
        /// <![CDATA[
        /// RunNonProcedure("Exs UserNo = @UserNo",new {UserNo = 0});
        /// ]]>
        /// <returns>影响函数</returns>
        protected int RunNonProcedure(string sql, dynamic namedParameters = null, bool isAutoClose = true)
        {

            return dbContext.SpHelper.ExecuteNonQuery(sql, namedParameters, isAutoClose);
        }

        ///// <summary>
        ///// 执行无查询SQL
        ///// </summary>
        ///// <param name="query">构建的插入,,<see cref="InsertQuery"/></param>
        ///// <param name="namedParameters">参数，可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">是否自动关闭连接</param>
        ///// <![CDATA[
        /////     InsertQuery query = new InsertQuery(tableName);
        /////     query.Terms.Add(new UpdateTerm("GROUP_ID", SqlExpression.Parameter("GroupID")));
        /////     query.Terms.Add(new UpdateTerm("group_name", SqlExpression.Parameter("GroupName")));
        /////     query.Terms.Add(new UpdateTerm("group_level", SqlExpression.Parameter("GroupLevel")));
        /////     query.Terms.Add(new UpdateTerm("parentid", SqlExpression.Parameter("ParentID")));
        /////     query.Terms.Add(new UpdateTerm("user_name", SqlExpression.Parameter("UserName")));
        /////     query.Terms.Add(new UpdateTerm("remark", SqlExpression.Parameter("Remark")));
        /////     query.Terms.Add(new UpdateTerm("time_stamp", SqlExpression.Parameter("TimeStamp")));
        /////     query.Terms.Add(new UpdateTerm("city_id", SqlExpression.Parameter("CityID")));
        /////     query.Terms.Add(new UpdateTerm("level_id", SqlExpression.Parameter("LevelID")));
        /////     query.Terms.Add(new UpdateTerm("sharetype", SqlExpression.Parameter("ShareType")));
        /////     ExecuteNonQuery(query, new
        /////     {
        /////         GroupName = item.GroupName,
        /////         GroupLevel = item.GroupLevel,
        /////         ParentID = item.ParentID,
        /////         UserName = item.UserName,
        /////         Remark = item.Remark,
        /////         TimeStamp = item.TimeStamp,
        /////         CityID = item.CityID,
        /////         LevelID = item.LevelID,
        /////         ShareType = item.ShareType,
        /////         GroupID = item.GroupID
        /////         });
        ///// ]]>
        ///// <returns></returns>
        //public int ExecuteNonQuery(InsertQuery query, dynamic namedParameters = null, bool isAutoClose = true)
        //{
        //    var sql = dbContext.DbConfiguration.Driver.Render.RenderInsert(query);
        //    return ExecuteNonQuery(sql, namedParameters, isAutoClose);
        //}

        ///// <summary>
        ///// 执行无查询SQL
        ///// </summary>
        ///// <param name="query">构建的删除,<see cref="DeleteQuery"/></param>
        ///// <param name="namedParameters">参数，可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">是否自动关闭连接</param>
        ///// <![CDATA[
        ///// DeleteQuery query = new DeleteQuery("products");
        ///// query.WhereClause.Terms.Add(WhereTerm.CreateCompare(SqlExpression.Field("productId"), SqlExpression.Number(999), CompareOperator.Equal));
        ///// ExecuteNonQuery(query);
        ///// ]]>
        ///// <returns></returns>
        //public int ExecuteNonQuery(DeleteQuery query, dynamic namedParameters = null, bool isAutoClose = true)
        //{
        //    var sql = dbContext.DbConfiguration.Driver.Render.RenderDelete(query);
        //    return ExecuteNonQuery(sql, namedParameters, isAutoClose);
        //}

        ///// <summary>
        ///// 执行无查询SQL
        ///// </summary>
        ///// <param name="query">构建的更新,<see cref="UpdateQuery"/></param>
        ///// <param name="namedParameters">参数，可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">是否自动关闭连接</param>
        ///// <![CDATA[
        ///// UpdateQuery updateQuery = new UpdateQuery(tableName);
        ///// updateQuery.Terms.Add(new UpdateTerm("group_name", SqlExpression.Parameter("GroupName")));
        ///// updateQuery.Terms.Add(new UpdateTerm("group_level", SqlExpression.Parameter("GroupLevel")));
        ///// updateQuery.Terms.Add(new UpdateTerm("parentid", SqlExpression.Parameter("ParentID")));
        ///// updateQuery.Terms.Add(new UpdateTerm("user_name", SqlExpression.Parameter("UserName")));
        ///// updateQuery.Terms.Add(new UpdateTerm("remark", SqlExpression.Parameter("Remark")));
        ///// updateQuery.Terms.Add(new UpdateTerm("time_stamp", SqlExpression.Parameter("TimeStamp")));
        ///// updateQuery.Terms.Add(new UpdateTerm("city_id", SqlExpression.Parameter("CityID")));
        ///// updateQuery.Terms.Add(new UpdateTerm("level_id", SqlExpression.Parameter("LevelID")));
        ///// updateQuery.Terms.Add(new UpdateTerm("sharetype", SqlExpression.Parameter("ShareType")));
        ///// updateQuery.WhereClause.Terms.Add(WhereTerm.CreateCompare(SqlExpression.Field("GroupID"),SqlExpression.Parameter("group_id"), FilterOperator.IsEqualTo));
        ///// ExecuteNonQuery(updateQuery, new
        ///// {
        /////     GroupName = item.GroupName,
        /////     GroupLevel = item.GroupLevel,
        /////     ParentID = item.ParentID,
        /////     UserName = item.UserName,
        /////     Remark = item.Remark,
        /////     TimeStamp = item.TimeStamp,
        /////     CityID = item.CityID,
        /////     LevelID = item.LevelID,
        /////     ShareType = item.ShareType,
        /////     GroupID = item.GroupID
        /////     });
        ///// ]]>
        ///// <returns></returns>
        //public int ExecuteNonQuery(UpdateQuery query, dynamic namedParameters = null, bool isAutoClose = true)
        //{
        //    var sql = dbContext.DbConfiguration.Driver.Render.RenderUpdate(query);
        //    return ExecuteNonQuery(sql, namedParameters, isAutoClose);
        //}

        ///// <summary>
        ///// 查询，返回DataTable
        ///// </summary>
        ///// <param name="query">构建的查询,<see cref="SelectQuery"/></param>
        ///// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">是否自动关闭连接</param>
        ///// <![CDATA[
        /////  FromTerm groups = FromTerm.Table(tableName);
        /////  var subquery = new SelectQuery();
        /////  subquery.Columns.AddRange(new SelectColumn[] {
        /////     new SelectColumn("group_id", groups,"sub_id"),
        /////     new SelectColumn("parentid", groups,"p_id")
        /////     });
        /////  FromTerm subgroups = FromTerm.SubQuery(subquery, "b");
        /////  SelectQuery query = new SelectQuery();
        /////  query.Columns.AddRange(new SelectColumn[] {
        /////     new SelectColumn("group_id", groups,"GroupID"),
        /////     new SelectColumn("group_name", groups,"GroupName"),
        /////     new SelectColumn("group_level", groups,"GroupLevel"),
        /////     new SelectColumn("parentID", groups,"ParentID"),
        /////     new SelectColumn("user_name", groups,"UserName"),
        /////     new SelectColumn("remark", groups,"Remark"),
        /////     new SelectColumn("time_stamp", groups,"TimeSstamp"),
        /////     new SelectColumn("ShareType", groups,"ShareType"),
        /////     new SelectColumn("level_id", groups,"LevelID"),
        /////     new SelectColumn("city_id", groups,"CityID"),
        /////     new SelectColumn("HasGroup", groups,""),
        /////     new SelectColumn("sub_id", subgroups,"HasGroup", SqlAggregationFunction.Min)
        /////     });
        /////  query.FromClause.BaseTable = groups;
        /////  query.FromClause.Join(LinqJoinType.Inner, query.FromClause.BaseTable, subgroups, "group_id", "p_id");
        /////  query.GroupByTerms.AddRange(new GroupByTerm[] {
        /////     new GroupByTerm("group_id", groups),
        /////     new GroupByTerm("group_name", groups),
        /////     new GroupByTerm("group_level", groups),
        /////     new GroupByTerm("parentid", groups),
        /////     new GroupByTerm("user_name", groups),
        /////     new GroupByTerm("remark", groups),
        /////     new GroupByTerm("city_id", groups),
        /////     new GroupByTerm("level_id", groups),
        /////     new GroupByTerm("sharetype", groups),
        /////     });
        /////  query.OrderByTerms.Add(new OrderByTerm("group_name", groups, OrderByDirection.Ascending));
        /////  IDictionary<string, object> parameters = new Dictionary<string, object>();
        /////  foreach (var condition in conditions)
        /////  {
        /////     query.WherePhrase.Terms.Add(WhereTerm.CreateCompare(SqlExpression.Field(condition.Name), SqlExpression.Parameter(condition.Name), condition.Operator));
        /////     parameters.Add(condition.Name, condition.Value);
        /////  }
        /////  return ExecuteDataTable(query, parameters).ToList<NeGroup>();
        ///// ]]>
        ///// <returns></returns>

        //public DataTable ExecuteDataTable(SelectQuery query, dynamic namedParameters = null, bool isAutoClose = true)
        //{
        //    var sql = dbContext.DbConfiguration.Driver.Render.RenderSelect(query);
        //    return ExecuteDataTable(sql, namedParameters, isAutoClose);
        //}


        ///// <summary>
        ///// 查询，返回DataSet
        ///// </summary>
        ///// <param name="query">构建的查询,<see cref="SelectQuery"/></param>
        ///// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">事务是否自动提交</param>
        ///// <![CDATA[
        /////  FromTerm groups = FromTerm.Table(tableName);
        /////  var subquery = new SelectQuery();
        /////  subquery.Columns.AddRange(new SelectColumn[] {
        /////     new SelectColumn("group_id", groups,"sub_id"),
        /////     new SelectColumn("parentid", groups,"p_id")
        /////     });
        /////  FromTerm subgroups = FromTerm.SubQuery(subquery, "b");
        /////  SelectQuery query = new SelectQuery();
        /////  query.Columns.AddRange(new SelectColumn[] {
        /////     new SelectColumn("group_id", groups,"GroupID"),
        /////     new SelectColumn("group_name", groups,"GroupName"),
        /////     new SelectColumn("group_level", groups,"GroupLevel"),
        /////     new SelectColumn("parentID", groups,"ParentID"),
        /////     new SelectColumn("user_name", groups,"UserName"),
        /////     new SelectColumn("remark", groups,"Remark"),
        /////     new SelectColumn("time_stamp", groups,"TimeSstamp"),
        /////     new SelectColumn("ShareType", groups,"ShareType"),
        /////     new SelectColumn("level_id", groups,"LevelID"),
        /////     new SelectColumn("city_id", groups,"CityID"),
        /////     new SelectColumn("HasGroup", groups,""),
        /////     new SelectColumn("sub_id", subgroups,"HasGroup", SqlAggregationFunction.Min)
        /////     });
        /////  query.FromClause.BaseTable = groups;
        /////  query.FromClause.Join(LinqJoinType.Inner, query.FromClause.BaseTable, subgroups, "group_id", "p_id");
        /////  query.GroupByTerms.AddRange(new GroupByTerm[] {
        /////     new GroupByTerm("group_id", groups),
        /////     new GroupByTerm("group_name", groups),
        /////     new GroupByTerm("group_level", groups),
        /////     new GroupByTerm("parentid", groups),
        /////     new GroupByTerm("user_name", groups),
        /////     new GroupByTerm("remark", groups),
        /////     new GroupByTerm("city_id", groups),
        /////     new GroupByTerm("level_id", groups),
        /////     new GroupByTerm("sharetype", groups),
        /////     });
        /////  query.OrderByTerms.Add(new OrderByTerm("group_name", groups, OrderByDirection.Ascending));
        /////  IDictionary<string, object> parameters = new Dictionary<string, object>();
        /////  foreach (var condition in conditions)
        /////  {
        /////     query.WherePhrase.Terms.Add(WhereTerm.CreateCompare(SqlExpression.Field(condition.Name), SqlExpression.Parameter(condition.Name), condition.Operator));
        /////     parameters.Add(condition.Name, condition.Value);
        /////  }
        /////  return ExecuteDataTable(query, parameters).ToList<NeGroup>();
        ///// ]]>
        ///// <returns></returns>
        //public DataSet ExecuteDataSet(SelectQuery query, dynamic namedParameters = null, bool isAutoClose = true)
        //{

        //    var sql = dbContext.DbConfiguration.Driver.Render.RenderSelect(query);
        //    return ExecuteDataSet(sql, namedParameters, isAutoClose);
        //}

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        /// <param name="isAutoClose">是否自动关闭连接</param>
        /// <![CDATA[
        /// ExecuteDataTable("select * from CFG_INDICATOR_BUSY WHERE ID = @ID", new {ID = 1});
        /// ]]>
        /// <returns>返回DataTable</returns>
        protected DataTable ExecuteDataTable(string sql, dynamic namedParameters = null, bool isAutoClose = true)
        {
            if (dbContext.DbConfiguration.Driver.NamedPrefix != '@')
                sql = ParameterHelper.rxParamsPrefix.Replace(sql, m => dbContext.DbConfiguration.Driver.NamedPrefix + m.Value.Substring(1));
            sql = sql.Replace("@@", "@");		   // <- double @@ escapes a single @

            return dbContext.DbHelper.ExecuteDataTable(sql, namedParameters, isAutoClose);
        }

        /// <summary>
        /// 执行查询，返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        /// <param name="isAutoClose">是否自动关闭连接</param>
        /// <![CDATA[
        /// ExecuteDataTable("select * from CFG_INDICATOR_BUSY WHERE ID = @ID", new {ID = 1});
        /// ]]>
        /// <returns>返回DataSet</returns>
        protected DataSet ExecuteDataSet(string sql, dynamic namedParameters = null, bool isAutoClose = true)
        {
            return dbContext.DbHelper.ExecuteDataSet(sql, namedParameters, isAutoClose);
        }

        ///// <summary>
        ///// 执行查询，首行首列值
        ///// </summary>
        ///// <param name="query">构建查询，<see cref="SelectQuery"/></param>
        ///// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">事务是否自动提交</param>
        ///// <returns>首行首列值</returns>
        //public object ExecuteScalar(SelectQuery query, dynamic namedParameters = null, bool isAutoClose = true)
        //{
        //    var sql = dbContext.DbConfiguration.Driver.Render.RenderSelect(query);
        //    return ExecuteScalar(sql, namedParameters, isAutoClose);
        //}
        /// <summary>
        /// 执行查询，首行首列值
        /// </summary>
        /// <param name="sql">构建查询</param>
        /// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        /// <param name="isAutoClose">是否自动关闭连接</param>
        /// <returns>首行首列值</returns>
        protected object ExecuteScalar(string sql, dynamic namedParameters = null, bool isAutoClose = true)
        {

            return dbContext.DbHelper.ExecuteScalar(sql, namedParameters, isAutoClose);
        }

        ///// <summary>
        ///// 执行查询，首行首列值
        ///// </summary>
        ///// <param name="query">构建查询，<see cref="SelectQuery"/></param>
        ///// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">事务是否自动提交</param>
        ///// <returns>首行首列值</returns>
        //public object ExecuteScalar(SelectQuery query, dynamic namedParameters = null, bool isAutoClose = true)
        //{
        //    var sql = dbContext.DbConfiguration.Driver.Render.RenderSelect(query);
        //    return ExecuteScalar(sql, namedParameters, isAutoClose);
        //}
        /// <summary>
        /// 执行查询，首行首列值
        /// </summary>
        /// <param name="sql">构建查询</param>
        /// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        /// <param name="isAutoClose">是否自动关闭连接</param>
        /// <returns>首行首列值</returns>
        protected Task<object> ExecuteScalarAsync(string sql, dynamic namedParameters = null, bool isAutoClose = true)
        {

            return dbContext.DbHelper.ExecuteScalarAsync(sql, namedParameters, isAutoClose);
        }

        ///// <summary>
        ///// 执行分页查询
        ///// </summary>
        ///// <param name="sql">查询语句</param>
        ///// <param name="pagination">分页信息</param>
        ///// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">是否自动关闭连接</param>
        ///// <returns>首行首列值</returns>
        //public DataTable ExecutePageDataTable(string sql, Pagination pagination, dynamic namedParameters = null, bool isAutoClose = true)
        //{
        //    if (pagination.SortExpress != null && pagination.SortExpress.Count > 0)
        //    {
        //        sql = string.Format("{0} ORDER BY {1}", sql, pagination.SortExpress.Concat());
        //    }

        //    if (pagination.Paging == false)
        //    {
        //        return dbContext.DbHelper.ExecuteDataTable(sql, namedParameters, isAutoClose);
        //    }
        //    else
        //    {
        //        long rowCount = 0;
        //        var table = dbContext.DbHelper.ExecutePageDataTable(sql, (long)((pagination.CurrentPageIndex - 1) * pagination.PageSize), (long)(pagination.CurrentPageIndex * pagination.PageSize), namedParameters, out rowCount, isAutoClose);
        //        pagination.RowCount = rowCount;
        //        return table;
        //    }
        //}

        ///// <summary>
        ///// 执行分页查询
        ///// </summary>
        ///// <param name="query">查询语句</param>
        ///// <param name="pagination">分页信息</param>
        ///// <param name="namedParameters">参数,可以为匿名类，IDictionary，Hashtable,NameValueCollection</param>
        ///// <param name="isAutoClose">是否自动关闭连接</param>
        ///// <returns>首行首列值</returns>
        //public DataTable ExecutePageDataTable(SelectQuery query, Pagination pagination, dynamic namedParameters = null, bool isAutoClose = true)
        //{
        //    var sql = dbContext.DbConfiguration.Driver.Render.RenderSelect(query).Replace("\"", "").SimplifyBracket();
        //    if (pagination.SortExpress != null && pagination.SortExpress.Count > 0)
        //    {
        //        sql = string.Format("{0} ORDER BY {1}", sql, pagination.SortExpress.Concat());
        //    }

        //    if (pagination.Paging == false)
        //    {
        //        return dbContext.DbHelper.ExecuteDataTable(sql, namedParameters, isAutoClose);
        //    }
        //    else
        //    {
        //        long rowCount = 0;
        //        var table = dbContext.DbHelper.ExecutePageDataTable(sql, (long)((pagination.CurrentPageIndex - 1) * pagination.PageSize), (long)(pagination.CurrentPageIndex * pagination.PageSize), namedParameters, out rowCount, isAutoClose);
        //        pagination.RowCount = rowCount;
        //        return table;
        //    }
        //}

        ///// <summary>
        ///// 导出CSV文件
        ///// </summary>
        ///// <param name="fileName">文件信息</param>
        ///// <param name="columns">列头</param>
        ///// <param name="sql">查询语句</param>
        ///// <param name="namedParameters">参数</param>
        //protected void ExportCSV(string fileName, Dictionary<string, string> columns, string sql, dynamic namedParameters = null)
        //{
        //    List<string> csvRows = new List<string>();
        //    StringBuilder sb = new StringBuilder();
        //    var length = 0;
        //    foreach (var item in columns)
        //    {
        //        sb.AppendFormat("\"{0}\"", item.Value.Replace("\n", ""));
        //        if (length < columns.Count - 1)
        //            sb.Append(",");

        //        length++;
        //    }
        //    sb.AppendLine();
        //    using (var fs = FileManager.OpenFile(fileName, FileMode.OpenOrCreate))
        //    {
        //        FileManager.Append(fs, Encoding.Default.GetBytes(sb.ToString()));
        //        length = 0;
        //        sb.Clear();
        //        using (DbDataReader reader = dbContext.DbHelper.ExecuteReader(sql, namedParameters, false))
        //        {
        //            var count = 0;
        //            while (reader.Read())
        //            {
        //                foreach (var item in columns)
        //                {
        //                    var index = reader.GetOrdinal(item.Key);
        //                    if (!reader.IsDBNull(index))
        //                    {
        //                        string value = reader.GetValue(index).ToString();
        //                        if (reader.GetFieldType(index) == typeof(String))
        //                        {
        //                            //If double quotes are used in value, ensure each are replaced but 2.
        //                            if (value.IndexOf("\"") >= 0)
        //                                value = value.Replace("\"", "\"\"");

        //                            //If separtor are is in value, ensure it is put in double quotes.
        //                            if (value.IndexOf(",") >= 0)
        //                                value = "\"" + value + "\"";
        //                        }
        //                        sb.Append(value);
        //                    }
        //                    if (length < columns.Count - 1)
        //                        sb.Append(",");

        //                    length++;
        //                }

        //                if (count > 60000)
        //                {
        //                    sb.AppendLine();
        //                    FileManager.Append(fs, Encoding.Default.GetBytes(sb.ToString()));
        //                    count = 0;
        //                    sb.Clear();
        //                }
        //                else
        //                {
        //                    sb.AppendLine();
        //                    count++;
        //                }

        //                length = 0;
        //            }

        //            reader.Close();
        //        }
        //        if (sb.Length > 0)
        //            FileManager.Append(fs, Encoding.Default.GetBytes(sb.ToString()));
        //    }
        //}

        ///// <summary>
        ///// 异步导出CSV文件
        ///// </summary>
        ///// <param name="userId">用户ID.</param>
        ///// <param name="fileName">文件名称.</param>
        ///// <param name="columns">列头.</param>
        ///// <param name="sql">查询语句.</param>
        ///// <param name="namedParameters">参数.</param>
        ///// <returns>Task&lt;System.Boolean&gt;.</returns>
        //protected Task<bool> ExportCSVAsync(int userId, string fileName, Dictionary<string, string> columns, string sql, dynamic namedParameters = null)
        //{
        //    string tempFileName = string.Format("\\exportjob\\{0}\\temp_{1}", userId, fileName);
        //    //先生成文件，以便下载列表显示
        //    FileManager.OpenFile(tempFileName, FileMode.Create).Close();
        //    var task = Task.Run<bool>(() =>
        //    {
        //        try
        //        {
        //            ExportCSV(tempFileName, columns, sql, namedParameters);
        //            fileName = string.Format("\\exportjob\\{0}\\{1}", userId, fileName);
        //            FileManager.Rename(tempFileName, fileName);
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggingManager.GetLogger("export").Error("Sql异步导出异常:" + ex.ToString());
        //            fileName = string.Format("\\exportjob\\{0}\\error_{1}", userId, fileName);
        //            FileManager.Rename(tempFileName, fileName);
        //        }
        //        return true;
        //    });
        //    return task;
        //}
    }
}