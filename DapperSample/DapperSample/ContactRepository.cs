using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DapperSample
{
    public class ContactRepository : BaseRepository
    {

        public ContactRepository(string connString) : base(connString)
        {
        }

        public async Task<Contact> Find(int id)
        {
            return await Find<Contact>("SELECT * FROM Contacts WHERE Id = @Id", id);
        }

        public async Task<List<Contact>> GetContactsById(params int[] ids)
        {
            return await Get<Contact>("SELECT * FROM Contacts WHERE ID IN @Ids", new { Ids = ids });
        }

        public async Task<Contact> Add(Contact contact)
        {
            var cmdText =
                "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title) VALUES (@FirstName, @LastName, @Email, @Company, @Title); " +
                "SELECT CAST(SCOPE_IDENTITY() as int)";

            return await WithConnection(async conn =>
            {
                int id = await conn.QuerySingleAsync<int>(cmdText, contact);
                contact.Id = id;
                return contact;
            });
        }

        public async Task Update(Contact contact)
        {
            var sql =
                "UPDATE Contacts " +
                "SET FirstName = @FirstName, " +
                "    LastName  = @LastName, " +
                "    Email     = @Email, " +
                "    Company   = @Company, " +
                "    Title     = @Title " +
                "WHERE Id = @Id";

            await ExecuteAsync<Contact>(sql, contact);
        }
    }
}
