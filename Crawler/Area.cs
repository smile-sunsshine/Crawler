// ********************************************
// 作者：洪根祥
// 时间：2017-10-09 17:02:54
// ********************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace Crawler
{
    public class Area
    {
        private const string BaseUrl = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2016";

        private const string ProvinceUrl = "/index.html";

        private const string CityUrl = "/32.html";

        public List<AreaUnit> CollectProvince()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var doc = new HtmlWeb().LoadFromWebAsync(BaseUrl + ProvinceUrl, Encoding.GetEncoding("gb2312")).Result;
            return doc.DocumentNode.SelectNodes("//tr/td/a").Select(c => new AreaUnit()
            {
                Code = c.GetAttributeValue("href", string.Empty).TrimEnd(".html".ToArray()),
                Name = c.InnerText,
                Level = Level.Province
            }).ToList();
        }


        public List<AreaUnit> CollectCity(List<AreaUnit> provinces)
        {
            var cities = new List<AreaUnit>();
            foreach (var province in provinces)
            {
                var doc = new HtmlWeb().LoadFromWebAsync(BaseUrl + CityUrl, Encoding.GetEncoding("gb2312")).Result;
                var result = doc.DocumentNode.SelectNodes("//table/table/tr").Select(
                       c => new AreaUnit()
                       {
                           Code = c.ChildNodes[0].InnerText,
                           Name = c.ChildNodes[1].InnerText,
                           Level = Level.City
                       }).ToList();
                cities.AddRange(result);
            }
            return cities;
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
        /// 未设置
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


    }
}