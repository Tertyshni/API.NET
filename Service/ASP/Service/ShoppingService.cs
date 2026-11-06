namespace MyWebApi.Services
{
    public class ShoppingService
    {
        private List<string> products = new List<string>();

        public void AddProduct(string product)
        {
            products.Add(product);
        }

        public List<string> GetProducts()
        {
            return products;
        }
    }
}