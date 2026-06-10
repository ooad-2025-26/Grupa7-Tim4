using System.ComponentModel.DataAnnotations;

namespace ZamETF.Models
{
    public class IspitCreateVM
    {
        [Required(ErrorMessage = "Odaberite predmet.")]
        [Display(Name = "Predmet")]
        public int PredmetId { get; set; }

        [Required(ErrorMessage = "Unesite datum ispita.")]
        [Display(Name = "Datum ispita")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Datum { get; set; }

        [Required(ErrorMessage = "Unesite rok za prijavu.")]
        [Display(Name = "Rok za prijavu")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime RokZaPrijavu { get; set; }
    }
}
