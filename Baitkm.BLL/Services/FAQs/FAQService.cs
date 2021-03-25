using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.FAQ;
using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.FAQs
{
    public class FAQService : IFAQService
    {
        private readonly IEntityRepository _repository;
        public FAQService(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Add(AddFAQModel model)
        {
            _repository.Create(new FAQ
            {
                Title = model.Title,
                Description = model.Description
            });
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var faq = _repository.GetById<FAQ>(id);
            _repository.Remove(faq);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Edit(FAQViewModel model)
        {
            var faq = await _repository.Filter<FAQ>(x => x.Id == model.Id && x.Title == model.Title).FirstOrDefaultAsync();
            dynamic property = new ExpandoObject();
            property.Description = model.Description;
            property.Title = model.Title;
            _repository.Update(faq);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<FAQViewModel> Get(int id)
        {
            return await _repository.Filter<FAQ>(x => x.Id == id).Select(x => new FAQViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description
            }).FirstOrDefaultAsync();
        }

        public async Task<List<FAQViewModel>> GetList()
        {
            return await _repository.GetAll<FAQ>().Select(x => new FAQViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description
            }).ToListAsync();
        }
    }
}
