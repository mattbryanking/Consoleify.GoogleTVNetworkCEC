using System.Text.Json;

namespace Consoleify.GoogleTVNetworkCEC
{
    public partial class SettingsForm : Form
    {
        private AppConfig config;
        public SettingsForm()
        {
            InitializeComponent();

            config = AppConfig.Load();

            txtIpAddress.Text = config.TvIpAddress;
            txtCommand.Text = config.HdmiCommand;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            config.TvIpAddress = txtIpAddress.Text.Trim();
            config.HdmiCommand = txtCommand.Text.Trim();

            config.Save();

            MessageBox.Show("Settings saved to settings.json!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}