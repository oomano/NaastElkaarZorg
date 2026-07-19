using BlazorApp1.Data;
using BlazorApp1.Models;

namespace BlazorApp1.Services
{
    public class AanmeldenService
    {
        private  ApplicationDbContext _context;

        public AanmeldenService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveAanmeldingAsync(AanmeldenModel model)
        {
            model.AanmeldDatum = DateTime.Now;
            await _context.Aanmeldingen.AddAsync(model);
            await _context.SaveChangesAsync();
        }
    }
}