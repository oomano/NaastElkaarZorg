using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models
{
    public class AanmeldenModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Naam { get; set; } = string.Empty;

        [Required]
        public string Telefoon { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ClientNaam { get; set; } = string.Empty;

        public DateTime? ClientGeboortedatum { get; set; }

        public string Relatie { get; set; } = string.Empty;

        [Required]
        public string Bericht { get; set; } = string.Empty;

        public DateTime AanmeldDatum { get; set; } = DateTime.Now;
    }
}