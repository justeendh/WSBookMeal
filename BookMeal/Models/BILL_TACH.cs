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
    
    public partial class BILL_TACH
    {
        public Nullable<System.Guid> KEY_HOA_DON { get; set; }
        public System.Guid KEY_BILL_TACH { get; set; }
        public Nullable<short> COD_BILL_TACH { get; set; }
        public Nullable<bool> USE_BILL_TACH { get; set; }
    
        public virtual HOA_DON HOA_DON { get; set; }
    }
}
