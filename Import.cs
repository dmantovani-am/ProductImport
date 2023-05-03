record Import
{
	public int Id { get; set; }
	
	required public string Title { get; set; }
	
	public decimal Price { get; set; }

	public decimal DiscountedPrice { get; set; }

	required public string Description { get; set; }

	required public string[] Categories { get; set; }

	required public string Image { get; set; }
}