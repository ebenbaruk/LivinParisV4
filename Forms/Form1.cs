using Rendu1;
using Rendu1.Modules;
using System.Diagnostics;

namespace Forms
{
    public partial class Form1 : Form
    {
        private DatabaseManager _db;
        private AuthModule _authModule;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Button btnConsole;
        private Label lblTitle;

        public Form1()
        {
            InitializeComponent();
            InitializeDatabase();
            SetupUI();
        }

        private void InitializeDatabase()
        {
            try
            {
                _db = new DatabaseManager(password: "2015Franc");
                if (!_db.Connect())
                {
                    MessageBox.Show("Erreur de connexion à la base de données", "Erreur", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                _authModule = new AuthModule(_db);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'initialisation : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void SetupUI()
        {
            /// Configuration de la fenêtre
            this.Text = "Liv'in Paris";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            /// Titre
            lblTitle = new Label
            {
                Text = "LIV'IN PARIS",
                Font = new Font("Arial", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(300, 50),
                Location = new Point(50, 30)
            };

            /// Champs de connexion
            var lblEmail = new Label
            {
                Text = "Email :",
                Size = new Size(100, 20),
                Location = new Point(50, 120)
            };

            txtEmail = new TextBox
            {
                Size = new Size(250, 20),
                Location = new Point(50, 150)
            };

            var lblPassword = new Label
            {
                Text = "Mot de passe :",
                Size = new Size(100, 20),
                Location = new Point(50, 190)
            };

            txtPassword = new TextBox
            {
                Size = new Size(250, 20),
                Location = new Point(50, 220),
                PasswordChar = '•'
            };

            /// Boutons
            btnLogin = new Button
            {
                Text = "Se connecter",
                Size = new Size(250, 40),
                Location = new Point(50, 270),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button
            {
                Text = "Créer un compte",
                Size = new Size(250, 40),
                Location = new Point(50, 330),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRegister.Click += BtnRegister_Click;

            btnConsole = new Button
            {
                Text = "Mode Console",
                Size = new Size(250, 40),
                Location = new Point(50, 390),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConsole.Click += BtnConsole_Click;

            ///     Ajout des contrôles
            this.Controls.AddRange(new Control[] {
                lblTitle,
                lblEmail, txtEmail,
                lblPassword, txtPassword,
                btnLogin, btnRegister, btnConsole
            });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var session = _authModule.AuthenticateUser(txtEmail.Text.Trim(), txtPassword.Text);
                if (session != null)
                {
                    Form mainForm;
                    if (session.Type == AuthModule.UserType.Cuisinier)
                    {
                        mainForm = new CuisinierForm(_db, session.UserId);
                    }
                    else
                    {
                        mainForm = new MainForm(_db, session.UserId);
                    }
                    
                    this.Hide();
                    mainForm.ShowDialog();
                    this.Show();
                    txtPassword.Text = string.Empty; /// Effacer le mot de passe par sécurité
                }
                else
                {
                    MessageBox.Show("Email ou mot de passe incorrect", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la connexion : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm(_db);
            this.Hide();
            registerForm.ShowDialog();
            this.Show();
        }

        private void BtnConsole_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous passer en mode console ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Hide();
                var programPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Rendu1", "bin", "Debug", "net8.0", "Rendu1.exe");
                if (File.Exists(programPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = programPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Le programme console n'a pas été trouvé.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Application.Exit();
            }
        }
    }
}
