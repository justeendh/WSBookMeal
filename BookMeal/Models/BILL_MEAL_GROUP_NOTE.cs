using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookMeal.Models
{
    public class BILL_MEAL_GROUP_NOTE
    { 
        public Guid KEY_MON_HANG { set; get; }
        public List<BILL_MEAL_GROUP_NOTE_ITEM> LIST_NOTE_QUANTITY { set; get; }
    }

    public class BILL_MEAL_GROUP_NOTE_ITEM
    {
        public string NOT_COOK_MIX { set; get; }
        public decimal SO_LUONG_MON { set; get; }
        public bool? COOK_VA_MIXE { set; get; }
    }
}