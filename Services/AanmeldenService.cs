using BlazorApp1.Data;
using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class AanmeldenService
    {
        private  ApplicationDbContext _context;

        public AanmeldenService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<AanmeldenModel>> GetAllAanmeldingenAsync()
        {
            return await _context.Aanmeldingen
                .OrderByDescending(a => a.AanmeldDatum)
                .ToListAsync();
        }
        public async Task SaveAanmeldingAsync(AanmeldenModel model)
        {
            model.AanmeldDatum = DateTime.Now;
            await _context.Aanmeldingen.AddAsync(model);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAanmeldingAsync(AanmeldenModel model)
        {
            _context.Aanmeldingen.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAanmeldingAsync(int id)
        {
            var item = await _context.Aanmeldingen.FindAsync(id);
            if (item == null) return false;

            _context.Aanmeldingen.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}