using Ivy.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.NavigationBacon
{
    public class NavigationBeacon<T>
    {
        public Func<T, NavigateArgs> ArgsBuilder { get; }
        public NavigationBeacon(Func<T, NavigateArgs> argsBuilder)
        {
            this.ArgsBuilder = argsBuilder;
        }
    }
}
