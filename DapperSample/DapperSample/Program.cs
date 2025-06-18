using System;

namespace DapperSample
{
    class Program
    {
        private static readonly string ConnectionString = "";

        static void Main(string[] args)
        {
            try
            {
                ContactRepository repository = new ContactRepository(ConnectionString);
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
