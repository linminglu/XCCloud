using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;
using XCCloudService.Common.Extensions;
using XCCloudService.Model.XCCloud;

namespace XCCloudService.DAL.Base
{
    public partial class BaseDAL<T> where T : class, new()
    {

        protected string dbContextName;
        private DbContext dbContext;        

        private void MakeVerifiction(bool identity, ref T t, T oldT = null)
        {
            try
            {
                //检查该实体是否需要校验
                if (t.ContainProperty("Verifiction"))
                {
                    //获取校验码
                    var verifiction = Convert.ToString(t.GetPropertyValue("Verifiction"));

                    //获取校验密钥
                    var merchSecret = string.Empty;
                    if (t.ContainProperty("MerchID"))
                    {
                        var merchId = Convert.ToString(t.GetPropertyValue("MerchID"));
                        if (!merchId.IsNull())
                        {
                            merchSecret = dbContext.Set<Base_MerchantInfo>().Where(w => w.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase))
                                .Select(o => o.MerchSecret).FirstOrDefault() ?? string.Empty;
                        }
                    }

                    //先校验                    
                    var str = string.Empty;
                    var md5 = string.Empty;
                    if (oldT != null)
                    {
                        str = oldT.GetClearText(identity, merchSecret);
                        md5 = Utils.MD5(str);                        
                        if (!verifiction.Equals(md5, StringComparison.OrdinalIgnoreCase))
                        {
                            LogHelper.SaveLog(t.GetType().Name);
                            LogHelper.SaveLog(str);
                            LogHelper.SaveLog(md5);
                            throw new Exception("数据校验失败");
                        }
                    }

                    //更新校验码                    
                    str = t.GetClearText(identity, merchSecret);
                    md5 = Utils.MD5(str);
                    //LogHelper.SaveLog(str);
                    //LogHelper.SaveLog(md5);
                    t.GetType().GetProperty("Verifiction").SetValue(t, md5, null);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public BaseDAL()
        {
            dbContext = DbContextFactory.CreateByModelNamespace(typeof(T).Namespace);
            dbContext.Configuration.UseDatabaseNullSemantics = true;
        }

        public BaseDAL(string containerName)
        {
            this.dbContextName = containerName;
            dbContext = DbContextFactory.CreateByContainerName(containerName);
            dbContext.Configuration.UseDatabaseNullSemantics = true;
        }

        private object GetEntityInDatabase(T entity)
        {
            var objContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            var objSet = objContext.CreateObjectSet<T>();
            var entityKey = objContext.CreateEntityKey(objSet.EntitySet.Name, entity);

            //第一次从缓存获取
            Object foundEntity = null;
            var exists = objContext.TryGetObjectByKey(entityKey, out foundEntity);

            if (exists)
            {
                //清除缓存里的对象
                objContext.Detach(foundEntity);

                //第二次从数据库获取
                exists = objContext.TryGetObjectByKey(entityKey, out foundEntity);                
            }

            return foundEntity;
        }

        //用于监测Context中的Entity是否存在，如果存在，将其Detach，防止出现问题。
        private bool RemoveHoldingEntityInContext(T entity)
        {
            var objContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            var objSet = objContext.CreateObjectSet<T>();
            var entityKey = objContext.CreateEntityKey(objSet.EntitySet.Name, entity);

            //第一次从缓存获取
            Object foundEntity = null;
            var exists = objContext.TryGetObjectByKey(entityKey, out foundEntity);

            if (exists)
            {
                //清除缓存里的对象
                objContext.Detach(foundEntity);                
            }

            return exists;
        }

        public void AddModel(T t, bool identity = true)
        {
            MakeVerifiction(identity, ref t);

            dbContext.Set<T>().Add(t);
        }
        
        public bool Add(T t, bool identity = true)
        {
            MakeVerifiction(identity, ref t);

            dbContext.Set<T>().Add(t);
            return dbContext.SaveChanges() > 0;
        }

        public bool Update(T t, bool identity = true)
        {
            MakeVerifiction(identity, ref t, (T)GetEntityInDatabase(t));
            RemoveHoldingEntityInContext(t);
            dbContext.Set<T>().Attach(t);
            dbContext.Entry<T>(t).State = EntityState.Modified;
            bool result = dbContext.SaveChanges() > 0;
            return result;
        }

        public void UpdateModel(T t, bool identity = true)
        {
            MakeVerifiction(identity, ref t, (T)GetEntityInDatabase(t));
            RemoveHoldingEntityInContext(t);
            dbContext.Set<T>().Attach(t);
            dbContext.Entry<T>(t).State = EntityState.Modified;
        }


        public bool Delete(T t)
        {
            RemoveHoldingEntityInContext(t);
            dbContext.Set<T>().Attach(t);
            dbContext.Entry<T>(t).State = EntityState.Deleted;
            return dbContext.SaveChanges() > 0;
        }

        public void DeleteModel(T t)
        {
            RemoveHoldingEntityInContext(t);
            dbContext.Set<T>().Attach(t);
            dbContext.Entry<T>(t).State = EntityState.Deleted;
        }


        public int GetCount(Expression<Func<T, bool>> whereLambda)
        {
            return dbContext.Set<T>().Count<T>(whereLambda);
        }

        public bool Any(Expression<Func<T, bool>> whereLambda)
        {
            return dbContext.Set<T>().Any<T>(whereLambda);
        }

        public IQueryable<T> GetModels(Expression<Func<T, bool>> whereLambda)
        {
            return dbContext.Set<T>().AsNoTracking().Where(whereLambda);
        }

        public IQueryable<T> GetModels()
        {
            return dbContext.Set<T>().AsNoTracking().AsQueryable<T>();
        }

        public IQueryable<T> GetModelsByPage<type>(int pageSize, int pageIndex, bool isAsc,
            Expression<Func<T, type>> OrderByLambda, Expression<Func<T, bool>> WhereLambda, out int recordCount)
        {
            recordCount = dbContext.Set<T>().AsNoTracking().Where(WhereLambda).Count<T>(); 
            //是否升序
            if (isAsc)
            {
                return dbContext.Set<T>().AsNoTracking().Where(WhereLambda).OrderBy(OrderByLambda).Skip(pageIndex * pageSize).Take(pageSize);
            }
            else
            {
                return dbContext.Set<T>().Where(WhereLambda).OrderByDescending(OrderByLambda).Skip(pageIndex * pageSize).Take(pageSize);
            }
        }

        public bool SaveChanges()
        {
            return dbContext.SaveChanges() >= 0;
        }

        public int ExecuteSqlCommand(string sql,params object[] parameters)
        {
            return dbContext.Database.ExecuteSqlCommand(sql, parameters);
        }

        public IQueryable<T> SqlQuery(string sql, params object[] parameters)
        {
            return dbContext.Database.SqlQuery<T>(sql, parameters).AsQueryable<T>();
        }

        public IQueryable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            return dbContext.Database.SqlQuery<TElement>(sql, parameters).AsQueryable<TElement>();
        }
    }
}
