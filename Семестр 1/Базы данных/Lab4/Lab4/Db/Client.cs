using System;
using System.Collections.Generic;

namespace Lab4.Db;

public partial class Client
{
    public int CodeC { get; set; }

    public string? Fio { get; set; }

    public string? AddrC { get; set; }

    public string? Tel { get; set; }

    public virtual ICollection<ClientOrder> ClientOrders { get; set; } = new List<ClientOrder>();
}
