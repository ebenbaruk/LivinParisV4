using Rendu1;
using Rendu1.Modules;
using MySql.Data.MySqlClient;

namespace Forms
{
    public partial class CommandeForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly int _userId;
        private readonly CommandeModule _commandeModule;
        private ListView listViewPlats;
        private ListView listViewRecap;
        private ComboBox cmbCuisiniers;
        private TextBox txtRecherche;
        private NumericUpDown numQuantite;
        private DateTimePicker dtpLivraison;
        private Label lblTotal;
        private Label lblReduction;
        private decimal totalCommande = 0;
        private List<(int platId, int quantite, decimal prix, string nom)> platsSelectionnes = new();
        private int? cuisinierSelectionne = null;

        public CommandeForm(DatabaseManager db, int userId, CommandeModule commandeModule)
        {
            _db = db;
            _userId = userId;
            _commandeModule = commandeModule;
            InitializeComponents();
            LoadCuisiniers();
        }

        private void InitializeComponents()
        {
            this.Text = "Nouvelle Commande";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 800);

            /// Panel de recherche
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            var lblCuisinier = new Label
            {
                Text = "Cuisinier :",
                Location = new Point(10, 20),
                AutoSize = true
            };

            cmbCuisiniers = new ComboBox
            {
                Location = new Point(80, 17),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCuisiniers.SelectedIndexChanged += CmbCuisiniers_SelectedIndexChanged;

            txtRecherche = new TextBox
            {
                Location = new Point(350, 17),
                Width = 250,
                PlaceholderText = "Rechercher un plat..."
            };
            txtRecherche.TextChanged += TxtRecherche_TextChanged;

            searchPanel.Controls.AddRange(new Control[] { lblCuisinier, cmbCuisiniers, txtRecherche });

            /// Liste des plats
            listViewPlats = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(10, 70),
                Size = new Size(960, 200),
                MultiSelect = false
            };
            listViewPlats.Columns.Add("ID", 0);
            listViewPlats.Columns.Add("Nom", 200);
            listViewPlats.Columns.Add("Description", 350);
            listViewPlats.Columns.Add("Prix", 100);
            listViewPlats.Columns.Add("Type", 100);
            listViewPlats.Columns.Add("Cuisine", 150);

            /// Nouveau ListView pour le récapitulatif
            var lblRecap = new Label
            {
                Text = "Récapitulatif de votre commande",
                Location = new Point(10, 280),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };
            this.Controls.Add(lblRecap);

            listViewRecap = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(10, 310),
                Size = new Size(960, 150)
            };
            listViewRecap.Columns.Add("Nom du plat", 300);
            listViewRecap.Columns.Add("Quantité", 100);
            listViewRecap.Columns.Add("Prix unitaire", 100);
            listViewRecap.Columns.Add("Total", 100);
            this.Controls.Add(listViewRecap);

            /// Panel de commande
            var orderPanel = new Panel
            {
                Location = new Point(10, 470),
                Size = new Size(960, 230)
            };

            numQuantite = new NumericUpDown
            {
                Location = new Point(10, 10),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 100,
                Value = 1
            };

            var btnAjouter = new Button
            {
                Text = "Ajouter à la commande",
                Location = new Point(100, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAjouter.Click += BtnAjouter_Click;

            var lblDate = new Label
            {
                Text = "Date de livraison souhaitée :",
                Location = new Point(10, 50),
                AutoSize = true
            };

            dtpLivraison = new DateTimePicker
            {
                Location = new Point(10, 75),
                Size = new Size(200, 25),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",
                MinDate = DateTime.Now.AddHours(1),
                MaxDate = DateTime.Now.AddDays(30)
            };

            lblTotal = new Label
            {
                Text = "Total : 0,00 €",
                Location = new Point(10, 110),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };

            lblReduction = new Label
            {
                Text = "",
                Location = new Point(10, 140),
                AutoSize = true,
                ForeColor = Color.Green
            };

            var btnCommander = new Button
            {
                Text = "Valider la commande",
                Location = new Point(10, 170),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCommander.Click += BtnCommander_Click;

            orderPanel.Controls.AddRange(new Control[] { 
                numQuantite, btnAjouter, lblDate, dtpLivraison, 
                lblTotal, lblReduction, btnCommander 
            });

            this.Controls.AddRange(new Control[] { searchPanel, listViewPlats, orderPanel });
        }

        private void LoadCuisiniers()
        {
            try
            {
                using var reader = new MySqlCommand(@"
                    SELECT DISTINCT 
                        u.ClientID,
                        u.NomU,
                        u.PrenomU,
                        CONCAT(u.PrenomU, ' ', u.NomU, ' (', c.SpecialiteC, ')') as NomComplet
                    FROM Utilisateur u
                    JOIN Cuisinier c ON u.ClientID = c.ClientID
                    JOIN Plat p ON c.ClientID = p.CuisinierID
                    WHERE p.EstDisponible = TRUE
                    ORDER BY u.NomU, u.PrenomU", _db.GetConnection()).ExecuteReader();

                var items = new List<KeyValuePair<int, string>>();
                items.Add(new KeyValuePair<int, string>(0, "Tous les cuisiniers"));

                while (reader.Read())
                {
                    items.Add(new KeyValuePair<int, string>(
                        reader.GetInt32("ClientID"),
                        reader.GetString("NomComplet")
                    ));
                }
                reader.Close();

                cmbCuisiniers.DataSource = items;
                cmbCuisiniers.DisplayMember = "Value";
                cmbCuisiniers.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des cuisiniers : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPlats()
        {
            try
            {
                _db.GetConnection().ClearAllPoolsAsync();

                /// Requête pour récupérer les plats disponibles, avec les ingrédients et les allergènes, 
                /// et les filtrer par cuisinier si un cuisinier est sélectionné, 
                string query = @"
                    SELECT 
                        p.PlatID,
                        p.NomPlat,
                        p.Description,
                        p.PrixParPersonne,
                        p.TypePlat,
                        p.NationaliteCuisine,
                        p.PlatDuJour,
                        GROUP_CONCAT(
                            CASE 
                                WHEN i.ContientAllergenes = 1 
                                THEN CONCAT(i.NomIngredient, ' (', i.TypeAllergene, ')')
                            END
                            SEPARATOR ', '
                        ) as Allergenes
                    FROM Plat p
                    LEFT JOIN PlatRecette pr ON p.PlatID = pr.PlatID
                    LEFT JOIN Ingredients i ON pr.IngredientID = i.IngredientID
                    WHERE p.EstDisponible = TRUE
                    AND (@cuisinierId = 0 OR p.CuisinierID = @cuisinierId)
                    AND (
                        @recherche IS NULL 
                        OR p.NomPlat LIKE @rechercheLike
                        OR p.Description LIKE @rechercheLike
                        OR p.NationaliteCuisine LIKE @rechercheLike
                    )
                    GROUP BY p.PlatID
                    ORDER BY p.PlatDuJour DESC, p.DateCreation DESC";

                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@cuisinierId", ((KeyValuePair<int, string>)cmbCuisiniers.SelectedItem).Key);
                cmd.Parameters.AddWithValue("@recherche", string.IsNullOrEmpty(txtRecherche.Text) ? DBNull.Value : (object)txtRecherche.Text);
                cmd.Parameters.AddWithValue("@rechercheLike", $"%{txtRecherche.Text}%");

                using var reader = cmd.ExecuteReader();
                listViewPlats.Items.Clear();

                while (reader.Read())
                {
                    var item = new ListViewItem(reader["PlatID"].ToString());
                    string nomPlat = reader["NomPlat"].ToString() ?? "";
                    if (Convert.ToBoolean(reader["PlatDuJour"]))
                        nomPlat = "⭐ " + nomPlat;

                    item.SubItems.Add(nomPlat);
                    item.SubItems.Add(reader["Description"].ToString());
                    item.SubItems.Add($"{Convert.ToDecimal(reader["PrixParPersonne"]):C2}");
                    item.SubItems.Add(reader["TypePlat"].ToString());
                    item.SubItems.Add(reader["NationaliteCuisine"].ToString());

                    if (!reader.IsDBNull(reader.GetOrdinal("Allergenes")))
                    {
                        item.ToolTipText = $"Allergènes : {reader["Allergenes"]}";
                    }

                    listViewPlats.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des plats : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbCuisiniers_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedCuisinier = (KeyValuePair<int, string>)cmbCuisiniers.SelectedItem;
            if (selectedCuisinier.Key != 0 && cuisinierSelectionne.HasValue && selectedCuisinier.Key != cuisinierSelectionne.Value)
            {
                if (platsSelectionnes.Count > 0)
                {
                    if (MessageBox.Show(
                        "Changer de cuisinier va vider votre commande en cours. Continuer ?",
                        "Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        cmbCuisiniers.SelectedValue = cuisinierSelectionne.Value;
                        return;
                    }
                    platsSelectionnes.Clear();
                    totalCommande = 0;
                    UpdateTotal();
                }
            }
            cuisinierSelectionne = selectedCuisinier.Key == 0 ? null : selectedCuisinier.Key;
            LoadPlats();
        }

        private void TxtRecherche_TextChanged(object sender, EventArgs e)
        {
            LoadPlats();
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (listViewPlats.SelectedItems.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner un plat", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = listViewPlats.SelectedItems[0];
            int platId = int.Parse(selectedItem.SubItems[0].Text);
            string nomPlat = selectedItem.SubItems[1].Text;
            decimal prix = decimal.Parse(selectedItem.SubItems[3].Text.Replace("€", "").Trim());
            int quantite = (int)numQuantite.Value;

            /// Vérifier le cuisinier
            if (!cuisinierSelectionne.HasValue)
            {
                try
                {
                    string query = "SELECT CuisinierID FROM Plat WHERE PlatID = @platId";
                    using var cmd = new MySqlCommand(query, _db.GetConnection());
                    cmd.Parameters.AddWithValue("@platId", platId);
                    cuisinierSelectionne = Convert.ToInt32(cmd.ExecuteScalar());
                    cmbCuisiniers.SelectedValue = cuisinierSelectionne;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la sélection du cuisinier : {ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            platsSelectionnes.Add((platId, quantite, prix, nomPlat));
            totalCommande += prix * quantite;

            // Mettre à jour le récapitulatif
            UpdateRecapitulatif();

            MessageBox.Show($"Ajouté : {nomPlat} x{quantite}", "Succès",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            UpdateTotal();
        }

        private void UpdateRecapitulatif()
        {
            listViewRecap.Items.Clear();
            foreach (var plat in platsSelectionnes)
            {
                var item = new ListViewItem(plat.nom);
                item.SubItems.Add(plat.quantite.ToString());
                item.SubItems.Add($"{plat.prix:C2}");
                item.SubItems.Add($"{(plat.prix * plat.quantite):C2}");
                listViewRecap.Items.Add(item);
            }
        }

        private void UpdateTotal()
        {
            if (cuisinierSelectionne.HasValue)
            {
                try
                {
                    string query = @"
                        SELECT 
                            COUNT(DISTINCT b.CommandeID) as NbCommandes,
                            CASE
                                WHEN COUNT(DISTINCT b.CommandeID) >= 10 THEN 0.20
                                WHEN COUNT(DISTINCT b.CommandeID) >= 7 THEN 0.15
                                WHEN COUNT(DISTINCT b.CommandeID) >= 4 THEN 0.10
                                ELSE 0
                            END as Reduction
                        FROM BonDeCommande_Liv b
                        WHERE b.ClientID = @clientId 
                        AND b.CuisinierID = @cuisinierId
                        AND b.Statut = 'Livrée'";

                    using var cmd = new MySqlCommand(query, _db.GetConnection());
                    cmd.Parameters.AddWithValue("@clientId", _userId);
                    cmd.Parameters.AddWithValue("@cuisinierId", cuisinierSelectionne.Value);
                    using var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        decimal reduction = Convert.ToDecimal(reader["Reduction"]);
                        int nbCommandes = Convert.ToInt32(reader["NbCommandes"]);

                        if (reduction > 0)
                        {
                            decimal montantReduction = totalCommande * reduction;
                            lblReduction.Text = $"Réduction fidélité ({reduction:P0}) : -{montantReduction:C2}";
                            lblTotal.Text = $"Total : {(totalCommande - montantReduction):C2}";
                        }
                        else
                        {
                            lblReduction.Text = $"Plus que {4 - nbCommandes} commande(s) pour obtenir 10% de réduction !";
                            lblTotal.Text = $"Total : {totalCommande:C2}";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du calcul de la réduction : {ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                lblTotal.Text = $"Total : {totalCommande:C2}";
                lblReduction.Text = "";
            }
        }

        private void BtnCommander_Click(object sender, EventArgs e)
        {
            if (platsSelectionnes.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner au moins un plat", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!cuisinierSelectionne.HasValue)
            {
                MessageBox.Show("Erreur : aucun cuisinier sélectionné", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string commandeId = $"CMD{DateTime.Now:yyyyMMddHHmmss}";
                decimal totalFinal = totalCommande;

                /// Calculer la réduction
                string queryReduction = @"
                    SELECT 
                        CASE
                            WHEN COUNT(DISTINCT b.CommandeID) >= 10 THEN 0.20
                            WHEN COUNT(DISTINCT b.CommandeID) >= 7 THEN 0.15
                            WHEN COUNT(DISTINCT b.CommandeID) >= 4 THEN 0.10
                            ELSE 0
                        END as Reduction
                    FROM BonDeCommande_Liv b
                    WHERE b.ClientID = @clientId 
                    AND b.CuisinierID = @cuisinierId
                    AND b.Statut = 'Livrée'";

                using (var cmd = new MySqlCommand(queryReduction, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@clientId", _userId);
                    cmd.Parameters.AddWithValue("@cuisinierId", cuisinierSelectionne.Value);
                    decimal reduction = Convert.ToDecimal(cmd.ExecuteScalar());
                    if (reduction > 0)
                    {
                        totalFinal = totalCommande * (1 - reduction);
                    }
                }

                /// Créer la commande
                string queryCommande = @"
                    INSERT INTO BonDeCommande_Liv 
                    (CommandeID, ClientID, CuisinierID, PrixPaye, DateSouhaitee, AdresseBon, Statut)
                    VALUES 
                    (@commandeId, @clientId, @cuisinierId, @prixTotal, @dateSouhaitee, @adresseBon, 'En attente')";

                using (var cmd = new MySqlCommand(queryCommande, _db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    cmd.Parameters.AddWithValue("@clientId", _userId);
                    cmd.Parameters.AddWithValue("@cuisinierId", cuisinierSelectionne.Value);
                    cmd.Parameters.AddWithValue("@prixTotal", totalFinal);
                    cmd.Parameters.AddWithValue("@dateSouhaitee", dtpLivraison.Value);
                    cmd.Parameters.AddWithValue("@adresseBon", "Adresse de l'utilisateur");
                    cmd.ExecuteNonQuery();
                }

                /// Ajouter les plats
                string queryPlats = @"
                    INSERT INTO Correspond 
                    (PlatID, CommandeID, Quantite, PrixUnitaire)
                    VALUES 
                    (@platId, @commandeId, @quantite, @prixUnitaire)";

                foreach (var plat in platsSelectionnes)
                {
                    using var cmd = new MySqlCommand(queryPlats, _db.GetConnection());
                    cmd.Parameters.AddWithValue("@platId", plat.platId);
                    cmd.Parameters.AddWithValue("@commandeId", commandeId);
                    cmd.Parameters.AddWithValue("@quantite", plat.quantite);
                    cmd.Parameters.AddWithValue("@prixUnitaire", plat.prix);
                    cmd.ExecuteNonQuery();
                }

                /// Afficher l'animation de progression de la commande
                var progressForm = new CommandeProgressForm(_db, commandeId);
                progressForm.ShowDialog();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création de la commande : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 