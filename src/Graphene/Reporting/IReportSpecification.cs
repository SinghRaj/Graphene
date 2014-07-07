﻿using System;
using System.Collections.Generic;
using System.Linq;
using Graphene.Util;

namespace Graphene.Reporting
{
    public enum ReportResolution
    {
        Minute = 0,
        Hour = 1,
        Day = 2,
        Month = 3,
        Year = 4
    }

    public interface IReportSpecification
    {
        IEnumerable<IFilterConditions> FilterCombinations { get; }

        IEnumerable<IMeasurement> Counters { get; }

        DateTime FromDateUtc { get; }

        DateTime ToDateUtc { get; }

        IEnumerable<string> TypeNames { get; }

        ReportResolution Resolution { get; }
    }

    public interface IFilterConditions
    {
        IEnumerable<string> Filters { get; }
    }
}