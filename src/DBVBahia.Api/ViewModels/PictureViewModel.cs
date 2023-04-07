using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DBVBahia.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DBVBahia.Api.ViewModels
{
    
    public class PictureViewModel
    {
        [Key]
        public Guid Id { get; set; }

		public string Name { get; set; }

        // Evita o erro de conversão de string vazia para IFormFile
        [JsonIgnore]
        public IFormFile ImagemUpload { get; set; }

        public string ImagemUpload64 { get; set; }
    }
}