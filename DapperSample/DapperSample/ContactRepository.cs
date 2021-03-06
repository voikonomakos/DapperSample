﻿using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DapperSample
{
    public class ContactRepository
    {
        private IDbConnection db;

        public ContactRepository(string connString)
        {
            db = new SqlConnection(connString);
        }

        public Contact Add(Contact contact)
        {
            var sql =
                "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title) VALUES(@FirstName, @LastName, @Email, @Company, @Title); " +
                "SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = db.Query<int>(sql, contact).Single();
            contact.Id = id;
            return contact;
        }

        public Contact Find(int id)
        {
            return db.Query<Contact>("SELECT * FROM Contacts WHERE Id = @Id", new { id }).SingleOrDefault();
        }

        public Contact GetFullContact(int id)
        {
            var sql =
                "SELECT * FROM Contacts WHERE Id = @Id; " +
                "SELECT * FROM Addresses WHERE ContactId = @Id";

            using (var multipleResults = this.db.QueryMultiple(sql, new { Id = id }))
            {
                var contact = multipleResults.Read<Contact>().SingleOrDefault();

                var addresses = multipleResults.Read<Address>().ToList();
                if (contact != null && addresses != null)
                {
                    contact.Addresses.AddRange(addresses);
                }

                return contact;
            }
        }

        public void Save(Contact contact)
        {
            using var txScope = new TransactionScope();

            if (contact.IsNew)
            {
                Add(contact);
            }
            else
            {
                Update(contact);
            }

            foreach (var addr in contact.Addresses.Where(a => !a.IsDeleted))
            {
                addr.ContactId = contact.Id;

                if (addr.IsNew)
                {
                    Add(addr);
                }
                else
                {
                    Update(addr);
                }
            }

            foreach (var addr in contact.Addresses.Where(a => a.IsDeleted))
            {
                db.Execute("DELETE FROM Addresses WHERE Id = @Id", new { addr.Id });
            }

            txScope.Complete();
        }

        public Address Add(Address address)
        {
            var sql =
                "INSERT INTO Addresses (ContactId, AddressType, StreetAddress, City, StateId, PostalCode) VALUES(@ContactId, @AddressType, @StreetAddress, @City, @StateId, @PostalCode); " +
                "SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = db.Query<int>(sql, address).Single();
            address.Id = id;
            return address;
        }

        public Address Update(Address address)
        {
            db.Execute("UPDATE Addresses " +
                "SET AddressType = @AddressType, " +
                "    StreetAddress = @StreetAddress, " +
                "    City = @City, " +
                "    StateId = @StateId, " +
                "    PostalCode = @PostalCode " +
                "WHERE Id = @Id", address);
            return address;
        }

        public List<Contact> GetAll()
        {
            return db.Query<Contact>("SELECT * FROM Contacts").ToList();
        }

        public void Remove(int id)
        {
            db.Execute("DELETE FROM Contacts WHERE Id = @Id", new { id });
        }

        public Contact Update(Contact contact)
        {
            var sql =
                "UPDATE Contacts " +
                "SET FirstName = @FirstName, " +
                "    LastName  = @LastName, " +
                "    Email     = @Email, " +
                "    Company   = @Company, " +
                "    Title     = @Title " +
                "WHERE Id = @Id";
            db.Execute(sql, contact);
            return contact;
        }

        public async Task<List<Contact>> GetAllAsync()
        {
            var contacts = await db.QueryAsync<Contact>("SELECT * FROM Contacts");
            return contacts.ToList();
        }

        public List<Contact> GetAllContactsWithAddresses()
        {
            var sql = "SELECT * FROM Contacts AS C INNER JOIN Addresses AS A ON A.ContactId = C.Id";

            var contactDict = new Dictionary<int, Contact>();

            var contacts = db.Query<Contact, Address, Contact>(sql, (contact, address) =>
            {
                if (!contactDict.TryGetValue(contact.Id, out var currentContact))
                {
                    currentContact = contact;
                    contactDict.Add(currentContact.Id, currentContact);
                }

                currentContact.Addresses.Add(address);
                return currentContact;
            });
            return contacts.Distinct().ToList();
        }

        public List<Address> GetAddressesByState(int stateId)
        {
            return this.db.Query<Address>("SELECT * FROM Addresses WHERE StateId = {=stateId}", new { stateId }).ToList();
        }

        public int BulkInsertContacts(List<Contact> contacts)
        {
            var sql =
                "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title) VALUES(@FirstName, @LastName, @Email, @Company, @Title); " +
                "SELECT CAST(SCOPE_IDENTITY() as int)";
            return db.Execute(sql, contacts);
        }

        public List<Contact> GetContactsById(params int[] ids)
        {
            return db.Query<Contact>("SELECT * FROM Contacts WHERE ID IN @Ids", new { Ids = ids }).ToList();
        }

        public List<dynamic> GetDynamicContactsById(params int[] ids)
        {
            return db.Query("SELECT * FROM Contacts WHERE ID IN @Ids", new { Ids = ids }).ToList();
        }
    }
}
