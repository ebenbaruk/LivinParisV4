using Rendu1;
using Rendu1.Modules;
using MySql.Data.MySqlClient;

namespace Forms
{
    public partial class RegisterForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly ClientModule _clientModule;
        private TextBox txtNom;
        private TextBox txtPrenom;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtTelephone;
        private TextBox txtRue;
        private TextBox txtNumero;
        private TextBox txtCodePostal;
        private TextBox txtVille;
        private TextBox txtStation;
        private Button btnValidate;
        private Button btnCancel;
        private Panel buttonPanel;

        public RegisterForm(DatabaseManager db)
        {
            _db = db;
            _clientModule = new ClientModule(_db);
            InitializeComponents();
            SetupUI();
        }

        private void InitializeComponents()
        {
            ///      Initialisation des TextBox
            txtNom = new TextBox();
            txtPrenom = new TextBox();
            txtEmail = new TextBox();
            txtPassword = new TextBox();
            txtConfirmPassword = new TextBox();
            txtTelephone = new TextBox();
            txtRue = new TextBox();
            txtNumero = new TextBox();
            txtCodePostal = new TextBox();
            txtVille = new TextBox();
            txtStation = new TextBox();

            /// Initialisation des boutons
            btnValidate = new Button();
            btnCancel = new Button();
            buttonPanel = new Panel();

            /// Configuration initiale de la fenêtre
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 800);
            this.MinimumSize = new System.Drawing.Size(500, 800);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            /// Configuration de la fenêtre
            this.Text = "Inscription - Liv'in Paris";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            /// Création des contrôles
            var controls = new List<(string label, TextBox textBox, Point location)>();
            int y = 30;
            int spacing = 50;

            /// Configuration des TextBox
            txtPassword.PasswordChar = '•';
            txtConfirmPassword.PasswordChar = '•';

            /// Informations personnelles
            controls.Add(("Nom :", txtNom, new Point(50, y))); y += spacing;
            controls.Add(("Prénom :", txtPrenom, new Point(50, y))); y += spacing;
            controls.Add(("Email :", txtEmail, new Point(50, y))); y += spacing;
            controls.Add(("Mot de passe :", txtPassword, new Point(50, y))); y += spacing;
            controls.Add(("Confirmer le mot de passe :", txtConfirmPassword, new Point(50, y))); y += spacing;
            controls.Add(("Téléphone :", txtTelephone, new Point(50, y))); y += spacing;
            controls.Add(("Rue :", txtRue, new Point(50, y))); y += spacing;
            controls.Add(("Numéro :", txtNumero, new Point(50, y))); y += spacing;
            controls.Add(("Code Postal :", txtCodePostal, new Point(50, y))); y += spacing;
            controls.Add(("Ville :", txtVille, new Point(50, y))); y += spacing;
            controls.Add(("Station de métro la plus proche :", txtStation, new Point(50, y))); y += spacing;

            /// Configuration des TextBox
            foreach (var (label, textBox, location) in controls)
            {
                var lblControl = new Label
                {
                    Text = label,
                    Size = new Size(200, 20),
                    Location = location,
                    AutoSize = true
                };

                textBox.Size = new Size(200, 23);
                textBox.Location = new Point(location.X + 200, location.Y);

                this.Controls.Add(lblControl);
                this.Controls.Add(textBox);
            }

            /// Configuration du panel de boutons
            buttonPanel.Size = new Size(400, 50);
            buttonPanel.Location = new Point(50, y + 20);

            /// Configuration des boutons
            btnValidate.Text = "Valider";
            btnValidate.Size = new Size(150, 40);
            btnValidate.Location = new Point(0, 0);
            btnValidate.BackColor = Color.FromArgb(34, 139, 34); // Vert
            btnValidate.ForeColor = Color.White;
            btnValidate.FlatStyle = FlatStyle.Flat;
            btnValidate.Click += BtnValidate_Click;

            btnCancel.Text = "Annuler";
            btnCancel.Size = new Size(150, 40);
            btnCancel.Location = new Point(200, 0);
            btnCancel.BackColor = Color.Gray;
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Click += (s, e) => this.Close();

            /// Ajout des boutons au panel
            buttonPanel.Controls.Add(btnValidate);
            buttonPanel.Controls.Add(btnCancel);

            /// Ajout du panel à la form
            this.Controls.Add(buttonPanel);

            /// Ajout d'une barre de défilement si nécessaire
            this.AutoScroll = true;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text) ||
                string.IsNullOrWhiteSpace(txtPrenom.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword.Text) ||
                string.IsNullOrWhiteSpace(txtTelephone.Text) ||
                string.IsNullOrWhiteSpace(txtRue.Text) ||
                string.IsNullOrWhiteSpace(txtNumero.Text) ||
                string.IsNullOrWhiteSpace(txtCodePostal.Text) ||
                string.IsNullOrWhiteSpace(txtVille.Text) ||
                string.IsNullOrWhiteSpace(txtStation.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Les mots de passe ne correspondent pas", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtNumero.Text, out _))
            {
                MessageBox.Show("Le numéro de rue doit être un nombre", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtCodePostal.Text, out _))
            {
                MessageBox.Show("Le code postal doit être un nombre", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtEmail.Text,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Format d'email invalide", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void BtnValidate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            try
            {
                /// Requête pour insérer un nouvellle utilisateur dans la base de donnéees.
                string query = @"
                    INSERT INTO Utilisateur (TypeClient, NomU, PrenomU, EmailU, MDPU, TelephoneU, 
                        RueU, NumeroU, CodePostalU, VilleU, StationPlusProcheU)
                    VALUES (@TypeClient, @Nom, @Prenom, @Email, @MotDePasse, @Telephone,
                        @Rue, @Numero, @CodePostal, @Ville, @Station)";

                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@TypeClient", "Particulier");
                cmd.Parameters.AddWithValue("@Nom", txtNom.Text.Trim());
                cmd.Parameters.AddWithValue("@Prenom", txtPrenom.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@MotDePasse", txtPassword.Text);
                cmd.Parameters.AddWithValue("@Telephone", txtTelephone.Text.Trim());
                cmd.Parameters.AddWithValue("@Rue", txtRue.Text.Trim());
                cmd.Parameters.AddWithValue("@Numero", int.Parse(txtNumero.Text));
                cmd.Parameters.AddWithValue("@CodePostal", int.Parse(txtCodePostal.Text));
                cmd.Parameters.AddWithValue("@Ville", txtVille.Text.Trim());
                cmd.Parameters.AddWithValue("@Station", txtStation.Text.Trim());

                cmd.ExecuteNonQuery();

                MessageBox.Show("Votre compte a été créé avec succès ! Vous pouvez maintenant vous connecter.", 
                    "Inscription réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'inscription : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 