namespace apkrezerwacjastanowisk
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelInfo = new Panel();
            btnAdmin = new Button();
            btnPanelAdmin = new Button();
            btnEdycja = new Button();
            btnAnuluj = new Button();
            lblWybraneInfo = new Label();
            lblPodsumowanie = new Label();
            lblZajete = new Label();
            lblWolne = new Label();
            lblLegenda = new Label();
            lblTytulInfo = new Label();
            lblDataOd = new Label();
            dtpOd = new DateTimePicker();
            lblDataDo = new Label();
            dtpDo = new DateTimePicker();
            panelMapa = new MapPanel();
            lblTytul = new Label();
            panelInfo.SuspendLayout();
            SuspendLayout();
            // 
            // panelInfo
            // 
            panelInfo.AutoScroll = true;
            panelInfo.BackColor = Color.FromArgb(245, 245, 240);
            panelInfo.BorderStyle = BorderStyle.FixedSingle;
            panelInfo.Controls.Add(btnAdmin);
            panelInfo.Controls.Add(btnPanelAdmin);
            panelInfo.Controls.Add(btnEdycja);
            panelInfo.Controls.Add(btnAnuluj);
            panelInfo.Controls.Add(lblWybraneInfo);
            panelInfo.Controls.Add(lblPodsumowanie);
            panelInfo.Controls.Add(lblZajete);
            panelInfo.Controls.Add(lblWolne);
            panelInfo.Controls.Add(lblLegenda);
            panelInfo.Controls.Add(dtpDo);
            panelInfo.Controls.Add(lblDataDo);
            panelInfo.Controls.Add(dtpOd);
            panelInfo.Controls.Add(lblDataOd);
            panelInfo.Controls.Add(lblTytulInfo);
            panelInfo.Dock = DockStyle.Right;
            panelInfo.Location = new Point(900, 50);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new Size(300, 700);
            panelInfo.TabIndex = 1;
            // 
            // lblTytulInfo
            // 
            lblTytulInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTytulInfo.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTytulInfo.ForeColor = Color.FromArgb(30, 80, 30);
            lblTytulInfo.Location = new Point(10, 10);
            lblTytulInfo.Name = "lblTytulInfo";
            lblTytulInfo.Size = new Size(275, 28);
            lblTytulInfo.TabIndex = 0;
            lblTytulInfo.Text = "Wybierz termin";
            // 
            // lblDataOd
            // 
            lblDataOd.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDataOd.Location = new Point(10, 42);
            lblDataOd.Name = "lblDataOd";
            lblDataOd.Size = new Size(35, 20);
            lblDataOd.Text = "Od:";
            // 
            // dtpOd
            // 
            dtpOd.Font = new Font("Segoe UI", 9.5F);
            dtpOd.Format = DateTimePickerFormat.Short;
            dtpOd.Location = new Point(50, 38);
            dtpOd.Name = "dtpOd";
            dtpOd.Size = new Size(120, 27);
            dtpOd.TabIndex = 1;
            dtpOd.ValueChanged += DtpOd_ValueChanged;
            // 
            // lblDataDo
            // 
            lblDataDo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDataDo.Location = new Point(10, 72);
            lblDataDo.Name = "lblDataDo";
            lblDataDo.Size = new Size(35, 20);
            lblDataDo.Text = "Do:";
            // 
            // dtpDo
            // 
            dtpDo.Font = new Font("Segoe UI", 9.5F);
            dtpDo.Format = DateTimePickerFormat.Short;
            dtpDo.Location = new Point(50, 68);
            dtpDo.Name = "dtpDo";
            dtpDo.Size = new Size(120, 27);
            dtpDo.TabIndex = 2;
            dtpDo.ValueChanged += DtpDo_ValueChanged;
            // 
            // lblLegenda
            // 
            lblLegenda.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblLegenda.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLegenda.Location = new Point(10, 105);
            lblLegenda.Name = "lblLegenda";
            lblLegenda.Size = new Size(275, 20);
            lblLegenda.Text = "Legenda:";
            // 
            // lblWolne
            // 
            lblWolne.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblWolne.Font = new Font("Segoe UI", 9F);
            lblWolne.ForeColor = Color.FromArgb(34, 139, 34);
            lblWolne.Location = new Point(15, 126);
            lblWolne.Name = "lblWolne";
            lblWolne.Size = new Size(270, 20);
            lblWolne.Text = "\U0001f7e2  Wolne";
            // 
            // lblZajete
            // 
            lblZajete.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblZajete.Font = new Font("Segoe UI", 9F);
            lblZajete.ForeColor = Color.FromArgb(200, 50, 50);
            lblZajete.Location = new Point(15, 146);
            lblZajete.Name = "lblZajete";
            lblZajete.Size = new Size(270, 20);
            lblZajete.Text = "🔴  Zajęte w wybranym terminie";
            // 
            // lblPodsumowanie
            // 
            lblPodsumowanie.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPodsumowanie.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPodsumowanie.Location = new Point(10, 175);
            lblPodsumowanie.Name = "lblPodsumowanie";
            lblPodsumowanie.Size = new Size(275, 45);
            lblPodsumowanie.Text = "Wolne: 30 / 30";
            // 
            // lblWybraneInfo
            // 
            lblWybraneInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblWybraneInfo.Font = new Font("Segoe UI", 9F);
            lblWybraneInfo.Location = new Point(10, 225);
            lblWybraneInfo.Name = "lblWybraneInfo";
            lblWybraneInfo.Size = new Size(275, 280);
            lblWybraneInfo.Text = "Kliknij stanowisko na mapie,\naby je zarezerwować.";
            // 
            // btnAnuluj
            // 
            btnAnuluj.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnAnuluj.BackColor = Color.FromArgb(220, 80, 60);
            btnAnuluj.FlatStyle = FlatStyle.Flat;
            btnAnuluj.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAnuluj.ForeColor = Color.White;
            btnAnuluj.Location = new Point(15, 560);
            btnAnuluj.Name = "btnAnuluj";
            btnAnuluj.Size = new Size(265, 38);
            btnAnuluj.Text = "Anuluj rezerwację";
            btnAnuluj.UseVisualStyleBackColor = false;
            btnAnuluj.Visible = false;
            btnAnuluj.Click += BtnAnuluj_Click;
            // 
            // btnEdycja
            // 
            btnEdycja.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnEdycja.BackColor = Color.FromArgb(80, 130, 200);
            btnEdycja.FlatStyle = FlatStyle.Flat;
            btnEdycja.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnEdycja.ForeColor = Color.White;
            btnEdycja.Location = new Point(15, 518);
            btnEdycja.Name = "btnEdycja";
            btnEdycja.Size = new Size(265, 34);
            btnEdycja.Text = "\u270F\uFE0F Ustaw stanowiska";
            btnEdycja.UseVisualStyleBackColor = false;
            btnEdycja.Visible = false;
            btnEdycja.Click += BtnEdycja_Click;
            // 
            // btnPanelAdmin
            // 
            btnPanelAdmin.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnPanelAdmin.BackColor = Color.FromArgb(50, 120, 200);
            btnPanelAdmin.FlatStyle = FlatStyle.Flat;
            btnPanelAdmin.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnPanelAdmin.ForeColor = Color.White;
            btnPanelAdmin.Location = new Point(15, 558);
            btnPanelAdmin.Name = "btnPanelAdmin";
            btnPanelAdmin.Size = new Size(265, 34);
            btnPanelAdmin.Text = "🛠️ Panel rezerwacji";
            btnPanelAdmin.UseVisualStyleBackColor = false;
            btnPanelAdmin.Visible = false;
            btnPanelAdmin.Click += BtnPanelAdmin_Click;
            // 
            // btnAdmin
            // 
            btnAdmin.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnAdmin.BackColor = Color.FromArgb(100, 100, 100);
            btnAdmin.FlatStyle = FlatStyle.Flat;
            btnAdmin.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAdmin.ForeColor = Color.White;
            btnAdmin.Location = new Point(15, 600);
            btnAdmin.Name = "btnAdmin";
            btnAdmin.Size = new Size(265, 34);
            btnAdmin.Text = "\uD83D\uDD10 Admin";
            btnAdmin.UseVisualStyleBackColor = false;
            btnAdmin.Click += BtnAdmin_Click;
            // 
            // panelMapa
            // 
            panelMapa.BackColor = Color.FromArgb(230, 240, 220);
            panelMapa.Dock = DockStyle.Fill;
            panelMapa.Location = new Point(0, 50);
            panelMapa.Name = "panelMapa";
            panelMapa.Size = new Size(900, 700);
            panelMapa.TabIndex = 2;
            panelMapa.Paint += PanelMapa_Paint;
            panelMapa.MouseDown += PanelMapa_MouseDown;
            panelMapa.MouseMove += PanelMapa_MouseMove;
            panelMapa.MouseUp += PanelMapa_MouseUp;
            panelMapa.MouseWheel += PanelMapa_MouseWheel;
            panelMapa.Resize += PanelMapa_Resize;
            // 
            // lblTytul
            // 
            lblTytul.BackColor = Color.FromArgb(220, 237, 200);
            lblTytul.Dock = DockStyle.Top;
            lblTytul.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTytul.ForeColor = Color.FromArgb(30, 80, 30);
            lblTytul.Location = new Point(0, 0);
            lblTytul.Name = "lblTytul";
            lblTytul.Size = new Size(1200, 50);
            lblTytul.TabIndex = 0;
            lblTytul.Text = "🎣  Łowisko Gniewino — Rezerwacja Stanowisk";
            lblTytul.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 750);
            Controls.Add(panelMapa);
            Controls.Add(panelInfo);
            Controls.Add(lblTytul);
            DoubleBuffered = true;
            MinimumSize = new Size(700, 450);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Łowisko Gniewino - Rezerwacja Stanowisk";
            panelInfo.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelInfo;
        private MapPanel panelMapa;
        private Label lblTytul;
        private Label lblTytulInfo;
        private Label lblDataOd;
        private DateTimePicker dtpOd;
        private Label lblDataDo;
        private DateTimePicker dtpDo;
        private Label lblLegenda;
        private Label lblWolne;
        private Label lblZajete;
        private Label lblPodsumowanie;
        private Label lblWybraneInfo;
        private Button btnAnuluj;
        private Button btnEdycja;
        private Button btnAdmin;
        private Button btnPanelAdmin;
    }
}
