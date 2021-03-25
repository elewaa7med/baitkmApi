using Baitkm.DAL.Repository.Entities;
using Baitkm.DTO.ViewModels.Email;
using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Emails
{
    public class EmailService : IEmailService
    {
        private readonly IEntityRepository _repository;
        public EmailService(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Add(AddEmailRequestModel model)
        {
            _repository.Create<Email>(new Email { EmailText = model.Email });
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var email = await _repository.Filter<Email>(e => e.Id == id).FirstOrDefaultAsync();
            _repository.Remove(email);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Edit(EditEmailRequestModel model)
        {
            var email = await _repository.Filter<Email>(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (email == null)
                throw new Exception("email not found");

            email.EmailText = model.Email;
            _repository.Update(email);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<EmailResponseModel> Get(int id)
        {
            Email email = await _repository.Filter<Email>(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();

            return new EmailResponseModel
            {
                Email = email.EmailText,
                Id = email.Id
            };
        }

        public async Task<List<EmailResponseModel>> List()
        {
            List<EmailResponseModel> result = new List<EmailResponseModel>();
            List<Email> emails = await _repository.Filter<Email>(x => !x.IsDeleted).ToListAsync();
            foreach (var e in emails)
            {
                result.Add(new EmailResponseModel
                {
                    Id = e.Id,
                    Email = e.EmailText
                });
            }
            return result;
        }
    }
}