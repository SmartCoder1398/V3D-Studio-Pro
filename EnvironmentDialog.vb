Imports System.Drawing
Imports System.Windows.Forms

Public Class EnvironmentDialog
    Inherits Form

    Private backgroundColorButton As Button
    Private ambientColorButton As Button
    Private fogCheckBox As CheckBox
    Private fogColorButton As Button
    Private fogNearNumeric As NumericUpDown
    Private fogFarNumeric As NumericUpDown

    Public Sub New()
        Me.Text = "Environment and Effects"
        Me.Size = New Size(500, 600)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        Dim tabControl As New TabControl With {
            .Dock = DockStyle.Fill
        }

        ' Environment tab
        Dim envTab As New TabPage("Environment")

        Dim lblBackground As New Label With {
            .Text = "Background Color:",
            .Location = New System.Drawing.Point(20, 20),
            .AutoSize = True
        }

        backgroundColorButton = New Button With {
            .Location = New System.Drawing.Point(130, 15),
            .Size = New Size(80, 25),
            .BackColor = System.Drawing.Color.Black,
            .Text = "Choose..."
        }

        Dim lblAmbient As New Label With {
            .Text = "Ambient Color:",
            .Location = New System.Drawing.Point(20, 60),
            .AutoSize = True
        }

        ambientColorButton = New Button With {
            .Location = New System.Drawing.Point(130, 55),
            .Size = New Size(80, 25),
            .BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
            .Text = "Choose..."
        }

        envTab.Controls.AddRange({lblBackground, backgroundColorButton, lblAmbient, ambientColorButton})

        ' Effects tab
        Dim effectsTab As New TabPage("Effects")

        fogCheckBox = New CheckBox With {
            .Text = "Enable Fog",
            .Location = New System.Drawing.Point(20, 20),
            .AutoSize = True
        }

        Dim lblFogColor As New Label With {
            .Text = "Fog Color:",
            .Location = New System.Drawing.Point(20, 50),
            .AutoSize = True
        }

        fogColorButton = New Button With {
            .Location = New System.Drawing.Point(100, 45),
            .Size = New Size(80, 25),
            .BackColor = System.Drawing.Color.Gray,
            .Text = "Choose..."
        }

        effectsTab.Controls.AddRange({fogCheckBox, lblFogColor, fogColorButton})

        tabControl.TabPages.Add(envTab)
        tabControl.TabPages.Add(effectsTab)

        ' Dialog buttons
        Dim buttonPanel As New Panel With {
            .Height = 40,
            .Dock = DockStyle.Bottom
        }

        Dim btnOK As New Button With {
            .Text = "OK",
            .Location = New System.Drawing.Point(300, 8),
            .DialogResult = DialogResult.OK
        }

        Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New System.Drawing.Point(380, 8),
            .DialogResult = DialogResult.Cancel
        }

        buttonPanel.Controls.AddRange({btnOK, btnCancel})

        Me.Controls.Add(tabControl)
        Me.Controls.Add(buttonPanel)
    End Sub
End Class