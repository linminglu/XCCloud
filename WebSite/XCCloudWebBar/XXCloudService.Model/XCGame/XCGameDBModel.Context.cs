﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudWebBar.Model.XCGame
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class XCGameDBEntities : DbContext
    {
        public XCGameDBEntities()
            : base("name=XCGameDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<t_goods> t_goods { get; set; }
        public virtual DbSet<t_device> t_device { get; set; }
        public virtual DbSet<t_foods> t_foods { get; set; }
        public virtual DbSet<flw_food_sale> flw_food_sale { get; set; }
        public virtual DbSet<flw_schedule> flw_schedule { get; set; }
        public virtual DbSet<t_memberlevel> t_memberlevel { get; set; }
        public virtual DbSet<u_users> u_users { get; set; }
        public virtual DbSet<flw_checkdate> flw_checkdate { get; set; }
        public virtual DbSet<flw_checkdate_schedule> flw_checkdate_schedule { get; set; }
        public virtual DbSet<t_workstation> t_workstation { get; set; }
        public virtual DbSet<flw_485_coin> flw_485_coin { get; set; }
        public virtual DbSet<flw_485_savecoin> flw_485_savecoin { get; set; }
        public virtual DbSet<flw_project_buy> flw_project_buy { get; set; }
        public virtual DbSet<t_project> t_project { get; set; }
        public virtual DbSet<t_game> t_game { get; set; }
        public virtual DbSet<t_head> t_head { get; set; }
        public virtual DbSet<flw_game_free> flw_game_free { get; set; }
        public virtual DbSet<t_game_free_rule> t_game_free_rule { get; set; }
        public virtual DbSet<t_push_rule> t_push_rule { get; set; }
        public virtual DbSet<flw_project_buy_codelist> flw_project_buy_codelist { get; set; }
        public virtual DbSet<flw_lottery> flw_lottery { get; set; }
        public virtual DbSet<t_parameters> t_parameters { get; set; }
        public virtual DbSet<t_member> t_member { get; set; }
        public virtual DbSet<flw_ticket_exit> flw_ticket_exit { get; set; }
        public virtual DbSet<flw_project_play> flw_project_play { get; set; }
    }
}
