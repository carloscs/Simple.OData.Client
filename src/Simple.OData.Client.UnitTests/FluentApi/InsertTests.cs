﻿using System.Dynamic;
using FluentAssertions;
using Xunit;
using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests.FluentApi;

public class InsertTests : TestBase
{
	[Fact]
	public async Task Insert()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = await client
			.For("Products")
			.Set(new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 18m } })
			.InsertEntryAsync();

		product["ProductName"].Should().Be("Test1");
	}

	[Fact]
	public async Task InsertWithValueConversion()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = await client
			.For("Products")
			.Set(new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 18.0d } })
			.InsertEntryAsync();

		product["ProductName"].Should().Be("Test1");
	}

	[Fact]
	public async Task InsertAutogeneratedID()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var product = await client
			.For("Products")
			.Set(new { ProductName = "Test1", UnitPrice = 18m })
			.InsertEntryAsync();

		((int)product["ProductID"] > 0).Should().BeTrue();
		product["ProductName"].Should().Be("Test1");
	}

	[Fact]
	public async Task InsertExpando()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		dynamic expando = new ExpandoObject();
		expando.ProductName = "Test9";
		expando.UnitPrice = 18m;

		var product = await ((Task<IDictionary<string, object>>)client
			.For("Products")
			.Set(expando)
			.InsertEntryAsync());

		((int)product["ProductID"] > 0).Should().BeTrue();
	}

	[Fact]
	public async Task InsertProductWithCategoryByID()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var category = await client
			.For("Categories")
			.Set(new { CategoryName = "Test3" })
			.InsertEntryAsync();
		var product = await client
			.For("Products")
			.Set(new { ProductName = "Test4", UnitPrice = 18m, CategoryID = category["CategoryID"] })
			.InsertEntryAsync();

		product["ProductName"].Should().Be("Test4");
		product["CategoryID"].Should().Be(category["CategoryID"]);
		category = await client
			.For("Categories")
			.Expand("Products")
			.Filter("CategoryName eq 'Test3'")
			.FindEntryAsync();
		((category["Products"] as IEnumerable<object>).Count() == 1).Should().BeTrue();
	}

	[Fact(Skip = "Cannot be mocked")]
	public async Task InsertProductRenewHttpConnection()
	{
		//var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var client = new ODataClient(new ODataClientSettings { BaseUri = _serviceUri, RenewHttpConnection = true });
		var category = await client
			.For("Categories")
			.Set(new { CategoryName = "Test3" })
			.InsertEntryAsync();
		var product = await client
			.For("Products")
			.Set(new { ProductName = "Test4", UnitPrice = 18m, CategoryID = category["CategoryID"] })
			.InsertEntryAsync();

		product["ProductName"].Should().Be("Test4");
		product["CategoryID"].Should().Be(category["CategoryID"]);
		category = await client
			.For("Categories")
			.Expand("Products")
			.Filter("CategoryName eq 'Test3'")
			.FindEntryAsync();
		((category["Products"] as IEnumerable<object>).Count() == 1).Should().BeTrue();
	}

	[Fact]
	public async Task InsertProductWithCategoryByAssociation()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var category = await client
			.For("Categories")
			.Set(new { CategoryName = "Test5" })
			.InsertEntryAsync();
		var product = await client
			.For("Products")
			.Set(new { ProductName = "Test6", UnitPrice = 18m, Category = category })
			.InsertEntryAsync();

		product["ProductName"].Should().Be("Test6");
		product["CategoryID"].Should().Be(category["CategoryID"]);
		category = await client
			.For("Categories")
			.Expand("Products")
			.Filter("CategoryName eq 'Test5'")
			.FindEntryAsync();
		((category["Products"] as IEnumerable<object>).Count() == 1).Should().BeTrue();
	}

	[Fact]
	public async Task InsertShip()
	{
		var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
		var ship = await client
			.For("Transport")
			.As("Ship")
			.Set(new { ShipName = "Test1" })
			.InsertEntryAsync();

		ship["ShipName"].Should().Be("Test1");
	}
}
