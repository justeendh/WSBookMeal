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
    
    public partial class PUTIN_OUT
    {
        public Nullable<System.Guid> KEY_NHAP_XUAT { get; set; }
        public Nullable<System.Guid> KEY_KHO_HANG { get; set; }
        public Nullable<System.Guid> KEY_KHO_MOVE { get; set; }
        public Nullable<System.Guid> KEY_MAT_HANG { get; set; }
        public System.Guid KEY_PUTIN_OUT { get; set; }
        public Nullable<short> COD_ROW_XLIN { get; set; }
        public Nullable<System.Guid> KEY_CHI_MUC { get; set; }
        public Nullable<System.Guid> KEY_HANG_FIFO { get; set; }
        public string SOLO_SAN_XUAT { get; set; }
        public string THOI_HAN_DUNG { get; set; }
        public Nullable<System.DateTime> NGAY_DEN_HAN { get; set; }
        public Nullable<decimal> SO_LUONG_HANG { get; set; }
        public Nullable<decimal> DON_GIA_HANG { get; set; }
        public Nullable<decimal> TIEN_VIET_HANG { get; set; }
        public Nullable<decimal> SO_CHIET_KHAU { get; set; }
        public Nullable<decimal> TIEN_CHIET_KHAU { get; set; }
        public Nullable<decimal> TIEN_SAU_KHAU { get; set; }
        public Nullable<decimal> HE_SO_THUE { get; set; }
        public Nullable<decimal> GIA_SAU_THUE { get; set; }
        public Nullable<decimal> TIEN_VIET_THUE { get; set; }
        public Nullable<decimal> TIEN_VIET_NAM { get; set; }
        public string GHI_DIEN_GIAI { get; set; }
    
        public virtual KHO_HANG KHO_HANG { get; set; }
        public virtual MON_HANG MON_HANG { get; set; }
        public virtual NHAP_XUAT NHAP_XUAT { get; set; }
    }
}
