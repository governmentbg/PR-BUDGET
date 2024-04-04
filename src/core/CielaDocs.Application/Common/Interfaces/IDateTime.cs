using System;
using System.Collections.Generic;
using System.Text;

namespace CielaDocs.Application.Common.Interfaces
{
    public interface IDateTime
    {
        DateTime NowUtc { get; }
    }
}
