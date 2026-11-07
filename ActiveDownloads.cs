using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using JRPC_Client;
using XDevkit;

internal static class Program
{
    internal const string DefaultAddress = "";
    internal static readonly string AddressFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xbox-address.txt");

    [STAThread]
    private static int Main(string[] args)
    {
        // Also provides a way to check the same connection path without a window.
        if (args.Length > 0 && args[0] == "--open")
        {
            try { OpenDownloads(args.Length > 1 ? args[1] : SavedAddress()); return 0; }
            catch { return 1; }
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DownloadsForm());
        return 0;
    }

    internal static string SavedAddress()
    {
        try
        {
            string address = File.ReadAllText(AddressFile).Trim();
            if (!String.IsNullOrWhiteSpace(address)) return address;
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return DefaultAddress;
    }

    internal static void OpenDownloads(string address)
    {
        IXboxConsole console = null;
        if (!JRPC.Connect(null, out console, address))
            throw new InvalidOperationException("Could not connect to the Xbox.");

        // XamShowMarketplaceUI on the system thread, ActiveDownloads entry 12.
        // This opens the screen; it does not add, cancel or change downloads.
        uint result = JRPC.Call<uint>(console, JRPC.ThreadType.System, "xam.xex", 0x2C7,
            new object[] { (uint)0, (uint)12, (ulong)0, (uint)0, (uint)0, (uint)0 });
        if (result != 0)
            throw new InvalidOperationException("The Xbox could not open Active Downloads.");
    }
}

internal sealed class DownloadsForm : Form
{
    private readonly TextBox address;
    private readonly Button open;
    private readonly Label status;
    private bool busy;

    internal DownloadsForm()
    {
        Text = "Xbox Active Downloads";
        ClientSize = new Size(424, 254);
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(248, 249, 247);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;

        Label title = new Label {
            Text = "Xbox 360 downloads", AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(22, 18)
        };
        Label addressLabel = new Label {
            Text = "Xbox address", AutoSize = true, Location = new Point(24, 65)
        };
        address = new TextBox {
            Text = Program.SavedAddress(), Location = new Point(24, 89),
            Size = new Size(376, 27), TabIndex = 0, AccessibleName = "Xbox address",
            MaxLength = 253
        };
        open = new Button {
            Text = "Open Active Downloads", Location = new Point(24, 134),
            Size = new Size(376, 44), TabIndex = 1,
            BackColor = Color.FromArgb(16, 124, 16), ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            AccessibleName = "Open Active Downloads"
        };
        open.FlatAppearance.BorderSize = 0;
        status = new Label {
            Text = "Ready. Turn on your Xbox, then click the button.",
            Location = new Point(24, 193), Size = new Size(376, 48),
            ForeColor = Color.FromArgb(75, 80, 75)
        };
        Controls.AddRange(new Control[] { title, addressLabel, address, open, status });
        AcceptButton = open;
        open.Click += async delegate { await OpenAsync(); };
    }

    private async Task OpenAsync()
    {
        if (busy) return;
        string target = address.Text.Trim();
        if (String.IsNullOrWhiteSpace(target) || Uri.CheckHostName(target) == UriHostNameType.Unknown)
        {
            status.ForeColor = Color.FromArgb(160, 40, 30);
            status.Text = "Enter the Xbox IP address or console name.";
            address.Focus();
            return;
        }

        busy = true;
        open.Enabled = false;
        address.Enabled = false;
        open.Text = "Opening...";
        status.ForeColor = Color.FromArgb(75, 80, 75);
        status.Text = "Connecting to your Xbox...";
        try
        {
            await Task.Run(delegate { Program.OpenDownloads(target); });
            if (IsDisposed) return;
            status.ForeColor = Color.FromArgb(16, 110, 16);
            status.Text = "Active Downloads opened on your Xbox.";
            try { File.WriteAllText(Program.AddressFile, target); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        catch (Exception)
        {
            if (IsDisposed) return;
            status.ForeColor = Color.FromArgb(160, 40, 30);
            status.Text = "Couldn't open downloads. Check the Xbox is on, its address is correct, and it is connected to your network.";
        }
        finally
        {
            busy = false;
            if (!IsDisposed)
            {
                open.Enabled = true;
                address.Enabled = true;
                open.Text = "Open Active Downloads";
            }
        }
    }
}
