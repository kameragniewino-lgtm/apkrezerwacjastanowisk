using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace apkrezerwacjastanowisk
{
    public sealed class BazaDanych : IDisposable
    {
        private readonly SqliteConnection _conn;

        public BazaDanych()
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "lowisko.db");
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
                    DataDo TEXT NOT NULL
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
                """;
            cmd.ExecuteNonQuery();

            // Domyślny admin jeśli nie ma żadnego
            using var check = _conn.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM Administratorzy";
            long count = (long)check.ExecuteScalar()!;
            if (count == 0)
                DodajAdmina("admin", "admin123");
        }

        #region Administratorzy

        private static string HashHasla(string haslo)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(haslo));
            return Convert.ToHexString(bytes);
        }

        public void DodajAdmina(string login, string haslo)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO Administratorzy (Login, HasloHash) VALUES (@l, @h)";
            cmd.Parameters.AddWithValue("@l", login);
            cmd.Parameters.AddWithValue("@h", HashHasla(haslo));
            cmd.ExecuteNonQuery();
        }

        public bool SprawdzLogowanie(string login, string haslo)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT HasloHash FROM Administratorzy WHERE Login = @l";
            cmd.Parameters.AddWithValue("@l", login);
            var result = cmd.ExecuteScalar();
            return result is string hash && hash == HashHasla(haslo);
        }

        public void ZmienHaslo(string login, string noweHaslo)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "UPDATE Administratorzy SET HasloHash = @h WHERE Login = @l";
            cmd.Parameters.AddWithValue("@l", login);
            cmd.Parameters.AddWithValue("@h", HashHasla(noweHaslo));
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region Rezerwacje

        public int DodajRezerwacje(Rezerwacja r)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Rezerwacje (StanowiskoNumer, Wedkarz, Telefon, DataOd, DataDo)
                VALUES (@nr, @kto, @tel, @od, @do);
                SELECT last_insert_rowid();
                """;
            cmd.Parameters.AddWithValue("@nr", r.StanowiskoNumer);
            cmd.Parameters.AddWithValue("@kto", r.Wędkarz);
            cmd.Parameters.AddWithValue("@tel", r.Telefon ?? "");
            cmd.Parameters.AddWithValue("@od", r.DataOd.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@do", r.DataDo.ToString("yyyy-MM-dd"));
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void UsunRezerwacje(int id)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Rezerwacje WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public List<Rezerwacja> PobierzRezerwacje(int stanowiskoNumer)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Wedkarz, Telefon, DataOd, DataDo FROM Rezerwacje WHERE StanowiskoNumer = @nr ORDER BY DataOd";
            cmd.Parameters.AddWithValue("@nr", stanowiskoNumer);
            var lista = new List<Rezerwacja>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Rezerwacja
                {
                    Id = reader.GetInt32(0),
                    StanowiskoNumer = stanowiskoNumer,
                    Wędkarz = reader.GetString(1),
                    Telefon = reader.GetString(2),
                    DataOd = DateTime.Parse(reader.GetString(3)),
                    DataDo = DateTime.Parse(reader.GetString(4))
                });
            }
            return lista;
        }

        public List<Rezerwacja> PobierzWszystkieRezerwacje()
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT Id, StanowiskoNumer, Wedkarz, Telefon, DataOd, DataDo FROM Rezerwacje ORDER BY DataOd";
            var lista = new List<Rezerwacja>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Rezerwacja
                {
                    Id = reader.GetInt32(0),
                    StanowiskoNumer = reader.GetInt32(1),
                    Wędkarz = reader.GetString(2),
                    Telefon = reader.GetString(3),
                    DataOd = DateTime.Parse(reader.GetString(4)),
                    DataDo = DateTime.Parse(reader.GetString(5))
                });
            }
            return lista;
        }

        public void WczytajRezerwacjeDlaStanowisk(List<Stanowisko> stanowiska)
        {
            foreach (var st in stanowiska)
                st.Rezerwacje = PobierzRezerwacje(st.Numer);
        }

        #endregion

        #region Pozycje stanowisk

        public void ZapiszPozycje(List<Stanowisko> stanowiska)
        {
            using var trans = _conn.BeginTransaction();
            using var del = _conn.CreateCommand();
            del.CommandText = "DELETE FROM PozycjeStanowisk";
            del.ExecuteNonQuery();

            foreach (var st in stanowiska)
            {
                using var cmd = _conn.CreateCommand();
                cmd.CommandText = "INSERT INTO PozycjeStanowisk (Numer, X, Y) VALUES (@n, @x, @y)";
                cmd.Parameters.AddWithValue("@n", st.Numer);
                cmd.Parameters.AddWithValue("@x", st.PozycjaNorm.X);
                cmd.Parameters.AddWithValue("@y", st.PozycjaNorm.Y);
                cmd.ExecuteNonQuery();
            }
            trans.Commit();
        }

        public void WczytajPozycje(List<Stanowisko> stanowiska)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT Numer, X, Y FROM PozycjeStanowisk";
            using var reader = cmd.ExecuteReader();
            var dict = new Dictionary<int, PointF>();
            while (reader.Read())
                dict[reader.GetInt32(0)] = new PointF(reader.GetFloat(1), reader.GetFloat(2));

            foreach (var st in stanowiska)
                if (dict.TryGetValue(st.Numer, out var pos))
                    st.PozycjaNorm = pos;
        }

        #endregion

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}
