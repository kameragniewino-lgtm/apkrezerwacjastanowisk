using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using LowiskoWeb.Models;
using Npgsql;
using Microsoft.Data.Sqlite;

namespace LowiskoWeb.Services;

public sealed class BazaDanych : IDisposable
{
    private readonly DbConnection _conn;
    private readonly bool _isPg;

    public BazaDanych()
    {
        var pgConn = Environment.GetEnvironmentVariable("DATABASE_URL")
                  ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL");

        if (!string.IsNullOrEmpty(pgConn))
        {
            _isPg = true;
            _conn = new NpgsqlConnection(KonwertujUrl(pgConn));
        }
        else
        {
            _isPg = false;
            var dbPath = Path.Combine(AppContext.BaseDirectory, "lowisko.db");
            _conn = new SqliteConnection($"Data Source={dbPath}");
        }
        _conn.Open();
        UtworzTabele();
    }

    private static string KonwertujUrl(string url)
    {
        if (url.StartsWith("postgresql://") || url.StartsWith("postgres://"))
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':');
            return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        }
        return url;
    }

    private DbCommand Cmd(string sql)
    {
        var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        return cmd;
    }

    private void Param(DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    private void UtworzTabele()
    {
        if (_isPg)
        {
            Exec(@"
                CREATE TABLE IF NOT EXISTS Rezerwacje (
                    Id SERIAL PRIMARY KEY,
                    StanowiskoNumer INTEGER NOT NULL,
                    Wedkarz TEXT NOT NULL,
                    Telefon TEXT DEFAULT '',
                    DataOd DATE NOT NULL,
                    DataDo DATE NOT NULL,
                    GodzinaOd TEXT NOT NULL DEFAULT '06:00',
                    GodzinaDo TEXT NOT NULL DEFAULT '21:00',
                    TypPobytu TEXT NOT NULL DEFAULT 'Dzien',
                    IloscLowiacych INTEGER NOT NULL DEFAULT 1,
                    IloscWedek INTEGER NOT NULL DEFAULT 2,
                    OsobaTowarzyszaca BOOLEAN NOT NULL DEFAULT FALSE,
                    Cena DECIMAL(10,2) NOT NULL DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS PozycjeStanowisk (
                    Numer INTEGER PRIMARY KEY,
                    X DOUBLE PRECISION NOT NULL,
                    Y DOUBLE PRECISION NOT NULL
                );
                CREATE TABLE IF NOT EXISTS Administratorzy (
                    Id SERIAL PRIMARY KEY,
                    Login TEXT NOT NULL UNIQUE,
                    HasloHash TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS Ustawienia (
                    Klucz TEXT PRIMARY KEY,
                    Wartosc TEXT NOT NULL
                );
            ");
        }
        else
        {
            Exec(@"
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
            ");
            // Migracje SQLite
            string[] migracje =
            [
                "ALTER TABLE Rezerwacje ADD COLUMN GodzinaOd TEXT NOT NULL DEFAULT '06:00'",
                "ALTER TABLE Rezerwacje ADD COLUMN GodzinaDo TEXT NOT NULL DEFAULT '21:00'",
                "ALTER TABLE Rezerwacje ADD COLUMN IloscLowiacych INTEGER NOT NULL DEFAULT 1",
                "ALTER TABLE Rezerwacje ADD COLUMN IloscWedek INTEGER NOT NULL DEFAULT 2",
                "ALTER TABLE Rezerwacje ADD COLUMN TypPobytu TEXT NOT NULL DEFAULT 'Dzien'",
                "ALTER TABLE Rezerwacje ADD COLUMN OsobaTowarzyszaca INTEGER NOT NULL DEFAULT 0",
                "ALTER TABLE Rezerwacje ADD COLUMN Cena REAL NOT NULL DEFAULT 0",
            ];
            foreach (var sql in migracje)
            {
                try { Exec(sql); } catch { }
            }
        }

        // Domyslny admin
        using var check = Cmd("SELECT COUNT(*) FROM Administratorzy");
        if (Convert.ToInt64(check.ExecuteScalar()!) == 0)
            DodajAdmina("admin", "admin123");

        // Domyslne ustawienia
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

    private void Exec(string sql)
    {
        using var cmd = Cmd(sql);
        cmd.ExecuteNonQuery();
    }

    private void UstawDomyslne(string klucz, string wartosc)
    {
        using var cmd = Cmd(_isPg
            ? "INSERT INTO Ustawienia (Klucz, Wartosc) VALUES (@k, @v) ON CONFLICT (Klucz) DO NOTHING"
            : "INSERT OR IGNORE INTO Ustawienia (Klucz, Wartosc) VALUES (@k, @v)");
        Param(cmd, "@k", klucz);
        Param(cmd, "@v", wartosc);
        cmd.ExecuteNonQuery();
    }

    public string PobierzUstawienie(string klucz, string domyslna = "")
    {
        using var cmd = Cmd("SELECT Wartosc FROM Ustawienia WHERE Klucz = @k");
        Param(cmd, "@k", klucz);
        return cmd.ExecuteScalar() is string v ? v : domyslna;
    }

    public void ZapiszUstawienie(string klucz, string wartosc)
    {
        using var cmd = Cmd(_isPg
            ? "INSERT INTO Ustawienia (Klucz, Wartosc) VALUES (@k, @v) ON CONFLICT (Klucz) DO UPDATE SET Wartosc = EXCLUDED.Wartosc"
            : "INSERT OR REPLACE INTO Ustawienia (Klucz, Wartosc) VALUES (@k, @v)");
        Param(cmd, "@k", klucz);
        Param(cmd, "@v", wartosc);
        cmd.ExecuteNonQuery();
    }

    private static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));

    public void DodajAdmina(string login, string haslo)
    {
        using var cmd = Cmd(_isPg
            ? "INSERT INTO Administratorzy (Login, HasloHash) VALUES (@l, @h) ON CONFLICT (Login) DO NOTHING"
            : "INSERT OR IGNORE INTO Administratorzy (Login, HasloHash) VALUES (@l, @h)");
        Param(cmd, "@l", login);
        Param(cmd, "@h", Hash(haslo));
        cmd.ExecuteNonQuery();
    }

    public bool SprawdzLogowanie(string login, string haslo)
    {
        using var cmd = Cmd("SELECT HasloHash FROM Administratorzy WHERE Login = @l");
        Param(cmd, "@l", login);
        return cmd.ExecuteScalar() is string h && h == Hash(haslo);
    }

    public int DodajRezerwacje(Rezerwacja r)
    {
        var sql = _isPg
            ? @"INSERT INTO Rezerwacje (StanowiskoNumer, Wedkarz, Telefon, DataOd, DataDo, GodzinaOd, GodzinaDo, TypPobytu, IloscLowiacych, IloscWedek, OsobaTowarzyszaca, Cena)
               VALUES (@nr, @kto, @tel, @od, @do, @god_od, @god_do, @typ, @il, @wed, @os, @cena) RETURNING Id"
            : @"INSERT INTO Rezerwacje (StanowiskoNumer, Wedkarz, Telefon, DataOd, DataDo, GodzinaOd, GodzinaDo, TypPobytu, IloscLowiacych, IloscWedek, OsobaTowarzyszaca, Cena)
               VALUES (@nr, @kto, @tel, @od, @do, @god_od, @god_do, @typ, @il, @wed, @os, @cena); SELECT last_insert_rowid();";
        using var cmd = Cmd(sql);
        Param(cmd, "@nr", r.StanowiskoNumer);
        Param(cmd, "@kto", r.Wedkarz);
        Param(cmd, "@tel", r.Telefon ?? "");
        Param(cmd, "@od", _isPg ? (object)DateOnly.FromDateTime(r.DataOd) : r.DataOd.ToString("yyyy-MM-dd"));
        Param(cmd, "@do", _isPg ? (object)DateOnly.FromDateTime(r.DataDo) : r.DataDo.ToString("yyyy-MM-dd"));
        Param(cmd, "@god_od", r.GodzinaOd.ToString("HH:mm"));
        Param(cmd, "@god_do", r.GodzinaDo.ToString("HH:mm"));
        Param(cmd, "@typ", r.TypPobytu);
        Param(cmd, "@il", r.IloscLowiacych);
        Param(cmd, "@wed", r.IloscWedek);
        Param(cmd, "@os", _isPg ? (object)r.OsobaTowarzyszaca : (r.OsobaTowarzyszaca ? 1 : 0));
        Param(cmd, "@cena", r.Cena);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void UsunRezerwacje(int id)
    {
        using var cmd = Cmd("DELETE FROM Rezerwacje WHERE Id = @id");
        Param(cmd, "@id", id);
        cmd.ExecuteNonQuery();
    }

    private Rezerwacja CzytajRezerwacje(DbDataReader r, bool withNr = false)
    {
        int i = 0;
        var rez = new Rezerwacja { Id = r.GetInt32(i++) };
        if (withNr) rez.StanowiskoNumer = r.GetInt32(i++);
        rez.Wedkarz = r.GetString(i++);
        rez.Telefon = r.GetString(i++);
        rez.DataOd = r.GetDateTime(i++);
        rez.DataDo = r.GetDateTime(i++);
        rez.GodzinaOd = TimeOnly.Parse(r.GetString(i++));
        rez.GodzinaDo = TimeOnly.Parse(r.GetString(i++));
        rez.TypPobytu = r.GetString(i++);
        rez.IloscLowiacych = r.GetInt32(i++);
        rez.IloscWedek = r.GetInt32(i++);
        rez.OsobaTowarzyszaca = _isPg ? r.GetBoolean(i++) : r.GetInt32(i++) == 1;
        rez.Cena = r.GetDecimal(i++);
        return rez;
    }

    public List<Rezerwacja> PobierzRezerwacje(int nr)
    {
        var dzis = _isPg ? "@dzis::date" : "@dzis";
        using var cmd = Cmd($"SELECT Id, Wedkarz, Telefon, DataOd, DataDo, GodzinaOd, GodzinaDo, TypPobytu, IloscLowiacych, IloscWedek, OsobaTowarzyszaca, Cena FROM Rezerwacje WHERE StanowiskoNumer = @nr AND DataDo >= {dzis} ORDER BY DataOd");
        Param(cmd, "@nr", nr);
        Param(cmd, "@dzis", _isPg ? (object)DateOnly.FromDateTime(DateTime.Today) : DateTime.Today.ToString("yyyy-MM-dd"));
        var lista = new List<Rezerwacja>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var rez = CzytajRezerwacje(r);
            rez.StanowiskoNumer = nr;
            lista.Add(rez);
        }
        return lista;
    }

    public List<Rezerwacja> PobierzWszystkie()
    {
        using var cmd = Cmd("SELECT Id, StanowiskoNumer, Wedkarz, Telefon, DataOd, DataDo, GodzinaOd, GodzinaDo, TypPobytu, IloscLowiacych, IloscWedek, OsobaTowarzyszaca, Cena FROM Rezerwacje ORDER BY DataOd");
        var lista = new List<Rezerwacja>();
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(CzytajRezerwacje(r, withNr: true));
        return lista;
    }

    public void ZapiszPozycje(List<Stanowisko> stanowiska)
    {
        using var t = _conn.BeginTransaction();
        Exec("DELETE FROM PozycjeStanowisk");
        foreach (var st in stanowiska)
        {
            using var cmd = Cmd(_isPg
                ? "INSERT INTO PozycjeStanowisk (Numer, X, Y) VALUES (@n, @x, @y) ON CONFLICT (Numer) DO UPDATE SET X = EXCLUDED.X, Y = EXCLUDED.Y"
                : "INSERT OR REPLACE INTO PozycjeStanowisk (Numer, X, Y) VALUES (@n, @x, @y)");
            Param(cmd, "@n", st.Numer);
            Param(cmd, "@x", st.X);
            Param(cmd, "@y", st.Y);
            cmd.ExecuteNonQuery();
        }
        t.Commit();
    }

    public void WczytajPozycje(List<Stanowisko> stanowiska)
    {
        using var cmd = Cmd("SELECT Numer, X, Y FROM PozycjeStanowisk");
        using var r = cmd.ExecuteReader();
        var dict = new Dictionary<int, (double x, double y)>();
        while (r.Read()) dict[r.GetInt32(0)] = (r.GetDouble(1), r.GetDouble(2));
        foreach (var st in stanowiska)
            if (dict.TryGetValue(st.Numer, out var p)) { st.X = p.x; st.Y = p.y; }
    }

    public void Dispose() { _conn.Close(); _conn.Dispose(); }
}
