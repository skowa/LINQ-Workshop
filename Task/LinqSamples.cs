// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
		public void Linq1()
		{
			int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

			var lowNums =
				from num in numbers
				where num < 5
				select num;

			Console.WriteLine("Numbers < 5:");
			foreach (var x in lowNums)
			{
				Console.WriteLine(x);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("This sample return return all presented in market products")]
		public void Linq2()
		{
			var products =
				from p in dataSource.Products
				where p.UnitsInStock > 0
				select p;

			foreach (var p in products)
			{
				ObjectDumper.Write(p);
			}
		}

        [Category("Restriction Operators")]
        [Title("Where - Task 3")]
        [Description("This sample return customers living in London")]
        public void Linq3()
        {
            var customers = from c in dataSource.Customers
                where c.City == "London"
                select c;

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task1")]
        [Description("Customers with orders sum higher than some value")]
        public void Linq4()
        {
            decimal summary = 4107.6m;

            var customers = from c in dataSource.Customers
                where c.Orders.Sum(od => od.Total) > summary
                select new
                {
                    Id = c.CustomerID,
                    OrdersSum = c.Orders.Sum(od => od.Total)
                };

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task2")]
        [Description("Customers and cities with the same city")]
        public void Linq5()
        {
            var customersSuppliers = from c in dataSource.Customers
                join s in dataSource.Suppliers on new {c.Country, c.City} equals new {s.Country, s.City}
                select new
                {
                    Id = c.CustomerID,
                    Name = c.CompanyName,
                    c.Country,
                    c.City,
                    s.SupplierName,
                    SCountry = s.Country,
                    SCity = s.City
                };

            foreach (var cs in customersSuppliers)
            {
                ObjectDumper.Write(cs);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task3")]
        [Description("Customers and orders greater than exact sum")]
        public void Linq6()
        {
            decimal exactSum = 1000m;

            var customers = from c in dataSource.Customers
                where c.Orders.Any(o => o.Total > exactSum)
                select c;

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task4")]
        [Description("Customers and their first month")]
        public void Linq7()
        {
            var customers = from c in dataSource.Customers
                where c.Orders.Length > 0
                select new
                {
                    c.CustomerID,
                    FirstYear = c.Orders.Min(o => o.OrderDate).Year,
                    FirstMonth = c.Orders.Min(o => o.OrderDate).Month
                };

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task5")]
        [Description("Customers and their first month sorted")]
        public void Linq8()
        {
            var customers = (from c in dataSource.Customers
                where c.Orders.Length > 0
                select new
                {
                    c.CompanyName,
                    FirstYear = c.Orders.Min(o => o.OrderDate).Year,
                    FirstMonth = c.Orders.Min(o => o.OrderDate).Month,
                    OrdersSum = c.Orders.Sum(od => od.Total)
                }).OrderBy(c => c.FirstYear).ThenBy(c => c.FirstMonth).ThenByDescending(c => c.OrdersSum).ThenBy(c => c.CompanyName);

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task6")]
        [Description("Customers with not dogot postal code, or null region or without phone's operator's code")]
        public void Linq9()
        {
            var customers = from c in dataSource.Customers
                where c.PostalCode == null || c.PostalCode.Any(p => !char.IsDigit(p)) ||
                      string.IsNullOrEmpty(c.Region) || !c.Phone.StartsWith("(")
                select c;

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task7")]
        [Description("Group products by categories, then by availability in the stock, then by price")]
        public void Linq10()
        {
            var productsGroups = from p in dataSource.Products
                group p by p.Category
                into grpCat
                select new
                {
                    Category = grpCat.Key,
                    ProductsByAvailability =
                        (from g in grpCat
                            group g by g.UnitsInStock > 0
                            into grpAvailable
                            select new
                            {
                                Availability = grpAvailable.Key,
                                Products = grpAvailable.OrderBy(p => p.UnitPrice * p.UnitsInStock)
                            })
                };

            foreach (var c in productsGroups)
            {
                ObjectDumper.Write($"Category: {c.Category}\n");
                foreach (var p in c.ProductsByAvailability)
                {
                    ObjectDumper.Write($"Availability: {p.Availability}\n");
                    foreach (var product in p.Products)
                    {
                        ObjectDumper.Write(product);
                    }
                }

                ObjectDumper.Write("\n");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task8")]
        [Description("Products grouped by price")]
        public void Linq11()
        {
            var productsGroups = from p in dataSource.Products
                group p by p.UnitPrice > 45 ? "Expensive" : p.UnitPrice > 15 ? "Normal" : "Cheap"
                into priceGroup
                select new
                {
                    Group = priceGroup.Key,
                    Products = priceGroup
                };

            foreach (var productsGroup in productsGroups)
            {
                ObjectDumper.Write($"Group: {productsGroup.Group}");
                foreach (var product in productsGroup.Products)
                {
                    ObjectDumper.Write($"{product.ProductName} - price: {product.UnitPrice}");
                }

                ObjectDumper.Write("\n");
            }
        }

        [Category("Restriction Operators")]
        [Title("Task9")]
        [Description("Average city profitability and intensity")]
        public void Linq12()
        {
            var cities = from c in dataSource.Customers
                group c by c.City
                into grpCities
                select new
                {
                    City = grpCities.Key,
                    Profitability = grpCities.Average(g => g.Orders.Sum(o => o.Total)),
                    Intensity = grpCities.Average(g => g.Orders.Length)
                };

            foreach (var city in cities)
            {
                ObjectDumper.Write(city);
            }
        }

        [Category("Restriction Operators")]
        [Title("Task10")]
        [Description("Customers activity statistics by year, by month and by both at the same time")]
        public void Linq13()
        {
            var orders = (from c in dataSource.Customers
                from o in c.Orders
                select new
                {
                    c.CustomerID,
                    o.OrderDate
                }).ToList();

            var monthStatistics = from order in (from o in orders
                    group o by new {o.CustomerID, o.OrderDate.Month}
                    into grp
                    select new
                    {
                        grp.Key.Month,
                        Count = grp.Count()
                    })
                group order by order.Month
                into monthGroup
                orderby monthGroup.Key
                select new
                {
                    monthGroup.Key,
                    AverageActivity = monthGroup.Average(g => g.Count)
                };

            var yearStatistics = from order in (from o in orders
                    group o by new {o.CustomerID, o.OrderDate.Year}
                    into grp
                    select new
                    {
                        grp.Key.Year,
                        Count = grp.Count()
                    })
                group order by order.Year
                into yearGroup
                orderby yearGroup.Key
                select new
                {
                    yearGroup.Key,
                    AverageActivity = yearGroup.Average(g => g.Count)
                };

            var monthYearStatistics = from order in (from o in orders
                    group o by new {o.CustomerID, o.OrderDate.Year, o.OrderDate.Month}
                    into grp
                    select new
                    {
                        grp.Key.Year,
                        grp.Key.Month,
                        Count = grp.Count()
                    })
                group order by new {order.Year, order.Month}
                into monthYearGroup
                orderby monthYearGroup.Key.Year, monthYearGroup.Key.Month
                select new
                {
                    monthYearGroup.Key.Year,
                    monthYearGroup.Key.Month,
                    AverageActivity = monthYearGroup.Average(g => g.Count)
                };

            ObjectDumper.Write("Month statistics: \n");
            foreach (var month in monthStatistics)
            {
                ObjectDumper.Write(month);
            }

            ObjectDumper.Write("\nYear statistics: \n");
            foreach (var year in yearStatistics)
            {
                ObjectDumper.Write(year);
            }

            ObjectDumper.Write("\nMonth and year statistics: \n");
            foreach (var monthYear in monthYearStatistics)
            {
                ObjectDumper.Write(monthYear);
            }
        }
    }
}
