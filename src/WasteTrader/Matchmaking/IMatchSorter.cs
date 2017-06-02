﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using WasteTrader.Database;

namespace WasteTrader.Matchmaking
{
    public interface IMatchSorter
    {
        IWaste[] Sort(IEnumerable<IWaste> waste);
    }
}
