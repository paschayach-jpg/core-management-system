# WFE Form UI Design Pattern — CMS Project (Master Reference: WorkflowDashboardFormNew)

ใช้ command นี้เป็น reference เมื่อสร้าง WinForms ใหม่ในโมดูล Workflow Engine (WFE)
**Master style มาจาก `WorkflowDashboardFormNew` — ทุก WFE form ต้องใช้ style นี้**

---

## โครงสร้าง Layout หลัก

```
Form (ClientSize: 1280×800, MinimumSize: 1000×650, WindowState=Maximized)
├── pnlRibbonTabs       (Dock=Top, Height=32, BackColor=#1F3864 navy)
├── pnlRibbonBorder     (Dock=Top, Height=1,  BackColor=#C8D7F0)
├── pnlToolbarContainer (Dock=Top, Height=82, BackColor=White)
└── pnlAppBody          (Dock=Fill, BackColor=#F3F6FB)
    ├── pnlSidebarContainer (Dock=Left, Width=202, BackColor=White, AutoScroll=true)
    ├── pnlNavBorder        (Dock=Left, Width=1,   BackColor=#E1E4EB)
    └── pnlContent          (Dock=Fill, BackColor=#F3F6FB)
        ├── pnlBreadcrumb   (Dock=Top, Height=28)
        ├── pnlTitleBar     (Dock=Top, Height=62, BackColor=White)
        └── pnlDashboard    (Dock=Fill, BackColor=White, Padding=(16,8,16,0))
            ├── flpCards        (Dock=Top, Height=98)
            ├── pnlFilterBar    (Dock=Top, Height=44)
            ├── lvTasks / dgv   (Dock=Fill)
            └── pnlPagination   (Dock=Bottom, Height=44)
```

**กฎสำคัญ:** ใช้ `Load` event เรียก `BuildRibbonTabs()`, `BuildToolbar()`, `BuildSidebar()` แทนการฝัง controls ใน Designer

---

## 1. Ribbon Tab Bar — BuildRibbonTabs()

```csharp
// BackColor: Color.FromArgb(31, 56, 100)  — #1F3864 dark navy
// Height: 32, Padding=(4,0,4,0)

private void BuildRibbonTabs()
{
    var tabs = new[]
    {
        new { Text = "แฟ้ม",      Active = false },
        new { Text = "หน้าแรก",   Active = true  },  // ← เปลี่ยนตาม active tab
        new { Text = "ดำเนินการ", Active = false },
        new { Text = "มุมมอง",    Active = false },
        new { Text = "เครื่องมือ",Active = false },
        new { Text = "ช่วยเหลือ", Active = false },
    };

    int x = 8;
    foreach (var t in tabs)
    {
        var btn = new Button
        {
            Text      = t.Text,
            Location  = new Point(x, 0),
            Size      = new Size(MeasureTabWidth(t.Text) + 24, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = t.Active ? Color.White : Color.FromArgb(190, 210, 245),
            Font      = t.Active
                ? new Font("Segoe UI", 9.5F, FontStyle.Bold)
                : new Font("Segoe UI", 9.5F),
            Cursor    = Cursors.Hand,
            TabStop   = false,
        };
        btn.FlatAppearance.BorderSize            = 0;
        btn.FlatAppearance.MouseOverBackColor    = Color.FromArgb(50, 80, 130);
        btn.FlatAppearance.MouseDownBackColor    = Color.FromArgb(40, 70, 120);

        if (t.Active)
        {
            // 3px white underline บอก active tab
            btn.Paint += (s, pe) =>
                pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
        }

        pnlRibbonTabs.Controls.Add(btn);
        x += btn.Width + 2;
    }
}

private int MeasureTabWidth(string text)
{
    using (var g = CreateGraphics())
    using (var f = new Font("Segoe UI", 9.5F))
        return (int)g.MeasureString(text, f).Width;
}
```

---

## 2. Toolbar — BuildToolbar()

```csharp
// White background, Height=82
// แต่ละปุ่ม = Panel(58×60): lblIcon (emoji 16pt, Y=6) + lblText (7.5pt, Y=36)
// Group label ที่ Y=62, separator Panel Width=1

private void BuildToolbar()
{
    var groups = new[]
    {
        new { Header = "ดำเนินการ", Items = new[]
        {
            new { Icon = "✔", Text = "ดำเนินการ", Tag = "execute" },
            new { Icon = "👁", Text = "รายละเอียด",Tag = "detail"  },
            new { Icon = "→", Text = "ส่งต่อ",    Tag = "forward" },
        }},
        // เพิ่ม group ตามความต้องการของแต่ละ form
    };

    int x = 8;
    foreach (var group in groups)
    {
        int groupStartX = x;

        foreach (var item in group.Items)
        {
            var pnlBtn = new Panel { Location = new Point(x, 2), Size = new Size(58, 60),
                BackColor = Color.White, Cursor = Cursors.Hand, Tag = item.Tag };

            var lblIcon = new Label { Text = item.Icon, Font = new Font("Segoe UI", 16F),
                ForeColor = _navyDark, TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 6), Size = new Size(58, 30) };

            var lblText = new Label { Text = item.Text, Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.FromArgb(55, 65, 90), TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 36), Size = new Size(58, 20) };

            pnlBtn.Controls.Add(lblIcon);
            pnlBtn.Controls.Add(lblText);

            // Hover: #F5F9FF, Leave: White
            EventHandler enter = (s, ev) => { pnlBtn.BackColor = lblIcon.BackColor = lblText.BackColor = _navHover; };
            EventHandler leave = (s, ev) => { pnlBtn.BackColor = lblIcon.BackColor = lblText.BackColor = Color.White; };
            var tag = item.Tag;
            EventHandler click = (s, ev) => OnToolbarClick(tag);

            foreach (Control c in new Control[] { pnlBtn, lblIcon, lblText })
            { c.MouseEnter += enter; c.MouseLeave += leave; c.Click += click; }

            pnlToolbarContainer.Controls.Add(pnlBtn);
            x += 60;
        }

        // Group label
        pnlToolbarContainer.Controls.Add(new Label
        {
            Text = group.Header, Font = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(120, 130, 150), TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(groupStartX, 62), Size = new Size(x - groupStartX, 10),
        });

        // Separator (ข้าม group สุดท้าย)
        if (group != groups[groups.Length - 1])
        {
            pnlToolbarContainer.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(220, 225, 235),
                Location = new Point(x + 2, 8), Size = new Size(1, 54),
            });
            x += 10;
        }
    }

    // Bottom border
    pnlToolbarContainer.Controls.Add(new Panel
        { BackColor = Color.FromArgb(220, 225, 235), Dock = DockStyle.Bottom, Height = 1 });
}
```

---

## 3. Sidebar Navigation — BuildSidebar()

```csharp
// White background, Width=202
// Section header: Segoe UI 8pt Bold, ForeColor=#828CA0

private readonly Color _navyDark  = Color.FromArgb(31, 56, 100);   // #1F3864
private readonly Color _navActive = Color.FromArgb(232, 240, 255); // #E8F0FF
private readonly Color _navHover  = Color.FromArgb(245, 249, 255); // #F5F9FF
private Panel _activeNavItem;

private void BuildSidebar()
{
    var orange = Color.FromArgb(230, 115, 0);
    var navy   = Color.FromArgb(31, 56, 100);

    var sections = new[]
    {
        new NavSection { Header = "การทำงาน", Items = new[]
        {
            new NavItem { Icon="🏠", Text="งานของฉัน",         Badge=8,  BadgeColor=orange, Tag="mytasks",  IsDefault=true  },
            new NavItem { Icon="✓",  Text="ตรวจสอบ Workflow",  Badge=24, BadgeColor=orange, Tag="monitor"                  },
            new NavItem { Icon="+",  Text="เริ่ม Workflow",    Badge=0,  BadgeColor=navy,   Tag="create"                   },
            new NavItem { Icon="📋", Text="ประวัติ Workflow",  Badge=0,  BadgeColor=navy,   Tag="history"                  },
        }},
        new NavSection { Header = "จัดการ", Items = new[]
        {
            new NavItem { Icon="📄", Text="จัดการ Template",  Badge=0, BadgeColor=navy,   Tag="template" },
            new NavItem { Icon="🎨", Text="Workflow Designer", Badge=0, BadgeColor=navy,   Tag="designer" },
            new NavItem { Icon="☑",  Text="อนุมัติ Template", Badge=3, BadgeColor=orange, Tag="approval" },
        }},
        new NavSection { Header = "ตั้งค่า", Items = new[]
        {
            new NavItem { Icon="👥", Text="Delegation",  Badge=0, BadgeColor=navy, Tag="delegation" },
            new NavItem { Icon="📊", Text="รายงาน SLA", Badge=0, BadgeColor=navy, Tag="sla"        },
            new NavItem { Icon="⚙",  Text="ตั้งค่า",   Badge=0, BadgeColor=navy, Tag="settings"   },
        }},
    };

    int y = 4;
    foreach (var section in sections)
    {
        pnlSidebarContainer.Controls.Add(new Label
        {
            Text = section.Header, Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(130, 140, 160), Location = new Point(16, y + 4), Size = new Size(180, 20),
        });
        y += 26;

        foreach (var item in section.Items)
        {
            var nav = BuildNavItem(item);
            nav.Location = new Point(0, y);
            pnlSidebarContainer.Controls.Add(nav);
            if (item.IsDefault) SetActiveNav(nav);
            y += 34;
        }
        y += 4;
    }
}
```

**BuildNavItem — Panel nav item:**
```csharp
private Panel BuildNavItem(NavItem item)
{
    var pnl = new Panel { Size = new Size(202, 34), BackColor = Color.White,
        Cursor = Cursors.Hand, Tag = item.Tag };

    // 3px accent strip ซ้าย
    var pnlAccent = new Panel { Location = new Point(0, 4), Size = new Size(3, 26),
        BackColor = Color.Transparent, Name = "accent" };

    var lblIcon = new Label { Text = item.Icon, Font = new Font("Segoe UI", 11F),
        Location = new Point(14, 7), Size = new Size(22, 22),
        TextAlign = ContentAlignment.MiddleCenter };

    var lblText = new Label { Text = item.Text, Font = new Font("Segoe UI", 9.5F),
        ForeColor = Color.FromArgb(44, 52, 70), Location = new Point(40, 8),
        Size = new Size(120, 20), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };

    pnl.Controls.Add(pnlAccent);
    pnl.Controls.Add(lblIcon);
    pnl.Controls.Add(lblText);

    // Badge (ถ้ามี) — rounded via Paint event
    if (item.Badge > 0)
    {
        var badgeColor = item.BadgeColor;
        var lblBadge = new Label
        {
            Text = item.Badge.ToString(), Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = Color.White, BackColor = badgeColor,
            Location = new Point(164, 10),
            Size = new Size(item.Badge >= 10 ? 26 : 20, 16),
            TextAlign = ContentAlignment.MiddleCenter,
        };
        lblBadge.Paint += (s, pe) =>
        {
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, lblBadge.Width - 1, lblBadge.Height - 1);
            using (var b = new SolidBrush(badgeColor))
            using (var path = RoundedRect(r, 8))
                pe.Graphics.FillPath(b, path);
            pe.Graphics.DrawString(lblBadge.Text, lblBadge.Font, Brushes.White,
                new RectangleF(0, 0, lblBadge.Width, lblBadge.Height),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        };
        pnl.Controls.Add(lblBadge);
    }

    // Hover / Click
    var tag = item.Tag;
    EventHandler enterH = (s, e) => { if (pnl != _activeNavItem) SetNavState(pnl, _navHover, false); };
    EventHandler leaveH = (s, e) => { if (pnl != _activeNavItem) SetNavState(pnl, Color.White, false); };
    EventHandler clickH = (s, e) => { SetActiveNav(pnl); OnNavItemClick(tag); };
    foreach (Control c in pnl.Controls)
    { c.MouseEnter += enterH; c.MouseLeave += leaveH; c.Click += clickH; }
    pnl.MouseEnter += enterH; pnl.MouseLeave += leaveH; pnl.Click += clickH;

    return pnl;
}
```

**SetActiveNav:**
```csharp
private void SetActiveNav(Panel pnl)
{
    // ปิด active เดิม
    if (_activeNavItem != null && _activeNavItem != pnl)
        SetNavState(_activeNavItem, Color.White, false);

    _activeNavItem = pnl;
    pnl.BackColor = _navActive;
    foreach (Control c in pnl.Controls)
    {
        if (c.Name == "accent") { c.BackColor = _navyDark; continue; }
        c.BackColor = _navActive;
        if (c is Label lbl)
        {
            lbl.ForeColor = _navyDark;
            if (lbl.Text.Length > 2) // text label (ไม่ใช่ icon/badge)
                lbl.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }
    }
}

private void SetNavState(Panel pnl, Color bg, bool active)
{
    pnl.BackColor = bg;
    foreach (Control c in pnl.Controls)
    {
        if (c.Name == "accent") { c.BackColor = active ? _navyDark : Color.Transparent; continue; }
        c.BackColor = bg;
        if (c is Label lbl) lbl.ForeColor = Color.FromArgb(44, 52, 70);
    }
}
```

---

## 4. Page Header

```csharp
// pnlBreadcrumb: Dock=Top, Height=28, BackColor=#F3F6FB, Padding=(20,0,16,0)
// lblBreadcrumb: Segoe UI 8.5pt, ForeColor=#646E82, Dock=Fill, TextAlign=MiddleLeft
// Text: "Core Management System  ›  [Module]  ›  [Page]"

// pnlTitleBar: Dock=Top, Height=62, BackColor=White, Padding=(20,0,16,0)
// lblTitle:    Segoe UI 15pt Bold, ForeColor=#16243E, AutoSize=true, Y=6
// lblSubtitle: Segoe UI 8.5pt,     ForeColor=#646E82, AutoSize=true, Y=36
// btnAction:   Anchor=Top|Right, FlatStyle, BorderColor=#D2D7E1, Size=(102,26)
//              ForeColor=#2C3E64, emoji prefix (เช่น "🗓  สัปดาห์นี้" หรือ "+ สร้างใหม่")
```

---

## 5. Summary Cards — FlowLayoutPanel

```csharp
// flpCards: Dock=Top, Height=98, BackColor=White, Padding=(0,0,0,8)
// FlowDirection=LeftToRight, WrapContents=false

// แต่ละ card Panel: BorderStyle.FixedSingle, Size(252-328, 82-88), Padding=(14,10,14,8)
// Margin=(0,0,10,0)

// layout ใน card:
//   lblIcon:  Segoe UI 14pt emoji, AutoSize=true, X=14, Y=8, ForeColor=colored
//   lblTitle: Segoe UI 9.5pt,      AutoSize=true, X=40, Y=12, ForeColor=#505A6E
//   lblCount: Segoe UI 22-26pt Bold, AutoSize=true, X=14, Y=28-30, ForeColor=colored
//   lblSub:   Segoe UI 8.5pt,      AutoSize=true, X=14, Y=66, ForeColor=#8C96A5 (หรือ colored)

// ตัวอย่าง card สี:
// กำลังทำงาน: #1964C3 (Blue)  icon="▶"
// เกิน SLA:   #C22636 (Red)   icon="⚠"
// เสร็จวันนี้: #178531 (Green) icon="✔"
// รอดำเนินการ: #C57810 (Amber) icon="⏰"
```

---

## 6. Filter Bar

```csharp
// pnlFilterBar: Dock=Top, Height=44, BackColor=White, Padding=(12,8,12,8)

// ComboBox (DropDownList, FlatStyle=Flat, Segoe UI 9pt):
//   cmbFilterStatus: Items = { "ทั้งหมด", "เกิน SLA", "รอดำเนินการ", "รอ", "เสร็จ" }
//   cmbFilterType:   Items = ตามประเภท entity ของ form

// txtSearch: BorderStyle.FixedSingle, Segoe UI 9pt, ForeColor=#8C96A5, Text="ค้นหา..."

// btnClearFilter: Anchor=Top|Right, FlatStyle, BorderColor=#D2D7E1, ForeColor=#505A6E
//                Text = "⏷  ล้างตัวกรอง", Size=(136,26)
```

---

## 7. ListView (OwnerDraw)

```csharp
// lvTasks: OwnerDraw=true, View=Details, BorderStyle=None, Dock=Fill
// FullRowSelect=true, GridLines=false, HideSelection=false, MultiSelect=true
// Font: Segoe UI 9.5pt

// Row height 46px via ImageList trick:
var img = new ImageList { ImageSize = new Size(1, 46) };
lvTasks.SmallImageList = img;

// Events:
lvTasks.DrawColumnHeader += LvTasks_DrawColumnHeader;
lvTasks.DrawItem         += LvTasks_DrawItem;
lvTasks.DrawSubItem      += LvTasks_DrawSubItem;
```

**Column Header:**
```csharp
private void LvTasks_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
{
    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(246, 248, 251)), e.Bounds);
    e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 224, 232)),
        e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

    if (e.ColumnIndex == 0) return; // checkbox column
    using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
    using (var f = new Font("Segoe UI Semibold", 9F, FontStyle.Bold))
    {
        var r = new RectangleF(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height);
        e.Graphics.DrawString(e.Header.Text, f, new SolidBrush(Color.FromArgb(55, 65, 90)), r, sf);
    }
}
```

**Row Item:**
```csharp
private void LvTasks_DrawItem(object sender, DrawListViewItemEventArgs e)
{
    bool sel = e.Item.Selected;
    var bg = sel ? Color.FromArgb(232, 241, 255)
                 : (e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(250, 251, 253));
    e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
    e.Graphics.DrawLine(new Pen(Color.FromArgb(235, 238, 244)),
        e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
}
```

**DrawSubItem per column type:**
```csharp
// col 0: DrawCheckBox — 14px box, navy fill + ✓ when selected
// col 1: WF ID       — Segoe UI 9pt, ForeColor=#005AAA (blue link)
// col 2: Title+Step  — 2 lines: 9.5pt Bold #1E2437 / 8.5pt #6E7880 (prefix "↳ ")
// col 3: Assignee    — 28px avatar circle + name (9pt #1E2437) + role (8pt #6E7880)
// col 4: Due date    — date text + relative "เหลือ/เกิน X วัน" (colored)
// col 5: SLA bar     — 8px bar, green/orange/red by SlaPercent
// col 6: Status pill — semi-transparent badge (see DrawStatusBadge)
```

---

## 8. Status Badge — DrawStatusBadge()

```csharp
// สี status:
private static readonly Dictionary<string, Color> StatusColors = new Dictionary<string, Color>
{
    { "เกิน SLA",    Color.FromArgb(194, 38,  54)  },
    { "รอดำเนินการ", Color.FromArgb(197, 120, 16)  },
    { "รอ",          Color.FromArgb(80,  130, 200)  },
    { "เสร็จ",       Color.FromArgb(23,  133, 49)  },
};

private void DrawStatusBadge(Graphics g, Rectangle r, string status)
{
    Color c;
    if (!StatusColors.TryGetValue(status, out c)) c = Color.FromArgb(120, 130, 150);

    var sz = g.MeasureString(status, new Font("Segoe UI", 8.5F, FontStyle.Bold));
    int bw = (int)sz.Width + 18;
    int bh = 20;
    var br = new Rectangle(r.X + 10, r.Y + (r.Height - bh) / 2, bw, bh);

    using (var bg = new SolidBrush(Color.FromArgb(30, c)))   // semi-transparent fill
    using (var path = RoundedRect(br, 4))
        g.FillPath(bg, path);

    using (var pen = new Pen(Color.FromArgb(80, c)))          // semi-transparent border
    using (var path = RoundedRect(br, 4))
        g.DrawPath(pen, path);

    g.DrawString(status, new Font("Segoe UI", 8.5F, FontStyle.Bold), new SolidBrush(c),
        new RectangleF(br.X, br.Y, br.Width, br.Height),
        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
}
```

---

## 9. Avatar Pattern

```csharp
// ขนาด 28px, วาดเป็นวงกลม (RoundedRect radius=14), text = GetInitials(name)
private static readonly Color[] AvatarColors =
{
    Color.FromArgb(0,  102, 180), Color.FromArgb(155, 89,  182),
    Color.FromArgb(22, 160, 133), Color.FromArgb(192, 57,  43),
    Color.FromArgb(41, 128, 185), Color.FromArgb(230, 126, 34),
};

// เลือกสีตาม: AvatarColors[Math.Abs(name.GetHashCode()) % AvatarColors.Length]
// Initials: 2 ตัว (ชื่อ+นามสกุล) หรือ 2 ตัวแรกของชื่อ
```

---

## 10. SLA Bar

```csharp
private void DrawSlaCell(Graphics g, Rectangle r, WorkflowTaskItem task)
{
    int barW = r.Width - 20, barH = 8;
    int barX = r.X + 10, barY = r.Y + (r.Height - barH) / 2 - 6;
    int fill = (int)(barW * Math.Min(1.0, task.SlaPercent / 100.0));

    g.FillRectangle(new SolidBrush(Color.FromArgb(228, 232, 240)), barX, barY, barW, barH);

    Color barColor = task.SlaPercent >= 100 ? Color.FromArgb(194, 38, 54)  // red
                   : task.SlaPercent >= 70  ? Color.FromArgb(197, 120, 16) // orange
                   :                          Color.FromArgb(23, 133, 49);  // green
    g.FillRectangle(new SolidBrush(barColor), barX, barY, fill, barH);

    string dayText = task.DaysLeft < 0
        ? string.Format("-{0} วัน", -task.DaysLeft)
        : string.Format("{0} วัน", task.DaysLeft);
    using (var f = new Font("Segoe UI", 8F))
        g.DrawString(dayText, f, new SolidBrush(barColor),
            new RectangleF(barX, barY + barH + 4, barW, 14),
            new StringFormat { Alignment = StringAlignment.Far });
}
```

---

## 11. Pagination Footer

```csharp
// pnlPagination: Dock=Bottom, Height=44, BackColor=White, Padding=(12,8,12,8)
// lblSelectionInfo: Segoe UI 8.5pt, ForeColor=#6E7880, X=12
//   Text: "เลือก {N} รายการจาก {Total} รายการ" หรือ "แสดง {Total} รายการ"

// btnPrev: Anchor=Top|Right, FlatStyle, BorderSize=0, Text="←  ก่อนหน้า"
// lblPage: Anchor=Top|Right, Segoe UI 9pt, TextAlign=MiddleCenter, Text="หน้า X / Y"
// btnNext: Anchor=Top|Right, FlatStyle, BorderSize=0, Text="ถัดไป  →"
// ForeColor ทั้งหมด: #2C3E64
```

---

## 12. Helpers (ต้องมีใน .cs ทุก form)

```csharp
private static RectangleF PadRect(Rectangle r, int hPad, int vPad)
    => new RectangleF(r.X + hPad, r.Y + vPad, r.Width - hPad * 2, r.Height - vPad * 2);

private static GraphicsPath RoundedRect(Rectangle r, int radius)
{
    var path = new GraphicsPath();
    int d = radius * 2;
    path.AddArc(r.X, r.Y, d, d, 180, 90);
    path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
    path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
    path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
    path.CloseFigure();
    return path;
}

private static string GetInitials(string name)
{
    if (string.IsNullOrWhiteSpace(name)) return "?";
    var parts = name.Trim().Split(' ');
    return parts.Length >= 2
        ? parts[0].Substring(0, 1) + parts[1].Substring(0, 1)
        : name.Substring(0, Math.Min(2, name.Length));
}
```

---

## 13. Color Palette Reference

| ชื่อ | Hex | RGB |
|------|-----|-----|
| Navy Dark (Ribbon / Accent) | #1F3864 | 31, 56, 100 |
| Navy Mid (Hover BG on ribbon) | #32569A | 50, 86, 154 |
| Navy Light | #D7E4F8 | 215, 228, 248 |
| Nav Active BG | #E8F0FF | 232, 240, 255 |
| Nav Hover BG | #F5F9FF | 245, 249, 255 |
| Body BG | #F3F6FB | 243, 246, 251 |
| Title Text | #16243E | 22, 36, 62 |
| Subtitle / Breadcrumb | #646E82 | 100, 110, 130 |
| Text Primary | #1E2437 | 30, 36, 55 |
| Text Secondary | #37415A | 55, 65, 90 |
| Text Muted | #6E7880 | 110, 120, 130 |
| Border / Separator | #DCE1EB | 220, 225, 235 |
| Table Header BG | #F6F8FB | 246, 248, 251 |
| Row Selected | #E8F1FF | 232, 241, 255 |
| Row Alt | #FAFBFD | 250, 251, 253 |
| Orange Badge | #E67300 | 230, 115, 0 |
| Green SLA | #178531 | 23, 133, 49 |
| Amber SLA | #C57810 | 197, 120, 16 |
| Red SLA / Error | #C22636 | 194, 38, 54 |

---

## 14. Sidebar Nav Icons (emoji ใน Segoe UI 11pt)

| Section | Nav Item | Emoji |
|---------|---------|-------|
| การทำงาน | งานของฉัน | 🏠 |
| การทำงาน | ตรวจสอบ Workflow | ✓ |
| การทำงาน | เริ่ม Workflow | + |
| การทำงาน | ประวัติ Workflow | 📋 |
| จัดการ | จัดการ Template | 📄 |
| จัดการ | Workflow Designer | 🎨 |
| จัดการ | อนุมัติ Template | ☑ |
| ตั้งค่า | Delegation | 👥 |
| ตั้งค่า | รายงาน SLA | 📊 |
| ตั้งค่า | ตั้งค่า | ⚙ |

---

## 15. MVP Pattern — ไฟล์ที่ต้องสร้างทุกครั้ง

### MDI Child Form (Full WFE Form)
```
CMS.Presentation/Modules/WFE/
├── {Name}Form.cs              ← Form (partial) + IView + Build* methods
├── {Name}Form.Designer.cs     ← เฉพาะ container panels ที่ไม่มี local variables
├── {Name}Form.resx            ← ต้องสร้างเสมอ (แม้ว่าจะว่างเปล่า)
└── Presenters/
    ├── {Name}Presenter.cs     ← Business logic
    └── Interfaces/
        └── I{Name}View.cs     ← View interface
```

### Dialog Form (ShowDialog)
```
CMS.Presentation/Modules/WFE/Dialogs/
├── {Name}Dialog.cs            ← Form (partial) + logic
├── {Name}Dialog.Designer.cs   ← container panels
└── {Name}Dialog.resx          ← ต้องสร้างเสมอ
```

**View Interface ต้อง implement:**
- `void Display{Entities}(PagedResult<{Entity}Result> data)`
- `void SetActionButtonsEnabled(bool canEdit, bool canDelete, ...)`
- `void ShowError(string message)` / `ShowInfo(string message)`
- `string GetSearchFilter()` / `string GetStatusFilter()`
- `string GetCategoryFilter()`

---

## 16. ไฟล์ .resx — กฎและ Pattern

### ทำไมต้องมี .resx
- WinForms ทุก Form ที่มี `InitializeComponent()` ต้องมี `.resx` คู่กัน
- ถ้าไม่มี → `MissingManifestResourceException` ตอน runtime
- แม้ว่า form จะไม่มี embedded image/icon ก็ต้องสร้างไฟล์ว่างเปล่า

### เนื้อหา .resx ขั้นต่ำ (สำหรับ form ที่ไม่มี embedded resources)
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
              xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1"/>
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string"/>
              <xsd:attribute name="type" type="xsd:string"/>
              <xsd:attribute name="mimetype" type="xsd:string"/>
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
</root>
```

### ตรวจสอบ .resx ที่มีอยู่ใน WFE
```
WorkflowDesignerForm.resx       ✅
WorkflowDetailFormNew.resx      ✅
WorkflowInstanceDetailForm.resx ✅
WorkflowTemplateListForm.resx   ✅
```

### Forms ที่ยังขาด .resx (ต้องสร้าง)
```
WorkflowDashboardFormNew.resx   ← ต้องสร้าง
WorkflowDashboardForm.resx      ← ต้องสร้าง
WorkflowMonitorForm.resx        ← ต้องสร้าง
WorkflowDelegationForm.resx     ← ต้องสร้าง
SLAReportForm.resx              ← ต้องสร้าง
HistorySearchForm.resx          ← ต้องสร้าง
TemplateApprovalForm.resx       ← ต้องสร้าง
WorkflowVisualFlowForm.resx     ← ต้องสร้าง
Dialogs/WorkflowStartDialog.resx      ← ต้องสร้าง
Dialogs/WorkflowStepActionForm.resx   ← ต้องสร้าง
Dialogs/ReassignDialog.resx           ← ต้องสร้าง
Dialogs/ApprovalReviewForm.resx       ← ต้องสร้าง
Dialogs/DelegationDialog.resx         ← ต้องสร้าง
```

---

## 17. csproj Registration — ทุกไฟล์ต้อง register

### Pattern ใน CMS.Presentation.csproj

```xml
<!-- Form .cs + Designer.cs -->
<Compile Include="Modules\WFE\{Name}Form.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\WFE\{Name}Form.Designer.cs">
  <DependentUpon>{Name}Form.cs</DependentUpon>
</Compile>

<!-- Presenter + Interface -->
<Compile Include="Modules\WFE\Presenters\{Name}Presenter.cs" />
<Compile Include="Modules\WFE\Presenters\Interfaces\I{Name}View.cs" />

<!-- .resx — ต้องอยู่ใน EmbeddedResource section -->
<EmbeddedResource Include="Modules\WFE\{Name}Form.resx">
  <DependentUpon>{Name}Form.cs</DependentUpon>
</EmbeddedResource>
```

### Dialog pattern
```xml
<Compile Include="Modules\WFE\Dialogs\{Name}Dialog.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\WFE\Dialogs\{Name}Dialog.Designer.cs">
  <DependentUpon>{Name}Dialog.cs</DependentUpon>
</Compile>
<EmbeddedResource Include="Modules\WFE\Dialogs\{Name}Dialog.resx">
  <DependentUpon>{Name}Dialog.cs</DependentUpon>
</EmbeddedResource>
```

---

## 18. Designer.cs — กฎสำคัญ (ห้ามละเมิด)

### ❌ ห้ามใช้ใน InitializeComponent()
```csharp
// ❌ WRONG — Designer ไม่รองรับ local variables
const int PX = 14;
int py = 10;
this.lblName.Location = new Point(PX, py);
py += 16;
```

### ✅ ต้องใช้ค่าตัวเลขตรงๆ เท่านั้น
```csharp
// ✅ CORRECT — hardcoded values เท่านั้น
this.lblName.Location = new Point(14, 10);
this.txtName.Location = new Point(14, 26);
this.lblAssignee.Location = new Point(14, 56);
```

### กฎอื่นๆ ของ Designer.cs
- ใส่เฉพาะ **container panels** (pnlRibbonTabs, pnlToolbarContainer, pnlAppBody ฯลฯ)
- **ห้าม** ใส่ nav items, toolbar buttons, dynamic controls
- **ห้าม** เรียก method ใดๆ นอกจาก `new`, property assignment, `Controls.Add`
- ทุก control ต้องมี `.Name` property
- `numericUpDown` ต้องมี `BeginInit()` / `EndInit()`

---

## 19. Dialog Pattern (WorkflowStartDialog / WorkflowStepActionForm / ReassignDialog)

### โครงสร้าง Dialog
```
Form (FormBorderStyle=None, StartPosition=CenterParent)
├── pnlTitleBar    (Dock=Top, Height=52-56, BackColor=#16243E navy)
│   ├── lblTitleIcon  (colored badge, 32×32)
│   ├── lblTitleText  (Segoe UI 11pt Bold, White)
│   ├── lblTitleSub   (Segoe UI 8.5pt, #B4C8EB)
│   └── btnClose      (×, FlatStyle, Transparent)
├── pnlScrollOuter (Dock=Fill, AutoScroll=true, BackColor=#F8F9FC)
│   └── pnlScrollContent (AutoSize=true, Padding=(16,12,16,12))
│       └── [sections ต่างๆ]
└── pnlFooter      (Dock=Bottom, Height=56, BackColor=White)
    ├── btnCancel  (FlatStyle, BorderColor=#D2D7E1)
    └── btnConfirm (FlatStyle, BackColor=#2563EB navy, ForeColor=White)
```

### Footer button sizes
```
btnCancel:  Size=(88, 32),  Anchor=Top|Right
btnConfirm: Size=(140-188, 32), Anchor=Top|Right  ← ขนาดตามความยาวข้อความ
```

### Scrollable content
```csharp
// pnlScrollContent ต้องปรับ Width ตาม outer panel
pnlScrollContent.Width = pnlScrollOuter.ClientSize.Width - 2;
pnlScrollOuter.Resize += (s, ev) =>
    pnlScrollContent.Width = pnlScrollOuter.ClientSize.Width - 2;
```

---

## 20. Visual Flow Canvas Pattern

### Layout (WorkflowVisualFlowForm)
```
pnlDesignerBody (Dock=Fill)
├── pnlRight       (Dock=Right, Width=260, BackColor=White)  ← Step info panel
├── pnlRightBorder (Dock=Right, Width=1)
└── pnlCanvasOuter (Dock=Fill)
    ├── pnlLegendBar   (Dock=Top, Height=32)   ← Legend chips
    ├── pnlCanvas      (Dock=Fill)              ← GDI+ paint area
    └── pnlCanvasHeader (Dock=Bottom, Height=32) ← Zoom controls
```

### Legend bar chips
```csharp
// สี node ตาม status:
// เริ่มแล้ว  = #22994A (Green)
// กำลังทำ   = #E67300 (Orange) — active step
// รอ         = #6B7280 (Gray)
// ข้าม       = #9CA3AF (Light gray, dashed border)
```

### Node drawing (GDI+)
```csharp
// Rounded rect radius=8, shadow offset=(3,3) alpha=30
// Top accent strip height=5 (step type color)
// Selection glow: Pen width=4, alpha=80, color=AccentBlue
// Step name: Segoe UI 8pt Bold, center
// Sub-label: Segoe UI 7pt, assignee or step type
```

### Right panel sections
```
Step ปัจจุบัน  → lblStepBadge "● Active"
Manager อนุมัติ → step name + "Step N จาก N"
Assignee card  → avatar + name + role
กำหนด          → date/time
เหลือ           → "● N วัน" badge (amber/red)
SLA Used        → progress bar + %
PATH ที่วิ่ง    → text description
ACTIONS         → ปุ่ม "ดำเนินการ Step นี้" (navy) + Reassign + แสดงความเห็น
```

---

## 21. Checklist ก่อน Implement Form ใหม่ (อัปเดต)

### ขั้นตอนที่ 1 — วางแผน
- [ ] วิเคราะห์ภาพ UI ให้ครบ (title bar, sections, footer)
- [ ] ระบุ type: MDI Child Form / Dialog / Visual Canvas
- [ ] ระบุ services ที่ต้องการ (IWorkflowTemplateService ฯลฯ)

### ขั้นตอนที่ 2 — สร้างไฟล์ (ตามลำดับ)
- [ ] `I{Name}View.cs` — view interface
- [ ] `{Name}Presenter.cs` — business logic
- [ ] `{Name}Form.Designer.cs` — container panels เท่านั้น, **ไม่มี local variables**
- [ ] `{Name}Form.cs` — Form + IView + Build* methods
- [ ] **`{Name}Form.resx`** — สร้างเสมอ แม้ว่าจะว่างเปล่า

### ขั้นตอนที่ 3 — Register ใน csproj
- [ ] `<Compile>` สำหรับ .cs ทุกไฟล์
- [ ] `<EmbeddedResource>` สำหรับ .resx ทุกไฟล์
- [ ] ตรวจสอบ `<DependentUpon>` ถูกต้อง

### ขั้นตอนที่ 4 — Implement
- [ ] `Form_Load`: เรียก `BuildRibbonTabs()`, `BuildToolbar()`, `BuildSidebar()`
- [ ] `BuildRibbonTabs()`: active tab = white bold + 3px white underline Paint
- [ ] `BuildToolbar()`: emoji icon 16pt + text 7.5pt + group label + separator
- [ ] `BuildSidebar()`: ใช้ `WorkflowNavigationBuilder.BuildSidebar()` เสมอ
- [ ] Summary cards ใน FlowLayoutPanel (ถ้าเป็น dashboard)
- [ ] ListView OwnerDraw: row 46px via ImageList
- [ ] DrawStatusBadge: semi-transparent fill(30) + border(80) + radius=4
- [ ] DrawAssigneeCell: 28px avatar + name/role 2 lines
- [ ] DrawSlaCell: 8px bar green/orange/red
- [ ] helpers: `RoundedRect`, `PadRect`, `GetInitials`
- [ ] Audit stamp: ADUserName (ไม่ใช่ UserID)
- [ ] `OnFormClosed`: dispose Timer ถ้ามี

### ขั้นตอนที่ 5 — ตรวจสอบก่อน Compile
- [ ] ไม่มี `const int`, `int py` ใน `InitializeComponent()`
- [ ] ทุก control มี `.Name` property
- [ ] .resx ครบทุก form
- [ ] csproj มี `<EmbeddedResource>` ครบ
- [ ] Compile ผ่านโดยไม่มี `error CS`

### ขั้นตอนที่ 6 — Action Key (ถ้าเปิดจาก menu tree)
- [ ] เพิ่ม `case "{ActionKey}"` ใน `DMS/MenuActions/MenuActionRegistry.cs`
- [ ] เพิ่ม `"{ActionKey}"` ใน `DMS/MenuActions/MenuActionKeyCatalog.cs` BuiltInActionKeys
- [ ] เพิ่ม alias ใน `ResolveConfiguredActionKey()` (เช่น `"workflowdesignerform"` → `"WorkflowDesigner"`)
- [ ] เพิ่ม `case "{actionkey}"` ใน `DMS/FolderForm.cs` ทั้ง `MapConfiguredActionToTag` และ `MapMenuKeyToActionTag`
- [ ] เพิ่ม `case "{ActionKey}"` ใน `MDI_Container.cs` `HandleFolderMenuActionRequested`
- [ ] เพิ่ม `Open{Name}()` method ใน `MDI_Container.cs`
- [ ] ส่ง service เข้า MDI_Container constructor ถ้าจำเป็น
- [ ] เพิ่ม SQL script `NNN_Seed_MenuItem_{Name}.sql` ใน `CMS.Data/Scripts/`
