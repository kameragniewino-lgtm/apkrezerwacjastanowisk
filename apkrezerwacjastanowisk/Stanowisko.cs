namespace apkrezerwacjastanowisk
{
    public class Stanowisko
    {
        public int Numer { get; set; }
        public PointF PozycjaNorm { get; set; }
        public List<Rezerwacja> Rezerwacje { get; set; } = [];

        public bool CzyZarezerwowane(DateTime od, DateTime doDaty)
        {
            return Rezerwacje.Any(r => r.KolidujZ(od, doDaty));
        }

        public Rezerwacja? PobierzRezerwacje(DateTime od, DateTime doDaty)
        {
            return Rezerwacje.FirstOrDefault(r => r.KolidujZ(od, doDaty));
        }

        public static float PobierzPromien(RectangleF imageRect)
        {
            float baseR = Math.Min(imageRect.Width, imageRect.Height) * 0.022f;
            return Math.Clamp(baseR, 10f, 35f);
        }

        public PointF PobierzPozycje(RectangleF imageRect)
        {
            return new PointF(
                imageRect.X + PozycjaNorm.X * imageRect.Width,
                imageRect.Y + PozycjaNorm.Y * imageRect.Height);
        }

        public RectangleF PobierzGranice(RectangleF imageRect)
        {
            var pos = PobierzPozycje(imageRect);
            float r = PobierzPromien(imageRect);
            return new RectangleF(pos.X - r, pos.Y - r, r * 2, r * 2);
        }

        public bool CzyZawieraPunkt(Point punkt, RectangleF imageRect)
        {
            var pos = PobierzPozycje(imageRect);
            float r = PobierzPromien(imageRect);
            float dx = punkt.X - pos.X;
            float dy = punkt.Y - pos.Y;
            return dx * dx + dy * dy <= r * r;
        }
    }
}
