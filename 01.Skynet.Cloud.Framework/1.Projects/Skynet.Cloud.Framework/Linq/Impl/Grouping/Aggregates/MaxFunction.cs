﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWay.Skynet.Cloud.Linq.Impl.Grouping.Aggregates
{
    public class MaxFunction : EnumerableSelectorAggregateFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxFunction"/> class.
        /// </summary>
        public MaxFunction()
        {
        }

        /// <summary>
        /// Gets the the Max method name.
        /// </summary>
        /// <value><c>Max</c>.</value>
        public override string AggregateMethodName
        {
            get
            {
                return "Max";
            }
        }
    }
}
