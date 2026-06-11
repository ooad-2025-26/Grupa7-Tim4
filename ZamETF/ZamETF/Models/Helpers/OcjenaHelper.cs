namespace ZamETF.Helpers
{
    public static class OcjenaHelper
    {
        // Skala: <55=5(pao), 55-64=6, 65-74=7, 75-84=8, 85-94=9, 95-100=10
        public static int BodoviUOcjenu(int bodovi)
        {
            if (bodovi < 55) return 5;
            if (bodovi < 65) return 6;
            if (bodovi < 75) return 7;
            if (bodovi < 85) return 8;
            if (bodovi < 95) return 9;
            return 10;
        }

        public static bool Polozen(int bodovi) => bodovi >= 55;
    }
}
