using System;
using System.Collections.Generic;

namespace Lab4.Db;

public partial class Good
{
    public string Art { get; set; } = null!;

    public string? NameG { get; set; }

    public string? Meas { get; set; }

    public decimal? PriceG { get; set; }

    public virtual ICollection<OrdGd> OrdGds { get; set; } = new List<OrdGd>();
}
