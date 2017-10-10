using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Crawler
{
    class Program
    {
        //镇    东莞市(441900000000) 中山市(442000000000) 儋州市(460400000000)
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var provices = new Area().CollectProvince();
            File.AppendAllLines(Path.Combine(baseDir, "province.txt"), provices.Where(c => c.Level != Level.None).Select(c => c.Name));

            var cities = new Area().CollectCity(provices);
            File.AppendAllLines(Path.Combine(baseDir, "city.txt"), cities.Where(c => c.Level != Level.None).Select(c => c.Name));

            var counties = new Area().CollectCounty(cities);
            File.AppendAllLines(Path.Combine(baseDir, "county.txt"), counties.Where(c => c.Level != Level.None).Select(c => c.Name));

            foreach (var county in counties)
            {
                var towns = new Area().CollectTown(new List<AreaUnit>() { county });
                File.AppendAllLines(Path.Combine(baseDir, "town.txt"), towns.Where(c => c.Level != Level.None).Select(c => c.Name));

                foreach (var town in towns)
                {
                    var villages = new Area().CollectVillage(new List<AreaUnit>() { town });
                    File.AppendAllLines(Path.Combine(baseDir, "village.txt"), villages.Where(c => c.Level != Level.None).Select(c => c.Name));
                }
            }

            Console.WriteLine("处理完成");
            Console.ReadKey();
        }
    }
}
