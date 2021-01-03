using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Contrib.Extensions;

namespace DapperSample
{
    public class ContactRepositoryContrib
    {
        private IDbConnection db;

        public ContactRepositoryContrib(string connString)
        {
            db = new SqlConnection(connString);
        }

        public Contact Add(Contact contact)
        {
            var id = db.Insert(contact);
            contact.Id = (int)id;
            return contact;
        }

        public Contact Find(int id)
        {
            return db.Get<Contact>(id);
        }

        public List<Contact> GetAll()
        {
            return db.GetAll<Contact>().ToList();
        }

        public void Remove(int id)
        {
            db.Delete(new Contact { Id = id });
        }

        public Contact Update(Contact contact)
        {
            db.Update(contact);
            return contact;
        }
    }
}
