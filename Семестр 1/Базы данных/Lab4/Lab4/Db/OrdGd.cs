using System;
using System.Collections.Generic;

namespace Lab4.Db;

public partial class OrdGd
{
    public string CodeZ { get; set; } = null!;

    public string Art { get; set; } = null!;

    public int? Qt { get; set; }

    public virtual Good ArtNavigation { get; set; } = null!;

    public virtual ClientOrder CodeZNavigation { get; set; } = null!;
}
