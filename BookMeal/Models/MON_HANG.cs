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
    
    public partial class MON_HANG
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MON_HANG()
        {
            this.BILL_MEAL = new HashSet<BILL_MEAL>();
            this.PUTIN_OUT = new HashSet<PUTIN_OUT>();
        }
    
        public Nullable<System.Guid> KEY_NHOM_MON { get; set; }
        public System.Guid KEY_MON_HANG { get; set; }
        public string MA_MON_HANG { get; set; }
        public string TEN_MON_HANG { get; set; }
        public string DON_VI_TINH { get; set; }
        public Nullable<short> LOAI_PHA_NAU { get; set; }
        public Nullable<decimal> DON_GIA_MUC1 { get; set; }
        public Nullable<decimal> DON_GIA_MUC2 { get; set; }
        public Nullable<decimal> DON_GIA_MUC3 { get; set; }
        public Nullable<short> MUC_GIAM_KHAU { get; set; }
        public Nullable<bool> MON_MAC_DINH { get; set; }
        public Nullable<decimal> DON_GIA_NHAP { get; set; }
        public Nullable<short> TYP_MON_HANG { get; set; }
        public Nullable<bool> CO_SU_DUNG { get; set; }
        public Nullable<System.Guid> KEY_KHO_HANG { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BILL_MEAL> BILL_MEAL { get; set; }
        public virtual NHOM_MON NHOM_MON { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PUTIN_OUT> PUTIN_OUT { get; set; }
    }
}