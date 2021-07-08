using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilometroWorker.Database
{
    public static class DatabaseConnector
    {
      
        public static void InsertData(Subscription subscription)
        {
            using FilometroContext filometroContext = new();

            filometroContext.Database.EnsureCreated();

            filometroContext.Subscriptions.Add(subscription);

            filometroContext.SaveChanges();
           
        }


        public static void RemoveByUsername(string username)
        {
            using FilometroContext filometroContext = new();

            filometroContext.Database.EnsureCreated();
            Subscription[] subscriptions = filometroContext.Subscriptions.Where(k => k.Username.Equals(username)).ToArray();
            if (subscriptions is not null)
                filometroContext.Subscriptions.RemoveRange(subscriptions);

            filometroContext.SaveChanges();

        }

        public static List<Subscription> GetSubscriptionsByUsername(string username)
        {
            using FilometroContext filometroContext = new();
            filometroContext.Database.EnsureCreated();

            List<Subscription> result = new();
            result = filometroContext.Subscriptions.Where(k => k.Username.Equals(username)).ToList();
            
            return result;
        }

        public static List<Subscription> GetSubscriptionsByPosto(string posto)
        {
            using FilometroContext filometroContext = new();
            filometroContext.Database.EnsureCreated();

            List<Subscription> result = new();
            result = filometroContext.Subscriptions.Where(k => k.NomePosto.Equals(posto)).ToList();

            return result;
        }

    }
}
