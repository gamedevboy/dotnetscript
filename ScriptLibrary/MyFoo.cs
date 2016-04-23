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

        public delegate void TestDelegate();

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

            OnDelegateTest();
        }

        void Test1(ref int a)
        {
            a = 20;
        }

        void invokeTarget()
        {
            Console.WriteLine("Hello user delegate!");
        }

        public int Test2(int a)
        {
            int c = 100;
            return value + a;
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
