using Rendu1;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace Forms
{
    public partial class CommandeProgressForm : Form
    {
        private readonly DatabaseManager _db;
        private readonly string _commandeId;
        private readonly System.Windows.Forms.Timer _timer;
        private int _currentStep = 0;
        private Panel mainPanel;
        private Label lblStatus;
        private ProgressBar progressBar;
        private PictureBox pictureBox;
        private Label lblDetails;

        private readonly string[] _steps = {
            "🕒 Commande en attente de validation",
            "✅ Commande validée par le restaurant",
            "👨‍🍳 Préparation en cours",
            "🚴 Livraison en cours",
            "🎉 Commande livrée"
        };

        private readonly string[] _details = {
            "Le restaurant examine votre commande...",
            "Le restaurant a accepté votre commande et va commencer la préparation",
            "Vos plats sont en cours de préparation avec soin",
            "Un livreur est en route avec votre commande",
            "Votre commande a été livrée avec succès !"
        };

        public CommandeProgressForm(DatabaseManager db, string commandeId)
        {
            _db = db;
            _commandeId = commandeId;
            InitializeComponents();

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 3000; /// 3 secondes entre chaque étape
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void InitializeComponents()
        {
            this.Text = "Suivi de votre commande";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            /// Titre de la commande
            var lblTitle = new Label
            {
                Text = $"Commande {_commandeId}",
                Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            /// Status actuel
            lblStatus = new Label
            {
                Text = _steps[0],
                Font = new Font(this.Font.FontFamily, 12),
                AutoSize = true,
                Location = new Point(20, 60)
            };

            /// Barre de progression
            progressBar = new ProgressBar
            {
                Location = new Point(20, 100),
                Size = new Size(540, 30),
                Maximum = _steps.Length - 1,
                Value = 0
            };

            /// Image représentative
            pictureBox = new PictureBox
            {
                Size = new Size(100, 100),
                Location = new Point(240, 150),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            /// Détails
            lblDetails = new Label
            {
                Text = _details[0],
                Font = new Font(this.Font.FontFamily, 10),
                AutoSize = true,
                Location = new Point(20, 270),
                MaximumSize = new Size(540, 0)
            };

            mainPanel.Controls.AddRange(new Control[] { lblTitle, lblStatus, progressBar, pictureBox, lblDetails });
            this.Controls.Add(mainPanel);
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            _currentStep++;
            if (_currentStep >= _steps.Length)
            {
                _timer.Stop();
                await Task.Delay(2000);
                this.Close();
                return;
            }

            progressBar.Value = _currentStep;
            lblStatus.Text = _steps[_currentStep];
            lblDetails.Text = _details[_currentStep];

            /// Mettre à jour le statut dans la base de données
            string status = _currentStep switch
            {
                0 => "En attente",
                1 => "Acceptée",
                2 => "En préparation",
                3 => "En livraison",
                4 => "Livrée",
                _ => "En attente"
            };

            try
            {
                string query = "UPDATE BonDeCommande_Liv SET Statut = @status WHERE CommandeID = @commandeId";
                using var cmd = new MySqlCommand(query, _db.GetConnection());
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@commandeId", _commandeId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour du statut : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 