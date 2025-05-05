using Rendu1;
using Rendu1.Modules;
using MySql.Data.MySqlClient;

namespace Forms
{
    public partial class CuisinierForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly int _userId;
        private readonly CommandeModule _commandeModule;
        private TabControl tabControl;
        private TabPage tabPlats;
        private TabPage tabCommandes;
        private TabPage tabProfil;
        private ListView listViewPlats;
        private ListView listViewCommandes;
        private Panel infoPanel;

        public CuisinierForm(DatabaseManager db, int userId)
        {
            _db = db;
            _userId = userId;
            _commandeModule = new CommandeModule(_db, null, null, null);
            InitializeComponents();
            LoadCuisinierData();
            LoadPlats();
            LoadCommandes();
        }

        private void InitializeComponents()
        {
            /// Configuration de la fenêtre
            this.Text = "Liv'in Paris - Interface Cuisinier";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);

            /// Création du TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            /// Onglet Plats
            tabPlats = new TabPage("Mes Plats");
            var btnAjouterPlat = new Button
            {
                Text = "Ajouter un Plat",
                Size = new Size(150, 40),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAjouterPlat.Click += BtnAjouterPlat_Click;

            var btnPlatDuJour = new Button
            {
                Text = "Définir Plat du Jour",
                Size = new Size(150, 40),
                Location = new Point(170, 10),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPlatDuJour.Click += BtnPlatDuJour_Click;

            listViewPlats = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(10, 60),
                Size = new Size(950, 500),
                MultiSelect = false
            };
            listViewPlats.Columns.Add("ID", 50);
            listViewPlats.Columns.Add("Nom", 200);
            listViewPlats.Columns.Add("Type", 100);
            listViewPlats.Columns.Add("Prix", 80);
            listViewPlats.Columns.Add("Description", 300);
            listViewPlats.Columns.Add("Disponible", 80);
            listViewPlats.Columns.Add("Plat du Jour", 80);
            listViewPlats.DoubleClick += ListViewPlats_DoubleClick;

            tabPlats.Controls.AddRange(new Control[] { btnAjouterPlat, btnPlatDuJour, listViewPlats });

            /// Onglet Commandes
            tabCommandes = new TabPage("Commandes");
            listViewCommandes = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(10, 10),
                Size = new Size(950, 550),
                MultiSelect = false
            };
            listViewCommandes.Columns.Add("ID", 100);
            listViewCommandes.Columns.Add("Date", 150);
            listViewCommandes.Columns.Add("Client", 150);
            listViewCommandes.Columns.Add("Statut", 100);
            listViewCommandes.Columns.Add("Prix", 100);
            listViewCommandes.Columns.Add("Détails", 300);
            listViewCommandes.DoubleClick += ListViewCommandes_DoubleClick;
            tabCommandes.Controls.Add(listViewCommandes);

            /// Onglet Profil
            tabProfil = new TabPage("Mon Profil");
            var btnModifier = new Button
            {
                Text = "Modifier Profil",
                Size = new Size(150, 40),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnModifier.Click += BtnModifier_Click;

            infoPanel = new Panel
            {
                Location = new Point(10, 60),
                Size = new Size(950, 500),
                AutoScroll = true
            };
            tabProfil.Controls.AddRange(new Control[] { btnModifier, infoPanel });

            /// Ajout des onglets
            tabControl.TabPages.AddRange(new TabPage[] { tabPlats, tabCommandes, tabProfil });
            this.Controls.Add(tabControl);
        }

        private void LoadCuisinierData()
        {
            try
            {
                /// Requête pour récupérer les informations du cuisinier et les statistiques. 
                string query = @"
                    SELECT 
                        u.NomU, u.PrenomU, u.EmailU, u.TelephoneU, 
                        u.RueU, u.NumeroU, u.CodePostalU, u.VilleU, u.StationPlusProcheU,
                        c.SpecialiteC,
                        (SELECT COUNT(*) FROM Plat WHERE CuisinierID = u.ClientID) as NbPlats,
                        (SELECT COUNT(*) FROM BonDeCommande_Liv WHERE CuisinierID = u.ClientID) as NbCommandes,
                        (SELECT COUNT(*) FROM BonDeCommande_Liv WHERE CuisinierID = u.ClientID AND Statut = 'Livrée') as NbCommandesLivrees,
                        (SELECT AVG(Note) FROM Cuisinier WHERE ClientID = u.ClientID) as NoteMoyenne
                    FROM Utilisateur u
                    JOIN Cuisinier c ON u.ClientID = c.ClientID
                    WHERE u.ClientID = @userId";

                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@userId", _userId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    infoPanel.Controls.Clear();

                    /// Titre de la section Informations Personnelles
                    var lblPersonnel = new Label
                    {
                        Text = "📋 Informations Personnelles",
                        Location = new Point(0, 0),
                        Size = new Size(400, 30),
                        Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
                    };
                    infoPanel.Controls.Add(lblPersonnel);

                    var labels = new[]
                    {
                        ("Nom:", reader["NomU"].ToString()),
                        ("Prénom:", reader["PrenomU"].ToString()),
                        ("Email:", reader["EmailU"].ToString()),
                        ("Téléphone:", reader["TelephoneU"].ToString()),
                        ("Adresse:", $"{reader["RueU"]} {reader["NumeroU"]}, {reader["CodePostalU"]} {reader["VilleU"]}"),
                        ("Station de métro:", reader["StationPlusProcheU"].ToString()),
                        ("Spécialité:", reader["SpecialiteC"].ToString())
                    };

                    int y = 40;
                    foreach (var (label, value) in labels)
                    {
                        var lblTitle = new Label
                        {
                            Text = label,
                            Location = new Point(0, y),
                            Size = new Size(120, 25),
                            Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
                        };

                        var lblValue = new Label
                        {
                            Text = value,
                            Location = new Point(130, y),
                            Size = new Size(400, 25),
                            Font = new Font(this.Font.FontFamily, 10)
                        };

                        infoPanel.Controls.Add(lblTitle);
                        infoPanel.Controls.Add(lblValue);
                        y += 35;
                    }

                    /// Titre de la section Statistiques
                    y += 20;
                    var lblStats = new Label
                    {
                        Text = "📊 Statistiques",
                        Location = new Point(0, y),
                        Size = new Size(400, 30),
                        Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
                    };
                    infoPanel.Controls.Add(lblStats);
                    y += 40;

                    var statsLabels = new[]
                    {
                        ("Note moyenne:", reader["NoteMoyenne"] != DBNull.Value ? $"{Convert.ToDouble(reader["NoteMoyenne"]):F1}/5" : "Pas encore noté"),
                        ("Nombre de plats:", reader["NbPlats"].ToString()),
                        ("Commandes totales:", reader["NbCommandes"].ToString()),
                        ("Commandes livrées:", reader["NbCommandesLivrees"].ToString())
                    };

                    foreach (var (label, value) in statsLabels)
                    {
                        var lblTitle = new Label
                        {
                            Text = label,
                            Location = new Point(0, y),
                            Size = new Size(150, 25),
                            Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
                        };

                        var lblValue = new Label
                        {
                            Text = value,
                            Location = new Point(160, y),
                            Size = new Size(400, 25),
                            Font = new Font(this.Font.FontFamily, 10)
                        };

                        infoPanel.Controls.Add(lblTitle);
                        infoPanel.Controls.Add(lblValue);
                        y += 35;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPlats()
        {
            try
            {
                /// Requête pour récupérer les plats du cuisinier.
                string query = @"
                    SELECT PlatID, NomPlat, TypePlat, Description, PrixParPersonne, 
                           EstDisponible, PlatDuJour
                    FROM Plat
                    WHERE CuisinierID = @userId
                    ORDER BY DateCreation DESC";

                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@userId", _userId);
                using var reader = cmd.ExecuteReader();

                listViewPlats.Items.Clear();
                while (reader.Read())
                {
                    var item = new ListViewItem(reader["PlatID"].ToString());
                    item.SubItems.Add(reader["NomPlat"].ToString());
                    item.SubItems.Add(reader["TypePlat"].ToString());
                    item.SubItems.Add($"{Convert.ToDecimal(reader["PrixParPersonne"]):C2}");
                    item.SubItems.Add(reader["Description"].ToString());
                    item.SubItems.Add(Convert.ToBoolean(reader["EstDisponible"]) ? "Oui" : "Non");
                    item.SubItems.Add(Convert.ToBoolean(reader["PlatDuJour"]) ? "Oui" : "Non");
                    listViewPlats.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des plats : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCommandes()
        {
            try
            {
                /// Requête pour récupérer les commandes du cuisinier.
                string query = @"
                    SELECT 
                        b.CommandeID,
                        b.DateCommande,
                        CONCAT(u.PrenomU, ' ', u.NomU) as NomClient,
                        b.Statut,
                        b.PrixPaye,
                        GROUP_CONCAT(CONCAT(p.NomPlat, ' x', c.Quantite) SEPARATOR ', ') as Details
                    FROM BonDeCommande_Liv b
                    JOIN Utilisateur u ON b.ClientID = u.ClientID
                    LEFT JOIN Correspond c ON b.CommandeID = c.CommandeID
                    LEFT JOIN Plat p ON c.PlatID = p.PlatID
                    WHERE b.CuisinierID = @userId
                    GROUP BY b.CommandeID, b.DateCommande, NomClient, b.Statut, b.PrixPaye
                    ORDER BY b.DateCommande DESC";

                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@userId", _userId);
                using var reader = cmd.ExecuteReader();

                listViewCommandes.Items.Clear();
                while (reader.Read())
                {
                    var item = new ListViewItem(reader["CommandeID"].ToString());
                    item.SubItems.Add(Convert.ToDateTime(reader["DateCommande"]).ToString("dd/MM/yyyy HH:mm"));
                    item.SubItems.Add(reader["NomClient"].ToString());
                    item.SubItems.Add(reader["Statut"].ToString());
                    item.SubItems.Add($"{Convert.ToDecimal(reader["PrixPaye"]):C2}");
                    item.SubItems.Add(reader["Details"].ToString() ?? "");
                    listViewCommandes.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des commandes : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAjouterPlat_Click(object sender, EventArgs e)
        {
            var ajouterPlatForm = new AjouterPlatForm(_db, _userId);
            if (ajouterPlatForm.ShowDialog() == DialogResult.OK)
            {
                LoadPlats();
            }
        }

        private void BtnPlatDuJour_Click(object sender, EventArgs e)
        {
            if (listViewPlats.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un plat.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int platId = int.Parse(listViewPlats.SelectedItems[0].Text);

                // Réinitialiser tous les plats du jour
                string sqlReset = "UPDATE Plat SET PlatDuJour = FALSE WHERE CuisinierID = @cuisinierId";
                using (var cmdReset = new MySqlCommand(sqlReset, _db.GetConnection()))
                {
                    cmdReset.Parameters.AddWithValue("@cuisinierId", _userId);
                    cmdReset.ExecuteNonQuery();
                }

                /// Définir le nouveau plat du jour
                string sqlUpdate = "UPDATE Plat SET PlatDuJour = TRUE WHERE PlatID = @platId";
                using (var cmdUpdate = new MySqlCommand(sqlUpdate, _db.GetConnection()))
                {
                    cmdUpdate.Parameters.AddWithValue("@platId", platId);
                    cmdUpdate.ExecuteNonQuery();
                }

                MessageBox.Show("Le plat du jour a été mis à jour avec succès.", "Succès",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPlats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour du plat du jour : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ListViewPlats_DoubleClick(object sender, EventArgs e)
        {
            if (listViewPlats.SelectedItems.Count > 0)
            {
                int platId = int.Parse(listViewPlats.SelectedItems[0].Text);
                var modifierPlatForm = new ModifierPlatForm(_db, _userId, platId);
                if (modifierPlatForm.ShowDialog() == DialogResult.OK)
                {
                    LoadPlats();
                }
            }
        }

        private void ListViewCommandes_DoubleClick(object sender, EventArgs e)
        {
            if (listViewCommandes.SelectedItems.Count > 0)
            {
                string commandeId = listViewCommandes.SelectedItems[0].Text;
                var detailsForm = new DetailsCommandeForm(_db, commandeId);
                if (detailsForm.ShowDialog() == DialogResult.OK)
                {
                    LoadCommandes();
                }
            }
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            var modifierForm = new ModifierProfilForm(_db, _userId);
            if (modifierForm.ShowDialog() == DialogResult.OK)
            {
                LoadCuisinierData();
            }
        }
    }
} 