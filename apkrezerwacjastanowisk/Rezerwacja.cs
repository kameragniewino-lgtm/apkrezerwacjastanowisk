namespace apkrezerwacjastanowisk
{
    public class Rezerwacja
    {
        public int Id { get; set; }
        public int StanowiskoNumer { get; set; }
        public string Wędkarz { get; set; } = "";
        public string? Telefon { get; set; }
        public DateTime DataOd { get; set; }
        public DateTime DataDo { get; set; }

        public bool KolidujZ(DateTime od, DateTime doDaty)
        {
            return DataOd < doDaty && DataDo > od;
        }

        public bool KolidujZ(DateTime dzien)
        {
            return DataOd.Date <= dzien.Date && DataDo.Date >= dzien.Date;
        }
    }
}
