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
    
    public partial class NHOM_MON
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NHOM_MON()
        {
            this.MON_HANG = new HashSet<MON_HANG>();
        }
    
        public Nullable<System.Guid> KEY_LOAI_MON { get; set; }
        public System.Guid KEY_NHOM_MON { get; set; }
        public string MA_NHOM_MON { get; set; }
        public string TEN_NHOM_MON { get; set; }
        public Nullable<bool> BAN_NHOM_MON { get; set; }
    
        public virtual LOAI_MON LOAI_MON { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MON_HANG> MON_HANG { get; set; }
    }
}