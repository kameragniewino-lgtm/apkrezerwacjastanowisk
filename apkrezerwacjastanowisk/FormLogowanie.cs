namespace apkrezerwacjastanowisk
{
    public class FormLogowanie : Form
    {
        private readonly TextBox txtLogin;
        private readonly TextBox txtHaslo;

        public string PodanyLogin => txtLogin.Text.Trim();
        public string PodaneHaslo => txtHaslo.Text;

        public FormLogowanie()
        {
            Text = "🔐 Logowanie — Panel Administratora";
            Size = new Size(380, 250);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var lblLogin = new Label
            {
                Text = "Login:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f)
            };

            txtLogin = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(320, 30),
                Font = new Font("Segoe UI", 11f)
            };

            var lblHaslo = new Label
            {
                Text = "Hasło:",
                Location = new Point(20, 85),
                AutoSize = true,
                Font = new Font("Segoe UI", 10f)
            };

            txtHaslo = new TextBox
            {
                Location = new Point(20, 110),
                Size = new Size(320, 30),
                Font = new Font("Segoe UI", 11f),
                UseSystemPasswordChar = true
            };

            var btnZaloguj = new Button
            {
                Text = "Zaloguj się",
                DialogResult = DialogResult.OK,
                Location = new Point(110, 155),
                Size = new Size(150, 42),
                BackColor = Color.FromArgb(50, 120, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };

            Controls.AddRange([lblLogin, txtLogin, lblHaslo, txtHaslo, btnZaloguj]);
            AcceptButton = btnZaloguj;
        }
    }
}
