namespace LowiskoWeb.Models;

public class Stanowisko
{
    public int Numer { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public List<Rezerwacja> Rezerwacje { get; set; } = [];

    // Sprawdzanie dokładne z godzinami (do kolizji przy rezerwacji)
    public bool CzyZajete(DateTime od, DateTime doDaty)
        => Rezerwacje.Any(r => r.PoczatekPelny < doDaty && r.KoniecPelny > od);

    // Status dostępności dnia: "wolne", "dzien", "nocka", "pelne"
    public string StatusDnia(DateTime dzien, TimeOnly startDnia, TimeOnly koniecDnia)
    {
        var dzienOd = dzien.Date + startDnia.ToTimeSpan();
        var dzienDo = dzien.Date + koniecDnia.ToTimeSpan();
        var nocOd = dzien.Date + koniecDnia.ToTimeSpan();
        var nocDo = dzien.Date.AddDays(1) + startDnia.ToTimeSpan();

        bool dzienZajety = Rezerwacje.Any(r => r.PoczatekPelny < dzienDo && r.KoniecPelny > dzienOd);
        bool nocZajeta = Rezerwacje.Any(r => r.PoczatekPelny < nocDo && r.KoniecPelny > nocOd);

        return (dzienZajety, nocZajeta) switch
        {
            (true, true) => "pelne",
            (true, false) => "dzien",
            (false, true) => "nocka",
            _ => "wolne"
        };
    }

    public Rezerwacja? RezerwacjaDnia(DateTime dzien)
        => Rezerwacje.FirstOrDefault(r => r.DataOd <= dzien.Date && r.DataDo >= dzien.Date);
}
