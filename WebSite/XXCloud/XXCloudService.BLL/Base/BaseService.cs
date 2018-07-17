﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL.IDAL;

namespace XCCloudService.BLL.Base
{
    public abstract partial class BaseService<T> where T : class, new()
    {
        protected string containerName;

        public BaseService()
        {
            SetDal();
        }
 
        public IBaseDAL<T> Dal{get;set;}
 
        public abstract void SetDal();

        public void AddModel(T t, bool syncData = false, string merchId = "", string merchSecret = "")
        {
            Dal.AddModel(t, syncData, merchId, merchSecret);
        }
        public void UpdateModel(T t, bool syncData = false, string merchId = "", string merchSecret = "")
        {
            Dal.UpdateModel(t, syncData, merchId, merchSecret);
        }
        public void DeleteModel(T t, bool syncData = false, string merchId = "", string merchSecret = "")
        {
            Dal.DeleteModel(t, syncData, merchId, merchSecret);
        }
        public bool Add(T t, bool syncData = false, string merchId = "", string merchSecret = "")
        {
            return Dal.Add(t, syncData, merchId, merchSecret);
        }
        public bool Update(T t, bool syncData = false, string merchId = "", string merchSecret = "")
        {
            return Dal.Update(t, syncData, merchId, merchSecret);
        }
        public bool Delete(T t, bool syncData = false, string merchId = "", string merchSecret = "")
        {
            return Dal.Delete(t, syncData, merchId, merchSecret);
        }
        public bool SaveChanges()
        {
            return Dal.SaveChanges();
        }
        public int GetCount(Expression<Func<T, bool>> whereLambda)
        {
            return Dal.GetCount(whereLambda);
        }
        public bool Any(Expression<Func<T, bool>> whereLambda)
        {
            return Dal.Any(whereLambda);
        }
        public IQueryable<T> GetModels(Expression<Func<T, bool>> whereLambda)
        {
            return Dal.GetModels(whereLambda);
        }

        public IQueryable<T> GetModels()
        {
            return Dal.GetModels();
        }
 
        public IQueryable<T> GetModelsByPage<type>(int pageSize, int pageIndex, bool isAsc,
            Expression<Func<T, type>> OrderByLambda, Expression<Func<T, bool>> WhereLambda, out int recordCount)
        {
            return Dal.GetModelsByPage(pageSize, pageIndex, isAsc, OrderByLambda, WhereLambda,out recordCount);
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return Dal.ExecuteSqlCommand(sql,parameters);
        }

        public IQueryable<T> SqlQuery(string sql ,params object[] parameters)
        {
            return Dal.SqlQuery(sql, parameters);
        }

        public IQueryable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            return Dal.SqlQuery<TElement>(sql, parameters);
        }
    }
}
