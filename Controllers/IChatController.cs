using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilometroWorker.Controllers
{
    public interface IChatController<T, K, Y>
    {
        public List<T> Data { get; set; }

        public string HandleMessage(K message);

        public List<Y> GetNotifications(List<T> modifiedData);


    }
}
