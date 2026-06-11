using System.Collections.Generic;
using ZamETF.Models;

namespace ZamETF.ViewModels
{
    public class StudentZadacaItemVM
    {
        public Zadaca Zadaca { get; set; }
        public PredajaZadace MojaPredaja { get; set; }   // null ako nije predano
    }

    public class StudentZadaceVM
    {
        public List<StudentZadacaItemVM> Stavke { get; set; } = new List<StudentZadacaItemVM>();
    }

    public class DetaljiZadaceVM
    {
        public Zadaca Zadaca { get; set; }
        public PredajaZadace MojaPredaja { get; set; }   // null ako nije predano
    }
}
