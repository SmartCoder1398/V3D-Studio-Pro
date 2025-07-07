Public Class RenderSetupDialog
    Inherits Form

    Private tabControl As TabControl
    Private widthNumeric As NumericUpDown
    Private heightNumeric As NumericUpDown
    Private qualityCombo As ComboBox
    Private antialiasingCheck As CheckBox

    Public Property RenderWidth As Integer = 1920
    Public Property RenderHeight As Integer = 1080
    Public Property RenderQuality As String = "High"
    Public Property UseAntialiasing As Boolean = True

    Public Sub New()
        Me.Text = "Render Setup"
        Me.Size = New Size(600, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        tabControl = New TabControl With {
            .Dock = DockStyle.Fill,
            .Location = New Point(0, 0)
        }

        ' Common tab
        Dim commonTab As New TabPage("Common")
        Dim commonPanel As New Panel With {.Dock = DockStyle.Fill, .AutoScroll = True}

        ' Output Size group
        Dim sizeGroup As New GroupBox With {
            .Text = "Output Size",
            .Location = New Point(20, 20),
            .Size = New Size(540, 120)
        }

        Dim lblWidth As New Label With {.Text = "Width:", .Location = New Point(20, 30), .AutoSize = True}
        widthNumeric = New NumericUpDown With {
            .Location = New Point(80, 28),
            .Width = 100,
            .Minimum = 100,
            .Maximum = 8192,
            .Value = 1920
        }

        Dim lblHeight As New Label With {.Text = "Height:", .Location = New Point(200, 30), .AutoSize = True}
        heightNumeric = New NumericUpDown With {
            .Location = New Point(260, 28),
            .Width = 100,
            .Minimum = 100,
            .Maximum = 8192,
            .Value = 1080
        }

        ' Preset buttons
        Dim btn720p As New Button With {.Text = "720p", .Location = New Point(20, 70), .Width = 60}
        Dim btn1080p As New Button With {.Text = "1080p", .Location = New Point(90, 70), .Width = 60}
        Dim btn4K As New Button With {.Text = "4K", .Location = New Point(160, 70), .Width = 60}

        sizeGroup.Controls.AddRange({lblWidth, widthNumeric, lblHeight, heightNumeric, btn720p, btn1080p, btn4K})

        ' Quality group
        Dim qualityGroup As New GroupBox With {
            .Text = "Render Quality",
            .Location = New Point(20, 150),
            .Size = New Size(540, 100)
        }

        Dim lblQuality As New Label With {.Text = "Quality:", .Location = New Point(20, 30), .AutoSize = True}
        qualityCombo = New ComboBox With {
            .Location = New Point(80, 28),
            .Width = 150,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        qualityCombo.Items.AddRange({"Draft", "Low", "Medium", "High", "Production"})
        qualityCombo.SelectedItem = "High"

        antialiasingCheck = New CheckBox With {
            .Text = "Enable Antialiasing",
            .Location = New Point(20, 60),
            .AutoSize = True,
            .Checked = True
        }

        qualityGroup.Controls.AddRange({lblQuality, qualityCombo, antialiasingCheck})

        commonPanel.Controls.AddRange({sizeGroup, qualityGroup})
        commonTab.Controls.Add(commonPanel)

        ' Renderer tab
        Dim rendererTab As New TabPage("Renderer")
        ' Add renderer-specific settings here

        ' GI tab (Global Illumination)
        Dim giTab As New TabPage("Global Illumination")
        ' Add GI settings here

        tabControl.TabPages.AddRange({commonTab, rendererTab, giTab})

        ' Dialog buttons
        Dim buttonPanel As New Panel With {.Height = 50, .Dock = DockStyle.Bottom}
        Dim btnRender As New Button With {
            .Text = "Render",
            .Location = New Point(340, 10),
            .Width = 80,
            .Height = 30
        }
        Dim btnOK As New Button With {
            .Text = "OK",
            .Location = New Point(425, 10),
            .Width = 75,
            .DialogResult = DialogResult.OK
        }
        Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New Point(505, 10),
            .Width = 75,
            .DialogResult = DialogResult.Cancel
        }

        buttonPanel.Controls.AddRange({btnRender, btnOK, btnCancel})

        Me.Controls.Add(tabControl)
        Me.Controls.Add(buttonPanel)

        ' Event handlers
        AddHandler btn720p.Click, Sub() SetResolution(1280, 720)
        AddHandler btn1080p.Click, Sub() SetResolution(1920, 1080)
        AddHandler btn4K.Click, Sub() SetResolution(3840, 2160)
        AddHandler btnRender.Click, AddressOf StartRender
        AddHandler btnOK.Click, AddressOf SaveSettings
    End Sub

    Private Sub SetResolution(width As Integer, height As Integer)
        widthNumeric.Value = width
        heightNumeric.Value = height
    End Sub

    Private Sub StartRender(sender As Object, e As EventArgs)
        SaveSettings(sender, e)
        MessageBox.Show($"Rendering at {RenderWidth}x{RenderHeight} with {RenderQuality} quality...", "Render", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub SaveSettings(sender As Object, e As EventArgs)
        RenderWidth = CInt(widthNumeric.Value)
        RenderHeight = CInt(heightNumeric.Value)
        RenderQuality = qualityCombo.SelectedItem.ToString()
        UseAntialiasing = antialiasingCheck.Checked
    End Sub
End Class