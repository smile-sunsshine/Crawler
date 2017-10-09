using System;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            var provices = new Area().CollectProvince();
            //foreach (var provice in provices)
            //{
            //    Console.WriteLine($" 编码:{provice.Code} 名称:{provice.Name}");
            //}

            var cities = new Area().CollectCity(provices);
            foreach (var city in cities)
            {
                Console.WriteLine($" 编码:{city.Code} 名称:{city.Name}");
            }

            Console.WriteLine("处理完成");
            Console.ReadKey();
        }
    }
}
