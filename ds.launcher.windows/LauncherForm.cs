using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ds.launcher.windows;

public class LauncherForm : Form
{
    // ── Project registry ──────────────────────────────────────────────────────

    private static readonly (string Name, string Description, string Project)[] Projects =
    [
        ("Array",          "DynamicArray — resize, sort, search",                  "ds.array.example"),
        ("Linked List",    "Singly linked list — insert, remove, traverse",        "ds.linkedlist.example"),
        ("Stack & Queue",  "LIFO stack and FIFO queue side by side",               "ds.stack.queue.example"),
        ("Hash Table",     "Separate chaining, load factor, rehash",               "ds.hashtable.example"),
        ("Binary Tree",    "BST — insert, remove, 4 traversals, height",           "ds.binarytree.example"),
        ("Binary Heap",    "MinHeap / MaxHeap — bubble-up/down, heap sort",        "ds.binaryheap.example"),
        ("AVL Tree",       "Self-balancing BST — LL/RR/LR/RL rotations",          "ds.avltree.example"),
        ("Red-Black Tree", "Color-based balancing — insert/delete fixup cases",    "ds.redblacktree.example"),
        ("Graph",          "BFS, DFS, cycle detection, topo sort, Dijkstra",       "ds.graph.example"),
    ];

    // ── Colors ────────────────────────────────────────────────────────────────

    private static readonly Color BgDark    = Color.FromArgb(28,  28,  28);
    private static readonly Color BgCard    = Color.FromArgb(40,  40,  40);
    private static readonly Color BgHover   = Color.FromArgb(55,  55,  60);
    private static readonly Color BgRunning = Color.FromArgb(0,   90,  158);
    private static readonly Color Accent    = Color.FromArgb(0,   120, 215);
    private static readonly Color Border    = Color.FromArgb(65,  65,  70);
    private static readonly Color TextPri   = Color.FromArgb(240, 240, 240);
    private static readonly Color TextSec   = Color.FromArgb(150, 150, 155);

    // ── UI elements ───────────────────────────────────────────────────────────

    private Label _statusLabel = null!;

    // ── Constructor ───────────────────────────────────────────────────────────

    public LauncherForm()
    {
        SuspendLayout();

        Text            = "Data Structure Examples";
        ClientSize      = new Size(520, 660);
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = BgDark;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        Font            = new Font("Segoe UI", 9f);

        BuildUi();
        ResumeLayout(false);
    }

    // ── Layout ────────────────────────────────────────────────────────────────

    private void BuildUi()
    {
        // Header
        var header = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 72,
            BackColor = Accent
        };
        var titleLabel = new Label
        {
            Text      = "Data Structure Examples",
            Dock      = DockStyle.Fill,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        header.Controls.Add(titleLabel);

        // Subtitle
        var subtitle = new Label
        {
            Text      = "Click a topic to open its console demo in a new window",
            Dock      = DockStyle.Top,
            Height    = 34,
            ForeColor = TextSec,
            Font      = new Font("Segoe UI", 9f),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = BgDark,
            Padding   = new Padding(0, 6, 0, 0)
        };

        // Button list
        var scroll = new Panel
        {
            Dock        = DockStyle.Fill,
            AutoScroll  = true,
            BackColor   = BgDark,
            Padding     = new Padding(24, 8, 24, 8)
        };

        int y = 8;
        foreach (var (name, desc, project) in Projects)
        {
            var card = MakeCard(name, desc, project, y);
            scroll.Controls.Add(card);
            y += card.Height + 8;
        }

        // Status bar
        _statusLabel = new Label
        {
            Dock      = DockStyle.Bottom,
            Height    = 30,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = TextSec,
            Font      = new Font("Segoe UI", 8.5f),
            TextAlign = ContentAlignment.MiddleCenter,
            Text      = "Ready"
        };

        Controls.Add(scroll);
        Controls.Add(subtitle);
        Controls.Add(header);
        Controls.Add(_statusLabel);
    }

    private Panel MakeCard(string name, string desc, string project, int y)
    {
        var card = new Panel
        {
            Location  = new Point(0, y),
            Size      = new Size(472, 58),
            BackColor = BgCard,
            Cursor    = Cursors.Hand,
            Tag       = project
        };

        // Left accent stripe
        var stripe = new Panel
        {
            Location  = new Point(0, 0),
            Size      = new Size(4, 58),
            BackColor = Accent
        };

        var nameLabel = new Label
        {
            Text      = name,
            Location  = new Point(18, 10),
            Size      = new Size(420, 20),
            ForeColor = TextPri,
            Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            BackColor = Color.Transparent
        };

        var descLabel = new Label
        {
            Text      = desc,
            Location  = new Point(18, 31),
            Size      = new Size(420, 18),
            ForeColor = TextSec,
            Font      = new Font("Segoe UI", 8.5f),
            BackColor = Color.Transparent
        };

        // Draw a subtle bottom border
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(Border, 1);
            e.Graphics.DrawLine(pen, 0, card.Height - 1, card.Width, card.Height - 1);
        };

        card.Controls.AddRange([stripe, nameLabel, descLabel]);

        // Hover + click on every child too
        foreach (Control c in new Control[] { card, nameLabel, descLabel })
        {
            c.MouseEnter += (_, _) => card.BackColor = BgHover;
            c.MouseLeave += (_, _) => card.BackColor = BgCard;
            c.Click      += OnCardClick;
            c.Tag         = project;
        }
        stripe.MouseEnter += (_, _) => card.BackColor = BgHover;
        stripe.MouseLeave += (_, _) => card.BackColor = BgCard;

        return card;
    }

    // ── Launch ────────────────────────────────────────────────────────────────

    private void OnCardClick(object? sender, EventArgs e)
    {
        var project      = (string)((Control)sender!).Tag!;
        var solutionRoot = FindSolutionRoot();

        if (solutionRoot is null)
        {
            MessageBox.Show(
                "Could not locate the solution root (datastructureexamples.slnx).\n" +
                "Make sure you are running from within the repository.",
                "Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var projectPath = Path.Combine(solutionRoot, project);
        var displayName = Projects.First(p => p.Project == project).Name;

        _statusLabel.Text      = $"Launching {displayName}…";
        _statusLabel.ForeColor = Accent;

        // Open a new console window and keep it open after the demo finishes (/K)
        Process.Start(new ProcessStartInfo
        {
            FileName         = "cmd.exe",
            Arguments        = $"/K dotnet run --project \"{projectPath}\"",
            UseShellExecute  = true,
            WorkingDirectory = solutionRoot
        });

        // Reset status after a moment
        var timer = new System.Windows.Forms.Timer { Interval = 2500 };
        timer.Tick += (_, _) =>
        {
            _statusLabel.Text      = "Ready";
            _statusLabel.ForeColor = TextSec;
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }

    // Walk up from the executable's location until we find the .slnx file
    private static string? FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.slnx").Length > 0) return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
