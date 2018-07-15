using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Marten;

namespace cManager.Controllers
{
    public class QueryContactsResponse
    {
        public IEnumerable<Contact> Items { get; set; } = Enumerable.Empty<Contact>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }
    }

    [Route("api/[controller]")]
    public class ContactController : Controller
    {
        public int DefaultPageRecordCount = 2;
        public QueryContactsResponse responce;
        public readonly IDocumentStore store = DocumentStore.For("host=localhost;port=5432;database=contact_manager_db;password=admin;username=postgres");

        public ContactController()
        {
            responce = new QueryContactsResponse();
        }

        public IEnumerable<Contact> GetContacts()
        {
            using (var session = store.OpenSession())
            {
                return session.Query<Contact>().ToList();
            }
        }
        [HttpGet]
        public IActionResult GetAll([FromQuery]int? page, [FromQuery] int? pageSize)
        {
            var takePage = page ?? 1;
            var takeCount =pageSize ?? DefaultPageRecordCount;
            var contacts = GetContacts()
                                .Skip((takePage - 1) * takeCount)
                                .Take(takeCount)
                                .ToList();
            responce.Items = contacts;
            responce.Page = takePage;
            responce.PageCount = (int)Math.Ceiling((decimal)GetContacts().Count() / takeCount);
            responce.PageSize = contacts.Count();
            responce.TotalCount = GetContacts().Count();
            return new OkObjectResult(responce);
        }

        [HttpGet]
        [Route("~/api/contact/{Id}")]
        public Contact GetContactDetails(Guid Id)
        {
            using (var session = store.OpenSession())
            {
                List<Contact> list = session.Query<Contact>().ToList();
                var contact = list.FirstOrDefault(x => x.Id == Id);
                if (contact == null)
                {
                    throw new Exception("error get contact");
                }
                return contact;
            }
        }
        [HttpPost]
        public void CreateContact([FromBody] Contact contact)
        {
            using (var session = store.OpenSession())
            {
                session.Store(contact);
                session.SaveChanges();
            }

        }

        [HttpPut("{Id}")]
        public void EditContact(Guid Id, [FromBody] Contact contact)
        {
            using (var session = store.OpenSession())
            {
                var Contact = session.Query<Contact>().FirstOrDefault(x => x.Id == Id);
                if (Contact == null)
                {

                }
                else
                {
                    Contact.FirstName = contact.FirstName;
                    Contact.LastName = contact.LastName;
                    Contact.Email = contact.Email;
                    Contact.PhoneNumber = contact.PhoneNumber;
                    session.Store(Contact);
                    session.SaveChanges();

                }
            }
        }

        [HttpDelete("{Id}")]
        public void DeleteContact(Guid Id)
        {
            using (var session = store.OpenSession())
            {
                var contact = session.Query<Contact>().FirstOrDefault(x => x.Id == Id);
                if (contact == null)
                    throw new Exception("contact delete");
                session.Delete(contact);
                session.SaveChanges();
            }
        }

        //[Route("SearchContact")]
        [HttpGet]
        [Route("~/api/contact/search")]
        public IEnumerable<Contact> SearchContact([FromQuery] string FirstName)
        {
            
            using (var session = store.QuerySession())
            {
                FirstName = FirstName?.ToLower() ??"";
                if (FirstName == "")
                {
                    List<Contact> list = session.Query<Contact>() 
                    .ToList();
                    return list;
                }
                else
                {
                    List<Contact> list = session.Query<Contact>()
                    .Where(x => x.FirstName.Contains(FirstName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                    return list;
                }
                
                
                
            }
        }
        public void Create()
        {
            using (var session = store.OpenSession())
            {
                var contact = new Contact
                {
                    FirstName = "Pece",
                    LastName = "Deteto",
                    Email = "pece@hotmail.com",
                    PhoneNumber = "070-898-770"
                };

                session.Store(contact);
                session.SaveChanges();
            }
            
        }

    }  
}