using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace BookMeal
{
    public static class NameValueCollectionExtensions
    {
        public static RouteValueDictionary ToRouteValues(this NameValueCollection col, Object obj)
        {
            var values = new RouteValueDictionary(obj);
            if (col != null)
            {
                foreach (string key in col)
                {
                    //values passed in object override those already in collection
                    if (!values.ContainsKey(key)) values[key] = col[key];
                }
            }
            return values;
        }
    }
}