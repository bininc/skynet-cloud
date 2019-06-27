﻿using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using UWay.Skynet.Cloud.Data.Common;
using UWay.Skynet.Cloud.Data.Driver;
using UWay.Skynet.Cloud.ExceptionHandle;

namespace UWay.Skynet.Cloud.Data
{
    class DbHelper : ConnectionHost, IDbHelper
    {

        internal DbConfiguration dbConfiguration;
        public IDriver Driver { get; set; }

        public DbConnection Connection { get { return connection; } }

        public DbConfiguration DbConfiguration { get { return dbConfiguration; } }

        public CommandType CommandType;

        ILogger log
        {
            get
            {
                return dbConfiguration.sqlLogger();
            }
        }


        public DbParameter Parameter(string name, object value)
        {
            var p = dbConfiguration.DbProviderFactory.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            return p;
        }

        public Task<int> ExecuteNonQueryAsync(string sql, dynamic namedParameters, bool isAutoClose = true)
        {
            Guard.NotNullOrEmpty(sql, "sql");
            Exception err = null;
            string cmdSql = string.Empty;
            string cmdParams = string.Empty;
            try
            {
                using (DbCommand cmd = Driver.CreateCommand(connection, sql, namedParameters))
                {

                    cmd.CommandType = CommandType;
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    cmdParams = cmd.Parameters.JsonSerialize();
                    cmdSql = cmd.CommandText;
                    return cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                string jsonString = namedParameters.ToJson();
                log.Log(LogLevel.Error, ex, GetSqlLogInfo("ExecuteNonQuery: sql:{0}\r\nparamters:{1}", sql, jsonString));

                throw new PersistenceException(ex.Message, ex);
            }
            finally
            {
                LogSql(err, cmdSql, cmdParams);
                err = null;
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
                //AddAttachInfo();
                //log.Log();
            }

        }



        public int ExecuteNonQuery(string sql, dynamic namedParameters, bool isAutoClose = true)
        {
            Guard.NotNullOrEmpty(sql, "sql");
            Exception err = null;
            string cmdSql = string.Empty;
            string cmdParams = string.Empty;
            try
            {
                using (DbCommand cmd = Driver.CreateCommand(connection, sql, namedParameters))
                {
                    
                    cmd.CommandType = CommandType;
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    cmdParams = cmd.Parameters.JsonSerialize();
                    cmdSql = cmd.CommandText;
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                string jsonString = namedParameters.ToJson();
                log.Log(LogLevel.Error,ex, GetSqlLogInfo("ExecuteNonQuery: sql:{0}\r\nparamters:{1}", sql, jsonString));
                
                throw new PersistenceException(ex.Message, ex);
            }
            finally
            {
                LogSql(err, cmdSql, cmdParams);
                err = null;
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
                //AddAttachInfo();
                //log.Log();
            }

        }

        public DbDataReader ExecuteReader(string sql, dynamic namedParameters, bool isAutoClose = true)
        {
            Guard.NotNullOrEmpty(sql, "sql");
            Exception err = null;
            string cmdSql = string.Empty;
            string cmdParams = string.Empty;
            try
            {
                using (DbCommand cmd = Driver.CreateCommand(connection, sql, namedParameters))
                {
                    if(cmd.Parameters != null && cmd.Parameters.Count > 0)
                        cmdParams = cmd.Parameters.JsonSerialize();
                    cmdSql = cmd.CommandText;
                    //log.AddCommand(cmd.CommandText, cmd.Parameters);
                    cmd.CommandType = CommandType;
                    if (connection.State != ConnectionState.Open)
                        connection.Open();


                    //log.Log(LogLevel.Information, GetSqlLogInfo("ExecuteReader: sql:{0}\r\nparamters:{1}", sqlString, jsonString));
                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                err = ex;
                throw new QueryException(ex.Message, ex);
            }
            finally
            {
                LogSql(err, cmdSql, cmdParams);
                err = null;
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            }
        }

        public Task<DbDataReader> ExecuteReaderAsync(string sql, dynamic namedParameters, bool isAutoClose = true)
        {
            Guard.NotNullOrEmpty(sql, "sql");
            Exception err = null;
            string cmdSql = string.Empty;
            string cmdParams = string.Empty;
            try
            {
                using (DbCommand cmd = Driver.CreateCommand(connection, sql, namedParameters))
                {
                    if (cmd.Parameters != null && cmd.Parameters.Count > 0)
                        cmdParams = cmd.Parameters.JsonSerialize();
                    cmdSql = cmd.CommandText;
                    //log.AddCommand(cmd.CommandText, cmd.Parameters);
                    cmd.CommandType = CommandType;
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    
                    //log.Log(LogLevel.Information, GetSqlLogInfo("ExecuteReader: sql:{0}\r\nparamters:{1}", sqlString, jsonString));
                    return cmd.ExecuteReaderAsync();
                }
            }
            catch (Exception ex)
            {
                err = ex;
                throw new QueryException(ex.Message, ex);
            }
            finally
            {
                LogSql(err, cmdSql, cmdParams);
                err = null;
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            }
        }

        public DataTable ExecuteDataTable(string sql, dynamic namedParameters, bool isAutoClose = true)
        {

            Guard.NotNullOrEmpty(sql, "sql");
            Exception err = null;
            string cmdSql = string.Empty;
            string cmdParams = string.Empty;
            try
            {
                using (var cmd = Driver.CreateCommand(connection, sql, namedParameters))
                {
                    
                    cmd.CommandType = CommandType;
                    var adp = this.dbConfiguration.DbProviderFactory.CreateDataAdapter();
                    adp.SelectCommand = cmd;
                    var tb = new DataTable("Table1");
                    adp.Fill(tb);
                    cmdParams = cmd.Parameters.JsonSerialize();
                    cmdSql = cmd.CommandText;
                    //log.Log(LogLevel.Information, GetSqlLogInfo("ExecuteReader: sql:{0}\r\nparamters:{1}", sqlString, jsonString));
                    return tb;
                }
            }
            catch (Exception ex)
            {
                err = ex;
                throw new QueryException(ex.Message, ex);
            }
            finally
            {
                LogSql(err, cmdSql, cmdParams);
                err = null;
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            }
        }

        private void LogSql(Exception ex, string sql, string params1)
        {
            if(ex == null)
            {
                LogInformation(sql, params1);
            } else
            {
                LogError(ex, sql, params1);
            }
        }

        private void LogInformation(string sql, string params1)
        {
            
            log.Log(LogLevel.Information, GetSqlLogInfo("ExecuteNonQuery: sql:{0}\r\nparamters:{1}", sql, params1));
        }

        private void LogError(Exception ex,string sql, string params1)
        {
            log.Log(LogLevel.Error, ex, GetSqlLogInfo("ExecuteNonQuery: sql:{0}\r\nparamters:{1}", sql, params1));
        }

        private string GetSqlLogInfo(string format, params object[] args) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-------------------Sql Begin----------------");
            sb.AppendLine("-- Key:" + DbConfiguration.ConnectionString.Split(';')[0]);
            sb.AppendLine("-- DbProviderName:" + DbConfiguration.DbProviderName);
            sb.AppendFormat(format, args).AppendLine();
            sb.AppendLine("-------------------Sql End----------------");
            return sb.ToString();
        }


        public DataSet ExecuteDataSet(string sql, dynamic namedParameters, bool isAutoClose = true)
        {

            Guard.NotNullOrEmpty(sql, "sql");
            Exception err = null;
            string cmdSql = string.Empty;
            string cmdParams = string.Empty;
            try
            {
                using (var cmd = Driver.CreateCommand(connection, sql, namedParameters))
                {
                    
                    cmd.CommandType = CommandType;
                    var adp = this.dbConfiguration.DbProviderFactory.CreateDataAdapter();
                    adp.SelectCommand = cmd;
                    var ds = new DataSet();
                    adp.Fill(ds);
                    cmdParams = cmd.Parameters.JsonSerialize();
                    cmdSql = cmd.CommandText;
                    return ds;
                }
            }
            catch (Exception ex)
            {
                err = ex;
                throw new QueryException(ex.Message, ex);
            }
            finally
            {
                LogSql(err, cmdSql, cmdParams);
                err = null;
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            }
        }

        public Task<object> ExecuteScalarAsync(string sql, dynamic namedParameters, bool isAutoClose = true)
        {

            Guard.NotNullOrEmpty(sql, "sql");
            Exception err = null;
            string cmdSql = string.Empty;
            string cmdParams = string.Empty;
            try
            {
                using (DbCommand cmd = Driver.CreateCommand(connection, sql, namedParameters))
                {

                    cmd.CommandType = CommandType;
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    cmdParams = cmd.Parameters.JsonSerialize();
                    cmdSql = cmd.CommandText;
                    return cmd.ExecuteScalarAsync();
                }
            }
            catch (Exception ex)
            {
                err = ex;
                throw new QueryException(ex.Message, ex);
            }
            finally
            {
                LogSql(err, cmdSql, cmdParams);
                err = null;
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
                //AddAttachInfo();
                ////log.AddMessage("")
                //log.Log();
            }
        }


        public object ExecuteScalar(string sql, dynamic namedParameters, bool isAutoClose = true)
        {

            Guard.NotNullOrEmpty(sql, "sql");
            Exception err = null;
            string cmdSql = string.Empty;
            string cmdParams = string.Empty;
            try
            {
                using (var cmd = Driver.CreateCommand(connection, sql, namedParameters))
                {
                    
                    cmd.CommandType = CommandType;
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    cmdParams = cmd.Parameters.JsonSerialize();
                    cmdSql = cmd.CommandText;
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                err = ex;
                throw new QueryException(ex.Message, ex);
            }
            finally
            {
                LogSql(err, cmdSql, cmdParams);
                err = null;
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
                //AddAttachInfo();
                ////log.AddMessage("")
                //log.Log();
            }
        }

        public DataTable ExecutePageDataTable(string sql, long skip, long take, dynamic nameparameters, out long rowCount, bool isAutoClose = true)
        {

            Guard.NotNullOrEmpty(sql, "sql");
            //sql = GetReplaceSql(sql);
            PagingHelper.SQLParts parts;
            rowCount = 0;
            
            
            if (!PagingHelper.SplitSQL(sql, out parts))
            {
                throw new Exception("Unable to parse SQL statement for paged query");
            }
            var sqlCount = parts.sqlCount;
            var pageSql = Driver.BuildPageQuery(skip, take, parts, nameparameters);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            var tempRowCount = 0;
            DataTable dt = null;
            try
            {
                System.Threading.Tasks.Parallel.Invoke(() => tempRowCount = (int)ExecuteScalar(parts.sqlCount, nameparameters, false), () => dt = ExecuteDataTable(pageSql, nameparameters, false));
            }
            catch (Exception ex)
            {  
                throw new QueryException(ex.Message, ex);
            }
            finally {
                if (isAutoClose == true)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            }
           
            rowCount = tempRowCount;
          
            return dt;
        }


    }
}