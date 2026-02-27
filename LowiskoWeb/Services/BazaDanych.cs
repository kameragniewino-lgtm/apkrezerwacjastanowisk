using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using LowiskoWeb.Models;

namespace LowiskoWeb.Services;

public sealed class BazaDanych : IDisposable
{
    private readonly SqliteConnection _conn;

    public BazaDanych()
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "lowisko.db");
        _conn = new SqliteConnection($"Data Source={dbPath}");
        _conn.Open();
        UtworzTabele();
    }

    private void UtworzTabele()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Rezerwacje (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StanowiskoNumer INTEGER NOT NULL,
                Wedkarz TEXT NOT NULL,
                Telefon TEXT,
                DataOd TEXT NOT NULL,
                DataDo TEXT NOT NULL,
                GodzinaOd TEXT NOT NULL DEFAULT '06:00',
                GodzinaDo TEXT NOT NULL DEFAULT '21:00'
            );
            CREATE TABLE IF NOT EXISTS PozycjeStanowisk (
                Numer INTEGER PRIMARY KEY,
                X REAL NOT NULL,
                Y REAL NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Administratorzy (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Login TEXT NOT NULL UNIQUE,
                HasloHash TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Ustawienia (
                Klucz TEXT PRIMARY KEY,
                Wartosc TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();

        // Migracja — dodaj kolumny jeśli ich nie ma
        string[] migracje =
        [
            "ALTER TABLE Rezerwacje ADD COLUMN GodzinaOd TEXT NOT NULL DEFAULT '06:00'",
            "ALTER TABLE Rezerwacje ADD COLUMN GodzinaDo TEXT NOT NULL DEFAULT '21:00'",
            "ALTER TABLE Rezerwacje ADD COLUMN IloscLowiacych INTEGER NOT NULL DEFAULT 1",
            "ALTER TABLE Rezerwacje ADD COLUMN IloscWedek INTEGER NOT NULL DEFAULT 2",
            "ALTER TABLE Rezerwacje ADD COLUMN TypPobytu TEXT NOT NULL DEFAULT 'Dzień'",
            "ALTER TABLE Rezerwacje ADD COLUMN OsobaTowarzyszaca INTEGER NOT NULL DEFAULT 0",
            "ALTER TABLE Rezerwacje ADD COLUMN Cena REAL NOT NULL DEFAULT 0",
        ];
        foreach (var sql in migracje)
        {
            try { using var m = _conn.CreateCommand(); m.CommandText = sql; m.ExecuteNonQuery(); } catch { }
        }

        using var check = _conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Administratorzy";
        if ((long)check.ExecuteScalar()! == 0)
            DodajAdmina("admin", "admin123");

        // Domyślne ustawienia
        UstawDomyslne("GodzinaStart", "06:00");
        UstawDomyslne("GodzinaKoniec", "21:00");
        UstawDomyslne("CenaDobowa", "80");
        UstawDomyslne("CenaNocna", "60");
        UstawDomyslne("CenaDoba", "120");
        UstawDomyslne("CenaOsobaTowarzyszaca", "20");
        UstawDomyslne("CenaDodatkowyLowiacy", "40");
        UstawDomyslne("WedkiWCenie", "2");
        UstawDomyslne("CenaDodatkowaWedka", "30");
    }

    private void UstawDomyslne(string klucz, string wartosc)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Ustawienia (Klucz, Wartosc) VALUES (@k, @v)";
        cmd.Parameters.AddWithValue("@k", klucz);
        cmd.Parameters.AddWithValue("@v", wartosc);
        cmd.ExecuteNonQuery();
    }

    public string PobierzUstawienie(string klucz, string domyslna = "")
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT Wartosc FROM Ustawienia WHERE Klucz = @k";
        cmd.Parameters.AddWithValue("@k", klucz);
        return cmd.ExecuteScalar() is string v ? v : domyslna;
    }

    public void ZapiszUstawienie(string klucz, string wartosc)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO Ustawienia (Klucz, Wartosc) VALUES (@k, @v)";
        cmd.Parameters.AddWithValue("@k", klucz);
        cmd.Parameters.AddWithValue("@v", wartosc);
        cmd.ExecuteNonQuery();
    }

    private static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));

    public void DodajAdmina(string login, string haslo)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Administratorzy (Login, HasloHash) VALUES (@l, @h)";
        cmd.Parameters.AddWithValue("@l", login);
        cmd.Parameters.AddWithValue("@h", Hash(haslo));
        cmd.ExecuteNonQuery();
    }

    public bool SprawdzLogowanie(string login, string haslo)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT HasloHash FROM Administratorzy WHERE Login = @l";
        cmd.Parameters.AddWithValue("@l", login);
        return cmd.ExecuteScalar() is string h && h == Hash(haslo);
    }

    public int DodajRezerwacje(Rezerwacja r)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Rezerwacje (StanowiskoNumer, Wedkarz, Telefon, DataOd, DataDo, GodzinaOd, GodzinaDo, TypPobytu, IloscLowiacych, IloscWedek, OsobaTowarzyszaca, Cena)
            VALUES (@nr, @kto, @tel, @od, @do, @god_od, @god_do, @typ, @il, @wed, @os, @cena);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("@nr", r.StanowiskoNumer);
        cmd.Parameters.AddWithValue("@kto", r.Wedkarz);
        cmd.Parameters.AddWithValue("@tel", r.Telefon ?? "");
        cmd.Parameters.AddWithValue("@od", r.DataOd.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@do", r.DataDo.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@god_od", r.GodzinaOd.ToString("HH:mm"));
        cmd.Parameters.AddWithValue("@god_do", r.GodzinaDo.ToString("HH:mm"));
        cmd.Parameters.AddWithValue("@typ", r.TypPobytu);
        cmd.Parameters.AddWithValue("@il", r.IloscLowiacych);
        cmd.Parameters.AddWithValue("@wed", r.IloscWedek);
        cmd.Parameters.AddWithValue("@os", r.OsobaTowarzyszaca ? 1 : 0);
        cmd.Parameters.AddWithValue("@cena", (double)r.Cena);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void UsunRezerwacje(int id)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Rezerwacje WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public List<Rezerwacja> PobierzRezerwacje(int nr)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Wedkarz, Telefon, DataOd, DataDo, GodzinaOd, GodzinaDo, TypPobytu, IloscLowiacych, IloscWedek, OsobaTowarzyszaca, Cena FROM Rezerwacje WHERE StanowiskoNumer = @nr AND DataDo >= @dzis ORDER BY DataOd";
        cmd.Parameters.AddWithValue("@nr", nr);
        cmd.Parameters.AddWithValue("@dzis", DateTime.Today.ToString("yyyy-MM-dd"));
        var lista = new List<Rezerwacja>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            lista.Add(new Rezerwacja
            {
                Id = r.GetInt32(0), StanowiskoNumer = nr,
                Wedkarz = r.GetString(1), Telefon = r.GetString(2),
                DataOd = DateTime.Parse(r.GetString(3)), DataDo = DateTime.Parse(r.GetString(4)),
                GodzinaOd = TimeOnly.Parse(r.GetString(5)), GodzinaDo = TimeOnly.Parse(r.GetString(6)),
                TypPobytu = r.GetString(7), IloscLowiacych = r.GetInt32(8), IloscWedek = r.GetInt32(9),
                OsobaTowarzyszaca = r.GetInt32(10) == 1, Cena = (decimal)r.GetDouble(11)
            });
        return lista;
    }

    public List<Rezerwacja> PobierzWszystkie()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT Id, StanowiskoNumer, Wedkarz, Telefon, DataOd, DataDo, GodzinaOd, GodzinaDo, TypPobytu, IloscLowiacych, IloscWedek, OsobaTowarzyszaca, Cena FROM Rezerwacje ORDER BY DataOd";
        var lista = new List<Rezerwacja>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            lista.Add(new Rezerwacja
            {
                Id = r.GetInt32(0), StanowiskoNumer = r.GetInt32(1),
                Wedkarz = r.GetString(2), Telefon = r.GetString(3),
                DataOd = DateTime.Parse(r.GetString(4)), DataDo = DateTime.Parse(r.GetString(5)),
                GodzinaOd = TimeOnly.Parse(r.GetString(6)), GodzinaDo = TimeOnly.Parse(r.GetString(7)),
                TypPobytu = r.GetString(8), IloscLowiacych = r.GetInt32(9), IloscWedek = r.GetInt32(10),
                OsobaTowarzyszaca = r.GetInt32(11) == 1, Cena = (decimal)r.GetDouble(12)
            });
        return lista;
    }

    public void ZapiszPozycje(List<Stanowisko> stanowiska)
    {
        using var t = _conn.BeginTransaction();
        using var del = _conn.CreateCommand();
        del.CommandText = "DELETE FROM PozycjeStanowisk";
        del.ExecuteNonQuery();
        foreach (var st in stanowiska)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "INSERT INTO PozycjeStanowisk (Numer, X, Y) VALUES (@n, @x, @y)";
            cmd.Parameters.AddWithValue("@n", st.Numer);
            cmd.Parameters.AddWithValue("@x", st.X);
            cmd.Parameters.AddWithValue("@y", st.Y);
            cmd.ExecuteNonQuery();
        }
        t.Commit();
    }

    public void WczytajPozycje(List<Stanowisko> stanowiska)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT Numer, X, Y FROM PozycjeStanowisk";
        using var r = cmd.ExecuteReader();
        var dict = new Dictionary<int, (double x, double y)>();
        while (r.Read()) dict[r.GetInt32(0)] = (r.GetDouble(1), r.GetDouble(2));
        foreach (var st in stanowiska)
            if (dict.TryGetValue(st.Numer, out var p)) { st.X = p.x; st.Y = p.y; }
    }

    public void Dispose() { _conn.Close(); _conn.Dispose(); }
}
