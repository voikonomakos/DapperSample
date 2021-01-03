using System;

namespace DapperSample
{
    class Program
    {
        private static string connectionString = "";

        static void Main(string[] args)
        {
            try
            {
                var repo = new ContactRepositoryContrib(connectionString);
                var contact = repo.Find(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
