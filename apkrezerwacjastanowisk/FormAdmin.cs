namespace apkrezerwacjastanowisk
{
    public class FormAdmin : Form
    {
        private readonly BazaDanych _db;
        private readonly List<Stanowisko> _stanowiska;
        private readonly DataGridView dgvRezerwacje;
        private readonly Label lblStatus;
        private List<Rezerwacja> _rezerwacje = [];

        public bool ZmienioneRezerwacje { get; private set; }

        public FormAdmin(BazaDanych db, List<Stanowisko> stanowiska)
        {
            _db = db;
            _stanowiska = stanowiska;

            Text = "🛠️ Panel Administratora — Łowisko Gniewino";
            Size = new Size(900, 550);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(700, 400);
            Font = new Font("Segoe UI", 10f);

            var lblTytul = new Label
            {
                Text = "Wszystkie rezerwacje",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 80, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            dgvRezerwacje = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9.5f)
            };

            dgvRezerwacje.Columns.AddRange(
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50, FillWeight = 8 },
                new DataGridViewTextBoxColumn { Name = "Nr", HeaderText = "Stanowisko", Width = 80, FillWeight = 12 },
                new DataGridViewTextBoxColumn { Name = "Kto", HeaderText = "Wędkarz", FillWeight = 25 },
                new DataGridViewTextBoxColumn { Name = "Tel", HeaderText = "Telefon", FillWeight = 18 },
                new DataGridViewTextBoxColumn { Name = "Od", HeaderText = "Od", Width = 100, FillWeight = 15 },
                new DataGridViewTextBoxColumn { Name = "Do", HeaderText = "Do", Width = 100, FillWeight = 15 }
            );

            var panelDol = new Panel { Dock = DockStyle.Bottom, Height = 55 };

            var btnUsun = new Button
            {
                Text = "🗑️ Usuń zaznaczone",
                Location = new Point(10, 8),
                Size = new Size(200, 38),
                BackColor = Color.FromArgb(220, 70, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnUsun.Click += BtnUsun_Click;

            var btnZmienHaslo = new Button
            {
                Text = "🔑 Zmień hasło",
                Location = new Point(220, 8),
                Size = new Size(160, 38),
                BackColor = Color.FromArgb(80, 130, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnZmienHaslo.Click += BtnZmienHaslo_Click;

            lblStatus = new Label
            {
                Location = new Point(400, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(30, 80, 30)
            };

            panelDol.Controls.AddRange([btnUsun, btnZmienHaslo, lblStatus]);

            Controls.Add(dgvRezerwacje);
            Controls.Add(lblTytul);
            Controls.Add(panelDol);

            OdswiezListe();
        }

        private void OdswiezListe()
        {
            _rezerwacje = _db.PobierzWszystkieRezerwacje();
            dgvRezerwacje.Rows.Clear();
            foreach (var r in _rezerwacje)
            {
                dgvRezerwacje.Rows.Add(
                    r.Id,
                    $"Nr {r.StanowiskoNumer}",
                    r.Wędkarz,
                    r.Telefon,
                    r.DataOd.ToString("dd.MM.yyyy"),
                    r.DataDo.ToString("dd.MM.yyyy"));
            }
            lblStatus.Text = $"Łącznie rezerwacji: {_rezerwacje.Count}";
        }

        private void BtnUsun_Click(object? sender, EventArgs e)
        {
            if (dgvRezerwacje.SelectedRows.Count == 0) return;

            var ids = new List<int>();
            foreach (DataGridViewRow row in dgvRezerwacje.SelectedRows)
                ids.Add((int)row.Cells["Id"].Value);

            var result = MessageBox.Show(
                $"Usunąć {ids.Count} zaznaczonych rezerwacji?",
                "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            foreach (int id in ids)
                _db.UsunRezerwacje(id);

            ZmienioneRezerwacje = true;
            _db.WczytajRezerwacjeDlaStanowisk(_stanowiska);
            OdswiezListe();
        }

        private void BtnZmienHaslo_Click(object? sender, EventArgs e)
        {
            using var dialog = new Form
            {
                Text = "Zmiana hasła",
                Size = new Size(360, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false
            };

            var lbl = new Label { Text = "Nowe hasło:", Location = new Point(15, 15), AutoSize = true };
            var txt = new TextBox { Location = new Point(15, 40), Size = new Size(310, 30), UseSystemPasswordChar = true, Font = new Font("Segoe UI", 11f) };
            var btn = new Button
            {
                Text = "Zapisz", DialogResult = DialogResult.OK,
                Location = new Point(110, 85), Size = new Size(130, 38),
                BackColor = Color.FromArgb(50, 180, 50), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            dialog.Controls.AddRange([lbl, txt, btn]);
            dialog.AcceptButton = btn;

            if (dialog.ShowDialog(this) == DialogResult.OK && txt.Text.Length >= 4)
            {
                _db.ZmienHaslo("admin", txt.Text);
                MessageBox.Show("Hasło zmienione!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
