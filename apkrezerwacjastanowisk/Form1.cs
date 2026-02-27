namespace apkrezerwacjastanowisk
{
    public partial class Form1 : Form
    {
        private readonly List<Stanowisko> stanowiska = [];
        private readonly BazaDanych db = new();
        private Stanowisko? wybraneStanowisko;
        private int? hoverIndex;
        private Image? mapaImage;
        private bool zalogowanyAdmin;

        // Pan & zoom
        private float mapZoom = 1.0f;
        private float mapOffsetX = 0f;
        private float mapOffsetY = 0f;
        private bool isDraggingMap;
        private Point dragStartMouse;
        private float dragStartOffsetX;
        private float dragStartOffsetY;
        private const float ZoomMin = 0.5f;
        private const float ZoomMax = 4.0f;
        private const float ZoomStep = 0.15f;

        // Tryb edycji stanowisk
        private bool trybEdycji;
        private Stanowisko? przeciaganeStanowisko;

        // Wybrany zakres dat
        private DateTime WybranaDataOd => dtpOd.Value.Date;
        private DateTime WybranaDataDo => dtpDo.Value.Date;

        public Form1()
        {
            InitializeComponent();
            WczytajMape();
            UtworzStanowiska();
            db.WczytajPozycje(stanowiska);
            db.WczytajRezerwacjeDlaStanowisk(stanowiska);
            OdswiezPodsumowanie();
            OdswiezPrzyciskiAdmin();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            db.Dispose();
            base.OnFormClosed(e);
        }

        private void WczytajMape()
        {
            string sciezka = Path.Combine(AppContext.BaseDirectory, "mapa.png");
            if (File.Exists(sciezka))
                mapaImage = Image.FromFile(sciezka);
        }

        private void UtworzStanowiska()
        {
            (int nr, float x, float y)[] pozycje =
            [
                ( 1, 0.352f, 0.066f), ( 2, 0.307f, 0.139f), ( 3, 0.307f, 0.193f),
                ( 4, 0.285f, 0.240f), ( 5, 0.201f, 0.303f), ( 6, 0.181f, 0.346f),
                ( 7, 0.110f, 0.413f), ( 8, 0.088f, 0.476f), ( 9, 0.097f, 0.530f),
                (10, 0.078f, 0.584f), (11, 0.071f, 0.645f), (12, 0.101f, 0.693f),
                (13, 0.142f, 0.744f), (14, 0.181f, 0.781f), (15, 0.220f, 0.850f),
                (16, 0.256f, 0.889f), (17, 0.595f, 0.955f), (18, 0.617f, 0.916f),
                (19, 0.660f, 0.874f), (20, 0.718f, 0.835f), (21, 0.773f, 0.789f),
                (22, 0.841f, 0.740f), (23, 0.822f, 0.682f), (24, 0.854f, 0.623f),
                (25, 0.867f, 0.565f), (26, 0.890f, 0.513f), (27, 0.919f, 0.449f),
                (28, 0.893f, 0.389f), (29, 0.932f, 0.332f), (30, 0.932f, 0.269f),
            ];

            foreach (var (nr, x, y) in pozycje)
                stanowiska.Add(new Stanowisko { Numer = nr, PozycjaNorm = new PointF(x, y) });
        }

        #region Data changed

        private void DtpOd_ValueChanged(object? sender, EventArgs e)
        {
            if (dtpDo.Value < dtpOd.Value)
                dtpDo.Value = dtpOd.Value;
            OdswiezPodsumowanie();
            OdswiezInfoWybranego();
            panelMapa.Invalidate();
        }

        private void DtpDo_ValueChanged(object? sender, EventArgs e)
        {
            if (dtpDo.Value < dtpOd.Value)
                dtpOd.Value = dtpDo.Value;
            OdswiezPodsumowanie();
            OdswiezInfoWybranego();
            panelMapa.Invalidate();
        }

        #endregion

        private RectangleF PobierzRectMapy()
        {
            if (mapaImage == null)
                return new RectangleF(0, 0, panelMapa.ClientSize.Width, panelMapa.ClientSize.Height);

            float pw = panelMapa.ClientSize.Width;
            float ph = panelMapa.ClientSize.Height;
            float imgRatio = (float)mapaImage.Width / mapaImage.Height;
            float panelRatio = pw / ph;

            float baseW, baseH;
            if (panelRatio > imgRatio) { baseH = ph; baseW = ph * imgRatio; }
            else { baseW = pw; baseH = pw / imgRatio; }

            float drawW = baseW * mapZoom;
            float drawH = baseH * mapZoom;
            float drawX = (pw - baseW) / 2f + mapOffsetX - (drawW - baseW) / 2f;
            float drawY = (ph - baseH) / 2f + mapOffsetY - (drawH - baseH) / 2f;

            return new RectangleF(drawX, drawY, drawW, drawH);
        }

        private void PanelMapa_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            var imgRect = PobierzRectMapy();

            if (mapaImage != null)
                g.DrawImage(mapaImage, imgRect.X, imgRect.Y, imgRect.Width, imgRect.Height);

            float spotR = Stanowisko.PobierzPromien(imgRect);
            float fontSizeNr = Math.Max(7f, spotR * 0.7f);
            float fontSizeNrSmall = Math.Max(6f, spotR * 0.6f);
            using var fontNr = new Font("Segoe UI", fontSizeNr, FontStyle.Bold);
            using var fontNrSmall = new Font("Segoe UI", fontSizeNrSmall, FontStyle.Bold);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            DateTime od = WybranaDataOd;
            DateTime doDaty = WybranaDataDo.AddDays(1); // do końca wybranego dnia

            for (int i = 0; i < stanowiska.Count; i++)
            {
                var st = stanowiska[i];
                var rect = st.PobierzGranice(imgRect);
                bool zajete = st.CzyZarezerwowane(od, doDaty);

                bool isHover = hoverIndex == i;
                bool isDraggedSpot = st == przeciaganeStanowisko;
                float inflate = (isHover || isDraggedSpot) ? spotR * 0.2f : 0f;
                var drawRect = inflate > 0
                    ? new RectangleF(rect.X - inflate, rect.Y - inflate,
                                     rect.Width + inflate * 2, rect.Height + inflate * 2)
                    : rect;

                Color fillColor;
                if (trybEdycji)
                {
                    fillColor = isDraggedSpot
                        ? Color.FromArgb(255, 255, 165, 0)
                        : Color.FromArgb(230, 50, 130, 220);
                }
                else
                {
                    fillColor = zajete
                        ? Color.FromArgb(255, 220, 50, 40)
                        : Color.FromArgb(255, 40, 170, 40);
                    if (isHover && !zajete) fillColor = Color.FromArgb(255, 60, 210, 60);
                    if (isHover && zajete) fillColor = Color.FromArgb(255, 240, 70, 60);
                }

                if (st == wybraneStanowisko && !trybEdycji)
                {
                    using var glowPen = new Pen(Color.FromArgb(200, 255, 255, 0), Math.Max(3f, spotR * 0.25f));
                    float glow = spotR * 0.35f;
                    g.DrawEllipse(glowPen, drawRect.X - glow, drawRect.Y - glow,
                        drawRect.Width + glow * 2, drawRect.Height + glow * 2);
                }

                using var spotBrush = new SolidBrush(fillColor);
                g.FillEllipse(spotBrush, drawRect);

                using var borderPen = new Pen(Color.White, Math.Max(1.5f, spotR * 0.12f));
                g.DrawEllipse(borderPen, drawRect);

                var usedFont = st.Numer >= 10 ? fontNrSmall : fontNr;
                g.DrawString(st.Numer.ToString(), usedFont, Brushes.White, drawRect, sf);
            }

            sf.Dispose();

            using var hintFont = new Font("Segoe UI", 8f);
            using var hintBg = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
            using var hintBrush = new SolidBrush(Color.FromArgb(230, 255, 255, 255));
            string hint = trybEdycji
                ? "TRYB EDYCJI — Przeciągnij kółka na właściwe miejsca."
                : $"Scroll = zoom ({mapZoom:0.0}x)  |  Przeciągnij = przesuń  |  Środkowy = reset";
            var hintSize = g.MeasureString(hint, hintFont);
            g.FillRectangle(hintBg, 2, panelMapa.ClientSize.Height - 22, hintSize.Width + 8, 20);
            g.DrawString(hint, hintFont, hintBrush, 6, panelMapa.ClientSize.Height - 20);

            if (trybEdycji)
            {
                using var editBorderPen = new Pen(Color.Orange, 3f);
                g.DrawRectangle(editBorderPen, 1, 1, panelMapa.ClientSize.Width - 3, panelMapa.ClientSize.Height - 3);
            }
        }

        #region Mouse handling

        private void PanelMapa_MouseDown(object? sender, MouseEventArgs e)
        {
            if (trybEdycji && e.Button == MouseButtons.Left)
            {
                var imgRect = PobierzRectMapy();
                for (int i = 0; i < stanowiska.Count; i++)
                {
                    if (stanowiska[i].CzyZawieraPunkt(e.Location, imgRect))
                    {
                        przeciaganeStanowisko = stanowiska[i];
                        panelMapa.Cursor = Cursors.Hand;
                        panelMapa.Invalidate();
                        return;
                    }
                }
            }

            if ((!trybEdycji && e.Button == MouseButtons.Left) ||
                (trybEdycji && e.Button == MouseButtons.Right))
            {
                isDraggingMap = true;
                dragStartMouse = e.Location;
                dragStartOffsetX = mapOffsetX;
                dragStartOffsetY = mapOffsetY;
                panelMapa.Cursor = Cursors.SizeAll;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                mapZoom = 1.0f; mapOffsetX = 0f; mapOffsetY = 0f;
                panelMapa.Invalidate();
            }
        }

        private void PanelMapa_MouseUp(object? sender, MouseEventArgs e)
        {
            if (trybEdycji && e.Button == MouseButtons.Left && przeciaganeStanowisko != null)
            {
                przeciaganeStanowisko = null;
                panelMapa.Cursor = Cursors.Default;
                panelMapa.Invalidate();
                return;
            }

            if ((!trybEdycji && e.Button == MouseButtons.Left) ||
                (trybEdycji && e.Button == MouseButtons.Right))
            {
                bool wasClick = Math.Abs(e.X - dragStartMouse.X) < 5
                             && Math.Abs(e.Y - dragStartMouse.Y) < 5;
                isDraggingMap = false;
                panelMapa.Cursor = Cursors.Default;
                if (wasClick && !trybEdycji) ObsluzKlikniecie(e.Location);
            }
        }

        private void PanelMapa_MouseMove(object? sender, MouseEventArgs e)
        {
            if (trybEdycji && przeciaganeStanowisko != null)
            {
                var imgRect = PobierzRectMapy();
                float nx = (e.X - imgRect.X) / imgRect.Width;
                float ny = (e.Y - imgRect.Y) / imgRect.Height;
                przeciaganeStanowisko.PozycjaNorm = new PointF(Math.Clamp(nx, 0f, 1f), Math.Clamp(ny, 0f, 1f));
                panelMapa.Invalidate();
                return;
            }

            if (isDraggingMap)
            {
                mapOffsetX = dragStartOffsetX + (e.X - dragStartMouse.X);
                mapOffsetY = dragStartOffsetY + (e.Y - dragStartMouse.Y);
                panelMapa.Invalidate();
                return;
            }

            var imgR = PobierzRectMapy();
            int? nowyHover = null;
            for (int i = 0; i < stanowiska.Count; i++)
                if (stanowiska[i].CzyZawieraPunkt(e.Location, imgR)) { nowyHover = i; break; }

            if (nowyHover != hoverIndex)
            {
                hoverIndex = nowyHover;
                panelMapa.Cursor = hoverIndex.HasValue ? Cursors.Hand : Cursors.Default;
                panelMapa.Invalidate();
            }
        }

        private void PanelMapa_MouseWheel(object? sender, MouseEventArgs e)
        {
            float oldZoom = mapZoom;
            mapZoom = e.Delta > 0
                ? Math.Min(ZoomMax, mapZoom + ZoomStep)
                : Math.Max(ZoomMin, mapZoom - ZoomStep);
            if (Math.Abs(oldZoom - mapZoom) > 0.001f) panelMapa.Invalidate();
        }

        #endregion

        private void ObsluzKlikniecie(Point location)
        {
            var imgRect = PobierzRectMapy();
            DateTime od = WybranaDataOd;
            DateTime doDaty = WybranaDataDo.AddDays(1);

            for (int i = 0; i < stanowiska.Count; i++)
            {
                if (!stanowiska[i].CzyZawieraPunkt(location, imgRect)) continue;
                var st = stanowiska[i];

                if (!st.CzyZarezerwowane(od, doDaty))
                {
                    var rez = PokazDialogRezerwacji(st.Numer, WybranaDataOd, WybranaDataDo);
                    if (rez != null)
                    {
                        rez.Id = db.DodajRezerwacje(rez);
                        st.Rezerwacje.Add(rez);
                    }
                }

                wybraneStanowisko = st;
                OdswiezInfoWybranego();
                OdswiezPodsumowanie();
                panelMapa.Invalidate();
                return;
            }

            wybraneStanowisko = null;
            lblWybraneInfo.Text = "Kliknij stanowisko na mapie,\naby je zarezerwować.";
            btnAnuluj.Visible = zalogowanyAdmin && false;
            panelMapa.Invalidate();
        }

        private Rezerwacja? PokazDialogRezerwacji(int numer, DateTime domyslnaOd, DateTime domyslnaDo)
        {
            using var dialog = new Form
            {
                Text = $"Rezerwacja stanowiska nr {numer}",
                Size = new Size(420, 370),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false
            };

            var lblNazwa = new Label { Text = "Imię i nazwisko:", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 10f) };
            var txtNazwa = new TextBox { Location = new Point(20, 40), Size = new Size(360, 30), Font = new Font("Segoe UI", 11f) };

            var lblTel = new Label { Text = "Telefon:", Location = new Point(20, 78), AutoSize = true, Font = new Font("Segoe UI", 10f) };
            var txtTel = new TextBox { Location = new Point(20, 103), Size = new Size(360, 30), Font = new Font("Segoe UI", 11f) };

            var lblOd = new Label { Text = "Od:", Location = new Point(20, 143), AutoSize = true, Font = new Font("Segoe UI", 10f) };
            var dtOd = new DateTimePicker { Location = new Point(20, 168), Size = new Size(170, 30), Format = DateTimePickerFormat.Short, Value = domyslnaOd, Font = new Font("Segoe UI", 10f) };

            var lblDo = new Label { Text = "Do:", Location = new Point(210, 143), AutoSize = true, Font = new Font("Segoe UI", 10f) };
            var dtDo = new DateTimePicker { Location = new Point(210, 168), Size = new Size(170, 30), Format = DateTimePickerFormat.Short, Value = domyslnaDo, Font = new Font("Segoe UI", 10f) };

            var btnOk = new Button
            {
                Text = "Zarezerwuj", DialogResult = DialogResult.OK,
                Location = new Point(140, 215), Size = new Size(140, 45),
                BackColor = Color.FromArgb(50, 180, 50), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };

            dialog.Controls.AddRange([lblNazwa, txtNazwa, lblTel, txtTel, lblOd, dtOd, lblDo, dtDo, btnOk]);
            dialog.AcceptButton = btnOk;

            if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(txtNazwa.Text))
                return null;

            var dataOd = dtOd.Value.Date;
            var dataDo = dtDo.Value.Date;
            if (dataDo < dataOd) dataDo = dataOd;

            return new Rezerwacja
            {
                StanowiskoNumer = numer,
                Wędkarz = txtNazwa.Text.Trim(),
                Telefon = txtTel.Text.Trim(),
                DataOd = dataOd,
                DataDo = dataDo
            };
        }

        private void BtnAnuluj_Click(object? sender, EventArgs e)
        {
            if (wybraneStanowisko == null) return;

            DateTime od = WybranaDataOd;
            DateTime doDaty = WybranaDataDo.AddDays(1);
            var rez = wybraneStanowisko.PobierzRezerwacje(od, doDaty);
            if (rez == null) return;

            var result = MessageBox.Show(
                $"Anulować rezerwację stanowiska nr {wybraneStanowisko.Numer}?\n\n" +
                $"Wędkarz: {rez.Wędkarz}\n" +
                $"Termin: {rez.DataOd:dd.MM.yyyy} – {rez.DataDo:dd.MM.yyyy}",
                "Anulowanie rezerwacji",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                db.UsunRezerwacje(rez.Id);
                wybraneStanowisko.Rezerwacje.Remove(rez);
                OdswiezInfoWybranego();
                OdswiezPodsumowanie();
                panelMapa.Invalidate();
            }
        }

        private void BtnEdycja_Click(object? sender, EventArgs e)
        {
            trybEdycji = !trybEdycji;
            if (trybEdycji)
            {
                btnEdycja.Text = "💾 Zapisz pozycje";
                btnEdycja.BackColor = Color.FromArgb(230, 150, 30);
                lblWybraneInfo.Text = "TRYB EDYCJI\n\nPrzeciągaj kółka lewym\nprzyciskiem myszy.\n\nPrawym przyciskiem\nprzesuwasz mapę.";
                btnAnuluj.Visible = false;
            }
            else
            {
                db.ZapiszPozycje(stanowiska);
                btnEdycja.Text = "✏️ Ustaw stanowiska";
                btnEdycja.BackColor = Color.FromArgb(80, 130, 200);
                lblWybraneInfo.Text = "Pozycje zapisane!";
            }
            panelMapa.Invalidate();
        }

        #region Admin

        private void BtnAdmin_Click(object? sender, EventArgs e)
        {
            if (zalogowanyAdmin)
            {
                zalogowanyAdmin = false;
                OdswiezPrzyciskiAdmin();
                lblWybraneInfo.Text = "Wylogowano z panelu administratora.";
                return;
            }

            using var loginForm = new FormLogowanie();
            if (loginForm.ShowDialog(this) != DialogResult.OK) return;

            if (db.SprawdzLogowanie(loginForm.PodanyLogin, loginForm.PodaneHaslo))
            {
                zalogowanyAdmin = true;
                OdswiezPrzyciskiAdmin();
                lblWybraneInfo.Text = "Zalogowano jako administrator.\n\nMasz dostęp do:\n• Panel rezerwacji\n• Edycja stanowisk\n• Anulowanie rezerwacji";
            }
            else
            {
                MessageBox.Show("Nieprawidłowy login lub hasło.", "Błąd logowania",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnPanelAdmin_Click(object? sender, EventArgs e)
        {
            using var adminForm = new FormAdmin(db, stanowiska);
            adminForm.ShowDialog(this);

            if (adminForm.ZmienioneRezerwacje)
            {
                OdswiezInfoWybranego();
                OdswiezPodsumowanie();
                panelMapa.Invalidate();
            }
        }

        private void OdswiezPrzyciskiAdmin()
        {
            if (zalogowanyAdmin)
            {
                btnAdmin.Text = "🔓 Wyloguj";
                btnAdmin.BackColor = Color.FromArgb(180, 80, 60);
                btnPanelAdmin.Visible = true;
                btnEdycja.Visible = true;
                btnAnuluj.Visible = false;
            }
            else
            {
                btnAdmin.Text = "🔐 Admin";
                btnAdmin.BackColor = Color.FromArgb(100, 100, 100);
                btnPanelAdmin.Visible = false;
                btnEdycja.Visible = false;
                btnAnuluj.Visible = false;
            }
        }

        #endregion

        private void PanelMapa_Resize(object? sender, EventArgs e) => panelMapa.Invalidate();

        private void OdswiezInfoWybranego()
        {
            if (wybraneStanowisko == null)
            {
                lblWybraneInfo.Text = "Kliknij stanowisko na mapie,\naby je zarezerwować.";
                btnAnuluj.Visible = false;
                return;
            }

            var st = wybraneStanowisko;
            DateTime od = WybranaDataOd;
            DateTime doDaty = WybranaDataDo.AddDays(1);
            var rez = st.PobierzRezerwacje(od, doDaty);

            string txt = $"Stanowisko nr {st.Numer}\n";

            if (rez != null)
            {
                txt += $"\n🔴 ZAJĘTE w wybranym terminie\n" +
                       $"Wędkarz: {rez.Wędkarz}\n" +
                       (string.IsNullOrEmpty(rez.Telefon) ? "" : $"Tel: {rez.Telefon}\n") +
                       $"Od: {rez.DataOd:dd.MM.yyyy}\n" +
                       $"Do: {rez.DataDo:dd.MM.yyyy}";
                btnAnuluj.Visible = zalogowanyAdmin;
            }
            else
            {
                txt += "\n🟢 WOLNE w wybranym terminie\n\nKliknij, aby zarezerwować.";
                btnAnuluj.Visible = false;
            }

            if (st.Rezerwacje.Count > 0)
            {
                txt += $"\n\n— Rezerwacje ({st.Rezerwacje.Count}) —";
                foreach (var r in st.Rezerwacje.OrderBy(r => r.DataOd))
                    txt += $"\n• {r.DataOd:dd.MM} – {r.DataDo:dd.MM} {r.Wędkarz}";
            }

            lblWybraneInfo.Text = txt;
        }

        private void OdswiezPodsumowanie()
        {
            DateTime od = WybranaDataOd;
            DateTime doDaty = WybranaDataDo.AddDays(1);
            int wolne = stanowiska.Count(s => !s.CzyZarezerwowane(od, doDaty));
            int zajete = stanowiska.Count - wolne;
            lblPodsumowanie.Text = $"Wolne: {wolne} / {stanowiska.Count}\nZajęte: {zajete} / {stanowiska.Count}";
        }
    }
}
