using System.Collections.Generic;
using ZamETF.Models;

namespace ZamETF.ViewModels
{
    public class ProfesorIspitiVM
    {
        public List<Ispit> Ispiti { get; set; } = new List<Ispit>();
        public IspitCreateVM Novi { get; set; } = new IspitCreateVM();
    }
}
