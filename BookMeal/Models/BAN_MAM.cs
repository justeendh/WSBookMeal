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
    
    public partial class BAN_MAM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BAN_MAM()
        {
            this.HOA_DON = new HashSet<HOA_DON>();
        }
    
        public System.Guid KEY_BAN_MAM { get; set; }
        public string MA_BAN_MAM { get; set; }
        public string TEN_BAN_MAM { get; set; }
        public string MA_KHU_VUC { get; set; }
        public Nullable<bool> DA_CO_KHACH { get; set; }
        public Nullable<System.Guid> KEY_HOA_DON { get; set; }
        public Nullable<bool> GIU_BAN_MAM { get; set; }
        public Nullable<System.DateTime> GIO_BAN_MAM { get; set; }
    
        public virtual KHU_VUC KHU_VUC { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HOA_DON> HOA_DON { get; set; }
    }
}