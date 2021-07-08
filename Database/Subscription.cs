using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilometroWorker.Database
{
    public record Subscription
    {
              
        public int ID { get; init; }

        public long ChatId { get; init; }
        public string Username { get; init;  }

        public string NomePosto { get; init; }
    }
}
