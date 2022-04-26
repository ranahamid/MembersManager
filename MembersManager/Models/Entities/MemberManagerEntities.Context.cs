﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MembersManager.Models.Entities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class MemberManagerEntities : DbContext
    {
        public MemberManagerEntities()
            : base("name=MemberManagerEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<BoardMember> BoardMembers { get; set; }
        public virtual DbSet<ConditionSet> ConditionSets { get; set; }
        public virtual DbSet<ExternalMember> ExternalMembers { get; set; }
        public virtual DbSet<MemberImportLog> MemberImportLogs { get; set; }
        public virtual DbSet<PrimaryMember> PrimaryMembers { get; set; }
        public virtual DbSet<Recipient> Recipients { get; set; }
        public virtual DbSet<RecipientSegment> RecipientSegments { get; set; }
        public virtual DbSet<Segment> Segments { get; set; }
        public virtual DbSet<UnionMember> UnionMembers { get; set; }
        public virtual DbSet<ProfileDetail> ProfileDetails { get; set; }
        public virtual DbSet<Profile> Profiles { get; set; }
    
        public virtual int UpdateProfileDetail(Nullable<int> segmentId)
        {
            var segmentIdParameter = segmentId.HasValue ?
                new ObjectParameter("SegmentId", segmentId) :
                new ObjectParameter("SegmentId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("UpdateProfileDetail", segmentIdParameter);
        }
    
        public virtual int UpdateProfileDetails(Nullable<int> segmentId)
        {
            var segmentIdParameter = segmentId.HasValue ?
                new ObjectParameter("SegmentId", segmentId) :
                new ObjectParameter("SegmentId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("UpdateProfileDetails", segmentIdParameter);
        }
    
        public virtual int UpdateRecipientSegments(Nullable<int> segmentId)
        {
            var segmentIdParameter = segmentId.HasValue ?
                new ObjectParameter("SegmentId", segmentId) :
                new ObjectParameter("SegmentId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("UpdateRecipientSegments", segmentIdParameter);
        }
    }
}