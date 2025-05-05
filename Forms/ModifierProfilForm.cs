using Rendu1;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace Forms
{
    public partial class ModifierProfilForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly int _userId;
        private TextBox txtNom;
        private TextBox txtPrenom;
        private TextBox txtEmail;
        private TextBox txtTelephone;
        private TextBox txtRue;
        private TextBox txtNumero;
        private TextBox txtCodePostal;
        private TextBox txtVille;
        private ComboBox cmbStation;
        private TextBox txtMotDePasse;
        private TextBox txtConfirmMotDePasse;

        public ModifierProfilForm(DatabaseManager db, int userId)
        {
            _db = db;
            _userId = userId;
            InitializeComponents();
            LoadUserData();
            LoadStations();
        }

        private void InitializeComponents()
        {
            this.Text = "Modifier mon profil";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 700);

            var panel = new Panel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            int y = 20;
            /// Informations personnelles
            var lblPersonnel = new Label
            {
                Text = "Informations personnelles",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };
            panel.Controls.Add(lblPersonnel);
            y += 30;

            /// Nom
            panel.Controls.Add(new Label
            {
                Text = "Nom :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtNom = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 25)
            };
            panel.Controls.Add(txtNom);
            y += 35;

            /// Prénom
            panel.Controls.Add(new Label
            {
                Text = "Prénom :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtPrenom = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 25)
            };
            panel.Controls.Add(txtPrenom);
            y += 35;

            /// Email
            panel.Controls.Add(new Label
            {
                Text = "Email :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtEmail = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 25)
            };
            panel.Controls.Add(txtEmail);
            y += 35;

            /// Téléphone
            panel.Controls.Add(new Label
            {
                Text = "Téléphone :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtTelephone = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 25)
            };
            panel.Controls.Add(txtTelephone);
            y += 50;

            /// Adresse
            var lblAdresse = new Label
            {
                Text = "Adresse",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };
            panel.Controls.Add(lblAdresse);
            y += 30;

            /// Rue
            panel.Controls.Add(new Label
            {
                Text = "Rue :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtRue = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(300, 25)
            };
            panel.Controls.Add(txtRue);
            y += 35;

            /// Numéro
            panel.Controls.Add(new Label
            {
                Text = "Numéro :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtNumero = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(100, 25)
            };
            panel.Controls.Add(txtNumero);
            y += 35;

            /// Code postal
            panel.Controls.Add(new Label
            {
                Text = "Code postal :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtCodePostal = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(100, 25)
            };
            panel.Controls.Add(txtCodePostal);
            y += 35;

            /// Ville
            panel.Controls.Add(new Label
            {
                Text = "Ville :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtVille = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 25)
            };
            panel.Controls.Add(txtVille);
            y += 35;

            /// Station de métro
            panel.Controls.Add(new Label
            {
                Text = "Station métro :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            cmbStation = new ComboBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panel.Controls.Add(cmbStation);
            y += 50;

            /// Mot de passe
            var lblPassword = new Label
            {
                Text = "Sécurité",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };
            panel.Controls.Add(lblPassword);
            y += 30;

            /// Nouveau mot de passe
            panel.Controls.Add(new Label
            {
                Text = "Nouveau mot de passe :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtMotDePasse = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 25),
                UseSystemPasswordChar = true
            };
            panel.Controls.Add(txtMotDePasse);
            y += 35;

            /// Confirmation mot de passe
            panel.Controls.Add(new Label
            {
                Text = "Confirmation :",
                Location = new Point(0, y),
                Size = new Size(120, 25)
            });

            txtConfirmMotDePasse = new TextBox
            {
                Location = new Point(130, y),
                Size = new Size(200, 25),
                UseSystemPasswordChar = true
            };
            panel.Controls.Add(txtConfirmMotDePasse);
            y += 50;

            /// Boutons
            var btnEnregistrer = new Button
            {
                Text = "Enregistrer",
                Location = new Point(130, y),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnEnregistrer.Click += BtnEnregistrer_Click;
            panel.Controls.Add(btnEnregistrer);

            var btnAnnuler = new Button
            {
                Text = "Annuler",
                Location = new Point(290, y),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(169, 169, 169),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAnnuler.Click += (s, e) => this.Close();
            panel.Controls.Add(btnAnnuler);

            this.Controls.Add(panel);
        }

        private void LoadUserData()
        {
            try
            {
                /// Requête pour récupérer les informations du client.
                string query = @"
                    SELECT NomU, PrenomU, EmailU, TelephoneU, RueU, NumeroU, CodePostalU, VilleU, StationPlusProcheU
                    FROM Utilisateur
                    WHERE ClientID = @userId";

                string station = "";
                using (var cmd = new MySqlCommand(query, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtNom.Text = reader["NomU"].ToString();
                            txtPrenom.Text = reader["PrenomU"].ToString();
                            txtEmail.Text = reader["EmailU"].ToString();
                            txtTelephone.Text = reader["TelephoneU"].ToString();
                            txtRue.Text = reader["RueU"].ToString();
                            txtNumero.Text = reader["NumeroU"].ToString();
                            txtCodePostal.Text = reader["CodePostalU"].ToString();
                            txtVille.Text = reader["VilleU"].ToString();
                            station = reader["StationPlusProcheU"].ToString();
                        }
                    }
                }
                LoadStations(station);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStations(string selectedStation = null)
        {
            try
            {
                /// Requête pour récupérer les stations de métro.
                var stations = new List<string>();
                string query = "SELECT DISTINCT StationPlusProcheU FROM Utilisateur WHERE StationPlusProcheU IS NOT NULL ORDER BY StationPlusProcheU";
                
                using (var cmd = new MySqlCommand(query, _db.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stations.Add(reader["StationPlusProcheU"].ToString());
                    }
                }

                cmbStation.Items.Clear();
                cmbStation.Items.AddRange(stations.ToArray());

                if (selectedStation != null && stations.Contains(selectedStation))
                {
                    cmbStation.SelectedItem = selectedStation;
                }
                else if (cmbStation.Items.Count > 0)
                {
                    cmbStation.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des stations : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text) ||
                string.IsNullOrWhiteSpace(txtPrenom.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtTelephone.Text) ||
                string.IsNullOrWhiteSpace(txtRue.Text) ||
                string.IsNullOrWhiteSpace(txtNumero.Text) ||
                string.IsNullOrWhiteSpace(txtCodePostal.Text) ||
                string.IsNullOrWhiteSpace(txtVille.Text))
            {
                MessageBox.Show("Tous les champs sont obligatoires.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            /// Validation de l'email
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(txtEmail.Text, emailPattern))
            {
                MessageBox.Show("L'adresse email n'est pas valide.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            /// Validation du numéro de téléphone
            string phonePattern = @"^0[1-9][0-9]{8}$";
            if (!Regex.IsMatch(txtTelephone.Text, phonePattern))
            {
                MessageBox.Show("Le numéro de téléphone doit être au format 0XXXXXXXXX.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            /// Validation du code postal
            string postalPattern = @"^[0-9]{5}$";
            if (!Regex.IsMatch(txtCodePostal.Text, postalPattern))
            {
                MessageBox.Show("Le code postal doit contenir 5 chiffres.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            /// Validation du numéro de rue
            if (!int.TryParse(txtNumero.Text, out _))
            {
                MessageBox.Show("Le numéro de rue doit être un nombre.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            /// Validation du mot de passe si renseigné
            if (!string.IsNullOrEmpty(txtMotDePasse.Text))
            {
                if (txtMotDePasse.Text.Length < 8)
                {
                    MessageBox.Show("Le mot de passe doit contenir au moins 8 caractères.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (txtMotDePasse.Text != txtConfirmMotDePasse.Text)
                {
                    MessageBox.Show("Les mots de passe ne correspondent pas.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                /// Requête pour mettre à jour les informations du client.
                string query;
                if (string.IsNullOrEmpty(txtMotDePasse.Text))
                {
                    query = @"
                        UPDATE Utilisateur 
                        SET NomU = @nom,
                            PrenomU = @prenom,
                            EmailU = @email,
                            TelephoneU = @telephone,
                            RueU = @rue,
                            NumeroU = @numero,
                            CodePostalU = @codePostal,
                            VilleU = @ville,
                            StationPlusProcheU = @station
                        WHERE ClientID = @userId";
                }
                else
                {
                    /// Requête pour mettre à jour les informations du client avec le mot de passe.
                    query = @"
                        UPDATE Utilisateur 
                        SET NomU = @nom,
                            PrenomU = @prenom,
                            EmailU = @email,
                            TelephoneU = @telephone,
                            RueU = @rue,
                            NumeroU = @numero,
                            CodePostalU = @codePostal,
                            VilleU = @ville,
                            StationPlusProcheU = @station,
                            MotDePasse = @motDePasse
                        WHERE ClientID = @userId";
                }

                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@nom", txtNom.Text.Trim());
                cmd.Parameters.AddWithValue("@prenom", txtPrenom.Text.Trim());
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@telephone", txtTelephone.Text.Trim());
                cmd.Parameters.AddWithValue("@rue", txtRue.Text.Trim());
                cmd.Parameters.AddWithValue("@numero", int.Parse(txtNumero.Text.Trim()));
                cmd.Parameters.AddWithValue("@codePostal", txtCodePostal.Text.Trim());
                cmd.Parameters.AddWithValue("@ville", txtVille.Text.Trim());
                cmd.Parameters.AddWithValue("@station", cmbStation.SelectedItem?.ToString());
                cmd.Parameters.AddWithValue("@userId", _userId);

                if (!string.IsNullOrEmpty(txtMotDePasse.Text))
                {
                    cmd.Parameters.AddWithValue("@motDePasse", txtMotDePasse.Text);
                }

                cmd.ExecuteNonQuery();

                MessageBox.Show("Vos informations ont été mises à jour avec succès !", "Succès",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour des données : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 