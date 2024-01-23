using System;
using System.Collections.Generic;

namespace Lab4.Db;

public partial class ClientOrder
{
    public string CodeZ { get; set; } = null!;

    public int? CodeC { get; set; }

    public DateOnly? AccDt { get; set; }

    public string? AddrD { get; set; }

    public DateOnly? DelDt { get; set; }

    public decimal? PriceD { get; set; }

    public virtual Client? CodeCNavigation { get; set; }

    public virtual ICollection<OrdGd> OrdGds { get; set; } = new List<OrdGd>();
}
