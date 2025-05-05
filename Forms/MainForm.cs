using Rendu1;
using Rendu1.Modules;
using MySql.Data.MySqlClient;

namespace Forms
{
    public partial class MainForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly int _userId;
        private readonly ClientModule _clientModule;
        private readonly CommandeModule _commandeModule;
        private readonly FidelisationModule _fidelisationModule;
        private TabControl tabControl;
        private TabPage tabCommandes;
        private TabPage tabProfil;
        private TabPage tabFidelite;
        private ListView listViewCommandes;
        private ListView listViewFidelite;
        private Panel infoPanel;

        public MainForm(DatabaseManager db, int userId)
        {
            _db = db;
            _userId = userId;
            _clientModule = new ClientModule(_db);
            var metroParisien = new MetroParisien(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataMetro", "metro.csv"));
            _fidelisationModule = new FidelisationModule(_db);
            _commandeModule = new CommandeModule(_db, metroParisien, _clientModule, _fidelisationModule);
            
            InitializeComponent();
            SetupUI();
            LoadUserData();
            LoadCommandes();
            LoadFideliteData();
        }

        private void SetupUI()
        {
            /// Configuration de la fenêtre
            this.Text = "Liv'in Paris - Menu Principal";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);

            /// Création du TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            /// Onglet Commandes
            tabCommandes = new TabPage("Commandes");
            var btnNouvelleCommande = new Button
            {
                Text = "Nouvelle Commande",
                Size = new Size(200, 40),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNouvelleCommande.Click += BtnNouvelleCommande_Click;
            tabCommandes.Controls.Add(btnNouvelleCommande);

            listViewCommandes = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(10, 60),
                Size = new Size(950, 500),
                MultiSelect = false
            };
            listViewCommandes.Columns.Add("ID", 100);
            listViewCommandes.Columns.Add("Date", 150);
            listViewCommandes.Columns.Add("Statut", 100);
            listViewCommandes.Columns.Add("Prix", 100);
            listViewCommandes.Columns.Add("Détails", 450);
            listViewCommandes.DoubleClick += ListViewCommandes_DoubleClick;
            tabCommandes.Controls.Add(listViewCommandes);

            /// Onglet Profil
            tabProfil = new TabPage("Profil");
            var btnModifier = new Button
            {
                Text = "Modifier Profil",
                Size = new Size(200, 40),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnModifier.Click += BtnModifier_Click;
            tabProfil.Controls.Add(btnModifier);

            infoPanel = new Panel
            {
                Location = new Point(10, 60),
                Size = new Size(950, 500),
                AutoScroll = true
            };
            tabProfil.Controls.Add(infoPanel);

            /// Onglet Fidélité
            tabFidelite = new TabPage("Fidélité");

            /// Titre et description
            var lblTitreFidelite = new Label
            {
                Text = "🌟 Programme de Fidélité Liv'in Paris",
                Location = new Point(10, 10),
                Size = new Size(950, 30),
                Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold)
            };
            tabFidelite.Controls.Add(lblTitreFidelite);

            var lblDescriptionFidelite = new Label
            {
                Text = "Plus vous commandez chez un même cuisinier, plus vous bénéficiez d'avantages ! " +
                      "Obtenez jusqu'à 20% de réduction sur vos commandes en fonction de votre fidélité. " +
                      "Découvrez ci-dessous vos réductions actuelles pour chaque cuisinier.",
                Location = new Point(10, 45),
                Size = new Size(950, 40),
                Font = new Font(this.Font.FontFamily, 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tabFidelite.Controls.Add(lblDescriptionFidelite);

            listViewFidelite = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(10, 100),
                Size = new Size(950, 410)
            };
            listViewFidelite.Columns.Add("Cuisinier", 300);
            listViewFidelite.Columns.Add("Commandes", 150);
            listViewFidelite.Columns.Add("Total dépensé", 200);
            listViewFidelite.Columns.Add("Réduction", 150);
            listViewFidelite.Columns.Add("Commandes restantes", 150);
            tabFidelite.Controls.Add(listViewFidelite);

            /// Ajout des onglets
            tabControl.TabPages.Add(tabCommandes);
            tabControl.TabPages.Add(tabProfil);
            tabControl.TabPages.Add(tabFidelite);

            this.Controls.Add(tabControl);
        }

        private void LoadUserData()
        {
            try
            {
                /// Requête pour récupérer les informations du client. 
                string query = @"
                    SELECT 
                        u.NomU, u.PrenomU, u.EmailU, u.TelephoneU, 
                        u.RueU, u.NumeroU, u.CodePostalU, u.VilleU, u.StationPlusProcheU,
                        u.TypeClient,
                        (SELECT COUNT(*) FROM BonDeCommande_Liv WHERE ClientID = u.ClientID) as NbCommandes,
                        (SELECT COUNT(*) FROM BonDeCommande_Liv WHERE ClientID = u.ClientID AND Statut = 'Livrée') as NbCommandesLivrees,
                        (SELECT SUM(PrixPaye) FROM BonDeCommande_Liv WHERE ClientID = u.ClientID) as MontantTotal,
                        (SELECT COUNT(DISTINCT CuisinierID) FROM BonDeCommande_Liv WHERE ClientID = u.ClientID) as NbCuisiniers,
                        (SELECT MIN(DateCommande) FROM BonDeCommande_Liv WHERE ClientID = u.ClientID) as PremiereCommande
                    FROM Utilisateur u
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
                        ("Type de compte:", reader["TypeClient"].ToString())
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

                    // Statut de fidélité
                    string statutFidelite = _fidelisationModule.ObtenirStatutFidelite(_userId);
                    var statsLabels = new[]
                    {
                        ("Statut fidélité:", statutFidelite),
                        ("Commandes totales:", reader["NbCommandes"].ToString()),
                        ("Commandes livrées:", reader["NbCommandesLivrees"].ToString()),
                        ("Montant total dépensé:", reader["MontantTotal"] != DBNull.Value ? $"{Convert.ToDecimal(reader["MontantTotal"]):C2}" : "0,00 €"),
                        ("Cuisiniers différents:", reader["NbCuisiniers"].ToString()),
                        ("Client depuis:", reader["PremiereCommande"] != DBNull.Value ? 
                            $"{Convert.ToDateTime(reader["PremiereCommande"]).ToString("dd/MM/yyyy")}" : "Pas encore de commande")
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

        private void LoadCommandes()
        {
            try
            {
                string query = @"
                    SELECT 
                        b.CommandeID,
                        b.DateCommande,
                        b.Statut,
                        b.PrixPaye,
                        GROUP_CONCAT(CONCAT(p.NomPlat, ' x', c.Quantite) SEPARATOR ', ') as Details
                    FROM BonDeCommande_Liv b
                    LEFT JOIN Correspond c ON b.CommandeID = c.CommandeID
                    LEFT JOIN Plat p ON c.PlatID = p.PlatID
                    WHERE b.ClientID = @userId
                    GROUP BY b.CommandeID, b.DateCommande, b.Statut, b.PrixPaye
                    ORDER BY b.DateCommande DESC";

                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@userId", _userId);
                using var reader = cmd.ExecuteReader();

                listViewCommandes.Items.Clear();
                while (reader.Read())
                {
                    var item = new ListViewItem(reader["CommandeID"].ToString());
                    item.SubItems.Add(Convert.ToDateTime(reader["DateCommande"]).ToString("dd/MM/yyyy HH:mm"));
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

        private void LoadFideliteData()
        {
            try
            {
                /// Requête pour récupérer les données de fidélité.
                string query = @"
                    SELECT 
                        CONCAT(u.PrenomU, ' ', u.NomU) as NomCuisinier,
                        COUNT(DISTINCT b.CommandeID) as NbCommandes,
                        SUM(b.PrixPaye) as TotalDepense,
                        CASE
                            WHEN COUNT(DISTINCT b.CommandeID) >= 10 THEN '20%'
                            WHEN COUNT(DISTINCT b.CommandeID) >= 7 THEN '15%'
                            WHEN COUNT(DISTINCT b.CommandeID) >= 4 THEN '10%'
                            ELSE '0%'
                        END as Reduction,
                        CASE
                            WHEN COUNT(DISTINCT b.CommandeID) >= 10 THEN 'Niveau maximum atteint'
                            WHEN COUNT(DISTINCT b.CommandeID) >= 7 THEN CONCAT(10 - COUNT(DISTINCT b.CommandeID), ' pour 20%')
                            WHEN COUNT(DISTINCT b.CommandeID) >= 4 THEN CONCAT(7 - COUNT(DISTINCT b.CommandeID), ' pour 15%')
                            ELSE CONCAT(4 - COUNT(DISTINCT b.CommandeID), ' pour 10%')
                        END as CommandesRestantes
                    FROM BonDeCommande_Liv b
                    JOIN Utilisateur u ON b.CuisinierID = u.ClientID
                    WHERE b.ClientID = @userId AND b.Statut = 'Livrée'
                    GROUP BY b.CuisinierID, u.PrenomU, u.NomU
                    ORDER BY NbCommandes DESC";

                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@userId", _userId);
                using var reader = cmd.ExecuteReader();

                listViewFidelite.Items.Clear();
                while (reader.Read())
                {
                    var item = new ListViewItem(reader["NomCuisinier"].ToString());
                    item.SubItems.Add(reader["NbCommandes"].ToString());
                    item.SubItems.Add($"{Convert.ToDecimal(reader["TotalDepense"]):C2}");
                    item.SubItems.Add(reader["Reduction"].ToString());
                    item.SubItems.Add(reader["CommandesRestantes"].ToString());
                    listViewFidelite.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données de fidélité : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNouvelleCommande_Click(object sender, EventArgs e)
        {
            var commandeForm = new CommandeForm(_db, _userId, _commandeModule);
            commandeForm.ShowDialog();
            LoadCommandes(); // Recharger la liste des commandes après la création
            LoadFideliteData(); // Mettre à jour les données de fidélité
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            var modifierForm = new ModifierProfilForm(_db, _userId);
            if (modifierForm.ShowDialog() == DialogResult.OK)
            {
                LoadUserData(); /// Recharger les données du profil après modification
            }
        }

        private void ListViewCommandes_DoubleClick(object sender, EventArgs e)
        {
            if (listViewCommandes.SelectedItems.Count > 0)
            {
                string commandeId = listViewCommandes.SelectedItems[0].Text;
                var detailsForm = new DetailsCommandeForm(_db, commandeId);
                detailsForm.ShowDialog();
            }
        }
    }
} 