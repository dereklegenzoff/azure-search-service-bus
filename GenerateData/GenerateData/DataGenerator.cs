using Bogus;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace GenerateData
{
    class DataGenerator
    {
        public List<ContactInfo> GetDocuments(int count)
        {
            var contactList = new Faker<ContactInfo>()
                //Basic rules using built-in generators
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.ZipCode, f => f.Address.ZipCode())
                .RuleFor(u => u.PhoneNumber, f => f.Random.Replace("(###) ###-####"));

            var output = new List<ContactInfo>();
            for (int i = 0; i < count; i++)
            {
                output.Add(contactList.Generate());
            }

            return output;
        }
    }
}
