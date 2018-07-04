﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.BLL.IBLL
{
    public partial interface IBaseService<T> where T : class ,new()
    {
        void AddModel(T t, bool identity = true);
        void UpdateModel(T t, bool identity = true);
        void DeleteModel(T t);
        bool Add(T t, bool identity = true);
        bool Update(T t, bool identity = true);
        bool Delete(T t);       
        bool SaveChanges();
        int GetCount(Expression<Func<T, bool>> whereLambda);
        bool Any(Expression<Func<T, bool>> whereLambda);
        IQueryable<T> GetModels(Expression<Func<T, bool>> whereLambda);
        IQueryable<T> GetModels();
        IQueryable<T> GetModelsByPage<type>(int pageSize, int pageIndex, bool isAsc, Expression<Func<T, type>> OrderByLambda, Expression<Func<T, bool>> WhereLambda, out int recordCount);
        int ExecuteSqlCommand(string sql,params object[] parameters);
        IQueryable<T> SqlQuery(string sql,params object[] parameters);
        IQueryable<TElement> SqlQuery<TElement>(string sql, params object[] parameters);
    }
}
