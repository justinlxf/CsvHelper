﻿using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvHelper.Tests.Mappings.ConstructorParameter
{
	[TestClass]
    public class DateTimeStylesAttributeTests
	{
		private const string DATE = "12/25/2020";
		private readonly DateTimeOffset date = DateTimeOffset.Parse(DATE);

		[TestMethod]
		public void AutoMap_WithCultureInfoAttributes_ConfiguresParameterMaps()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture);
			var map = config.AutoMap<Foo>();

			Assert.AreEqual(2, map.ParameterMaps.Count);
			Assert.IsNull(map.ParameterMaps[0].Data.TypeConverterOptions.DateTimeStyle);
			Assert.AreEqual(DateTimeStyles.AllowLeadingWhite, map.ParameterMaps[1].Data.TypeConverterOptions.DateTimeStyle);
		}

		[TestMethod]
		public void GetRecords_WithCultureInfoAttributes_HasHeader_CreatesRecords()
		{
			var rows = new Queue<string[]>(new List<string[]>
			{
				new [] { "id", "date" },
				new [] { "1", $" {DATE}" },
				null
			});
			using (var parser = new ParserMock(rows))
			using (var csv = new CsvReader(parser))
			{
				var records = csv.GetRecords<Foo>().ToList();

				Assert.AreEqual(1, records.Count);
				Assert.AreEqual(1, records[0].Id);
				Assert.AreEqual(date, records[0].Date);
			}
		}

		[TestMethod]
		public void GetRecords_WithCultureInfoAttributes_NoHeader_CreatesRecords()
		{
			var rows = new Queue<string[]>(new List<string[]>
			{
				new [] { "1", $" {DATE}" },
				null
			});
			using (var parser = new ParserMock(rows))
			using (var csv = new CsvReader(parser))
			{
				csv.Configuration.HasHeaderRecord = false;

				var records = csv.GetRecords<Foo>().ToList();

				Assert.AreEqual(1, records.Count);
				Assert.AreEqual(1, records[0].Id);
				Assert.AreEqual(date, records[0].Date);
			}
		}

		[TestMethod]
		public void WriteRecords_WithCultureInfoAttributes_DoesntUseParameterMaps()
		{
			var records = new List<Foo>
			{
				new Foo(1, date),
			};

			using (var serializer = new SerializerMock())
			using (var csv = new CsvWriter(serializer))
			{
				csv.WriteRecords(records);

				Assert.AreEqual(2, serializer.Records.Count);

				Assert.AreEqual("Id", serializer.Records[0][0]);
				Assert.AreEqual("Date", serializer.Records[0][1]);

				Assert.AreEqual("1", serializer.Records[1][0]);
				Assert.AreEqual(date.ToString(null, CultureInfo.InvariantCulture), serializer.Records[1][1]);
			}
		}

		private class Foo
		{
			public int Id { get; private set; }

			public DateTimeOffset Date { get; private set; }

			public Foo(int id, [DateTimeStyles(DateTimeStyles.AllowLeadingWhite)] DateTimeOffset date)
			{
				Id = id;
				Date = date;
			}
		}
	}
}
