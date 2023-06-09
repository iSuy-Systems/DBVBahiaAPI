﻿using System.Text.Json.Serialization;

namespace DBVBahia.Business.Models
{
    public class Produto : Entity
    {
        public Guid FornecedorId { get; set; }
        public Guid PictureId { get; set; }

        public string Nome { get; set; }
        public string Descricao { get; set; }
        //public string Imagem { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }

        /* EF Relations */
        [JsonIgnore]
		public Fornecedor Fornecedor { get; set; }
        [JsonIgnore]
        public Picture Picture { get; set; }
    }
}