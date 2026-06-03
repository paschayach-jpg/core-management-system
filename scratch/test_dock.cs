using System;
using System.Drawing;
using System.Windows.Forms;

public class TestForm : Form
{
    public TestForm()
    {
        this.Size = new Size(400, 400);

        Panel fillPanel = new Panel();
        fillPanel.BackColor = Color.LightBlue;
        fillPanel.Dock = DockStyle.Fill;

        Label fillLabel = new Label();
        fillLabel.Text = "This is Fill Panel (starts at Y=0 if overlap)";
        fillLabel.Location = new Point(10, 10);
        fillLabel.AutoSize = true;
        fillPanel.Controls.Add(fillLabel);

        Panel topPanel = new Panel();
        topPanel.BackColor = Color.LightCoral;
        topPanel.Dock = DockStyle.Top;
        topPanel.Height = 100;

        Label topLabel = new Label();
        topLabel.Text = "This is Top Panel (height 100)";
        topLabel.Location = new Point(10, 10);
        topLabel.AutoSize = true;
        topPanel.Controls.Add(topLabel);

        // Try adding topPanel first, then fillPanel
        this.Controls.Add(topPanel);
        this.Controls.Add(fillPanel);

        this.Load += (s, e) => {
            Console.WriteLine("fillPanel bounds: " + fillPanel.Bounds);
            Console.WriteLine("topPanel bounds: " + topPanel.Bounds);
            Application.Exit();
        };
    }

    public static void Main()
    {
        Application.Run(new TestForm());
    }
}
