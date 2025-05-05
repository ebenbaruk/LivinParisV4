using Rendu1;
using MySql.Data.MySqlClient;

namespace Forms
{
    public partial class ModifierPlatForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly int _cuisinierId;
        private readonly int _platId;

        public ModifierPlatForm(DatabaseManager db, int cuisinierId, int platId)
        {
            _db = db;
            _cuisinierId = cuisinierId;
            _platId = platId;
            InitializeComponents();
            ChargerDonneesPlat();
        }

        private void InitializeComponents()
        {
            this.Text = "Modifier un Plat";
            this.Size = new Size(500, 600);
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

            /// Nom du plat
            panel.Controls.Add(new Label { Text = "Nom du plat :", Anchor = AnchorStyles.Left }, 0, 0);
            var txtNom = new TextBox { Width = 300, Anchor = AnchorStyles.Left, Name = "txtNom" };
            panel.Controls.Add(txtNom, 1, 0);

            /// Type de plat
            panel.Controls.Add(new Label { Text = "Type de plat :", Anchor = AnchorStyles.Left }, 0, 1);
            var cmbType = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left,
                Name = "cmbType"
            };
            cmbType.Items.AddRange(new string[] { "Entrée", "Plat Principal", "Dessert" });
            panel.Controls.Add(cmbType, 1, 1);

            /// Description
            panel.Controls.Add(new Label { Text = "Description :", Anchor = AnchorStyles.Left }, 0, 2);
            var txtDescription = new TextBox
            {
                Width = 300,
                Height = 100,
                Multiline = true,
                Anchor = AnchorStyles.Left,
                Name = "txtDescription"
            };
            panel.Controls.Add(txtDescription, 1, 2);

            /// Prix
            panel.Controls.Add(new Label { Text = "Prix par personne (€) :", Anchor = AnchorStyles.Left }, 0, 3);
            var txtPrix = new TextBox { Width = 300, Anchor = AnchorStyles.Left, Name = "txtPrix" };
            panel.Controls.Add(txtPrix, 1, 3);

            /// Nombre de personnes
            panel.Controls.Add(new Label { Text = "Nombre de personnes :", Anchor = AnchorStyles.Left }, 0, 4);
            var numPersonnes = new NumericUpDown
            {
                Width = 300,
                Minimum = 1,
                Maximum = 10,
                Value = 1,
                Anchor = AnchorStyles.Left,
                Name = "numPersonnes"
            };
            panel.Controls.Add(numPersonnes, 1, 4);

            /// Nationalité de la cuisine
            panel.Controls.Add(new Label { Text = "Nationalité de la cuisine :", Anchor = AnchorStyles.Left }, 0, 5);
            var txtNationalite = new TextBox { Width = 300, Anchor = AnchorStyles.Left, Name = "txtNationalite" };
            panel.Controls.Add(txtNationalite, 1, 5);

            /// Date de péremption
            panel.Controls.Add(new Label { Text = "Date de péremption :", Anchor = AnchorStyles.Left }, 0, 6);
            var datePeremption = new DateTimePicker
            {
                Width = 300,
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today,
                Anchor = AnchorStyles.Left,
                Name = "datePeremption"
            };
            panel.Controls.Add(datePeremption, 1, 6);

            /// Disponibilité
            panel.Controls.Add(new Label { Text = "Disponible :", Anchor = AnchorStyles.Left }, 0, 7);
            var chkDisponible = new CheckBox { Checked = true, Anchor = AnchorStyles.Left, Name = "chkDisponible" };
            panel.Controls.Add(chkDisponible, 1, 7);

            /// Plat du jour
            panel.Controls.Add(new Label { Text = "Plat du jour :", Anchor = AnchorStyles.Left }, 0, 8);
            var chkPlatDuJour = new CheckBox { Anchor = AnchorStyles.Left, Name = "chkPlatDuJour" };
            panel.Controls.Add(chkPlatDuJour, 1, 8);

            /// Boutons
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(10)
            };

            var btnAnnuler = new Button
            {
                Text = "Annuler",
                DialogResult = DialogResult.Cancel
            };
            btnAnnuler.Click += (s, e) => this.Close();

            var btnValider = new Button
            {
                Text = "Valider",
                DialogResult = DialogResult.OK
            };
            btnValider.Click += (s, e) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtNom.Text))
                        throw new Exception("Le nom du plat est obligatoire.");

                    if (cmbType.SelectedIndex == -1)
                        throw new Exception("Veuillez sélectionner un type de plat.");

                    if (string.IsNullOrWhiteSpace(txtDescription.Text))
                        throw new Exception("La description est obligatoire.");

                    if (!decimal.TryParse(txtPrix.Text, out decimal prix) || prix <= 0)
                        throw new Exception("Le prix doit être un nombre positif.");

                    if (string.IsNullOrWhiteSpace(txtNationalite.Text))
                        throw new Exception("La nationalité de la cuisine est obligatoire.");

                    string sql = @"
                        UPDATE Plat SET
                            NomPlat = @nom,
                            TypePlat = @type,
                            Description = @description,
                            DatePeremption = @datePeremption,
                            PrixParPersonne = @prix,
                            NombrePersonnes = @nombrePersonnes,
                            NationaliteCuisine = @nationalite,
                            EstDisponible = @disponible,
                            PlatDuJour = @platDuJour
                        WHERE PlatID = @platId AND CuisinierID = @cuisinierId";

                    using var cmd = new MySqlCommand(sql, _db.GetConnection());
                    cmd.Parameters.AddWithValue("@platId", _platId);
                    cmd.Parameters.AddWithValue("@cuisinierId", _cuisinierId);
                    cmd.Parameters.AddWithValue("@nom", txtNom.Text.Trim());
                    cmd.Parameters.AddWithValue("@type", cmbType.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@description", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@datePeremption", datePeremption.Value.Date);
                    cmd.Parameters.AddWithValue("@prix", prix);
                    cmd.Parameters.AddWithValue("@nombrePersonnes", numPersonnes.Value);
                    cmd.Parameters.AddWithValue("@nationalite", txtNationalite.Text.Trim());
                    cmd.Parameters.AddWithValue("@disponible", chkDisponible.Checked);
                    cmd.Parameters.AddWithValue("@platDuJour", chkPlatDuJour.Checked);

                    cmd.ExecuteNonQuery();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                }
            };

            btnPanel.Controls.AddRange(new Control[] { btnValider, btnAnnuler });

            this.Controls.AddRange(new Control[] { panel, btnPanel });
        }

        private void ChargerDonneesPlat()
        {
            try
            {
                string sql = @"
                    SELECT * FROM Plat 
                    WHERE PlatID = @platId AND CuisinierID = @cuisinierId";

                using var cmd = new MySqlCommand(sql, _db.GetConnection());
                cmd.Parameters.AddWithValue("@platId", _platId);
                cmd.Parameters.AddWithValue("@cuisinierId", _cuisinierId);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    this.Controls.Find("txtNom", true)[0].Text = reader["NomPlat"].ToString();
                    ((ComboBox)this.Controls.Find("cmbType", true)[0]).SelectedItem = reader["TypePlat"].ToString();
                    this.Controls.Find("txtDescription", true)[0].Text = reader["Description"].ToString();
                    this.Controls.Find("txtPrix", true)[0].Text = reader["PrixParPersonne"].ToString();
                    ((NumericUpDown)this.Controls.Find("numPersonnes", true)[0]).Value = Convert.ToInt32(reader["NombrePersonnes"]);
                    this.Controls.Find("txtNationalite", true)[0].Text = reader["NationaliteCuisine"].ToString();
                    ((DateTimePicker)this.Controls.Find("datePeremption", true)[0]).Value = Convert.ToDateTime(reader["DatePeremption"]);
                    ((CheckBox)this.Controls.Find("chkDisponible", true)[0]).Checked = Convert.ToBoolean(reader["EstDisponible"]);
                    ((CheckBox)this.Controls.Find("chkPlatDuJour", true)[0]).Checked = Convert.ToBoolean(reader["PlatDuJour"]);
                }
                else
                {
                    MessageBox.Show("Plat non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
    }
} 