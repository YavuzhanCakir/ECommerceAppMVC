﻿namespace ECommerceApp.Models
{
    public class ProductModel : BaseEntitiy
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }

        public int Quantity { get; set; } 

        public string Image {  get; set; }
    }
}
