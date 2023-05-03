using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

var path = args.Length == 0
	? @"Data\Products.json"
	: args[0];

var stream = File.OpenRead(path);
var products = await JsonSerializer.DeserializeAsync<Import[]>(stream);

var options = new DbContextOptionsBuilder<DataContext>()
	.UseSqlite("Data Source=Products.sqlite;")
	.Options;

using var context = new DataContext(options);

if (!await context.Categories.AnyAsync())
{
	var categoryNames = ( 
		from p in products
		from c in p.Categories
		select c
	).Distinct();

	var categories = categoryNames
		.Select(c => new Category { Name = c });

	await context.Categories.AddRangeAsync(categories);
	await context.SaveChangesAsync();
}

if (!await context.Products.AnyAsync())
{
	foreach (var p in products)
	{
		var categories = await context.Categories.Where(c => p.Categories.Contains(c.Name)).ToListAsync();
		await context.Products.AddAsync(new Product
		{
			Title = p.Title,
			Description = p.Description,
			Price = p.Price,
			DiscountedPrice = p.DiscountedPrice,
			Image = p.Image,
			Categories = categories,
		});
	}

	await context.SaveChangesAsync();
}

class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlite("Data Source=Products.sqlite;");

        return new DataContext(optionsBuilder.Options);
    }
}

class DataContext : DbContext
{
	public DataContext(DbContextOptions<DataContext> options)
		: base(options)
	{ }

	public DbSet<Product> Products => Set<Product>();

	public DbSet<Category> Categories => Set<Category>();
}

record Category
{
	public Category()
	{
		Products = new List<Product>();
	}

	public int Id { get; set; }

	required public string Name { get; set; }

	public ICollection<Product> Products { get; set; }
}

record Product
{
	public Product()
	{
		Categories = new List<Category>();
	}

	public int Id { get; set; }
	
	required public string Title { get; set; }
	
	public decimal Price { get; set; }

	public decimal DiscountedPrice { get; set; }

	required public string Description { get; set; }

	required public string Image { get; set; }

	public ICollection<Category> Categories { get; set; }
}