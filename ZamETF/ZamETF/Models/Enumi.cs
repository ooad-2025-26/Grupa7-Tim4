namespace ZamETF.Models
{
    public enum Uloga
    {
        Student,
        Profesor,
        Administrator,
        StudentskaSluzba
    }

    public enum TipDokumenta
    {
        PrepisOcjena,
        OcjenePoSemestrima,
        Potvrda
    }

    public enum StatusZadace
    {
        Predana,
        Pregledana,
        NijePredana
    }

    public enum StatusPrijaveIspit
    {
        PrijavljenIspit,
        PopunjenTermin,
        Odjavljen,
        IstekaoRok
    }

    public enum TipAdminZahtjeva
    {
        StatistikaProfesor,
        StatistikaStudent,
        StatistikaStudentskaSluzba,
        KreiranjeKorisnika,
        BrisanjeKorisnika,
        AzuriranjeKorisnika
    }
}