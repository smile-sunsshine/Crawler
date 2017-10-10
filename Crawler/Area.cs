// ********************************************
// 作者：洪根祥
// 时间：2017-10-09 17:02:54
// ********************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Polly;

namespace Crawler
{
    public class Area
    {

        readonly string[] specialName =
            {"市辖区",
               "省直辖县级行政区划",
            "自治区直辖县级行政区划"
        };

        private const string BaseUrl = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2016";

        private const string ProvinceUrl = "/index.html";


        /// <summary>
        /// 省
        /// </summary>
        /// <returns></returns>
        public List<AreaUnit> CollectProvince()
        {
            var doc = Policy.Handle<Exception>().Retry(5).Execute(() => new HtmlWeb().LoadFromWebAsync(BaseUrl + ProvinceUrl, Encoding.GetEncoding("gb2312")).Result);
            return doc.DocumentNode.SelectNodes("//tr/td/a").Select(c => new AreaUnit()
            {
                Code = c.GetAttributeValue("href", string.Empty).TrimEnd(".html".ToArray()),
                Name = c.InnerText,
                Level = Level.Province
            }).ToList();
        }

        //无县的市  东莞市(441900000000) 中山市(442000000000)
        /// <summary>
        /// 市
        /// </summary>
        /// <param name="provinces"></param>
        /// <returns></returns>
        public List<AreaUnit> CollectCity(List<AreaUnit> provinces)
        {
            var cities = new ConcurrentBag<AreaUnit>();
            Parallel.ForEach(provinces, province =>
            {
                var doc = Policy.Handle<Exception>().Retry(5).Execute(() => new HtmlWeb().LoadFromWebAsync(BaseUrl + $"/{province.Code}.html", Encoding.GetEncoding("gb2312")).Result);
                foreach (var node in doc.DocumentNode.SelectNodes("//tr[@class='citytr']"))
                {
                    var area = new AreaUnit()
                    {
                        Code = node.ChildNodes[0].FirstChild.InnerText,
                        Name = node.ChildNodes[1].FirstChild.InnerText,
                        Level = specialName.Contains(node.ChildNodes[1].FirstChild.InnerText) ? Level.None : Level.City
                    };
                    cities.Add(area);
                    Console.WriteLine($"Level:{area.Level} Code:{area.Code}  Name:{area.Name}");
                }
            });
            return cities.ToList();
        }

        //不存在区的市
        private readonly string[] specialCities = { "东莞市", "中山市", "儋州市" };
        /// <summary>
        /// 区
        /// </summary>
        /// <param name="cities"></param>
        /// <returns></returns>
        public List<AreaUnit> CollectCounty(List<AreaUnit> cities)
        {
            var counties = new ConcurrentBag<AreaUnit>();
            Parallel.ForEach(cities, city =>
            {
                if (specialCities.Contains(city.Name)) return;
                var doc = Policy.Handle<Exception>().Retry(5).Execute(() => new HtmlWeb()
                    .LoadFromWebAsync(BaseUrl + $"/{city.Code.Substring(0, 2)}/{city.Code.Substring(0, 4)}.html",
                        Encoding.GetEncoding("gb2312")).Result);
                foreach (var node in doc.DocumentNode.SelectNodes("//tr[@class='countytr']"))
                {
                    var area = new AreaUnit()
                    {
                        Code = node.ChildNodes[0].FirstChild.InnerText,
                        Name = node.ChildNodes[1].FirstChild.InnerText,
                        Level = specialName.Contains(node.ChildNodes[1].FirstChild.InnerText)
                            ? Level.None
                            : Level.County
                    };
                    counties.Add(area);
                    Console.WriteLine($"Level:{area.Level} Code:{area.Code}  Name:{area.Name}");
                }
            });
            return counties.ToList();
        }


        /// <summary>
        /// 镇
        /// </summary>
        /// <param name="counties"></param>
        /// <returns></returns>
        public List<AreaUnit> CollectTown(List<AreaUnit> counties)
        {
            var towns = new ConcurrentBag<AreaUnit>();
            Parallel.ForEach(counties, county =>
            {
                if (county.Level == Level.None) return;
                var doc = Policy.Handle<Exception>().Retry(5).Execute(() => new HtmlWeb()
                    .LoadFromWebAsync(
                        BaseUrl +
                        $"/{county.Code.Substring(0, 2)}/{county.Code.Substring(2, 2)}/{county.Code.Substring(0, 6)}.html",
                        Encoding.GetEncoding("gb2312")).Result);
                foreach (var node in doc.DocumentNode.SelectNodes("//tr[@class='towntr']"))
                {
                    var area = new AreaUnit()
                    {
                        Code = node.ChildNodes[0].FirstChild.InnerText,
                        Name = node.ChildNodes[1].FirstChild.InnerText,
                        Level = specialName.Contains(node.ChildNodes[1].FirstChild.InnerText) ? Level.None : Level.Town
                    };
                    towns.Add(area);
                    Console.WriteLine($"Level:{area.Level} Code:{area.Code}  Name:{area.Name}");
                }
            });
            return towns.ToList();
        }


        /// <summary>
        /// 村
        /// </summary>
        /// <param name="towns"></param>
        /// <returns></returns>
        public List<AreaUnit> CollectVillage(List<AreaUnit> towns)
        {
            var villages = new ConcurrentBag<AreaUnit>();
            Parallel.ForEach(towns, town =>
            {
                var doc = Policy.Handle<Exception>().Retry(5).Execute(() => new HtmlWeb()
                    .LoadFromWebAsync(
                        BaseUrl +
                        $"/{town.Code.Substring(0, 2)}/{town.Code.Substring(2, 2)}/{town.Code.Substring(4, 2)}/{town.Code.Substring(0, 9)}.html",
                        Encoding.GetEncoding("gb2312")).Result);
                foreach (var node in doc.DocumentNode.SelectNodes("//tr[@class='villagetr']"))
                {
                    var area = new AreaUnit()
                    {
                        Code = node.ChildNodes[0].InnerText,
                        Name = node.ChildNodes[2].InnerText,
                        Level = Level.Village
                    };
                    villages.Add(area);
                    Console.WriteLine($"Level:{area.Level} Code:{area.Code}  Name:{area.Name}");
                }
            });
            return villages.ToList();
        }
    }



    public class AreaUnit
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public Level Level { get; set; }
    }

    public enum Level
    {
        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 省
        /// </summary>
        Province,

        /// <summary>
        /// 市
        /// </summary>
        City,

        /// <summary>
        /// 县
        /// </summary>
        County,

        /// <summary>
        /// 镇
        /// </summary>
        Town,

        /// <summary>
        /// 村
        /// </summary>
        Village


    }
}