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
    
    public partial class LOAI_DICH
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LOAI_DICH()
        {
            this.GIAO_DICH = new HashSet<GIAO_DICH>();
        }
    
        public string COD_LOAI_DICH { get; set; }
        public string TEN_LOAI_DICH { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GIAO_DICH> GIAO_DICH { get; set; }
    }
}