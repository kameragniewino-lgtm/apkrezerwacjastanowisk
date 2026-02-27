using LowiskoWeb.Models;

namespace LowiskoWeb.Services;

public class StanowiskaService
{
    private readonly BazaDanych _db;
    public List<Stanowisko> Stanowiska { get; } = [];

    public TimeOnly DomyslnaGodzinaOd { get; private set; } = new(6, 0);
    public TimeOnly DomyslnaGodzinaDo { get; private set; } = new(21, 0);
    public decimal CenaDobowa { get; private set; } = 80;
    public decimal CenaNocna { get; private set; } = 60;
    public decimal CenaDoba { get; private set; } = 120;
    public decimal CenaOsobaTowarzyszaca { get; private set; } = 20;
    public decimal CenaDodatkowyLowiacy { get; private set; } = 40;
    public int WedkiWCenie { get; private set; } = 2;
    public decimal CenaDodatkowaWedka { get; private set; } = 30;

    public StanowiskaService(BazaDanych db)
    {
        _db = db;
        Utworz();

        // Jeśli baza nie ma pozycji — zapisz domyślne
        if (!_db.MaPozycje())
            _db.ZapiszPozycje(Stanowiska);

        // Zawsze czytaj pozycje z bazy (baza jest źródłem prawdy)
        _db.WczytajPozycje(Stanowiska);
        OdswiezUstawienia();
        OdswiezRezerwacje();
    }

    private void Utworz()
    {
        (int nr, double x, double y)[] poz =
        [
            (1,35.2,6.6),(2,30.7,13.9),(3,30.7,19.3),(4,28.5,24.0),(5,20.1,30.3),
            (6,18.1,34.6),(7,11.0,41.3),(8,8.8,47.6),(9,9.7,53.0),(10,7.8,58.4),
            (11,7.1,64.5),(12,10.1,69.3),(13,14.2,74.4),(14,18.1,78.1),(15,22.0,85.0),
            (16,25.6,88.9),(17,59.5,95.5),(18,61.7,91.6),(19,66.0,87.4),(20,71.8,83.5),
            (21,77.3,78.9),(22,84.1,74.0),(23,82.2,68.2),(24,85.4,62.3),(25,86.7,56.5),
            (26,89.0,51.3),(27,91.9,44.9),(28,89.3,38.9),(29,93.2,33.2),(30,93.2,26.9)
        ];
        foreach (var (nr, x, y) in poz)
            Stanowiska.Add(new Stanowisko { Numer = nr, X = x, Y = y });
    }

    public void OdswiezRezerwacje()
    {
        foreach (var st in Stanowiska)
            st.Rezerwacje = _db.PobierzRezerwacje(st.Numer);
    }

    public void OdswiezUstawienia()
    {
        DomyslnaGodzinaOd = TimeOnly.Parse(_db.PobierzUstawienie("GodzinaStart", "06:00"));
        DomyslnaGodzinaDo = TimeOnly.Parse(_db.PobierzUstawienie("GodzinaKoniec", "21:00"));
        CenaDobowa = decimal.Parse(_db.PobierzUstawienie("CenaDobowa", "80"));
        CenaNocna = decimal.Parse(_db.PobierzUstawienie("CenaNocna", "60"));
        CenaDoba = decimal.Parse(_db.PobierzUstawienie("CenaDoba", "120"));
        CenaOsobaTowarzyszaca = decimal.Parse(_db.PobierzUstawienie("CenaOsobaTowarzyszaca", "20"));
        CenaDodatkowyLowiacy = decimal.Parse(_db.PobierzUstawienie("CenaDodatkowyLowiacy", "40"));
        WedkiWCenie = int.Parse(_db.PobierzUstawienie("WedkiWCenie", "2"));
        CenaDodatkowaWedka = decimal.Parse(_db.PobierzUstawienie("CenaDodatkowaWedka", "30"));
    }

    public decimal ObliczCene(string typPobytu, DateTime dataOd, DateTime dataDo, int iloscLowiacych, int iloscWedek, bool osobaTow)
    {
        int iloscDni = (dataDo.Date - dataOd.Date).Days + 1;

        // Bazowa cena za typ pobytu
        decimal bazowa = typPobytu switch
        {
            "Nocka" => CenaNocna,
            "Doba" => CenaDoba,
            _ => CenaDobowa // Dzień
        };
        decimal suma = bazowa * iloscDni;

        // Dodatkowi łowiący
        if (iloscLowiacych > 1)
            suma += CenaDodatkowyLowiacy * (iloscLowiacych - 1) * iloscDni;

        // Dodatkowe wędki ponad te w cenie
        if (iloscWedek > WedkiWCenie)
            suma += CenaDodatkowaWedka * (iloscWedek - WedkiWCenie) * iloscDni;

        // Osoba towarzysząca
        if (osobaTow)
            suma += CenaOsobaTowarzyszaca * iloscDni;

        return suma;
    }

    public int Rezerwuj(Rezerwacja r)
    {
        var id = _db.DodajRezerwacje(r);
        r.Id = id;
        Stanowiska.First(s => s.Numer == r.StanowiskoNumer).Rezerwacje.Add(r);
        return id;
    }

    public void Usun(int id)
    {
        _db.UsunRezerwacje(id);
        foreach (var st in Stanowiska)
            st.Rezerwacje.RemoveAll(r => r.Id == id);
    }

    public List<Rezerwacja> WszystkieRezerwacje() => _db.PobierzWszystkie();
}
