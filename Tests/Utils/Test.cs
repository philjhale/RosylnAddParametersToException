using System;

namespace Tests.Utils
{
    public class Test
    {
        public void TestMethod()
        {
            try
            {
                Console.WriteLine("stuff");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Things");
            }
        }
    }
}