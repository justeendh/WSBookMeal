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
    
    public partial class CHUNG_TU
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CHUNG_TU()
        {
            this.NHAP_XUAT = new HashSet<NHAP_XUAT>();
        }
    
        public Nullable<short> IDX_CHUNG_TU { get; set; }
        public string MA_NGHIEP_VU { get; set; }
        public System.Guid KEY_CHUNG_TU { get; set; }
        public string MA_CHUNG_TU { get; set; }
        public string TEN_CHUNG_TU { get; set; }
        public Nullable<short> NUM_CHUNG_TU { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NHAP_XUAT> NHAP_XUAT { get; set; }
    }
}