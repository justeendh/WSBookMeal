//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BookMeal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PHONG_BAN
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PHONG_BAN()
        {
            this.NHAN_VIEN = new HashSet<NHAN_VIEN>();
        }
    
        public System.Guid KEY_PHONG_BAN { get; set; }
        public string MA_PHONG_BAN { get; set; }
        public string TEN_PHONG_BAN { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NHAN_VIEN> NHAN_VIEN { get; set; }
    }
}