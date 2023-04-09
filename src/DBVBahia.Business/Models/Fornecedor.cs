namespace DBVBahia.Business.Models
{
    public class Fornecedor : Entity
    {
        public Fornecedor() { 
            Produtos = new List<Produto>();
        }

        public string Nome { get; set; }
        public string Documento { get; set; }
        public TipoFornecedor TipoFornecedor { get; set; }
        public Endereco Endereco { get; set; }
        public bool Ativo { get; set; }

        /* EF Relations */
        public IList<Produto> Produtos { get; set; }
    }
}