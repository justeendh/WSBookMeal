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
    
    public partial class CHI_NHANH
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CHI_NHANH()
        {
            this.NHAP_XUAT = new HashSet<NHAP_XUAT>();
        }
    
        public System.Guid KEY_CHI_NHANH { get; set; }
        public string MA_CHI_NHANH { get; set; }
        public string TEN_CHI_NHANH { get; set; }
        public string DIA_CHI_NHANH { get; set; }
        public string SO_DIEN_THOAI { get; set; }
        public Nullable<bool> BAO_CAO_TONG { get; set; }
        public Nullable<bool> CON_HOAT_DONG { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NHAP_XUAT> NHAP_XUAT { get; set; }
    }
}