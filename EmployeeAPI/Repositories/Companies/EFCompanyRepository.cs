using EmployeeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Repositories.Companies
{
    public class EFCompanyRepository : ICompanyRepository
    {
        private readonly AppDbContext _context;
        public EFCompanyRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Company> GetCompanyByIdAsync(Guid id)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies.Where(c => !c.IsDeleted).ToListAsync();
        }


        public async Task<Company> AddCompanyAsync(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<Company> UpdateCompanyAsync(Company company)
        {
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
            return company;
        }

        //public async Task DeleteCompanyAsync(int id)
        //{
        //    var company = await _context.Companies.FindAsync(id);
        //    if (company != null)
        //    {
        //        _context.Companies.Remove(company);
        //        await _context.SaveChangesAsync();
        //    }
        //}
    }
}
