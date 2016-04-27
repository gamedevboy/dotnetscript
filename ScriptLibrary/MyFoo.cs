using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HostLibrary;

namespace ScriptLibrary
{
    public class MyFoo : Foo<int>
    {
        int value = 10;
        int[,] values = new int[10,10];

        public enum Types
        {
            None = 0,
            Type1 = 1 << 0,
            Type2 = 1 << 2,
            All = Type1 | Type2
        }

        private Types _type;

        public delegate int TestDelegate();

        public event EventHandler<EventArgs> OnUserTest;

        private TestDelegate OnDelegateTest;

        private void MyFoo_OnTest(object sender, EventArgs e)
        {
            Console.WriteLine("Hello System Delegate !");
        }

        public override void VirtualTest(string name)
        {
            base.VirtualTest(name);
        }

        public MyFoo()
        {
            Console.WriteLine("Hello Script World !");
            Test1(ref value);
            Test4<int>();

            OnTest += MyFoo_OnTest;
            OnDelegateTest = invokeTarget;
            OnUserTest += MyFoo_OnTest1;

            OnDelegateTest();
            values[0,0] = 100;

            _type = Types.All;
        }

        private void MyFoo_OnTest1(object sender, EventArgs e)
        {
            Console.WriteLine("Hello event !");
        }

        void Test1(ref int a)
        {
            a = 20;
        }

        int invokeTarget()
        {
            Console.WriteLine("Hello user delegate!");
            return value;
        }

        public int Test2(int a)
        {
            int c = 100;
            return OnDelegateTest() + a;
        }

        public static int Test3(int a)
        {
            return a*a;
        }

        public static T Test4<T>() where T : new()
        {
            return new T();
        }
    }
}
