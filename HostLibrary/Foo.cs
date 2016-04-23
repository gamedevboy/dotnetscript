using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostLibrary
{
    public class Foo<TS>
    {
        public event EventHandler OnTest;

        public virtual void VirtualTest(string name)
        {
            OnTest?.Invoke(this, null);
        }

        public TS Call<T>(T @in)
        {
            return default(TS);
        }

        public int Test(ref int a)
        {
            a += 10;
            return a;
        }
    }
}
