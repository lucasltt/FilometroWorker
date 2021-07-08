using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilometroWorker.Entities
{
    public record ReplyMessage(string body, long chatid);
}
