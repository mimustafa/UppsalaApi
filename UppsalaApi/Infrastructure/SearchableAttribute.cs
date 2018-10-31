﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UppsalaApi.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SearchableAttribute : Attribute
    {
        public ISearchExpressionProvider ExpressionProvider { get; set; }
            = new DefaultSearchExpressionProvider();
    }
}
