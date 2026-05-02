---
inclusion: manual
---

# WFE Form — Checklist สร้าง/ตรวจสอบ (Quick Reference)

ใช้ไฟล์นี้เป็น checklist ด่วนเมื่อสร้าง WFE form ใหม่
สำหรับรายละเอียดเต็ม ดู `.claude/commands/wfe-form-ui.md`

---

## ไฟล์ที่ต้องสร้างทุกครั้ง

### MDI Child Form
```
✅ {Name}Form.cs
✅ {Name}Form.Designer.cs
✅ {Name}Form.resx            ← ห้ามลืม!
✅ Presenters/{Name}Presenter.cs
✅ Presenters/Interfaces/I{Name}View.cs
```

### Dialog Form
```
✅ Dialogs/{Name}Dialog.cs
✅ Dialogs/{Name}Dialog.Designer.cs
✅ Dialogs/{Name}Dialog.resx  ← ห้ามลืม!
```

---

## csproj — ต้อง register ทุกไฟล์

```xml
<!-- .cs files -->
<Compile Include="Modules\WFE\{Name}Form.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="Modules\WFE\{Name}Form.Designer.cs">
  <DependentUpon>{Name}Form.cs</DependentUpon>
</Compile>
<Compile Include="Modules\WFE\Presenters\{Name}Presenter.cs" />
<Compile Include="Modules\WFE\Presenters\Interfaces\I{Name}View.cs" />

<!-- .resx — ต้องอยู่ใน EmbeddedResource -->
<EmbeddedResource Include="Modules\WFE\{Name}Form.resx">
  <DependentUpon>{Name}Form.cs</DependentUpon>
</EmbeddedResource>
```

---

## .resx ขั้นต่ำ (copy ใช้ได้เลย)

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
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
</root>
```

---

## Designer.cs — ห้ามทำ

```csharp
// ❌ WRONG — ทำให้ Designer crash
const int PX = 14;
int py = 10;
this.lblName.Location = new Point(PX, py);
py += 16;

// ✅ CORRECT
this.lblName.Location = new Point(14, 10);
this.txtName.Location = new Point(14, 26);
```

---

## Action Key — 6 ไฟล์ที่ต้องแก้

เมื่อเพิ่มเมนูใหม่ใน WKE tree ต้องแก้ทุกไฟล์นี้:

| ไฟล์ | สิ่งที่เพิ่ม |
|------|------------|
| `DMS/MenuActions/MenuActionRegistry.cs` | `case "{ActionKey}"` → `TryRequestMdiAction` |
| `DMS/MenuActions/MenuActionKeyCatalog.cs` | เพิ่มใน `BuiltInActionKeys[]` + alias ใน `ResolveConfiguredActionKey` |
| `DMS/FolderForm.cs` | เพิ่มใน `MapConfiguredActionToTag` + `MapMenuKeyToActionTag` |
| `MDI_Container.cs` | `case "{ActionKey}"` ใน `HandleFolderMenuActionRequested` + `Open{Name}()` method |
| `Program.cs` | ส่ง service เข้า MDI_Container constructor (ถ้าจำเป็น) |
| `CMS.Data/Scripts/NNN_Seed_MenuItem_{Name}.sql` | INSERT เมนูเข้า `core.MenuItem` |

---

## Compile Check

```bash
# Build เฉพาะ compile (ไม่ต้อง link)
msbuild CMS.Presentation\CMS.Presentation.csproj /p:Configuration=Debug /nologo /v:minimal /t:Compile

# ต้องไม่มี error CS ใดๆ
# warning CS ที่มีอยู่เดิมไม่นับ
```

---

## WFE Forms ที่มีอยู่แล้ว (อย่าสร้างซ้ำ)

| Form | .resx | csproj |
|------|-------|--------|
| WorkflowDashboardFormNew | ❌ ขาด | ✅ |
| WorkflowDashboardForm | ❌ ขาด | ✅ |
| WorkflowDesignerForm | ✅ | ✅ |
| WorkflowDetailFormNew | ✅ | ✅ |
| WorkflowInstanceDetailForm | ✅ | ✅ |
| WorkflowMonitorForm | ❌ ขาด | ✅ |
| WorkflowTemplateListForm | ✅ | ✅ |
| TemplateApprovalForm | ❌ ขาด | ✅ |
| SLAReportForm | ❌ ขาด | ✅ |
| HistorySearchForm | ❌ ขาด | ✅ |
| WorkflowDelegationForm | ❌ ขาด | ✅ |
| WorkflowVisualFlowForm | ❌ ขาด | ❌ ขาด |
| Dialogs/WorkflowStartDialog | ❌ ขาด | ✅ |
| Dialogs/WorkflowStepActionForm | ❌ ขาด | ✅ |
| Dialogs/ReassignDialog | ❌ ขาด | ❌ ขาด |
| Dialogs/ApprovalReviewForm | ❌ ขาด | ✅ |
| Dialogs/DelegationDialog | ❌ ขาด | ✅ |
