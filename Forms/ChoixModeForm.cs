using Rendu1.Modules;

namespace Forms
{
    public partial class ChoixModeForm : Form
    {
        public AuthModule.UserType? SelectedMode { get; private set; }

        public ChoixModeForm(string nomUtilisateur)
        {
            InitializeComponents(nomUtilisateur);
        }

        private void InitializeComponents(string nomUtilisateur)
        {
            this.Text = "Choix du Mode de Connexion";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 3,
                ColumnCount = 1
            };

            var lblMessage = new Label
            {
                Text = $"Bienvenue {nomUtilisateur} !\n\nVous êtes enregistré comme cuisinier.\nComment souhaitez-vous vous connecter ?",
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            panel.Controls.Add(lblMessage);

            var btnClient = new Button
            {
                Text = "Mode Client\n(pour commander des plats)",
                Height = 50,
                Dock = DockStyle.Fill
            };
            btnClient.Click += (s, e) =>
            {
                SelectedMode = AuthModule.UserType.Client;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            panel.Controls.Add(btnClient);

            var btnCuisinier = new Button
            {
                Text = "Mode Cuisinier\n(pour gérer vos plats)",
                Height = 50,
                Dock = DockStyle.Fill
            };
            btnCuisinier.Click += (s, e) =>
            {
                SelectedMode = AuthModule.UserType.Cuisinier;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            panel.Controls.Add(btnCuisinier);

            this.Controls.Add(panel);
        }
    }
} 