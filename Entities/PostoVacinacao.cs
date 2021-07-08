using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilometroWorker.Entities
{
   public record PostoVacinacao(string equipamento, string endereco, string tipo_posto, string id_tipo_posto, string id_distrito, string distrito, string id_crs, string crs, string data_hora, string indice_fila, string status_fila);

  
      
}
