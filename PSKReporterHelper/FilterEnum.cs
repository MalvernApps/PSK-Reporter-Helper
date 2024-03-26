
using System;
using System.ComponentModel;

namespace PSKReporterHelper
{
    public enum FilterEnum
    {
        fifteen = 15,
        twenty = 20,
        thirty = 30,

		    [Description("One Hour")]
		    hour = 60,
        two_hour = 120,
        six_hour = (6*60),
        Twelve_Hour = (12 * 60),
        lastday = (24 * 60)
    }

}