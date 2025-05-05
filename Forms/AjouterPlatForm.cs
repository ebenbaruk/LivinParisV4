using Rendu1;
using MySql.Data.MySqlClient;

namespace Forms
{
    public partial class AjouterPlatForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly int _cuisinierId;

        public AjouterPlatForm(DatabaseManager db, int cuisinierId)
        {
            _db = db;
            _cuisinierId = cuisinierId;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Ajouter un Plat";
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
            var txtNom = new TextBox { Width = 300, Anchor = AnchorStyles.Left };
            panel.Controls.Add(txtNom, 1, 0);

            /// Type de plat
            panel.Controls.Add(new Label { Text = "Type de plat :", Anchor = AnchorStyles.Left }, 0, 1);
            var cmbType = new ComboBox
            {
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left
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
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(txtDescription, 1, 2);

            /// Prix
            panel.Controls.Add(new Label { Text = "Prix par personne (€) :", Anchor = AnchorStyles.Left }, 0, 3);
            var txtPrix = new TextBox { Width = 300, Anchor = AnchorStyles.Left };
            panel.Controls.Add(txtPrix, 1, 3);

            /// Nombre de personnes
            panel.Controls.Add(new Label { Text = "Nombre de personnes :", Anchor = AnchorStyles.Left }, 0, 4);
            var numPersonnes = new NumericUpDown
            {
                Width = 300,
                Minimum = 1,
                Maximum = 10,
                Value = 1,
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(numPersonnes, 1, 4);

            /// Nationalité de la cuisine
            panel.Controls.Add(new Label { Text = "Nationalité de la cuisine :", Anchor = AnchorStyles.Left }, 0, 5);
            var txtNationalite = new TextBox { Width = 300, Anchor = AnchorStyles.Left };
            panel.Controls.Add(txtNationalite, 1, 5);

            /// Date de péremption
            panel.Controls.Add(new Label { Text = "Date de péremption :", Anchor = AnchorStyles.Left }, 0, 6);
            var datePeremption = new DateTimePicker
            {
                Width = 300,
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today,
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(datePeremption, 1, 6);

            /// Disponibilité
            panel.Controls.Add(new Label { Text = "Disponible :", Anchor = AnchorStyles.Left }, 0, 7);
            var chkDisponible = new CheckBox { Checked = true, Anchor = AnchorStyles.Left };
            panel.Controls.Add(chkDisponible, 1, 7);

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
                        INSERT INTO Plat (
                            CuisinierID, NomPlat, TypePlat, Description, 
                            DatePeremption, PrixParPersonne, NombrePersonnes, 
                            NationaliteCuisine, EstDisponible, DateCreation
                        ) VALUES (
                            @cuisinierId, @nom, @type, @description,
                            @datePeremption, @prix, @nombrePersonnes,
                            @nationalite, @disponible, NOW()
                        )";

                    using var cmd = new MySqlCommand(sql, _db.GetConnection());
                    cmd.Parameters.AddWithValue("@cuisinierId", _cuisinierId);
                    cmd.Parameters.AddWithValue("@nom", txtNom.Text.Trim());
                    cmd.Parameters.AddWithValue("@type", cmbType.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@description", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@datePeremption", datePeremption.Value.Date);
                    cmd.Parameters.AddWithValue("@prix", prix);
                    cmd.Parameters.AddWithValue("@nombrePersonnes", numPersonnes.Value);
                    cmd.Parameters.AddWithValue("@nationalite", txtNationalite.Text.Trim());
                    cmd.Parameters.AddWithValue("@disponible", chkDisponible.Checked);

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
    }
} 