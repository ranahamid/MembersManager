//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class ConditionSet
    {
        public int Id { get; set; }
        public int SegmentId { get; set; }
        public string ColumnName { get; set; }
        public string FilterId { get; set; }
        public string SearchTerm { get; set; }
    
        public virtual Segment Segment { get; set; }
    }
}
