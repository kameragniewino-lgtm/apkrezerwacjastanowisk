namespace LowiskoWeb.Models;

public class Rezerwacja
{
    public int Id { get; set; }
    public int StanowiskoNumer { get; set; }
    public string Wedkarz { get; set; } = "";
    public string? Telefon { get; set; }
    public DateTime DataOd { get; set; }
    public DateTime DataDo { get; set; }
    public TimeOnly GodzinaOd { get; set; } = new(6, 0);
    public TimeOnly GodzinaDo { get; set; } = new(21, 0);
    public string TypPobytu { get; set; } = "Dzień";
    public int IloscLowiacych { get; set; } = 1;
    public int IloscWedek { get; set; } = 2;
    public bool OsobaTowarzyszaca { get; set; }
    public decimal Cena { get; set; }

    public DateTime PoczatekPelny => DataOd.Date + GodzinaOd.ToTimeSpan();
    public DateTime KoniecPelny
    {
        get
        {
            var koniec = DataDo.Date + GodzinaDo.ToTimeSpan();
            // Nocka/Doba — godzina końca <= godzina startu = koniec następnego dnia
            if (GodzinaDo <= GodzinaOd)
                koniec = koniec.AddDays(1);
            return koniec;
        }
    }
}
