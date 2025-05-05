using Rendu1;
using MySql.Data.MySqlClient;

namespace Forms
{
    public partial class DetailsCommandeForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly string _commandeId;
        private ComboBox cmbStatut;

        public DetailsCommandeForm(DatabaseManager db, string commandeId)
        {
            _db = db;
            _commandeId = commandeId;
            InitializeComponents();
            ChargerDetailsCommande();
        }

        private void InitializeComponents()
        {
            this.Text = $"Détails de la Commande {_commandeId}";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                RowCount = 10,
                ColumnCount = 2
            };

            // Labels pour les informations
            var labels = new[]
            {
                "Date de commande :",
                "Date souhaitée :",
                "Client :",
                "Adresse de livraison :",
                "Statut :",
                "Prix total :",
                "Plats commandés :"
            };

            int row = 0;
            foreach (var label in labels)
            {
                panel.Controls.Add(new Label
                {
                    Text = label,
                    Anchor = AnchorStyles.Left,
                    Font = new Font(this.Font, FontStyle.Bold)
                }, 0, row);

                if (label == "Statut :")
                {
                    cmbStatut = new ComboBox
                    {
                        Width = 300,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Anchor = AnchorStyles.Left
                    };
                    cmbStatut.Items.AddRange(new string[]
                    {
                        "En attente",
                        "Acceptée",
                        "En préparation",
                        "En livraison",
                        "Livrée",
                        "Annulée"
                    });
                    panel.Controls.Add(cmbStatut, 1, row);
                }
                else
                {
                    panel.Controls.Add(new Label
                    {
                        Width = 300,
                        Anchor = AnchorStyles.Left,
                        Name = $"lbl{label.Replace(" :", "").Replace(" ", "")}"
                    }, 1, row);
                }
                row++;
            }

            /// Boutons
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(10)
            };

            var btnFermer = new Button
            {
                Text = "Fermer",
                DialogResult = DialogResult.Cancel
            };
            btnFermer.Click += (s, e) => this.Close();

            var btnEnregistrer = new Button
            {
                Text = "Enregistrer",
                DialogResult = DialogResult.OK
            };
            btnEnregistrer.Click += (s, e) =>
            {
                try
                {
                    /// Requête pour mettre à jour le statut de la commande.
                    string sql = @"
                        UPDATE BonDeCommande_Liv 
                        SET Statut = @statut,
                            DateLivraison = CASE 
                                WHEN @statut = 'Livrée' THEN NOW()
                                ELSE DateLivraison
                            END
                        WHERE CommandeID = @commandeId";

                    using var cmd = new MySqlCommand(sql, _db.GetConnection());
                    cmd.Parameters.AddWithValue("@commandeId", _commandeId);
                    cmd.Parameters.AddWithValue("@statut", cmbStatut.SelectedItem.ToString());
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Statut mis à jour avec succès.", "Succès",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la mise à jour : {ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                }
            };

            btnPanel.Controls.AddRange(new Control[] { btnEnregistrer, btnFermer });

            this.Controls.AddRange(new Control[] { panel, btnPanel });
        }

        private void ChargerDetailsCommande()
        {
            try
            {
                /// Requête pour récupérer les détails de la commande.
                string sql = @"
                    SELECT 
                        b.DateCommande,
                        b.DateSouhaitee,
                        CONCAT(u.PrenomU, ' ', u.NomU) as NomClient,
                        b.AdresseBon,
                        b.Statut,
                        b.PrixPaye,
                        GROUP_CONCAT(CONCAT(p.NomPlat, ' x', c.Quantite) SEPARATOR '\n') as Plats
                    FROM BonDeCommande_Liv b
                    JOIN Utilisateur u ON b.ClientID = u.ClientID
                    LEFT JOIN Correspond c ON b.CommandeID = c.CommandeID
                    LEFT JOIN Plat p ON c.PlatID = p.PlatID
                    WHERE b.CommandeID = @commandeId
                    GROUP BY b.CommandeID, b.DateCommande, b.DateSouhaitee, 
                             NomClient, b.AdresseBon, b.Statut, b.PrixPaye";

                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@commandeId", _commandeId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    this.Controls.Find("lblDatecommande", true)[0].Text =
                        Convert.ToDateTime(reader["DateCommande"]).ToString("dd/MM/yyyy HH:mm");
                    
                    this.Controls.Find("lblDatesouhaitee", true)[0].Text =
                        Convert.ToDateTime(reader["DateSouhaitee"]).ToString("dd/MM/yyyy HH:mm");
                    
                    this.Controls.Find("lblClient", true)[0].Text = reader["NomClient"].ToString();
                    this.Controls.Find("lblAdressedelivraison", true)[0].Text = reader["AdresseBon"].ToString();
                    this.Controls.Find("lblPrixtotal", true)[0].Text = $"{Convert.ToDecimal(reader["PrixPaye"]):C2}";
                    this.Controls.Find("lblPlatscommandes", true)[0].Text = reader["Plats"].ToString();

                    cmbStatut.SelectedItem = reader["Statut"].ToString();
                }
                else
                {
                    MessageBox.Show("Commande non trouvée.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
    }
} 