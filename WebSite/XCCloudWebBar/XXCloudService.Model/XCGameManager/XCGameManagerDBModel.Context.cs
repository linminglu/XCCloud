﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudWebBar.Model.XCGameManager
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class XCGameManagerDBEntities : DbContext
    {
        public XCGameManagerDBEntities()
            : base("name=XCGameManagerDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<t_MPOrder> t_MPOrder { get; set; }
        public virtual DbSet<t_UserRegister> t_UserRegister { get; set; }
        public virtual DbSet<t_promotion> t_promotion { get; set; }
        public virtual DbSet<T_MemberToken> T_MemberToken { get; set; }
        public virtual DbSet<Data_Order> Data_Order { get; set; }
        public virtual DbSet<t_device> t_device { get; set; }
        public virtual DbSet<t_ticket> t_ticket { get; set; }
        public virtual DbSet<t_MobileToken> t_MobileToken { get; set; }
        public virtual DbSet<t_user> t_user { get; set; }
        public virtual DbSet<t_AdminUser> t_AdminUser { get; set; }
        public virtual DbSet<t_apiRequestLog> t_apiRequestLog { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<t_Enum> t_Enum { get; set; }
        public virtual DbSet<t_store_dog> t_store_dog { get; set; }
        public virtual DbSet<t_store> t_store { get; set; }
        public virtual DbSet<t_usertoken> t_usertoken { get; set; }
    }
}
