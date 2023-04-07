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
		public byte[] Image { get; set; }

	}
}