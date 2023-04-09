namespace DBVBahia.Business.Models
{
    public class Picture : Entity
    {
        public string Nome { get; set; }
        public byte[] Image { get; set; }

        /* EF Relations */
        public Produto Produto { get; set; }
    }
}